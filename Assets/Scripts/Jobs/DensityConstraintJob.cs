using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct DensityConstraintJob
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

    public void Execute()
    {
        new DensityJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();

        new DensityConstraintGradientJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();

        new DensityConstraintGradientSumJob
        {
            Particles = Particles,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();

        new PredictPositionJob
        {
            Particles = Particles
        }.Schedule(Particles.Length, 64).Complete();
    }

    [BurstCompile]
    private struct DensityJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float Radius;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float3 r = -particle.DensityConstraintGradient * particle.DensityConstraint / (particle.DensityConstraintGradientSum + 1e-6f);
        particle.Position += r;
        Particles[index] = particle;
    }
}

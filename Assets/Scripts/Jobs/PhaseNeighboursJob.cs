using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public struct PhaseNeighboursJob
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<NativeList<int>> Neighbours;

    public void Execute()
    {
        for (int i = 0; i < Particles.Length; i++)
        {
            Neighbours[i] = new NativeList<int>(Allocator.Temp);
        }

        new FindNeighboursJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();

        new MassRatioJob
        {
            Particles = Particles,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();

        new MassRatioGradientJob
        {
            Particles = Particles,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64).Complete();
    }

    private struct MassRatioJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float ownPhaseMass = 0f;
            float otherPhaseMass = 0f;
            for (int i = 0; i < Neighbours.Length; i++)
            {
                Particle otherParticle = Particles[i];
                if (otherParticle.Phase == particle.Phase)
                {
                    ownPhaseMass += otherParticle.Mass;
                }
                else
                {
                    otherPhaseMass += otherParticle.Mass;
                }
            }
            particle.MassRatio = ownPhaseMass / (ownPhaseMass + otherPhaseMass);
            Particles[index] = particle;
        }
    }

    private struct MassRatioGradientJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            particle.MassRatioGradient = float3.zero;
            Particles[index] = particle;
        }
    }
}

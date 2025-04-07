using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

public struct VelocityGradientJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public NativeArray<NativeList<int>> Neighbours;
    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float3 velocityGradient = 0;
        for (int i = 0; i < Neighbours[index].Length; i++)
        {
            Particle otherParticle = Particles[Neighbours[index][i]];
            float W = Kernels.Poly6(particle.PredictedPosition - otherParticle.PredictedPosition, Radius);
            float3 V = otherParticle.Velocity - particle.Velocity;
            velocityGradient += V * W;
        }
        particle.VorticityGradient = velocityGradient;
        Particles[index] = particle;
    }
}
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct VorticityJob
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

    public void Execute()
    {
        // Calc vorticity based on neighbouring density and speed differences
        JobHandle handle = new VorticityValueJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 32);
        
        // Calc vorticity gradient
        new VorticityGradientJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 32, handle).Complete();
    }
    
    
    private struct VorticityValueJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float Radius;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float3 vorticity = 0;
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 W = Kernels.SpikyGradient(particle.PredictedPosition - otherParticle.PredictedPosition, Radius);
                float3 V = otherParticle.Velocity - particle.Velocity;
                vorticity += math.cross(V, W);
            }
            particle.Vorticity = vorticity;
            Particles[index] = particle;
        }
    }
    
    private struct VorticityGradientJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float Radius;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float3 vorticityGradient = 0;
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 W = Kernels.SpikyGradient(particle.PredictedPosition - otherParticle.PredictedPosition, Radius);
                vorticityGradient += math.length(particle.Vorticity) * W;
            }
            particle.VorticityGradient = vorticityGradient;
            Particles[index] = particle;
        }
    }
}
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
        JobHandle densityJobHandle = new DensityJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 32);

        JobHandle applyConstraintJobHandle = new ApplyConstraintJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 32, densityJobHandle);

        applyConstraintJobHandle.Complete();
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
            float density = particle.Mass * Kernels.Poly6(float3.zero, Radius);
            float3 densityConstraintGradient = float3.zero;
            float densityConstraintGradientSum = 0f;
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 r = particle.PredictedPosition - otherParticle.PredictedPosition;
                density += otherParticle.Mass * Kernels.Poly6(r, Radius);
                densityConstraintGradient += otherParticle.Mass * Kernels.SpikyGradient(-r, Radius);
                densityConstraintGradientSum += math.lengthsq(otherParticle.Mass * Kernels.SpikyGradient(r, Radius)) / particle.RestDensity;
            }
            particle.Density = density;
            particle.DensityConstraintGradient = densityConstraintGradient / particle.RestDensity;
            particle.DensityConstraintGradientSum += math.lengthsq(densityConstraintGradient / particle.RestDensity);
            Particles[index] = particle;
        }
    }

    [BurstCompile]
    private struct ApplyConstraintJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float Radius;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float3 p = float3.zero;
            float lambda = -particle.DensityConstraint / (particle.DensityConstraintGradientSum + 1e-6f);
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float otherLambda = -otherParticle.DensityConstraint / (otherParticle.DensityConstraintGradientSum + 1e-6f);
                float3 r = particle.PredictedPosition - otherParticle.PredictedPosition;
                p += (lambda + otherLambda) * Kernels.SpikyGradient(r, Radius);
            }
            particle.PredictedPosition += p / particle.RestDensity;
            particle.PredictedPositionWithoutCollision += p / particle.RestDensity;
            Particles[index] = particle;
        }
    }
}

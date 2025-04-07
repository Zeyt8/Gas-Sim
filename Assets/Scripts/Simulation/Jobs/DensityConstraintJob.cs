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
        }.Schedule(Particles.Length, 64);

        JobHandle densityConstraintGradientJobHandle = new DensityConstraintGradientJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64, densityJobHandle);

        JobHandle densityConstraintGradientSumJobHandle = new DensityConstraintGradientSumJob
        {
            Particles = Particles,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64, densityConstraintGradientJobHandle);

        JobHandle applyConstraintJobHandle = new ApplyConstraintJob
        {
            Particles = Particles,
            Radius = Radius,
            Neighbours = Neighbours
        }.Schedule(Particles.Length, 64, densityConstraintGradientSumJobHandle);

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
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 r = particle.PredictedPosition - otherParticle.PredictedPosition;
                density += otherParticle.Mass * Kernels.Poly6(r, Radius);
            }
            particle.Density = density;
            Particles[index] = particle;
        }
    }

    [BurstCompile]
    private struct DensityConstraintGradientJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float Radius;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float3 densityConstraintGradient = float3.zero;
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 r = otherParticle.PredictedPosition - particle.PredictedPosition;
                densityConstraintGradient += otherParticle.Mass * Kernels.SpikyGradient(r, Radius);
            }
            particle.DensityConstraintGradient = densityConstraintGradient / particle.RestDensity;
            Particles[index] = particle;
        }
    }

    [BurstCompile]
    private struct DensityConstraintGradientSumJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float densityConstraintGradientSum = 0f;
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                densityConstraintGradientSum += math.lengthsq(otherParticle.DensityConstraintGradient);
            }
            particle.DensityConstraintGradientSum = densityConstraintGradientSum;
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
            Particles[index] = particle;
        }
    }
}

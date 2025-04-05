using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public struct DensityContraintNeighboursJob
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
    }

    public static float Poly6(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (r2 > h2) return 0f;

        float coefficient = 315f / (64f * math.PI * math.pow(h, 9));
        return coefficient * math.pow(h2 - r2, 3);
    }

    public static float3 Poly6Gradient(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (r2 > h2) return float3.zero;

        float coefficient = -945f / (32f * math.PI * math.pow(h, 9));
        return coefficient * r * math.pow(h2 - r2, 2);
    }

    #region Nested

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
            float density = particle.Mass * Poly6(float3.zero, Radius);
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
                float3 r = particle.Position - otherParticle.Position;
                density += otherParticle.Mass * Poly6(r, Radius);
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
                if (otherParticle.Position.Equals(particle.Position)) continue;
                float3 r = otherParticle.Position - particle.Position;
                float3 gradient = Poly6Gradient(r, Radius);
            }
            particle.DensityConstraintGradient = densityConstraintGradient;
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

    #endregion
}

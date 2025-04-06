using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PhaseJob
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public NativeArray<NativeList<int>> Neighbours;
    [ReadOnly] public float DiffusionalCoefficient;
    [ReadOnly] public float Epsilon;

    public void Execute()
    {
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

        new ChemicalPotentialJob
        {
            Particles = Particles,
            DiffusionalCoefficient = DiffusionalCoefficient,
            Epsilon = Epsilon
        }.Schedule(Particles.Length, 64).Complete();
    }

    [BurstCompile]
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
            for (int i = 0; i < Neighbours[index].Length; i++)
            {
                Particle otherParticle = Particles[Neighbours[index][i]];
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

    [BurstCompile]
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

    [BurstCompile]
    private struct ChemicalPotentialJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Particle> Particles;
        [ReadOnly] public float DiffusionalCoefficient;
        [ReadOnly] public float Epsilon;

        public void Execute(int index)
        {
            Particle particle = Particles[index];
            float c = particle.MassRatio;
            //float F = DiffusionalCoefficient * (c - 0.5f) * (c - 0.5f) * ((1 - c) - 0.5f) * ((1 - c) - 0.5f);
            float dF = DiffusionalCoefficient * 2 * (c - 0.5f) * ((1 - c) - 0.5f) * (1 * 2 * c);
            particle.ChemicalPotential = dF - Epsilon * Epsilon * math.dot(particle.MassRatioGradient, particle.MassRatioGradient);
            Particles[index] = particle;
        }
    }
}

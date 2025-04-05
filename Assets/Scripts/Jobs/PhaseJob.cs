using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PhaseJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DiffusionalCoefficient;
    [ReadOnly] public float Epsilon;
    [ReadOnly] public float DeltaTime;

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

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PhaseJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float density = particle.Density;
        float Ci = density / particle.RestDensity - 1;

        float c1 = particle.Mass;
        float c2 = particle.Mass;
        float F = particle.DiffusionalCoefficient * (math.pow(c1 - 0.5f, 2) * math.pow(c2 - 0.5f, 2));

        float3 gradient = particle.Gradient;
        // TODO
        float chemicalPotential = 0;

        float3 ri = -Ci * gradient / (particle.GradientSum + 1e-6f);
        particle.Position += ri;
        particle.Density = density;
        particle.ChemicalPotential = chemicalPotential;
        Particles[index] = particle;
    }
}

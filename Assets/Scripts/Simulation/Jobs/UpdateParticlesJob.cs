using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Velocity = (particle.PredictedPosition - particle.Position) / DeltaTime;
        // TODO: apply vorticity confinement, viscosity and reactive stress

        // Placeholder for reactive stress calculation
        float3 reactiveStress = -particle.ChemicalPotential * particle.MassRatioGradient;
        particle.Velocity += reactiveStress * DeltaTime;

        particle.Position = particle.PredictedPosition;
        Particles[index] = particle;
    }
}

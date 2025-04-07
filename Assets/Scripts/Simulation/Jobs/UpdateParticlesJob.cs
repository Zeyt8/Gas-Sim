using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float SurfaceTension;
    [ReadOnly] public float Epsilon;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Velocity = (particle.PredictedPosition - particle.Position) / DeltaTime;
        // TODO: apply vorticity confinement, viscosity and reactive stress

        //particle.Velocity += CalculateReactiveStress(particle) * DeltaTime;
        particle.Velocity += CalculateVorticityForce(particle) / particle.Mass * DeltaTime;
        particle.Velocity += 0.001f * particle.VelocityGradient; // Viscosity

        particle.Position = particle.PredictedPosition;
        Particles[index] = particle;
    }

    private float3 CalculateVorticityForce(Particle particle)
    {
        if (math.length(particle.VorticityGradient) == 0.0) return float3.zero;
        float3 vorticityForce = 0.01f * math.cross(math.normalize(particle.VorticityGradient), particle.Vorticity);
        return vorticityForce;
    }

    private float3 CalculateReactiveStress(Particle particle)
    {
        float3 reactiveStress = SurfaceTension / 2 * (Sf(particle.MassRatioGradient) + Sf(-particle.MassRatioGradient)) * X(particle.MassRatio, 1 - particle.MassRatio);

        return reactiveStress;
    }

    private float3 Sf(float3 c)
    {
        return -6 * math.sqrt(2) * Epsilon * math.length(c) * c;
    }

    private float3 X(float c1, float c2)
    {
        return 5 * c1 * c2;
    }
}


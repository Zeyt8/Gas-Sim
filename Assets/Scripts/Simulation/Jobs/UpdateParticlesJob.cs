using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float SurfaceTension;
    [ReadOnly] public float Epsilon;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    [ReadOnly] public NativeArray<NativeList<int>> Neighbours;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Velocity = (particle.PredictedPosition - particle.Position) / DeltaTime;

        particle.Velocity += CalculateReactiveStress(particle, index) * DeltaTime;
        particle.Velocity += CalculateVorticityForce(particle) * DeltaTime;
        particle.Velocity += Epsilon * particle.VelocityGradient; // Viscosity

        particle.Position = particle.PredictedPosition;
        Particles[index] = particle;
    }

    private float3 CalculateVorticityForce(Particle particle)
    {
        if (math.length(particle.VorticityGradient) == 0.0) return float3.zero;
        float3 vorticityForce = Epsilon * Epsilon * math.cross(math.normalize(particle.VorticityGradient), particle.Vorticity);
        return vorticityForce;
    }

    private float3 CalculateReactiveStress(Particle particle, int index)
    {
        float3 reactiveStress = SurfaceTension / 2 * (CalculateSurfaceTensionForce(particle, index) + CalculateSurfaceTensionForce(particle, index, true)) * 5 * particle.MassRatio * (1 - particle.MassRatio);
        return reactiveStress;
    }

    private float3 CalculateSurfaceTensionForce(Particle particle, int index, bool invertGradient = false)
    {
        float3 massRatioGradient = invertGradient ? -particle.MassRatioGradient : particle.MassRatioGradient;
        float magnitude = math.length(massRatioGradient);
        if (magnitude == 0.0f) return float3.zero;

        float3 normalizedGradient = massRatioGradient / magnitude;
        float divergence = CalculateDivergence(normalizedGradient, particle.PredictedPosition, index);

        return -6 * math.sqrt(2) * Epsilon * divergence * magnitude * massRatioGradient;
    }

    private float CalculateDivergence(float3 normalizedGradient, float3 position, int index)
    {
        float3 divergence = float3.zero;
        for (int i = 0; i < Neighbours[index].Length; i++)
        {
            Particle neighbor = Particles[Neighbours[index][i]];
            float3 r = neighbor.PredictedPosition - position;
            float3 gradKernel = Kernels.Poly6Gradient(r, Radius);
            divergence += gradKernel * math.dot(normalizedGradient, r);
        }
        return math.dot(divergence, new float3(1, 1, 1));
    }
}


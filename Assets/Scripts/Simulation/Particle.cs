using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct Particle
{
    // Particle properties
    public readonly uint Phase;
    public readonly float Mass;
    public readonly float RestDensity;

    // Particle state
    public float3 Position;
    public float3 PredictedPosition;
    public float3 Velocity;
    public float Density;

    public float MassRatio;
    public float3 MassRatioGradient;
    public float ChemicalPotential;

    public float3 DensityConstraintGradient;
    public float DensityConstraintGradientSum;

    public float DensityConstraint => Density / RestDensity - 1;

    public Particle(uint phase, float mass, float restDensity)
    {
        Phase = phase;
        Mass = mass;
        RestDensity = restDensity;
        Position = float3.zero;
        PredictedPosition = float3.zero;
        Velocity = float3.zero;
        Density = 0;
        MassRatio = 0;
        MassRatioGradient = float3.zero;
        DensityConstraintGradient = float3.zero;
        DensityConstraintGradientSum = 0;
        ChemicalPotential = 0;
    }

}

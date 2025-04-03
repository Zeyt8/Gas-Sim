using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct Particle
{
    public readonly float Mass;
    public readonly float RestDensity;
    public readonly float DiffusionalCoefficient;

    public float3 Position;
    public float3 Velocity;
    public float Density;
    public float3 Gradient;
    public float GradientSum;
    public float ChemicalPotential;

    public Particle(float mass, float restDensity, float diffusionalCoefficient)
    {
        Mass = mass;
        RestDensity = restDensity;
        DiffusionalCoefficient = diffusionalCoefficient;
        Position = float3.zero;
        Velocity = float3.zero;
        Density = 0;
        Gradient = float3.zero;
        GradientSum = 0;
        ChemicalPotential = 0;
    }
}

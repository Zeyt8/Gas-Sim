using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct Particle
{
    public float3 Position;
    public float3 Velocity;
    public float Mass;
    public float Density;
}

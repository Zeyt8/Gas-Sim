using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct Force
{
    public float3 Direction;
    public float Magnitude;
    public readonly float3 FinalForce => Direction * Magnitude;

    public Force(float3 direction, float magnitude)
    {
        Direction = math.normalize(direction);
        Magnitude = magnitude;
    }
}

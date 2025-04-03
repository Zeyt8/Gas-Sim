using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct EnvironmentCollisionJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float3 SimulationBounds;
    [ReadOnly] public float CollisionDamping;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float3 position = particle.Position;
        float3 velocity = particle.Velocity;
        float3 boundsMin = -SimulationBounds / 2;
        float3 boundsMax = SimulationBounds / 2;

        position = math.clamp(position, boundsMin, boundsMax);
        velocity *= math.select(1.0f, -1.0f, position == boundsMin | position == boundsMax);
        if (math.any(position == boundsMin | position == boundsMax))
        {
            velocity *= CollisionDamping;
        }

        particle.Position = position;
        particle.Velocity = velocity;
        Particles[index] = particle;
    }
}

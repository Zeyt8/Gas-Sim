using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct EnvironmentCollisionJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float3 SimulationBounds;
    [ReadOnly] public float CRCoeff;
    [ReadOnly] public bool UpdateVelocity;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float3 position;
        if (UpdateVelocity)
        {
            position = particle.Position;
        }
        else
        {
            position = particle.PredictedPosition;
        }
        float3 velocity = particle.Velocity;
        float3 boundsMin = -SimulationBounds / 2;
        float3 boundsMax = SimulationBounds / 2;
        float3 collisionNormal = float3.zero;

        if (position.x < boundsMin.x)
        {
            collisionNormal = new float3(1, 0, 0);
            position.x = boundsMin.x;
        }
        else if (position.x > boundsMax.x)
        {
            collisionNormal = new float3(-1, 0, 0);
            position.x = boundsMax.x;
        }

        if (position.y < boundsMin.y)
        {
            collisionNormal = new float3(0, 1, 0);
            position.y = boundsMin.y;
        }
        else if (position.y > boundsMax.y)
        {
            collisionNormal = new float3(0, -1, 0);
            position.y = boundsMax.y;
        }

        if (position.z < boundsMin.z)
        {
            collisionNormal = new float3(0, 0, 1);
            position.z = boundsMin.z;
        }
        else if (position.z > boundsMax.z)
        {
            collisionNormal = new float3(0, 0, -1);
            position.z = boundsMax.z;
        }

        if (UpdateVelocity)
        {
            particle.Position = position;
        }
        else
        {
            particle.PredictedPosition = position;
        }
        if (UpdateVelocity)
        {
            particle.Velocity += -(1 + CRCoeff) * math.dot(velocity, collisionNormal) * collisionNormal;
        }
        Particles[index] = particle;
    }
}

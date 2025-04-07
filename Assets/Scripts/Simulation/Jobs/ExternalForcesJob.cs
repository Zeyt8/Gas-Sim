using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct ExternalForcesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public NativeArray<ForceEmitter> Forces;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        for (int i = 0; i < Forces.Length; i++)
        {
            ForceEmitter force = Forces[i];
            particle.Velocity += force.GetAcceleration(particle) * DeltaTime;
        }
        particle.PredictedPosition = particle.Position + particle.Velocity * DeltaTime;
        particle.PredictedPositionWithoutCollision = particle.PredictedPosition;
        Particles[index] = particle;
    }
}

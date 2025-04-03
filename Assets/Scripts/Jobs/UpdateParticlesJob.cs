using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Drag;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Position += particle.Velocity * DeltaTime;
        particle.Velocity -= (Drag * particle.Velocity) * DeltaTime;
        Particles[index] = particle;
    }
}

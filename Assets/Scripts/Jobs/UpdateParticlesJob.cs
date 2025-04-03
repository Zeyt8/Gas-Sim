using Unity.Jobs;
using Unity.Collections;

public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Position += particle.Velocity * DeltaTime;
        Particles[index] = particle;
    }
}

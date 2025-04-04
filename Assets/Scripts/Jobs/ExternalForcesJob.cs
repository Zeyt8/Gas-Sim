using Unity.Jobs;
using Unity.Collections;

public struct ExternalForcesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public NativeArray<Force> Forces;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        for (int i = 0; i < Forces.Length; i++)
        {
            Force force = Forces[i];
            particle.Velocity += force.FinalForce * DeltaTime;
        }
        Particles[index] = particle;
    }
}

using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public float Drag; // TODO: this might be accounted for by the other stuff

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        particle.Velocity -= (Drag * particle.Velocity) * DeltaTime;
        // TODO: apply vorticity confinement, viscosity and reactive stress
        particle.Position += particle.Velocity * DeltaTime;
        Particles[index] = particle;
    }
}

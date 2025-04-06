using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct UpdateParticlesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        // TODO: apply vorticity confinement, viscosity and reactive stress
        particle.Velocity = (particle.Position - particle.PrevPosition) / DeltaTime;
        Particles[index] = particle;
    }
}

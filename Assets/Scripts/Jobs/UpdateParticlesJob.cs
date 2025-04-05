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
        // TODO: apply vorticity confinement, viscosity and reactive stress
        particle.Velocity = (particle.Position - particle.PrevPosition) / DeltaTime;
        Particles[index] = particle;
    }
}

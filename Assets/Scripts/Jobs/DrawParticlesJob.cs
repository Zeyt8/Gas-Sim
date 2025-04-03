using Unity.Jobs;
using Unity.Collections;

public struct DrawParticlesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Particle> Particles;

    public void Execute(int index)
    {
    }
}

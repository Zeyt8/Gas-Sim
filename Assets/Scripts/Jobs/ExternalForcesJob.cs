using Unity.Jobs;
using Unity.Collections;

public struct ExternalForcesJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;

    public void Execute(int index)
    {
    }
}

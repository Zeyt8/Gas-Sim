using Unity.Jobs;
using Unity.Collections;

public struct FindNeighboursJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Particle> Particles;

    public void Execute(int index)
    {
    }
}

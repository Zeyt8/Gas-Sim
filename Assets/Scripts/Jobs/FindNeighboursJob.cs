using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct FindNeighboursJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;

    public void Execute(int index)
    {
    }
}

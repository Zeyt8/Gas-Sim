using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public struct PhaseJob : IJobParallelFor
{
    public void Execute(int index)
    {
    }
}

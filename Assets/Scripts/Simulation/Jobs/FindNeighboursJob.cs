using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public struct FindNeighboursJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<NativeList<int>> Neighbours;
    [ReadOnly] public SpatialHashGrid SpatialHashGrid;

    public void Execute(int index)
    {
        Neighbours[index].Clear();
        Particle particle = Particles[index];
        NativeList<int> potentialNeighbors = SpatialHashGrid.GetNeighbors(particle.PredictedPosition, Allocator.Temp);
        for (int i = 0; i < potentialNeighbors.Length; i++)
        {
            int neighborIndex = potentialNeighbors[i];
            if (neighborIndex == index) continue;
            Particle otherParticle = Particles[neighborIndex];
            float distance = math.distance(particle.PredictedPosition, otherParticle.PredictedPosition);
            if (distance < Radius)
            {
                Neighbours[index].Add(neighborIndex);
            }
        }
    }
}
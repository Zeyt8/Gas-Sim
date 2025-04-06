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

    public void Execute(int index)
    {
        Neighbours[index].Clear();
        Particle particle = Particles[index];
        for (int i = 0; i < Particles.Length; i++)
        {
            if (i == index) continue;
            Particle otherParticle = Particles[i];
            float distance = math.distance(particle.PredictedPosition, otherParticle.PredictedPosition);
            if (distance < Radius)
            {
                Neighbours[index].Add(i);
            }
        }
    }
}
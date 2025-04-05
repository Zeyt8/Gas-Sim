using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//[BurstCompile]
public struct DensityConstraintJob : IJobParallelFor
{
    public NativeArray<Particle> Particles;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float3 r = -particle.DensityConstraintGradient * particle.DensityConstraint / (particle.DensityConstraintGradientSum + 1e-6f);
        if (index == 0)
        {
            Debug.Log(particle.DensityConstraintGradient);
        }
        particle.Position += r * DeltaTime;
        Particles[index] = particle;
    }
}

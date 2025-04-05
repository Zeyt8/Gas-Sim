using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct PhaseNeighboursJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float ownPhaseMass = 0f;
        float otherPhaseMass = 0f;
        for (int i = 0; i < Particles.Length; i++)
        {
            if (i == index) continue;
            Particle otherParticle = Particles[i];
            float distance = math.distance(particle.Position, otherParticle.Position);
            if (distance < Radius)
            {
                if (otherParticle.Phase == particle.Phase)
                {
                    ownPhaseMass += otherParticle.Mass;
                }
                else
                {
                    otherPhaseMass += otherParticle.Mass;
                }
            }
        }
        particle.MassRatio = ownPhaseMass / (ownPhaseMass + otherPhaseMass);
        particle.MassRatioGradient = float3.zero;
        Particles[index] = particle;
    }
}

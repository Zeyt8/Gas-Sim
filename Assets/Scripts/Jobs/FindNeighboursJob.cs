using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct FindNeighboursJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;

    public void Execute(int index)
    {
        Particle particle = Particles[index];
        float density = 0f;
        float3 gradient = float3.zero;
        float gradientSum = 0f;
        for (int i = 0; i < Particles.Length; i++)
        {
            if (i != index)
            {
                Particle otherParticle = Particles[i];
                float distance = math.distance(particle.Position, otherParticle.Position);
                if (distance < Radius)
                {
                    density += otherParticle.Mass / math.pow(distance, 2);
                    float3 direction = math.normalize(otherParticle.Position - particle.Position);
                    gradient += direction * (otherParticle.Mass / math.pow(distance, 2));
                    gradientSum += otherParticle.Mass / math.pow(distance, 2);
                }
            }
        }
        particle.Density = density;
        particle.Gradient = gradient;
        particle.GradientSum = gradientSum;
        Particles[index] = particle;
    }
}

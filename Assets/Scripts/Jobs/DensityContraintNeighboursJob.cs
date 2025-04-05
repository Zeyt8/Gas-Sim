using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct DensityContraintNeighboursJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Particle> Particles;
    [ReadOnly] public float Radius;

    public void Execute(int index)
    {
        Particle particle = Particles[index];

        // Get neighbours
        NativeList<int> neighbours = new NativeList<int>(Allocator.Temp);
        for (int i = 0; i < Particles.Length; i++)
        {
            if (i == index) continue;
            Particle otherParticle = Particles[i];
            float distance = math.distance(particle.Position, otherParticle.Position);
            if (distance < Radius)
            {
                neighbours.Add(i);
            }
        }

        // Calculate density
        float density = 0f;
        for (int i = 0; i < neighbours.Length; i++)
        {
            Particle otherParticle = Particles[neighbours[i]];
            density += otherParticle.Mass;
        }
        particle.Density = density / (4f / 3f) * math.PI * math.pow(Radius, 3);

        // Calculate density constraint gradient
        float3 densityConstraintGradient = float3.zero;
        for (int i = 0; i < neighbours.Length; i++)
        {
            Particle otherParticle = Particles[neighbours[i]];
            if (otherParticle.Position.Equals(particle.Position)) continue;
            float3 dir = math.normalize(otherParticle.Position - particle.Position);
            float constraintDiff = otherParticle.DensityConstraint - particle.DensityConstraint;
            densityConstraintGradient += dir * constraintDiff;
        }
        particle.DensityConstraintGradient = densityConstraintGradient;

        // Calculate density constraint gradient sum
        float densityConstraintGradientSum = 0f;
        for (int i = 0; i < neighbours.Length; i++)
        {
            Particle otherParticle = Particles[neighbours[i]];
            densityConstraintGradientSum += math.dot(otherParticle.DensityConstraintGradient, otherParticle.DensityConstraintGradient);
        }
        particle.DensityConstraintGradientSum = densityConstraintGradientSum;
        
        Particles[index] = particle;
    }
}

using Unity.Burst;

[BurstCompile]
public class ParticleFactory
{
    public enum ParticleType
    {
        Air,
        Water
    }

    public static Particle CreateParticle(uint phase, ParticleType type)
    {
        float restDensity = 0;
        float mass = 0;
        switch (type)
        {
            case ParticleType.Air:
                mass = 0.1f;
                restDensity = 1.225f;
                break;
            case ParticleType.Water:
                mass = 1;
                restDensity = 997f;
                break;
        }
        Particle particle = new Particle(phase, mass, restDensity);
        return particle;
    }
}

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
                mass = 0.005f;
                restDensity = 1f;
                break;
            case ParticleType.Water:
                mass = 0.01f;
                restDensity = 2f;
                break;
        }
        Particle particle = new Particle(phase, mass, restDensity);
        return particle;
    }
}

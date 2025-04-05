public class ParticleFactory
{
    public enum ParticleType
    {
        Air,
        Water
    }

    public static Particle CreateParticle(uint phase, ParticleType type, int particleCount, float volume)
    {
        float restDensity = 0;
        switch (type)
        {
            case ParticleType.Air:
                restDensity = 1.225f;
                break;
            case ParticleType.Water:
                restDensity = 997f;
                break;
        }
        float mass = restDensity * volume / particleCount;
        Particle particle = new Particle(phase, mass, restDensity);
        return particle;
    }
}

public class ParticleFactory
{
    public enum ParticleType
    {
        Air,
        Water
    }

    public static Particle CreateParticle(uint phase, ParticleType type)
    {
        float mass = 0;
        float restDensity = 0;
        switch (type)
        {
            case ParticleType.Air:
                mass = 0.001f;
                restDensity = 1.225f;
                break;
            case ParticleType.Water:
                mass = 0.001f;
                restDensity = 997f;
                break;
        }
        Particle particle = new Particle(phase, mass, restDensity);
        return particle;
    }
}

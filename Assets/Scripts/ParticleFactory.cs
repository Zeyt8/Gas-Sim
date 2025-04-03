public class ParticleFactory
{
    public enum ParticleType
    {
        Air,
        Water
    }

    public static Particle CreateParticle(ParticleType type)
    {
        float mass = 0;
        float restDensity = 0;
        float diffusionalCoefficient = 0;
        switch (type)
        {
            case ParticleType.Air:
                mass = 0.001f;
                restDensity = 1.225f;
                diffusionalCoefficient = 0.1f;
                break;
            case ParticleType.Water:
                mass = 0.001f;
                restDensity = 997f;
                diffusionalCoefficient = 0.01f;
                break;
        }
        Particle particle = new Particle(mass, restDensity, diffusionalCoefficient);
        return particle;
    }
}

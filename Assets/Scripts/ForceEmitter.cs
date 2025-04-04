using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct ForceEmitter
{
    public enum Type
    {
        Gravity,
        Directional,
        Radial
    }
    private Type _type;
    private float3 _direction;
    private float _magnitude;
    private float3 _position;

    public ForceEmitter(Type type, float3 direction, float magnitude) : this(type, direction, magnitude, float3.zero) { }
    public ForceEmitter(Type type, float magnitude, float3 position) : this(type, float3.zero, magnitude, position) { }

    public ForceEmitter(Type type, float3 direction, float magnitude, float3 position)
    {
        _type = type;
        _direction = math.normalize(direction);
        _magnitude = magnitude;
        _position = position;
    }

    public float3 GetAcceleration(Particle particle)
    {
        return _type switch
        {
            Type.Gravity => _direction * _magnitude,
            Type.Directional => _direction * _magnitude / particle.Mass,
            Type.Radial => math.normalize(particle.Position - _position) * _magnitude / particle.Mass,
            _ => float3.zero,
        };
    }
}

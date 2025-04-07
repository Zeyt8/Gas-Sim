using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class Kernels
{
    public static float Poly6(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (math.length(r) > h) return 0f;

        float coefficient = 315f / (64f * math.PI * math.pow(h, 9));
        return coefficient * math.pow(h2 - r2, 3);
    }

    public static float3 Poly6Gradient(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (math.length(r) > h) return float3.zero;

        float coefficient = -945f / (32f * math.PI * math.pow(h, 9));
        return coefficient * math.pow(h2 - r2, 2) * math.normalize(r);
    }

    public static float Spiky(float3 r, float h)
    {
        if (math.length(r) > h) return 0f;

        float coefficient = 15f / (math.PI * math.pow(h, 6));
        return coefficient * math.pow(h - math.length(r), 3);
    }

    public static float3 SpikyGradient(float3 r, float h)
    {
        float r_len = math.length(r);
        if (r_len > h) return float3.zero;

        float coefficient = -45f / (math.PI * math.pow(h, 6) * r_len);
        return coefficient * math.pow(h - r_len, 2) * math.normalize(r);
    }
}
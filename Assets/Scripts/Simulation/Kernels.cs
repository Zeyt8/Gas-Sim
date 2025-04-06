using Unity.Mathematics;

public static class Kernels
{
    public static float Poly6(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (r2 > h2) return 0f;

        float coefficient = 315f / (64f * math.PI * math.pow(h, 9));
        return coefficient * math.pow(h2 - r2, 3);
    }

    public static float3 Poly6Gradient(float3 r, float h)
    {
        float r2 = math.lengthsq(r);
        float h2 = h * h;
        if (r2 > h2) return float3.zero;

        float coefficient = -945f / (32f * math.PI * math.pow(h, 9));
        return coefficient * r * math.pow(h2 - r2, 2);
    }
}
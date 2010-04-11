////////////////////////////////////////////////////////
// DofCombine
////////////////////////////////////////////////////////

sampler baseSampler : register(s0);
sampler BlurSampler : register(s1);
sampler PositionSampler : register(s2);

float4 FocalPlane = float4( 0.0f, 0.0f, 2.0f, -0.6f );

float4 DofCombinePS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 ColorOrig = tex2D(baseSampler, texCoord);
    float4 ColorBlur = tex2D(BlurSampler, texCoord);
    float Blur = dot(tex2D(PositionSampler, texCoord).x, FocalPlane);
    
    float4 final = lerp(ColorOrig, ColorBlur, saturate(abs(Blur)));
    final.a = 1.0f;
    return final;
}

technique DofCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 DofCombinePS();
    }
}
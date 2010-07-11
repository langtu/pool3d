sampler baseSampler : register(s0);

Texture DistortionMap;
sampler DistortionMapSampler = sampler_state
{
    Texture = <DistortionMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float texelx = 0.003;
float texely = 0.003;
float burn = 0.15f;
float saturation = 1.0f;
float r = 1.0f;
float g = 1.0f;
float b = 1.0f;
float brite = 0.0f;

float2 GetDif(float2 _tex)
{
	float2 dif;
	float2 tex = _tex;
	float2 btex = _tex;
	tex.x -= texelx;
	btex.x += texelx;
	dif.x = tex2D(DistortionMapSampler, tex).r - tex2D(DistortionMapSampler, btex).r;
	tex = _tex;
	btex = _tex;
	tex.y -= texely;
	btex.y += texely;
	dif.y = tex2D(DistortionMapSampler, tex).r - tex2D(DistortionMapSampler, btex).r;
	tex = _tex;
	dif *= (1.5 - tex2D(DistortionMapSampler, tex).r);
	return dif;
}

float4 Distortion(float2 texCoord : TEXCOORD0) : COLOR0
{
    
    float2 tex = texCoord + GetDif(texCoord) * 0.1f;
	float4 col = tex2D(baseSampler, tex);
	/*float d = sqrt((tex.x - 0.5) * (tex.x - 0.5) + (tex.y - 0.5) * (tex.y - 0.5));
	col.rgb -= d * burn;
	float a = col.r + col.g + col.b;
	a /= 3.0f;
	a *= 1.0f - saturation;
	col.r = (col.r * saturation + a) * r;
	col.g = (col.g * saturation + a) * g;
	col.b = (col.b * saturation + a) * b;
	col.rgb += brite;*/
    return col;
}

technique DistortionCombine
{
    pass P0
    {
        PixelShader = compile ps_2_0 Distortion();
    }
}

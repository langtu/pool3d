float2 halfPixel = 0;

texture NormalMap;
texture PositionMap;
texture RandomMap;
float g_screen_size;
float g_self_occlusion = 0.1f;

//this is used to compute the world-position
float4x4 InvertViewProjection;

bool calculatePosition = false;

float random_size = 64.0f * 64.0f;
float g_sample_rad = 8.85f;
float g_intensity = 5.0f;
float g_scale = 7.0f;
float g_bias = 0.001f;
float g_gamma = 0.1f;

sampler g_buffer_norm = sampler_state
{
    Texture = (NormalMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler g_buffer_pos = sampler_state
{
    Texture = (PositionMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler g_random = sampler_state
{
    Texture = (RandomMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float3 getPosition(in float2 uv)
{
	float3 position;
	float4 w;
	if (calculatePosition)
	{
		float depthVal = tex2D(g_buffer_pos, uv).r;
	
		float4 pos4;
		pos4.x = uv.x * 2.0f - 1.0f;
		pos4.y = -(uv.y * 2.0f - 1.0f);
		pos4.z = depthVal;
		pos4.w = 1.0f;
		//transform to world space
		pos4 = mul(pos4, InvertViewProjection);
		pos4 /= pos4.w;
		position = pos4.xyz;
    }
	else 
	{
		w = tex2D(g_buffer_pos,uv);
		position = w.xyz;
		position /= w.w;
	}
	
	return position;
}

float3 getNormal(in float2 uv)
{
	return normalize(tex2D(g_buffer_norm, uv).xyz * 2.0f - 1.0f);
}

float2 getRandom(in float2 uv)
{
	return normalize(tex2D(g_random, g_screen_size * uv / random_size).xy * 2.0f - 1.0f);
}

float doAmbientOcclusion(in float2 tcoord,in float2 uv, in float3 p, in float3 cnorm)
{
	float3 diff = getPosition(tcoord + uv) - p;
	const float3 v = normalize(diff);
	const float d = length(diff) * g_scale;
	return max(0.0, dot(cnorm, v) - g_bias) * (1.0f / (1.0f + d)) * g_intensity;
	//return max(0.0 - g_self_occlusion,dot(cnorm, v) - g_bias)*(1.0/(1.0+d)) * g_intensity;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color;
	color.rgb = 1.0f;
	color.a = 1.0f;
	const float2 vec[4] = {float2(1,0),float2(-1,0),float2(0,1),float2(0,-1)};
	//const float2 vec[6] = {float2(1,0),float2(-1,0),float2(0,1),float2(0,-1),float2(-1,-1),float2(1,-1)};

	float3 p = getPosition(input.TexCoord);
	float3 n = getNormal(input.TexCoord);
	float2 rand = getRandom(input.TexCoord);

	float ao = 0.0f;
	float rad = g_sample_rad / p.z;

	/////////// SSAO Calculation ///////////
	int iterations = 4;
	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand) * rad;
		float2 coord2 = float2(coord1.x - coord1.y, coord1.x + coord1.y) * 0.707f;

		ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.25f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2 * 0.5f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.75f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2, p, n);
		
		/*ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.166f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2 * 0.332f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.498f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2 * 0.664f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2 * 0.83f, p, n);
		ao += doAmbientOcclusion(input.TexCoord, coord2, p, n);*/
		
		//ao += g_self_occlusion;
	} 
	ao /= (float)iterations * 4.0f;
	// ao is "black"
		
	////ao  = (ao - 0.5f) * 1.5f + 0.5f; //contrast
	ao = pow(ao, g_gamma); //gamma
	//if (ao < 0.5f)
	//	ao = pow(ao, ao);
	//else
	//	ao = pow(ao, 1.0f - ao); 
	
	color.rgb = 1.0f - ao;
    return (color);
}

technique SSAOTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

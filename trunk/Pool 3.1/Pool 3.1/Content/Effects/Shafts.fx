float4x4 World;
float4x4 ViewProj;
float4x4 LightProjection;
float4x4 LightView;

float3 LightColor;
float3 CameraPosition;
float3 LightPosition;
texture TexColor;

sampler ModelTextureSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture AlphaMap;

sampler AlphaSampler = sampler_state
{
    Texture = <AlphaMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture NoiseMap;

sampler NoiseSampler = sampler_state
{
    Texture = <NoiseMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};
texture NoiseMap2;

sampler NoiseSampler2 = sampler_state
{
    Texture = <NoiseMap2>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float4 LightVS : TEXCOORD2;
    float2 dist : TEXCOORD3;
    float3 PositionWS : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, ViewProj);
    output.TexCoord = input.TexCoord;
    output.Normal = mul(input.Normal, World);
    
    output.PositionWS = mul(input.Position, World).xyz;
	
	output.LightVS = mul(worldPosition, LightView);
	//output.LightVS = float4(LightPosition, 1.0f) - worldPosition;
	//output.dist = length(output.LightVS.xyz);

	output.dist.x = -(output.LightVS.z / 45.0f);
	output.dist.y = output.LightVS.w;
	
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float alpha = tex2D(AlphaSampler, input.TexCoord).r;
	float4 color = tex2D(ModelTextureSampler, input.TexCoord);
	float3 noise1 = tex2D(NoiseSampler, input.TexCoord).rgb;
	float3 noise2 = tex2D(NoiseSampler2, input.TexCoord).rgb;
	
	//float att = input.dist * 0.9f;
	//att += input.dist * input.dist * 0.045f;
	//att = 1.0 / att;
	
	
	//float att = (0.25f + 45000.0f / dot(input.LightVS.xyz, input.LightVS.xyz));
	float dist = -input.LightVS.z / 450.0f;
	//float dist = input.dist.x / input.dist.y;
	float att = 1-(1/(dist));
	//att = pow(att, 0.1f);
	//att = exp(-att);
	float4 output = 1;
	
	//att = 1 - att;
	float3 ViewDir = normalize(CameraPosition - input.PositionWS);
	
	// Approximate a Fresnel coefficient for the environment map.
	// This makes the surface less reflective when you are looking
	// straight at it, and more reflective when it is viewed edge-on.
	float Fresnel = (dot(-ViewDir, input.Normal));
	Fresnel = pow(Fresnel, 1.9f);
	float compositeNoise = noise1.r * noise2.g;// * 0.5f;
	output.rgb = color.rgb * att * LightColor * compositeNoise* Fresnel * alpha;// * (1-pow(alpha,1/(dist*dist)));
	//output.a = saturate(att);
	output.a = saturate(dot(output.rgb, float3(1.0f, 1.0f, 1.0f))) ;
	//output.a *= pow(alpha,dist*dist );
	//output.rgb=Fresnel;
	//output.a=1.0f;
    return output;
}

technique RenderHalf4
{
    pass Pass1
    {
        sampler[0] = <ModelTextureSampler>;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

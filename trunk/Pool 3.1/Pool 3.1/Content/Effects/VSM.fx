//-----------------------------------------
//	VSM
//-----------------------------------------
float epsilon = 0.00001f;
//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;
float4x4 Projection;
float4x4 TexProj;


//LIGHTS
float4x4 LightViews[2];
//float4x4 LightProjs[2];
float MaxDepths[2];
float depthBias[2];
int totalLights;

Texture ShadowMap0;
sampler ShadowMapSampler0 = sampler_state
{
    Texture = <ShadowMap0>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture ShadowMap1;
sampler ShadowMapSampler1 = sampler_state
{
    Texture = <ShadowMap1>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
    AddressU = Clamp;
    AddressV = Clamp;
};

//------------------
//--- Structures ---
struct VertexShaderInput
{
	float4 Position		: POSITION;
	float3 Normal	: NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position			: POSITION;
	float3 LightViewPos[2]		: TEXCOORD0;
	float4 TexLookupPos[2]     : TEXCOORD2;
	float3 Normal     : TEXCOORD4;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput PCFSM_VS(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	output.Position = mul(worldPosition, ViewProj);
	
	for (int light_i = 0; light_i < totalLights; ++light_i)
    {
		//transform position into light space
		float4 LVP = mul(worldPosition, LightViews[light_i]);
		output.LightViewPos[light_i] = LVP.xyz;
		output.TexLookupPos[light_i] = mul(mul(LVP, Projection), TexProj);
		//output.TexLookupPos[light_i] = mul(mul(LVP, LightProjs[light_i]), TexProj);
	}
	
	//////////////////////
	//transform and project position onto screen
    /*
    float4 WorldPos = mul(Position, World);
    Out.Pos = mul(mul(WorldPos, View), Proj);
    
    //transform position into light space
    float4 LightViewPos = mul(WorldPos, LightView);
    Out.LightViewPos = LightViewPos.xyz;
    Out.TexLookupPos = mul(mul(LightViewPos, Proj), TexProj);
    */
 
    //transform normal to the lights view space
    output.Normal = mul(mul(input.Normal, World), LightViews[0]);
	
	return output;
}

//-------------------
//--- PixelShader ---
float4 PCFSM_PS(VertexShaderOutput input) : COLOR
{
    //get depth
    float depth = (length(input.LightViewPos[0]) - depthBias[0]) / MaxDepths[0];

    //get moments
    float2 moments = tex2Dproj(ShadowMapSampler0, input.TexLookupPos[0]).rg;

    //calculate variance    
    float variance = moments.y - moments.x * moments.x;
    variance = min(max(variance, 0) + epsilon, 1);

    //depth comparison
    float depth_diff = moments.x - depth;
    float p_max = variance / (variance + depth_diff * depth_diff);
    float light = max(depth <= moments.x, p_max);

    //lookup spot light
    //float spot = tex2Dproj(texSpotSample, In.TexLookupPos).r;
    //spot = In.TexLookupPos.w < 10.0 ? 0.f : spot;
    
    //compute diffuse lighting
    float3 normal = normalize(input.Normal);
    float diff = saturate( dot(normal, float3(0,0,-1)) );
    
    //compute final light
    light = max(0.1f, light); 
    //return light * diff;
    float4 color;
    color.rgb = max(0.1f, light);
    color.a = 1.0f;
    return color;
}

////////////////////////////////////////////////////////////


//------------------
//--- Techniques ---
technique PCFSMTechnique
{
    pass P0
    {
          VertexShader = compile vs_3_0 PCFSM_VS();
          PixelShader = compile ps_3_0 PCFSM_PS();
    }
}


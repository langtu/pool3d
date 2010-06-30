//-----------------------------------------
//	ShadowMap
//-----------------------------------------

//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;
float4x4 LightViewProj0;
float4x4 LightViewProj1;
float MaxDepth0;
float MaxDepth1;
float2 PCFSamples[9];
float depthBias;

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
};

struct VertexShaderOutput
{
	float4 Position			: POSITION;
	float4 ShadowMapPos0		: TEXCOORD0;
	float4 RealDistance0     : TEXCOORD1;
	float4 ShadowMapPos1		: TEXCOORD2;
	float4 RealDistance1     : TEXCOORD3;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput VertexShader(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4 worldPosition = mul(input.Position, World);
	output.Position = mul(worldPosition, ViewProj);
	output.ShadowMapPos0 = mul(worldPosition, LightViewProj0);
	output.RealDistance0 = output.ShadowMapPos0.z / MaxDepth0;

	output.ShadowMapPos1 = mul(worldPosition, LightViewProj1);
	output.RealDistance1 = output.ShadowMapPos1.z / MaxDepth1;
	return output;
}

//-------------------
//--- PixelShader ---
float4 PixelShader(VertexShaderOutput input) : COLOR
{
    float2 ProjectedTexCoords;
    
    ProjectedTexCoords[0] = input.ShadowMapPos0.x / input.ShadowMapPos0.w / 2.0f + 0.5f;
    ProjectedTexCoords[1] = -input.ShadowMapPos0.y / input.ShadowMapPos0.w / 2.0f + 0.5f;
    
    float4 result0 = {1.0f,1.0f,1.0f,1.0f};
    if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && 
		(saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
	{
		result0 = float4(0,0,0,0);
		float shadowTerm = 0.0f;
		for( int i = 0; i < 9; i++ )
		{
			float StoredDepthInShadowMap = tex2D(ShadowMapSampler0, ProjectedTexCoords + PCFSamples[i]).x;
			
			if ((input.RealDistance0.z - depthBias) <= StoredDepthInShadowMap)
			{
				shadowTerm++;
			}
		}	
		
		shadowTerm /= 9.0f;
		
		if (shadowTerm < 60.0 / 255.0f) shadowTerm = 60.0 / 255.0f;
		result0 = 1.0f * shadowTerm;
		result0.w = 1.0f;
	}
    
    ProjectedTexCoords[0] = input.ShadowMapPos1.x / input.ShadowMapPos1.w / 2.0f + 0.5f;
    ProjectedTexCoords[1] = -input.ShadowMapPos1.y / input.ShadowMapPos1.w / 2.0f + 0.5f;
    
    float4 result1 = {1.0f,1.0f,1.0f,1.0f};
    if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && 
		(saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
	{
		result1 = float4(0,0,0,0);
		float shadowTerm = 0.0f;
		for( int i = 0; i < 9; i++ )
		{
			float StoredDepthInShadowMap = tex2D(ShadowMapSampler1, ProjectedTexCoords + PCFSamples[i]).x;
			
			if ((input.RealDistance1.z - depthBias) <= StoredDepthInShadowMap)
			{
				shadowTerm++;
			}
		}	
		
		shadowTerm /= 9.0f;
		
		
		if (shadowTerm < 60.0 / 255.0f) shadowTerm = 60.0 / 255.0f;
		result1 = 1.0f * shadowTerm;
		result1.w = 1.0f;
	}
    return saturate(result0*result1);
}


//------------------
//--- Techniques ---
technique PCFSMTechnique
{
    pass P0
    {
          VertexShader = compile vs_3_0 VertexShader();
          PixelShader = compile ps_3_0 PixelShader();
    }
}
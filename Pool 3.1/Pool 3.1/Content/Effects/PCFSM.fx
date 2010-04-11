//-----------------------------------------
//	ShadowMap
//-----------------------------------------

//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;
float4x4 LightViewProj;
float MaxDepth;
float2 PCFSamples[9];
float depthBias;

Texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;

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
	float4 ShadowMapPos		: TEXCOORD0;
	float4 RealDistance     : TEXCOORD1;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput VertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	output.Position = mul(worldPosition, ViewProj);
	output.ShadowMapPos = mul(worldPosition, LightViewProj);
	output.RealDistance = output.ShadowMapPos.z / MaxDepth;

	return output;
}

//-------------------
//--- PixelShader ---
float4 PixelShader(VertexShaderOutput input) : COLOR
{
    float2 ProjectedTexCoords;
    
    ProjectedTexCoords[0] = input.ShadowMapPos.x / input.ShadowMapPos.w / 2.0f + 0.5f;
    ProjectedTexCoords[1] = -input.ShadowMapPos.y / input.ShadowMapPos.w / 2.0f + 0.5f;
    
    float4 result = {1.0f,1.0f,1.0f,1.0f};
    if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && 
		(saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
	{
		result = float4(0,0,0,0);
		float shadowTerm = 0.0f;
		for( int i = 0; i < 9; i++ )
		{
			float StoredDepthInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords + PCFSamples[i]).x;
			//float StoredDepthInShadowMap = tex2D(ShadowMapSampler, ProjectedTexCoords).x;
			if ((input.RealDistance.z - depthBias) <= StoredDepthInShadowMap)
			{
				shadowTerm++;
			}
		}	
		
		shadowTerm /= 9.0f;
		//shadowTerm /= 4.0f;
		//result.w = 1.0f;
		
		
		if (shadowTerm < 60.0 / 255.0f) shadowTerm = 60.0 / 255.0f;
		result = 1.0f * shadowTerm;
		//result.w = 1.0f;
	}
    
    return result;
}


//------------------
//--- Techniques ---
technique PCFSMTechnique
{
    pass P0
    {
          VertexShader = compile vs_2_0 VertexShader();
          PixelShader = compile ps_2_0 PixelShader();
    }
}
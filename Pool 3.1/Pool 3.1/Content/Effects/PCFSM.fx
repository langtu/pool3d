//-----------------------------------------
//	ShadowMap
//-----------------------------------------

//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;

//LIGHTS
float4x4 LightViewProjs[2];
float MaxDepths[2];
int totalLights;

float2 PCFSamples[9];
float depthBias[2];

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
	float4 ShadowMapPos[2]		: TEXCOORD0;
	float4 RealDistance[2]     : TEXCOORD2;
	//float4 ShadowMapPos1		: TEXCOORD2;
	//float4 RealDistance1     : TEXCOORD3;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput VertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	output.Position = mul(worldPosition, ViewProj);
	for (int light_i = 0; light_i < totalLights; ++light_i)
    {
		output.ShadowMapPos[light_i] = mul(worldPosition, LightViewProjs[light_i]);
		output.RealDistance[light_i] = output.ShadowMapPos[light_i].z / MaxDepths[light_i];
	}
	
	return output;
}

//-------------------
//--- PixelShader ---
float4 PixelShader(VertexShaderOutput input) : COLOR
{
    float2 ProjectedTexCoords;
    float4 wt[2];//= {{1.0f,1.0f,1.0f,1.0f}, {1.0f,1.0f,1.0f,1.0f}};
    for (int j = 0; j < totalLights; ++j)
    {
		ProjectedTexCoords[0] = input.ShadowMapPos[j].x / input.ShadowMapPos[j].w / 2.0f + 0.5f;
		ProjectedTexCoords[1] = -input.ShadowMapPos[j].y / input.ShadowMapPos[j].w / 2.0f + 0.5f;
	    
		float4 result0 = {1.0f,1.0f,1.0f,1.0f};
		if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && 
			(saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
		{
			float shadowTerm = 0.0f;
			for( int i = 0; i < 9; i++ )
			{
				
				float StoredDepthInShadowMap;
				if (j == 0) StoredDepthInShadowMap = tex2D(ShadowMapSampler0, ProjectedTexCoords + PCFSamples[i]).x;
				else StoredDepthInShadowMap = tex2D(ShadowMapSampler1, ProjectedTexCoords + PCFSamples[i]).x;
				
				if ((input.RealDistance[j].z - depthBias[j]) <= StoredDepthInShadowMap)
				{
					shadowTerm++;
				}
			}
			
			shadowTerm /= 9.0f;
			
			if (shadowTerm < 60.0 / 255.0f) shadowTerm = 60.0 / 255.0f;
			result0 = 1.0f * shadowTerm;
			//result0.w = 1.0f;
		}
		wt[j] = result0;
    }
    if (totalLights == 1) wt[1] = float4(1,1,1,1);
    return saturate(wt[0]*wt[1]);
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
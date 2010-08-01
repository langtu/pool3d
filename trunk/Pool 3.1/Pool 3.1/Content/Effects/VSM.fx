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
float4x4 LightViewProjs[2];
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
	float4 ShadowMapPos[2]		: TEXCOORD0;
	float4 RealDistance[2]     : TEXCOORD2;
	
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
		output.ShadowMapPos[light_i] = mul(worldPosition, LightViewProjs[light_i]);
		output.RealDistance[light_i] = output.ShadowMapPos[light_i].z / MaxDepths[light_i];
	}
	
	return output;
}

//-------------------
//--- PixelShader ---
float4 PCFSM_PS(VertexShaderOutput input) : COLOR
{
	float2 ProjectedTexCoords;
    float4 wt[2];
    
    for (int j = 0; j < totalLights; ++j)
    {
		ProjectedTexCoords[0] = input.ShadowMapPos[j].x / input.ShadowMapPos[j].w / 2.0f + 0.5f;
		ProjectedTexCoords[1] = -input.ShadowMapPos[j].y / input.ShadowMapPos[j].w / 2.0f + 0.5f;
		
		
		float4 result0 = {1.0f,1.0f,1.0f,1.0f};
		if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
		{
			float len = input.RealDistance[j].x;
			
			float4 moments;
			if (j == 0) moments = tex2D(ShadowMapSampler0, ProjectedTexCoords);
			else moments = tex2D(ShadowMapSampler1, ProjectedTexCoords);
					
  			float E_x2 = moments.y;
			float Ex_2 = moments.x * moments.x;
			float variance = min(max(E_x2 - Ex_2, 0.0f) + 0.0005f, 1.0);
			//float variance = min(max(E_x2 - Ex_2, 0.0f) + depthBias[j], 1.0);
			float m_d = (moments.x - len);
			float p = variance / (variance + m_d * m_d);
			
			result0 = max(step(len, moments.x), p);
			result0.a = 1.0f;
			
		}
		wt[j] = result0;
    }
    if (totalLights == 1) wt[1] = float4(1,1,1,1);
    return saturate(wt[0]*wt[1]);
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


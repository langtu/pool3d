//-----------------------------------------
//	ShadowMap
//-----------------------------------------
#define MAX_SHADER_MATRICES 60


// Array of instance transforms used by the VFetch and ShaderInstancing techniques.
float4x4 InstanceTransforms[MAX_SHADER_MATRICES];

//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;

//LIGHTS
float4x4 LightViewProjs[2];
float MaxDepths[2];
float depthBias[2];
int totalLights;
float3 eyePosition;
float3 LightPosition;
float CubeBias = 10.5f;

float2 PCFSamples[9];

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


textureCUBE cubeShadowMap;
samplerCUBE cubeShadowMapSampler = sampler_state
{
	Texture = <cubeShadowMap>;
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
    AddressU = WRAP;
    AddressV = WRAP;
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
};

//--------------------
//--- VertexShader ---
VertexShaderOutput PCFSM_VS(VertexShaderInput input)
{
	VertexShaderOutput output=(VertexShaderOutput)0;
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
    float4 wt[2]= {{1.0f,1.0f,1.0f,1.0f}, {1.0f,1.0f,1.0f,1.0f}};
    for (int j = 0; j < totalLights; ++j)
    {
		ProjectedTexCoords[0] = input.ShadowMapPos[j].x / input.ShadowMapPos[j].w / 2.0f + 0.5f;
		ProjectedTexCoords[1] = -input.ShadowMapPos[j].y / input.ShadowMapPos[j].w / 2.0f + 0.5f;
	    
		float4 result0 = {1.0f,1.0f,1.0f,1.0f};
		if ((saturate(ProjectedTexCoords.x) == ProjectedTexCoords.x) && 
			(saturate(ProjectedTexCoords.y) == ProjectedTexCoords.y))
		{
			float shadowTerm = 0.0f;
			for(int i = 0; i < 9; i++)
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

////////////////////////////////////////////////////////////
/*
// Vertex shader helper function shared between the different instancing techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input,
                                      float4x4 instanceTransform)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4 worldPosition = mul(input.Position, instanceTransform);
	output.Position = mul(worldPosition, ViewProj);
	for (int light_i = 0; light_i < totalLights; ++light_i)
    {
		output.ShadowMapPos[light_i] = mul(worldPosition, LightViewProjs[light_i]);
		output.RealDistance[light_i] = output.ShadowMapPos[light_i].z / MaxDepths[light_i];
	}
	return output;
}

// On Windows, we can use an array of shader constants to implement
// instancing. The instance index is passed in as part of the vertex
// buffer data, and we use that to decide which world transform should apply.
VertexShaderOutput ShaderInstancingVertexShader(VertexShaderInput input,
                                                float instanceIndex : TEXCOORD1)
{
    return VertexShaderCommon(input, InstanceTransforms[instanceIndex]);
}

// On Windows shader 3.0 cards, we can use hardware instancing, reading
// the per-instance world transform directly from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                float4x4 instanceTransform : TEXCOORD1)
{
    return VertexShaderCommon(input, transpose(instanceTransform));
}*/
struct VS_CUBIC_OUTPUT
{
	float4 position  :  POSITION0;
	float3 worldPos  :  TEXCOORD0;
};
VS_CUBIC_OUTPUT cubicShadowMapping_VS(float4 inPosition  : POSITION0)
{
    VS_CUBIC_OUTPUT output;
	
	float4 positionW = mul(inPosition, World);

    output.position = mul(positionW, ViewProj);
    output.worldPos = positionW.xyz;
    
    return output;
}

float4 cubicShadowMapping_PS(VS_CUBIC_OUTPUT In) : COLOR0
{
    float4 color = 0.0f;

	//compute attenuation factor
	//PLightDirection.w = max(0.0f, 1.0f / (lightAttenuation.x + 
    //              			 lightAttenuation.y * distance + 
    //               			 lightAttenuation.z * distance * distance) );
    
    float shadowTerm = 0.0f;
    
    float3 PLightDirection = 0.0f;
	//for(int i = 0; i < 9; i++)
    {
		PLightDirection = LightPosition - In.worldPos;
		float distance = length(PLightDirection.xyz);
		PLightDirection = PLightDirection / distance;
		//sample depth from cubic shadow map
		float shadowMapDepth = texCUBE(cubeShadowMapSampler, float3(-PLightDirection.xy, PLightDirection.z));
	
		//depth comparison
		if((distance - CubeBias) <= shadowMapDepth)
			//shadowTerm++;
			color.xyz = 1.0f;
	}
    color.w = 1.0f;
    return color;
}
/*
// Windows instancing technique for shader 2.0 cards.
technique ShaderInstancingPCFSMTechnique
{
    pass P0
    {
        VertexShader = compile vs_2_0 ShaderInstancingVertexShader();
        PixelShader = compile ps_3_0 PCFSM_PS();
    }
}

// Windows instancing technique for shader 3.0 cards.
technique HardwareInstancingPCFSMTechnique
{
    pass P0
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PCFSM_PS();
    }
}
*/
//#endif

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

technique cubicShadowMapping
{
    pass P0
    {          
        VertexShader = compile vs_2_0 cubicShadowMapping_VS( );
        PixelShader  = compile ps_2_0 cubicShadowMapping_PS( ); 
    }
}

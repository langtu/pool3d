//-----------------------------------------
//	Depth Map
//  PoolGame
//  Edgar Bernal
//-----------------------------------------

#define MAX_SHADER_MATRICES 60


// Array of instance transforms used by the VFetch and ShaderInstancing techniques.
//float4x4 InstanceTransforms[MAX_SHADER_MATRICES];

//------------------
//--- Parameters ---
float4x4 World : WORLD;
float4x4 ViewProj;
float MaxDepth;

//------------------
//--- Structures ---
struct VertexShaderInput
{
	float4 Position : POSITION;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float Depth : TEXCOORD0;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput ShadowMapVS(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 preLight = mul(World, ViewProj);
	
	output.Position = mul(input.Position, preLight);
	output.Depth = output.Position.z / MaxDepth;
	
	return output;
}

//-------------------
//--- PixelShader ---
float4 ShadowMapPS(VertexShaderOutput IN) : COLOR0
{
	return float4(IN.Depth, 0.0f, 0.0f, 1.0f);
    //return float4(IN.Depth, IN.Depth, IN.Depth, 1.0f);
}

//-------------------
//--- PixelShader ---
float4 ShadowMapPS_VSM(VertexShaderOutput IN) : COLOR0
{
    return float4(IN.Depth, IN.Depth * IN.Depth, 0.0f, 1.0f);
}

////////////////////////////////////////////////////////////

// Vertex shader helper function shared between the different instancing techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input,
                                      float4x4 instanceTransform)
{
    VertexShaderOutput output;
    
    float4x4 preLight = mul(instanceTransform, ViewProj);
	
	output.Position = mul(input.Position, preLight);
	output.Depth = output.Position.z / MaxDepth;
	
    return output;
}

// On either platform, when instancing is disabled we can read
// the world transform directly from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
    return VertexShaderCommon(input, World);
}

/*#ifdef XBOX360


// On Xbox, we can use the GPU "vfetch" instruction to implement
// instancing. We perform arithmetic on the input index to compute
// both the vertex and instance indices.
int VertexCount;

VertexShaderOutput VFetchInstancingVertexShader(int index : INDEX)
{
    int vertexIndex = (index + 0.5) % VertexCount;
    int instanceIndex = (index + 0.5) / VertexCount;

    float4 position;
    float4 normal;
    float4 textureCoordinate;

    asm
    {
        vfetch position,          vertexIndex, position0
        vfetch normal,            vertexIndex, normal0
        vfetch textureCoordinate, vertexIndex, texcoord0
    };

    VertexShaderInput input;

    input.Position = position;
    input.Normal = normal;
    input.TextureCoordinate = textureCoordinate;

    return VertexShaderCommon(input, InstanceTransforms[instanceIndex]);
}


#else*/
/*
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
}
*/
//#endif

float3 LightPosition;
struct VS_OUTPUT_CUBEDEPTH
{
    float4 oPositionLight : POSITION0;
    float3 lightVec       : TEXCOORD0;
};

VS_OUTPUT_CUBEDEPTH CubeDepthMap_VS( float4 inPosition : POSITION0 )
{
    VS_OUTPUT_CUBEDEPTH output;
    
    float4 positionW = mul(inPosition, World);
    output.oPositionLight = mul(positionW, ViewProj);
    
    output.lightVec = LightPosition - positionW.xyz; 

    return output;
}
//-------------------------------------------------------------------------------------------------
//Pixel Shader
//-------------------------------------------------------------------------------------------------
float4 CubeDepthMap_PS( VS_OUTPUT_CUBEDEPTH In ) : COLOR0
{
    return length(In.lightVec) + 0.5f;
}

//------------------
//--- Techniques ---
technique DepthMap
{
    pass P0
    {
          VertexShader = compile vs_2_0 ShadowMapVS();
          PixelShader = compile ps_2_0 ShadowMapPS();
    }
}

technique DepthMap_VSM
{
    pass P0
    {
          VertexShader = compile vs_2_0 ShadowMapVS();
          PixelShader = compile ps_2_0 ShadowMapPS_VSM();
    }
}

// Used on both platforms, for rendering without instancing.
technique NoInstancingDepthMap
{
    pass P0
    {
        VertexShader = compile vs_1_1 NoInstancingVertexShader();
        PixelShader = compile ps_1_1 ShadowMapPS();
    }
}


/*#ifdef XBOX360


// Xbox instancing technique.
technique VFetchInstancingDepthMap
{
    pass P0
    {
        VertexShader = compile vs_3_0 VFetchInstancingVertexShader();
        PixelShader = compile ps_3_0 ShadowMapPS();
    }
}


#else
*/
/*
// Windows instancing technique for shader 2.0 cards.
technique ShaderInstancingDepthMap
{
    pass P0
    {
        VertexShader = compile vs_2_0 ShaderInstancingVertexShader();
        PixelShader = compile ps_1_1 ShadowMapPS();
    }
}


// Windows instancing technique for shader 3.0 cards.
technique HardwareInstancingDepthMap
{
    pass P0
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 ShadowMapPS();
    }
}

*/
//#endif



technique CubeDepthMap
{
    pass P0
    {          
        VertexShader = compile vs_2_0 CubeDepthMap_VS();
        PixelShader  = compile ps_2_0 CubeDepthMap_PS(); 
    }
}
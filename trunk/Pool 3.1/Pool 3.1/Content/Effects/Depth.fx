//-----------------------------------------
//	Depth Map
//  PoolGame
//  Edgar Bernal
//-----------------------------------------

//------------------
//--- Parameters ---
float4x4 World;
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
	VertexShaderOutput OUT = (VertexShaderOutput)0;

	float4x4 preLight = mul(World, ViewProj);
	
	OUT.Position = mul(input.Position, preLight);
	OUT.Depth = OUT.Position.z / MaxDepth;
	
	
	return OUT;
}

struct PS_DEPTH_Output
{
	float4 depth : COLOR0;
};

//-------------------
//--- PixelShader ---
PS_DEPTH_Output ShadowMapPS(VertexShaderOutput IN)
{
	PS_DEPTH_Output output;
	output.depth = float4(IN.Depth, IN.Depth, IN.Depth, 1.0f);
	
    return output;
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
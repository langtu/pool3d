//-----------------------------------------
//	ShadowMap
//-----------------------------------------

//------------------
//--- Parameters ---
float4x4 World;
float4x4 ViewProj;
float MaxDepth;

//------------------
//--- Structures ---
struct VertexShaderOutput
{
	float4 position : POSITION;
	float depth : TEXCOORD0;
};

//--------------------
//--- VertexShader ---
VertexShaderOutput ShadowMapVS(float4 inPos : POSITION)
{
	VertexShaderOutput OUT = (VertexShaderOutput)0;

	OUT.position = mul(mul(inPos, World), ViewProj);
	OUT.depth = OUT.position.z / MaxDepth;
	
	return OUT;
}

//-------------------
//--- PixelShader ---
float4 ShadowMapPS(VertexShaderOutput IN) : COLOR0
{
    return float4(IN.depth, IN.depth, IN.depth, 1.0f);
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
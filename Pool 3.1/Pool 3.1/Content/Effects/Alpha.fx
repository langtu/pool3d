float4x4 World;
//float4x4 View;
float4x4 ViewProj;
//float4x4 PrevWorldViewProj;

//float4 CameraPosition;
float3 LightPosition;


Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VS_ModelInput
{
    float4 Position : POSITION;
    //float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_ModelOutput
{
    float4 Position	: POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    
};


VS_ModelOutput VertexShaderFunction(VS_ModelInput input)
{
    VS_ModelOutput output;

    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
    
    //
	output.TexCoord = input.TexCoord;
    	
    return output;
}

float4 NoMRTPixelShaderFunction(VS_ModelOutput input) : COLOR0
{
    float4 Color = tex2D(ColorSampler, input.TexCoord);
    
    float intensity = (Color.x + Color.y + Color.z) / 3.0f;
    //Color.xyz = intensity;
    
    float3 att = LightPosition - input.WorldPosition.xyz;
    
    
	float alpha = saturate(1.0f - length(att) / 35.0f);
	//if (alpha < 0.0f) alpha = -alpha;
	//if (alpha == 0.0f) alpha = 0.1f;
	
	
	Color.a *= alpha;
    return Color;
}

technique RenderHalf4
{
    pass P0
    {

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 NoMRTPixelShaderFunction();
    }
}

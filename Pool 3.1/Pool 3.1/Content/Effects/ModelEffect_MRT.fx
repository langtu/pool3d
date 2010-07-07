float4x4 World;
float4x4 View;
float4x4 ViewProj;
float4x4 LightViewProj;
float4x4 PrevWorldViewProj;
float4 LightPosition[2];

float4 CameraPosition;
float MaxDepth;
float Shineness = 96.0f;
float4 vSpecularColor[2];// = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient[2]; //= {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor[2];// = {1.0f, 1.0f, 1.0f, 1.0f};
int totalLights;

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
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_ModelOutput
{
    float4 Position	: POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal	: TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float4 vCurrPositionCS			: TEXCOORD3;
    float4 vPrevPositionCS			: TEXCOORD4;
    float4 PositionViewSpace			: TEXCOORD5;
    
};

struct PS_Model_Output
{
	float4 Color : COLOR0;
	float4 DepthColor : COLOR1;
	float4 Velocity : COLOR2;
};

VS_ModelOutput VertexShaderFunction(VS_ModelInput input)
{
    VS_ModelOutput output;

    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
    
    //
	output.TexCoord = input.TexCoord;
    
    //
	output.PositionViewSpace = mul(mul(input.Position, World), View);
	output.vCurrPositionCS = output.Position;
	output.vPrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	
    return output;
}

PS_Model_Output PixelShaderFunction(VS_ModelOutput input)
{
	PS_Model_Output output;
    float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float4 totalDiffuse = float4(0,0,0,0);
	float4 totalAmbient = float4(0,0,0,0);
	float4 totalSpecular = float4(0,0,0,0);
	for (int k = 0; k < totalLights; k++)
	{
		//
		float3 LightDir = normalize(LightPosition[k] - input.WorldPosition);
		float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
	    
		// Calculate normal diffuse light.
		float DiffuseLightingFactor = dot(LightDir, input.Normal);
	    
		// R = 2 * (N.L) * N – L
		float3 Reflect = normalize(2 * DiffuseLightingFactor * input.Normal - LightDir);  
		float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
	    
		// I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
	    totalDiffuse += vDiffuseColor[k] * DiffuseLightingFactor;
	    totalSpecular += vSpecularColor[k] * Specular;
	    totalAmbient += vAmbient[k];
    }
    
    //totalDiffuse /= totalLights;
    totalDiffuse = saturate(totalDiffuse);
    //
    //output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    output.Color = saturate((Color * (totalAmbient + totalDiffuse) + totalSpecular));
    
    //
    output.DepthColor = float4(-input.PositionViewSpace.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.vCurrPositionCS.xy / input.vCurrPositionCS.w) - (input.vPrevPositionCS.xy / input.vPrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1.0f;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	return output;
}
float4 NoMRTPixelShaderFunction(VS_ModelOutput input) : COLOR0
{
    float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float4 totalDiffuse = float4(0,0,0,0);
	float4 totalAmbient = float4(0,0,0,0);
	float4 totalSpecular = float4(0,0,0,0);
	for (int k = 0; k < totalLights; k++)
	{
		//
		float3 LightDir = normalize(LightPosition[k] - input.WorldPosition);
		float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
	    
		// Calculate normal diffuse light.
		float DiffuseLightingFactor = dot(LightDir, input.Normal);
	    
		// R = 2 * (N.L) * N – L
		float3 Reflect = normalize(2 * DiffuseLightingFactor * input.Normal - LightDir);  
		float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
	    
		// I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
	    totalDiffuse += vDiffuseColor[k] * DiffuseLightingFactor;
	    totalSpecular += vSpecularColor[k] * Specular;
	    totalAmbient += vAmbient[k];
    }
    
    //totalDiffuse /= totalLights;
    totalDiffuse = saturate(totalDiffuse);
    //
    //output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    return saturate((Color * (totalAmbient + totalDiffuse) + totalSpecular));
    
    
}
technique ModelTechnique
{
    pass Pass1
    {

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
technique NoMRTModelTechnique
{
    pass Pass1
    {

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 NoMRTPixelShaderFunction();
    }
}

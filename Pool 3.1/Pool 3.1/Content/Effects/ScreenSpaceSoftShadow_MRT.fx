float4x4 World;
float4x4 View;
float4x4 ViewProj;
float4x4 LightViewProj;
float4x4 PrevWorldViewProj;
float4 LightPosition;

float4 CameraPosition;

float Shineness = 96.0f;
float MaxDepth;
float4 vSpecularColor = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient = {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};

// PARALLAX
float    g_fHeightMapScale; 
bool     g_bDisplayShadows = true;        
float    g_fShadowSoftening = 0.5f;       

int      g_nMinSamples;            
int      g_nMaxSamples;

//sampler ColorSampler : register(s0);


Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};

Texture TexBlurV;
sampler BlurVSampler = sampler_state
{
    Texture = <TexBlurV>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture SceneTexture;
sampler2D sceneTex = sampler_state
{
	Texture = <SceneTexture>;
    AddressU = Clamp;
	AddressV = Clamp;
	MagFilter = Linear;
	MinFilter = Linear;
};

sampler normalSampler : register(s3);
//texture NormalMap;
/*sampler2D normalSampler = sampler_state
{
	Texture = <NormalMap>;
	//AddressU = WRAP;
	//AddressV = WRAP;
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};*/

struct VS_ScreenSpaceShadow_Input
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

struct VS_ScreenSpaceShadow_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float2 ProjCoord		: TEXCOORD1;
    float4 ScreenCoord		: TEXCOORD2;
    float4 WorldPosition	: TEXCOORD3;
    
    float4 PrevPositionCS	: TEXCOORD4;
    float4 CurrPositionCS	: TEXCOORD5;
    float4 PositionViewS	: TEXCOORD6;
    float3x3 TBN				: TEXCOORD7;
    
    
};

struct PS_ScreenSpaceShadow_Output
{
	float4 Color : COLOR0;
	float4 DepthColor : COLOR1;
	float4 Velocity : COLOR2;
};

VS_ScreenSpaceShadow_Output VS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Input input)
{
    VS_ScreenSpaceShadow_Output output;
	
	//
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);

	//
	output.PositionViewS = mul(mul(input.Position, World), View);
	output.CurrPositionCS = output.Position;
	output.PrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
	output.TexCoord = input.TexCoord;
	
    float4 ShadowMapPos = mul(output.WorldPosition, LightViewProj);
    output.ProjCoord[0] = ShadowMapPos.x / ShadowMapPos.w / 2.0f + 0.5f;
    output.ProjCoord[1] = -ShadowMapPos.y / ShadowMapPos.w / 2.0f + 0.5f;
    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    //
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
		
	// TBN SPACE
	output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);
    
    return output;
}

PS_ScreenSpaceShadow_Output PS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Output input)
{
	PS_ScreenSpaceShadow_Output output;
	///////////////////////////////////////////////////////////////////////
	
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	
	//
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = dot(LightDir, input.TBN[2]);
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * input.TBN[2] - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
    
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
    //
    output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    
    //
    output.DepthColor = float4(-input.PositionViewS.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.CurrPositionCS.xy / input.CurrPositionCS.w) - (input.PrevPositionCS.xy / input.PrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	
    /////////////////////////////////////////////////////////////////////////
    //output.Color = float4(0.0, 0.0, 0.0, 1.0);
    //output.DepthColor = float4(input.Depth, input.Depth, input.Depth, 1.0f);
    //output.Color = output.DepthColor ;
	return output;
	
}

PS_ScreenSpaceShadow_Output PS_ScreenSpaceShadowWithNormalMapping(VS_ScreenSpaceShadow_Output input)
{
	PS_ScreenSpaceShadow_Output output;
	///////////////////////////////////////////////////////////////////////
	
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	
	
	//
	float3 Normal = tex2D(normalSampler, input.TexCoord);
	
    Normal = 2.0f * Normal - 1.0f;
    Normal = normalize(mul(Normal, input.TBN));
	
	//
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = saturate(dot(LightDir, Normal));
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
    
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
    //
    output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    
    //
    output.DepthColor = float4(-input.PositionViewS.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.CurrPositionCS.xy / input.CurrPositionCS.w) - (input.PrevPositionCS.xy / input.PrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	
    /////////////////////////////////////////////////////////////////////////
    //output.Color = float4(0.0, 0.0, 0.0, 1.0);
    //output.DepthColor = float4(input.Depth, input.Depth, input.Depth, 1.0f);
    //output.Color = output.DepthColor ;
	return output;
	
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*struct VS_OUTPUT_POM
{
    float4 position          : POSITION0;
    float2 Texcoord          : TEXCOORD0;
    float3 vLightTS          : TEXCOORD1;   
    float3 vViewTS           : TEXCOORD2;  
    float2 vParallaxOffsetTS : TEXCOORD3;   
    float3 vNormalWS         : TEXCOORD4;   
    float3 vViewWS           : TEXCOORD5; 
    float4 dist				 : TEXCOORD6;
}; */
/*
struct VS_ScreenSpaceShadowPOM_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0_centroid;
    float2 ProjCoord		: TEXCOORD1_centroid;
    float4 ScreenCoord		: TEXCOORD2_centroid;
    float4 WorldPosition	: TEXCOORD3_centroid;
    
    float4 PrevPositionCS	: TEXCOORD4_centroid;
    float4 CurrPositionCS	: TEXCOORD5_centroid;
    float4 PositionViewS	: TEXCOORD6_centroid;
    float2 vParallaxOffsetTS				: TEXCOORD7_centroid;
    //float3x3 TBN				: TEXCOORD7;
    
    
};

struct VS_ScreenSpaceShadow_Input
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

*/

/*
VS_ScreenSpaceShadowPOM_Output VertexShaderPOM (VS_ScreenSpaceShadow_Input input)
{
	VS_ScreenSpaceShadowPOM_Output output; 
	
	//
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
    
    // TBN SPACE
    
	output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);
    
    //
	//output.TexCoord = input.TexCoord;
	output.TexCoord = input.TexCoord * 2.0;
	
	//
	output.PositionViewS = mul(mul(input.Position, World), View);
	output.CurrPositionCS = output.Position;
	output.PrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
    float4 ShadowMapPos = mul(output.WorldPosition, LightViewProj);
    output.ProjCoord[0] = ShadowMapPos.x / ShadowMapPos.w / 2.0f + 0.5f;
    output.ProjCoord[1] = -ShadowMapPos.y / ShadowMapPos.w / 2.0f + 0.5f;
    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    //
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
	
	//float3 vNormalWS   = mul(Input.Normal, World);
    //float3 vTangentWS  = mul(Input.Tangent, World);
    //float3 vBinormalWS = mul(Input.Binormal, World);
    //Output.vNormalWS = vNormalWS;
    //vNormalWS   = normalize( vNormalWS );
    //vTangentWS  = normalize( vTangentWS );
    //vBinormalWS = normalize( vBinormalWS );
    //float4 vPositionWS = mul( Input.Position, World );
    //Output.vViewWS = vViewWS;
    //Output.dist = length(vLightWS);
    //float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );
    
    
    float3 vLightWS = LightPosition.xyz - output.WorldPosition.xyz; //LightDirection;
    float3 vViewWS = CameraPosition.xyz - output.WorldPosition.xyz;
    float3 vLightTS = mul(output.TBN, vLightWS);
    float3 vViewTS  = mul(output.TBN, vViewWS);
    
    float2 vParallaxDirection = normalize(vViewTS.xy);
    float fLength = length(vViewTS);
    float fParallaxLength = sqrt(fLength * fLength - vViewTS.z * vViewTS.z) / vViewTS.z;
    output.vParallaxOffsetTS = vParallaxDirection * fParallaxLength;
    output.vParallaxOffsetTS *= g_fHeightMapScale;
    
	return output;
}

float4 PixelShaderPOM( VS_ScreenSpaceShadowPOM_Output input ) : COLOR0
{   
	float3 vViewWS = normalize(CameraPosition.xyz - input.WorldPosition.xyz);
	//float3 vViewTS  = mul(output.TBN, vViewWS);
	
	float3 vViewTS   = normalize(mul(input.TBN, vViewWS));
	//float3 vViewWS   = normalize(input.vViewWS);
	float3 vLightWS = normalize(LightPosition.xyz - input.WorldPosition.xyz);
	
	float3 vLightTS  = mul(input.TBN, vLightWS);
	float3 vNormalWS = normalize(input.TBN[2]);
    
	float4 cResultColor = float4( 0, 0, 0, 1 );

	float2 dx = ddx( input.TexCoord );
	float2 dy = ddy( input.TexCoord );
    
	int nNumSteps = (int) lerp( g_nMaxSamples, g_nMinSamples, dot( vViewWS, vNormalWS ) );

	float fCurrHeight = 0.0;
	float fStepSize   = 1.0 / (float) nNumSteps;
	float fPrevHeight = 1.0;
	float fNextHeight = 0.0;

	int    nStepIndex = 0;
	bool   bCondition = true;

	float2 vTexOffsetPerStep = fStepSize * input.vParallaxOffsetTS;
	float2 vTexCurrentOffset = input.texCoord;
	float  fCurrentBound     = 1.0;
	float  fParallaxAmount   = 0.0;

	float2 pt1 = 0;
	float2 pt2 = 0;

	float2 texOffset2 = 0;

	while ( nStepIndex < nNumSteps ) 
	{
		vTexCurrentOffset -= vTexOffsetPerStep;

		fCurrHeight = tex2Dgrad( heightSampler, vTexCurrentOffset, dx, dy ).r;

		fCurrentBound -= fStepSize;

		if ( fCurrHeight > fCurrentBound ) 
		{     
			pt1 = float2( fCurrentBound, fCurrHeight );
			pt2 = float2( fCurrentBound + fStepSize, fPrevHeight );

			texOffset2 = vTexCurrentOffset - vTexOffsetPerStep;

			nStepIndex = nNumSteps + 1;
		}
		else
		{
			nStepIndex++;
			fPrevHeight = fCurrHeight;
		}
	}   

	float fDelta2 = pt2.x - pt2.y;
	float fDelta1 = pt1.x - pt1.y;
	fParallaxAmount = (pt1.x * fDelta2 - pt2.x * fDelta1 ) / ( fDelta2 - fDelta1 );
   
	float2 vParallaxOffset = i.vParallaxOffsetTS * (1 - fParallaxAmount );

	float2 texSample = input.texCoord - vParallaxOffset;
	float fOcclusionShadow = 1;
   
	if ( g_bDisplayShadows == true )
	{
		float2 vLightRayTS = vLightTS.xy * g_fHeightMapScale;
	      
		float sh0 =  tex2Dgrad( heightSampler, texSample, dx, dy ).r;
		float shA = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.88, dx, dy ).r - sh0 - 0.88 ) *  1 * g_fShadowSoftening;
		float sh9 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.77, dx, dy ).r - sh0 - 0.77 ) *  2 * g_fShadowSoftening;
		float sh8 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.66, dx, dy ).r - sh0 - 0.66 ) *  4 * g_fShadowSoftening;
		float sh7 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.55, dx, dy ).r - sh0 - 0.55 ) *  6 * g_fShadowSoftening;
		float sh6 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.44, dx, dy ).r - sh0 - 0.44 ) *  8 * g_fShadowSoftening;
		float sh5 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.33, dx, dy ).r - sh0 - 0.33 ) * 10 * g_fShadowSoftening;
		float sh4 = (tex2Dgrad( heightSampler, texSample + vLightRayTS * 0.22, dx, dy ).r - sh0 - 0.22 ) * 12 * g_fShadowSoftening;
	   
		fOcclusionShadow = 1 - max( max( max( max( max( max( shA, sh9 ), sh8 ), sh7 ), sh6 ), sh5 ), sh4 );
		fOcclusionShadow = fOcclusionShadow * 0.65 + 0.35; 
	}   
   
	cResultColor = ComputeIllumination( texSample, vLightTS, vViewTS, fOcclusionShadow, input.dist );
	
	return cResultColor;
	
	//return 0;
}


technique POM
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = CCW;
		AlphaTestEnable = false;
		StencilEnable = true;
		StencilFunc = ALWAYS;
		StencilRef = 1;
		StencilPass = REPLACE;
		StencilZFail = KEEP;
		VertexShader = compile vs_3_0 VertexShaderPOM();
        PixelShader = compile ps_3_0 PixelShaderPOM();
        
        StencilEnable = false;
    }
}
*/
technique SSSTechnique
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = CCW;
		AlphaTestEnable = false;
		StencilEnable = true;
		StencilFunc = ALWAYS;
		StencilRef = 1;
		StencilPass = REPLACE;
		StencilZFail = KEEP;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow();
        
        StencilEnable = false;
    }
}

technique NormalMappingSSSTechnique
{
    pass Pass1
    {
		//SamplerStates[3] = NormalMap;
		sampler[3] = <normalSampler>;

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = CCW;
		AlphaTestEnable = false;
		StencilEnable = true;
		StencilFunc = ALWAYS;
		StencilRef = 1;
		StencilPass = REPLACE;
		StencilZFail = KEEP;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowWithNormalMapping();
        
        StencilEnable = false;
    }
}
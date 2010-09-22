//=======================================================================
//
//	DepthOfFieldSample
//
//		by MJP 
//      mpettineo@gmail.com
//      http://mynameismjp.wordpress.com/
//		12/1/09
//
//=======================================================================
//
//	File:		pp_Blur.fx
//
//	Desc:		Implements several variants of post-processing blur
//				techiques.
//
//======================================================================

#include "DOF_Common.fxh"
bool bDeferred = false;
float4x4 InvertProjection;
float g_fFarClip;

float g_fSigma = 0.5f;

float CalcGaussianWeight(int iSamplePoint)
{
	float g = 1.0f / sqrt(2.0f * 3.14159 * g_fSigma * g_fSigma);  
	return (g * exp(-(iSamplePoint * iSamplePoint) / (2 * g_fSigma * g_fSigma)));
}

float4 GaussianBlurH (	in float2 in_vTexCoord			: TEXCOORD0,
						uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {   
		float fWeight = CalcGaussianWeight(i);
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float4 vSample = tex2D(PointSampler0, vTexCoord);
		vColor += vSample * fWeight;
    }
	
	return vColor;
}

float4 GaussianBlurV (	in float2 in_vTexCoord			: TEXCOORD0,
						uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {   
		float fWeight = CalcGaussianWeight(i);
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float4 vSample = tex2D(PointSampler0, vTexCoord);
		vColor += vSample * fWeight;
    }

    return vColor;
}

float4 GaussianDepthBlurH (	in float2 in_vTexCoord			: TEXCOORD0,
							uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;
	float4 vCenterColor = tex2D(PointSampler0, in_vTexCoord);
	float fCenterDepth;

	if (bDeferred)
	{
		//compute screen-space position
		float4 position;
		position.x = in_vTexCoord.x * 2.0f - 1.0f;
		position.y = -(in_vTexCoord.y * 2.0f - 1.0f);
		position.z = tex2D(PointSampler1, in_vTexCoord).x;
		position.w = 1.0f;
		
		//transform to world space
		position = mul(position, InvertProjection);
		position /= position.w;
		fCenterDepth = -position.z / g_fFarClip;
	} else
		fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x;
		
    for (int i = -iRadius; i < 0; i++)
    {   
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float fDepth;// = tex2D(PointSampler1, vTexCoord).x;
		
		if (bDeferred)
		{
			//compute screen-space position
			float4 position;
			position.x = vTexCoord.x * 2.0f - 1.0f;
			position.y = -(vTexCoord.y * 2.0f - 1.0f);
			position.z = tex2D(PointSampler1, vTexCoord).x;
			position.w = 1.0f;
			
			//transform to world space
			position = mul(position, InvertProjection);
			position /= position.w;
			fDepth = -position.z / g_fFarClip;
		} else
			fDepth = tex2D(PointSampler1, vTexCoord).x;
			
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor += vCenterColor * fWeight;
    }
    
    for (int i = 1; i < iRadius; i++)
    {   
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float fDepth;// = tex2D(PointSampler1, vTexCoord).x;
		
		if (bDeferred)
		{
			//compute screen-space position
			float4 position;
			position.x = vTexCoord.x * 2.0f - 1.0f;
			position.y = -(vTexCoord.y * 2.0f - 1.0f);
			position.z = tex2D(PointSampler1, vTexCoord).x;
			position.w = 1.0f;
			
			//transform to world space
			position = mul(position, InvertProjection);
			position /= position.w;
			fDepth = -position.z / g_fFarClip;
		} else
			fDepth = tex2D(PointSampler1, vTexCoord).x;
			
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
    
    vColor += vCenterColor * CalcGaussianWeight(0);
	
	return vColor;
}

float4 GaussianDepthBlurV(	in float2 in_vTexCoord			: TEXCOORD0,
							uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;
	float4 vCenterColor = tex2D(PointSampler0, in_vTexCoord);
	float fCenterDepth;// = tex2D(PointSampler1, in_vTexCoord).x; 
	
	if (bDeferred)
	{
		//compute screen-space position
		float4 position;
		position.x = in_vTexCoord.x * 2.0f - 1.0f;
		position.y = -(in_vTexCoord.y * 2.0f - 1.0f);
		position.z = tex2D(PointSampler1, in_vTexCoord).x;
		position.w = 1.0f;
		
		//transform to world space
		position = mul(position, InvertProjection);
		position /= position.w;
		fCenterDepth = -position.z / g_fFarClip;
	} else
		fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x;
		
    for (int i = -iRadius; i < 0; i++)
    {   
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float fDepth;// = tex2D(PointSampler1, vTexCoord).x;
		
		if (bDeferred)
		{
			//compute screen-space position
			float4 position;
			position.x = vTexCoord.x * 2.0f - 1.0f;
			position.y = -(vTexCoord.y * 2.0f - 1.0f);
			position.z = tex2D(PointSampler1, vTexCoord).x;
			position.w = 1.0f;
			
			//transform to world space
			position = mul(position, InvertProjection);
			position /= position.w;
			fDepth = -position.z / g_fFarClip;
		} else
			fDepth = tex2D(PointSampler1, vTexCoord).x;
		float fWeight = CalcGaussianWeight(i);
		
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
    
    for (int i = 1; i < iRadius; i++)
    {   
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float fDepth;// = tex2D(PointSampler1, vTexCoord).x;
		
		if (bDeferred)
		{
			//compute screen-space position
			float4 position;
			position.x = vTexCoord.x * 2.0f - 1.0f;
			position.y = -(vTexCoord.y * 2.0f - 1.0f);
			position.z = tex2D(PointSampler1, vTexCoord).x;
			position.w = 1.0f;
			
			//transform to world space
			position = mul(position, InvertProjection);
			position /= position.w;
			fDepth = -position.z / g_fFarClip;
		} else
			fDepth = tex2D(PointSampler1, vTexCoord).x;
			
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
	
	vColor += vCenterColor * CalcGaussianWeight(0);
	
	return vColor;
}

technique GaussianBlurH
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 GaussianBlurH(6);
        
        ZEnable = false;
        ZWriteEnable = false;
        StencilEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
    }
}

technique GaussianBlurV
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 GaussianBlurV(6);
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}

technique GaussianDepthBlurH
{
    pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 GaussianDepthBlurH(6);
        
        ZEnable = false;
        ZWriteEnable = false;
        StencilEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
    }
}

technique GaussianDepthBlurV
{
    pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 GaussianDepthBlurV(6);
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}



//======================================================================
//
//	DepthOfFieldSample
//
//		by MJP 
//      mpettineo@gmail.com
//      http://mynameismjp.wordpress.com/
//		12/1/09
//
//======================================================================
//	File:		pp_DOF.fx
//
//	Desc:		Combines the original image with a blurred image
//				based on values from the depth buffer.
//
//======================================================================

#include "DOF_Common.fxh"

float g_fFarClip;
float g_fFocalDistance;
float g_fFocalWidth;
float g_fAttenuation;

static const int NUM_DOF_TAPS = 12;
static const float MAX_COC = 10.0f;

float2 g_vFilterTaps[NUM_DOF_TAPS];

float GetBlurFactor(in float fDepthVS)
{
	return smoothstep(0, g_fFocalWidth, abs(g_fFocalDistance - (fDepthVS * g_fFarClip)));
}

float4 DOFDiscPS(in float2 in_vTexCoord		: TEXCOORD0) : COLOR
{
	// Start with center sample color
	float4 vColorSum = tex2D(PointSampler0, in_vTexCoord);
	float fTotalContribution = 1.0f;

	// Depth and blurriness values for center sample
	float fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x;
	float fCenterBlur = GetBlurFactor(fCenterDepth);

	if (fCenterBlur > 0)
	{
		// Compute CoC size based on blurriness
		float fSizeCoC = fCenterBlur * MAX_COC;

		// Run through all filter taps
		for (int i = 0; i < NUM_DOF_TAPS; i++)
		{
			// Compute sample coordinates
			float2 vTapCoord = in_vTexCoord + g_vFilterTaps[i] * fSizeCoC;

			// Fetch filter tap sample
			float4 vTapColor = tex2D(LinearSampler0, vTapCoord);
			float fTapDepth = tex2D(PointSampler1, vTapCoord).x;
			float fTapBlur = GetBlurFactor(fTapDepth);

			// Compute tap contribution based on depth and blurriness
			float fTapContribution = (fTapDepth > fCenterDepth) ? 1.0f : fTapBlur;

			// Accumulate color and sample contribution
			vColorSum += vTapColor * fTapContribution;
			fTotalContribution += fTapContribution;
		}
	}

	// Normalize color sum
	float4 vFinalColor = vColorSum / fTotalContribution;
	return vFinalColor;
}

float4 DOFBlurBufferPS (	in float2 in_vTexCoord			: TEXCOORD0	)	: COLOR0 
{
	float4 vOriginalColor = tex2D(PointSampler0, in_vTexCoord);
	float4 vBlurredColor = tex2D(LinearSampler1, in_vTexCoord);
	float fDepthVS = tex2D(PointSampler2, in_vTexCoord).x;

	float fBlurFactor = GetBlurFactor(fDepthVS);
	
    return lerp(vOriginalColor, vBlurredColor, saturate(fBlurFactor) * g_fAttenuation);
}

technique DOFDiscBlur
{
	pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 DOFDiscPS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
    }
}

technique DOFBlurBuffer
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 DOFBlurBufferPS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
    }
}
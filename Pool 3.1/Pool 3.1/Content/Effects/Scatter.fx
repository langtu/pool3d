Texture blackTex;

sampler2D blackTexture = sampler_state
{
	Texture = <blackTex>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


Texture frameTex;

sampler2D frameSampler = sampler_state
{
	Texture = <frameTex>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float4x4 View;
float4x4 WorldViewProjection;

half Density = 0.8f;
half Weight = 0.9f;
half Decay = 0.5f;
half Exposition = 0.5f;

half3 LightPosition;
half3 LightDirection = {1.0f,1.0f,1.0f};
half3 CameraPos;

int numSamples;

struct VertexShaderInput
{
    float3 Position : POSITION0;
    half2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    half2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PSColor(VertexShaderOutput input) : COLOR0
{
    half4 screenPos = mul(LightPosition, WorldViewProjection); 
    half2 ssPos = screenPos.xy / screenPos.w 
			* float2(0.5,-0.5) + 0.5;
			
	half2 oriTexCoord = input.TexCoord;
			
	half2 deltaTexCoord = (input.TexCoord - ssPos);
    
    deltaTexCoord *= 1.0f / numSamples * Density;
    
    half3 color = tex2D(blackTexture, input.TexCoord).r;
    if (color.r != 0.0) color = tex2D(frameSampler, input.TexCoord);
    
    half illuminationDecay = 1.0f;
    
	for (int i = 0; i < numSamples; i++)
	{
		oriTexCoord -= deltaTexCoord;
			
		half3 sample = tex2D(blackTexture, oriTexCoord);
		sample *= illuminationDecay * Weight;
		color += sample;
			
		illuminationDecay *= Decay;
	}
	
	float amount = dot(mul(LightDirection, View), float3(0.0f, 0.0f, -1.0f));
	float4 sampleFrame = tex2D(frameSampler, input.TexCoord);
    
    return saturate(amount - 0.5) * float4(color * Exposition, 1) + sampleFrame;
}

half4 PSHalf4(VertexShaderOutput input) : COLOR0
{
	// Calculate vector from pixel to light source in screen space.  	
    half4 screenPos = mul(LightPosition, WorldViewProjection); 
    half2 ssPos = screenPos.xy / screenPos.w * float2(0.5, -0.5) + 0.5;
    
	half2 oriTexCoord = input.TexCoord;
		
	half2 deltaTexCoord = (input.TexCoord - ssPos);
    
    // Divide by number of samples and scale by control factor.  
    deltaTexCoord *= 1.0f / numSamples * Density;
    
    // Store initial sample.
    half3 color = tex2D(blackTexture, input.TexCoord).r;
    if (color.r != 0.0) color = tex2D(frameSampler, input.TexCoord);
    
    // Set up illumination decay factor.  
    half illuminationDecay = 1.0f;
    
    // Evaluate summation from Equation 3 NUM_SAMPLES iterations. 
	for (int i = 0; i < numSamples; i++)
	{
		// Step sample location along ray.  
		oriTexCoord -= deltaTexCoord;
		
		// Retrieve sample at new location.	
		half3 sample = tex2D(blackTexture, oriTexCoord);
		
		// Apply sample attenuation scale/decay factors.  
		sample *= illuminationDecay * Weight;
		
		// Accumulate combined color.  
		color += sample;
		
		// Update exponential decay factor. 
		illuminationDecay *= Decay;
	}
	
	half amount = dot(mul(LightDirection, View), half3(0.0f, 0.0f, -1.0f));
	half4 sampleFrame = tex2D(frameSampler, input.TexCoord);
    
    // Output final color with a further scale control factor.  
    return saturate(amount - 0.5) * half4(color * Exposition, 1.0f) + sampleFrame;
    //return half4(color * Exposition, 1.0f) + sampleFrame;
}



technique ScatterColor
{
    pass P0
    {
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSColor();
    }
}

technique ScatterHalf4
{
    pass P0
    {
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSHalf4();
    }
}
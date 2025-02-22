#include "deferredParticleHeader.fxh"

float time;
// Global Variables
float4x4 World : World;
float4x4 vp :ViewProjection;

float3 EyePosition;
float3 worldUp = float3(0,1,0);
float min = 0;
float max = 150;
float fallSpeed = .05f;
float3 wind = float3(0,0,0);

float4 color = 1;

texture depthMap;
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

texture textureMat;
sampler textureSample = sampler_state 
{
    texture = <textureMat>;    
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TexCoord0;
	float4 screenPos : TexCoord1;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunctionH(VertexShaderInput input,VertexShaderInput2 input2)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 world = transpose(input2.instanceTransform);
	float3 p = float3(world._41,world._42,world._43);
	
	input.Position.y = lerp(max, min, (p.y + (time * fallSpeed)) % 1 );

	// Calcs to wobble the snow flakes
	input.Position.x = p.x +  cos(time + (input2.extras.x*100));
	input.Position.z = p.z + sin(time + (input2.extras.y*100));

	input.Position.xyz += (wind * time);

	float3 center = mul(input.Position ,World);	
	float3 eyeVector = center - EyePosition;
	
	float3 finalPos = center;
	float3 sideVector;
	float3 upVector;	
	
	sideVector = normalize(cross(eyeVector,worldUp));			
	upVector = normalize(cross(sideVector,eyeVector));	
	
	finalPos += (input.TexCoord.x - 0.5) * sideVector * world._13;
	finalPos += (0.5 - input.TexCoord.y) * upVector * (world._24);	
	
	half4 finalPos4 = half4(finalPos,1);
	
	output.Position = mul(finalPos4,vp);// mul(mul(input.Position, world),vp);
    output.TexCoord = input.TexCoord;
    output.screenPos = output.Position;

    return output;
}

PixelShaderOutput PSBasicTexture(VertexShaderOutput input) : COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)0;
	
	output.Color = tex2D(textureSample,input.TexCoord);
	
	input.screenPos /= input.screenPos.w;
	float2 texCoord = 0.5f * (float2(input.screenPos.x,-input.screenPos.y) + 1);
	
	float depthVal = 1-tex2D(depthSampler,texCoord).r;

	if(input.screenPos.z > depthVal)
	{
		output.Color = 0;
	}
	else
	{
		if(output.Color.r > .55)
		{
			output.Depth.r = (1-input.screenPos.z); // Flip to keep accuracy away from floating point issues.
		}
		output.Depth.a = output.Color.r;		
	}
		
	return output; 
}



technique BasicTextureH
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunctionH();
        PixelShader = compile ps_3_0 PSBasicTexture();
    }
}


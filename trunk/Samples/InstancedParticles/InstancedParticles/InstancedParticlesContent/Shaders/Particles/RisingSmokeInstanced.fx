float time;
// Global Variables
float4x4 World : WORLD;
float4x4 vp :ViewProjection;

float3 EyePosition;
float3 worldUp = float3(0,1,0);
float min = -1.5;
float max = 100;
float riseSpeed = .025f;


float4 color = 1;

struct PixelShaderOutput
{
	float4 Color	: COLOR0;	// Color	
	float4 Depth	: COLOR1;	// Depth map (R) Specular (G) Glow (B) and Reflection (A)
};

texture textureMat;
sampler textureSample = sampler_state 
{
    texture = <textureMat>;    
};

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

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TexCoord0;
	float4 screenPos : TexCoord1;
	float  alpha : TexCoord2;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderInput2
{
	float4x4 instanceTransform : BLENDWEIGHT;
	float4   extras : BLENDWEIGHT4;
};

VertexShaderOutput VertexShaderFunctionH(VertexShaderInput input,VertexShaderInput2 input2)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 world = transpose(input2.instanceTransform);

	float3 p = float3(world._41,world._42,world._43);

	input.Position.y = lerp(p.y , p.y + input2.extras.z, (time * riseSpeed * input2.extras.x * input2.extras.y) % 1 );
	output.alpha =  lerp(1,0,(time * riseSpeed * input2.extras.x * input2.extras.y) % 1);

	input.Position.x = p.x +  cos(time + (input2.extras.x*100)) * .25f;
	input.Position.z = p.z + sin(time + (input2.extras.y*100)) * .25f;
	
	float3 center = mul(input.Position,World);	
	float3 eyeVector = center - EyePosition;
	
	float3 finalPos = center;
	float3 sideVector;
	float3 upVector;	
	
	sideVector = normalize(cross(eyeVector,worldUp));			
	upVector = normalize(cross(sideVector,eyeVector));	
	
	float sc = (input.Position.y +1) * .5f;
	finalPos += (input.TexCoord.x - 0.5) * sideVector * ((world._13) * sc);
	finalPos += (0.5 - input.TexCoord.y) * upVector * ((world._24) * sc);	
	
	half4 finalPos4 = half4(finalPos,1);
	
	output.Position = mul(finalPos4,vp);// mul(mul(input.Position, world),vp);
    output.TexCoord = input.TexCoord;

	output.screenPos = output.Position;
	return output;
}

PixelShaderOutput PSBasicTexture(VertexShaderOutput input) : COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)0;
	
	output.Color = tex2D(textureSample,input.TexCoord);//  * color;
	
	input.screenPos /= input.screenPos.w;
	float2 texCoord = 0.5f * (float2(input.screenPos.x,-input.screenPos.y) + 1);
	
	float depthVal = 1-tex2D(depthSampler,texCoord).r;

	if(input.screenPos.z > depthVal)
	{
		output.Color = 0;
	}
	else
	{
		if(output.Color.r > .0125)
		{
			output.Depth.r = (1-input.screenPos.z); // Flip to keep accuracy away from floating point issues.
		}
		output.Depth.a = output.Color.r;		
	}

	output.Color.a = input.alpha;

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


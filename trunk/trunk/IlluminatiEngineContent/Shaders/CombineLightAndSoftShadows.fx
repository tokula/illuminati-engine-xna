sampler screen : register(s0);

texture buff2;

sampler buff2Sampler = sampler_state // Shadows
{
	texture = <buff2>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;
};

struct PS_INPUT 
{
	float2 TexCoord	: TEXCOORD0;
};

float4 Render(PS_INPUT Input) : COLOR0 
{
	float4 col = tex2D(screen, Input.TexCoord);
	float col2 = 1-tex2D(buff2Sampler, Input.TexCoord).r;

	return col * col2;
}

technique PostInvert 
{
	pass P0
	{
		PixelShader = compile ps_2_0 Render();
	}
}
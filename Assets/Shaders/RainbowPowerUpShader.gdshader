shader_type canvas_item;

// strength/brightness of red, green and blue
const vec3 luma = vec3(0.213, 0.715, 0.072);

const float rainbowSpeed = 2.0;
const float rainbowWidth = 16.0;

varying vec2 pos;

void vertex()
{
	// Get vertex position
	pos = VERTEX.xy;
}

void fragment()
{

	// colour to colourise with
	vec3 colouriseColour;

	float a = -TIME * rainbowSpeed + (pos.x + pos.y) / rainbowWidth;

	if (mod(-a, 2.0) >= 1.0)
		colouriseColour.r = mod(-a, 2.0) - 1.0;
	else
		colouriseColour.r = mod(a, 2.0) - 1.0;

	if (mod(-a + 0.666, 2.0) >= 1.0)
		colouriseColour.g = mod(-a + 0.666, 2.0) - 1.0;
	else
		colouriseColour.g = mod(a - 0.666, 2.0) - 1.0;

	if (mod(-a + 1.333, 2.0) >= 1.0)
		colouriseColour.b = mod(-a + 1.333, 2.0) - 1.0;
	else
		colouriseColour.b = mod(a - 1.333, 2.0) - 1.0;

	// colourise colour luma value
	float colouriseLuma = colouriseColour.r * luma.r + colouriseColour.g * luma.g + colouriseColour.b * luma.b;
	// luma value of texture pixel's colour
	float texLuma = COLOR.r * luma.r + COLOR.g * luma.g + COLOR.b * luma.b;

	if (texLuma - colouriseLuma > 0.0)
	{
		float interpolater = texLuma / ((1.0 - colouriseLuma)) - (colouriseLuma / ((1.0 - colouriseLuma)));
		COLOR.rgb = mix(colouriseColour.rgb, vec3(1.0, 1.0, 1.0), interpolater);
	}
	else
		COLOR.rgb = colouriseColour.rgb * texLuma / colouriseLuma;
}

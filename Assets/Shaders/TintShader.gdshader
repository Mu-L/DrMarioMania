shader_type canvas_item;

// colour to tint texture with
uniform vec3 tintColour;
// strength/brightness of red, green and blue
uniform vec3 luma;

void fragment()
{
	// colour of this pixel to replace
	vec3 ogColour = texture(TEXTURE, UV).rgb;

	// colourise colour luma value
	float colouriseLuma = tintColour.r * luma.r + tintColour.g * luma.g + tintColour.b * luma.b;
	// screen pixel colour luma value
	float screenLuma = ogColour.r * luma.r + ogColour.g * luma.g + ogColour.b * luma.b;

	if (screenLuma - colouriseLuma > 0.0)
	{
		float interpolater = screenLuma / ((1.0 - colouriseLuma)) - (colouriseLuma / ((1.0 - colouriseLuma)));
		COLOR.rgb = mix(tintColour.rgb, vec3(1.0, 1.0, 1.0), interpolater);
		//COLOR.rgb = colouriseColour.rgb + (screenLuma - colouriseLuma);
	}
	else
		COLOR.rgb = tintColour.rgb * screenLuma / colouriseLuma;
}
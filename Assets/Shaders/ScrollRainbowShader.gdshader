shader_type canvas_item;

const float scrollSpeed = 0.1;
const float rainbowSpeed = 0.05;
const float rainbowWidth = 2.0;

void fragment()
{
	// colour to colourise with
	vec3 colouriseColour;

	vec2 newUV = UV - TIME * scrollSpeed;
	COLOR = texture(TEXTURE, newUV);

	float a = -TIME * rainbowSpeed + (SCREEN_UV.x + SCREEN_UV.y) / rainbowWidth;

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

	colouriseColour *= 1.25;
	colouriseColour += 0.25;

	COLOR.rgb *= colouriseColour.rgb;


}

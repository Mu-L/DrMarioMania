shader_type canvas_item;

const float pulseSpeed = 8.0;
const float brightnessModifier = 0.4;

const float rate = 3.0;
const float height = 2.0;

void fragment()
{
	vec4 newCol = COLOR;

	float brighten = ((sin(TIME * pulseSpeed) + 1.0) / 2.0) * brightnessModifier;

	newCol += vec4(brighten, brighten, brighten, 0);

	COLOR = newCol;
}
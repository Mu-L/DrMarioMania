shader_type canvas_item;

const float pulseSpeed = 8.0;

void fragment()
{
	float fade = (sin(TIME * pulseSpeed) + 1.0) / 2.0;

	COLOR.a = COLOR.a * (1.0 - fade);
}
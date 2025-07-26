shader_type canvas_item;

const float pulseSpeed = 8.0;

const vec3 red = vec3(1.0, 0.0, 0.0);

void fragment()
{
	float fade = (sin(TIME * pulseSpeed) + 1.0) / 2.0;

	vec4 newCol;
	newCol.rgb = COLOR.rgb * (fade) + red * (1.0 - fade);
	newCol.a = COLOR.a * (1.0 - fade / 2.0);

	COLOR = newCol;
}
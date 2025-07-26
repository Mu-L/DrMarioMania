shader_type canvas_item;

const float speed = 0.1;

void fragment()
{
	// Called for every pixel the material is visible on.

	vec2 newUV = UV - TIME * speed;
	COLOR = texture(TEXTURE, newUV);
}
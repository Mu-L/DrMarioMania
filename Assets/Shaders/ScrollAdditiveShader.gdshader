shader_type canvas_item;

render_mode blend_add;

uniform vec2 speed;
uniform float opacity;

void fragment()
{
	// Called for every pixel the material is visible on.

	vec2 newUV = UV - TIME * speed;
	COLOR = texture(TEXTURE, newUV);
	COLOR.a *= opacity;
}
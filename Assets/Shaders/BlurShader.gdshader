shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, repeat_disable, filter_linear_mipmap;

const float strength = 0.1;

void fragment()
{
	float a = COLOR.a;
	COLOR = textureLod(screen_tex, SCREEN_UV, strength / (sqrt(SCREEN_PIXEL_SIZE.y)));
	COLOR.a = a;
}
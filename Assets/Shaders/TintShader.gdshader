shader_type canvas_item;

// colour to tint texture with
uniform vec3 tint_colour;
uniform bool is_active;

void fragment()
{
	if (is_active)
	{
		COLOR.rgb *= tint_colour;
	}
}
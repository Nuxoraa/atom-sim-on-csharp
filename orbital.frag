#version 330

in vec4 fragColor;
in float fragDepth;

out vec4 finalColor;

void main()
{
    vec2 uv = gl_PointCoord - vec2(0.5);
    float dist = length(uv);
    if (dist > 0.5) discard;

    float softEdge = 1.0 - smoothstep(0.3, 0.5, dist);
    float glow = exp(-dist * 6.0) * 0.6;

    vec3 color = fragColor.rgb * (softEdge + glow);
    float alpha = fragColor.a * softEdge * fragDepth;

    finalColor = vec4(color, alpha);
}

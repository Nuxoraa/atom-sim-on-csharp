#version 330

in vec3 vertexPosition;
in vec4 vertexColor;

uniform mat4 mvp;
uniform float time;

out vec4 fragColor;
out float fragDepth;

void main()
{
    vec4 worldPos = mvp * vec4(vertexPosition, 1.0);

    float pulse = 1.0 + 0.015 * sin(time * 2.0 + length(vertexPosition) * 0.5);
    gl_Position = vec4(worldPos.xyz * pulse, worldPos.w);

    float depth = clamp(1.0 - length(vertexPosition) / 20.0, 0.0, 1.0);
    fragColor = vertexColor * vec4(1.0, 1.0, 1.0, depth);
    fragDepth = depth;
}

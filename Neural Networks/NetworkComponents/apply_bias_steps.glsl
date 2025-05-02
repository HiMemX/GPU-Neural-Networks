#version 430

layout(local_size_x = 128) in;


layout(std430, binding = 0) buffer Biases{
	float biases[];
};

layout(std430, binding = 1) buffer BiasAdjustments{
	float bias_adjustments[];
};

uniform int nodeCount;
uniform float step;

void main(){
    uint node_index = gl_GlobalInvocationID.x;

    if(node_index >= nodeCount){return;}

	biases[node_index] -= step * bias_adjustments[node_index];
}
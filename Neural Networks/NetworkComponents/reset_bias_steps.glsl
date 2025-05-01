#version 430

layout(local_size_x = 64) in;


layout(std430, binding = 0) buffer BiasAdjustments{
	float bias_adjustments[];
};

uniform int nodeCount;

void main(){
    uint node_index = gl_GlobalInvocationID.x;

    if(node_index >= nodeCount){return;}

	bias_adjustments[node_index] = 0;
}
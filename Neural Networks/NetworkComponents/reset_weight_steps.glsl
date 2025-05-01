#version 430

// 2D, one axis used for addressing one layer, the other for the next layer
layout(local_size_x = 32, local_size_y = 32) in;

layout(std430, binding = 0) buffer WeightAdjustments{
	float weight_adjustments[];
};

uniform int nodeCount;
uniform int incomingNodeCount;

void main(){
    uint nodeA_index = gl_GlobalInvocationID.x;
    uint nodeB_index = gl_GlobalInvocationID.y;

	if(nodeA_index >= incomingNodeCount){return;}
    if(nodeB_index >= nodeCount){return;}

	uint index = nodeB_index * incomingNodeCount + nodeA_index;

	weight_adjustments[index] = 0;
}
#version 430

// 2D, one axis used for addressing one layer, the other for the next layer
layout(local_size_x = 32, local_size_y = 32) in;

layout(std430, binding = 0) buffer OutputDerivatives{ // ActDerivative(WeightMatrix * Layer), where layer is the layer to the left
	float output_derivatives[];
};

// NOTE: This is the outputs of the layer before, not this current layer.
layout(std430, binding = 1) buffer Outputs{
	float outputs[];
};

layout(std430, binding = 2) buffer dC_dN{
	float dCdN[];
};

layout(std430, binding = 3) buffer WeightAdjustments{
	float weight_adjustments[];
};

uniform int nodeCount;
uniform int incomingNodeCount;
uniform float step;

void main(){
    uint nodeA_index = gl_GlobalInvocationID.x;
    uint nodeB_index = gl_GlobalInvocationID.y;

	if(nodeA_index >= incomingNodeCount){return;}
    if(nodeB_index >= nodeCount){return;}

	uint index = nodeB_index * incomingNodeCount + nodeA_index;

	weight_adjustments[index] += dCdN[nodeB_index] * output_derivatives[nodeB_index] * outputs[nodeA_index];
}
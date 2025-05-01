#version 430

layout(local_size_x = 64) in;

layout(std430, binding = 0) buffer OutputDerivatives{ // WeightMatrix * Layer, where layer is the layer to the left
	float output_derivatives[];
};

layout(std430, binding = 1) buffer dC_dN{ // dCdN of the nodes we are processing the biases for
	float dCdN[];
};

layout(std430, binding = 2) buffer BiasAdjustments{
	float bias_adjustments[];
};

uniform int nodeCount;
uniform float step;

void main(){
    uint node_index = gl_GlobalInvocationID.x;

    if(node_index >= nodeCount){return;}

	bias_adjustments[node_index] += step * dCdN[node_index] * output_derivatives[node_index];
}
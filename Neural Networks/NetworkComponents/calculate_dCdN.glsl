#version 430

layout(local_size_x = 64) in;

layout(std430, binding = 0) buffer OutputDerivatives{ // WeightMatrix * Layer, where layer is the layer to the left
	float output_derivatives[];
};

layout(std430, binding = 1) buffer Weights{ // Weights connecting the 2 layers
	float weights[];
};

layout(std430, binding = 2) buffer dC_dN{ // Values will be calculated and stored in here
    float dCdN[];
};

layout(std430, binding = 3) buffer dC_dN_NextLayer{ // This is the dCdN of the NEXT Layer, meaning the one that was already calculated.
    float dCdN_NextLayer[];
};

uniform int nodeCount;
uniform int incomingNodeCount;


// Calculates dC/dN for the last layer to initiate the backpropogation

void main() {
    uint node_index = gl_GlobalInvocationID.x;

    if(node_index >= incomingNodeCount){return;} // Index of the left node, not as usual of the right node

    //dCdN[node_index] = CostDerivative(expected[node_index], outputs[node_index]);

    float sum = 0;
    for(int i=0; i<nodeCount; i++){
        sum += dCdN_NextLayer[i] * output_derivatives[i] * weights[incomingNodeCount * i + node_index]; // Derivative math
    }

    dCdN[node_index] = sum;
}
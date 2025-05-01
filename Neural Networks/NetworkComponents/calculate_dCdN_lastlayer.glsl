#version 430

layout(local_size_x = 64) in;

layout(std430, binding = 0) buffer ExpectedOutputs{
	float expected[];
};

layout(std430, binding = 1) buffer Outputs{
	float outputs[];
};

layout(std430, binding = 2) buffer dC_dN{
    float dCdN[];
};

uniform int nodeCount;

float CostDerivative(float expect, float actual_output){
    return -2*(expect - actual_output);
}

// Calculates dC/dN for the last layer to initiate the backpropogation

void main() {
    uint node_index = gl_GlobalInvocationID.x;

    if(node_index >= nodeCount){return;}

    dCdN[node_index] = CostDerivative(expected[node_index], outputs[node_index]);
}
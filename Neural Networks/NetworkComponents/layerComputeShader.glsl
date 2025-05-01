#version 430

layout(local_size_x = 64) in;

layout(std430, binding = 0) buffer Inputs{
	float inputs[];
};

layout(std430, binding = 1) buffer Weights{
	float weights[];
};

layout(std430, binding = 2) buffer Biases{
	float biases[];
};

layout(std430, binding = 3) buffer OutputDerivatives{
	float output_derivatives[];
};

layout(std430, binding = 4) buffer Outputs{
	float outputs[];
};

uniform int incomingNodeCount;
uniform int nodeCount;


float Activation(float x){
    return 1 / (1 + exp(-x));
}

float ActivationDerivative(float x){
    return 1 / (2 + exp(x) + exp(-x));
}


void main() {
    uint nodeB_index = gl_GlobalInvocationID.x;

    if(nodeB_index >= nodeCount){return;}

    uint baseindex = nodeB_index * incomingNodeCount;

    float sum = 0.0;
    for (int i = 0; i < incomingNodeCount; ++i) {
        sum += weights[baseindex + i] * inputs[i];
    }
    
    float value = sum + biases[nodeB_index];
    output_derivatives[nodeB_index] = ActivationDerivative(value);
    outputs[nodeB_index] = Activation(value);
}
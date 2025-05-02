#version 430

layout(local_size_x = 128) in;

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
uniform int seed;


float Activation(float x){
    return 1 / (1 + exp(-x));
}

float ActivationDerivative(float x){
    return 1 / (2 + exp(x) + exp(-x));
}

// Returns a float in [0,1] based on a uint seed (From ChatGPT)
float random(uint seed) {
    seed = (seed ^ 61u) ^ (seed >> 16);
    seed *= 9u;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2du;
    seed = seed ^ (seed >> 15);
    return float(seed) / float(0xffffffffu);
}



void main() {
    uint nodeB_index = gl_GlobalInvocationID.x;

    if(nodeB_index >= nodeCount){return;}

    uint baseindex = nodeB_index * incomingNodeCount;
    
    float sum = 0.0;
    if (seed == 0){ // No randomization shall take place
        for (int i = 0; i < incomingNodeCount; ++i) {
            sum += weights[baseindex + i] * inputs[i];
        }
    }
    else{
        for (int i = 0; i < incomingNodeCount; ++i) {
            sum += weights[baseindex + i] * (inputs[i] + (random(seed + nodeB_index + i * 687401) - 0.5) / 3.0f);
        }
    }
    
    
    float value = sum + biases[nodeB_index];
    output_derivatives[nodeB_index] = ActivationDerivative(value);
    outputs[nodeB_index] = Activation(value);
}
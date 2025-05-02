#version 430 core

layout(std430, binding = 0) buffer Values {
    float values[];
};

uniform float width;
out vec4 FragColor;

void main() {
    //float val = values[gl_FragCoord.x]; // example access
    FragColor = vec4(0.0, 0.0, 0.0, 1.0);

    vec2 pos = gl_FragCoord.xy / width ;
    float r = 0.18;
    float x = 0.5;

    float sum = 0;
    for(int y=0; y<10; y++){
        sum += values[y];
    }

    float v;
    float dist;
    for(int y=0; y<10; y++){
        dist = length(pos - vec2(x,float(y) / 2 + 0.5));
        if(dist >= r){continue;}
        if(dist >= r - 0.02){
            FragColor = vec4(1,1,1,1);
            return;
        }

        v = values[y] / sum;
        FragColor = vec4(0, v, v ,1);
        return;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Neural_Networks.NetworkComponents
{
    internal class Layer
    {
        public int incomingNodeCount;
        public int nodeCount;
        public ActivationFunction activationFunction;

        public int output_ssbo;
        public int output_derivative_ssbo; // Preprocessed for optimization in adjustment calculations
        public int biases_ssbo;
        public int bias_adjustments_ssbo; // For training
        public int weights_ssbo;
        public int weight_adjustments_ssbo; // For training
        public int dCdN_ssbo; // Describes how each Node affects the cost. Only valid for the current specific example, rewritten often.


        public Layer(int incomingNodeCount, int nodeCount) {
            this.incomingNodeCount = incomingNodeCount;
            this.nodeCount = nodeCount;

            Random rng = new Random();

            float[] weights = new float[incomingNodeCount * nodeCount];
            for (int w = 0; w < incomingNodeCount * nodeCount; w++)
            {
                weights[w] = (float)(rng.NextDouble() - 0.5) / 1.0f;// Only slight randomization
            }

            float[] biases = new float[nodeCount];
            for(int b=0; b<nodeCount; b++)
            {
                biases[b] = (float)(rng.NextDouble() - 0.5) / 1.0f;
            }

            output_ssbo = GL.GenBuffer();
            output_derivative_ssbo = GL.GenBuffer();
            biases_ssbo = GL.GenBuffer();
            bias_adjustments_ssbo = GL.GenBuffer();
            weights_ssbo = GL.GenBuffer();
            weight_adjustments_ssbo = GL.GenBuffer();
            dCdN_ssbo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, biases_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * sizeof(float), biases, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bias_adjustments_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * sizeof(float), new float[nodeCount], BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, weights_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * incomingNodeCount * sizeof(float), weights, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, weight_adjustments_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * incomingNodeCount * sizeof(float), new float[nodeCount * incomingNodeCount], BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, output_derivative_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * sizeof(float), new float[nodeCount], BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, dCdN_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * sizeof(float), new float[nodeCount], BufferUsageHint.StaticDraw);

            WriteToOutput(new float[nodeCount]);
        }

        public void WriteToOutput(float[] outputs) // Method to be called for starting nodes
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, output_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, nodeCount * sizeof(float), outputs, BufferUsageHint.StaticDraw);
        }

        public float[] ReadOutput()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, output_ssbo);
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            float[] data = new float[nodeCount];
            Marshal.Copy(ptr, data, 0, nodeCount);
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
            return data;
        }

        
    }
}

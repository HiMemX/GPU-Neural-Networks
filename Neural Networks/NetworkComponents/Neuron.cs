using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Neural_Networks.NetworkComponents
{
    internal enum ActivationFunction
    {
        ReLU,
        Logistic,
    }

    internal class Neuron
    {
        public int incomingNodes; // Count of incoming nodes

        public int weights_and_bias_ssbo; // Incoming weights + bias (float)
        public int output_ssbo;

        public Neuron(int incomingNodes) {
            this.incomingNodes = incomingNodes;

            Random rng = new Random();

            float[] weights_and_bias = new float[incomingNodes + 1];

            for(int w=0; w<incomingNodes; w++)
            {
                weights_and_bias[w] = (float)(rng.NextDouble() - 0.5) / 1000.0f;// Only slight randomization
            }

            weights_and_bias_ssbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, weights_and_bias_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, weights_and_bias.Length * sizeof(float), weights_and_bias, BufferUsageHint.StaticDraw);

            float[] outputs = new float[incomingNodes];
            output_ssbo = GL.GenBuffer();
            WriteToOutput(outputs);
        }

        public void WriteToOutput(float[] outputs) // Method to be called for incoming nodes
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, output_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, outputs.Length * sizeof(float), outputs, BufferUsageHint.StaticDraw);
        }

        
    }
}

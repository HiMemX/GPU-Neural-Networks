using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Neural_Networks.NetworkComponents
{
    internal class NeuralNetwork
    {
        public Layer[] layers;
        public Shader layerComputeShader;
        public Shader calculate_dCdN_lastlayer;
        public Shader calculate_dCdN;

        private BufferRangeTarget rangeTarget = BufferRangeTarget.ShaderStorageBuffer; // Makes the code a little shorter

        public NeuralNetwork(int[] dimensions)
        {
            layers = new Layer[dimensions.Length];

            int incomingNodeCount = 0;
            for(int l=0; l<layers.Length; l++)
            {
                layers[l] = new Layer(incomingNodeCount, dimensions[l]);
                incomingNodeCount = dimensions[l];
            }

            layerComputeShader       = new Shader("NetworkComponents/layerComputeShader.glsl");
            calculate_dCdN_lastlayer = new Shader("NetworkComponents/calculate_dCdN_lastlayer.glsl");
            calculate_dCdN           = new Shader("NetworkComponents/calculate_dCdN.glsl");
        }

        public void WriteInputs(float[] inputs)
        {
            layers[0].WriteToOutput(inputs);
        }

        public float[] ReadOutput()
        {
            return layers[layers.Length - 1].ReadOutput();
        }

        public void Evaluate()
        {
            layerComputeShader.Use();

            for(int i=1; i<layers.Length; i++)
            {
                layerComputeShader.SetInt("incomingNodeCount", layers[i-1].nodeCount);
                layerComputeShader.SetInt("nodeCount", layers[i].nodeCount);
                GL.BindBufferBase(rangeTarget, 0, layers[i - 1].output_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[i].weights_ssbo);
                GL.BindBufferBase(rangeTarget, 2, layers[i].biases_ssbo);
                GL.BindBufferBase(rangeTarget, 3, layers[i].output_derivative_ssbo);
                GL.BindBufferBase(rangeTarget, 4, layers[i].output_ssbo);

                GL.DispatchCompute((layers[i].nodeCount + 63) / 64, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }
        }

        public void GradientDescent(int expected_ssbo, float step)
        {
            calculate_dCdN_lastlayer.Use();
            calculate_dCdN_lastlayer.SetInt("nodeCount", layers[layers.Length - 1].nodeCount);
            GL.BindBufferBase(rangeTarget, 0, expected_ssbo);
            GL.BindBufferBase(rangeTarget, 1, layers[layers.Length - 1].output_ssbo);
            GL.BindBufferBase(rangeTarget, 2, layers[layers.Length - 1].dCdN_ssbo);

            GL.DispatchCompute((layers[layers.Length - 1].nodeCount + 63) / 64, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            // Backpropogate (NOTE: Doesn't compute anything when no hidden layers exist)
            calculate_dCdN.Use();
            for(int l=layers.Length - 2; l>0; l--) // Start at second to last layer, all the way to the first layer
            {
                calculate_dCdN.SetInt("nodeCount", layers[l+1].nodeCount);
                calculate_dCdN.SetInt("incomingNodeCount", layers[l].nodeCount);
                GL.BindBufferBase(rangeTarget, 0, layers[l+1].output_derivative_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[l + 1].weights_ssbo);
                GL.BindBufferBase(rangeTarget, 2, layers[l].dCdN_ssbo);
                GL.BindBufferBase(rangeTarget, 3, layers[l + 1].dCdN_ssbo);

                GL.DispatchCompute((layers[l].nodeCount + 63) / 64, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }
        }

        public void ApplyGradientStep() // Seperated from GradientDescent so you can batch multiple datapoints and average them
        {

        }
    }
}

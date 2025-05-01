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
        public Shader calculate_bias_steps;
        public Shader calculate_weight_steps;
        public Shader apply_bias_steps;
        public Shader apply_weight_steps;
        public Shader reset_bias_steps;
        public Shader reset_weight_steps;

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
            calculate_bias_steps     = new Shader("NetworkComponents/calculate_bias_steps.glsl");
            calculate_weight_steps   = new Shader("NetworkComponents/calculate_weight_steps.glsl");
            apply_bias_steps         = new Shader("NetworkComponents/apply_bias_steps.glsl");
            apply_weight_steps       = new Shader("NetworkComponents/apply_weight_steps.glsl");
            reset_bias_steps         = new Shader("NetworkComponents/reset_bias_steps.glsl");
            reset_weight_steps       = new Shader("NetworkComponents/reset_weight_steps.glsl");
        }

        public void WriteInputs(float[] inputs)
        {
            layers[0].WriteToOutput(inputs);
        }

        public void SetInput(int input_ssbo)
        {
            layers[0].output_ssbo = input_ssbo;
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

        public float GetAverageError(List<TrainingExample> examples)
        {
            float error = 0;
            foreach(TrainingExample example in examples)
            {
                error += GetError(example);
            }
            return error / (float)examples.Count;
        }

        public float GetError(TrainingExample example)
        {
            SetInput(example.input_ssbo);
            Evaluate();
            float[] outputs = layers[layers.Length - 1].ReadOutput();

            float error = 0;
            for(int i=0; i<outputs.Length; i++) {
                error += (float)Math.Pow(outputs[i] - example.expected_outputs[i], 2);
            }

            return error / (float)outputs.Length;
        }


        public void Train(List<TrainingExample> examples, float learnrate, int iterationcount)
        {
            for (int i = 0; i < iterationcount; i++) Train(examples, learnrate);
        }
        public void Train(List<TrainingExample> examples, float learnrate)
        {
            ResetGradient();
            foreach(TrainingExample example in examples)
            {
                SetInput(example.input_ssbo);
                Evaluate();
                CalculateGradient(example.expected_output_ssbo, 1.0f / (float)examples.Count);
            }
            ApplyGradientStep(learnrate);
        }

        public void ResetGradient()
        {
            reset_bias_steps.Use();
            for (int l = 1; l < layers.Length; l++) // Order doesn't matter :)
            {
                reset_bias_steps.SetInt("nodeCount", layers[l].nodeCount);
                GL.BindBufferBase(rangeTarget, 0, layers[l].bias_adjustments_ssbo);

                GL.DispatchCompute((layers[l].nodeCount + 63) / 64, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }

            reset_weight_steps.Use();
            for (int l = 1; l < layers.Length; l++) // Order doesn't matter :)
            {
                reset_weight_steps.SetInt("nodeCount", layers[l].nodeCount);
                reset_weight_steps.SetInt("incomingNodeCount", layers[l - 1].nodeCount);
                GL.BindBufferBase(rangeTarget, 0, layers[l].weight_adjustments_ssbo);

                GL.DispatchCompute((layers[l - 1].nodeCount + 31) / 32, (layers[l].nodeCount + 31) / 32, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }
        }

        public void CalculateGradient(int expected_ssbo, float step) // step should be 1/batchsize
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

            

         

            calculate_bias_steps.Use();
            for(int l=1; l<layers.Length; l++) // Order doesn't matter :)
            {
                calculate_bias_steps.SetInt("nodeCount", layers[l].nodeCount);
                calculate_bias_steps.SetFloat("step", step);
                GL.BindBufferBase(rangeTarget, 0, layers[l].output_derivative_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[l].dCdN_ssbo);
                GL.BindBufferBase(rangeTarget, 2, layers[l].bias_adjustments_ssbo);

                GL.DispatchCompute((layers[l].nodeCount + 63) / 64, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }

            calculate_weight_steps.Use();
            for (int l = 1; l < layers.Length; l++) // Order doesn't matter :)
            {
                calculate_weight_steps.SetInt("nodeCount", layers[l].nodeCount);
                calculate_weight_steps.SetInt("incomingNodeCount", layers[l-1].nodeCount);
                calculate_weight_steps.SetFloat("step", step);
                GL.BindBufferBase(rangeTarget, 0, layers[l].output_derivative_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[l-1].output_ssbo);
                GL.BindBufferBase(rangeTarget, 2, layers[l].dCdN_ssbo);
                GL.BindBufferBase(rangeTarget, 3, layers[l].weight_adjustments_ssbo);

                GL.DispatchCompute((layers[l-1].nodeCount + 31) / 32, (layers[l].nodeCount + 31) / 32, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }

        }

        public void ApplyGradientStep(float step) // Seperated from GradientDescent so you can batch multiple datapoints and average them
        {
            apply_bias_steps.Use();
            for(int l=1; l<layers.Length; l++)
            {
                apply_bias_steps.SetInt("nodeCount", layers[l].nodeCount);
                apply_bias_steps.SetFloat("step", step);
                GL.BindBufferBase(rangeTarget, 0, layers[l].biases_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[l].bias_adjustments_ssbo);

                GL.DispatchCompute((layers[l].nodeCount + 63) / 64, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }

            apply_weight_steps.Use();
            for (int l = 1; l < layers.Length; l++)
            {
                apply_weight_steps.SetInt("nodeCount", layers[l].nodeCount);
                apply_weight_steps.SetInt("incomingNodeCount", layers[l-1].nodeCount);
                apply_weight_steps.SetFloat("step", step);
                GL.BindBufferBase(rangeTarget, 0, layers[l].weights_ssbo);
                GL.BindBufferBase(rangeTarget, 1, layers[l].weight_adjustments_ssbo);

                GL.DispatchCompute((layers[l - 1].nodeCount + 31) / 32, (layers[l].nodeCount + 31) / 32, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }
        }
    }
}

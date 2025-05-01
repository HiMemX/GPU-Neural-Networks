using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Neural_Networks.NetworkComponents
{
    internal class TrainingExample
    {
        public int input_ssbo;
        public int expected_output_ssbo;

        public float[] inputs;
        public float[] expected_outputs;

        public TrainingExample(float[] inputs, float[] expected_outputs)
        {
            this.inputs = inputs;
            this.expected_outputs = expected_outputs;

            input_ssbo = GL.GenBuffer();
            expected_output_ssbo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, input_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, inputs.Length * sizeof(float), inputs, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, expected_output_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, expected_outputs.Length * sizeof(float), expected_outputs, BufferUsageHint.StaticDraw);
        }

        public void WriteToInput(float[] inputs) {
            this.inputs = inputs;

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, input_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, inputs.Length * sizeof(float), inputs, BufferUsageHint.StaticDraw);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Neural_Networks
{
    public class Shader
    {
        int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string VertexShaderSource = File.ReadAllText(vertexPath);

            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compilation and error checking
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                MessageBox.Show("Error", infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                MessageBox.Show("Error", infoLog);
            }

            // Attach
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                MessageBox.Show("Error", infoLog);
            }

            // Cleanup
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);


        }

        public Shader(string computePath) // Compute shader
        {
            string ComputeShaderSource = File.ReadAllText(computePath);


            int VertexShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(VertexShader, ComputeShaderSource);

            // Compilation and error checking
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                MessageBox.Show(infoLog, computePath);
            }

            // Attach
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                //MessageBox.Show(infoLog);
            }

            // Cleanup
            GL.DetachShader(Handle, VertexShader);
            GL.DeleteShader(VertexShader);


        }

        public void SetInt(string name, int i)
        {
            GL.Uniform1(GL.GetUniformLocation(Handle, name), i);
        }
        public void SetFloat(string name, float f)
        {
            GL.Uniform1(GL.GetUniformLocation(Handle, name), f);
        }


        public void Use()
        {
            GL.UseProgram(Handle);
        }
    }
}

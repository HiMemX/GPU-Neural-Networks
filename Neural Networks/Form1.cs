using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neural_Networks.NetworkComponents;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Neural_Networks
{
    public partial class Form1 : Form
    {
        GLControl glControl;
        Shader shader;
        int VertexBufferObject;
        int VertexArrayObject;
        float[] vertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
             0.5f, -0.5f, 0.0f, //Bottom-right vertex
             0.0f,  0.5f, 0.0f  //Top vertex
        };
        public Form1()
        {
            InitializeComponent();


        }

        public void Run()
        {

            NeuralNetwork network = new NeuralNetwork(new int[] { 2, 2, 2});

            List<TrainingExample> examples = new List<TrainingExample>
            {
                new TrainingExample(new float[]{0,0 }, new float[] { 0,0}),
                new TrainingExample(new float[]{0,1 }, new float[] { 0,1}),
                new TrainingExample(new float[]{1,0 }, new float[] { 1,0}),
                new TrainingExample(new float[]{1,1 }, new float[] { 1,1}),
            };

            Stopwatch stopwatch = new Stopwatch();
            for(int i=0; i<10; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                network.Train(examples, 1f, 100);
                stopwatch.Stop();

                debugTextBox.AppendText(i.ToString() + ": " + network.GetAverageError(examples).ToString() + ", took " + stopwatch.ElapsedMilliseconds.ToString() + "ms\r\n");
            }
            
            //MessageBox.Show(String.Join(", ", result.Take(20)), elapsedMs.ToString());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            glControl = new GLControl();
            glControlPanel.Controls.Add(glControl);
            glControl.Parent = glControlPanel;
            glControl.Dock = DockStyle.Fill;
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            GL.Viewport(0, 0, glControlPanel.Width, glControlPanel.Height);


            shader = new Shader("shader.vert", "shader.frag");


            // Render Stuff
            GL.ClearColor(0,0,0, 1.0f);

            VertexBufferObject = GL.GenBuffer();

            // Probs shift this into paint when updating
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            Run();
        }


        private void glControl_Resize(object sender, EventArgs e)
        {

            glControl.MakeCurrent();    // Tell OpenGL to use MyGLControl.

            GL.Viewport(0, 0, glControlPanel.Width, glControlPanel.Height);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            glControl.MakeCurrent();    // Tell OpenGL to draw on MyGLControl.
            GL.Clear(ClearBufferMask.ColorBufferBit);                // Clear any prior drawing.

            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


            glControl.SwapBuffers();    // Display the result.
        }
    }
    
}

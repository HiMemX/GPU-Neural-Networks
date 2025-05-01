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

        public float[] Decode(int count, byte input)
        {
            float[] output = new float[count];
            output[input] = 1;
            return output;
        }

        private List<TrainingExample> ReadMNISTExamples(string path, string img, string lbls) {
            BinaryReaderEndian file;

            file = new BinaryReaderEndian(path + lbls, false);
            file.ReadBytes(4);
            uint count = file.ReadUInt32E();

            byte[] labels = new byte[count];
            for(int i=0; i<count; i++)
            {
                labels[i] = file.ReadByte();
            }

            file.Close();

            file = new BinaryReaderEndian(path + img, false);
            file.ReadBytes(4);
            count = file.ReadUInt32E();
            uint rows = file.ReadUInt32E();
            uint cols = file.ReadUInt32E();

            List<TrainingExample> examples = new List<TrainingExample>();
            float[] image;
            for(int i=0; i<count; i++)
            { 
                image = new float[rows * cols];
                for (int y=0; y<rows; y++)
                {
                    for(int x=0; x<cols; x++)
                    {
                        image[y * cols + x] = (float)file.ReadByte() / 255.0f;
                    }
                }

                examples.Add(new TrainingExample(image, Decode(10, labels[i])));
            }

            return examples;
        }

        public async void Run()
        {

            NeuralNetwork network = new NeuralNetwork(new int[] { 28*28,100, 100, 10});

            int testcount = 100;

            List<TrainingExample> trainingexamples = ReadMNISTExamples("C:\\Users\\felix\\Desktop\\Random Stuff\\MNIST_Numbers\\", "train-images.dat", "train-labels.dat");
            List<TrainingExample> alltestexamples = ReadMNISTExamples("C:\\Users\\felix\\Desktop\\Random Stuff\\MNIST_Numbers\\", "test-images.dat", "test-labels.dat");
            List<TrainingExample> testexamples = alltestexamples.GetRange(0, testcount);

            int batchcount = 100;
            int batchsize = trainingexamples.Count / batchcount;
            List<List<TrainingExample>> batches = new List<List<TrainingExample>>();
            for (int b=0; b<batchcount; b++)
            {
                batches.Add(trainingexamples.GetRange(b * batchsize, batchsize));
            }

            Stopwatch stopwatch = new Stopwatch();
            int itercount = 40;
            for(int i=0; i<itercount; i++)
            {
                for (int b = 0; b < batchcount; b++)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    network.Train(batches[b], 0.002f);
                    stopwatch.Stop();

                    if(b % 10 != 0) { continue; }

                    debugTextBox.AppendText(i.ToString() + ", " + b.ToString() + ": " + network.GetAverageError(testexamples).ToString() + ", took " + stopwatch.ElapsedMilliseconds.ToString() + "ms\r\n");

                    debugTextBox.SelectionStart = debugTextBox.Text.Length;
                    debugTextBox.ScrollToCaret();
                    await Task.Delay(10);
                }

                int correct = 0;
                float[] output;
                int largestindex;
                float largest;
                int l1;
                for (int k=0; k<((i < itercount-1) ? testcount : alltestexamples.Count); k++)
                {
                    if (i == itercount - 1) network.SetInput(alltestexamples[k].input_ssbo);
                    else network.SetInput(testexamples[k].input_ssbo);
                    network.Evaluate();
                    output = network.ReadOutput();

                    largestindex = 0;
                    largest = 0;
                    for(int x=0; x<10; x++)
                    {
                        if (output[x] > largest)
                        {
                            largest = output[x];
                            largestindex = x;
                        }
                    }
                    l1 = largestindex;
                    largestindex = 0;
                    largest = 0;
                    for (int x = 0; x < 10; x++)
                    {
                        float expected = (i < itercount - 1) ? testexamples[k].expected_outputs[x] : alltestexamples[k].expected_outputs[x];
                        if (expected > largest)
                        {
                            largest = output[x];
                            largestindex = x;
                        }
                    }

                    if(l1 == largestindex) { correct++; }
                }

                debugTextBox.AppendText(i.ToString() + ": " + correct.ToString() + "/" + ((i == itercount - 1) ? alltestexamples.Count.ToString() : testcount.ToString()) + "\r\n");

                debugTextBox.SelectionStart = debugTextBox.Text.Length;
                debugTextBox.ScrollToCaret();
                await Task.Delay(10);
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neural_Networks.NetworkComponents;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using static System.Windows.Forms.AxHost;

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

        NeuralNetwork network;
        List<TrainingExample> trainingexamples;
        List<TrainingExample> testexamples;
        List<TrainingExample> alltestexamples;

        int pixelWidth = 28;
        int pixelHeight = 28;

        public Graphics g;
        public Graphics graph;

        public Pen pen = new Pen(Color.Black, 5);

        Bitmap surface;

        Point old = new Point(0, 0);

        TrainingExample canvas; // This is where the canvas will store it's pixels in for predictions

        public Form1()
        {
            InitializeComponent();
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, drawingPanel, new object[] { true });
            //g = drawingPanel.CreateGraphics();
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            
            //pen.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);

            surface = new Bitmap(pixelWidth, pixelHeight);

            
            ResetCanvas();
            
            ///graph = Graphics.FromImage(surface);

            //drawingPanel.BackgroundImage = surface;
            //drawingPanel.BackgroundImageLayout = ImageLayout.None;

        }

        public void SetupNetwork()
        {
            network = new NeuralNetwork(new int[] { 28 * 28, 500, 100, 10 });
            canvas = new TrainingExample(new float[28 * 28], new float[10]);

            int testcount = 100;

            trainingexamples = ReadMNISTExamples("C:\\Users\\felix\\Desktop\\Random_Stuff\\MNIST_Numbers\\", "train-images.dat", "train-labels.dat");
            alltestexamples = ReadMNISTExamples("C:\\Users\\felix\\Desktop\\Random_Stuff\\MNIST_Numbers\\", "test-images.dat", "test-labels.dat");
            testexamples = alltestexamples.GetRange(0, testcount);

        }

        public void ResetCanvas()
        {
            for (int x = 0; x < pixelWidth; x++)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    SetPixel(x, y, Color.White);
                }
            }

            drawingPanel.Invalidate();
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

            
            
            int batchcount = 100;
            int batchsize = trainingexamples.Count / batchcount;
            List<List<TrainingExample>> batches = new List<List<TrainingExample>>();
            for (int b=0; b<batchcount; b++)
            {
                batches.Add(trainingexamples.GetRange(b * batchsize, batchsize));
            }

            Stopwatch stopwatch = new Stopwatch();
            int itercount = 10;
            for(int i=0; i<itercount; i++)
            {
                for (int b = 0; b < batchcount; b++)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    network.Train(batches[b], 0.0020f);
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
                for (int k=0; k<testexamples.Count; k++)
                {
                    network.SetInput(testexamples[k].input_ssbo);
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

                debugTextBox.AppendText(i.ToString() + ": " + correct.ToString() + "/" + testexamples.Count.ToString() + "\r\n");

                debugTextBox.SelectionStart = debugTextBox.Text.Length;
                debugTextBox.ScrollToCaret();
                await Task.Delay(10);
            }

            //MessageBox.Show(String.Join(", ", result.Take(20)), elapsedMs.ToString());
        }

        public void UpdatePrediction()
        {
            // Upload data to training example
            float avrgx = 0;
            float avrgy = 0;
            float sum = 0;

            for (int x = 0; x < 28; x++)
            {
                for (int y = 0; y < 28; y++)
                {
                    float value = (1 - (float)surface.GetPixel(x, y).R / 255.0f);
                    sum += value;
                    avrgx += value * x;
                    avrgy += value * y;
                }
            }
            int xoffset = (int)(avrgx / sum)  -14;
            int yoffset = (int)(avrgy / sum) -14;

            debugTextBox.AppendText(xoffset + ", " + yoffset + "\r\n\r\n");

            int xcord;
            int ycord;
            float[] image = new float[28*28];
            for(int x=0; x<28; x++) {
                for (int y= 0; y < 28; y++)
                {
                    xcord = x - xoffset;
                    ycord = y - yoffset;

                    if (xcord < 0 || xcord >= 28) continue;
                    if (ycord < 0 || ycord >= 28) continue;


                    image[y*28 + x] = 1 - (float)surface.GetPixel(xcord, ycord).R / 255.0f;
                }
            }

            canvas.WriteToInput(image);

            network.SetInput(canvas.input_ssbo);
            network.Evaluate();
            float[] outputs = network.ReadOutput();
            sum = 0;

            for (int i = 0; i < 10; i++)
            {
                sum += outputs[i];
            }

            //debugTextBox.AppendText(String.Join("\r\n", outputs) + "\r\n\r\n");
            for (int i = 0; i < 10; i++)
            {
                debugTextBox.AppendText(i.ToString() + ": " + (outputs[i] / sum).ToString("0.00") + "\r\n");
            }
            debugTextBox.AppendText("\r\n");

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

            SetupNetwork();
            //Run();
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

        private void drawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            old = e.Location;
        }

        public static Color ScaleColor(Color color, float scale)
        {
            int r = (int)(color.R * scale);
            int g = (int)(color.G * scale);
            int b = (int)(color.B * scale);

            // Clamp values to 0–255
            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color InvertColor(Color color)
        {
            int r = (int)(255 -color.R);
            int g = (int)(255-color.G);
            int b = (int)(255 - color.B);

            return Color.FromArgb(color.A, r, g, b);
        }
        public static Color SubtractColor(Color colora, Color colorb)
        {
            int r = (int)(colora.R - colorb.R);
            int g = (int)(colora.G - colorb.G);
            int b = (int)(colora.B - colorb.B);
            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return Color.FromArgb(colora.A, r, g, b);
        }

        public static Color BlendColor(Color colora, Color colorb)
        {

            int r = (int)( (float)((colora.R) * ( colorb.R)) / 255.0f);
            int g = (int)( (float)((colora.G) * ( colorb.G)) / 255.0f);
            int b = (int)( (float)((colora.B) * ( colorb.B)) / 255.0f);

            return Color.FromArgb(colora.A, r, g, b);
        }

        private void drawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left) { return; }


            int scale = 20;
            float xf = (float)e.X / (float)scale;
            float yf = (float)e.Y / (float)scale;

            int x = e.X / scale;
            int y = e.Y / scale;

            if (x == old.X / scale && (y == old.Y / scale)) { return; }

            float xp = xf - x;
            float yp = yf - y;

            Color c = Color.White;

            Color c1 = ScaleColor(c, 0.1f);
            Color c2 = ScaleColor(c, 0.4f);
            Color c3 = ScaleColor(c, 0.75f);



            BlendPixel(x, y, c1);
            BlendPixel(x+1, y, c2);
            BlendPixel(x, y+1, c2);
            BlendPixel(x+1, y+1, c3);

            BlendPixel(x - 1, y, c2);
            BlendPixel(x, y - 1, c2);
            BlendPixel(x - 1, y - 1, c3);

            BlendPixel(x - 1, y + 1, c3);
            BlendPixel(x + 1, y - 1, c3);

            drawingPanel.Invalidate(); // Redraw
            UpdatePrediction();
            old = e.Location;
        }

        private void SetPixel(int x, int y, Color c)
        {
            if (x >= 0 && x < pixelWidth && y >= 0 && y < pixelHeight)
            {
                surface.SetPixel(x, y, c); // Or use a selected color
                
            }
        }

        private void BlendPixel(int x, int y, Color c)
        {
            if (x >= 0 && x < pixelWidth && y >= 0 && y < pixelHeight)
            {
                Color original = surface.GetPixel(x, y); // Or use a selected color
                surface.SetPixel(x, y, BlendColor(original, c));
            }
        }

        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            int scale = 20; // Each pixel will be 20x20 screen pixels
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor; // Prevent smoothing
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half; // Optional, sharper results

            e.Graphics.DrawImage(surface, new Rectangle(0, 0, pixelWidth * scale, pixelHeight * scale));

        }

        private void canvasResetButton_Click(object sender, EventArgs e)
        {
            ResetCanvas();
        }

        private void startTrainingButton_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void randomTestButton_Click(object sender, EventArgs e)
        {
            Random rng = new Random();
            TrainingExample example = alltestexamples[rng.Next(alltestexamples.Count)];

            for(int y=0; y<28; y++)
            {
                for(int x=0; x<28; x++)
                {
                    surface.SetPixel(x, y, ScaleColor(Color.White, 1 - example.inputs[y*28+x]));
                }
            }

            drawingPanel.Invalidate();
            UpdatePrediction();
        }
    }
    
}

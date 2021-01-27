using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace SoftEngine
{
    public partial class Form1 : Form
    {
        private Stopwatch stopwatch = new Stopwatch();
        private long runTIme;
        private Device device;
        Mesh mesh = new Mesh("Cube", 8, 12);
        Camera mera = new Camera();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(640, 480);

            device = new Device(bmp);

            mesh.Vertices[0] = new Vector3(-1, 1, 1);
            mesh.Vertices[1] = new Vector3(1, 1, 1);
            mesh.Vertices[2] = new Vector3(-1, -1, 1);
            mesh.Vertices[3] = new Vector3(1, -1, 1);
            mesh.Vertices[4] = new Vector3(-1, 1, -1);
            mesh.Vertices[5] = new Vector3(1, 1, -1);
            mesh.Vertices[6] = new Vector3(1, -1, -1);
            mesh.Vertices[7] = new Vector3(-1, -1, -1);

            mesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            mesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            mesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            mesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            mesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            mesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };

            mesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            mesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            mesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            mesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            mesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            mesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            mera.Position = new Vector3(0, 0, 0.05f);
            mera.Target = Vector3.Zero;
            stopwatch.Start();

        }
        int i = 0;
        private void Form1_Paint_1(object sender, PaintEventArgs e)
        {
            //创建画板,这里的画板是由Form提供的.
            Graphics grfc = e.Graphics;
            ////定义了一个绿色,宽度为的画笔
            //Pen pen = new Pen(Color.Green, 2);
            ////在画板上画直线,起始坐标为(this.Width / 4, this.Height/4),终点坐标为(this.Width*3 / 4, this.Height*3 / 4)
            //grfc.DrawLine(pen, this.Width / 4 + i++, this.Height / 4, this.Width * 3 / 4, this.Height * 3 / 4);
            ////在画板上画矩形,起始坐标为(this.Width / 4, this.Height/4),宽为,高为
            //grfc.DrawRectangle(pen, this.Width / 4, this.Height / 4, this.Width / 2, this.Height / 2);
            ////在画板上画椭圆,起始坐标为(this.Width / 4, this.Height / 4),外接矩形的宽为200,高为200
            //grfc.DrawEllipse(pen, this.Width / 4, this.Height / 4, 200, 200);
            ////绘制文字，起点为160，160；
            //grfc.DrawString("Drawing is here!", new Font("宋体", 28), new SolidBrush(Color.Blue), new PointF(160, 160));

            //backBuffer = new byte[800 .];
            // Graphics grfc = e.Graphics;
            //Bitmap canvans = new Bitmap(ClientSize.Width, ClientSize.Height);

            //for (int i = 0; i < canvans.Width/2; i++)
            //{
            //    for (int j = 0; j < canvans.Height/2; j++)

            //        canvans.SetPixel(i, j, Color.Red);
            //}

            //grfc.DrawImage(canvans, new Point(0, 0));


            device.Clear(0, 0, 0, 255);

            // rotating slightly the cube during each frame rendered
            mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);

            //// Doing the various matrix operations
            device.Render(mera, mesh);
            // Flushing the back buffer into the front buffer
            device.Present(grfc);
            grfc.DrawString((stopwatch.ElapsedMilliseconds - runTIme).ToString(), new Font("宋体", 28), new SolidBrush(Color.Blue), new PointF(10, 10));
            runTIme = stopwatch.ElapsedMilliseconds;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine(stopwatch.ElapsedMilliseconds - runTIme);
            //runTIme = stopwatch.ElapsedMilliseconds;
            Rectangle rectangle = new Rectangle(0, 0, 600, 480);
            this.Invalidate(rectangle, false);
        }
    }
}

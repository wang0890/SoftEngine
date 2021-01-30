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
        Mesh[] meshes;
        Camera mera = new Camera();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(640, 480);

            device = new Device(bmp);
            meshes = device.LoadJSONFileAsync("Json\\monkey.txt");
            mera.Position = new Vector3(0, 0, 0.02f);
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

            foreach (var mesh in meshes)
            {
                // rotating slightly the meshes during each frame rendered
                mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
            }

            // Doing the various matrix operations
            device.Render(mera, meshes);
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

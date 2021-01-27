using System.Runtime.InteropServices.WindowsRuntime;
using System.Numerics;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.IO;

namespace SoftEngine
{
    public class Device
    {
        private byte[] backBuffer;
        private Bitmap bmp;
        PointBitmap lockbmp;

        public Device(Bitmap bmp)
        {
            this.bmp = bmp;
            // the back buffer size is equal to the number of pixels to draw
            // on screen (width*height) * 4 (R,G,B & Alpha values). 
            backBuffer = new byte[bmp.Width * bmp.Height * 4];
            lockbmp = new PointBitmap(bmp);
        }

        // This method is called to clear the back buffer with a specific color
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }
        }

        // Once everything is ready, we can flush the back buffer
        // into the front buffer. 
        public void Present(Graphics grfc)
        {
            lockbmp.LockBits();


            int x, y = 0;
            y = 4 * bmp.Width;
            int index = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                x = 4 * i;
                index = x - y;
                for (int j = 0; j < bmp.Height; j++)
                {
                    index += y;
                    lockbmp.SetPixel(i, j, Color.FromArgb(backBuffer[index + 3], backBuffer[index + 2], backBuffer[index + 1], backBuffer[index]));
                }

            }
            lockbmp.UnlockBits();
            grfc.Clear(Color.Red);
            grfc.DrawImage(bmp, new Point(0, 0));
        }


        // Called to put a pixel on screen at a specific X,Y coordinates
        public void PutPixel(int x, int y, Color color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * bmp.Width) * 4;

            backBuffer[index] = color.B;
            backBuffer[index + 1] = color.G;
            backBuffer[index + 2] = color.R;
            backBuffer[index + 3] = color.A;
        }

        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix
        public Vector2 Project(Vector3 coord, Matrix4x4 transMat)
        {
            // transforming the coordinates
            var point = Vector3.Transform(coord, transMat);
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X / transMat.M44 + bmp.Width / 2.0f;
            var y = -point.Y / transMat.M44 + bmp.Height / 2.0f;
            return (new Vector2(x, y));
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(Vector2 point)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.Width && point.Y < bmp.Height)
            {
                // Drawing a yellow point
                PutPixel((int)point.X, (int)point.Y, Color.FromArgb(255, 255, 255, 255));
            }
        }

        // The main method of the engine that re-compute each vertex projection
        // during each frame
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // To understand this part, please read the prerequisites resources
            var viewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(0.78f,
                                                           (float)bmp.Width / bmp.Height,
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                // Beware to apply rotation before translation 
                var worldMatrix = Matrix4x4.CreateFromYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix4x4.CreateTranslation(mesh.Position);


                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (var vertex in mesh.Vertices)
                {
                    // First, we project the 3D coordinates into the 2D space
                    var point = Project(vertex, transformMatrix);
                    // Then we can draw on screen
                    DrawPoint(point);
                }
            }
        }
    }
}

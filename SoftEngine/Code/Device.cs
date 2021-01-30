﻿using System.Runtime.InteropServices.WindowsRuntime;
using System.Numerics;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace SoftEngine
{
    public class Device
    {
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private Bitmap bmp;
        PointBitmap lockbmp;

        private object[] lockBuffer;

        private readonly int renderWidth;
        private readonly int renderHeight;
        public Device(Bitmap bmp)
        {
            this.bmp = bmp;
            // the back buffer size is equal to the number of pixels to draw
            // on screen (width*height) * 4 (R,G,B & Alpha values). 
            renderWidth = bmp.Width;
            renderHeight = bmp.Height;
            backBuffer = new byte[bmp.Width * bmp.Height * 4];
            lockbmp = new PointBitmap(bmp);
            depthBuffer = new float[bmp.Width * bmp.Height];

            lockBuffer = new object[bmp.Width * bmp.Height];

            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }

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

            // Clearing Depth Buffer
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
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
        public void PutPixel(int x, int y, float z, Color color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen


            var index = (x + y * renderWidth);
            var index4 = index * 4;
            lock (lockBuffer[index])
            {
                if (depthBuffer[index] < z)
                {
                    return; // Discard
                }

                depthBuffer[index] = z;


                backBuffer[index4] = color.B;
                backBuffer[index4 + 1] = color.G;
                backBuffer[index4 + 2] = color.R;
                backBuffer[index4 + 3] = color.A;
            }
        }

        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix
        public Vector3 Project(Vector3 coord, Matrix4x4 transMat)
        {
            // transforming the coordinates
            var point = Vector3.Transform(coord, transMat);
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X / transMat.M44 + renderWidth / 2.0f;
            var y = -point.Y / transMat.M44 + renderHeight / 2.0f;
            return (new Vector3(x, y, point.Z));
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(Vector3 point, Color color)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
            {
                // Drawing a yellow point
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }
        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        void ProcessScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, Color color)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                DrawPoint(new Vector3(x, y, z), color);
            }
        }


        // Clamping values to keep them between 0 and 1
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }



        public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            // computing lines' directions
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(y, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p1, p3, p2, p3, color);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p2, p3, p1, p3, color);
                    }
                }
            }

            //if (dP1P2 > dP1P3)
            //{
            //    Parallel.For((int)p1.Y, (int)p3.Y + 1, y =>
            //        {
            //            if (y < p2.Y)
            //            {
            //                ProcessScanLine(y, p1, p3, p1, p2, color);
            //            }
            //            else
            //            {
            //                ProcessScanLine(y, p1, p3, p2, p3, color);
            //            }
            //        });
            //}
            //else
            //{
            //    Parallel.For((int)p1.Y, (int)p3.Y + 1, y =>
            //        {
            //            if (y < p2.Y)
            //            {
            //                ProcessScanLine(y, p1, p2, p1, p3, color);
            //            }
            //            else
            //            {
            //                ProcessScanLine(y, p2, p3, p1, p3, color);
            //            }
            //        });
            //}
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

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    var face = mesh.Faces[faceIndex];
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix);
                    var pixelB = Project(vertexB, transformMatrix);
                    var pixelC = Project(vertexC, transformMatrix);

                    var color = 0.25f + (faceIndex % mesh.Faces.Length) * 0.75f / mesh.Faces.Length;
                    DrawTriangle(pixelA, pixelB, pixelC, Color.FromArgb((int)(color * 255), (int)(color * 255), (int)(color * 255), 255));
                    faceIndex++;
                });
            }
        }
        // Loading the JSON file in an asynchronous manner
        public Mesh[] LoadJSONFileAsync(string fileName)
        {
            var meshes = new List<Mesh>();

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string jsonText = ReadString(Path.Combine(baseDirectory, fileName));
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText, jSetting);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    mesh.Vertices[index] = new Vector3(x, y, z);
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c };
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }

        public string ReadString(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.AppendLine(sr.ReadLine());
            }
            return sb.ToString();
        }
    }
}

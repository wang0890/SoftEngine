using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SoftEngine
{
    public class Texture
    {
        private byte[] internalBuffer;
        private int width;
        private int height;

        // Working with a fix sized texture (512x512, 1024x1024, etc.).
        public Texture(string filename, int width, int height)
        {
            this.width = width;
            this.height = height;
            Load(filename);
        }

        void Load(string filename)
        {
            // var file = System.Drawing.Image.FromFile(filename);
            //using (var stream = await file.OpenReadAsync())
            //{
            //    var bmp = new WriteableBitmap(width, height);
            //    bmp.SetSource(stream);

            //    internalBuffer = bmp.PixelBuffer.ToArray();
            //}

            //Stream ms = new MemoryStream();

            //file.Save(ms, ImageFormat.Jpeg);

            //ms.Seek(0, SeekOrigin.Begin); //一定不要忘记将流的初始位置重置


            //internalBuffer = new byte[ms.Length];
            //ms.Read(internalBuffer, 0, internalBuffer.Length); //如果上面流没有seek 则这里读取的数据全会为0

            //ms.Dispose();


            //FileStream sFile = new FileStream(filename, FileMode.Open);
            //获取文件长度 
            //int nFileLength = (int)sFile.Seek(0, SeekOrigin.End);
            //修正偏移 
            //sFile.Seek(0, SeekOrigin.Begin);
            //申请空间 
            //internalBuffer = new byte[nFileLength];
            //读文件 
            //sFile.Read(internalBuffer, 0, nFileLength);
            //sFile.Close();

            LockBitmap bitMap = new LockBitmap(new Bitmap(filename));
            bitMap.LockBits();
            internalBuffer = bitMap.Pixels;
            bitMap.UnlockBits();
        }

        public byte[] GetPictureData(string imagePath)
        {
            FileStream fs = new FileStream(imagePath, FileMode.Open);
            byte[] byteData = new byte[fs.Length];
            fs.Read(byteData, 0, byteData.Length);
            fs.Close();
            return byteData;
        }
        public static byte[] BitmapByte(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
        }
        // Takes the U & V coordinates exported by Blender
        // and return the corresponding pixel color in the texture
        public Color Map(float tu, float tv)
        {
            // Image is not loaded yet
            if (internalBuffer == null)
            {
                return Color.White;
            }
            // using a % operator to cycle/repeat the texture if needed
            int u = Math.Abs((int)(tu * width) % width);
            int v = Math.Abs((int)(tv * height) % height);

            int pos = (u + v * width) * 3;
           // Console.WriteLine(pos);
            byte b = internalBuffer[pos];
            byte g = internalBuffer[pos + 1];
            byte r = internalBuffer[pos + 2];
            byte a = internalBuffer[pos + 3];

            return Color.FromArgb(a, r, g, b);
        }
    }
}

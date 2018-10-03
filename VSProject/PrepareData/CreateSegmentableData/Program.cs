using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CreateSegmentableData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Where is the working-folder located? (full path to folder)");
            String rootPath = Console.ReadLine();
            String jsonPath = Path.Combine(rootPath, "labels.json");
            String imagePath = Path.Combine(jsonPath.Substring(0, jsonPath.LastIndexOf('\\')), "Bilder");
            String[] fileNames = Directory.GetFiles(imagePath);
            String targetDirectoryImages = Path.Combine(rootPath, "changed");
            Directory.CreateDirectory(targetDirectoryImages);

            int currentImageCount = 0;
            Image currentImage = Image.FromFile(Path.Combine(imagePath, fileNames[currentImageCount]));

            JSON_Reader rootFiles = new JSON_Reader(jsonPath);
            JSON_Reader createdFiles = new JSON_Reader(Path.Combine(rootPath, "created.json"));




            Console.WriteLine("-----------Configuration----------");
            Console.WriteLine("First Image is: " + getPureFileName(fileNames[currentImageCount], true) + " | " + currentImage.Width + " x " + currentImage.Height);

            Console.WriteLine("Max rotation in degree? (default 360)");
            string maxRot = Console.ReadLine();
            double maxRotation = maxRot.Equals("") ? 360 : Double.Parse(maxRot);

            Console.WriteLine("Max shift horizontal? (default (1/4)*ImageWidth)");
            string maxShiftHor = Console.ReadLine();
            int maxShift_x = maxShiftHor.Equals("") ? (int)(0.25 * currentImage.Width) : Int32.Parse(maxShiftHor);

            Console.WriteLine("Max shift vertical? (default (1/4)*ImageHeight)");
            string maxShiftVer = Console.ReadLine();
            int maxShift_y = maxShiftVer.Equals("") ? (int)(0.25 * currentImage.Height) : Int32.Parse(maxShiftVer);

            Console.WriteLine("How much samples to create from 1 Image? (default 30)");
            string sampleCountIn = Console.ReadLine();
            int sampleCount = sampleCountIn.Equals("") ? 30 : Int32.Parse(sampleCountIn);

            Console.WriteLine("Generating Images...");

            foreach (JsonFile file in rootFiles.getFiles())
            {
                Console.Write(file.getFilename() + ": ");
                Random rnd = new Random();
                Bitmap currentBitmap = (Bitmap)Bitmap.FromFile(Path.Combine(imagePath, file.getFilename()));

                createdFiles.addFile(file);
                currentBitmap.Save(Path.Combine(targetDirectoryImages, file.getFilename()), ImageFormat.Jpeg);

                for (int i = 0; i < sampleCount; i++)
                {
                    float angle = (float)(rnd.Next(-50, 50) * maxRotation / 100.0);
                    int shift_x = (int)(rnd.Next(-50, 50) * maxShift_x / 100);
                    int shift_y = (int)(rnd.Next(-50, 50) * maxShift_y / 100);
                    
                    Bitmap changed = changeImage(currentBitmap, angle, shift_x, shift_y);
                    

                    string fileName = generateFileName(file.getFilename(), i);
                    String newFilePath = Path.Combine(targetDirectoryImages, fileName);
                    changed.Save(newFilePath, ImageFormat.Png);

                    createdFiles.addFile(file.getChanged(newFilePath, angle, shift_x, shift_y));
                    Console.Write("#");
                }

                createdFiles.writeToFile();
                Console.WriteLine();
            }

        }

        private static Bitmap changeImage(Bitmap bitmap, float angle, int shift_x, int shift_y)
        {
            Point center = new Point(bitmap.Width / 2, bitmap.Height / 2);

            Bitmap returnBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics graphics = Graphics.FromImage(returnBitmap);
            graphics.TranslateTransform(center.X, center.Y);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-center.X, -center.Y);
            graphics.DrawImage(bitmap, new Point(shift_x, shift_y));
            return returnBitmap;
        }

        private static string getPureFileName(string file, bool withEnding)
        {

            if (withEnding)
                return Path.GetFileName(file);
            return Path.GetFileName(file).Substring(0, Path.GetFileName(file).Length - 4);
        }

        private static string generateFileName(string filename, int i)
        {
            string pureFileName = filename.Substring(0, filename.LastIndexOf('.'));
            string numberSuffix = i.ToString();
            if (i < 10)
            {
                numberSuffix = "00" + i;
            }
            else if (i < 100)
            {
                numberSuffix = "0" + i;
            }

            return (pureFileName + "_" + numberSuffix + filename.Substring(filename.LastIndexOf('.')));
        }

    }
}

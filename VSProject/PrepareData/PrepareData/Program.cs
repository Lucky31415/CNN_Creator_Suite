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
using IronPython.Hosting;

namespace PrepareData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Where are the images located? (full path to folder)");
            string sourceImages = Console.ReadLine();
            string[] fileNames = Directory.GetFiles(sourceImages);

            string targetDirectoryImages = Path.Combine(sourceImages, "images");
            string directoryAnnotations = Path.Combine(sourceImages, "annotations");
            string targetDirectoryXmls = Path.Combine(directoryAnnotations, "xmls");
            Directory.CreateDirectory(targetDirectoryImages);
            Directory.CreateDirectory(directoryAnnotations);
            Directory.CreateDirectory(targetDirectoryXmls);

            int currentImageCount = 0;
            //StreamWriter currentLabelFile = new StreamWriter(Path.Combine(targetDirectoryXmls, getPureFileName(fileNames[0], false) + ".txt"), false);
            XDocument currentLabelFile = XDocument.Load(Path.Combine(targetDirectoryXmls, getPureFileName(fileNames[0], false) + ".xml"));
            Image currentImage = Image.FromFile(Path.Combine(sourceImages, fileNames[currentImageCount]));

            Console.WriteLine("Name of final \".tfrecord\"-file:");
            string recordFileName = Console.ReadLine();

            /*
            foreach (string file in fileNames)
            {
                currentImage = Image.FromFile(Path.Combine(sourceImages, fileNames[currentImageCount]));
                using (var b = new Bitmap(currentImage.Width, currentImage.Height))
                {
                    b.SetResolution(currentImage.HorizontalResolution, currentImage.VerticalResolution);

                    using (var g = Graphics.FromImage(b))
                    {
                        g.Clear(Color.White);
                        g.DrawImageUnscaled(currentImage, 0, 0);
                    }

                    b.Save(Path.Combine(targetDirectoryImages, Path.ChangeExtension(Path.GetFileNameWithoutExtension(file), ".jpg")), ImageFormat.Jpeg);
                }
            }*/

            /*
            List<string> useFiles = new List<string>();
            foreach (string file in fileNames)
            {
                
                Console.WriteLine("----------------------" + getPureFileName(file, false) + "----------------------------");
                Console.WriteLine("\"" + getPureFileName(file, true) + "\"" + " Use this file? (y/n)");
                string answer = Console.ReadLine();

                if (answer.Equals("y"))
                {
                    Console.Write("\tLabel: ");
                    string cLabel = Console.ReadLine();
                    useFiles.Add(file);
                    labelFile.WriteLine(getPureFileName(file, false) + ":" + cLabel);
                }
            }
            labelFile.Dispose();
            */

            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-----------Configuration----------");
            Console.WriteLine("First Image is: " + getPureFileName(fileNames[currentImageCount], true) + " | " + currentImage.Width + " x " + currentImage.Height);
            string targetWidth = currentLabelFile.Descendants("width").First().Value;
            int targetSize_x = Int32.Parse(targetWidth);
            Console.WriteLine("Target width: " + targetSize_x);

            string targetHeight = currentLabelFile.Descendants("width").First().Value;
            int targetSize_y = Int32.Parse(targetHeight);
            Console.WriteLine("Target height: " + targetSize_y);

            Console.WriteLine("Max rotation in degree? (default 360)");
            string maxRot = Console.ReadLine();
            double maxRotation = maxRot.Equals("") ? 360 : Double.Parse(maxRot);

            Console.WriteLine("Max shift horizontal? (default (1/4)*targetWidth)");
            string maxShiftHor = Console.ReadLine();
            int maxShift_x = maxShiftHor.Equals("") ? (int)(0.25 * targetSize_x) : Int32.Parse(maxShiftHor);

            Console.WriteLine("Max shift vertical? (default (1/4)*targetHeight)");
            string maxShiftVer = Console.ReadLine();
            int maxShift_y = maxShiftVer.Equals("") ? (int)(0.25 * targetSize_y) : Int32.Parse(maxShiftVer);

            Console.WriteLine("How much samples to create from 1 Image? (default 30)");
            string sampleCountIn = Console.ReadLine();
            int sampleCount = sampleCountIn.Equals("") ? 30 : Int32.Parse(sampleCountIn);

            Console.WriteLine("Generating Images...");


            StreamWriter trainvalFile = new StreamWriter(Path.Combine(directoryAnnotations, "trainval.txt"), false);
            StreamWriter label_mapFile = new StreamWriter(Path.Combine(directoryAnnotations, "label_map.pbtxt"), false);
            List<String> labels = new List<string>();
            foreach (string file in fileNames)
            {
                Console.Write(getPureFileName(file, true) + ": ");
                Random rnd = new Random();
                Bitmap currentBitmap = (Bitmap)Bitmap.FromFile(file);
                currentLabelFile = XDocument.Load(Path.Combine(targetDirectoryXmls, getPureFileName(file, false) + ".xml"));
                currentBitmap.Save(Path.Combine(targetDirectoryImages, getPureFileName(file, true)), ImageFormat.Jpeg);
                trainvalFile.WriteLine(getPureFileName(file, false));

                for (int i = 0; i < sampleCount; i++)
                {
                    float angle = (float)(rnd.Next(-50, 50) * maxRotation / 100.0);
                    int shift_x = (int)(rnd.Next(-50, 50) * maxShift_x / 100);
                    int shift_y = (int)(rnd.Next(-50, 50) * maxShift_y / 100);

                    XDocument changedLabelFile = new XDocument(currentLabelFile);

                    changeLabel(currentBitmap, angle, shift_x, shift_y, changedLabelFile, labels);
                    Bitmap changed = changeImage(currentBitmap, angle, shift_x, shift_y, changedLabelFile);

                    string fileName = generateFileName(file, i);
                    changedLabelFile.Descendants("filename").First().SetValue(getPureFileName(fileName, true));
                    changedLabelFile.Save(Path.Combine(targetDirectoryXmls, getPureFileName(fileName, false) + ".xml"));
                    changed.Save(Path.Combine(targetDirectoryImages, fileName), ImageFormat.Png);
                    trainvalFile.WriteLine(getPureFileName(fileName, false));
                    Console.Write("#");
                }

                Console.WriteLine();
            }

            int id = 1;
            foreach(String label in labels.Distinct())
            {
                label_mapFile.WriteLine("item {");
                label_mapFile.WriteLine("    id: " + id);
                label_mapFile.WriteLine("    name: \'" + label + "\'");
                label_mapFile.WriteLine("}");
                label_mapFile.WriteLine("");
                id++;
            }

            trainvalFile.Dispose();
            label_mapFile.Dispose();

            //var py = Python.CreateEngine();
            //List<string> arguments = new List<string>();
            //arguments.Add("packer.py");
            //arguments.Add(targetSize_x.ToString());
            //arguments.Add(targetSize_y.ToString());
            //arguments.Add("2");
            //arguments.Add(recordFileName);
            //py.ExecuteFile(arguments[0]);

    

        }

        private static Bitmap changeImage(Bitmap bitmap, float angle, int shift_x, int shift_y, XDocument labelFile)
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

        private static void changeLabel(Bitmap bitmap, float angle, int shift_x, int shift_y, XDocument labelFile, List<String> labels)
        {
            Point center = new Point(bitmap.Width / 2, bitmap.Height / 2);
            double radianAngle = angle * (Math.PI / 180.0);

            IEnumerable<XElement> markedObjects = labelFile.Descendants("object");
            foreach (XElement o in markedObjects)
            {
                labels.Add(o.Descendants("name").First().Value);

                int xMin = Int32.Parse(o.Descendants("xmin").First().Value);
                int yMin = Int32.Parse(o.Descendants("ymin").First().Value);
                int xMax = Int32.Parse(o.Descendants("xmax").First().Value);
                int yMax = Int32.Parse(o.Descendants("ymax").First().Value);

                Point min = new Point(xMin, yMin);
                Point max = new Point(xMax, yMax);
                Point topRight = new Point(xMax, yMin);
                Point botLeft = new Point(xMin, yMax);

                Point newMin = RotatePoint(min, center, radianAngle);
                Point newMax = RotatePoint(max, center, radianAngle);
                Point newTopRight = RotatePoint(topRight, center, radianAngle);
                Point newBotLeft = RotatePoint(botLeft, center, radianAngle);

                List<int> x_Values = new List<int>();
                x_Values.Add(newMin.X);
                x_Values.Add(newMax.X);
                x_Values.Add(newTopRight.X);
                x_Values.Add(newBotLeft.X);

                List<int> y_Values = new List<int>();
                y_Values.Add(newMin.Y);
                y_Values.Add(newMax.Y);
                y_Values.Add(newTopRight.Y);
                y_Values.Add(newBotLeft.Y);


                Point newBoxMin = new Point(x_Values.Min(), y_Values.Min());
                Point newBoxMax = new Point(x_Values.Max(), y_Values.Max());


                newBoxMin.X += shift_x;
                newBoxMax.X += shift_x;
                newBoxMin.Y += shift_y;
                newBoxMax.Y += shift_y;

                newBoxMin.X = newBoxMin.X < 0 ? 0 : newBoxMin.X;
                newBoxMin.X = newBoxMin.X > bitmap.Width ? bitmap.Width - 1 : newBoxMin.X;

                newBoxMax.X = newBoxMax.X < 0 ? 0 : newBoxMax.X;
                newBoxMax.X = newBoxMax.X > bitmap.Width ? bitmap.Width - 1 : newBoxMax.X;

                newBoxMin.Y = newBoxMin.Y < 0 ? 0 : newBoxMin.Y;
                newBoxMin.Y = newBoxMin.Y > bitmap.Height ? bitmap.Height - 1 : newBoxMin.Y;

                newBoxMax.Y = newBoxMax.Y < 0 ? 0 : newBoxMax.Y;
                newBoxMax.Y = newBoxMax.Y > bitmap.Height ? bitmap.Height - 1 : newBoxMax.Y;

                o.Descendants("xmin").First().SetValue(newBoxMin.X);
                o.Descendants("ymin").First().SetValue(newBoxMin.Y);
                o.Descendants("xmax").First().SetValue(newBoxMax.X);
                o.Descendants("ymax").First().SetValue(newBoxMax.Y);
            }
        }

        private static string generateFileName(string file, int i)
        {
            int l = file.Length;
            int s1 = file.LastIndexOf("\\");
            int s2 = file.LastIndexOf('.');
            string pureFileName = file.Substring(s1 + 1, l - file.Substring(0, s1).Length - file.Substring(s2).Length - 1);
            string numberSuffix = i.ToString();
            if (i < 10)
            {
                numberSuffix = "00" + i;
            } else if (i < 100)
            {
                numberSuffix = "0" + i;
            }

            return (pureFileName + "_" + numberSuffix + file.Substring(file.LastIndexOf('.')));
        }

        private static string getPureFileName(string file, bool withEnding)
        {
            
            if (withEnding)
                return Path.GetFileName(file);
            return Path.GetFileName(file).Substring(0, Path.GetFileName(file).Length - 4);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInRadians)
        {
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
    }
}

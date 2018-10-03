using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CNN_Creator_Trainer_Evaluater
{
    class ODImageCreator
    {
        public ODImageCreator(string sourceImagePath, string sourceLabelFileDirectory, string targetImageDirectory, string targetAnnotationDirectory, 
                              double maxRot = 360, int maxShiftHor = -1, int maxShiftVer = -1, int sampleCount = 30)
        {
            string[] fileNames = Directory.GetFiles(sourceImagePath);
            string targetXmlDirectory = Path.Combine(targetAnnotationDirectory, "xmls");

            int currentImageCount = 0;
            XDocument currentLabelFile = XDocument.Load(Path.Combine(sourceLabelFileDirectory, Path.GetFileNameWithoutExtension(fileNames[0]) + ".xml"));
            Image currentImage = Image.FromFile(Path.Combine(sourceImagePath, fileNames[currentImageCount]));

            StreamWriter trainvalFile = new StreamWriter(Path.Combine(targetAnnotationDirectory, "trainval.txt"), false);
            StreamWriter label_mapFile = new StreamWriter(Path.Combine(targetAnnotationDirectory, "label_map.pbtxt"), false);
            List<String> labels = new List<string>();
            foreach (string file in fileNames)
            {
                if (file.Contains("Thumbs.db"))
                    break;

                Random rnd = new Random();
                Bitmap currentBitmap = (Bitmap)Bitmap.FromFile(file);
                currentLabelFile = XDocument.Load(Path.Combine(sourceLabelFileDirectory, Path.GetFileNameWithoutExtension(file) + ".xml"));
                currentLabelFile.Descendants("filename").First().SetValue(Path.GetFileNameWithoutExtension(file) + ".jpg");
                currentLabelFile.Save(Path.Combine(targetXmlDirectory, Path.GetFileNameWithoutExtension(file) + ".xml"));
                currentBitmap.Save(Path.Combine(targetImageDirectory, Path.GetFileNameWithoutExtension(file) + ".jpg"), ImageFormat.Jpeg);
                trainvalFile.WriteLine(Path.GetFileNameWithoutExtension(file));

                for (int i = 0; i < sampleCount; i++)
                {
                    int maxShift_x = maxShiftHor == -1 ? (int)(0.25 * currentBitmap.Width) : maxShiftHor;
                    int maxShift_y = maxShiftVer == -1 ? (int)(0.25 * currentBitmap.Height) : maxShiftVer;

                    float angle = (float)(rnd.Next(-50, 50) * maxRot / 100.0);
                    int shift_x = (int)(rnd.Next(-50, 50) * maxShift_x / 100);
                    int shift_y = (int)(rnd.Next(-50, 50) * maxShift_y / 100);

                    XDocument changedLabelFile = new XDocument(currentLabelFile);

                    changeLabel(currentBitmap, angle, shift_x, shift_y, changedLabelFile, labels);
                    Bitmap changed = changeImage(currentBitmap, angle, shift_x, shift_y, changedLabelFile);

                    string fileName = generateFileName(file, i);
                    changedLabelFile.Descendants("filename").First().SetValue(Path.GetFileNameWithoutExtension(fileName) + ".jpg");
                    changedLabelFile.Save(Path.Combine(targetXmlDirectory, Path.GetFileNameWithoutExtension(fileName) + ".xml"));
                    changed.Save(Path.Combine(targetImageDirectory, Path.GetFileNameWithoutExtension(fileName) + ".jpg"), ImageFormat.Jpeg);
                    trainvalFile.WriteLine(Path.GetFileNameWithoutExtension(fileName));
                    Console.Write("#");
                    changed.Dispose();
                }

                Console.WriteLine();
                currentBitmap.Dispose();
            }

            int id = 1;
            foreach (String label in labels.Distinct())
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
            }
            else if (i < 100)
            {
                numberSuffix = "0" + i;
            }

            return (pureFileName + "_" + numberSuffix + file.Substring(file.LastIndexOf('.')));
        }
    }
}

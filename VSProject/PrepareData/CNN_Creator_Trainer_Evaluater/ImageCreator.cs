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
    class ImageCreator
    {

        public static void createMoreJson(string sourceImagePath, string sourceLabelFileDirectory, string targetImageDirectory, string targetAnnotationDirectory,
                                          double maxRot = 360, int maxShiftHor = -1, int maxShiftVer = -1, int sampleCount = 30)
        {
            JSON_Reader labelFile = new JSON_Reader(sourceLabelFileDirectory);
            JSON_Reader createdFiles = new JSON_Reader(Path.Combine(targetAnnotationDirectory, "created.json"));

            string[] filePaths = Directory.GetFiles(sourceImagePath);

            int currentImageCount = 0;
            Image currentImage = Image.FromFile(filePaths[currentImageCount]);

            foreach (JsonFile file in labelFile.getFiles())
            {
                Console.Write(file.getFilename() + ": ");
                Random rnd = new Random();
                Bitmap currentBitmap = (Bitmap)Bitmap.FromFile(Path.Combine(sourceImagePath, file.getFilename()));

                createdFiles.addFile(file);
                currentBitmap.Save(Path.Combine(targetImageDirectory, file.getFilename()), ImageFormat.Jpeg);

                for (int i = 0; i < sampleCount; i++)
                {
                    float angle = (float)(rnd.Next(-50, 50) * maxRot / 100.0);
                    int shift_x = (int)(rnd.Next(-50, 50) * maxShiftVer / 100);
                    int shift_y = (int)(rnd.Next(-50, 50) * maxShiftHor / 100);

                    Bitmap changed = changeImage(currentBitmap, angle, shift_x, shift_y);

                    string fileName = generateFileName(file.getFilename(), i);
                    String newFilePath = Path.Combine(targetImageDirectory, fileName);
                    changed.Save(newFilePath, ImageFormat.Jpeg);

                    JsonFile changedFile = file.getChanged(newFilePath, angle, shift_x, shift_y);
                    createdFiles.addFile(changedFile);
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

        private static XMLAnnotation createBoxLabelFileFromJson(JsonFile jsonFile, Bitmap bitmap)
        {
            XMLAnnotation annot = new XMLAnnotation(jsonFile.getFilename(), bitmap.Width, bitmap.Height, 1);
            List<Region> regions = jsonFile.getRegions();

            foreach (Region r in regions)
            {
                int xMin = bitmap.Width, yMin = bitmap.Height, xMax = 0, yMax = 0;

                foreach (Point p in r.getPoints())
                {
                    if (p.X < xMin)
                        xMin = p.X;

                    if (p.X > xMax)
                        xMax = p.X;

                    if (p.Y < yMin)
                        yMin = p.Y;

                    if (p.Y > yMax)
                        yMax = p.Y;
                }

                annot.addBox(r.getKlasse(), xMin, yMin, xMax, yMax);
            }


            return annot;
        }

        public static void createBoxesFromJson(string sourceImagePath, string sourceLabelFileDirectory, string targetImageDirectory, string targetAnnotationDirectory,
                                       double maxRot = 360, int maxShiftHor = -1, int maxShiftVer = -1, int sampleCount = 30)
        {
            JSON_Reader labelFile = new JSON_Reader(Directory.GetFiles(sourceLabelFileDirectory)[0]);

            string[] filePaths = Directory.GetFiles(sourceImagePath);

            int currentImageCount = 0;
            Image currentImage = Image.FromFile(filePaths[currentImageCount]);
            String targetXMLDirectory = Path.Combine(targetAnnotationDirectory, "xmls");

            List<String> labels = new List<string>();
            StreamWriter trainvalFile = new StreamWriter(Path.Combine(targetAnnotationDirectory, "trainval.txt"), false);
            StreamWriter label_mapFile = new StreamWriter(Path.Combine(targetAnnotationDirectory, "label_map.pbtxt"), false);

            foreach (JsonFile file in labelFile.getFiles())
            {
                foreach (Region r in file.getRegions())
                {
                    labels.Add(r.getKlasse());
                }

                Console.Write(file.getFilename() + ": ");
                Random rnd = new Random();
                Bitmap currentBitmap = (Bitmap)Bitmap.FromFile(Path.Combine(sourceImagePath, file.getFilename()));

                //currentBitmap.Save(Path.Combine(targetImageDirectory, file.getFilename()), ImageFormat.Jpeg);

                for (int i = 0; i < sampleCount; i++)
                {
                    float angle = (float)(rnd.Next(-50, 50) * maxRot / 100.0);
                    int shift_x = (int)(rnd.Next(-50, 50) * maxShiftVer / 100);
                    int shift_y = (int)(rnd.Next(-50, 50) * maxShiftHor / 100);

                    Bitmap changed = changeImage(currentBitmap, angle, shift_x, shift_y);


                    string fileName = generateFileName(file.getFilename(), i);
                    String newFilePath = Path.Combine(targetImageDirectory, fileName);
                    changed.Save(newFilePath, ImageFormat.Jpeg);

                    JsonFile changedFile = file.getChanged(newFilePath, angle, shift_x, shift_y);

                    XMLAnnotation changedLabelFile = createBoxLabelFileFromJson(changedFile, changed);
                    changedLabelFile.save(targetXMLDirectory);
                    trainvalFile.WriteLine(changedFile.getFilename());
                    changed.Dispose();
                    Console.Write("#");
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
    }
}

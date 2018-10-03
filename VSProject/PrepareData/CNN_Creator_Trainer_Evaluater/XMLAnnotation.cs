using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNN_Creator_Trainer_Evaluater
{
    class XMLAnnotation
    {
        String filename;
        int width;
        int height;
        int depth;
        List<BndBox> boxes;

        public XMLAnnotation(String filename, int width, int height, int depth)
        {
            this.filename = filename;
            this.width = width;
            this.height = height;

            boxes = new List<BndBox>();
        }

        public void addBox(String name, int xMin, int yMin, int xMax, int yMax)
        {
            this.boxes.Add(new BndBox(name, xMin, yMin, xMax, yMax));
        }

        override
        public String ToString()
        {
            String text = "<annotation>\n";
            text += "\t<filename>" + this.filename + "</filename>\n";
            text += "\t<size>\n";
            text += "\t\t<width>" + this.width + "</width>\n";
            text += "\t\t<height>" + this.height + "</height>\n";
            text += "\t\t<depth>" + this.depth + "</depth>\n";
            text += "\t</size>\n";
            text += "\t<segmented>0</segmented>\n";

            foreach (BndBox box in boxes)
            {
                text += box.ToString();
            }

            text += "</annotation>";

            return text;
        }

        public void save(String targetDirectory)
        {
            File.WriteAllText(Path.Combine(targetDirectory, Path.GetFileNameWithoutExtension(this.filename) + ".xml"), this.ToString(), Encoding.ASCII);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNN_Creator_Trainer_Evaluater
{
    class BndBox
    {
        String name;
        int xMin, yMin, xMax, yMax;

        public BndBox(String name, int xMin, int yMin, int xMax, int yMax)
        {
            this.name = name;
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }

        override
        public String ToString()
        {
            String text = "\t<object>\n";
            text += "\t\t<name>" + this.name + "</name>\n";
            text += "\t\t<bndbox>\n";
            text += "\t\t\t<xmin>" + this.xMin + "</xmin>\n";
            text += "\t\t\t<ymin>" + this.yMin + "</ymin>\n";
            text += "\t\t\t<xmax>" + this.xMax + "</xmax>\n";
            text += "\t\t\t<ymax>" + this.yMax + "</ymax>\n";
            text += "\t\t</bndbox>\n";
            text += "\t</object>\n";

            return text;
        }
    }
}

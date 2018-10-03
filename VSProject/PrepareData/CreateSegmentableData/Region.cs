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
    class Region
    {
        String name;
        List<Point> points = new List<Point>();
        String klasse;

        public Region(String name, String allPointsX, String allPointsY, String klasse)
        {
            this.name = name;
            this.klasse = klasse;

            List<string> xListString = allPointsX.Split(',').ToList<string>();
            //xListString.Reverse();
            List<string> yListString = allPointsY.Split(',').ToList<string>();
            //yListString.Reverse();

            for (int i = 0; i < xListString.Count; i++)
            {
                points.Add(new Point(Int32.Parse(xListString[i]), Int32.Parse(yListString[i])));
            }
        }

        public Region()
        {

        }

        public void addPoint(Point p)
        {
            this.points.Add(p);
        }

        override
        public String ToString()
        {
            String rep = "\"" + name + "\":{\"shape_attributes\":{\"name\":\"polygon\",\"all_points_x\":[";

            foreach (Point p in points)
            {
                rep += p.X + ",";
            }
            rep = rep.Substring(0, rep.Length - 1);
            rep += "],\"all_points_y\":[";

            foreach (Point p in points)
            {
                rep += p.Y + ",";
            }
            rep = rep.Substring(0, rep.Length - 1);
            rep += "]},\"region_attributes\":{\"Class\":\"" + klasse + "\"}}";
            return rep;
        }

        public Region getChanged(float angle, int shift_x, int shift_y, Point center, int width, int height)
        {
            Region changed = new Region();

            changed.name = this.name;
            changed.klasse = this.klasse;

            foreach (Point p in this.points)
            {
                Point rotated = new Point(p.X, p.Y);
                rotated.X += shift_x;
                rotated.Y += shift_y;

                rotated = RotatePoint(rotated, center, angle * (Math.PI / 180.0));

                if (rotated.X < 0)
                    rotated.X = 0;

                if (rotated.Y < 0)
                    rotated.Y = 0;

                if (rotated.X > width)
                    rotated.X = width - 1;

                if (rotated.Y > height)
                    rotated.Y = height - 1;

                changed.addPoint(rotated);
            }

            return changed;
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

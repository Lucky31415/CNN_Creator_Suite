using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNN_Creator_Trainer_Evaluater
{
    class JsonFile
    {
        String name;
        long size;
        String fileName;
        List<Region> regions = new List<Region>();

        public JsonFile(String text)
        {
            name = text.Substring(1, text.IndexOf(':') - 2);
            fileName = getValueOf(text, "filename");
            size = Int32.Parse(getValueOf(text, "size"));

            String regionString = text.Substring(text.IndexOf("\"regions\":"));
            regionString = regionString.Substring(regionString.IndexOf(':') + 1);
            List<String> regionStrings = new List<String>();

            int bracesCount = 0;
            int lastStop = 1;
            for (int i = 1; i < regionString.Length; i++)
            {
                switch (regionString[i])
                {
                    case ',':
                        if (bracesCount == 0)
                        {
                            regionStrings.Add(regionString.Substring(lastStop, i - lastStop));
                            lastStop = i + 1;
                        }
                        break;
                    case '{':
                        bracesCount++;
                        break;
                    case '}':
                        bracesCount--;
                        break;
                }
            }
            regionStrings.Add(regionString.Substring(lastStop, regionString.Length - lastStop - 2));

            extractRegions(regionStrings);
        }

        private void extractRegions(List<String> regionStrings)
        {
            foreach (String reg in regionStrings)
            {
                String name = reg[1].ToString();
                String klasse = getValueOf(reg, "Class");

                String subPropCutFirst = reg.Substring(reg.IndexOf("all_points_x"));
                String subProp = subPropCutFirst.Substring(0, subPropCutFirst.IndexOf(']'));
                subProp = subProp.Replace("\\", String.Empty).Replace("\"", String.Empty);
                String all_points_x = subProp.Substring(subProp.IndexOf(':') + 1).Replace("[", String.Empty).Replace("]", String.Empty);

                subPropCutFirst = reg.Substring(reg.IndexOf("all_points_y"));
                subProp = subPropCutFirst.Substring(0, subPropCutFirst.IndexOf(']'));
                subProp = subProp.Replace("\\", String.Empty).Replace("\"", String.Empty);
                String all_points_y = subProp.Substring(subProp.IndexOf(':') + 1).Replace("[", String.Empty).Replace("]", String.Empty);

                this.regions.Add(new Region(name, all_points_x, all_points_y, klasse));
            }

        }

        public JsonFile()
        {

        }

        public String getFilename()
        {
            return fileName;
        }

        public String toString()
        {
            String rep = "\"" + fileName + "" + size +"\":{\"fileref\":\"\",\"size\":" + size + ",\"filename\":\"" + fileName + "\",\"base64_img_data\":\"\",\"file_attributes\":{},\"regions\":{";

            foreach (Region r in regions)
            {
                rep += r.ToString() + ",";
            }
            rep = rep.Substring(0, rep.Length - 1);
            rep += "}}";

            return rep;
        }

        public void addRegion(Region r)
        {
            regions.Add(r);
        }

        public JsonFile getChanged(String filePath, float angle, int shift_x, int shift_y)
        {
            JsonFile changed = new JsonFile();
            changed.size = new FileInfo(filePath).Length;
            changed.fileName = Path.GetFileName(filePath);
            changed.name = changed.fileName + changed.size;
            Bitmap img = new Bitmap(filePath);
            Point center = new Point(img.Width / 2, img.Height / 2);

            foreach (Region r in this.regions)
            {
                changed.addRegion(r.getChanged(angle, shift_x, shift_y, center, img.Width, img.Height));
            }

            return changed;
        }

        private String getValueOf(String allText, String property)
        {
            int propertyIndex = allText.IndexOf(property);
            String subPropCutFirst = allText.Substring(propertyIndex);
            String subProp = subPropCutFirst.Substring(0, subPropCutFirst.IndexOfAny(new char[] {'}', ',' }));
            subProp = subProp.Replace("\\", String.Empty);
            subProp = subProp.Replace("\"", String.Empty);
            String value = subProp.Substring(subProp.IndexOf(':') + 1);
            return value;
        }

        public List<Region> getRegions()
        {
            return this.regions;
        }
    }
}

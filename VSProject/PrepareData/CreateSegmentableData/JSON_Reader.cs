using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSegmentableData
{
    class JSON_Reader
    {
        String path;
        List<JsonFile> files = new List<JsonFile>();
        public JSON_Reader(String path)
        {
            this.path = path;

            if (File.Exists(path))
            {
                List<String> fileStrings = this.seperateInFileStrings();

                foreach (String fileString in fileStrings)
                {
                    files.Add(new JsonFile(fileString));
                }
            } else
            {
                File.Create(path).Close();
            }
        }

        public int getFileCount()
        {
            return this.seperateInFileStrings().Count;
        }

        public List<JsonFile> getFiles()
        {
            return files;
        }

        private List<String> seperateInFileStrings()
        {
            String allText = File.ReadAllText(path);

            List<String> fileStrings = new List<String>();

            int bracesCount = 0;
            int lastStop = 1;
            for (int i = 1; i < allText.Length; i++)
            {
                switch (allText[i])
                {
                    case ',':
                        if (bracesCount == 0)
                        {
                            fileStrings.Add(allText.Substring(lastStop, i - lastStop));
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
            fileStrings.Add(allText.Substring(lastStop, allText.Length - lastStop - 1));

            return fileStrings;
        }

        public void addFile(JsonFile file)
        {
            this.files.Add(file);
        }

        public void writeToFile()
        {
            String allText = "{";

            foreach (JsonFile f in files)
            {
                allText += f.toString();
                allText += ",";
            }
            allText = allText.Substring(0, allText.Length - 1);

            allText += "}";

            File.WriteAllText(path, allText);
            //StreamWriter fileWriter = new StreamWriter(path);
            //fileWriter.WriteLine(allText);
            //fileWriter.Dispose();
        }

        


    }
}

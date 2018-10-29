using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace CNN_Creator_Trainer_Evaluater
{
    static class LocalConfig
    {
        static XDocument configFile;

        static string propertyRecentProjects = "RecentProjects";
        static string propertyImageGenerator = "ImageGeneratorSettings";

        public static void init()
        {
            if (!File.Exists(Path.Combine(Application.StartupPath, "localConfig.xml")))
            {
                XElement generatorSettings = new XElement(propertyImageGenerator,
                    new XElement("imagePath"),
                    new XElement("xmlPath"),
                    new XElement("maxRot"),
                    new XElement("maxShiftX"),
                    new XElement("maxShiftY"),
                    new XElement("genCount"));
                configFile = new XDocument(new XElement("config" , new XElement(propertyRecentProjects), generatorSettings));
                configFile.Save("localConfig.xml");
                //File.Create(Path.Combine(Application.StartupPath, "localConfig.xml"));
            }
            configFile = XDocument.Load(Path.Combine(Application.StartupPath, "localConfig.xml"));
        }

        public static List<String> getRecentProjects()
        {
            List<XElement> recentProjectsX = configFile.Descendants("RecentProject").ToList();

            List<String> recentProjects = new List<String>();
            foreach (XElement el in recentProjectsX)
            {
                recentProjects.Add(el.Value);
            }

            return recentProjects;
        }

        public static void addRecentProject(String projectPath)
        {
            List<String> rP = getRecentProjects();

            if (!rP.Contains(projectPath))
            {
                configFile.Descendants().First().Add(new XElement("RecentProject", projectPath));
                configFile.Save("localConfig.xml");
                //configFile.Add(new XElement(propertyRecentProjects, projectPath));
            }
        }

        public static void setGenerateSettings(String[] settings)
        {
            XElement imageGeneratorSettings = configFile.Descendants(propertyImageGenerator).ToList()[0];

            imageGeneratorSettings.Descendants("imagePath").ToList()[0].Value = settings[0];
            imageGeneratorSettings.Descendants("xmlPath").ToList()[0].Value = settings[1];
            imageGeneratorSettings.Descendants("maxRot").ToList()[0].Value = settings[2];
            imageGeneratorSettings.Descendants("maxShiftX").ToList()[0].Value = settings[3];
            imageGeneratorSettings.Descendants("maxShiftY").ToList()[0].Value = settings[4];
            imageGeneratorSettings.Descendants("genCount").ToList()[0].Value = settings[5];

            configFile.Save("localConfig.xml");
        }

        public static String[] getGenerateSettings()
        {
            String[] settings = new String[6];

            XElement imageGeneratorSettings = configFile.Descendants(propertyImageGenerator).ToList()[0];
            settings[0] = imageGeneratorSettings.Descendants("imagePath").ToList()[0].Value;
            settings[1] = imageGeneratorSettings.Descendants("xmlPath").ToList()[0].Value;
            settings[2] = imageGeneratorSettings.Descendants("maxRot").ToList()[0].Value;
            settings[3] = imageGeneratorSettings.Descendants("maxShiftX").ToList()[0].Value;
            settings[4] = imageGeneratorSettings.Descendants("maxShiftY").ToList()[0].Value;
            settings[5] = imageGeneratorSettings.Descendants("genCount").ToList()[0].Value;

            return settings;
        }

    }
}

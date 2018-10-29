using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CNN_Creator_Trainer_Evaluater
{
    class ConfigManager
    {
        XDocument configFile;

        string propertyRecentProjects = "RecentProjects";
        string propertyImageGenerator = "ImageGeneratorSettings";

        public ConfigManager(String projectPath)
        {
            if (!File.Exists(Path.Combine(projectPath, "localConfig.xml")))
            {
                File.Create(Path.Combine(projectPath, "localConfig.xml"));
            }
            this.configFile = XDocument.Load(Path.Combine(projectPath, "localConfig.xml"));
        }

        public List<String> getRecentProjects()
        {
            List<XElement> recentProjectsX = configFile.Descendants(propertyRecentProjects).ToList();

            List<String> recentProjects = new List<String>();
            foreach (XElement el in recentProjectsX)
            {
                recentProjects.Add(el.Value);
            }

            return recentProjects;
        }

        public void addRecentProject(String projectPath)
        {
            List<String> rP = this.getRecentProjects();

            if (!rP.Contains(projectPath))
                configFile.Add(new XElement(propertyRecentProjects, projectPath));
        }

        public String[] getGenerateSettings()
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

        public void setGenerateSettings(String[] settings)
        {
            XElement imageGeneratorSettings = configFile.Descendants(propertyImageGenerator).ToList()[0];

            imageGeneratorSettings.Descendants("imagePath").ToList()[0].Value = settings[0];
            imageGeneratorSettings.Descendants("xmlPath").ToList()[0].Value = settings[1];
            imageGeneratorSettings.Descendants("maxRot").ToList()[0].Value = settings[2];
            imageGeneratorSettings.Descendants("maxShiftX").ToList()[0].Value = settings[3];
            imageGeneratorSettings.Descendants("maxShiftY").ToList()[0].Value = settings[4];
            imageGeneratorSettings.Descendants("genCount").ToList()[0].Value = settings[5];
        }
    }
}

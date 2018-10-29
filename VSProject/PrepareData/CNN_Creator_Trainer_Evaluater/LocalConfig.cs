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
                configFile = new XDocument(new XElement(propertyRecentProjects));
                configFile.Save("localConfig.xml");
                //File.Create(Path.Combine(Application.StartupPath, "localConfig.xml"));
            }
            configFile = XDocument.Load(Path.Combine(Application.StartupPath, "localConfig.xml"));
        }

        public static List<String> getRecentProjects()
        {
            List<XElement> recentProjectsX = configFile.Descendants(propertyRecentProjects).ToList();

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
                configFile.Descendants().First().Add(new XElement(propertyRecentProjects, projectPath));
                configFile.Save("localConfig.xml");
                //configFile.Add(new XElement(propertyRecentProjects, projectPath));
            }
        }
    }
}

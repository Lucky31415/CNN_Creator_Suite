using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNN_Creator_Trainer_Evaluater
{
    public partial class ObjectDetectionMenuForm : Form
    {
        private string projectDirectory;
        private string imageDirectory;
        private string annotationDirectory;

        public ObjectDetectionMenuForm()
        {
            InitializeComponent();
        }

        public ObjectDetectionMenuForm(string projectDirectory)
        {
            InitializeComponent();

            this.projectDirectory = projectDirectory;
            this.Text = projectDirectory;

            this.imageDirectory = Path.Combine(projectDirectory, "research/images");
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            this.annotationDirectory = Path.Combine(projectDirectory, "research/annotations");
            if (!Directory.Exists(annotationDirectory))
            {
                Directory.CreateDirectory(annotationDirectory);
                Directory.CreateDirectory(Path.Combine(annotationDirectory, "xmls"));
            }

            reloadImageList();

            reloadXmlList();
        }

        private void ObjectDetectionMenuForm_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            GenerateODImagePrepareForm goipf1 = new GenerateODImagePrepareForm(this);
            goipf1.Show();
        }

        public void reloadImageList()
        {
            string[] files = Directory.GetFiles(imageDirectory);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                ListViewItem item = new ListViewItem(fileName);
                item.Tag = file;
                listView1.Items.Add(item);
            }
        }

        public void reloadXmlList()
        {
            string[] files = Directory.GetFiles(Path.Combine(annotationDirectory, "xmls"));
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                ListViewItem item = new ListViewItem(fileName);
                item.Tag = file;
                listView2.Items.Add(item);
            }
        }

        public String getImageDirectory()
        {
            return this.imageDirectory;
        }

        public String getAnnotationDirectory()
        {
            return this.annotationDirectory;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            deleteImages();
        }

        public void deleteImages()
        {
            string[] files = Directory.GetFiles(imageDirectory);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            reloadImageList();
        }

        public void deleteXmls()
        {
            string[] files = Directory.GetFiles(annotationDirectory);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            files = Directory.GetFiles(Path.Combine(annotationDirectory, "xmls"));
            foreach (string file in files)
            {
                File.Delete(file);
            }

            reloadXmlList();
        }

        public void resetData()
        {
            deleteImages();
            deleteXmls();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PowerShell ps = PowerShell.Create();
            compileProtos();


            //ps.AddScript("SET PYTHONPATH=$PYTHONPATH:`pwd`:`pwd`/slim");
            ps.AddScript("cd " + Path.Combine(projectDirectory, "research"));
            ps.AddScript("python create_tf_record.py --data_dir =`pwd` --output_dir =`pwd`");

            Collection<PSObject> PSOutput = ps.Invoke();

            foreach (PSObject outputItem in PSOutput)
            {
                if (outputItem != null)
                {
                    richTextBox1.Text += outputItem.ToString();
                }
            }
        }

        private void compileProtos()
        {
            PowerShell ps = PowerShell.Create();
            String protoDir = Path.Combine(Path.Combine(Path.Combine(projectDirectory, "research"), "object_detection"), "protos");

            String[] files = Directory.GetFiles(protoDir);
            foreach (String file in files)
            {
                if (file.Substring(file.LastIndexOf(".")).Equals(".proto"))
                {
                    if (!files.Contains(file.Substring(0, file.LastIndexOf(".")) + "_pb2.py"))
                    {
                        ps.AddScript("cd " + Path.Combine(projectDirectory, "research"));
                        ps.AddScript("protoc .\\object_detection\\protos\\" + Path.GetFileName(file) + " --python_out=.");
                        ps.Invoke();
                    }
                }
            }
        }

        public String getProjectDirectory()
        {
            return this.projectDirectory;
        }
    }
}

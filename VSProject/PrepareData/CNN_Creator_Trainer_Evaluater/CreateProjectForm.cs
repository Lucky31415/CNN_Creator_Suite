using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management.Automation;

namespace CNN_Creator_Trainer_Evaluater
{
    public partial class CreateProjectForm : Form
    {
        Form caller;
        public CreateProjectForm(Form caller)
        {
            InitializeComponent();
            //textBox1.Text = LocalConfig.getRecentProjects()[0];
            List<string> recentProjectList = LocalConfig.getRecentProjects();
            comboBox1.Items.AddRange(recentProjectList.ToArray());
            comboBox1.SelectedIndex = 0;
            this.caller = caller;
        }

        private void CreateProjectForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            String rootPath = comboBox1.Text;
            String projectName = textBox2.Text;

            if (rootPath.Equals("") || projectName.Equals(""))
            {
                System.Windows.Forms.MessageBox.Show("Please fill all fields");
            } else
            {
                String projectPath = Path.Combine(rootPath, projectName);

                if (Directory.Exists(projectPath))
                {
                    System.Windows.Forms.MessageBox.Show("This project already exists!");
                } else
                {
                    LocalConfig.addRecentProject(projectPath);
                    Directory.CreateDirectory(projectPath);

                    getObjectDetectionAndSlim(projectPath);

                    ObjectDetectionMenuForm odmf1 = new ObjectDetectionMenuForm(projectPath);
                    odmf1.Show();
                    caller.Hide();
                    this.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                String projectDirectory = fbd.SelectedPath;
                comboBox1.Text = projectDirectory;
            }
        }

        private void getObjectDetectionAndSlim(String path)
        {
            PowerShell ps = PowerShell.Create();

            label3.Text = "Downloading Assets";

            Console.WriteLine(path);

            ps.AddScript("cd " + path);
            ps.AddScript("git init");
            ps.AddScript("git remote add -f origin https://github.com/tensorflow/models.git");
            ps.AddScript("git config core.sparseCheckout true");
            ps.AddScript("echo \"research/object_detection `nresearch/slim\" | out-file -encoding ascii .git/info/sparse-checkout");
            ps.AddScript("git pull origin master");
            ps.Invoke();

            while(!Directory.Exists(Path.Combine(path, "research")))
            {
                
            }

            File.WriteAllBytes(Path.Combine(Path.Combine(path, "research"), "create_tf_record.py"), Properties.Resources.create_tf_record);
            File.WriteAllBytes(Path.Combine(Path.Combine(path, "research"), "utils.py"), Properties.Resources.utils);
            File.WriteAllBytes(Path.Combine(Path.Combine(path, "research"), "requirements.txt"), Encoding.ASCII.GetBytes(Properties.Resources.requirements));
        }
    }
}

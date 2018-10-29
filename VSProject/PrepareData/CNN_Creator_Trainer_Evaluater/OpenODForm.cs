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

namespace CNN_Creator_Trainer_Evaluater
{
    public partial class OpenODForm : Form
    {
        Form caller;
        public OpenODForm(Form caller)
        {
            InitializeComponent();
            List<string> recentProjectList = LocalConfig.getRecentProjects();
            comboBox1.Items.AddRange(recentProjectList.ToArray());
            comboBox1.SelectedIndex = 0;
            this.caller = caller;
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

        private void button2_Click(object sender, EventArgs e)
        {
            String projectPath = comboBox1.Text;

            if (projectPath.Equals(""))
            {
                System.Windows.Forms.MessageBox.Show("Please select a directory");
            }
            else
            {
                if (Path.GetFileName(Directory.GetDirectories(projectPath)[1]).Equals("research"))
                {
                    LocalConfig.addRecentProject(projectPath);
                    ObjectDetectionMenuForm odmf1 = new ObjectDetectionMenuForm(projectPath);
                    odmf1.Show();
                    caller.Hide();
                    this.Close();
                } else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Project Directory!");
                }
            }
        }
    }
}

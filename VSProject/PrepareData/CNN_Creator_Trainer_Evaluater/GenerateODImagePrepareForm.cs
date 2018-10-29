using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CNN_Creator_Trainer_Evaluater
{
    public partial class GenerateODImagePrepareForm : Form
    {
        ObjectDetectionMenuForm caller;
        public GenerateODImagePrepareForm(ObjectDetectionMenuForm caller)
        {
            InitializeComponent();
            this.caller = caller;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                String projectDirectory = fbd.SelectedPath;
                textBox1.Text = projectDirectory;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                String projectDirectory = fbd.SelectedPath;
                textBox2.Text = projectDirectory;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            caller.resetData();

            String sourceImageDirectory = textBox1.Text;
            String sourceXmlDirectory = textBox2.Text;

            double maxRotation = textBox3.Text.Equals("") ? 360 : Double.Parse(textBox3.Text);
            int maxShift_x = textBox4.Text.Equals("") ? -1 : Int32.Parse(textBox4.Text);
            int maxShift_y = textBox5.Text.Equals("") ? -1 : Int32.Parse(textBox5.Text);
            int sampleCount = textBox6.Text.Equals("") ? 30 : Int32.Parse(textBox6.Text);

            ImageCreator.createBoxesFromJson(sourceImageDirectory, sourceXmlDirectory, caller.getImageDirectory(), caller.getAnnotationDirectory(), maxRotation, maxShift_x, maxShift_y, sampleCount);
            //ODImageCreator ic = new ODImageCreator(sourceImageDirectory, sourceXmlDirectory, caller.getImageDirectory(), caller.getAnnotationDirectory(), maxRotation, maxShift_x, maxShift_y, sampleCount);
            LocalConfig.setGenerateSettings(new String[] { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text });
            caller.reloadImageList();
            caller.reloadXmlList();
            this.Close();
        }

        private void GenerateODImagePrepareForm_Load(object sender, EventArgs e)
        {
            String[] settings = LocalConfig.getGenerateSettings();

            textBox1.Text = settings[0];
            textBox2.Text = settings[1];
            textBox3.Text = settings[2];
            textBox4.Text = settings[3];
            textBox5.Text = settings[4];
            textBox6.Text = settings[5];
        }
    }
}

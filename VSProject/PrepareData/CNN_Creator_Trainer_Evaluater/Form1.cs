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
    public partial class Form1 : Form
    {
        String projectDirectory = "";
        public Form1()
        {
            InitializeComponent();
            LocalConfig.init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateProjectForm cpf1 = new CreateProjectForm(this);
            cpf1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenODForm oodf1 = new OpenODForm(this);
            oodf1.Show();
        }
    }
}

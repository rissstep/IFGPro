using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IFGPro
{
    public partial class OpenDialog : Form
    {
        
        //opening new project
        public string name;
        public bool OK = false;
        private string path;

        public OpenDialog(string path,string s)
        {
            InitializeComponent();
            textBox1.Text = s;
            this.path = path;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(path + "//" + textBox1.Text+".ifg"))
            {
                DialogResult dialogResult = MessageBox.Show("Project file already exists. Do you want to overwrite it?", "Warning", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    OK = true;
                    name = textBox1.Text;
                    this.Close();
                }
                else
                    return;
            }
            else
            {
                OK = true;
                name = textBox1.Text;
                this.Close();
            }
        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (File.Exists(path + "//" + textBox1.Text + ".ifg"))
                {
                    DialogResult dialogResult = MessageBox.Show("Project file already exists. Do you want to overwrite it?", "Warning", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        OK = true;
                        name = textBox1.Text;
                        this.Close();
                    }
                    else
                        return;
                }
                else
                {
                    OK = true;
                    name = textBox1.Text;
                    this.Close();
                }
            }
        }

    }
}

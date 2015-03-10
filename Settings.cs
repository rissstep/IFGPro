using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IFGPro
{
    public partial class Settings : Form
    {
        public bool apply = false;
        private Font _index = GlobalSettings.fontLines;
        private Font _description = GlobalSettings.fontDescription;
        private Font _points = GlobalSettings.fontPoints;
        private Font _fringeLabelsFont = GlobalSettings.fringeLabelsFont;
        Cyotek.Windows.Forms.ImageBox imageBox;
        bool afterInit = false;

        public Settings(ref Cyotek.Windows.Forms.ImageBox ib)
        {

            InitializeComponent();
            initComboBox();

            imageBox = ib;

            btn_cross.BackColor = GlobalSettings.crossPen.Color;
            btn_lines.BackColor = GlobalSettings.linesPen.Color;
            btn_profil.BackColor = GlobalSettings.profilPen.Color;
            btn_selected.BackColor = GlobalSettings.selectedPen.Color;

            btn_colorIndex.BackColor = GlobalSettings.indexBrush.Color;
            btn_colorDesc.BackColor = GlobalSettings.descriptionBrush.Color;

            tb_length.Text = GlobalSettings.lineLength.ToString();


            cb_description.Checked = GlobalSettings.desc;
            cb_physical_points.Checked = GlobalSettings.points;
            cb_physical_points_desc.Checked = GlobalSettings.pointsDesc;
            cb__lines.Checked = GlobalSettings.lines;
            cb_lines_index.Checked = GlobalSettings.linesDesc;
            cb_lineNumber.Checked = GlobalSettings.lineFringeNumber;
            cb__profil.Checked = GlobalSettings.profil;

            textBox1.Text = GlobalSettings.roundTime.ToString();
            textBox2.Text = GlobalSettings.roundPitchPlunge.ToString();
            textBox3.Text = GlobalSettings.roundOthers.ToString();

            cb_fringeLabelsPanel.Checked = GlobalSettings.fringeLabelsPanel;
            cb_fringeLabels.Checked = GlobalSettings.fringeLabels;
            tb_fringeLabelsCircleSize.Text = GlobalSettings.fringeCircleSize.ToString();
            tb_from.Text = GlobalSettings.fringeLabelsFrom.ToString();
            tb_to.Text = GlobalSettings.fringeLabelsTo.ToString();
            btn_fringeLabelsColor.BackColor = GlobalSettings.fringeLabelsColor;


            afterInit = true;

        }
        private void btn_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            var v = (Button)sender;
            v.BackColor = colorDialog1.Color;
            string name = v.Name;

            refresh();
        }
        private void rounding_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                    && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
                textBox1.Text = "1";
        }
        private void initComboBox()
        {
            List<float> lc1 = new List<float>();
            lc1.Add(0.5f);
            lc1.Add(1f);
            lc1.Add(1.5f);
            lc1.Add(2f);
            lc1.Add(2.5f);
            lc1.Add(3f);
            lc1.Add(3.5f);
            lc1.Add(4f);
            lc1.Add(4.5f);
            lc1.Add(5f);

            List<float> lc2 = new List<float>();
            lc2.Add(0.5f);
            lc2.Add(1f);
            lc2.Add(1.5f);
            lc2.Add(2f);
            lc2.Add(2.5f);
            lc2.Add(3f);
            lc2.Add(3.5f);
            lc2.Add(4f);
            lc2.Add(4.5f);
            lc2.Add(5f);

            List<float> lc3 = new List<float>();
            lc3.Add(0.5f);
            lc3.Add(1f);
            lc3.Add(1.5f);
            lc3.Add(2f);
            lc3.Add(2.5f);
            lc3.Add(3f);
            lc3.Add(3.5f);
            lc3.Add(4f);
            lc3.Add(4.5f);
            lc3.Add(5f);

            List<float> lc4 = new List<float>();
            lc4.Add(0.25f);
            lc4.Add(0.5f);
            lc4.Add(1f);
            lc4.Add(1.5f);
            lc4.Add(2f);
            lc4.Add(3f);
            lc4.Add(4f);
            lc4.Add(5f);
            lc4.Add(10f);

            cb_cross.DataSource = lc1;
            cb_lines.DataSource = lc2;
            cb_profil.DataSource = lc3;
            cb_fringeLabelsStep.DataSource = lc4;

            cb_cross.SelectedItem = GlobalSettings.crossPen.Width;
            cb_lines.SelectedItem = GlobalSettings.linesPen.Width;
            cb_profil.SelectedItem = GlobalSettings.profilPen.Width;
            cb_fringeLabelsStep.SelectedItem = GlobalSettings.fringeStep;
        }
        private void btn_font_Click(object sender, EventArgs e)
        {
            var v = (Button)sender;
            if (v.Name.Equals("tbn_lines"))
            {
                fontDialog1.Font = GlobalSettings.fontLines;
            }
            else if (v.Name.Equals("btn_description"))
            {
                fontDialog1.Font = GlobalSettings.fontDescription;
            }
            else if (v.Name.Equals("btn_points"))
            {
                fontDialog1.Font = GlobalSettings.fontPoints;
            }
            else if (v.Name.Equals("btn_frinteLablesFont"))
            {
                fontDialog1.Font = GlobalSettings.fringeLabelsFont;
            }
            try
            {
                fontDialog1.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            //var v = (Button)sender;
            if (v.Name.Equals("tbn_lines"))
            {
                _index = fontDialog1.Font;
            }
            else if (v.Name.Equals("btn_description"))
            {
                _description = fontDialog1.Font;
            }
            else if (v.Name.Equals("btn_points"))
            {
                _points = fontDialog1.Font;
            }
            else if (v.Name.Equals("btn_frinteLablesFont"))
            {
                _fringeLabelsFont = fontDialog1.Font;
            }

            refresh();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!refresh())
            {
                return;
            }


            apply = true;

            this.Close();

        }
        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text))
                textBox2.Text = "1";
            refresh();
        }
        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox3.Text))
                textBox3.Text = "1";
            refresh();
        }

        private bool refresh()
        {
            if (!afterInit || !tbCorrect())
                return false;

            GlobalSettings.crossPen.Color = btn_cross.BackColor;
            GlobalSettings.linesPen.Color = btn_lines.BackColor;
            GlobalSettings.profilPen.Color = btn_profil.BackColor;
            GlobalSettings.selectedPen.Color = btn_selected.BackColor;


            GlobalSettings.crossPen.Width = (float)cb_cross.SelectedItem;
            GlobalSettings.linesPen.Width = (float)cb_lines.SelectedItem;
            GlobalSettings.profilPen.Width = (float)cb_profil.SelectedItem;

            GlobalSettings.fontLines = _index;
            GlobalSettings.fontDescription = _description;
            GlobalSettings.fontPoints = _points;

            GlobalSettings.indexBrush.Color = btn_colorIndex.BackColor;
            GlobalSettings.descriptionBrush.Color = btn_colorDesc.BackColor;

            GlobalSettings.desc = cb_description.Checked;
            GlobalSettings.points = cb_physical_points.Checked;
            GlobalSettings.pointsDesc = cb_physical_points_desc.Checked;
            GlobalSettings.lines = cb__lines.Checked;
            GlobalSettings.linesDesc = cb_lines_index.Checked;
            GlobalSettings.lineFringeNumber = cb_lineNumber.Checked;
            GlobalSettings.profil = cb__profil.Checked;

            GlobalSettings.roundTime = int.Parse(textBox1.Text);
            GlobalSettings.roundPitchPlunge = int.Parse(textBox2.Text);
            GlobalSettings.roundOthers = int.Parse(textBox3.Text);

            GlobalSettings.fringeLabels = cb_fringeLabels.Checked;
            GlobalSettings.fringeLabelsPanel = cb_fringeLabelsPanel.Checked;
            GlobalSettings.fringeLabelsFont = _fringeLabelsFont;
            GlobalSettings.fringeLabelsColor = btn_fringeLabelsColor.BackColor;
            GlobalSettings.fringeStep = (float)cb_fringeLabelsStep.SelectedItem;

            imageBox.Invalidate();

            return true;

        }

        private bool tbCorrect()
        {

            if (!int.TryParse(tb_length.Text, out GlobalSettings.lineLength))
            {
                MessageBox.Show("Line length parameter is not filled right. Integers only!");
                return false;
            }
            if (!int.TryParse(tb_fringeLabelsCircleSize.Text, out GlobalSettings.fringeCircleSize))
            {
                MessageBox.Show("Circle size parameter is not filled right. Integers only!");
                return false;
            }
            if (!float.TryParse(tb_from.Text, out GlobalSettings.fringeLabelsFrom))
            {
                MessageBox.Show("From parameter is not filled right. Floats only!");
                return false;
            }
            if (!float.TryParse(tb_to.Text, out GlobalSettings.fringeLabelsTo))
            {
                MessageBox.Show("To length parameter is not filled right. Floats only!");
                return false;
            }
            return true;
        }

        private void cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            refresh();
        }

        private void cb_description_CheckedChanged(object sender, EventArgs e)
        {
            refresh();
        }

        private void tb_leave(object sender, EventArgs e)
        {
            refresh();
        }
    }
}

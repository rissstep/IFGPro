using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace IFGPro
{
    public partial class FixedInImage : Form
    {
        private MainWindow window;
        private ImagePoint point;

        public FixedInImage()
        {
            InitializeComponent();
        }

        public FixedInImage(Form w, ImagePoint p)
        {
            InitializeComponent();
            window = (MainWindow)w;
            point = p;

            if (GlobalSettings.ratio != Double.NaN)
            {
                tb_x.Text = px2mm(point.location.X).ToString();
                tb_y.Text = px2mm(point.location.Y).ToString();
            }
            else
            {
                tb_x.Text = "NaN";
                tb_y.Text = "Nan";
            }


            tb_label.Text = p.label;
            tb_ratio.Text = p.ratio.ToString();

            btn_color.BackColor = p.pen.Color;
        }

        public void setPointLocation(Point p)
        {
            window.DeleteImagePoint(point);
            point.location = p;
            window.SetImagePoint(point);
            update();
        }
        private void update()
        {
            if (GlobalSettings.ratio != Double.NaN)
            {
                tb_x.Text = px2mm(point.location.X).ToString();
                tb_y.Text = px2mm(point.location.Y).ToString();
            }
            else
            {
                tb_x.Text = "NaN";
                tb_y.Text = "Nan";
            }

        }
        private void tb_x_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                    && !char.IsDigit(e.KeyChar)
                    && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
        private void tb_y_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                    && !char.IsDigit(e.KeyChar)
                    && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
        private void tb_label_KeyPress(object sender, KeyPressEventArgs e)
        {   //max 3 characters
            //if((sender as TextBox).Text.Length >= 3 && e.KeyChar != 8)
            //{
            //    e.Handled = true;
            //}
        }
        private void tb_x_TextChanged(object sender, EventArgs e)
        {
            if (GlobalSettings.ratio == double.NaN)
                return;
            try
            {
                window.DeleteImagePoint(point);
                point.location.X = (float)mm2px(Double.Parse(tb_x.Text.Replace(',', '.'), CultureInfo.InvariantCulture));
                window.SetImagePoint(point);
            }
            catch
            {
            }
        }
        private void tb_y_TextChanged(object sender, EventArgs e)
        {
            if (GlobalSettings.ratio == double.NaN)
                return;
            try
            {
                window.DeleteImagePoint(point);
                point.location.Y = (float)mm2px(Double.Parse(tb_y.Text.Replace(',', '.'), CultureInfo.InvariantCulture));
                window.SetImagePoint(point);
            }
            catch
            {
            }
        }
        private void tb_label_TextChanged(object sender, EventArgs e)
        {
            window.DeleteImagePoint(point);
            point.label = tb_label.Text;
            window.SetImagePoint(point);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            window.DeleteImagePoint(point);
            this.Close();
        }
        private void FixedInImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
        private void tb_ratio_TextChanged(object sender, EventArgs e)
        {
            window.DeleteImagePoint(point);
            try
            {
                point.ratio = (int)Double.Parse(tb_ratio.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            catch { }
            window.SetImagePoint(point);
        }
        private void tb_ratio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                    && !char.IsDigit(e.KeyChar)
                    && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }
        private double px2mm(double px)
        {
            return Math.Round((px * (1 / GlobalSettings.ratio)), 3);
        }
        private double mm2px(double mm)
        {
            return Math.Round((mm * GlobalSettings.ratio), 3);
        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            btn_color.BackColor = colorDialog1.Color;

            window.DeleteImagePoint(point);
            point.pen.Color = colorDialog1.Color;
            window.SetImagePoint(point);
        }
        
    }
}

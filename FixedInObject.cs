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
    public partial class FixedInObject : Form
    {
        private MainWindow window;
        private ObjectPoint point;

        public FixedInObject()
        {
            InitializeComponent();
        }
        public FixedInObject(Form w, ObjectPoint p)
        {
            InitializeComponent();
            window = (MainWindow)w;
            point = p;

            if (p.isElastic)
                this.Text = "Set elastic axis";

            if (!double.IsNaN(GlobalSettings.ratio))
            {
                tb_x.Text = px2mm(p.locationWithOutOffset.X).ToString();
                tb_y.Text = px2mm((p.locationWithOutOffset.Y * (-1))).ToString();
            }
            else
            {
                tb_x.Text = "NaN";
                tb_y.Text = "Nan";
            }
            tb_label.Text = p.label;
            tb_ratio.Text = p.ratio.ToString();

            checkBox1.Checked = p.onSurface;
            if (checkBox1.Checked)
            {
                if (p.locationWithOutOffset.Y > 0)
                    cb_upper.Checked = true;
                else
                    cb_upper.Checked = false;

                tb_sur_coor.Text = p.sufraceDist.ToString().Replace(',', '.');
            }

            btn_color.BackColor = p.pen.Color;
        }
        private void update()
        {
            if (GlobalSettings.ratio != Double.NaN)
            {
                tb_x.Text = px2mm(point.locationWithOutOffset.X).ToString();
                tb_y.Text = px2mm((point.locationWithOutOffset.Y * (-1))).ToString();
            }
            else
            {
                tb_x.Text = "NaN";
                tb_y.Text = "Nan";
            }

            tb_sur_coor.TextChanged -= tb_sur_coor_TextChanged;
            tb_sur_coor.Text = point.sufraceDist.ToString();
            tb_sur_coor.TextChanged += tb_sur_coor_TextChanged;
            if (point.onSurface)
            {
                if (point.locationWithOutOffset.Y > 0)
                    cb_upper.Checked = true;
                else
                    cb_upper.Checked = false;
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
        private void tb_x_TextChanged(object sender, EventArgs e)
        {
            if (GlobalSettings.ratio == double.NaN)
                return;
            try
            {
                window.DeleteObjectPoint(point);
                point.locationWithOutOffset.X = (float)mm2px(Double.Parse(tb_x.Text.Replace(',', '.'), CultureInfo.InvariantCulture));
                window.SetObjectPoint(point);
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
                window.DeleteObjectPoint(point);
                point.locationWithOutOffset.Y = (-1) * (float)mm2px(Double.Parse(tb_y.Text.Replace(',', '.'), CultureInfo.InvariantCulture));
                window.SetObjectPoint(point);
            }
            catch
            {
            }
        }
        private void tb_label_KeyPress(object sender, KeyPressEventArgs e)
        {   //max 3 characters
            //if ((sender as TextBox).Text.Length >= 3 && e.KeyChar != 8)
            //{
            //    e.Handled = true;
            //}
        }
        private void tb_label_TextChanged(object sender, EventArgs e)
        {
            window.DeleteObjectPoint(point);
            point.label = tb_label.Text;
            window.SetObjectPoint(point);
        }
        private void tb_sur_coor_KeyPress(object sender, KeyPressEventArgs e)
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
            try
            {
                if (double.Parse((tb_sur_coor.Text + e.KeyChar).Replace(',', '.'), CultureInfo.InvariantCulture) > 1)
                {
                    MessageBox.Show(double.Parse(tb_sur_coor.Text, CultureInfo.InvariantCulture).ToString());
                }
            }
            catch { }
        }
        private void tb_sur_coor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                window.DeleteObjectPoint(point);
                point.locationWithOutOffset = pointSurfaceCoor(double.Parse(tb_sur_coor.Text.Replace(',', '.'), CultureInfo.InvariantCulture), cb_upper.Checked);
                point.sufraceDist = double.Parse(tb_sur_coor.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                window.SetObjectPoint(point);
            }
            catch
            {
                //MessageBox.Show("Dickhead");
            }
        }
        private void cb_upper_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                window.DeleteObjectPoint(point);
                point.locationWithOutOffset = pointSurfaceCoor(double.Parse(tb_sur_coor.Text.Replace(',', '.'), CultureInfo.InvariantCulture), cb_upper.Checked);
                point.sufraceDist = double.Parse(tb_sur_coor.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                window.SetObjectPoint(point);
            }
            catch
            {
                //MessageBox.Show("Dickhead");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            window.DeleteObjectPoint(point);
            this.Close();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                tb_sur_coor.Enabled = true;
                cb_upper.Enabled = true;
                tb_x.Enabled = false;
                tb_y.Enabled = false;
                window.DeleteObjectPoint(point);
                point.onSurface = true;
                window.SetObjectPoint(point);
            }
            else
            {
                tb_sur_coor.Enabled = false;
                cb_upper.Enabled = false;
                tb_x.Enabled = true;
                tb_y.Enabled = true;
                window.DeleteObjectPoint(point);
                point.onSurface = false;
                window.SetObjectPoint(point);
            }
        }
        public void setPointLocation(PointF p, PointF leading, PointF falling)
        {
            window.DeleteObjectPoint(point);

            if (point.onSurface)
            {
                if (MainWindow.level != 3)
                {
                    p = MyImage.GetPointOnProfile(MainWindow.arrayProfile, p);
                }
                else if (MainWindow.level == 3)
                {
                    p = MyImage.GetPointOnProfile(window.images.getActual().getArrayProfile(), p);
                }
                try
                {
                    point.sufraceDist = double.Parse(tb_sur_coor.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                }
                catch { }
            }
            point.setPointLocationByPoint(p, leading, falling);
            window.SetObjectPoint(point);
            update();
        }
        private PointF pointSurfaceCoor(double dist, bool down)
        {
            int i = 0;
            double droveDistance = 0;
            double upperDistance = 0;
            double lowerDistance = 0;
            PointF pointTmp = new PointF();
            int flag = 0;

            for (i = 0; i < MainWindow.nacaProfile.Count - 1; i++)
            {
                if (flag == 1 && MainWindow.nacaProfile[i].X == 0 && MainWindow.nacaProfile[i].Y == 0)
                    flag = 2;
                if (flag == 0)
                {
                    upperDistance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                    if (MainWindow.nacaProfile[i + 1].X == 0 ||
                       (MainWindow.nacaProfile[i].X <= MainWindow.A.X && MainWindow.nacaProfile[i + 1].X >= MainWindow.A.X))
                    {
                        flag = 1;
                    }
                }
                else if (flag == 2)
                {
                    lowerDistance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                    if (MainWindow.nacaProfile[i + 1].X == 0 ||
                       (MainWindow.nacaProfile[i].X <= MainWindow.B.X && MainWindow.nacaProfile[i + 1].X >= MainWindow.B.X))
                    {
                        break;
                    }
                }
            }

            for (i = 0; i < MainWindow.nacaProfile.Count - 1; i++)
            {
                if (MainWindow.nacaProfile[i].Y >= 0 && MainWindow.nacaProfile[i + 1].Y > 0 && down)
                {
                    droveDistance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);

                    if (droveDistance >= dist * lowerDistance)
                    {
                        System.Windows.Vector v = new System.Windows.Vector(MainWindow.nacaProfile[i].X - MainWindow.nacaProfile[i + 1].X,
                            MainWindow.nacaProfile[i].Y - MainWindow.nacaProfile[i + 1].Y);
                        v.Normalize();
                        v = System.Windows.Vector.Multiply(droveDistance - dist * lowerDistance, v);

                        pointTmp = new PointF((float)(MainWindow.nacaProfile[i + 1].X + v.X), (float)(MainWindow.nacaProfile[i + 1].Y + v.Y));
                        break;
                    }
                }
                else if (MainWindow.nacaProfile[i].Y <= 0 && MainWindow.nacaProfile[i + 1].Y < 0 && !down)
                {
                    droveDistance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);

                    if (droveDistance >= dist * upperDistance)
                    {
                        System.Windows.Vector v = new System.Windows.Vector(MainWindow.nacaProfile[i].X - MainWindow.nacaProfile[i + 1].X,
                            MainWindow.nacaProfile[i].Y - MainWindow.nacaProfile[i + 1].Y);
                        v.Normalize();
                        v = System.Windows.Vector.Multiply(droveDistance - dist * upperDistance, v);

                        pointTmp = new PointF((float)(MainWindow.nacaProfile[i + 1].X + v.X), (float)(MainWindow.nacaProfile[i + 1].Y + v.Y));
                        break;
                    }
                }
            }
            PointF leading = new PointF();
            PointF falling = new PointF();

            if (MainWindow.level != 3)
            {
                leading = MainWindow.calibrateProfilePoint1.Point;
                falling = MainWindow.calibrateProfilePoint2.Point;
            }
            else if (MainWindow.level == 3)
            {
                leading = window.images.getActual().point1.Point;
                falling = window.images.getActual().point2.Point;
            }

            double d = MainWindow.GetDistanceBetween(leading, falling);
            if (MainWindow.PercentRealLenght == 100)
                MainWindow.PercentRealLenght = 99.999f;
            d = (d / MainWindow.PercentRealLenght) * 100;

            pointTmp.X *= (float)d;
            pointTmp.Y *= (float)d;

            return pointTmp;
        }
        private void tb_ratio_TextChanged(object sender, EventArgs e)
        {
            window.DeleteObjectPoint(point);
            try
            {
                point.ratio = (int)Double.Parse(tb_ratio.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            catch { }
            window.SetObjectPoint(point);
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

            window.DeleteObjectPoint(point);
            point.pen.Color = colorDialog1.Color;
            window.SetObjectPoint(point);

        }
    }
}

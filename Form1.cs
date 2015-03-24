using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Windows.Media;
namespace IFGPro
{
    public partial class Form1 : Form
    {
        List<PointF> arr;
        Mark s = new Mark(); Mark ea = new Mark(); Mark n = new Mark();
        List<PointF> sracky = new List<PointF>();
        List<PointF> funkce_tlaku = new List<PointF>();
        string x = "";
        string p = "";
        public Form1(List<PointF> array)
        {
            InitializeComponent();
            arr = array;
            imageBox1.Invalidate();
        }

        private void imageBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!s.IsEmpty())
            {
                e.Graphics.DrawLines(GlobalSettings.profilPen, offsetPoints(arr.ToArray()));
                s.DrawToGraphics(e.Graphics, imageBox1.GetOffsetPoint(s.Point));
                ea.DrawToGraphics(e.Graphics, imageBox1.GetOffsetPoint(ea.Point));
                n.DrawToGraphics(e.Graphics, imageBox1.GetOffsetPoint(n.Point));

                e.Graphics.DrawLine(new Pen(Color.Blue, 2), imageBox1.GetOffsetPoint(s.Point), imageBox1.GetOffsetPoint(ea.Point));
                e.Graphics.DrawLine(new Pen(Color.Red, 2), imageBox1.GetOffsetPoint(s.Point), imageBox1.GetOffsetPoint(n.Point));

                if (sracky.Count > 2)
                    e.Graphics.DrawLines(new Pen(Color.Green, 2), offsetPoints(sracky.ToArray()));
                if (funkce_tlaku.Count > 2)
                    e.Graphics.DrawLines(new Pen(Color.Green, 2), special_offsetPoints(funkce_tlaku.ToArray()));

            }
            
        }
        private PointF[] offsetPoints(PointF[] p)
        {
            List<PointF> list = new List<PointF>();

            foreach (PointF point in p)
            {
                list.Add(imageBox1.GetOffsetPoint(point));
            }
            return list.ToArray();
        }


        private PointF[] special_offsetPoints(PointF[] p)
        {
            List<PointF> list = new List<PointF>();

            foreach (PointF point in p)
            {
                list.Add(imageBox1.GetOffsetPoint(new PointF(point.X + 100, point.Y * (-1))));
            }
            return list.ToArray();
        }

        public void set(Mark _s, Mark _ea, Mark _n, PointF fce_tlaku)
        {
            s = _s;
            ea = _ea;
            n = _n;

            sracky.Add(_s.Point);
            funkce_tlaku.Add(fce_tlaku);
            x += fce_tlaku.X.ToString() + ", ";
            p += fce_tlaku.Y.ToString() + ", ";
            imageBox1.Invalidate();
        }
    }
}

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace IFGPro
{
    [Serializable()]
    public class Mark
    {
        public bool DrawCross = true;
        public PointF Point { set; get; }
        public int size { set; get; }
        public Color color { set; get; }
        public float width { set; get; }
        private PointF line1point1 = new PointF();
        private PointF line1point2 = new PointF();
        private PointF line2point1 = new PointF();
        private PointF line2point2 = new PointF();

        public Mark()
        {
            DrawCross = false;
        }
        public Mark(PointF p)
        {
            Point = p;
            DrawCross = false;
        }
        public Mark(int s, Color c)
        {
            Point = new PointF();
            size = s;
            color = c;
            width = 1;
        }
        public Mark(int s, Color c,float w)
        {
            Point = new PointF();
            size = s;
            color = c;
            width = w;
        }
        public Mark(PointF p, int s, Color c, float w)
        {
            Point = p;
            size = s;
            color = c;
            width = w;
        }

        public void setPoint(float x, float y)
        {
            Point = new PointF(x, y);
        }
        public void setPointDifference(float x, float y)
        {
            Point = new PointF(Point.X - x, Point.Y - y);
        }


        public bool IsHit(PointF p,Cyotek.Windows.Forms.ImageBox i)
        {
            if (!Point.IsEmpty)
            {
                PointF offsetPoint = i.GetOffsetPoint(Point);
                p = i.GetOffsetPoint(p);
                double s = size * 1;
                if ((offsetPoint.X + (s / 2)) >= p.X && (offsetPoint.X - (s / 2)) <= p.X && (offsetPoint.Y + (s / 2)) >= p.Y && (offsetPoint.Y - (s / 2)) <= p.Y)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public bool IsEmpty()
        {
            if (Point.IsEmpty)
                return true;
            else
                return false;
        }

        public void DrawToGraphics(Graphics g, PointF center)
        {
            if (DrawCross)
            {
                line1point1.X = center.X - size / 2;
                line1point1.Y = center.Y - size / 2;
                line1point2.X = center.X + size / 2;
                line1point2.Y = center.Y + size / 2;
                line2point1.X = center.X + size / 2;
                line2point1.Y = center.Y - size / 2;
                line2point2.X = center.X - size / 2;
                line2point2.Y = center.Y + size / 2;
                g.DrawLine(new Pen(color, width), line1point1, line1point2);
                g.DrawLine(new Pen(color, width), line2point1, line2point2);
            }
        }

        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}", (int)Point.X, (int)Point.Y);
        }
    }
}

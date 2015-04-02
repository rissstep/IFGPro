using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace IFGPro
{
    [Serializable()]
    public class ObjectPoint
    {
        public PointF location;
        public PointF locationWithOutOffset;
        public string label;
        public PointF labelLocation;
        public float labelOffsetX = 0;
        public float labelOffsetY = -30;
        public SizeF sizeString;
        public double angle;
        public double distance;
        [NonSerialized]
        public Pen pen;
        public string color;
        public bool onSurface = false;
        public double sufraceDist;
        public bool isElastic = false;
        public float ratio = 3;

        public ObjectPoint() { }
        public ObjectPoint(Pen p)
        {
            pen = p;
        }
        public ObjectPoint(Pen p,PointF l)
        {
            pen = p;
            location = l;
        }
        public void DrawToGraphics(Graphics g, PointF center)
        {
            g.DrawEllipse(pen, center.X - ratio, center.Y - ratio, ratio * 2, ratio * 2);
        }
        public void DrawToGraphics(Graphics g, PointF center,int n)
        {
            g.DrawEllipse(pen, center.X - ratio * n, center.Y - ratio * n, ratio * n * 2, ratio * n * 2);
        }

        public PointF GetLabelLocation(Cyotek.Windows.Forms.ImageBox i)
        {
            return new PointF(i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY)).X, i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY)).Y + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3)));
        }
        public PointF GetLabelLocation()
        {
            return new PointF(location.X + labelOffsetX, location.Y + labelOffsetY);
        }
        public bool IsHitLabel(PointF p, Cyotek.Windows.Forms.ImageBox i)
        {
            p = i.GetOffsetPoint(p);
            PointF offsetPoint = i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY));
            if (p.X < offsetPoint.X + sizeString.Width
                && p.X > offsetPoint.X
                && p.Y < offsetPoint.Y + sizeString.Height + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3))
                && p.Y > offsetPoint.Y + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3)))
                return true;
            return false;
        }

        public bool IsHit(PointF p, Cyotek.Windows.Forms.ImageBox i)
        {
            p = i.GetOffsetPoint(p);
            PointF offsetPoint = i.GetOffsetPoint(location);
            if ((p.X - offsetPoint.X) * (p.X - offsetPoint.X) + (p.Y - offsetPoint.Y) * (p.Y - offsetPoint.Y) <= ratio*ratio)
                return true;
            return false;
        }

        public void setPointLocationByPoint(PointF p, PointF leading, PointF falling)
        {
            System.Windows.Vector v1 = new System.Windows.Vector(falling.X - leading.X, falling.Y - leading.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(p.X - leading.X, p.Y - leading.Y);

            distance = MainWindow.GetDistanceBetween(p, leading);
            angle = System.Windows.Vector.AngleBetween(v1, v2);

            angle = angle * Math.PI / 180;

            double y = Math.Sin(angle) * distance;
            double x = Math.Cos(angle) * distance;

            locationWithOutOffset = new PointF((float)x, (float)y);
            setPointLocation(leading, falling);
            if (this.onSurface)
            {
                setSurfaceDistanceByPoint(leading, falling);
            }
            
        }

        public void setPointLocation(PointF leading, PointF falling)
        {
            //double d = MainWindow.GetDistanceBetween(leading, falling);
            //int distance = (int)d;

            System.Windows.Vector v1 = new System.Windows.Vector(falling.X - leading.X, falling.Y - leading.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            float sin = (float)Math.Sin(angleBetween);
            float cos = (float)Math.Cos(angleBetween);

            double xTmp, yTmp;

            location = new PointF(locationWithOutOffset.X, locationWithOutOffset.Y);

            xTmp = location.X * cos + location.Y * sin; //rotating
            yTmp = -location.X * sin + location.Y * cos;
            xTmp = xTmp + leading.X; // mooving
            yTmp = yTmp + leading.Y;

            location.X = (float)xTmp;
            location.Y = (float)yTmp;
        }

        private void setSurfaceDistanceByPoint(PointF leading, PointF falling)
        {
            double ratio = MainWindow.GetDistanceBetween(leading, falling);
            ratio = (ratio / MainWindow.PercentRealLenght) * 100;
            PointF pointTmp = new PointF((float)(locationWithOutOffset.X / ratio),(float)(locationWithOutOffset.Y / ratio));
            double distance = 0;
            //get surface distance
            int i;
            int flag = 0;
            double upperDistance = 0;
            double lowerDistance = 0;

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

            for (i = 0;i<MainWindow.nacaProfile.Count - 1;i++)
            {
                if (MainWindow.nacaProfile[i].Y >= 0 && MainWindow.nacaProfile[i + 1].Y > 0 && locationWithOutOffset.Y > 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        sufraceDist = distance / upperDistance;
                        break;
                    }
                    distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
                else if (MainWindow.nacaProfile[i].Y <= 0 && MainWindow.nacaProfile[i + 1].Y < 0 && locationWithOutOffset.Y < 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        sufraceDist = distance / lowerDistance;
                        break;
                    }
                    distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
            }

            //sufraceDist *= (MainWindow.PercentRealLenght/100); 
            if (pointTmp.X == 1)
                sufraceDist = 1;
            if (pointTmp.X == 0)
                sufraceDist = 0;


            sufraceDist = Math.Round(sufraceDist, 2);
        }
        
    }
}

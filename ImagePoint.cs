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
    public class ImagePoint
    {
        public PointF location;
        public string label;
        public PointF labelLocation;
        public float labelOffsetX = 0;
        public float labelOffsetY = -30;
        public SizeF sizeString;
        [NonSerialized]
        public Pen pen;
        public string color;
        public float ratio = 3;
        public bool exist = true;

        public ImagePoint() { }
        public ImagePoint(Pen p)
        {
            pen = p;
        }

        public ImagePoint(Pen p,PointF l)
        {
            pen = p;
            location = l;
        }
        public void DrawToGraphics(Graphics g, PointF center, bool isFringe = false, bool isSelected = false)
        {
            if(isFringe)
            {
                if(isSelected)
                    g.DrawEllipse(new Pen(GlobalSettings.selectedPen.Color,1.5f), center.X - GlobalSettings.fringeCircleSize, center.Y - GlobalSettings.fringeCircleSize, GlobalSettings.fringeCircleSize * 2, GlobalSettings.fringeCircleSize * 2);
                else
                    g.DrawEllipse(new Pen(GlobalSettings.fringeLabelsColor, 1.5f), center.X - GlobalSettings.fringeCircleSize, center.Y - GlobalSettings.fringeCircleSize, GlobalSettings.fringeCircleSize * 2, GlobalSettings.fringeCircleSize * 2);
            }
            else
            {
                g.DrawEllipse(pen, center.X - ratio, center.Y - ratio, ratio * 2, ratio * 2);
            }
            
        }

        public void DrawToGraphics(Graphics g, PointF center, int n)
        {
            g.DrawEllipse(pen, center.X - ratio * n, center.Y - ratio * n, ratio * n * 2, ratio * n * 2);
        }
        public PointF GetLabelLocation(Cyotek.Windows.Forms.ImageBox i)
        {
            return new PointF (i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY )).X,i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY )).Y + (float)(8 * ((i.ZoomFactor - 0.7)/0.3)));
        }
        public PointF GetLabelLocation()
        {
            return new PointF(location.X + labelOffsetX, location.Y + labelOffsetY);
        }
        public bool IsHitLabel(PointF p, Cyotek.Windows.Forms.ImageBox i, bool isFringeLabel = false)
        {
            PointF offsetPoint;
            if (isFringeLabel)
            {
                SizeF size = i.CreateGraphics().MeasureString(this.label,GlobalSettings.fringeLabelsFont);
                var l = i.GetOffsetPoint(location);
                //click = i.GetOffsetPoint(p);
                offsetPoint = i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY));
                if (p.X < offsetPoint.X + size.Width
                    && p.X > offsetPoint.X - 4
                    && p.Y < offsetPoint.Y + size.Height + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3)) 
                    && p.Y > offsetPoint.Y + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3)))
                    return true;
                return false;
            }

            p = i.GetOffsetPoint(p);
            offsetPoint = i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY));
            if (p.X < offsetPoint.X + sizeString.Width
                && p.X > offsetPoint.X
                && p.Y < offsetPoint.Y + sizeString.Height + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3))
                && p.Y > offsetPoint.Y + (float)(8 * ((i.ZoomFactor - 0.7) / 0.3)))
                return true;
            return false;
        }
        public bool IsHit(Point p, Cyotek.Windows.Forms.ImageBox i, bool isFringe = false)
        {
            if (isFringe)
            {
                p = i.PointToImage(p);
                if ((p.X - location.X) * (p.X - location.X) + (p.Y - location.Y) * (p.Y - location.Y) <= ratio * ratio)
                    return true;
                return false;
            }

            p = i.GetOffsetPoint(p);
            PointF offsetPoint = i.GetOffsetPoint(location);
            if ((p.X - offsetPoint.X) * (p.X - offsetPoint.X) + (p.Y - offsetPoint.Y) * (p.Y - offsetPoint.Y) <= ratio * ratio)
                return true;
            return false;
        }
        
    }
}

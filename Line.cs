using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Numerics;

namespace IFGPro
{
    [Serializable()]
    public class Line : INotifyPropertyChanged
    {
        public Mark pointOfProfile = new Mark();
        public Mark pointOnProfile = new Mark();
        public bool upSide { set; get; }
        private int _index { set; get; }
        private double _value { set; get; }
        public double distanceFromLeadingPoint;
        private string s { set; get; }
        private List<Line> ListUnDo = new List<Line>();
        public string viewIndex;

        private string f_index = String.Empty;
        private double fringe_i = 0;
        public double _pressure = 0;
        private double _velocity = 0;
        private double _density = 0;
        private double _mach = 0;
        private double _sur_coor = 0;

        public const double pi4 = Math.PI/4;
        public const double pi2 = Math.PI / 2;
        public const double _3pi4 = (3*Math.PI) / 4;
        public const double pi = Math.PI;
        public const double _5pi4 = (5 * Math.PI) / 4;
        public const double _3pi2 = (3 * Math.PI) / 2;
        public const double _7pi4 = (7 * Math.PI) / 4;
        public const double _2pi = Math.PI *2;
        
        public string Position
        { get { return string.Format("X:{0}, Y:{1}", (int)pointOnProfile.Point.X, (int)pointOnProfile.Point.Y); } set { this.s = value; } }

        public string F_Index
        {
            get { return f_index; }
            set
            {
                try
                {
                    fringe_i = Double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
                    CalculateParameters();
                    f_index = value;
                }
                catch
                {
                    f_index = String.Empty;
                }

                //OnPropertyChanged("F_Index");
            }
        }
        public int Index{ get { return _index; } set {
            _index = value;
            //OnPropertyChanged("Index"); 
        } }
        public double Sur_Coor { get { return Math.Round(_sur_coor,3); } set { _sur_coor = value; } }
        public double Pressure { get { return Math.Round(_pressure, 3) / 1000; } set { _pressure = value; } }
        public double Mach { get { return Math.Round(_mach, 3); } set { _mach = value; } }
        public double Velocity { get { return Math.Round(_velocity, 3); } set { _velocity = value; } }
        public double Desity { get { return Math.Round(_density, 3); } set { _density = value; } }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Line()
        {
        }
        public Line(Line l)
        {
            this.pointOfProfile.Point = l.pointOfProfile.Point;
            this.pointOnProfile.Point = l.pointOnProfile.Point;
        }
        public bool UnDo()
        {
            if (ListUnDo.Count > 1)
            {
                this.pointOfProfile.Point = ListUnDo[ListUnDo.Count - 1].pointOfProfile.Point;
                this.pointOnProfile.Point = ListUnDo[ListUnDo.Count - 1].pointOnProfile.Point;
                ListUnDo.RemoveAt(ListUnDo.Count - 1);
                return true;
            }
            else
                return false;
        }
        public void MakeRestorePoint()
        {
            ListUnDo.Add(new Line(this));
        }
        public bool IsHit(PointF p, Cyotek.Windows.Forms.ImageBox i)
        {
            p = i.GetOffsetPoint(p);
            PointF offsetPointOn = i.GetOffsetPoint(pointOnProfile.Point);
            PointF offsetPointOf = i.GetOffsetPoint(pointOfProfile.Point);

            System.Windows.Vector vAC = new System.Windows.Vector(p.X - offsetPointOn.X, p.Y - offsetPointOn.Y);
            System.Windows.Vector vAB = new System.Windows.Vector(offsetPointOf.X - offsetPointOn.X, offsetPointOf.Y - offsetPointOn.Y);

            double distanceOfProfile = GetDistanceBetween(offsetPointOn, p);
            double distanceFromProfile = GetDistanceBetween(offsetPointOf, p);

            double distace = GetDistanceBetween(offsetPointOn, offsetPointOf);
            double Ca = ((distanceOfProfile * distanceOfProfile) - (distanceFromProfile * distanceFromProfile) + (distace * distace)) / (2 * distace);
            double Vc = Math.Sqrt((distanceOfProfile * distanceOfProfile) - (Ca * Ca));

            double angle = System.Windows.Vector.AngleBetween(vAB, vAC);
            //angle = angle * (180 / Math.PI);
            double distanceFromLine = distanceFromProfile * Math.Sin(angle);

            distanceFromLine /= i.ZoomFactor;
            if (Vc <= 5 && distanceOfProfile<distace && distanceFromProfile<distace)
                return true;
            else
                return false;

        }
        public PointF LocationIndex(Cyotek.Windows.Forms.ImageBox i,Graphics e)
        {
            SizeF s = new SizeF();
            System.Windows.Vector v1 = new System.Windows.Vector(this.pointOfProfile.Point.X - this.pointOnProfile.Point.X, this.pointOfProfile.Point.Y - this.pointOnProfile.Point.Y);
            //v1.Normalize();
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            if (angleBetween < 0)
                angleBetween = 2 * pi + angleBetween;
            if(GlobalSettings.lineFringeNumber && GlobalSettings.linesDesc)
                s = e.MeasureString(this.Index.ToString() + "/" + this.F_Index, GlobalSettings.fontLines);
            else if (GlobalSettings.lineFringeNumber && !GlobalSettings.linesDesc)
                s = e.MeasureString( this.F_Index, GlobalSettings.fontLines);
            else if (!GlobalSettings.lineFringeNumber && GlobalSettings.linesDesc)
                s = e.MeasureString(this.Index.ToString(), GlobalSettings.fontLines);
            s = new SizeF((float)(s.Width / i.ZoomFactor), (float)(s.Height / i.ZoomFactor));

            PointF tmpPoint = new PointF(pointOfProfile.Point.X + (float)(s.Width * xFactor(angleBetween)), pointOfProfile.Point.Y + (float)(s.Height * yFactor(angleBetween)));

            return i.GetOffsetPoint(tmpPoint);
        }

        public PointF LocationIndexSuper(Cyotek.Windows.Forms.ImageBox i)
        {
            System.Windows.Vector v = new System.Windows.Vector(pointOfProfile.Point.X - pointOnProfile.Point.X, pointOfProfile.Point.Y - pointOnProfile.Point.Y);
            v.Normalize();
            double distancePlus = GetDistanceBetween(pointOnProfile.Point, pointOfProfile.Point) + (20);
            v = System.Windows.Vector.Multiply(distancePlus, v);
            PointF p = new PointF((float)(pointOnProfile.Point.X + v.X), (float)(pointOnProfile.Point.Y + v.Y));
            p.X -= 3;
            p.Y -= 3;

            return p;
        }

        private double GetDistanceBetween(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        private void CalculateParameters()
        {
            double T0 = MeasureParameters.t0 + 273.15;
            double C = (MeasureParameters.wave_lenght * 1e-9) / (MeasureParameters.L * 1e-3 * MeasureParameters.K);
            double ro0 = MeasureParameters.p0 / (MeasureParameters.R * T0);

            double tmp = ((ro0 - (C * fringe_i)) / ro0);

            double p0 = ro0 * Math.Pow(1 + (((MeasureParameters.k - 1) / 2) * Math.Pow(MeasureParameters.M,2)), (-1) / (MeasureParameters.k - 1));

            _pressure = MeasureParameters.p0 * Math.Pow(((p0 - (C * fringe_i)) / ro0), MeasureParameters.k);
            //_pressure = _pressure / 1000;


            //_mach = (Math.Sqrt(((2 / (MeasureParameters.k - 1)) * (Math.Pow(_pressure / MeasureParameters.p0, (1 - MeasureParameters.k) / MeasureParameters.k)) - 1)));

            _mach = Math.Sqrt(((2 / (MeasureParameters.k - 1)) * ((Math.Pow(_pressure / MeasureParameters.p0, (1 - MeasureParameters.k) / MeasureParameters.k)) - 1)));

            double Ti = T0 * Math.Pow((1 + (((MeasureParameters.k - 1) / 2) * _mach * _mach)), -1);

            _velocity = _mach * Math.Sqrt(MeasureParameters.k * MeasureParameters.R * Ti);
            _density = _pressure / (MeasureParameters.R * Ti);

            
        }

        public override string ToString()
        {
            return 
                F_Index.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t"
                + Index.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t" 
                + Math.Round(_sur_coor,GlobalSettings.roundOthers).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t"
                + Math.Round(_pressure,GlobalSettings.roundOthers).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t"
                + Math.Round(_mach,GlobalSettings.roundOthers).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t"
                + Math.Round(_velocity,GlobalSettings.roundOthers).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "\t"
                + Math.Round(_density, GlobalSettings.roundOthers).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
        }
        
        private double xFactor(double p)
        {
            if (isInInterval(0, pi4, p) || isInInterval(_7pi4, _2pi, p))
                return 0;
            else if(isInInterval(pi4,_3pi4,p))
            {
                return ((-2*p) / Math.PI) + 0.5;
            }
            else if (isInInterval(_3pi4, _5pi4, p))
            {
                return -1;
            }
            else if (isInInterval(_5pi4, _7pi4, p))
            {
                return ((2 * p) / Math.PI) - 3.5;
            }

            return 0;
        }
        private double yFactor(double p)
        {
            if (isInInterval(0, pi4, p) )
                return ((-2 * p) / Math.PI) - 0.5;
            else if (isInInterval(pi4, _3pi4, p))
            {
                return -1;
            }
            else if (isInInterval(_3pi4, _5pi4, p))
            {
                return ((2 * p) / Math.PI) - 2.5;
            }
            else if (isInInterval(_5pi4, _7pi4, p))
            {
                return 0;
            }
            else if (isInInterval(_7pi4, _2pi, p))
            {
                return ((-2 * p) / Math.PI) + 3.5;
            }

            return 0;
        }
        private bool isInInterval(double low, double high, double n)
        {
            if (low <= n && n <= high)
                return true;
            else
                return false;
        }
        
    }
}

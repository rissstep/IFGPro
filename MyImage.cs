using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Windows;


namespace IFGPro
{
    [Serializable()]
    public class MyImage : INotifyPropertyChanged
    {

        public string name { set; get; }
        public string path { set; get; }
        public List<Line> listLines { set; get; }
        private BindingList<Line> _listUpperLines;
        private BindingList<Line> _listDownerLines;
        //public List<Line> listUpperLines { set; get; }
        //public List<Line> listDownerLines { set; get; }
        //[XmlIgnoreAttribute]
        public PointF[] arrayProfile { set; get; }
        public string description { set; get; }
        public PointF locationDesc = new PointF(0,0);
        public SizeF sizeString;
        public bool isDone { set; get; }
        public Mark point1 { set; get; }
        public Mark point2 { set; get; }
        private List<Line> ListUnDo = new List<Line>();
        private int i;
        public PointF A;
        public PointF B;
        public PointF EA;

        public double drag_force = Double.NaN; 
        public double drag_force_u = Double.NaN; 
        public double drag_force_l = Double.NaN; 
        public double lift_force = Double.NaN; 
        public double lift_force_u = Double.NaN; 
        public double lift_force_l = Double.NaN; 
        public double M = Double.NaN; 
        public double M_u = Double.NaN; 
        public double M_l = Double.NaN; 
        private double upper_distance = Double.NaN;
        private double lower_distance = Double.NaN;
        private double drove_distance = 0;

        public List<ImagePoint> listFringeLabels = new List<ImagePoint>();

        public double time = Double.NaN;
        public double plunge = Double.NaN;
        public double vel_plunge = Double.NaN;
        public double pitch = Double.NaN;
        public double vel_pitch = Double.NaN;

        //public PointF[] arrayProfile { get { return _arrayProfile; } set { _arrayProfile = value;} }    

        public BindingList<Line> listUpperLines { get { return _listUpperLines; } set { 
            _listUpperLines = value; 
            //OnPropertyChanged("listUpperLines"); 
        } }
        public BindingList<Line> listDownerLines
        {
            get { return _listDownerLines; }
            set
            { 
            _listDownerLines = value; 
            //OnPropertyChanged("listDownerLines"); 
            } }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public MyImage()
        { 
        
        }

        public MyImage(string path)
        {
            point1 = new Mark(6, Color.Red);
            point2 = new Mark(6, Color.Red);

            listLines = new List<Line>();
            //listUpperLines = new BindingList<Line>();
            //listDownerLines = new BindingList<Line>();
            listUpperLines = new BindingList<Line>();
            listDownerLines = new BindingList<Line>();

            this.path = path;
            this.name = Path.GetFileName(path);
        }
        public void AddLine(PointF p)
        {
            Line l = new Line();
            PointF pointOnProfile = GetPointOnProfile(arrayProfile, p);
            PointF pointOfProfile = GetOffPoint(pointOnProfile, p, GlobalSettings.lineLength);

            l.pointOnProfile = new Mark(pointOnProfile);
            l.pointOfProfile = new Mark(pointOfProfile);
            //l.PenColor = Color.Green;
            //l.PenWidth = 2;
            l.upSide = resortLines(l);
            listLines.Add(l);
            sortLines();
            this.MakeRestorePoint(l);
            l.Sur_Coor = GetSurfaceCoor(l);
        }
        public void setLine(Line l,PointF p)
        {
            PointF pointOnProfile = GetPointOnProfile(arrayProfile, p);
            PointF pointOfProfile = GetOffPoint(pointOnProfile,p,GlobalSettings.lineLength);
            try
            {
                l.pointOfProfile.Point = pointOfProfile;
                l.pointOnProfile.Point = pointOnProfile;
            }
            catch (Exception e)
            {
                MessageBox.Show("neco se posralo kamarade> "+e.ToString());
            }

            try
            {
                l.upSide = resortLines(l);
                sortLines();
            }
            catch (Exception e)
            {
                MessageBox.Show("neco se posralo kamarade> " + e.ToString());
            }

            //l.Sur_Coor = GetSurfaceCoor(l);
        }
        public void setPoint1(PointF p, bool isMoving = false, float diferenceX = 0,float diferenceY = 0)
        {
            point1.Point = p;
            if (isMoving)
            {
                MoveProfile(diferenceX, diferenceY);
                point2.setPoint(point2.Point.X - diferenceX, point2.Point.Y - diferenceY); 
            }
            else
                arrayProfile = ArrayProfil();

            foreach (Line l in listLines)
            {
                setLine(l, l.pointOfProfile.Point);
            }
        }
        public void setPoint2(PointF p, bool isMoving = false, float diferenceX = 0, float diferenceY = 0)
        {
            point2.Point = p;
            if (isMoving)
            {
                MoveProfile(diferenceX, diferenceY);
                point1.setPoint(point1.Point.X - diferenceX, point1.Point.Y - diferenceY);
            }
            else
                arrayProfile = ArrayProfil();
            foreach (Line l in listLines)
            {
                setLine(l, l.pointOfProfile.Point);
            }
        }
        public PointF[] GetAllLines()
        {
            List<PointF> list = new List<PointF>();

            foreach(Line l in listLines)
            {
                list.Add(l.pointOfProfile.Point);
                list.Add(l.pointOnProfile.Point);
            }
            return list.ToArray();
        }
        public PointF[] ArrayProfil() 
        {
            arrayProfile = ProfileNaca(point1.Point, point2.Point);
            return arrayProfile;
        }
        public bool resortLines(Line l)
        {
            System.Windows.Vector v1 = new System.Windows.Vector(point2.Point.X - point1.Point.X, point2.Point.Y - point1.Point.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(l.pointOnProfile.Point.X - point1.Point.X, l.pointOnProfile.Point.Y - point1.Point.Y);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            //MessageBox.Show(angleBetween.ToString());

            if (angleBetween < 0)
            {
                if (!l.upSide)
                {
                    listUpperLines.Add(l);
                    if (IsLineInList(l,listUpperLines))
                        listDownerLines.Remove(l);
                }
                return true;
            }
            else
            {
                if (!IsLineInList(l,listDownerLines))
                {
                    listDownerLines.Add(l);
                    if (IsLineInList(l,listDownerLines))
                        listUpperLines.Remove(l);
                }
                return false;
            }
        }
        public void MoveProfile(float differenceX = 0, float differenceY = 0)
        {
            for (i = 0; i < arrayProfile.Length;i++)
            {
                arrayProfile[i].X = (float)(arrayProfile[i].X - differenceX);
                arrayProfile[i].Y = (float)(arrayProfile[i].Y - differenceY);
            }

            A.X -= differenceX;
            A.Y -= differenceY;
            B.X -= differenceX;
            B.Y -= differenceY;
        }
        private PointF[] ProfileNaca(PointF p1, PointF p2)
        {
            double d = (GetDistanceBetween(p1, p2) / MainWindow.PercentRealLenght) * 100;
            int distance = (int)d;

            System.Windows.Vector v1 = new System.Windows.Vector(p2.X - p1.X, p2.Y - p1.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            float sin = (float)Math.Sin(angleBetween);
            float cos = (float)Math.Cos(angleBetween);

            double xTmp, yTmp;
            float xMiddleBtwnAB = 0;
            float yMiddleBtwnAB = 0;

            if (!(MainWindow.A == null || MainWindow.B == null))
            {
                A = MainWindow.A;
                B = MainWindow.B;

                xTmp = A.X * cos + A.Y * sin; //rotating
                yTmp = -A.X * sin + A.Y * cos;
                xTmp = xTmp * d + p1.X; // mooving and scaling
                yTmp = yTmp * d + p1.Y;
                A.X = (float)xTmp;
                A.Y = (float)yTmp;

                xTmp = B.X * cos + B.Y * sin; //rotating
                yTmp = -B.X * sin + B.Y * cos;
                xTmp = xTmp * d + p1.X; // mooving and scaling
                yTmp = yTmp * d + p1.Y;
                B.X = (float)xTmp;
                B.Y = (float)yTmp;


                xMiddleBtwnAB = MainWindow.B.X * cos + ((MainWindow.A.Y - MainWindow.B.Y) / 2 + MainWindow.B.Y) * sin; //rotating
                yMiddleBtwnAB = -MainWindow.B.X * sin + ((MainWindow.A.Y - MainWindow.B.Y) / 2 + MainWindow.B.Y) * cos;
                xMiddleBtwnAB = (float)(xMiddleBtwnAB * d + p1.X); // mooving and scaling
                yMiddleBtwnAB = (float)(yMiddleBtwnAB * d + p1.Y);
            }

            List<PointF> testList = new List<PointF>();
            bool flagIfOverRealLenght = false;
            foreach (PointF p in MainWindow.nacaProfile)
            {
                if (p.X > MainWindow.A.X)
                {
                    if (!flagIfOverRealLenght)
                    {
                        testList.Add(A);
                        testList.Add(new PointF(xMiddleBtwnAB, yMiddleBtwnAB));
                        testList.Add(new PointF(p1.X, p1.Y));
                        flagIfOverRealLenght = true;
                    }
                    continue;
                }

                xTmp = p.X * cos + p.Y * sin; //rotating
                yTmp = -p.X * sin + p.Y * cos;

                xTmp = xTmp * d + p1.X; // mooving
                yTmp = yTmp * d + p1.Y;
                testList.Add(new PointF((float)xTmp, (float)yTmp));
            }

            testList.Add(B);
            testList.Add(new PointF(xMiddleBtwnAB, yMiddleBtwnAB));

            
            return testList.ToArray();

        }
        public static PointF GetPointOnProfile(PointF[] array, PointF p)
        {
            PointF fClosest = PointF.Empty;    //first closest
            double fDistance = double.MaxValue;
            PointF sClosest = PointF.Empty;    //second closest
            double sDistance = double.MaxValue;
            PointF tmpP;
            double tmpD;


            foreach (PointF testPoint in array)
            {
                if (MainWindow.GetDistanceBetween(testPoint, p) < sDistance)
                {
                    sDistance = MainWindow.GetDistanceBetween(testPoint, p);
                    sClosest = testPoint;                    
                }
                if (MainWindow.GetDistanceBetween(testPoint, p) < fDistance)
                {
                    tmpD = fDistance;
                    tmpP = fClosest;
                    fDistance = sDistance;
                    fClosest = sClosest;
                    sClosest = tmpP;
                    sDistance = tmpD;
                }
            }

            //User normal vector of vector fClosest and sClosest and select orientation
            double distace = MainWindow.GetDistanceBetween(fClosest, sClosest);
            double Ca = ((fDistance * fDistance) - (sDistance * sDistance) + (distace * distace)) / (2 * distace);
            double Vc = Math.Sqrt((fDistance * fDistance) - (Ca * Ca));

            double prdel = sClosest.X - fClosest.X;
            double kysela = sClosest.Y - fClosest.Y;
            System.Windows.Vector v1 = new System.Windows.Vector((-1) * kysela, prdel);
            System.Windows.Vector v2 = new System.Windows.Vector(kysela, (-1) * prdel);

            v1.Normalize();
            v1 = System.Windows.Vector.Multiply(Vc, v1);
            v2.Normalize();
            v2 = System.Windows.Vector.Multiply(Vc, v2);

            if (MainWindow.GetDistanceBetween(fClosest, new PointF((float)((p.X + v1.X)), (float)((p.Y + v1.Y)))) > MainWindow.GetDistanceBetween(fClosest, new PointF((float)((p.X + v2.X)), (float)((p.Y + v2.Y)))))
            {
                v1.X = v2.X;
                v1.Y = v2.Y;
            }
            if ((distace < MainWindow.GetDistanceBetween(sClosest, new PointF((float)((p.X + v1.X)), (float)((p.Y + v1.Y))))) || double.IsNaN(v1.X))
            {
                return fClosest;
            }

            return new PointF((float)((p.X + v1.X)), (float)((p.Y + v1.Y)));
            
        }
        private PointF GetOffPoint(PointF p1,PointF p2,int lenght)
        {
            double distance = GetDistanceBetween(p1, p2);
            double ratio = lenght / distance;
            double vectorX = (p2.X - p1.X) * ratio;
            double vectorY = (p2.Y - p1.Y) * ratio;
            return new PointF((float)(p1.X+vectorX),(float)(p1.Y+vectorY));
   
        }
        private double GetDistanceBetween(PointF p1, PointF p2) 
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        private bool IsLineInList(Line testL, BindingList<Line> bl)
        {
            foreach (Line l in bl)
            {
                if (l.Equals(testL))
                    return true;
            }
            return false;
        }
        public void sortLines()
        {
            foreach (Line l in listUpperLines)
            {
                l.distanceFromLeadingPoint = GetDistanceBetween(l.pointOnProfile.Point, point1.Point);
            }
            foreach (Line l in listDownerLines)
            {
                l.distanceFromLeadingPoint = GetDistanceBetween(l.pointOnProfile.Point, point1.Point);
            }
            listDownerLines = new BindingList<Line>(listDownerLines.OrderBy(o => o.distanceFromLeadingPoint).ToList());
            listUpperLines = new BindingList<Line>(listUpperLines.OrderBy(o => o.distanceFromLeadingPoint).ToList());
            int i = 1;
            foreach (Line l in listUpperLines)
            {
                l.Index = i;
                i++;
            }
            i = 1;
            foreach (Line l in listDownerLines)
            {
                l.Index = i;
                i++;
            }
        }
        public void MakeRestorePoint(Line l)
        {
            ListUnDo.Add(l);
            l.MakeRestorePoint();
        }
        public void UnDo()
        {
            if (ListUnDo.Count > 0)
            {
                if (!ListUnDo[ListUnDo.Count - 1].UnDo())
                {
                    if (ListUnDo[ListUnDo.Count - 1].upSide)
                    {
                        if (listUpperLines.Count == 0)
                        {
                            listUpperLines.Add(ListUnDo[ListUnDo.Count - 1]);
                        }
                            listUpperLines.Remove(ListUnDo[ListUnDo.Count - 1]);
                    }
                    else
                    {
                        listDownerLines.Remove(ListUnDo[ListUnDo.Count - 1]);
                    }
                    listLines.Remove(ListUnDo[ListUnDo.Count - 1]);
                }
                else
                {
                    setLine(ListUnDo[ListUnDo.Count - 1], ListUnDo[ListUnDo.Count - 1].pointOfProfile.Point);
                }

                ListUnDo.RemoveAt(ListUnDo.Count - 1);
            }
        }
        public double GetSurfaceCoor(Line l)
        {
            if (l == null)
                return double.NaN;
            System.Windows.Vector v1 = new System.Windows.Vector(point2.Point.X - point1.Point.X, point2.Point.Y - point1.Point.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(l.pointOnProfile.Point.X - point1.Point.X, l.pointOnProfile.Point.Y - point1.Point.Y);

            double distance = MainWindow.GetDistanceBetween(l.pointOnProfile.Point, point1.Point);
            double angle = System.Windows.Vector.AngleBetween(v1, v2);

            angle = angle * Math.PI / 180;
            
            double y = Math.Sin(angle) * distance;
            double x = Math.Cos(angle) * distance;

            PointF locationWithOutOffset = new PointF((float)x, (float)y);

            double ratio = MainWindow.GetDistanceBetween(point1.Point, point2.Point);
            ratio = (ratio / MainWindow.PercentRealLenght) * 100;
            PointF pointTmp = new PointF((float)(locationWithOutOffset.X / ratio), (float)(locationWithOutOffset.Y / ratio));
            double sufraceDist = 0;


            int i;
            int flag = 0;
            double upperDistance = 0;
            double lowerDistance = 0;
            double droveDist = 0;
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
                if (MainWindow.nacaProfile[i].Y >= 0 && MainWindow.nacaProfile[i + 1].Y > 0 && locationWithOutOffset.Y > 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        droveDist += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        sufraceDist = droveDist / upperDistance;
                        break;
                    }
                    droveDist += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
                else if (MainWindow.nacaProfile[i].Y <= 0 && MainWindow.nacaProfile[i + 1].Y < 0 && locationWithOutOffset.Y < 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        droveDist += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        sufraceDist = droveDist / lowerDistance;
                        break;
                    }
                    droveDist += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
            }

            if (pointTmp.X == 1)
                sufraceDist = 1;
            if (pointTmp.X == 0)
                sufraceDist = 0;


            sufraceDist = Math.Round(sufraceDist, 2);
            return sufraceDist;
          
        }
        public override string ToString() 
        {
            return name + "\t" + Math.Round(time,GlobalSettings.roundTime + 1).ToString(CultureInfo.CreateSpecificCulture("en-GB"))
                + "\t" + Math.Round(pitch, GlobalSettings.roundPitchPlunge + 1).ToString(CultureInfo.CreateSpecificCulture("en-GB"))
                + "\t" + Math.Round(plunge, GlobalSettings.roundPitchPlunge + 1).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
        }
        public bool isDesHit(PointF p, Cyotek.Windows.Forms.ImageBox i)
        {
            p = i.GetOffsetPoint(p);
            PointF offsetPoint = i.GetOffsetPoint(locationDesc);
            if (p.X < offsetPoint.X + sizeString.Width
                && p.X > offsetPoint.X
                && p.Y < offsetPoint.Y + sizeString.Height
                && p.Y > offsetPoint.Y)
                return true;
            return false;
        }
        public PointF[] getArrayProfile()
        {
            return arrayProfile;
        }

        private void upper_and_lower_distance(out double _upper_distance, out double _lower_distance){
            int i = 0;
            _upper_distance = 0;
            _lower_distance = 0;
            int flag = 0;

            for (i = 0; i < MainWindow.nacaProfile.Count - 1; i++)
            {
                if (flag == 1 && MainWindow.nacaProfile[i].X == 0 && MainWindow.nacaProfile[i].Y == 0)
                    flag = 2;
                if (flag == 0)
                {
                    _upper_distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                    if (MainWindow.nacaProfile[i + 1].X == 0 ||
                       (MainWindow.nacaProfile[i].X <= MainWindow.A.X && MainWindow.nacaProfile[i + 1].X >= MainWindow.A.X))
                    {
                        flag = 1;
                    }
                }
                else if (flag == 2)
                {
                    _lower_distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                    if (MainWindow.nacaProfile[i + 1].X == 0 ||
                       (MainWindow.nacaProfile[i].X <= MainWindow.B.X && MainWindow.nacaProfile[i + 1].X >= MainWindow.B.X))
                    {
                        break;
                    }
                }
            }
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

            double d = MainWindow.GetDistanceBetween(point1.Point, point1.Point);
            if (MainWindow.PercentRealLenght == 100)
                MainWindow.PercentRealLenght = 99.999f;
            d = (d / MainWindow.PercentRealLenght) * 100;

            pointTmp.X *= (float)d;
            pointTmp.Y *= (float)d;

            return pointTmp;
        }

        private void iterate_upper_side(int steps)
        {
            int i = 0,j;
            PointF pointTmp;
            double drove_distance = 0;
            double dist = (1-0.5)*(1/steps);

            double T0 = MeasureParameters.t0 + 273.15;
            double ro0 = MeasureParameters.p0 / (MeasureParameters.R * T0);
            double p0 = ro0 * Math.Pow(1 + (((MeasureParameters.k - 1) / 2) * Math.Pow(MeasureParameters.M,2)), (-1) / (MeasureParameters.k - 1));

            drag_force_u =0;
            lift_force_u =0;
            M_u =0;
            double w = MeasureParameters.L * 1e-3;

            for (j = 0; j < steps; j++ ){
                dist = (j + 1.0 - 0.5) * (1.0 / (double)(steps));
                for (i = i; i < MainWindow.nacaProfile.Count - 1; i++)
                {
                    if (MainWindow.nacaProfile[i].Y >= 0 && MainWindow.nacaProfile[i + 1].Y > 0)
                    {
                        drove_distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);

                        if (drove_distance >= dist * upper_distance)
                        {
                            Vector v = new Vector(  MainWindow.nacaProfile[i].X - MainWindow.nacaProfile[i + 1].X,
                                                    MainWindow.nacaProfile[i].Y - MainWindow.nacaProfile[i + 1].Y);
                            v.Normalize();
                            Vector n1 = new Vector(v.Y, -v.X);
                            Vector n2 = new Vector(-v.Y, v.X);
                            Vector n;

                            v = Vector.Multiply(drove_distance - dist * upper_distance, v);
                            pointTmp = new PointF((float)(MainWindow.nacaProfile[i + 1].X + v.X), (float)(MainWindow.nacaProfile[i + 1].Y + v.Y));

                            if (MainWindow.GetDistanceBetween(new PointF((float)(pointTmp.X + n1.X), (float)(pointTmp.Y + n1.Y)), new PointF((float)0.5, (float)0.0)) < 
                                MainWindow.GetDistanceBetween(new PointF((float)(pointTmp.X + n2.X), (float)(pointTmp.Y + n2.Y)), new PointF((float)0.5, (float)0.0))
                                )
                                n = n1;
                            else
                                n = n2;

                            //vecotr of 1m pointing inside
                            n = Vector.Multiply(mm2px(1000), n);
                            
                            PointF nPoint = new PointF((float)(pointTmp.X + n.X), (float)(pointTmp.Y + n.Y));

                            double dis = MainWindow.GetDistanceBetween(point1.Point, point2.Point);
                            if (MainWindow.PercentRealLenght == 100)
                                MainWindow.PercentRealLenght = 99.999f;
                            dis = (dis / MainWindow.PercentRealLenght) * 100;

                            pointTmp.X *= (float)dis;
                            pointTmp.Y *= (float)dis;

                            pointTmp = rotation_and_moving(pointTmp);
                            n = rotation(n);
                            n.Y *= -1;
                            Vector dTmp = new Vector((float)(EA.X - pointTmp.X), (float)(-EA.Y + pointTmp.Y));

                            double p_tmp = (p_upper((j - 0.5) * (1.0 / steps)) - MeasureParameters.p0);


                            drag_force_u += p_tmp * px2mm(n.X) * 1e-3;
                            lift_force_u += p_tmp * px2mm(n.Y) * 1e-3;
                            M_u += px2mm((Vector.CrossProduct(dTmp, n))) * 1e-3 * p_tmp;
                            break;                                
                        }
                    }
                }
            }

            drag_force_u *= (1.0 / steps) * w;
            lift_force_u *= (1.0 / steps)* w ;
            M_u *= (1.0 / steps)* w;

        }

        private void iterate_lower_side(int steps)
        {
            int i = 0, j;
            PointF pointTmp;
            double drove_distance = 0;
            double dist = (1 - 0.5) * (1 / steps);

            double T0 = MeasureParameters.t0 + 273.15;
            double ro0 = MeasureParameters.p0 / (MeasureParameters.R * T0);
            double p0 = ro0 * Math.Pow(1 + (((MeasureParameters.k - 1) / 2) * Math.Pow(MeasureParameters.M, 2)), (-1) / (MeasureParameters.k - 1));

            drag_force_l = 0;
            lift_force_l = 0;
            M_l = 0;
            double w = MeasureParameters.L * 1e-3;

            for (j = 0; j < steps; j++)
            {
                dist = (j + 1.0 - 0.5) * (1.0 / (double)(steps));
                for (i = i; i < MainWindow.nacaProfile.Count - 1; i++)
                {
                    if (MainWindow.nacaProfile[i].Y <= 0 && MainWindow.nacaProfile[i + 1].Y < 0)
                    {
                        drove_distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);

                        if (drove_distance >= dist * upper_distance)
                        {
                            Vector v = new Vector(MainWindow.nacaProfile[i].X - MainWindow.nacaProfile[i + 1].X,
                                                    MainWindow.nacaProfile[i].Y - MainWindow.nacaProfile[i + 1].Y);
                            v.Normalize();
                            Vector n1 = new Vector(v.Y, -v.X);
                            Vector n2 = new Vector(-v.Y, v.X);
                            Vector n;

                            v = Vector.Multiply(drove_distance - dist * lower_distance, v);
                            pointTmp = new PointF((float)(MainWindow.nacaProfile[i + 1].X + v.X), (float)(MainWindow.nacaProfile[i + 1].Y + v.Y));

                            if (MainWindow.GetDistanceBetween(new PointF((float)(pointTmp.X + n1.X), (float)(pointTmp.Y + n1.Y)), new PointF((float)0.5, (float)0.0)) <
                                MainWindow.GetDistanceBetween(new PointF((float)(pointTmp.X + n2.X), (float)(pointTmp.Y + n2.Y)), new PointF((float)0.5, (float)0.0))
                                )
                                n = n1;
                            else
                                n = n2;

                            //vecotr of 1m pointing inside
                            n = Vector.Multiply(mm2px(1000), n);

                            PointF nPoint = new PointF((float)(pointTmp.X + n.X), (float)(pointTmp.Y + n.Y));

                            double dis = MainWindow.GetDistanceBetween(point1.Point, point2.Point);
                            if (MainWindow.PercentRealLenght == 100)
                                MainWindow.PercentRealLenght = 99.999f;
                            dis = (dis / MainWindow.PercentRealLenght) * 100;

                            pointTmp.X *= (float)dis;
                            pointTmp.Y *= (float)dis;

                            pointTmp = rotation_and_moving(pointTmp);
                            n = rotation(n);
                            Vector dTmp = new Vector((float)(EA.X - pointTmp.X), (float)(EA.Y - pointTmp.Y));

                            double p_tmp = (p_lower((j - 0.5) * (1.0 / steps)) - MeasureParameters.p0);


                            drag_force_l += p_tmp * px2mm(n.X) * 1e-3;
                            lift_force_l += p_tmp * px2mm(n.Y) * 1e-3;
                            M_l += px2mm(Math.Abs(Vector.CrossProduct(dTmp, n))) * 1e-3 * p_tmp;
                            break;
                        }
                    }
                }
            }

            drag_force_l *= (1.0 / steps) * w * (1.0 / steps);
            lift_force_l *= (1.0 / steps) * w * (1.0 / steps);
            M_l *= (1.0 / steps) * w * (1.0 / steps);

        }

        private PointF rotation_and_moving(PointF p)
        {
            System.Windows.Vector v1 = new System.Windows.Vector(point1.Point.X - point2.Point.X, point1.Point.Y - point2.Point.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            float sin = (float)Math.Sin(angleBetween);
            float cos = (float)Math.Cos(angleBetween);

            double xTmp, yTmp;

            

            xTmp = p.X * cos + p.Y * sin; //rotating
            yTmp = -p.X * sin + p.Y * cos;
            xTmp = xTmp + point1.Point.X; // mooving
            yTmp = yTmp + point1.Point.Y;

            return new PointF((float)xTmp, (float)yTmp);
        }

        private Vector rotation(Vector p)
        {
            System.Windows.Vector v1 = new System.Windows.Vector(point1.Point.X - point2.Point.X, point1.Point.Y - point2.Point.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            float sin = (float)Math.Sin(angleBetween);
            float cos = (float)Math.Cos(angleBetween);

            double xTmp, yTmp;



            xTmp = p.X * cos + p.Y * sin; //rotating
            yTmp = -p.X * sin + p.Y * cos;

            return new Vector((float)xTmp, (float)yTmp);
        }


        private double p_upper(double s)
        {
            int i;
            double a = 0, b = 0;
            int lastLine = listUpperLines.Count-1;
            // ____l2(x2,y2)______s_______l1(x1,y2)_____
            // p(s) = s*a+b
            // y = ax+b
            // a = (y2-y1)/(x2-x1)
            // b = y1-x1a
            if(surface_coor_by_point(listUpperLines[0].pointOnProfile.Point) > s)
            {
                a = (listDownerLines[0]._pressure - listUpperLines[0]._pressure)/(surface_coor_by_point(listDownerLines[0].pointOnProfile.Point) - surface_coor_by_point(listUpperLines[0].pointOnProfile.Point));
                b = listUpperLines[0]._pressure - (surface_coor_by_point(listUpperLines[0].pointOnProfile.Point)*a);
                return s*a+b;
            }
            if(surface_coor_by_point(listUpperLines[lastLine].pointOnProfile.Point) < s)
            {
                double x1 = 1.0;
                double y1 = 0.0;
                a = (listUpperLines[lastLine]._pressure - y1)/(surface_coor_by_point(listUpperLines[lastLine].pointOnProfile.Point) - x1);
                b = y1 - (x1 * a);
                return s*a+b;
            }
            for (i = 0; i < listUpperLines.Count - 1; i++)
            {
                if(surface_coor_by_point(listUpperLines[i].pointOnProfile.Point) < s && s < surface_coor_by_point(listUpperLines[i+1].pointOnProfile.Point))
                {
                    a = (listUpperLines[i]._pressure - listUpperLines[i + 1]._pressure) / (surface_coor_by_point(listUpperLines[i].pointOnProfile.Point) - surface_coor_by_point(listUpperLines[i + 1].pointOnProfile.Point));
                    b = listUpperLines[i+1]._pressure - (surface_coor_by_point(listUpperLines[i+1].pointOnProfile.Point) * a);
                    return s * a + b;
                }
            }
            return 0;
        }

        private double p_lower(double s)
        {
            int i;
            double a = 0, b = 0;
            int lastLine = listDownerLines.Count - 1;
            // ____l2(x2,y2)______s_______l1(x1,y2)_____
            // p(s) = s*a+b
            // y = ax+b
            // a = (y2-y1)/(x2-x1)
            // b = y1-x1a
            if (surface_coor_by_point(listDownerLines[0].pointOnProfile.Point) > s)
            {
                a = (listUpperLines[0]._pressure - listDownerLines[0]._pressure) / (surface_coor_by_point(listUpperLines[0].pointOnProfile.Point) - surface_coor_by_point(listDownerLines[0].pointOnProfile.Point));
                b = listDownerLines[0]._pressure - (surface_coor_by_point(listDownerLines[0].pointOnProfile.Point) * a);
                return s * a + b;
            }
            if (surface_coor_by_point(listDownerLines[lastLine].pointOnProfile.Point) < s)
            {
                double x1 = 1;
                double y1 = 0;
                a = (listDownerLines[lastLine]._pressure - y1) / (surface_coor_by_point(listDownerLines[lastLine].pointOnProfile.Point) - x1);
                b = y1 - (x1 * a);
                return s * a + b;
            }
            for (i = 0; i < listDownerLines.Count - 1; i++)
            {
                if (surface_coor_by_point(listDownerLines[i].pointOnProfile.Point) < s && s < surface_coor_by_point(listDownerLines[i + 1].pointOnProfile.Point))
                {
                    a = (listDownerLines[i]._pressure - listDownerLines[i + 1]._pressure) / (surface_coor_by_point(listDownerLines[i].pointOnProfile.Point) - surface_coor_by_point(listDownerLines[i + 1].pointOnProfile.Point));
                    b = listDownerLines[i + 1]._pressure - (surface_coor_by_point(listDownerLines[i + 1].pointOnProfile.Point) * a);
                    return s * a + b;
                }
            }
            return 0;
        }

        private double surface_coor_by_point(PointF p)
        {
            double ratio = MainWindow.GetDistanceBetween(point1.Point, point2.Point);
            ratio = (ratio / MainWindow.PercentRealLenght) * 100;
            p = funky_diana_rose(p);
            PointF pointTmp = new PointF((float)(p.X / ratio), (float)(p.Y / ratio));
            double distance = 0;
            //get surface distance
            int i;
            double surfaceDist = 0;

            for (i = 0; i < MainWindow.nacaProfile.Count - 1; i++)
            {
                if (MainWindow.nacaProfile[i].Y >= 0 && MainWindow.nacaProfile[i + 1].Y > 0 && p.Y > 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        surfaceDist = distance / upper_distance;
                        break;
                    }
                    distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
                else if (MainWindow.nacaProfile[i].Y <= 0 && MainWindow.nacaProfile[i + 1].Y < 0 && p.Y < 0)
                {
                    if (MainWindow.nacaProfile[i].X < pointTmp.X && pointTmp.X < MainWindow.nacaProfile[i + 1].X)
                    {
                        distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], pointTmp);
                        surfaceDist = distance / lower_distance;
                        break;
                    }
                    distance += MainWindow.GetDistanceBetween(MainWindow.nacaProfile[i], MainWindow.nacaProfile[i + 1]);
                }
            }

            //sufraceDist *= (MainWindow.PercentRealLenght/100); 
            if (pointTmp.X == 1)
                surfaceDist = 1;
            if (pointTmp.X == 0)
                surfaceDist = 0;

            return surfaceDist;
        }

        private PointF funky_diana_rose(PointF _p)
        {
            System.Windows.Vector v1 = new System.Windows.Vector(point2.Point.X - point1.Point.X, point2.Point.Y - point1.Point.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(_p.X - point1.Point.X, _p.Y - point1.Point.Y);

            double distance = MainWindow.GetDistanceBetween(_p, point1.Point);
            double angle = System.Windows.Vector.AngleBetween(v1, v2);

            angle = angle * Math.PI / 180;

            double y = Math.Sin(angle) * distance;
            double x = Math.Cos(angle) * distance;

            return new PointF((float)x, (float)y);
        }

        public void computate_DLM(ref PointF ea)
        {
            EA = ea;
            if (Double.IsNaN(upper_distance) || Double.IsNaN(lower_distance))
                upper_and_lower_distance(out upper_distance,out lower_distance);

            iterate_upper_side(1000);
            iterate_lower_side(1000);

            drag_force = drag_force_l + drag_force_u;
            lift_force = lift_force_l + lift_force_u;
            M = M_l + M_u;
        }

        public void set_DML_NaN()
        {
            drag_force = Double.NaN; 
            drag_force_u = Double.NaN; 
            drag_force_l = Double.NaN; 
            lift_force = Double.NaN; 
            lift_force_u = Double.NaN; 
            lift_force_l = Double.NaN; 
            M = Double.NaN; 
            M_u = Double.NaN; 
            M_l = Double.NaN; 
        }

        public void test_to_compute_DLM(ref PointF ea)
        {
            if (this.listDownerLines.Count >= 2 && this.listUpperLines.Count >= 2)
            {
                foreach (Line l in this.listDownerLines)
                    if (l.F_Index == String.Empty)
                    {
                        this.set_DML_NaN();
                        return;
                    }

                foreach (Line l in this.listUpperLines)
                    if (l.F_Index == String.Empty)
                    {
                        this.set_DML_NaN();
                        return;
                    }


                this.computate_DLM(ref ea);
                return;
            }
            this.set_DML_NaN();
        }

        private double px2mm(double px)
        {
            return Math.Round((px * (1 / GlobalSettings.ratio)), 3);
        }
        private double mm2px(double mm)
        {
            return Math.Round((mm * GlobalSettings.ratio), 3);
        }

    }
    
}

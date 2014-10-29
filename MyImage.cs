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

        public List<ImagePoint> listFringeLabels = new List<ImagePoint>();

        public double time = Double.NaN;
        public double plunge = Double.NaN;
        public double vel_plunge = Double.NaN;
        public double pitch = Double.NaN;
        public double vel_pitch = Double.NaN;

        //public PointF[] arrayProfile { get { return _arrayProfile; } set { _arrayProfile = value;} }    

        public BindingList<Line> listUpperLines { get { return _listUpperLines; } set { _listUpperLines = value; OnPropertyChanged("listUpperLines"); } }
        public BindingList<Line> listDownerLines { get { return _listDownerLines; } set { _listDownerLines = value; OnPropertyChanged("listDownerLines"); } }

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
                + "\t" + Math.Round(pitch, GlobalSettings.roundPitch + 1).ToString(CultureInfo.CreateSpecificCulture("en-GB"))
                + "\t" + Math.Round(plunge, GlobalSettings.roundPlunge + 1).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
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

    }
}

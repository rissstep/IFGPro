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
    internal partial class  MainWindow : Form
    {
        private string formName = "IFGPro 0.93beta - ";
        #region Variableeees
        //Project name
        public string projectName;

        
        //List of points for naca profile
        public static List<PointF> nacaProfile;
        //Points for cutten profile (real length)
        public static PointF A;
        public static PointF B;
        //
        public PointF cal_A;
        public PointF cal_B;
        //opening new project
        string path;
        string path_airfoil;
        bool openDialogOk;
        //class for all images
        public ImagesClass images = new ImagesClass();
        //image on screen
        Bitmap image;
        //bw for switching pictures
        public BackgroundWorker bw = new BackgroundWorker();
        //if picture sequence is playing
        private Boolean isPlaying = false;
        //click control
        private float x, y;     
        //calibrate points
        private Mark calibratePoint1;
        private Mark calibratePoint2;
        //Points for moving calibrated profile< Wtf am I talking about
        public static Mark calibrateProfilePoint1;
        public static Mark calibrateProfilePoint2;
        //Calibrate profile
        public static PointF[] arrayProfile = null;
        //Distance between calibrate points
        private double distanceBetweenCalibratePoints = 0;
        //Calculated distance for airfoil
        private double idealLength;
        private double realLength;
        //Cross point
        PointF cross;
        //Selected point/line to move
        private string isSelected = String.Empty;
        Line selectedLine;
        ImagePoint selectedImagePoint;
        ObjectPoint selectedObjectPoint;
        ImagePoint selectedFringeLabels;
        //Zoomlimit
        int zoomLimitMin = 300;
        int zoomLimitMax = 50;
        //mouse clik
        private bool isMouseDown = false;
        //Selectiong lines
        int upLastSelectedIndex = 0;
        int downLastSelectedIndex = 0;
        //CheckBox histogram equalizer
        bool isChecked = false;
        //dgv - kvuli posranymu bindingu
        int indexUp = 0;
        int indexDown = 0;
        //Physical points
        List<ImagePoint> listFixedInImage = new List<ImagePoint>();
        FixedInImage formFixedInImage;
        List<ObjectPoint> listFixedInObject = new List<ObjectPoint>();
        FixedInObject formFixedInObject;
        float offsetMovingLabelX;
        float offsetMovingLabelY;
        //Moving description
        private SizeF offsetMovingDesc;
        //Init
        private bool isInit = false;
        //Level 
        public static int level = 0;
        //real lenght
        static public float PercentRealLenght;
        Image<Gray, Byte> image_for_emgu;
        //graphics width
        static public float graphicsWidth;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            comboBoxScale.SelectedIndex = 0;
            this.Text = formName;
            
            try
            {
                loadSettings();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            

            toolTip1.SetToolTip(this.toNext, "[E]");
            toolTip1.SetToolTip(this.toPrevious, "[Q]");

            splitContainer1.Panel2.Enabled = false;
            flowLayoutPanel7.Enabled = false;
            flowLayoutPanel8.Enabled = false;
            panel2.Enabled = false;
            
            bw.DoWork += new DoWorkEventHandler(bw_LoadImage);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_LoadingCompleted);

            tabControl1.SelectedIndex = 0;

            //calibrate Points
            calibratePoint1 = new Mark(6, Color.Red);
            calibratePoint2 = new Mark(6, Color.Red);
            //calibrate moving profile
            calibrateProfilePoint1 = new Mark(6, Color.Red);
            calibrateProfilePoint2 = new Mark(6, Color.Red);
            
        }

        #region Events of main window

        private void loadImage_Click(object sender, EventArgs e)
        {
            if(images.countImages != 0)
                if (!SureClose("Unsaved data will be lost, do you want to create new project?", "Warning"))
                    return;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                if (openFileDialog.FileName != null)
                {
                    
                    path = openFileDialog.FileName;

                    OpenDialog od = new OpenDialog(Path.GetDirectoryName(path), new DirectoryInfo(Path.GetDirectoryName(path)).Name);
                    od.ShowDialog();

                    if (od.OK == false)
                        return;
                    
                    images = new ImagesClass(Path.GetDirectoryName(path), Path.GetExtension(path));       //vytvoreni tridy vsech obrazku

                    //tb_project_name.Text = od.name;
                    this.Text = formName + od.name;
                    projectName = od.name;
                    //calibrate Points
                    calibratePoint1 = new Mark(6, Color.Red);
                    calibratePoint2 = new Mark(6, Color.Red);
                    //calibrate moving profile
                    calibrateProfilePoint1 = new Mark(6, Color.Red);
                    calibrateProfilePoint2 = new Mark(6, Color.Red);

                    //Physical points
                    listFixedInImage = new List<ImagePoint>();
                    listFixedInObject = new List<ObjectPoint>();
                    
                    arrayProfile = null;

                    flowLayoutPanel7.Enabled = false;
                    flowLayoutPanel8.Enabled = false;
                    panel2.Enabled = false;

                    //panel5.Enabled = true;
                    panelScale.Enabled = true;
                    //panel5.Enabled = true;
                    panelScale.Enabled = true;

                    tabControl1.SelectedIndex = 0;

                    initProgram();
                    #region Clear tb
                    tb_scale.Text = "";
                    tb_real_length.Text = "";
                    tb_ideal_legth.Text = "";

                    tb_dTau.Text = "";
                    tb_heat.Text = "";
                    tb_K.Text = "";
                    tb_L.Text = "";
                    tb_p0.Text = "";
                    tb_R.Text = "";
                    tb_t0.Text = "";
                    tb_tau0.Text = "";
                    tb_w0.Text = "";
                    tb_wave.Text = "";

                    l_min.Text = "";
                    l_max.Text = "";

                    #endregion
                    isInit = true;
                }
            }
        }
        private void initProgram()
        {
            try
            {
                image_for_emgu = new Image<Gray, byte>(images.getActual().path);
            }
            catch(Exception e)
            {
                MessageBox.Show("Problem with EMGU library!!"+e.Message);
                this.Close();
            }

            //get from default folder
            GetAirfoils();
            //open profile
            LoadActualAirfoil();
            if(isInit == false)
                foreach (MyImage im in images.imagesList)
                {
                    im.setPoint1(im.point1.Point);
                }

            OpenImage(image_for_emgu);
            imageBox.ZoomToFit();


            //load settings
            try 
            {
                loadSettings();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            

            //set trackbar maximum and label of count of images
            trackBar1.Maximum = images.getCount();
            trackBar1.Value = images.pointer + 1;
            
            //allFramesLabel.Text = images.getCount().ToString();

            //enable components
            splitContainer1.Panel2.Enabled = true;
            flowLayoutPanel7.Enabled = false;

            imageBox.Enabled = true;
        }
        private void tableLayoutPanel1_SizeChanged(object sender, EventArgs e)
        {
            tabControl1.Refresh();
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (!isPlaying && !bw.IsBusy)
            {
                //OpenImage(new Bitmap(images.getByIndex(trackBar1.Value-1).path));
                bw.RunWorkerAsync(trackBar1.Value - 1);
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            setDataGrids();
            imageBox.Invalidate();
            UpdateObjectFixedPoints();
            if (tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                initDataGrids();
                level = 3;
            }
            else if (tabControl1.SelectedTab.Equals(tabCalibrate))
            {
                level = 0;
            }
            else if (tabControl1.SelectedTab.Equals(tabParameters))
            {
                level = 1;
            }
            
        }
        private void imageBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (imageBox.Zoom > zoomLimitMin)
            {
                if (e.Delta > 0)
                    imageBox.AllowZoom = false;
                else
                    imageBox.AllowZoom = true;
            }
            if (imageBox.Zoom < zoomLimitMax)
            {
                if (e.Delta < 0)
                    imageBox.AllowZoom = false;
                else
                    imageBox.AllowZoom = true;
            }

        }
        private void imageBox_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            //if (!imageBox.IsPointInImage(new Point((int)e.Location.X, (int)e.Location.Y)))
            //    MessageBox.Show("pica");
            selectedFringeLabels = null;
            ImagePoint fringeLabelsPoint = FringeLabelHit(e.Location);
            if (fringeLabelsPoint != null)
            {
                offsetMovingLabelX = imageBox.PointToImage(e.Location).X;
                offsetMovingLabelY = imageBox.PointToImage(e.Location).Y;
                isSelected = "fringeLabels";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                images.getActual().listFringeLabels.Add(fringeLabelsPoint);
                selectedFringeLabels = fringeLabelsPoint;
                return;
            }

            foreach (ImagePoint p in images.getActual().listFringeLabels)
            {
                if (p.IsHitLabel(e.Location, imageBox,true) || p.IsHit(e.Location,imageBox,true))
                {
                    offsetMovingLabelX = imageBox.PointToImage(e.Location).X;
                    offsetMovingLabelY = imageBox.PointToImage(e.Location).Y;
                    isSelected = "fringeLabels";
                    imageBox.AutoPan = false;
                    imageBox.MouseMove += imageBox_MouseMove;
                    selectedFringeLabels = p;
                    imageBox.Invalidate();
                    return;
                }
            }

            if (isSelected.Equals("point1") || isSelected.Equals("point2") || isSelected.Equals("EA"))
            {
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            if (selectedLine != null)
            {
                setDataGrids();
                selectedLine = null;
                isSelected = String.Empty;
            }
            if (!(isSelected == String.Empty))
            {
                isSelected = String.Empty;
            }

            //Test MoveDesc
            if (images.getActual().isDesHit(imageBox.PointToImage(e.Location), imageBox))
            {
                isSelected = "description";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
                offsetMovingDesc = new SizeF(imageBox.PointToImage(e.Location).X-images.getActual().locationDesc.X,
                    imageBox.PointToImage(e.Location).Y - images.getActual().locationDesc.Y);

            }
            // testing all lines if hit
            foreach (Line l in images.getActual().listLines)
            {
                if (l.IsHit(imageBox.PointToImage(e.Location),imageBox))
                {
                    images.getActual().MakeRestorePoint(l);
                    selectedLine = l;
                    isSelected = "line";
                    imageBox.AutoPan = false;
                    imageBox.MouseMove += imageBox_MouseMove;
                    imageBox.Invalidate();
                    if (l.upSide)
                        dataGridUpper.Rows[l.Index - 1].Selected = true;
                    else
                        dataGridDowner.Rows[l.Index - 1].Selected = true;
                    return;
                }
            }

            foreach (ImagePoint p in listFixedInImage)
            {
                if(p.IsHitLabel(imageBox.PointToImage(e.Location),imageBox))
                {
                    offsetMovingLabelX = imageBox.PointToImage(e.Location).X - p.GetLabelLocation().X;
                    offsetMovingLabelY = imageBox.PointToImage(e.Location).Y - p.GetLabelLocation().Y;
                    isSelected = "Ilabel";
                    selectedImagePoint = p;
                    imageBox.AutoPan = false;
                    imageBox.MouseMove += imageBox_MouseMove;
                    imageBox.Invalidate();
                    return;
                }
            }
            foreach (ObjectPoint p in listFixedInObject)
            {
                if (p.IsHitLabel(imageBox.PointToImage(e.Location), imageBox))
                {
                    offsetMovingLabelX = imageBox.PointToImage(e.Location).X - p.GetLabelLocation().X;
                    offsetMovingLabelY = imageBox.PointToImage(e.Location).Y - p.GetLabelLocation().Y ;
                    isSelected = "Olabel";
                    selectedObjectPoint = p;
                    imageBox.AutoPan = false;
                    imageBox.MouseMove += imageBox_MouseMove;
                    imageBox.Invalidate();
                    return;
                }
            }

            //test points front and back point 
            
            if(images.getActual().point1.IsHit(imageBox.PointToImage(e.Location),imageBox))
            {
                isSelected = "point1";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if(images.getActual().point2.IsHit(imageBox.PointToImage(e.Location),imageBox))
            {
                isSelected = "point2";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (GetElasticAxis() != null && tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                if (GetElasticAxis().IsHit(imageBox.PointToImage(e.Location), imageBox))
                {
                    isSelected = "EA";
                    imageBox.AutoPan = false;
                    imageBox.MouseMove += imageBox_MouseMove;
                    imageBox.Invalidate();
                }
            }
            if (calibrateProfilePoint1.IsHit(imageBox.PointToImage(e.Location), imageBox) && !tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                isSelected = "calPoint1";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (calibrateProfilePoint2.IsHit(imageBox.PointToImage(e.Location), imageBox) && !tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                isSelected = "calPoint2";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (IsHitProfile(arrayProfile, new PointF(e.Location.X, e.Location.Y)) && !tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                isSelected = "profile";
                imageBox.AutoPan = false;
                imageBox.MouseMove += imageBox_MouseMove;
                imageBox.Invalidate();
            }

            x = e.Location.X;
            y = e.Location.Y;
        }
        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Application.OpenForms.OfType<FixedInImage>().Any() || Application.OpenForms.OfType<FixedInObject>().Any())
            {
                return;
            }

            if (isSelected.Equals("description"))
            {
                images.getActual().locationDesc.X = imageBox.PointToImage(e.Location).X - offsetMovingDesc.Width;
                images.getActual().locationDesc.Y = imageBox.PointToImage(e.Location).Y - offsetMovingDesc.Height;
                imageBox.Invalidate();
            }
            if (isSelected.Equals("line") && isMouseDown)
            {
                if (selectedLine == null)
                {
                    isSelected = String.Empty;
                    return;
                }
                images.getActual().setLine(selectedLine, imageBox.PointToImage(e.Location));
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("point1") && isMouseDown)
            {
                double distance = GetDistanceBetween(images.getActual().point2.Point, imageBox.PointToImage(e.Location));
                double ratio = distanceBetweenCalibratePoints / distance;
                double vectorX = ((imageBox.PointToImage(e.Location).X - images.getActual().point2.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                double vectorY = ((imageBox.PointToImage(e.Location).Y - images.getActual().point2.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                images.getActual().point1.Point = new PointF((float)(images.getActual().point2.Point.X + vectorX),
                    (float)(images.getActual().point2.Point.Y + vectorY));

                images.getActual().setPoint1(images.getActual().point1.Point);

                UpdateObjectFixedPoints();
                imageBox.Invalidate();
                
            }
            else if (isSelected.Equals("point2") && isMouseDown)
            {
                
                double distance = GetDistanceBetween(images.getActual().point1.Point, imageBox.PointToImage(e.Location));
                double ratio = distanceBetweenCalibratePoints / distance;
                double vectorX = ((imageBox.PointToImage(e.Location).X - images.getActual().point1.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                double vectorY = ((imageBox.PointToImage(e.Location).Y - images.getActual().point1.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                images.getActual().point2.Point = new PointF((float)(images.getActual().point1.Point.X + vectorX),
                    (float)(images.getActual().point1.Point.Y + vectorY));

                images.getActual().setPoint2(images.getActual().point2.Point);

                UpdateObjectFixedPoints();
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("EA") && isMouseDown)
            {
                float diffX = imageBox.PointToImage(x, y).X - imageBox.PointToImage(e.Location).X;
                float diffY = imageBox.PointToImage(x, y).Y - imageBox.PointToImage(e.Location).Y;
                images.getActual().setPoint1(new PointF(images.getActual().point1.Point.X - diffX,
                                            images.getActual().point1.Point.Y - diffY),
                                            true,
                                            diffX,
                                            diffY);

                x = e.Location.X;
                y = e.Location.Y;

                UpdateObjectFixedPoints();
                imageBox.Invalidate();
                UpdateInfo();
            }
            else if (isSelected.Equals("calPoint1"))
            {

                if (!calibrateProfilePoint2.IsEmpty())
                {
                    double distance = GetDistanceBetween(calibrateProfilePoint2.Point, imageBox.PointToImage(e.Location));
                    double ratio = distanceBetweenCalibratePoints / distance;
                    //GlobalSettings.ratio = ratio;
                    double vectorX = ((imageBox.PointToImage(e.Location).X - calibrateProfilePoint2.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                    double vectorY = ((imageBox.PointToImage(e.Location).Y - calibrateProfilePoint2.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                    calibrateProfilePoint1.Point = new PointF((float)(calibrateProfilePoint2.Point.X + vectorX), 
                        (float)(calibrateProfilePoint2.Point.Y + vectorY));

                    //CalculateScale();
                    ComputePointsForRealLength();
                    arrayProfile = profileNaca(calibrateProfilePoint1.Point, calibrateProfilePoint2.Point);
                    UpdateObjectFixedPoints();
                    imageBox.Invalidate();
                }
                else
                {
                    calibrateProfilePoint1.Point = imageBox.PointToImage(e.Location);
                }
            }
            else if (isSelected.Equals("calPoint2"))
            {
                if (!calibrateProfilePoint1.IsEmpty())
                {
                    double distance = GetDistanceBetween(calibrateProfilePoint1.Point, imageBox.PointToImage(e.Location));
                    double ratio = distanceBetweenCalibratePoints / distance;
                    double vectorX = ((imageBox.PointToImage(e.Location).X - calibrateProfilePoint1.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                    double vectorY = ((imageBox.PointToImage(e.Location).Y - calibrateProfilePoint1.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                    calibrateProfilePoint2.Point = new PointF((float)(calibrateProfilePoint1.Point.X + vectorX), 
                        (float)(calibrateProfilePoint1.Point.Y + vectorY));

                    //CalculateScale();
                    ComputePointsForRealLength();
                    arrayProfile = profileNaca(calibrateProfilePoint1.Point, calibrateProfilePoint2.Point);
                    UpdateObjectFixedPoints();
                    imageBox.Invalidate();
                }
                else
                {
                    calibrateProfilePoint2.Point = imageBox.PointToImage(e.Location);
                }
            }
            else if (isSelected.Equals("profile"))
            {
                MoveProfile((float)(imageBox.PointToImage(x, y).X - imageBox.PointToImage(e.Location).X), 
                    (float)(imageBox.PointToImage(x, y).Y - imageBox.PointToImage(e.Location).Y));
                calibrateProfilePoint1.setPointDifference((float)(imageBox.PointToImage(x, y).X - imageBox.PointToImage(e.Location).X), 
                    (float)(imageBox.PointToImage(x, y).Y - imageBox.PointToImage(e.Location).Y));
                calibrateProfilePoint2.setPointDifference((float)(imageBox.PointToImage(x, y).X - imageBox.PointToImage(e.Location).X), 
                    (float)(imageBox.PointToImage(x, y).Y - imageBox.PointToImage(e.Location).Y));
                x = e.Location.X;
                y = e.Location.Y;

                UpdateObjectFixedPoints();

                imageBox.Invalidate();
            }
            else if(isSelected.Equals("Ilabel"))
            {
                selectedImagePoint.labelOffsetX = imageBox.PointToImage(e.Location).X - selectedImagePoint.location.X - offsetMovingLabelX;
                selectedImagePoint.labelOffsetY = imageBox.PointToImage(e.Location).Y - selectedImagePoint.location.Y - offsetMovingLabelY;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("Olabel"))
            {
                selectedObjectPoint.labelOffsetX = imageBox.PointToImage(e.Location).X - selectedObjectPoint.location.X - offsetMovingLabelX;
                selectedObjectPoint.labelOffsetY = imageBox.PointToImage(e.Location).Y - selectedObjectPoint.location.Y - offsetMovingLabelY;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("fringeLabels"))
            {
                //images.getActual().listFringeLabels.
                selectedFringeLabels.location.X += imageBox.PointToImage(e.Location).X - offsetMovingLabelX;
                selectedFringeLabels.location.Y += imageBox.PointToImage(e.Location).Y - offsetMovingLabelY;
                offsetMovingLabelX = imageBox.PointToImage(e.Location).X;
                offsetMovingLabelY = imageBox.PointToImage(e.Location).Y;
                if (!imageBox.IsPointInImage(e.Location))
                    selectedFringeLabels.exist = false;
                else
                {
                    selectedFringeLabels.exist = true;
                    foreach (Line l in images.getActual().listLines)
                    {
                        if (l.IsHit(imageBox.PointToImage(e.Location), imageBox))
                        {
                            selectedFringeLabels.exist = false;
                        }
                    }
                }
                    
                
                imageBox.Invalidate();
            }
        }
        private void imageBox_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            if (isSelected.Equals("line") || isSelected.Equals("dataGrid"))
            {
                if (selectedLine != null)
                    selectedLine.Sur_Coor = images.getActual().GetSurfaceCoor(selectedLine);
                //selectedLine = null;
                //isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                //setDataGrids();
                imageBox.Invalidate();

                dataGridUpper.Update();
                dataGridDowner.Update();
            }
            else if (isSelected.Equals("EA") || isSelected.Equals("point1") || isSelected.Equals("point2"))
            {
                //isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                setDataGrids();
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("description"))
            {
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("calPoint1") || isSelected.Equals("calPoint2"))
            {
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("profile"))
            {
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("Ilabel"))
            {
                selectedImagePoint = null;
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("Olabel"))
            {
                selectedObjectPoint = null;
                isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
            }
            else if (isSelected.Equals("fringeLabels"))
            {

                foreach (Line l in images.getActual().listLines)
                {
                    if (l.IsHit(imageBox.PointToImage(e.Location), imageBox))
                    {
                        l.F_Index = selectedFringeLabels.label;
                        images.getActual().listFringeLabels.Remove(selectedFringeLabels);
                    }
                }
                //selectedFringeLabels = null;
                //isSelected = String.Empty;
                imageBox.AutoPan = true;
                imageBox.MouseMove -= imageBox_MouseMove;
                imageBox.Invalidate();
                setDataGrids();
                return;
            }
            else if (x == e.Location.X && y == e.Location.Y)
            {
                if (Application.OpenForms.OfType<FixedInImage>().Any())
                {
                    formFixedInImage.setPointLocation(imageBox.PointToImage(e.Location));
                }
                if (Application.OpenForms.OfType<FixedInObject>().Any())
                {
                    if (tabControl1.SelectedTab.Equals(tabAirfoil))
                        formFixedInObject.setPointLocation(imageBox.PointToImage(e.Location),
                            calibrateProfilePoint1.Point,
                            calibrateProfilePoint2.Point);
                    else if (tabControl1.SelectedTab.Equals(tabAnalyze))
                    {
                        formFixedInObject.setPointLocation(imageBox.PointToImage(e.Location),
                            images.getActual().point1.Point,
                            images.getActual().point2.Point);
                    }
                }
                cross = imageBox.PointToImage(e.Location);

                if (!isPlaying)
                    imageBox.Invalidate();

                UpdateStatusBar();
            }
            UpdateInfo();
        }
        private void imageBox_DoubleClick(object sender, EventArgs e)
        {
            var _e = (MouseEventArgs)e;
            if (!Application.OpenForms.OfType<FixedInImage>().Any())
            {
                foreach (ImagePoint p in listFixedInImage)
                {
                    if (p.IsHit(imageBox.PointToImage(_e.Location), imageBox) || p.IsHitLabel(imageBox.PointToImage(_e.Location), imageBox))
                    {
                        formFixedInImage = new FixedInImage(this, p);
                        formFixedInImage.Show();
                        break;
                    }
                }
            }
            if (!Application.OpenForms.OfType<FixedInObject>().Any())
            {
                foreach (ObjectPoint p in listFixedInObject)
                {
                    if (p.IsHit(imageBox.PointToImage(_e.Location), imageBox) || p.IsHitLabel(imageBox.PointToImage(_e.Location), imageBox))
                    {
                        formFixedInObject = new FixedInObject(this, p);
                        formFixedInObject.Show();
                        break;
                    }
                }
            }

            imageBox.Invalidate();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (dataGridUpper.IsCurrentCellInEditMode || dataGridDowner.IsCurrentCellInEditMode)
            {
                if (dataGridUpper.IsCurrentCellInEditMode && keyData == Keys.Enter)
                {
                    if ((dataGridUpper.RowCount - 1) == dataGridUpper.SelectedRows[0].Index)
                    {
                        DataGridViewCell cell = dataGridUpper.Rows[0].Cells[3];
                        dataGridUpper.CurrentCell = cell;
                        dataGridUpper.BeginEdit(true);
                    }
                    else
                    {
                        DataGridViewCell cell = dataGridUpper.Rows[dataGridUpper.SelectedRows[0].Index + 1].Cells[3];
                        dataGridUpper.CurrentCell = cell;
                        dataGridUpper.BeginEdit(true);
                    }
                    return true;
                }

                if (dataGridDowner.IsCurrentCellInEditMode && keyData == Keys.Enter)
                {
                    if ((dataGridDowner.RowCount - 1) == dataGridDowner.SelectedRows[0].Index)
                    {
                        DataGridViewCell cell = dataGridDowner.Rows[0].Cells[3];
                        dataGridDowner.CurrentCell = cell;
                        dataGridDowner.BeginEdit(true);
                    }
                    else
                    {
                        DataGridViewCell cell = dataGridDowner.Rows[dataGridDowner.SelectedRows[0].Index + 1].Cells[3];
                        dataGridDowner.CurrentCell = cell;
                        dataGridDowner.BeginEdit(true);
                    }
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if(tb_description.Focused)
                return base.ProcessCmdKey(ref msg, keyData);
            
            if (images != null)
            {
                if (keyData == (Keys.Control | Keys.Z))
                {
                    try
                    {
                        //images.getActual().UnDo();
                        imageBox.Invalidate();
                        setDataGrids();
                    }
                    catch { }
                    return true;
                }
                if (tabControl1.SelectedTab.Equals(tabCalibrate) || tabControl1.SelectedTab.Equals(tabAnalyze))
                {
                    if (tabControl1.SelectedTab.Equals(tabCalibrate) && (panel2.Enabled || flowLayoutPanel8.Enabled))
                        return base.ProcessCmdKey(ref msg, keyData);

                    if (keyData == (Keys.A))
                    {
                        setPoint1(cross);
                    }
                    else if (keyData == Keys.D)
                    {
                        setPoint2(cross);
                    }
                    else if (tabControl1.SelectedTab.Equals(tabAnalyze) && keyData == Keys.S && level == 3 && !images.getActual().point1.IsEmpty() && !images.getActual().point2.IsEmpty())
                    {
                        images.getActual().AddLine(cross);
                        imageBox.Invalidate();
                        setDataGrids();
                    }
                }
            }
            if(keyData == Keys.Alt && keyData == Keys.F4)
            {
                this.Close();
            }
            if (keyData == Keys.Control && keyData == Keys.S)
            {
                if (level == 3)
                    SaveProject();
            }
            if (tabControl1.SelectedTab.Equals(tabAnalyze) && keyData == Keys.Delete && selectedLine != null)
            {
                if (dataGridUpper.SelectedRows.Count > 0)
                {
                    int i = dataGridUpper.SelectedRows[0].Index;
                    //dataGridUpper.DataSource = null;
                    images.getActual().listUpperLines[i].MakeRestorePoint();
                    images.getActual().listLines.Remove(images.getActual().listUpperLines[i]);
                    images.getActual().listUpperLines.RemoveAt(i);
                    images.getActual().sortLines();
                    setDataGrids();

                    isSelected = String.Empty; ;
                }
                if (dataGridDowner.SelectedRows.Count > 0)
                {
                    int i = dataGridDowner.SelectedRows[0].Index;
                    //dataGridDowner.DataSource = null;
                    images.getActual().listDownerLines[i].MakeRestorePoint();
                    images.getActual().listLines.Remove(images.getActual().listDownerLines[i]);
                    images.getActual().listDownerLines.RemoveAt(i);
                    images.getActual().sortLines();
                    setDataGrids();

                    isSelected = String.Empty; ;
                }
            }
            if (tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                if (keyData == Keys.E)
                {
                    OpenImage(new Image<Gray, Byte>(images.getNext().path));
                    trackBar1.Value = images.getPosition();
                    UpdateObjectFixedPoints();
                    UpdateStatusBar();
                    UpdateInfo();
                    setDataGrids();
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                if (keyData == Keys.Q)
                {
                    OpenImage(new Image<Gray, Byte>(images.getPrevious().path));
                    trackBar1.Value = images.getPosition();
                    UpdateObjectFixedPoints();
                    UpdateStatusBar();
                    UpdateInfo();
                    setDataGrids();
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                if (isSelected.Equals("point1"))
                {
                    if (keyData == Keys.Up || keyData == Keys.Down)
                    {
                        float sin;
                        float cos;
                        if (keyData == Keys.Up)
                        {
                            sin = (float)Math.Sin(-0.001);
                            cos = (float)Math.Cos(-0.001);
                        }
                        else
                        {
                            sin = (float)Math.Sin(0.001);
                            cos = (float)Math.Cos(0.001);
                        }

                        float xTmp = images.getActual().point1.Point.X - images.getActual().point2.Point.X; //rotating
                        float yTmp = images.getActual().point1.Point.Y - images.getActual().point2.Point.Y;

                        xTmp = xTmp * cos + yTmp * sin; //rotating
                        yTmp = -xTmp * sin + yTmp * cos;

                        images.getActual().setPoint1(new PointF(xTmp + images.getActual().point2.Point.X, yTmp + images.getActual().point2.Point.Y));
                        
                        msg.WParam = IntPtr.Zero;

                        UpdateObjectFixedPoints();
                        imageBox.Invalidate();
                        UpdateInfo();
                    }
                }
                else if (isSelected.Equals("point2"))
                {
                    if (keyData == Keys.Up || keyData == Keys.Down)
                    {
                        float sin;
                        float cos;
                        if (keyData == Keys.Up)
                        {
                            sin = (float)Math.Sin(0.001);
                            cos = (float)Math.Cos(0.001);
                        }
                        else
                        {
                            sin = (float)Math.Sin(-0.001);
                            cos = (float)Math.Cos(-0.001);
                        }

                        float xTmp = images.getActual().point2.Point.X - images.getActual().point1.Point.X; //rotating
                        float yTmp = images.getActual().point2.Point.Y - images.getActual().point1.Point.Y;

                        xTmp = xTmp * cos + yTmp * sin; //rotating
                        yTmp = -xTmp * sin + yTmp * cos;

                        images.getActual().setPoint2(new PointF(xTmp + images.getActual().point1.Point.X, yTmp + images.getActual().point1.Point.Y));

                        msg.WParam = IntPtr.Zero;

                        UpdateObjectFixedPoints();
                        imageBox.Invalidate();
                        UpdateInfo();
                    }
                }
                else if (isSelected.Equals("EA"))
                {
                    if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
                    {
                        float diffX =0;
                        float diffY =0;
                        if (keyData == Keys.Down)
                        {
                            diffX = 0;
                            diffY = -1;
                        }
                        else if (keyData == Keys.Up)
                        {
                            diffX = 0;
                            diffY = 1;
                        }
                        else if (keyData == Keys.Right)
                        {
                            diffX = -1;
                            diffY = 0;
                        }
                        else if (keyData == Keys.Left)
                        {
                            diffX = 1;
                            diffY = 0;
                        }

                        images.getActual().setPoint1(new PointF(images.getActual().point1.Point.X - diffX,
                                                    images.getActual().point1.Point.Y - diffY),
                                                    true,
                                                    diffX,
                                                    diffY);

                        msg.WParam = IntPtr.Zero;
                        UpdateObjectFixedPoints();
                        imageBox.Invalidate();
                        UpdateInfo();
                    }
                }
                    
            }
            if(keyData == Keys.Delete && isSelected.Equals("fringeLabels"))
            {
                images.getActual().listFringeLabels.Remove(selectedFringeLabels);
                imageBox.Invalidate();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void imageBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Keys keyData = (Keys)char.ToUpper(e.KeyChar);

            //if (images != null)
            //{
            //    if (tabControl1.SelectedTab.Equals(tabCalibrate) || tabControl1.SelectedTab.Equals(tabAnalyze))
            //    {
            //        if (keyData == (Keys.A))
            //        {
            //            setPoint1(cross);
            //        }
            //        else if (keyData == Keys.D)
            //        {
            //            setPoint2(cross);
            //        }
            //        else if (tabControl1.SelectedTab.Equals(tabAnalyze) && keyData == Keys.S)
            //        {
            //            images.getActual().AddLine(cross);
            //            imageBox.Invalidate();
            //            setDataGrids();
            //        }
            //    }
            //}
            //if (tabControl1.SelectedTab.Equals(tabAnalyze) && keyData == Keys.Delete && selectedLine != null)
            //{
            //    if (dataGridUpper.SelectedRows.Count > 0)
            //    {
            //        int i = dataGridUpper.SelectedRows[0].Index;
            //        //dataGridUpper.DataSource = null;
            //        images.getActual().listUpperLines[i].MakeRestorePoint();
            //        images.getActual().listLines.Remove(images.getActual().listUpperLines[i]);
            //        images.getActual().listUpperLines.RemoveAt(i);
            //        images.getActual().sortLines();
            //        setDataGrids();

            //        isSelected = String.Empty; ;
            //    }
            //    if (dataGridDowner.SelectedRows.Count > 0)
            //    {
            //        int i = dataGridDowner.SelectedRows[0].Index;
            //        //dataGridDowner.DataSource = null;
            //        images.getActual().listDownerLines[i].MakeRestorePoint();
            //        images.getActual().listLines.Remove(images.getActual().listDownerLines[i]);
            //        images.getActual().listDownerLines.RemoveAt(i);
            //        images.getActual().sortLines();
            //        setDataGrids();

            //        isSelected = String.Empty; ;
            //    }
            //}

            //if (tabControl1.SelectedTab.Equals(tabAnalyze))
            //{
            //    if (isSelected.Equals("point1"))
            //    {
            //        if (keyData == Keys.Up || keyData == Keys.Down)
            //        {
            //            float sin;
            //            float cos;
            //            if (keyData == Keys.Up)
            //            {
            //                sin = (float)Math.Sin(-0.001);
            //                cos = (float)Math.Cos(-0.001);
            //            }
            //            else
            //            {
            //                sin = (float)Math.Sin(0.001);
            //                cos = (float)Math.Cos(0.001);
            //            }

            //            float xTmp = images.getActual().point1.Point.X - images.getActual().point2.Point.X; //rotating
            //            float yTmp = images.getActual().point1.Point.Y - images.getActual().point2.Point.Y;

            //            xTmp = xTmp * cos + yTmp * sin; //rotating
            //            yTmp = -xTmp * sin + yTmp * cos;

            //            images.getActual().setPoint1(new PointF(xTmp + images.getActual().point2.Point.X, yTmp + images.getActual().point2.Point.Y));

            //            msg.WParam = IntPtr.Zero;

            //            UpdateObjectFixedPoints();
            //            imageBox.Invalidate();
            //            UpdateInfo();
            //        }
            //    }
            //    else if (isSelected.Equals("point2"))
            //    {
            //        if (keyData == Keys.Up || keyData == Keys.Down)
            //        {
            //            float sin;
            //            float cos;
            //            if (keyData == Keys.Up)
            //            {
            //                sin = (float)Math.Sin(0.001);
            //                cos = (float)Math.Cos(0.001);
            //            }
            //            else
            //            {
            //                sin = (float)Math.Sin(-0.001);
            //                cos = (float)Math.Cos(-0.001);
            //            }

            //            float xTmp = images.getActual().point2.Point.X - images.getActual().point1.Point.X; //rotating
            //            float yTmp = images.getActual().point2.Point.Y - images.getActual().point1.Point.Y;

            //            xTmp = xTmp * cos + yTmp * sin; //rotating
            //            yTmp = -xTmp * sin + yTmp * cos;

            //            images.getActual().setPoint2(new PointF(xTmp + images.getActual().point1.Point.X, yTmp + images.getActual().point1.Point.Y));

            //            msg.WParam = IntPtr.Zero;

            //            UpdateObjectFixedPoints();
            //            imageBox.Invalidate();
            //            UpdateInfo();
            //        }
            //    }
            //    else if (isSelected.Equals("EA"))
            //    {
            //        if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            //        {
            //            float diffX = 0;
            //            float diffY = 0;
            //            if (keyData == Keys.Down)
            //            {
            //                diffX = 0;
            //                diffY = -1;
            //            }
            //            else if (keyData == Keys.Up)
            //            {
            //                diffX = 0;
            //                diffY = 1;
            //            }
            //            else if (keyData == Keys.Right)
            //            {
            //                diffX = -1;
            //                diffY = 0;
            //            }
            //            else if (keyData == Keys.Left)
            //            {
            //                diffX = 1;
            //                diffY = 0;
            //            }

            //            images.getActual().setPoint1(new PointF(images.getActual().point1.Point.X - diffX,
            //                                        images.getActual().point1.Point.Y - diffY),
            //                                        true,
            //                                        diffX,
            //                                        diffY);

            //            msg.WParam = IntPtr.Zero;
            //            UpdateObjectFixedPoints();
            //            imageBox.Invalidate();
            //            UpdateInfo();
            //        }
            //    }

            //}
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings s = new Settings(ref imageBox);
            s.ShowDialog();
            if (s.apply)
            {
                saveSettings();
                if(images.imagesList != null)
                    foreach (Line l in images.getActual().listLines)
                    {
                        images.getActual().setLine(l, l.pointOfProfile.Point);
                    }

                imageBox.Invalidate();
            }
        }
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export();
        }
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadProject();
            }
            catch(Exception ex) 
            {
                MessageBox.Show("File you want to load is not compatible with version of IFGPro you are using.\n"+ex.Message);
            }
            
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (level == 3)
                if (!SureClose("Unsaved data will be lost, do you really want to exit IFGPro?", "Warning"))
                    e.Cancel = true;
            saveSettings();
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (images != null)
            {
                try
                {
                    images.getActual().UnDo();
                    imageBox.Invalidate();
                    setDataGrids();
                }
                catch { }
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion 
        
        #region Redrawing imagebox
        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            if (imageBox.Image != null)
            {
                
                imageBox.SuspendLayout();
                graphicsWidth = e.Graphics.ClipBounds.Width;
                if (!isPlaying)
                {
                    if (images.imagesList != null)
                        foreach (Line l in images.getActual().listLines)
                        {
                            images.getActual().setLine(l, l.pointOfProfile.Point);
                        }
                    DrawGraphics(e.Graphics);
                }

                imageBox.ResumeLayout();
                
                if (!isPlaying)
                {
                    UpdateStatusBar();
                }

            }
        }
        private void DrawGraphics(Graphics e)
        {

            GlobalSettings.profilPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
            e.SmoothingMode = SmoothingMode.AntiAlias;
            //e.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            //Cross 
            if (isSelected.Equals(String.Empty) && !isMouseDown)
            {
                float crossY = imageBox.GetOffsetPoint(cross).Y;
                float crossX = imageBox.GetOffsetPoint(cross).X;
                e.DrawLine(GlobalSettings.crossPen, new PointF(0, crossY), new PointF(imageBox.Width, crossY));
                e.DrawLine(GlobalSettings.crossPen, new PointF(crossX, 0), new PointF(crossX, imageBox.Height));
            }

            if (GlobalSettings.fringeLabels)
            {
                foreach (ImagePoint p in images.getActual().listFringeLabels)
                {

                    if (!p.exist)
                        continue;

                    if (p.Equals(selectedFringeLabels))
                    {
                        p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location), true, true);
                        e.DrawString(p.label, GlobalSettings.fringeLabelsFont, new SolidBrush(GlobalSettings.selectedPen.Color),
                                        p.GetLabelLocation(imageBox));
                    }else
                    {
                        p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location), true);
                        e.DrawString(p.label, GlobalSettings.fringeLabelsFont, new SolidBrush(GlobalSettings.fringeLabelsColor),
                                        p.GetLabelLocation(imageBox));
                    }
                    
                }
            }


            if (tabControl1.SelectedTab.Equals(tabCalibrate))
            {
                if (!calibratePoint1.IsEmpty())
                {
                    calibratePoint1.DrawToGraphics(e, imageBox.GetOffsetPoint(calibratePoint1.Point));
                }
                if (!calibratePoint2.IsEmpty())
                {
                    calibratePoint2.DrawToGraphics(e, imageBox.GetOffsetPoint(calibratePoint2.Point));
                }
            }
            //Calibration
            if (tabControl1.SelectedTab.Equals(tabAirfoil) || tabControl1.SelectedTab.Equals(tabParameters) || tabControl1.SelectedTab.Equals(tabCalibrate))   
            {
                //Point fixed in image
                if (listFixedInImage.Count > 0)
                {
                    //double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size / 100;
                    //if (tmp <= 1)
                    //    tmp = 1;
                    //Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp));
                    Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, GlobalSettings.fontPoints.Size);
                    foreach (ImagePoint p in listFixedInImage)
                    {
                        p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location));
                        p.sizeString = e.MeasureString(p.label, fontPoints);
                        e.DrawString(p.label, fontPoints, p.pen.Brush, p.GetLabelLocation(imageBox));

                    }
                }

                //Point fixed in object
                if (listFixedInObject.Count > 0)
                {
                    //double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size / 100;
                    //if (tmp <= 1)
                    //    tmp = 1;
                    //Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp));
                    Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, GlobalSettings.fontPoints.Size);
                    foreach (ObjectPoint p in listFixedInObject)
                    {
                        p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location));
                        p.sizeString = e.MeasureString(p.label, fontPoints);
                        e.DrawString(p.label, fontPoints, p.pen.Brush, p.GetLabelLocation(imageBox));

                    }
                }

                

                if (!(arrayProfile == null))
                {
                    e.DrawLines(GlobalSettings.profilPen, offsetPoints(MainWindow.arrayProfile));
                    //e.DrawLine(GlobalSettings.profilPen, imageBox.GetOffsetPoint(cal_A), imageBox.GetOffsetPoint(cal_B));

                    calibrateProfilePoint1.DrawToGraphics(e, imageBox.GetOffsetPoint(calibrateProfilePoint1.Point));
                    calibrateProfilePoint1.DrawToGraphics(e, imageBox.GetOffsetPoint(calibrateProfilePoint2.Point));
                }

            }
            //Analyze tab
            else if (tabControl1.SelectedTab.Equals(tabAnalyze))
            {
                
                //Image description
                if (GlobalSettings.desc)
                {
                    //double tmpD = imageBox.Zoom * GlobalSettings.fontDescription.Size / 100;
                    //if (tmpD <= 0)
                    //    tmpD = 1;
                    //Font fontDesc = new Font(GlobalSettings.fontDescription.FontFamily, (int)(tmpD));
                    Font fontDesc = new Font(GlobalSettings.fontDescription.FontFamily, GlobalSettings.fontDescription.Size);
                    images.getActual().sizeString = e.MeasureString(images.getActual().description, fontDesc);
                    e.DrawString(images.getActual().description, fontDesc,
                        GlobalSettings.descriptionBrush.Brush,
                        imageBox.GetOffsetPoint(images.getActual().locationDesc));
                }

                //Point fixed in image
                if (listFixedInImage.Count > 0)
                {
                    //double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size/100;
                    //if (tmp <= 0)
                    //    tmp = 1;
                    //Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp));
                    Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, GlobalSettings.fontPoints.Size);
                    foreach (ImagePoint p in listFixedInImage)
                    {
                        if (GlobalSettings.points)
                        {
                            p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location));
                        }
                        p.sizeString = e.MeasureString(p.label, fontPoints);
                        if (GlobalSettings.pointsDesc)
                        {
                            e.DrawString(p.label, fontPoints, p.pen.Brush, p.GetLabelLocation(imageBox));
                        }

                    }
                }

                if(!images.getActual().point1.IsEmpty())
                {
                    if (isSelected.Equals("point1"))
                    {
                        e.DrawEllipse(GlobalSettings.selectedPen, 
                            imageBox.GetOffsetPoint(images.getActual().point1.Point).X-4, 
                            imageBox.GetOffsetPoint(images.getActual().point1.Point).Y-4, 
                            8, 
                            8);
                    }
                    images.getActual().point1.DrawToGraphics(e, imageBox.GetOffsetPoint(images.getActual().point1.Point));
                }
                if (!images.getActual().point2.IsEmpty())
                {
                    if (isSelected.Equals("point2"))
                    {
                        e.DrawEllipse(GlobalSettings.selectedPen, 
                            imageBox.GetOffsetPoint(images.getActual().point2.Point).X-4, 
                            imageBox.GetOffsetPoint(images.getActual().point2.Point).Y-4, 
                            8, 
                            8);
                    }
                    images.getActual().point2.DrawToGraphics(e, imageBox.GetOffsetPoint(images.getActual().point2.Point));
                }
                if (!images.getActual().point2.IsEmpty() && !images.getActual().point1.IsEmpty())
                {
                    if (GlobalSettings.profil)
                    {
                        if (isSelected.Equals("EA"))
                        {
                            e.DrawLines(GlobalSettings.selectedPen, offsetPoints(images.getActual().getArrayProfile()));
                        }
                        else
                        {
                            e.DrawLines(GlobalSettings.profilPen, offsetPoints(images.getActual().getArrayProfile()));
                        }
                    }

                    //Point fixed in object
                    if (listFixedInObject.Count > 0)
                    {
                        //double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size / 100;
                        //if (tmp < 1)
                        //    tmp = 1;
                        //Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp));
                        Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, GlobalSettings.fontPoints.Size);
                        foreach (ObjectPoint p in listFixedInObject)
                        {
                            if (GlobalSettings.points)
                            {
                                p.DrawToGraphics(e, imageBox.GetOffsetPoint(p.location));
                            }
                            p.sizeString = e.MeasureString(p.label, fontPoints);
                            if (GlobalSettings.pointsDesc)
                            {
                                e.DrawString(p.label, fontPoints, p.pen.Brush, p.GetLabelLocation(imageBox));
                            }

                        }
                    }
                }
                if (!(images.getActual().listLines.Count == 0) )
                {
                    foreach (Line l in images.getActual().listLines)
                    {
                        if (GlobalSettings.lines)
                        {
                            if (l.Equals(selectedLine))
                                e.DrawLine(GlobalSettings.selectedPen,
                                    imageBox.GetOffsetPoint(l.pointOfProfile.Point),
                                    imageBox.GetOffsetPoint(l.pointOnProfile.Point));
                            else
                                e.DrawLine(GlobalSettings.linesPen,
                                    imageBox.GetOffsetPoint(l.pointOfProfile.Point),
                                    imageBox.GetOffsetPoint(l.pointOnProfile.Point));
                        }

                        if (GlobalSettings.linesDesc && GlobalSettings.lineFringeNumber )
                        {
                            if (String.IsNullOrWhiteSpace(l.F_Index))
                                e.DrawString(l.Index.ToString(), GlobalSettings.fontLines, GlobalSettings.indexBrush.Brush, l.LocationIndex(imageBox, e));
                            else
                                e.DrawString(l.Index.ToString() + "/" + l.F_Index, GlobalSettings.fontLines, GlobalSettings.indexBrush.Brush, l.LocationIndex(imageBox,e));
                        }
                        else if (GlobalSettings.linesDesc && !GlobalSettings.lineFringeNumber)
                        {
                            e.DrawString(l.Index.ToString(), GlobalSettings.fontLines, GlobalSettings.indexBrush.Brush, l.LocationIndex(imageBox, e));
                        }
                        else if (!GlobalSettings.linesDesc && GlobalSettings.lineFringeNumber)
                        {
                            if (!String.IsNullOrWhiteSpace(l.F_Index))
                                e.DrawString("/" + l.F_Index, GlobalSettings.fontLines, GlobalSettings.indexBrush.Brush, l.LocationIndex(imageBox, e));
                        }
                            
                    }
                }
            }

            //Fringe labes panel
            if (GlobalSettings.fringeLabelsPanel)
            {

                List<float> delkaRadku = new List<float>();
                delkaRadku.Add(0);
                int pocetRadku = 0;
                for (int i = 0; (GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep) <= GlobalSettings.fringeLabelsTo; ++i)
                {

                    if (delkaRadku[pocetRadku] + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width > e.ClipBounds.Width)
                    {
                        pocetRadku++;
                        delkaRadku.Add(0);
                    }

                    delkaRadku[pocetRadku] += (3 + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width);
                }

                e.FillRectangle(Brushes.Gray,
                    new Rectangle(-1, -1,
                        (int)e.ClipBounds.Width - 0,
                        (int)(delkaRadku.Count * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f) + 2)));

                delkaRadku = new List<float>();
                delkaRadku.Add(0);
                pocetRadku = 0;
                e.DrawLine(new Pen(Color.Black, 1), 0, (delkaRadku.Count * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)), (int)e.ClipBounds.Width - 1, (delkaRadku.Count * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)));
                for (int i = 0; (GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep) <= GlobalSettings.fringeLabelsTo; ++i)
                {

                    if (delkaRadku[pocetRadku] + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width > e.ClipBounds.Width)
                    {
                        pocetRadku++;
                        delkaRadku.Add(0);
                        e.DrawLine(new Pen(Color.Black, 1),
                            0, (delkaRadku.Count * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)),
                            e.ClipBounds.Width - 1, (delkaRadku.Count * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)));
                    }

                    e.DrawString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(),
                        GlobalSettings.fringeLabelsFont,
                        new SolidBrush(GlobalSettings.fringeLabelsColor),
                        new PointF(delkaRadku[pocetRadku] + 3, 1 + (1.5f + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Height) * pocetRadku)
                        );

                    delkaRadku[pocetRadku] += (3 + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width);
                    e.DrawLine(new Pen(Color.Black, 1),
                            delkaRadku[pocetRadku] + 1.5f, (pocetRadku * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)),
                            delkaRadku[pocetRadku] + 1.5f, ((pocetRadku + 1) * (e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height + 1.5f)));

                }

            }
        }
        #endregion

        #region Events of AIRFOIL tab
            //Clicing for calibration
            
            private void tb_ideal_legth_KeyPress(object sender, KeyPressEventArgs e)
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
            private void tb_ideal_legth_Leave(object sender, EventArgs e)
            {
                if (String.IsNullOrWhiteSpace(tb_ideal_legth.Text))
                    tb_ideal_legth.Text = "0";
                CalculateScale();
            }
            private void tb_real_length_KeyPress(object sender, KeyPressEventArgs e)
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
            private void tb_real_length_Leave(object sender, EventArgs e)
            {
                if (String.IsNullOrWhiteSpace(tb_real_length.Text))
                    tb_real_length.Text = "0";

                if (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) > 100)
                    tb_real_length.Text = "100";

                PercentRealLenght = float.Parse(tb_real_length.Text.Replace(".", ","));
                CalculateScale();
            }
            private void tb_real_length_TextChanged(object sender, EventArgs e)
            {
                //CalculateScale();
            }
            
            private void CalculateScale()
            {
                if( !calibratePoint1.IsEmpty() && !calibratePoint2.IsEmpty()){
                    textBoxCalibrate.Text = Math.Round(distanceBetweenCalibratePoints,5).ToString();

                    if (!(tb_scale.Text == String.Empty))
                    {
                        GlobalSettings.ratio = distanceBetweenCalibratePoints / Double.Parse(tb_scale.Text.Replace(".",","));
                    }
                    else
                    {
                        GlobalSettings.ratio = Double.NaN;
                    }
                    if (!(tb_ideal_legth.Text == String.Empty) && !(tb_real_length.Text == String.Empty) && !(tb_scale.Text == String.Empty))
                    {
                        double tmp;
                        //distanceBetweenCalibratePoints = (distanceBetweenCalibratePoints/95)*100;
                        tmp = Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture);
                        tmp = distanceBetweenCalibratePoints / tmp;
                        
                        idealLength = Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) * tmp;
                        realLength = (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) * idealLength) / 100;

                        if (Double.IsNaN(idealLength))
                            return;

                        calibrateProfilePoint1.setPoint((float)((imageBox.Image.Size.Width / 2) - (realLength / 2)),
                            (imageBox.Image.Size.Height / 2));
                        calibrateProfilePoint2.setPoint((float)((imageBox.Image.Size.Width / 2) + (realLength / 2)),
                            (imageBox.Image.Size.Height / 2));

                        ComputePointsForRealLength();
                        arrayProfile = profileNaca(calibrateProfilePoint1.Point, calibrateProfilePoint2.Point);

                        

                        imageBox.Invalidate();
                    }
                    else
                    {
                        arrayProfile = null;
                        idealLength = Double.NaN;
                        realLength = Double.NaN; 
                        imageBox.Invalidate();
                    }
                }
            }
            private void GetAirfoils()
            {
                string path = @"\airfoils";
                string execudetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string[] filesArray;
                List<string> txtArray = new List<string>();

                if (!Directory.Exists(execudetPath + path))
                {
                    Directory.CreateDirectory(execudetPath + path);
                }
                else
                {
                    filesArray = Directory.GetFiles(execudetPath + path);                 //loading all files with same format like selected one
                    foreach (string fileName in filesArray)
                    {
                        if (fileName.EndsWith(".txt"))
                        {
                            txtArray.Add(Path.GetFileNameWithoutExtension(fileName));
                        }
                    }
                    cb_airfoils.DataSource = txtArray;
                }
            }
            private void LoadActualAirfoil(bool forever = false)
            {
                if (cb_airfoils.SelectedItem == null)
                {
                    MessageBox.Show("There are no airfoils avaliable. Please load one!");
                    nacaProfile = null;
                    return;
                }
                string sourceFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\airfoils\\" + cb_airfoils.SelectedItem.ToString() + ".txt";
                if (!forever)
                {
                    nacaProfile = LoadNacaProfile(sourceFile);
                }else
                {
                    string destFile = path + "\\" + cb_airfoils.SelectedItem.ToString() + ".txt";
                    System.IO.File.Copy(sourceFile, destFile, true);
                    nacaProfile = LoadNacaProfile(destFile);
                }
            }

            private void btn_PointFixedInImage_Click(object sender, EventArgs e)
            {
                if (!Application.OpenForms.OfType<FixedInImage>().Any() && !Application.OpenForms.OfType<FixedInObject>().Any())
                {
                    ImagePoint tmp = new ImagePoint(new Pen(Color.Green, 2),cross);
                    listFixedInImage.Add(tmp);
                    formFixedInImage = new FixedInImage(this, tmp);
                    formFixedInImage.Show();
                    imageBox.Invalidate();
                }
            }
            private void btn_PointFixedInObject_Click(object sender, EventArgs e)
            {
                if (!Application.OpenForms.OfType<FixedInObject>().Any() && !Application.OpenForms.OfType<FixedInImage>().Any())
                {
                    if (tabControl1.SelectedTab.Equals(tabAirfoil) && !calibrateProfilePoint1.IsEmpty() && !calibrateProfilePoint2.IsEmpty()
                        || tabControl1.SelectedTab.Equals(tabAnalyze) && !images.getActual().point1.IsEmpty() && !images.getActual().point2.IsEmpty())
                    {
                        ObjectPoint tmp = new ObjectPoint(new Pen(Color.Green,2));
                        tmp.setPointLocationByPoint(cross, calibrateProfilePoint1.Point, calibrateProfilePoint2.Point);
                        listFixedInObject.Add(tmp);
                        formFixedInObject = new FixedInObject(this, tmp);
                        formFixedInObject.Show();
                        imageBox.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("There is no object to get fixed on!");
                    }
                }
            }
            private void btn_addElastic_Click(object sender, EventArgs e)
            {
                if (GetElasticAxis() != null)
                {
                    formFixedInObject = new FixedInObject(this, GetElasticAxis());
                    formFixedInObject.Show();
                    return;
                }

                if (!Application.OpenForms.OfType<FixedInObject>().Any() && !Application.OpenForms.OfType<FixedInImage>().Any())
                {
                    if (tabControl1.SelectedTab.Equals(tabAirfoil) && !calibrateProfilePoint1.IsEmpty() && !calibrateProfilePoint2.IsEmpty()
                        || tabControl1.SelectedTab.Equals(tabAnalyze) && !images.getActual().point1.IsEmpty() && !images.getActual().point2.IsEmpty())
                    {
                        ObjectPoint tmp = new ObjectPoint(new Pen(Color.Green, 2),cross);
                        tmp.setPointLocationByPoint(cross, calibrateProfilePoint1.Point, calibrateProfilePoint2.Point);
                        tmp.isElastic = true;
                        listFixedInObject.Add(tmp);
                        formFixedInObject = new FixedInObject(this, tmp);
                        formFixedInObject.Show();
                        imageBox.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("There is no object to get fixed on!");
                    }
                }
            }
            private void btn_next_Click(object sender, EventArgs e)
            {
                if (tb_scale.Text == String.Empty || tb_ideal_legth.Text == String.Empty || tb_real_length.Text == String.Empty)
                {
                    MessageBox.Show("You must fill all parameters!");
                    return;
                }
                if(GetElasticAxis() == null)
                {
                    MessageBox.Show("Define your elastic axis!");
                    return;
                }


                DialogResult dialogResult = MessageBox.Show("You won’t be allowed to modify the data entered up to this point later, do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //projectName = tb_project_name.Text;
                    tabControl1.SelectedIndex = 2;
                    flowLayoutPanel7.Enabled = false;
                    flowLayoutPanel8.Enabled = true;
                    //panel5.Enabled = false;
                    panelScale.Enabled = false;
                    level = 1;                    
                }
                
            }
            
        #endregion

        #region Events of PROJECT tab
            private void btnSetPoint1_Click(object sender, EventArgs e)
            {
                setPoint1(cross);
            }
            private void setPoint1(PointF p = new PointF())
            {
                if (tabControl1.SelectedTab.Equals(tabCalibrate)) // setPoint for calibration
                {
                    if (!p.Equals(calibratePoint2.Point))
                        calibratePoint1.Point = p;
                    else
                        MessageBox.Show("Points can not be it the same location!");

                    if (!calibratePoint1.IsEmpty() && !calibratePoint2.IsEmpty())
                    {
                        distanceBetweenCalibratePoints = GetDistanceBetween(calibratePoint1.Point, calibratePoint2.Point);

                        CalculateScale();
                    }
                }
                else if (tabControl1.SelectedTab.Equals(tabAnalyze) && level == 3) // setPoint for analyze 
                {

                    if (!p.Equals(images.getActual().point2.Point))
                    {
                        if (!images.getActual().point2.IsEmpty())
                        {

                            double distance = GetDistanceBetween(images.getActual().point2.Point, p);
                            double ratio = distanceBetweenCalibratePoints / distance;
                            double vectorX = ((p.X - images.getActual().point2.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                            double vectorY = ((p.Y - images.getActual().point2.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                            images.getActual().setPoint1(new PointF((float)(images.getActual().point2.Point.X + vectorX), (float)(images.getActual().point2.Point.Y + vectorY)));

                            UpdateObjectFixedPoints();
                            UpdateInfo();
                        }
                        else
                            images.getActual().setPoint1(p);

                    }
                    else
                        MessageBox.Show("Points can not be it the same location!");
                }

                imageBox.Invalidate();
            }
            private void btnSetPoint2_Click(object sender, EventArgs e)
            {
                setPoint2(cross);
            }
            private void setPoint2(PointF p = new PointF())
            {
                if (tabControl1.SelectedTab.Equals(tabCalibrate))
                {
                    if (!p.Equals(calibratePoint1.Point))
                        calibratePoint2.Point = p;
                    else
                        MessageBox.Show("Points can not be it the same location!");

                    if (!calibratePoint1.IsEmpty() && !calibratePoint2.IsEmpty()) // setPoint for calibration
                    {
                        distanceBetweenCalibratePoints = GetDistanceBetween(calibratePoint1.Point, calibratePoint2.Point);

                        CalculateScale();
                    }
                }
                else if (tabControl1.SelectedTab.Equals(tabAnalyze) && level == 3) // setPoint for analyze 
                {
                    if (!p.Equals(images.getActual().point1.Point))
                    {
                        if (!images.getActual().point1.IsEmpty())
                        {

                            double distance = GetDistanceBetween(images.getActual().point1.Point, p);
                            double ratio = distanceBetweenCalibratePoints / distance;
                            double vectorX = ((p.X - images.getActual().point1.Point.X) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                            double vectorY = ((p.Y - images.getActual().point1.Point.Y) * ratio) * (Double.Parse(tb_real_length.Text, CultureInfo.InvariantCulture) / 100) * (Double.Parse(tb_ideal_legth.Text, CultureInfo.InvariantCulture) / Double.Parse(tb_scale.Text, CultureInfo.InvariantCulture));
                            images.getActual().setPoint2(new PointF((float)(images.getActual().point1.Point.X + vectorX), (float)(images.getActual().point1.Point.Y + vectorY)));
                            

                            UpdateObjectFixedPoints();
                            UpdateInfo();
                        }
                        else
                            images.getActual().setPoint2(p);

                    }
                    else
                        MessageBox.Show("Points can not be it the same location!");
                }

                imageBox.Invalidate();
            }
            //Scale
            private void tb_scale_KeyPress(object sender, KeyPressEventArgs e)
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
            private void tb_scale_Leave(object sender, EventArgs e)
            {
                if (String.IsNullOrWhiteSpace(tb_scale.Text))
                    tb_scale.Text = "0";
                CalculateScale();
            }
            private void checkBox1_CheckedChanged(object sender, EventArgs e)
            {
                if (check_eqHist.Checked == true)
                    isChecked = true;
                else
                    isChecked = false;

                OpenImage(new Image<Gray, byte>(images.getActual().path));
            }

            private void trackBar2_ValueChanged(object sender, EventArgs e)
            {
                OpenImage(new Image<Gray, byte>(images.getActual().path));
            }

            private void btn_reset_Click(object sender, EventArgs e)
            {
                track_gamma.Value = 10;
            }

            private void button3_Click(object sender, EventArgs e)
            {

                if (tb_scale.Text == String.Empty)
                {
                    MessageBox.Show("You must fill all parameters!");
                    return;
                }

                //if (tb_project_name.Text == String.Empty)
                //{
                //    MessageBox.Show("Name your project!");
                //    return;
                //}

                if (calibratePoint1.IsEmpty() || calibratePoint2.IsEmpty())
                {
                    MessageBox.Show("Set calibration points!");
                    return;
                }

                DialogResult dialogResult = MessageBox.Show("You won’t be allowed to modify the data entered up to this point later (except for the gamma correction and equalization), do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //projectName = tb_project_name.Text;
                    tabControl1.SelectedIndex = 1;
                    //panel5.Enabled = false;
                    panelScale.Enabled = false;
                    flowLayoutPanel7.Enabled = true;
                    flowLayoutPanel8.Enabled = false;
                    //panel5.Enabled = false;
                    panelScale.Enabled = false;
                    level = 1;
                }
            }
        #endregion

        #region Events of PARAMETERS tab

        private void btn_setFromEA_Click(object sender, EventArgs e)
        {
            if (GetElasticAxis() == null)
            {
                MessageBox.Show("Define EA axis you foool!!!");
                return;
            }


            tb_w0.Text = (Math.Round(px2mm(imageBox.Image.Size.Height - GetElasticAxis().location.Y), 2)).ToString();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (isNotNumeric(tb_heat.Text) ||
                isNotNumeric(tb_wave.Text) ||
                isNotNumeric(tb_L.Text) ||
                isNotNumeric(tb_R.Text) ||
                isNotNumeric(tb_K.Text) ||
                isNotNumeric(tb_t0.Text) ||
                isNotNumeric(tb_p0.Text) ||
                isNotNumeric(tb_tau0.Text) ||
                isNotNumeric(tb_dTau.Text) ||
                isNotNumeric(tb_w0.Text))
            {
                MessageBox.Show("Some invalid parameter");
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("You won't be allowed to modify these parameters later, do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    MeasureParameters.k = (float)Double.Parse(tb_heat.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.wave_lenght = (float)Double.Parse(tb_wave.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.L = (float)Double.Parse(tb_L.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.R = (float)Double.Parse(tb_R.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.K = (float)Double.Parse(tb_K.Text.Replace(',', '.'), CultureInfo.InvariantCulture);

                    MeasureParameters.t0 = (float)Double.Parse(tb_t0.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.p0 = (float)Double.Parse(tb_p0.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.tau0 = (float)Double.Parse(tb_tau0.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.dTau = (float)Double.Parse(tb_dTau.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.M = (float)Double.Parse(tb_mach.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    MeasureParameters.w0 = (float)mm2px(Double.Parse(tb_w0.Text.Replace(',', '.'), CultureInfo.InvariantCulture));

                    writeLastUsed();

                    tabControl1.SelectedIndex = 3;
                    flowLayoutPanel8.Enabled = false;
                    panel2.Enabled = true;
                    saveToolStripMenuItem.Enabled = true;
                    level = 3;

                    
                    l_min.Text = MeasureParameters.tau0.ToString();

                    l_max.Text = (MeasureParameters.tau0 + (images.imagesList.Count) * MeasureParameters.dTau).ToString();


                }
            }
        }

        private void btn_loadLast_Click(object sender, EventArgs e)
        {
            tb_t0.Text = readLastUsed("t0:");
            tb_p0.Text = readLastUsed("p0:");
            tb_dTau.Text = readLastUsed("dTau:");
            tb_heat.Text = readLastUsed("heat:");
            tb_wave.Text = readLastUsed("wave:");
            tb_L.Text = readLastUsed("L:");
            tb_R.Text = readLastUsed("R:");
            tb_K.Text = readLastUsed("K:");
        }

        private bool isNotNumeric(string s)
        {
            try
            {
                double t = Double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);
                return false;
            }
            catch
            {
                return true;
            }
        }


        #endregion

        #region Events of ANALYS tab
        private void button1_Click(object sender, EventArgs e)
        {
            setPoint1(); // funcion in calibrate region
        }
        private void button2_Click(object sender, EventArgs e)
        {
            setPoint2(); // funcion in calibrate region
        }
        private void setPointForLine_Click(object sender, EventArgs e)
        {
            if (images.getActual().point1.IsEmpty() || images.getActual().point2.IsEmpty())
                return;

            images.getActual().AddLine(cross);
            imageBox.Invalidate();
            setDataGrids();
        }
        private void initDataGrids()
        {
                
            dataGridUpper.DataSource = images.getActual().listUpperLines;
            dataGridDowner.DataSource = images.getActual().listDownerLines;

            dataGridUpper.Columns[0].Visible = false;
            dataGridUpper.Columns[1].Visible = false;

            dataGridUpper.Columns[2].HeaderText = "Fringe position";
            dataGridUpper.Columns[3].HeaderText = "Fringe number";
            dataGridUpper.Columns[4].HeaderText = "Surface coor";
            dataGridUpper.Columns[5].HeaderText = "Pressure [kPa]";
            dataGridUpper.Columns[6].HeaderText = "Mach [1]";
            dataGridUpper.Columns[7].HeaderText = "Velocity [m/s]";
            dataGridUpper.Columns[8].HeaderText = "Density [kg/m³]";

            dataGridDowner.Columns[0].Visible = false;
            dataGridDowner.Columns[1].Visible = false;

            dataGridDowner.Columns[2].HeaderText = "Fringe position";
            dataGridDowner.Columns[3].HeaderText = "Fringe number";
            dataGridDowner.Columns[4].HeaderText = "Surface coor";
            dataGridDowner.Columns[5].HeaderText = "Pressure [kPa]";
            dataGridDowner.Columns[6].HeaderText = "Mach [1]";
            dataGridDowner.Columns[7].HeaderText = "Velocity [m/s]";
            dataGridDowner.Columns[8].HeaderText = "Density [kg/m³]";

            dataGridDowner.ClearSelection();
            dataGridUpper.ClearSelection();
        }
        private void setDataGrids()
        {

            dataGridUpper.DataSource = images.getActual().listUpperLines;
            dataGridUpper.ClearSelection();
            dataGridDowner.DataSource = images.getActual().listDownerLines;
            dataGridDowner.ClearSelection();


            dataGridUpper.Refresh();
            dataGridDowner.Refresh();

            

        }
        private void dataGridUpper_LostFocus(object sender, EventArgs e)
        {
            if(isSelected.Equals("dataGrid"))
                selectedLine = null;
            dataGridUpper.ClearSelection();
        }
        private void dataGridDowner_LostFocus(object sender, EventArgs e)
        {
            if (isSelected.Equals("dataGrid"))
                selectedLine = null; 
            dataGridDowner.ClearSelection();
        }
        private void dataGridUpper_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridUpper.SelectedRows.Count > 0)
            {
                selectedLine = images.getActual().listUpperLines[dataGridUpper.SelectedRows[0].Index];
                if(dataGridUpper.Focused)
                    isSelected = "dataGrid";
            }
            else
                selectedLine = null;

            imageBox.Invalidate();
        }
        private void dataGridDowner_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridDowner.SelectedRows.Count > 0)
            {
                selectedLine = images.getActual().listDownerLines[dataGridDowner.SelectedRows[0].Index];
                if (dataGridDowner.Focused)
                    isSelected = "dataGrid";
            }
            else
                selectedLine = null;

            imageBox.Invalidate();
        }
        private void dataGridUpper_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridUpper.SelectedRows.Count > 0)
            {
                editCellUp(dataGridUpper.SelectedRows[0].Index, true);
            }
        }
        private void editCellUp(int index, bool upper)
        {
            DataGridViewCell cell = dataGridUpper.Rows[index].Cells[3];
            dataGridUpper.CurrentCell = cell;
            dataGridUpper.BeginEdit(true);
        }
        private void dataGridDowner_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridDowner.SelectedRows.Count > 0)
            {
                editCellDown(dataGridDowner.SelectedRows[0].Index, true);
            }
        }
        private void editCellDown(int index, bool upper)
        {
            DataGridViewCell cell = dataGridDowner.Rows[index].Cells[3];
            dataGridDowner.CurrentCell = cell;
            dataGridDowner.BeginEdit(true);
        }
        private void dataGridUpper_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }
        private void tb_description_TextChanged(object sender, EventArgs e)
        {
            images.getActual().description = tb_description.Text;
            imageBox.Invalidate();
        }

    #endregion

        #region Save-Load und Export
        
        public void SaveProject()
        {
            images.k = MeasureParameters.k;
            images.wave_lenght = MeasureParameters.wave_lenght;
            images.L = MeasureParameters.L;
            images.R = MeasureParameters.R;
            images.K = MeasureParameters.K;
            images.p0 = MeasureParameters.p0;
            images.t0 = MeasureParameters.t0;
            images.tau0 = MeasureParameters.tau0;
            images.dTau = MeasureParameters.dTau;
            images.w0 = MeasureParameters.w0;
            images.M = MeasureParameters.M;
            

            images.A = A;
            images.B = B;
            images.calibrateProfilePoint1 = calibrateProfilePoint1;
            images.calibrateProfilePoint2 = calibrateProfilePoint2;
            images.calibratePoint1 = calibratePoint1;
            images.calibratePoint2 = calibratePoint2;
            images.realLength = realLength;
            images.PercentRealLenght = PercentRealLenght;
            images.idealLength = idealLength;
            images.scale = Double.Parse(tb_scale.Text);
            images.ratio = GlobalSettings.ratio;
            images.listFixedInImage = listFixedInImage;
            images.listFixedInObject = listFixedInObject;

            images.path_airfoil = path_airfoil;
            images.arrayProfile = MainWindow.arrayProfile;

            images.isEqualizer = check_eqHist.Checked;
            images.gammaCorrection = track_gamma.Value;

            images.tb_ideal = tb_ideal_legth.Text;
            images.tb_real = tb_real_length.Text;


            foreach (ImagePoint p in images.listFixedInImage)
            {
                p.color = p.pen.Color.ToString();
            }
            foreach (ObjectPoint p in images.listFixedInObject)
            {
                p.color = p.pen.Color.ToString();
            }

            foreach (MyImage mi in images.imagesList)
            {
                mi.arrayProfile = null;
            }
            try
            {
                SerializeObject(Path.GetDirectoryName(images.imagesList[0].path) + "\\" + projectName + ".ifg", images);
                MessageBox.Show("Project was saved as "+Path.GetDirectoryName(images.imagesList[0].path)+"\\"+projectName+".ifg !");  
            }
            catch (Exception e){
                MessageBox.Show("Project was not saved!" + e.Message);  
            }
            foreach (MyImage im in images.imagesList)
            {
                im.setPoint1(im.point1.Point);
            }
                

            
        }
        public void LoadProject()
        {

            if (level == 3)
                if (!SureClose("Unsaved data will be lost, do you want to continue?", "Warning"))
                    return;

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                if (images == null)
                    images = new ImagesClass();

                ImagesClass mages = DeSerializeObject(openFileDialog1.FileName);

                projectName =  Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                this.Text = formName + projectName;

                for (int i = 0; i < mages.listFixedInImage.Count; i++)
                {
                    string[] s = mages.listFixedInImage[i].color.Replace("]", "[").Split('[');
                    mages.listFixedInImage[i].pen = new Pen(Color.FromName(s[1]), 2);
                }
                for (int i = 0; i < mages.listFixedInObject.Count; i++)
                {
                    string[] s = mages.listFixedInObject[i].color.Replace("]", "[").Split('[');
                    mages.listFixedInObject[i].pen = new Pen(Color.FromName(s[1]), 2);
                }

                MeasureParameters.k = mages.k;
                MeasureParameters.wave_lenght = mages.wave_lenght;
                MeasureParameters.L = mages.L;
                MeasureParameters.R = mages.R;
                MeasureParameters.K = mages.K;
                MeasureParameters.p0 = mages.p0;
                MeasureParameters.t0 = mages.t0;
                MeasureParameters.tau0 = mages.tau0;
                MeasureParameters.dTau = mages.dTau;
                MeasureParameters.w0 = mages.w0;
                MeasureParameters.M = mages.M;

                tb_heat.Text = mages.k.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_wave.Text = mages.wave_lenght.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_L.Text = mages.L.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_R.Text = mages.R.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_K.Text = mages.K.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_p0.Text = mages.p0.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                
                tb_dTau.Text = mages.dTau.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_tau0.Text = mages.tau0.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                tb_t0.Text = mages.t0.ToString(CultureInfo.CreateSpecificCulture("en-GB"));

                A = mages.A;
                B = mages.B;
                calibrateProfilePoint1 = mages.calibrateProfilePoint1;
                calibrateProfilePoint2 = mages.calibrateProfilePoint2;
                calibratePoint1 = mages.calibratePoint1;
                calibratePoint2 = mages.calibratePoint2;
                PercentRealLenght = mages.PercentRealLenght;
                realLength = mages.realLength;
                idealLength = mages.idealLength;
                tb_scale.Text = mages.scale.ToString();
                GlobalSettings.ratio = mages.ratio;
                listFixedInImage = mages.listFixedInImage;
                listFixedInObject = mages.listFixedInObject;

                distanceBetweenCalibratePoints = GetDistanceBetween(calibratePoint1.Point, calibratePoint2.Point);

                path_airfoil = mages.path_airfoil;
                MainWindow.arrayProfile = mages.arrayProfile;

                images = mages;

                check_eqHist.Checked = mages.isEqualizer;
                track_gamma.Value = mages.gammaCorrection;

                tb_real_length.Text = mages.tb_real;
                tb_ideal_legth.Text = mages.tb_ideal;

                tb_w0.Text = px2mm(mages.w0).ToString(CultureInfo.CreateSpecificCulture("en-GB"));

                string new_path = Path.GetDirectoryName(openFileDialog1.FileName);
                path = new_path + "\\" + images.imagesList[0].name;

                if (!isInit)
                {
                    initProgram();
                    isInit = true;
                    flowLayoutPanel7.Enabled = false;
                    flowLayoutPanel8.Enabled = false;
                    panelScale.Enabled = false;

                    panel2.Enabled = true;
                    saveToolStripMenuItem.Enabled = true;
                }

                tabControl1.SelectedIndex = 3;

                images.getByIndex(images.pointer);
                OpenImage(new Image<Gray, Byte>(images.getActual().path));

                trackBar1.Maximum = images.getCount();
                trackBar1.Value = images.pointer + 1;
                l_min.Text = MeasureParameters.tau0.ToString();
                l_max.Text = (MeasureParameters.tau0 + (images.imagesList.Count) * MeasureParameters.dTau).ToString();

                UpdateInfo();
                UpdateObjectFixedPoints();
                UpdateStatusBar();

                foreach (MyImage im in images.imagesList)
                {
                    im.path = new_path + "\\" + im.name;
                    im.setPoint1(im.point1.Point);
                }
                

                if (images.imagesList != null)
                    foreach (Line l in images.getActual().listLines)
                    {
                        images.getActual().setLine(l, l.pointOfProfile.Point);
                    }

                imageBox.Invalidate();
                setDataGrids();

                level = 3;
                
            }
        }

        public void Export()    
        {
            if (level != 3)
                return;

            folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(path);
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;
            
            //expBit();
            string export_path = folderBrowserDialog1.SelectedPath + @"\export_" + projectName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            System.IO.Directory.CreateDirectory(export_path);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(export_path + @"\"+projectName+".txt"))
            {
                int inc=0;
                file.WriteLine("Index\tName\tTime\tPitch\tPlunge");
                foreach (MyImage image in images.imagesList)
                {
                    if (image.point1.Point == Point.Empty || image.point2.Point == Point.Empty)
                        continue;

                    if(image.time != 0)
                        file.WriteLine(inc.ToString()+"\t"+image.ToString());
                    inc++;
                }
            }

            System.IO.Directory.CreateDirectory(export_path + @"\Upper surface");
            System.IO.Directory.CreateDirectory(export_path + @"\Lower surface");

            foreach (MyImage image in images.imagesList)
            {
                if (image.listUpperLines.Count != 0)
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(export_path + @"\Upper surface\" + Path.GetFileNameWithoutExtension(image.name) + ".txt"))
                    {
                        file.WriteLine("Marker\tSurface coordinate\tFringe index\tPressure\tMach\tVelocity\tDensity");
                        foreach (Line line in image.listUpperLines)
                        {
                            file.WriteLine(line.ToString());
                        }
                    }
                if (image.listDownerLines.Count != 0)
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(export_path + @"\Lower surface\" + Path.GetFileNameWithoutExtension(image.name) + ".txt"))
                    {
                        file.WriteLine("Marker\tSurface coordinate\tFringe index\tPressure\tMach\tVelocity\tDensity");
                        foreach (Line line in image.listDownerLines)
                        {
                            file.WriteLine(line.ToString());
                        }
                    }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(export_path + @"\parameters.txt"))
            {
                file.WriteLine("Heat capacity ratio\tSpecific fas constant R\tWave lenght λ\tTest section widht L\tGladston-Dale constant K");
                file.WriteLine(MeasureParameters.k.ToString() + "\t" + 
                                MeasureParameters.R.ToString() + "\t" +
                                MeasureParameters.wave_lenght.ToString() + "\t" +
                                MeasureParameters.L.ToString() + "\t" + 
                                MeasureParameters.K.ToString());

                file.WriteLine("Temperature t0\tTotal pressure p0\tTime of first frame τ0\tCamera rate  Δτ\tInitial plunge w0\tInlet Mach number M");
                file.WriteLine(MeasureParameters.t0.ToString() + "\t" +
                                MeasureParameters.p0.ToString() + "\t" +
                                MeasureParameters.tau0.ToString() + "\t" +
                                MeasureParameters.dTau.ToString() + "\t" +
                                MeasureParameters.w0.ToString() + "\t" +
                                MeasureParameters.M.ToString());
                
            }

            MessageBox.Show("Export was successly completed. Folders \n" + export_path + "\n" + export_path + @"\Upper surface"+"\n"+export_path+@"\Lower suface"+"\n were created!");

        }
        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (level == 3)
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    expBit(saveFileDialog1.FileName);
                }
        }
        
        public void SerializeObject(string filename, ImagesClass objectToSerialize)
        {
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(objectToSerialize.GetType());
            //using (TextWriter writer = new StreamWriter(filename))
            //{
            //    x.Serialize(writer, objectToSerialize); 
            //} 

            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, objectToSerialize);
            stream.Close();
        }
        public ImagesClass DeSerializeObject(string filename)
        {
            //XmlSerializer deserializer = new XmlSerializer(typeof(ImagesClass));
            //TextReader reader = new StreamReader(filename);
            //object obj = deserializer.Deserialize(reader);
            //ImagesClass XmlData = (ImagesClass)obj;
            //reader.Close(); 

            ImagesClass objectToSerialize;
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            objectToSerialize = (ImagesClass)bFormatter.Deserialize(stream);
            stream.Close();

            return objectToSerialize;
        }
        #endregion

        #region Backroundworker
        private void bw_LoadImage(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null)
            {
                image_for_emgu = new Image<Gray, Byte>(images.getNext().path);
            }
            else
            {
                image_for_emgu = new Image<Gray, Byte>(images.getByIndex((int)e.Argument).path);
            }
            if (isPlaying)
                System.Threading.Thread.Sleep(1);
        }
        private void bw_LoadingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OpenImage(image_for_emgu);
            GC.Collect();
            if (isPlaying)
            {
                trackBar1.Value = images.getPosition();
                if (images.isLast() || images.isFirst())
                {
                    setDataGrids();
                    isPlaying = false;
                }
                else
                    bw.RunWorkerAsync();
            }
            else
                setDataGrids();
        }
        #endregion

        #region Buttons for player
        private void play_Click(object sender, EventArgs e)
        {
            if (isPlaying)
                isPlaying = false;
            else
                if (!bw.IsBusy)
                {
                    isPlaying = true;
                    bw.RunWorkerAsync();
                }
        }
        private void stop_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            UpdateStatusBar();
            UpdateInfo();
        }
        private void toFirst_Click(object sender, EventArgs e)
        {
            OpenImage(new Image<Gray, Byte>(images.getFirst().path));
            trackBar1.Value = images.getPosition();
            UpdateObjectFixedPoints();
            UpdateStatusBar();
            UpdateInfo();
            setDataGrids();
        }
        private void toPrevious_Click(object sender, EventArgs e)
        {
            OpenImage(new Image<Gray, Byte>(images.getPrevious().path));
            trackBar1.Value = images.getPosition();
            UpdateObjectFixedPoints();
            UpdateStatusBar();
            UpdateInfo();
            setDataGrids();
        }
        private void toNext_Click(object sender, EventArgs e)
        {
            OpenImage(new Image<Gray, Byte>(images.getNext().path));
            trackBar1.Value = images.getPosition();
            UpdateObjectFixedPoints();
            UpdateStatusBar();
            UpdateInfo();
            setDataGrids();

        }
        private void toLast_Click(object sender, EventArgs e)
        {
            OpenImage(new Image<Gray, Byte>(images.getLast().path));
            trackBar1.Value = images.getPosition();
            UpdateObjectFixedPoints();
            UpdateStatusBar();
            UpdateInfo();
            setDataGrids();
        }
        #endregion

        #region Funkcions
        private void OpenImage(Image<Gray, Byte> img)
        {
            if (isChecked)
                img._EqualizeHist();

            img._GammaCorrect((double)track_gamma.Value / 10);
            image_for_emgu = img;
            imageBox.Image = img.ToBitmap();
            if (images.imagesList != null && images.getActual().arrayProfile != null)
                foreach (Line l in images.getActual().listLines)
                {
                    images.getActual().setLine(l, l.pointOfProfile.Point);
                }
            this.UpdateStatusBar();
        }
        protected string FormatPoint(Point point)
        {
            return string.Format("X:{0}, Y:{1}", point.X, point.Y);
        }
        protected string FormatSize(Size size)
        {
            return string.Format("X:{0}, Y:{1}", size.Width, size.Height);
        }
        private void UpdateStatusBar()   //vsechny zobrazovadla aktualizovat
        {
            //frameLabel.Text = images.getPosition().ToString();
            if(!(l_name.Text == images.getActual().name))
                l_name.Text = images.getActual().name;
            //zoomStripStatusLabel.Text = imageBox.Zoom.ToString();
            //sizeStripStatusLabel.Text = FormatSize(imageBox.Size);
            l_frame.Text = images.getPosition().ToString() + "/" + images.getCount().ToString()+" ";

            if (MeasureParameters.tau0 != null && MeasureParameters.dTau != null)
            {
                // τ = τ0 + (iF-1) * Δτ
                images.getActual().time = MeasureParameters.tau0 + (images.getPosition() - 1) * MeasureParameters.dTau;
                l_time.Text = (Math.Round(images.getActual().time, 4)).ToString();

            }

        }
        public static double GetDistanceBetween(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        private List<PointF> LoadNacaProfile(string s)
        {

            List<PointF> nacaProfile = new List<PointF>();
            bool isSym = true;
            System.IO.StreamReader stream;
            string line;

            // Read the file line by line.
            stream = new System.IO.StreamReader(s);
            bool firstTime = true;
            int count = -1;
            string[] separ;
            while ((line = stream.ReadLine()) != null)
            {
                count++;
                if (firstTime)
                {
                    firstTime = false;
                    if (line.Equals("sym"))
                    {
                        isSym = true;
                    }
                    else
                    {
                        isSym = false;
                    }
                }
                else
                {
                    separ = line.Split('\t');

                    nacaProfile.Add(new PointF(float.Parse(separ[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat), float.Parse(separ[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));

                }

            }

            if (isSym)
            {
                List<PointF> tmpList = new List<PointF>();
                foreach (PointF p in nacaProfile)
                {
                    tmpList.Add(new PointF(p.X, -p.Y));
                }
                foreach (PointF p in tmpList)
                {
                    nacaProfile.Add(p);
                }

            }
            stream.Close();
            return nacaProfile;

        }
        private string GetAngle(double decimal_degrees)
        {
            // set decimal_degrees value here

            double minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            double seconds = (minutes - Math.Floor(minutes)) * 60.0;
            double tenths = (seconds - Math.Floor(seconds)) * 10.0;
            // get rid of fractional part
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);

            string s = "";

            if (tenths != 0)
            {
                s += tenths.ToString() + "° ";
            }
            if (minutes != 0)
            {
                s += minutes.ToString() + "' ";
            }
            if (seconds != 0)
            {
                s += seconds.ToString() + "\"";
            }

            return s;
        }
        private void ComputePointsForRealLength()
        {
            double xPoint = realLength / idealLength;

            int i = 0;
            for (i = 0; i < nacaProfile.Count-1; i++)
            {
                if (nacaProfile[i + 1].X == 0)
                    continue;

                if (nacaProfile[i].Y > 0)
                {
                    if ((nacaProfile[i].X > xPoint && nacaProfile[i + 1].X < xPoint) || (nacaProfile[i].X < xPoint && nacaProfile[i + 1].X > xPoint)) // testing if [i]---p---[i+1] or [i+1]---p---[i]
                    {
                        double fDistance; // distance on x-axis between point of real length and point on right/left side 
                        double sDistance; // distance on x-axis between point of real length and point on right/left side 
                        double ratio;     
                        double distance; // distance between those two points 


                        fDistance = Math.Abs(xPoint - nacaProfile[i].X);
                        sDistance = Math.Abs(nacaProfile[i + 1].X - xPoint);

                        ratio = fDistance / sDistance;
                        System.Windows.Vector v1 = new System.Windows.Vector(nacaProfile[i].X - nacaProfile[i + 1].X, nacaProfile[i].Y - nacaProfile[i + 1].Y);
                        distance = GetDistanceBetween(nacaProfile[i], nacaProfile[i + 1]);
                        double lenghtNewVector = (distance * sDistance) / (fDistance + sDistance);
                        v1.Normalize();
                        v1 = System.Windows.Vector.Multiply(lenghtNewVector, v1);

                        A = new PointF((float)(nacaProfile[i + 1].X + v1.X), (float)(nacaProfile[i + 1].Y + v1.Y));
                    }
                    else if (nacaProfile[i].X == xPoint) 
                    {
                        A = new PointF((float)(nacaProfile[i].X), (float)(nacaProfile[i].Y));
                    }
                    else if (nacaProfile[i + 1].X == xPoint)
                    {
                        A = new PointF((float)(nacaProfile[i + 1].X), (float)(nacaProfile[i + 1].Y));
                    }
                }
                else if (nacaProfile[i].Y < 0)
                {
                    if ((nacaProfile[i].X > xPoint && nacaProfile[i + 1].X < xPoint) || (nacaProfile[i].X < xPoint && nacaProfile[i + 1].X > xPoint)) // testing if [i]---p---[i+1] or [i+1]---p---[i]
                    {
                        double fDistance; // distance on x-axis between point of real length and point on right/left side 
                        double sDistance; // distance on x-axis between point of real length and point on right/left side 
                        double ratio;
                        double distance; // distance between those two points 


                        fDistance = Math.Abs(xPoint - nacaProfile[i].X);
                        sDistance = Math.Abs(nacaProfile[i + 1].X - xPoint);

                        ratio = fDistance / sDistance;
                        System.Windows.Vector v1 = new System.Windows.Vector(nacaProfile[i].X - nacaProfile[i + 1].X, nacaProfile[i].Y - nacaProfile[i + 1].Y);
                        distance = GetDistanceBetween(nacaProfile[i], nacaProfile[i + 1]);
                        double lenghtNewVector = (distance * sDistance) / (fDistance + sDistance);
                        v1.Normalize();
                        v1 = System.Windows.Vector.Multiply(lenghtNewVector, v1);

                        B = new PointF((float)(nacaProfile[i + 1].X + v1.X), (float)(nacaProfile[i + 1].Y + v1.Y));
                    }
                    else if (nacaProfile[i].X == xPoint)
                    {
                        B = new PointF((float)(nacaProfile[i].X), (float)(nacaProfile[i].Y));
                    }
                    else if (nacaProfile[i + 1].X == xPoint)
                    {
                        B = new PointF((float)(nacaProfile[i + 1].X), (float)(nacaProfile[i + 1].Y));
                    }
                }
            }

        }
        public void DeleteImagePoint(ImagePoint p)
        {
            listFixedInImage.Remove(p);
            imageBox.Invalidate();
        }
        public void SetImagePoint(ImagePoint p)
        {
            listFixedInImage.Add(p);
            imageBox.Invalidate();
        }
        public void DeleteObjectPoint(ObjectPoint p)
        {
            listFixedInObject.Remove(p);
            imageBox.Invalidate();
        }
        public void SetObjectPoint(ObjectPoint p)
        {
            listFixedInObject.Add(p);
            UpdateObjectFixedPoints();
            imageBox.Invalidate();
        }
        public void UpdateObjectFixedPoints()
        {
            foreach (ObjectPoint p in listFixedInObject)
            {
                if (tabControl1.SelectedTab.Equals(tabAnalyze))
                {
                    p.setPointLocation(
                        images.getActual().point1.Point,
                        images.getActual().point2.Point);
                }
                else
                {
                    p.setPointLocation(
                        calibrateProfilePoint1.Point, 
                        calibrateProfilePoint2.Point);
                    
                }
            }
        }
        private ObjectPoint GetElasticAxis()
        {
            foreach (ObjectPoint op in listFixedInObject)
                if (op.isElastic)
                    return op;
            return null;
        }
        private void UpdateInfo()
        {
            if (tabControl1.SelectedTab.Equals(tabAnalyze) && !images.getActual().point1.IsEmpty() && !images.getActual().point2.IsEmpty())
            {
                

                //vertical shift 
                images.getActual().plunge = (float)(Math.Round(((imageBox.Image.Size.Height - GetElasticAxis().location.Y) - MeasureParameters.w0), 2));
                l_plunge.Text = px2mm(images.getActual().plunge).ToString();

                //pitch
                System.Windows.Vector v1 = new System.Windows.Vector(images.getActual().point2.Point.X - images.getActual().point1.Point.X,
                                                                    images.getActual().point2.Point.Y - images.getActual().point1.Point.Y);
                System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
                double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
                angleBetween *= -1;
                images.getActual().pitch = (float)Math.Round(angleBetween, 2);
                l_pitch.Text = Math.Round(images.getActual().pitch,3).ToString() + "°";


                if (images.getPosition() > 1)
                {
                    images.getActual().vel_plunge = (images.getActual().plunge - images.get(images.pointer - 1).plunge) / MeasureParameters.dTau;
                    images.getActual().vel_pitch = (images.getActual().pitch - images.get(images.pointer - 1).pitch) / MeasureParameters.dTau;

                    l_plunge_vel.Text = px2mm(images.getActual().vel_plunge).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                    l_pitch_vel.Text = Math.Round(images.getActual().vel_pitch, 3).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                }
                else
                {
                    l_plunge_vel.Text = "NaN";
                    l_pitch_vel.Text = "NaN";
                }
                

            }
            else
            {
                l_plunge_vel.Text = l_pitch_vel.Text = l_pitch.Text = l_plunge.Text = "-";
            }
            tb_description.Text = images.getActual().description;

        }
        private double px2mm(double px)
        {
            return Math.Round((px*Double.Parse(tb_scale.Text))/distanceBetweenCalibratePoints,3);
        }
        private double mm2px(double mm)
        {
            return Math.Round((mm * distanceBetweenCalibratePoints) / Double.Parse(tb_scale.Text),3);
        }
        public string pen2string(Pen p)
        {
            return color2string(p.Color) + "#" + p.Width.ToString();
        }
        private static String color2string(Color c)
        {
            return c.R.ToString() + "@" + c.G.ToString() + "@" + c.B.ToString() + "@"+c.A.ToString();
        }
        private static Color string2color(string s)
        {
            string[] a1 = s.Split('@'); //R@G@B@A
            int r = GetColorValue(a1[0]);
            int g = GetColorValue(a1[1]);
            int b = GetColorValue(a1[2]);
            int a = GetColorValue(a1[3]);
            var color = Color.FromArgb(a,r,g,b);
            return color;
        }
        private static int GetColorValue(string s)
        {
            return int.Parse(s);
        }
        public string font2string(Font f)
        {
            var cvt = new FontConverter();
            return cvt.ConvertToString(f);
        }
        public Pen string2pen(string s)
        {
            Pen p;
            try
            {
                string[] a1 = s.Split('#');
                string[] a2 = a1[0].Replace("]", "[").Split('[');
                p = new Pen(string2color(a1[0]), (float)Double.Parse(a1[1].Replace(',', '.'), CultureInfo.InvariantCulture));
                
            }
            catch
            {   
                p = new Pen(Color.Red, 2);
            }
            return p;
        }
        public Font string2font(string s)
        {
            var cvt = new FontConverter();
            return cvt.ConvertFromString(s) as Font;
        }
        public bool string2bool(string s)
        {
            if (s == null)
                return true;

            if (s.Equals("False"))
                return false;
            else
                return true;
        }
        public void saveSettings()
        {

            string execudetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            System.IO.Directory.CreateDirectory(execudetPath + @"\IFGPro");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(execudetPath + @"\IFGPro\settings.txt"))
            {
                //file.WriteLine("Marker\tSurface coordinate\tFringe index\tPressure\tMach\tVelocity\tDensity");
                file.WriteLine(pen2string(GlobalSettings.crossPen));
                file.WriteLine(pen2string(GlobalSettings.profilPen));
                file.WriteLine(pen2string(GlobalSettings.calibratePen));
                file.WriteLine(pen2string(GlobalSettings.selectedPen));
                file.WriteLine(pen2string(GlobalSettings.linesPen));
                file.WriteLine(font2string(GlobalSettings.fontLines));
                file.WriteLine(pen2string(GlobalSettings.indexBrush));
                file.WriteLine(font2string(GlobalSettings.fontDescription));
                file.WriteLine(pen2string(GlobalSettings.descriptionBrush));
                file.WriteLine(font2string(GlobalSettings.fontPoints));
                file.WriteLine(GlobalSettings.lineLength.ToString());

                file.WriteLine(GlobalSettings.desc.ToString());
                file.WriteLine(GlobalSettings.points.ToString());
                file.WriteLine(GlobalSettings.pointsDesc.ToString());
                file.WriteLine(GlobalSettings.lines.ToString());
                file.WriteLine(GlobalSettings.linesDesc.ToString());
                file.WriteLine(GlobalSettings.lineFringeNumber.ToString());
                file.WriteLine(GlobalSettings.profil.ToString());

                file.WriteLine(GlobalSettings.roundTime.ToString());
                file.WriteLine(GlobalSettings.roundPitch.ToString());
                file.WriteLine(GlobalSettings.roundPlunge.ToString());

                file.WriteLine(GlobalSettings.fringeLabels.ToString());
                file.WriteLine(GlobalSettings.fringeLabelsPanel.ToString());
                file.WriteLine(GlobalSettings.fringeCircleSize.ToString());
                file.WriteLine(GlobalSettings.fringeStep.ToString());
                file.WriteLine(color2string(GlobalSettings.fringeLabelsColor));
                file.WriteLine(font2string(GlobalSettings.fringeLabelsFont));
                file.WriteLine(GlobalSettings.fringeLabelsFrom.ToString());
                file.WriteLine(GlobalSettings.fringeLabelsTo.ToString());

                file.WriteLine(this.Width.ToString());
                file.WriteLine(this.Height.ToString());

                

            }

        }
        public void loadSettings()
        {
            string execudetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            if (File.Exists(execudetPath + @"\IFGPro\settings.txt"))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(execudetPath + @"\IFGPro\settings.txt"))
                {
                    //file.WriteLine("Marker\tSurface coordinate\tFringe index\tPressure\tMach\tVelocity\tDensity");
                    GlobalSettings.crossPen = string2pen(file.ReadLine());
                    GlobalSettings.profilPen = string2pen(file.ReadLine());
                    GlobalSettings.calibratePen = string2pen(file.ReadLine());
                    GlobalSettings.selectedPen = string2pen(file.ReadLine());
                    GlobalSettings.linesPen = string2pen(file.ReadLine());
                    GlobalSettings.fontLines = string2font(file.ReadLine());
                    GlobalSettings.indexBrush = string2pen(file.ReadLine());
                    GlobalSettings.fontDescription = string2font(file.ReadLine());
                    GlobalSettings.descriptionBrush = string2pen(file.ReadLine());
                    GlobalSettings.fontPoints = string2font(file.ReadLine());
                    GlobalSettings.lineLength = int.Parse(file.ReadLine());

                    GlobalSettings.desc = string2bool(file.ReadLine());
                    GlobalSettings.points = string2bool(file.ReadLine());
                    GlobalSettings.pointsDesc = string2bool(file.ReadLine());
                    GlobalSettings.lines = string2bool(file.ReadLine());
                    GlobalSettings.linesDesc = string2bool(file.ReadLine());
                    GlobalSettings.lineFringeNumber = string2bool(file.ReadLine());
                    GlobalSettings.profil = string2bool(file.ReadLine());

                    GlobalSettings.roundTime = int.Parse(file.ReadLine());
                    GlobalSettings.roundPitch = int.Parse(file.ReadLine());
                    GlobalSettings.roundPlunge = int.Parse(file.ReadLine());

                    GlobalSettings.fringeLabels = string2bool(file.ReadLine());
                    GlobalSettings.fringeLabelsPanel = string2bool(file.ReadLine());
                    GlobalSettings.fringeCircleSize = int.Parse(file.ReadLine());
                    GlobalSettings.fringeStep = float.Parse(file.ReadLine());
                    GlobalSettings.fringeLabelsColor = string2color(file.ReadLine());
                    GlobalSettings.fringeLabelsFont = string2font(file.ReadLine());
                    GlobalSettings.fringeLabelsFrom = float.Parse(file.ReadLine());
                    GlobalSettings.fringeLabelsTo = float.Parse(file.ReadLine());


                    try
                    {
                        this.Width = int.Parse(file.ReadLine());
                        this.Height = int.Parse(file.ReadLine())-20;
                    }
                    catch { }


                }
            }
        }
        private bool SureClose(string message, string title)
        {
            DialogResult dialogResult = MessageBox.Show(message, title, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        private string readLastUsed(string s)
        {
            string execudetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(execudetPath + @"\IFGPro\parameters.txt"))
                {
                    string testString;
                    while (file.Peek() >= 0)
                    {
                        testString = file.ReadLine();
                        if (testString.StartsWith(s))
                        {
                            return testString.Remove(0, s.Length);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Problem with opening parameters.txt \n"+e.Message);
            }

            return null;
        }
        private void writeLastUsed()
        {
            string execudetPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            System.IO.Directory.CreateDirectory(execudetPath + @"\IFGPro");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(execudetPath + @"\IFGPro\parameters.txt"))
            {
                file.WriteLine("t0:"+tb_t0.Text);
                file.WriteLine("p0:" + tb_p0.Text);
                file.WriteLine("dTau:" + tb_dTau.Text);
                file.WriteLine("heat:" + tb_heat.Text);
                file.WriteLine("wave:" + tb_wave.Text);
                file.WriteLine("L:" + tb_L.Text);
                file.WriteLine("R:" + tb_R.Text);
                file.WriteLine("K:" + tb_K.Text);
            }

        }
        public void expBit(string outputFileName)
        {
            int n = 3;

            //Bitmap tmpIma = new Bitmap(imageBox.Image.Width, imageBox.Image.Height);
            Bitmap tmpIma = new Bitmap(imageBox.Image, new Size(imageBox.Image.Width * n, imageBox.Image.Height*n));
            //outputFileName = "C:\\temp\\p.png";

            using (Graphics e = Graphics.FromImage(tmpIma))
                    {
                        GlobalSettings.profilPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                        e.SmoothingMode = SmoothingMode.AntiAlias;
                        // Crop and resize the image.
                        //Image description
                        if (GlobalSettings.desc)
                        {
                            double tmpD = imageBox.Zoom * GlobalSettings.fontDescription.Size / 100;
                            if (tmpD <= 0)
                                tmpD = 1;
                            Font fontDesc = new Font(GlobalSettings.fontDescription.FontFamily, (int)(tmpD * n));
                            images.getActual().sizeString = e.MeasureString(images.getActual().description, fontDesc);
                            e.DrawString(images.getActual().description, fontDesc,
                                GlobalSettings.descriptionBrush.Brush,
                                pOffset(images.getActual().locationDesc,n));
                        }

                        //Point fixed in image
                        if (listFixedInImage.Count > 0)
                        {
                            double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size / 100;
                            if (tmp <= 0)
                                tmp = 1;
                            Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp * n));
                            foreach (ImagePoint p in listFixedInImage)
                            {
                                if (GlobalSettings.points)
                                {
                                    p.DrawToGraphics(e, pOffset(p.location, n), n);
                                }
                                p.sizeString = e.MeasureString(p.label, fontPoints);
                                if (GlobalSettings.pointsDesc)
                                {
                                    e.DrawString(p.label, fontPoints, p.pen.Brush, pOffset(p.GetLabelLocation(imageBox), n));
                                }

                            }
                        }

                        if (!images.getActual().point1.IsEmpty())
                        {
                            if (isSelected.Equals("point1"))
                            {
                                e.DrawEllipse(GlobalSettings.selectedPen,
                                    pOffset(images.getActual().point1.Point, n).X - 4,
                                    pOffset(images.getActual().point1.Point, n).Y - 4,
                                    8,
                                    8);
                            }
                            images.getActual().point1.DrawToGraphics(e, pOffset(images.getActual().point1.Point, n));
                        }
                        if (!images.getActual().point2.IsEmpty())
                        {
                            if (isSelected.Equals("point2"))
                            {
                                e.DrawEllipse(GlobalSettings.selectedPen,
                                    pOffset(images.getActual().point2.Point, n).X - 4,
                                    pOffset(images.getActual().point2.Point, n).Y - 4,
                                    8,
                                    8);
                            }
                            images.getActual().point2.DrawToGraphics(e, pOffset(images.getActual().point2.Point, n));
                        }
                        if (!images.getActual().point2.IsEmpty() && !images.getActual().point1.IsEmpty())
                        {
                            if (GlobalSettings.profil)
                            {
                                if (isSelected.Equals("EA"))
                                {
                                    e.DrawLines(GlobalSettings.selectedPen, aOffset(images.getActual().getArrayProfile(), n));
                                }
                                else
                                {
                                    e.DrawLines(GlobalSettings.profilPen, aOffset(images.getActual().getArrayProfile(), n));
                                }
                            }

                            //Point fixed in object
                            if (listFixedInObject.Count > 0)
                            {
                                double tmp = imageBox.Zoom * GlobalSettings.fontPoints.Size / 100;
                                if (tmp < 1)
                                    tmp = 1;
                                Font fontPoints = new Font(GlobalSettings.fontPoints.FontFamily, (int)(tmp*n));
                                foreach (ObjectPoint p in listFixedInObject)
                                {
                                    if (GlobalSettings.points)
                                    {
                                        p.DrawToGraphics(e, pOffset(p.location, n),n);
                                    }
                                    p.sizeString = e.MeasureString(p.label, fontPoints);
                                    if (GlobalSettings.pointsDesc)
                                    {
                                        e.DrawString(p.label, fontPoints, p.pen.Brush, pOffset(p.GetLabelLocation(imageBox), n));
                                    }

                                }
                            }
                        }
                        if (!(images.getActual().listLines.Count == 0))
                        {
                            foreach (Line l in images.getActual().listLines)
                            {
                                if (GlobalSettings.lines)
                                {
                                    if (l.Equals(selectedLine))
                                        e.DrawLine(GlobalSettings.selectedPen,
                                            pOffset(l.pointOfProfile.Point, n),
                                            pOffset(l.pointOnProfile.Point, n));
                                    else
                                        e.DrawLine(GlobalSettings.linesPen,
                                            pOffset(l.pointOfProfile.Point, n),
                                            pOffset(l.pointOnProfile.Point, n));
                                }

                                Font fontLines = new Font(GlobalSettings.fontLines.FontFamily, (int)(GlobalSettings.fontLines.Size * n));
                                
                                if (GlobalSettings.linesDesc)
                                    e.DrawString(l.Index.ToString(), fontLines, GlobalSettings.indexBrush.Brush, pOffset(l.LocationIndexSuper(imageBox), n));
                            }
                        }
                    }
                

            tmpIma.Save(outputFileName);

        }
        private PointF pOffset(PointF p,int n)
        {
            return new PointF(p.X * n, p.Y * n);
        }
        private PointF[] aOffset(PointF[] a,int n)
        {
            List<PointF> ret = new List<PointF>();
            foreach (PointF p in a)
            {
                ret.Add(new PointF(p.X * n, p.Y * n));
            }
            return ret.ToArray();
        }
        private ImagePoint FringeLabelHit(Point click)
        {
            using (Graphics e = imageBox.CreateGraphics())
            {
                List<float> delkaRadku = new List<float>();
                delkaRadku.Add(0);
                int pocetRadku = 0;
                for (int i = 0; (GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep) <= GlobalSettings.fringeLabelsTo; ++i)
                {

                    if (delkaRadku[pocetRadku] + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width > graphicsWidth)
                    {
                        pocetRadku++;
                        delkaRadku.Add(0);
                    }

                    if(IsHitLabel(click,
                        e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont),
                        new PointF(delkaRadku[pocetRadku] + 3, 1 + (1.5f + e.MeasureString("123", GlobalSettings.fringeLabelsFont).Height) * pocetRadku))
                        )
                    {
                        //MessageBox.Show((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString());
                        ImagePoint pointTmp = new ImagePoint(new Pen(new SolidBrush(GlobalSettings.fringeLabelsColor)), imageBox.PointToImage(click));
                        pointTmp.label = (GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString();
                        
                        return pointTmp;
                    }

                    delkaRadku[pocetRadku] += (3 + e.MeasureString((GlobalSettings.fringeLabelsFrom + i * GlobalSettings.fringeStep).ToString(), GlobalSettings.fringeLabelsFont).Width);
                    
                }    
            }

            return null;
        }
        private static bool IsHitLabel(PointF click, SizeF size, PointF location)
        {
            SizeF sizeString = size;
            //click = i.GetOffsetPoint(p);
            //PointF offsetPoint = i.GetOffsetPoint(new PointF(location.X + labelOffsetX, location.Y + labelOffsetY));
            if (click.X < location.X + sizeString.Width
                && click.X > location.X
                && click.Y < location.Y + sizeString.Height
                && click.Y > location.Y)
                return true;
            return false;
        }
        
        #endregion

        #region Calculation profile

        private PointF[] profileNaca(PointF p1, PointF p2)
       {
           double d = (GetDistanceBetween(p1, p2) / Double.Parse(tb_real_length.Text.Replace(".", ","))) * 100;
            int distance = (int)d;

            System.Windows.Vector v1 = new System.Windows.Vector(p2.X - p1.X, p2.Y - p1.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(1, 0);
            double angleBetween = System.Windows.Vector.AngleBetween(v1, v2);
            angleBetween = angleBetween * Math.PI / 180;

            float sin = (float)Math.Sin(angleBetween);
            float cos = (float)Math.Cos(angleBetween);

            double xTmp, yTmp;
            float xMiddleBtwnAB =0;
            float yMiddleBtwnAB =0;
            cal_A = A;
            cal_B = B;
            if (!(A == null || B == null))
            {
                xTmp = A.X * cos + A.Y * sin; //rotating
                yTmp = -A.X * sin + A.Y * cos;
                xTmp = xTmp * d + p1.X; // mooving and scaling
                yTmp = yTmp * d + p1.Y;
                cal_A.X = (float)xTmp;
                cal_A.Y = (float)yTmp;

                xTmp = B.X * cos + B.Y * sin; //rotating
                yTmp = -B.X * sin + B.Y * cos;
                xTmp = xTmp * d + p1.X; // mooving and scaling
                yTmp = yTmp * d + p1.Y;
                cal_B.X = (float)xTmp;
                cal_B.Y = (float)yTmp;

                xMiddleBtwnAB = B.X * cos + ((A.Y - B.Y) / 2 + B.Y) * sin; //rotating
                yMiddleBtwnAB = -B.X * sin + ((A.Y - B.Y) / 2 + B.Y) * cos;
                xMiddleBtwnAB = (float)(xMiddleBtwnAB * d + p1.X); // mooving and scaling
                yMiddleBtwnAB = (float)(yMiddleBtwnAB * d + p1.Y);

                
            }

            
            List<PointF> testList = new List<PointF>();
            bool flagIfOverRealLenght = false;
            foreach (PointF p in nacaProfile)
            {
                if (p.X > A.X)
                {
                    if (!flagIfOverRealLenght)
                    {
                        testList.Add(cal_A);
                        testList.Add(new PointF(xMiddleBtwnAB, yMiddleBtwnAB));
                        testList.Add(new PointF(p1.X, p1.Y));
                        flagIfOverRealLenght = true;
                    }
                    continue;
                }

                xTmp = p.X * cos + p.Y * sin; //rotating
                yTmp = -p.X * sin + p.Y * cos;

                xTmp = xTmp * d + p1.X; // mooving and scaling
                yTmp = yTmp * d + p1.Y;
                testList.Add(new PointF((float)xTmp, (float)yTmp));
            }
            testList.Add(cal_B);
            testList.Add(new PointF(xMiddleBtwnAB, yMiddleBtwnAB));

            return testList.ToArray();

        }
        private PointF[] offsetPoints(PointF[] p)
    {
        List<PointF> list = new List<PointF>();

        foreach (PointF point in p)
        {
            list.Add(imageBox.GetOffsetPoint(point));
        }
        return list.ToArray();
    }
        private bool IsHitProfile(PointF[] array, PointF p)
        {
            if (array == null)
                return false;
            PointF fClosest = PointF.Empty;    //first closest
            double fDistance = double.MaxValue;
            PointF sClosest = PointF.Empty;    //second closest
            double sDistance = double.MaxValue;
            PointF tmpP;
            double tmpD;

            array = offsetPoints(array);
            foreach (PointF testPoint in array)
            {
                if (GetDistanceBetween(testPoint, p) < sDistance)
                {
                    sDistance = GetDistanceBetween(testPoint, p);
                    sClosest = testPoint;
                }
                if (GetDistanceBetween(testPoint, p) < fDistance)
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
            double distace = GetDistanceBetween(fClosest, sClosest);
            double Ca = ((fDistance * fDistance) - (sDistance * sDistance) + (distace * distace)) / (2 * distace);
            double Vc = Math.Sqrt((fDistance * fDistance) - (Ca * Ca));

            if (Vc < 5)
                return true;
            else
                return false;

        }
        private void MoveProfile(float diferenceX = 0, float diferenceY = 0)
            {
                int i;
                for (i = 0; i < arrayProfile.Length; i++)
                {
                    arrayProfile[i].X = (float)(arrayProfile[i].X - diferenceX);
                    arrayProfile[i].Y = (float)(arrayProfile[i].Y - diferenceY);
                }
                cal_A.X -= diferenceX;
                cal_A.Y -= diferenceY;
                cal_B.X -= diferenceX;
                cal_B.Y -= diferenceY;
            }


        #endregion       

       

        
    }   
}
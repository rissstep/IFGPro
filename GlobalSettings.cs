using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace IFGPro
{
    public static class GlobalSettings
    {
        static public Pen crossPen = new Pen(Color.Red);
        static public Pen profilPen = new Pen(Color.Yellow, 1.5f);
        static public Pen calibratePen = new Pen(Color.Red, 2);
        static public Pen selectedPen = new Pen(Color.Fuchsia, 2);
        static public Pen linesPen = new Pen(Color.Green, 2);
        static public Font fontLines = new Font("Thaoma", 11);
        static public Pen indexBrush = new Pen(Color.Green, 2);
        static public Font fontDescription = new Font("Thaoma", 11);
        static public Pen descriptionBrush = new Pen(Color.Red, 2);
        static public Font fontPoints = new Font("Thaoma", 11);
        static public int lineLength = 20;

        static public int roundTime = 4;
        static public int roundPitchPlunge = 2;
        static public int roundOthers = 2;

        static public bool desc = true;
        static public bool points = true;
        static public bool pointsDesc = true;
        static public bool lines = true;
        static public bool linesDesc = true; 
        static public bool lineFringeNumber = true;
        static public bool profil;

        // ratio = distancebetweencalibrationpoints/realdistance [px/mm]
        static public double ratio = Double.NaN;

        static public bool fringeLabels = false;
        static public bool fringeLabelsPanel = false;
        static public int fringeCircleSize = 3;
        static public float fringeStep = 1;
        static public Color fringeLabelsColor = Color.Green;
        static public Font fringeLabelsFont = new Font("Thaoma", 11);
        static public float fringeLabelsFrom = 0;
        static public float fringeLabelsTo = 1;

    }
}

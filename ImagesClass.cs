using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Cyotek.Windows.Forms;



namespace IFGPro
{

    [Serializable()]
    public class ImagesClass
    {
        private string[] imagesArray;                                   //array of all files in folder
        public List<MyImage> imagesList { set; get; }

        //[NonSerialized]
        public int pointer = 0;
        //[NonSerialized]                                       //pointer to image
        public int countImages = 0;
        //

        #region Save parameters - serializatione
        public float k;
        public float wave_lenght;
        public float L;
        public float R;
        public float K;
        public float t0;
        public float p0;
        public float tau0;
        public float dTau;
        public float w0;
        public float M;

        public PointF A;
        public PointF B;

        public Mark calibratePoint1;
        public Mark calibratePoint2;

        public Mark calibrateProfilePoint1;
        public Mark calibrateProfilePoint2;

        public double idealLength;
        public float PercentRealLenght;
        public double realLength;
        public double scale;
        public double ratio;

        public List<ObjectPoint> listFixedInObject = new List<ObjectPoint>();
        public List<ImagePoint> listFixedInImage = new List<ImagePoint>();

        public string path_airfoil;

        public PointF[] arrayProfile = null;

        public bool isEqualizer;
        public int gammaCorrection;

        public string tb_real;
        public string tb_ideal;

       

        #endregion

        public ImagesClass()
        { }
        public ImagesClass(string path, string extension)               //path to selected folder
        {
            imagesList = new List<MyImage>();                           //init list 

            imagesArray = Directory.GetFiles(path);                     //loading all files with same format like selected one
            foreach (string fileName in imagesArray)                    //filtering images
            {
                if (fileName.EndsWith(extension))
                {
                    imagesList.Add(new MyImage(fileName));
                }
            }
            countImages = imagesList.Count();
        }
        public MyImage getByPath(string path)
        {
            int i = 0;
            foreach (MyImage myImage in imagesList)
            {
                if (myImage.path.Equals(path))
                {
                    pointer = i;
                    return myImage;
                }
                i++;
            }
            return null;
        }
        public MyImage getActual()
        {
            return imagesList[pointer];
        }
        public MyImage getFirst()
        {
            pointer = 0;
            return imagesList[pointer]; 
        }
        public MyImage getLast()
        {
            pointer = countImages - 1;
            return imagesList[pointer];
        }
        public MyImage getNext()
        {
            if(pointer != countImages-1)
                pointer++;
            return imagesList[pointer];
        }
        public MyImage getPrevious()
        {
            if (pointer != 0)
                pointer--;
            return imagesList[pointer];
        }
        public MyImage getByIndex(int index)
        {
            pointer = index;

            if (pointer < 0)
                pointer = 0;
            if (pointer >= countImages)
                pointer = countImages - 1;

            return imagesList[pointer];
        }

        public MyImage get(int index)
        {
            if (index < 0)
                index = 0;
            if (index >= countImages)
                index = countImages - 1;

            return imagesList[index];
        }
        public Boolean isLast() 
        {
            if (pointer == countImages - 1)
                return true;
            else
                return false;
        }
        public Boolean isFirst()
        {
            if (pointer == 0)
                return true;
            else
                return false;
        }
        public int getPosition()
        {
            return pointer + 1;
        }
        public int getCount()
        {
            return countImages;
        }

    }
}

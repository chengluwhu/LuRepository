using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractFeaturePts
{
    public class FeaPtInform
    {
        private double elev; 

        public double Elev
        {
            get { return elev; }
            set { elev = value; }
        }
        private int ptAtNumber; 
        public int PtAtNumber
        {
            get { return ptAtNumber; }
            set { ptAtNumber = value; }
        }
        private int ptAtPlyOid; 

        public int PtAtPlyOid
        {
            get { return ptAtPlyOid; }
            set { ptAtPlyOid = value; }
        }
        private IPoint ptCoord; 

        public IPoint PtCoord
        {
            get { return ptCoord; }
            set { ptCoord = value; }
        }
        private bool isAffluent; 

        public bool IsAffluent
        {
            get { return isAffluent; }
            set { isAffluent = value; }
        }
        private double curvValues; 

        public double CurvValues
        {
            get { return curvValues; }
            set { curvValues = value; }
        }
        private int isContanSameZTerlk;

        public int IsContanSameZTerlk
        {
            get { return isContanSameZTerlk; }
            set { isContanSameZTerlk = value; }
        }
        private int mark = 0;

        public int Mark
        {
            get { return mark; }
            set { mark = value; }
        }
    }
}

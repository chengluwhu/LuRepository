using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractFeaturePts
{
   public class conFeaInfor
    {
        private double elev;

        public double Elev
        {
            get { return elev; }
            set { elev = value; }
        }
        private double area;

        public double Area
        {
            get { return area; }
            set { area = value; }
        }
        private IFeature conFea;

        public IFeature ConFea
        {
            get { return conFea; }
            set { conFea = value; }
        }
        private IPolygon polygonOfCon;

        public IPolygon PolygonOfCon
        {
            get { return polygonOfCon; }
            set { polygonOfCon = value; }
        }
    }
}

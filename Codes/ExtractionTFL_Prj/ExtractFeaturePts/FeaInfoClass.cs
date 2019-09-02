using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractFeaturePts
{
    public  class FeaInfoClass
    {
        private IFeature conFea;

        public IFeature ConFea
        {
            get { return conFea; }
            set { conFea = value; }
        }
        private double z;

        public double Z
        {
            get { return z; }
            set { z = value; }
        }
    }
}

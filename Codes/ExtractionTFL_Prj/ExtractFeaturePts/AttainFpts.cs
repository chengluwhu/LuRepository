using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractFeaturePts
{
   public  class AttainFpts
    {
       
       static public void CreateFPtByDictionary(IFeatureLayer featurePtLyr, int terlkFid, IPoint newFpt, int locAtCounter, double c)
        {
                          
                int sMark = 0;
                if (c < 0)
                {
                    sMark = -1;
                }
                else if (c > 0)
                {
                    if (c==2)
                    {
                        sMark = 2;
                    }
                    else
                    {
                        sMark = 1;
                    }                    
                }
                else if (c == 0)
                {
                    sMark = 0;
                }
                
                CreateFeaturePt(featurePtLyr, terlkFid, newFpt, locAtCounter, sMark, c);            

        }

         
       static public void CreateFeaturePt(IFeatureLayer featurePtLyr, int terlkFid, IPoint newFpt, int location, int sMark, double c)
        {

            int elev = featurePtLyr.FeatureClass.FindField("Elev");
            int terlkAtFid = featurePtLyr.FeatureClass.FindField("TerlkFID");
            int whereAtCou = featurePtLyr.FeatureClass.FindField("WhereAtCou");
            int mark = featurePtLyr.FeatureClass.FindField("Mark");
            int cur = featurePtLyr.FeatureClass.FindField("Curve");

            IFeatureBuffer featureBuffer = featurePtLyr.FeatureClass.CreateFeatureBuffer();
            IFeatureCursor featureCursor = featurePtLyr.FeatureClass.Insert(true);
            IZAware pZAware = (IZAware)newFpt;
            pZAware.ZAware = true;
            featureBuffer.Shape = newFpt;                      
            featureBuffer.set_Value(terlkAtFid, terlkFid);
            featureBuffer.set_Value(elev, Math.Round(newFpt.Z,2));
            featureBuffer.set_Value(whereAtCou, location);
            featureBuffer.set_Value(mark, sMark);
            featureBuffer.set_Value(cur, c);
            featureCursor.InsertFeature(featureBuffer);
            featureCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
        }

    }
}

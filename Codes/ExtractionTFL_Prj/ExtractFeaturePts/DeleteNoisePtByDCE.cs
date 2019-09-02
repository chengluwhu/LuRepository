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
   public   class DeleteNoisePtByDCE
    {
        /// /////Using（Discrete Curve Evolution ,DCE）algorithm 
        static public double onePtThreshold = 4;
        static public double TwoPtThreshold = 7;
        static public void SurplesFptsByDCE(IFeatureLayer feaPtFyr, IFeatureLayer terlkFyr, IFeatureLayer newFeaPtFyr)
        {
            
            SortFeaturePtsAndTerlks.GetFeaturePt(feaPtFyr);
            HowToDelete( SortFeaturePtsAndTerlks.sortedValleyPts, terlkFyr, newFeaPtFyr);
            HowToDelete(SortFeaturePtsAndTerlks.sortedRidgePts, terlkFyr, newFeaPtFyr);
            for (int i = 0; i < SortFeaturePtsAndTerlks.sortedPeakPts.Count; i++)
            {
                 FeaPtInform fpt = SortFeaturePtsAndTerlks.sortedPeakPts[i];
                 
               AttainFpts.CreateFeaturePt(newFeaPtFyr, fpt.PtAtPlyOid, fpt.PtCoord, fpt.PtAtNumber,0, 0);
            }
            for (int i = 0; i < SortFeaturePtsAndTerlks.sortedSaddlePts.Count; i++)
            {
                  FeaPtInform fpt = SortFeaturePtsAndTerlks.sortedSaddlePts[i];
                  AttainFpts.CreateFeaturePt(newFeaPtFyr, fpt.PtAtPlyOid, fpt.PtCoord, fpt.PtAtNumber, 2, 0);                 
            }
        }
       static private void HowToDelete(List<FeaPtInform> vaOrRiPtList, IFeatureLayer terlkFyr, IFeatureLayer newFeaPtFyr)
       {
           List<FeaPtInform> valleyOrRidgeList = new List<FeaPtInform>();
           List<List<FeaPtInform>> groups = new List<List<FeaPtInform>>();
           for (int i = 0; i < vaOrRiPtList.Count - 1; i++)
           {
               double dce = 0;
               FeaPtInform fptInform_1 = vaOrRiPtList[i];
               FeaPtInform fptInform_2 = vaOrRiPtList[i + 1];
               double D = Math.Sqrt(Math.Pow(fptInform_1.PtCoord.X - fptInform_2.PtCoord.X, 2) + Math.Pow(fptInform_1.PtCoord.Y - fptInform_2.PtCoord.Y, 2));
               if (fptInform_2.PtAtNumber - fptInform_1.PtAtNumber == 1 && fptInform_2.Elev == fptInform_1.Elev && fptInform_1.PtAtPlyOid == fptInform_2.PtAtPlyOid && D < 27)
               {
                   valleyOrRidgeList.Add(fptInform_1);
                   if (i == vaOrRiPtList.Count - 2)
                   {
                       valleyOrRidgeList.Add(vaOrRiPtList[i + 1]);
                       groups.Add(valleyOrRidgeList);
                       if (valleyOrRidgeList.Count < 3)
                       {
                           if (valleyOrRidgeList.Count == 1)
                           {
                               FeaPtInform fpt = valleyOrRidgeList[0];
                               dce = DCEFun(fpt, terlkFyr);
                               if (dce < onePtThreshold)
                               {
                                   groups.Remove(valleyOrRidgeList);
                               }
                           }
                           else
                           {
                               for (int j = 0; j < valleyOrRidgeList.Count; j++)
                               {
                                   dce += DCEFun(valleyOrRidgeList[j], terlkFyr);
                               }
                               if (Math.Abs(dce) < TwoPtThreshold)
                               {
                                   groups.Remove(valleyOrRidgeList);
                               }
                           }
                       }
                   }
               }
               else
               {
                   valleyOrRidgeList.Add(fptInform_1);
                   groups.Add(valleyOrRidgeList);
                   if (valleyOrRidgeList.Count < 3)
                   {
                       if (valleyOrRidgeList.Count == 1)
                       {
                           FeaPtInform fpt = valleyOrRidgeList[0];
                           dce = DCEFun(fpt, terlkFyr);
                           if (dce < onePtThreshold)
                           {
                               groups.Remove(valleyOrRidgeList);
                           }
                       }
                       else
                       {

                           for (int j = 0; j < valleyOrRidgeList.Count; j++)
                           {
                               dce += DCEFun(valleyOrRidgeList[j], terlkFyr);
                           }
                           if (Math.Abs(dce) < TwoPtThreshold)
                           {
                               groups.Remove(valleyOrRidgeList);
                           }
                       }
                   }                  
                   valleyOrRidgeList = new List<FeaPtInform>();
               }
           }
           for (int i = 0; i < groups.Count; i++)
           {
               List<FeaPtInform> list = groups[i];
               for (int j = 0; j < list.Count; j++)
               {
                   FeaPtInform fpt = list[j];
                   AttainFpts.CreateFPtByDictionary(newFeaPtFyr, fpt.PtAtPlyOid, fpt.PtCoord, fpt.PtAtNumber, fpt.CurvValues);
               }
           }
       
       
       }      
       static private double DCEFun(FeaPtInform fpt, IFeatureLayer terlkFyr)
        {
            IFeature feature = terlkFyr.FeatureClass.GetFeature(fpt.PtAtPlyOid);
            IPointCollection ptCol = feature.Shape as IPointCollection;
            IPoint pt1 = ptCol.get_Point(fpt.PtAtNumber - 1);
            IPoint pt2 = ptCol.get_Point(fpt.PtAtNumber);
            IPoint pt3 = ptCol.get_Point(fpt.PtAtNumber + 1);
            ILine ply = new LineClass();
            ply.FromPoint = pt1;
            ply.ToPoint = pt2;
            IPolyline ply1 = new PolylineClass();
            ply1.FromPoint = pt2;
            ply1.ToPoint = pt3;
            double turnAngle =CalCurvature.GetCurvature(pt1, pt2, pt3);
            double dce = Math.Abs(turnAngle) * ply.Length * ply1.Length / (ply.Length + ply1.Length);
            return dce;
        }


    }
}

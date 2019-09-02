using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
 
namespace ExtractFeaturePts
{
    public class SortFeaturePtsAndTerlks
    {
        static public List<FeaPtInform> sortedValleyPts = new List<FeaPtInform>(); 
        static public List<FeaPtInform> sortedRidgePts = new List<FeaPtInform>(); 
        static public List<FeaPtInform> sortedPeakPts = new List<FeaPtInform>(); 
        static public List<FeaPtInform> sortedSaddlePts = new List<FeaPtInform>(); 
        static public Dictionary<IFeature, double> openTerlkStored = new Dictionary<IFeature, double>();
        static public Dictionary<IFeature, double> closedTerlkStored = new Dictionary<IFeature, double>();
         
        static public void GetTerlkFun(IFeatureLayer terlkFyr)
        {
            Dictionary<IFeature, double> openTerlkDic = new Dictionary<IFeature, double>();
            Dictionary<IFeature, double> closedTerlkDic = new Dictionary<IFeature, double>();
            IFeatureClass fClass = terlkFyr.FeatureClass;
            IFeatureCursor fCursor = fClass.Search(null, false);
            IFeature pFeature = fCursor.NextFeature();
            while (pFeature != null)
            {
                
                int zIndex = pFeature.Fields.FindField("Elev");
                double z = (double)pFeature.get_Value(zIndex);

                
                int markIndex = pFeature.Fields.FindField("Mark");
                short mark = (short)pFeature.get_Value(markIndex);
                if (mark == 0)
                {
                    closedTerlkDic.Add(pFeature, z);
                }
                else
                {
                    openTerlkDic.Add(pFeature, z);
                }
                pFeature = fCursor.NextFeature();
            }
            var dicSort_1 = from objDic in openTerlkDic orderby objDic.Value ascending select objDic;
            foreach (KeyValuePair<IFeature, double> keyv in dicSort_1)
            {
                openTerlkStored.Add(keyv.Key, keyv.Value);
            }
            var dicSort = from objDic in closedTerlkDic orderby objDic.Value ascending select objDic;
            foreach (KeyValuePair<IFeature, double> keyv in dicSort)
            {
                closedTerlkStored.Add(keyv.Key, keyv.Value);
            }
        }

         
        static public void GetFeaturePt(IFeatureLayer feaPtFyr)
        {
            List<FeaPtInform> valleyPtList = new List<FeaPtInform>(); 
            List<FeaPtInform> ridgePtList = new List<FeaPtInform>(); 
            List<FeaPtInform> peakPtList = new List<FeaPtInform>(); 
            List<FeaPtInform> saddlePtList = new List<FeaPtInform>(); 
            FeaPtInform peakPtClass = null;
            FeaPtInform ridgePtClass = null;
            FeaPtInform valleyPtClass = null;
            FeaPtInform saddlePtClass = null;
            IFeatureClass ptFClass = feaPtFyr.FeatureClass;
            IFeatureCursor ptFCursor = ptFClass.Search(null, false);
            IFeature ptFeature = ptFCursor.NextFeature();
            while (ptFeature != null)
            {
                 
                int markIndex = ptFeature.Fields.FindField("Mark");
                int mark = (int)ptFeature.get_Value(markIndex);
                
                int zIndex = ptFeature.Fields.FindField("Elev");
                double z = (double)ptFeature.get_Value(zIndex);
                z = Math.Round(z, 0);
                 
                int pNumberIndex = ptFeature.Fields.FindField("WhereAtcou");
                Int32 number = (Int32)ptFeature.get_Value(pNumberIndex);
                        
                int oid = ptFeature.Fields.FindField("TerlkFID");
                Int32 lineID = (Int32)ptFeature.get_Value(oid);
                 
                int curveIndex = ptFeature.Fields.FindField("Curve");
                double curve = (double)ptFeature.get_Value(curveIndex);
                IPoint pt = new PointClass();
                pt = ptFeature.Shape as IPoint;
                if (mark == 1)
                {
                    ridgePtClass = new FeaPtInform();
                    ridgePtClass.Elev = z;
                    ridgePtClass.PtAtPlyOid = lineID;
                    ridgePtClass.PtAtNumber = number;
                    ridgePtClass.PtCoord = pt;
                    ridgePtClass.CurvValues = curve;
                    ridgePtClass.Mark = mark;
                    ridgePtList.Add(ridgePtClass);

                }
                else if (mark == -1)
                {
                    valleyPtClass = new FeaPtInform();
                    valleyPtClass.Elev = z;
                    valleyPtClass.PtAtPlyOid = lineID;
                    valleyPtClass.PtAtNumber = number;
                    valleyPtClass.PtCoord = pt;
                    valleyPtClass.CurvValues = curve;
                    valleyPtClass.Mark = mark;
                    valleyPtList.Add(valleyPtClass);

                }
                else if (mark == 0)
                {
                    peakPtClass = new FeaPtInform();
                    peakPtClass.Elev =Math.Round(z,1);
                    peakPtClass.PtAtPlyOid = lineID;
                    peakPtClass.PtCoord = pt;
                    peakPtClass.Mark = mark;
                    peakPtList.Add(peakPtClass);
                }
                else if (mark == 2)
                {
                    saddlePtClass = new FeaPtInform();
                    saddlePtClass.PtCoord = pt;
                    saddlePtClass.Mark = mark;
                    saddlePtClass.Elev = Math.Round(z, 1); ;
                    saddlePtList.Add(saddlePtClass);
                }
                ptFeature = ptFCursor.NextFeature();
            }
            sortedValleyPts = valleyPtSort(valleyPtList);
            sortedRidgePts = RidgePtSort(ridgePtList);
            sortedPeakPts = PeakOrSaddlePtSort(peakPtList);
            sortedSaddlePts = PeakOrSaddlePtSort(saddlePtList);
        }

         
        static private List<FeaPtInform> valleyPtSort(List<FeaPtInform> valPtList)
        {
            List<FeaPtInform> featList1 = new List<FeaPtInform>();
            for (int i = 0; i < valPtList.Count - 1; i++)
            {
                FeaPtInform fClass = new FeaPtInform();
                for (int j = 0; j < valPtList.Count - 1; j++)
                {
                    if (valPtList[j + 1].Elev == valPtList[j].Elev && valPtList[j + 1].PtAtPlyOid == valPtList[j].PtAtPlyOid && valPtList[j].PtAtNumber > valPtList[j + 1].PtAtNumber)
                    {
                        fClass = valPtList[j + 1];
                        valPtList[j + 1] = valPtList[j];
                        valPtList[j] = fClass;

                    }
                    else if (valPtList[j + 1].Elev < valPtList[j].Elev)
                    {

                        fClass = valPtList[j + 1];
                        valPtList[j + 1] = valPtList[j];
                        valPtList[j] = fClass;

                    }

                }

            }
            return featList1 = valPtList;

        }

         
        static public List<FeaPtInform> RidgePtSort(List<FeaPtInform> ridgePtList)
        {

            List<FeaPtInform> RList = new List<FeaPtInform>();
            for (int i = 0; i < ridgePtList.Count - 1; i++)
            {
                FeaPtInform fClass = new FeaPtInform();
                for (int j = 0; j < ridgePtList.Count - 1; j++)
                {
                    if (ridgePtList[j + 1].Elev == ridgePtList[j].Elev && ridgePtList[j + 1].PtAtPlyOid == ridgePtList[j].PtAtPlyOid && ridgePtList[j].PtAtNumber > ridgePtList[j + 1].PtAtNumber)
                    {
                        fClass = ridgePtList[j + 1];
                        ridgePtList[j + 1] = ridgePtList[j];
                        ridgePtList[j] = fClass;

                    }
                    else if (ridgePtList[j + 1].Elev > ridgePtList[j].Elev)
                    {

                        fClass = ridgePtList[j + 1];
                        ridgePtList[j + 1] = ridgePtList[j];
                        ridgePtList[j] = fClass;

                    }

                }

            }
            return RList = ridgePtList;
        }

         
        static private List<FeaPtInform> PeakOrSaddlePtSort(List<FeaPtInform> PeakPtList)
        {
            List<FeaPtInform> RList = new List<FeaPtInform>();
            for (int i = 0; i < PeakPtList.Count - 1; i++)
            {
                FeaPtInform fClass = new FeaPtInform();
                for (int j = 0; j < PeakPtList.Count - 1; j++)
                {
                    if (PeakPtList[j + 1].Elev < PeakPtList[j].Elev)
                    {
                        fClass = PeakPtList[j + 1];
                        PeakPtList[j + 1] = PeakPtList[j];
                        PeakPtList[j] = fClass;
                    }

                }

            }
            return RList = PeakPtList;
        }
    }
}
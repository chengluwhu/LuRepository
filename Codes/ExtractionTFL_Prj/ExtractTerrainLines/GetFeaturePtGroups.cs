using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ExtractFeaturePts;
namespace ExtractTerrainLines
{
    public class GetFeaturePtGroups
    {
        static public List<List<FeaPtInform>> allValleyFeaPtGroups = new List<List<FeaPtInform>>(); 
        static public List<List<FeaPtInform>> allRidgeFeaPtGroups = new List<List<FeaPtInform>>(); 

        static public void GetValleyOrRidgeFeaPtGroups(IFeatureLayer feaPtFyr)
        {
            SortFeaturePtsAndTerlks.GetFeaturePt(feaPtFyr); 
            List<FeaPtInform> valleyOrRidgeList = new List<FeaPtInform>();    
            FeaPtInform fptInform_1 =new FeaPtInform ();
            FeaPtInform fptInform_2 = new FeaPtInform();
            double dis = 0;
            for (int i = 0; i < SortFeaturePtsAndTerlks.sortedValleyPts.Count-1; i++) 
            {
                fptInform_1 = SortFeaturePtsAndTerlks.sortedValleyPts[i];
                fptInform_2 = SortFeaturePtsAndTerlks.sortedValleyPts[i + 1];
                dis = Math.Sqrt(Math.Pow(fptInform_1.PtCoord.X - fptInform_2.PtCoord.X, 2) + Math.Pow(fptInform_1.PtCoord.Y - fptInform_2.PtCoord.Y, 2));
                if (fptInform_2.PtAtNumber - fptInform_1.PtAtNumber == 1 && fptInform_2.Elev == fptInform_1.Elev && fptInform_1.PtAtPlyOid == fptInform_2.PtAtPlyOid && dis < 27)
                {
                    valleyOrRidgeList.Add(fptInform_1);
                    if (i == SortFeaturePtsAndTerlks.sortedValleyPts.Count - 2)
                    {
                        valleyOrRidgeList.Add(SortFeaturePtsAndTerlks.sortedValleyPts[i + 1]);
                        allValleyFeaPtGroups.Add(valleyOrRidgeList);
                    }
                }
                else
                {
                    valleyOrRidgeList.Add(fptInform_1);
                    allValleyFeaPtGroups.Add(valleyOrRidgeList);
                    valleyOrRidgeList = new List<FeaPtInform>();
                }
            }
            valleyOrRidgeList = new List<FeaPtInform>();
            for (int i = 0; i < SortFeaturePtsAndTerlks.sortedRidgePts.Count-1; i++) 
            {
                fptInform_1 = SortFeaturePtsAndTerlks.sortedRidgePts[i];
                fptInform_2 = SortFeaturePtsAndTerlks.sortedRidgePts[i + 1];
                dis = Math.Sqrt(Math.Pow(fptInform_1.PtCoord.X - fptInform_2.PtCoord.X, 2) + Math.Pow(fptInform_1.PtCoord.Y - fptInform_2.PtCoord.Y, 2));
                if (fptInform_2.PtAtNumber - fptInform_1.PtAtNumber == 1 && fptInform_2.Elev == fptInform_1.Elev && fptInform_1.PtAtPlyOid == fptInform_2.PtAtPlyOid && dis < 27)
                {
                    valleyOrRidgeList.Add(fptInform_1);
                    if (i == SortFeaturePtsAndTerlks.sortedRidgePts.Count - 2)
                    {
                        valleyOrRidgeList.Add(SortFeaturePtsAndTerlks.sortedRidgePts[i + 1]);
                        allRidgeFeaPtGroups.Add(valleyOrRidgeList);
                    }
                }
                else
                {
                    valleyOrRidgeList.Add(fptInform_1);
                    allRidgeFeaPtGroups.Add(valleyOrRidgeList);
                    valleyOrRidgeList = new List<FeaPtInform>();
                }            
            }
        }
    }
}

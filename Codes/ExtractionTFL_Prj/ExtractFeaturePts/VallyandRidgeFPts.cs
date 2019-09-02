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
    public class VallyandRidgeFPts
    {
        static public double thresholdOfCurvature = 0.004;
        static public void GetValleyAndRidgePts(IFeatureLayer fyr, IFeatureLayer newFeaPtLyr)
        {
            Dictionary<int, double> fPtAtTerlkList = new Dictionary<int, double>();
            IFeatureClass fclass = fyr.FeatureClass;
            IFeatureCursor pfecur = fclass.Search(null, false);
            IFeature pFeature = pfecur.NextFeature();
            while (pFeature != null)
            {
                fPtAtTerlkList = new Dictionary<int, double>();
                IPointCollection ptCol = pFeature.Shape as IPointCollection;
                Dictionary<int, double> InitialCValuesSetted = new Dictionary<int, double>();
                double C_1 = 0;
                for (int i = 0; i < ptCol.PointCount - 3; i++)
                {
                    IPoint fristPt = ptCol.get_Point(i);
                    IPoint secondPt = ptCol.get_Point(i + 1);
                    IPoint thridPt = ptCol.get_Point(i + 2);
                    ILine ply = new LineClass();
                    ply.FromPoint = fristPt;
                    ply.ToPoint = secondPt;
                    IPolyline ply1 = new PolylineClass();
                    ply1.FromPoint = secondPt;
                    ply1.ToPoint = thridPt;
                    double C = CalCurvature.GetCurvature(fristPt, secondPt, thridPt);
                    if (C_1 == 0)
                    {
                        C_1 = ply.Angle;
                    }
                    double D = 0;
                    if (C < 0)
                    {
                        D = -(Math.Abs(C) + Math.Abs(C_1)) / (ply.Length + ply1.Length);
                    }
                    else
                    {
                        D = (Math.Abs(C) + Math.Abs(C_1)) / (ply.Length + ply1.Length);
                    }
                    if (Math.Abs(D) > thresholdOfCurvature)
                    {
                        fPtAtTerlkList.Add(i + 1, D);
                    }
                    C_1 = C;
                }
                for (int j = 0; j < fPtAtTerlkList.Count; j++)
                {
                    int index = fPtAtTerlkList.Keys.ElementAt(j);
                    IPoint newPt = ptCol.get_Point(index);
                    double curve = fPtAtTerlkList.Values.ElementAt(j);
                    AttainFpts.CreateFPtByDictionary(newFeaPtLyr, pFeature.OID, newPt, index, curve);
                }
                pFeature = pfecur.NextFeature();
            }            
        }
    }
}

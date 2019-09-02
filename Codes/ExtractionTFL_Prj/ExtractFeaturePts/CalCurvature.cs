using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractFeaturePts
{
   public class CalCurvature
   {
       static public double teamValue;
       static public Dictionary<int, double> GetInitialCValues(IFeature pFeature)
       {
           IPointCollection ptCol = pFeature.Shape as IPointCollection;
           Dictionary<int, double> InitialCValuesSetted = new Dictionary<int, double>();
           double v = 0;
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
               double c1 = GetCurvature(fristPt, secondPt, thridPt);
               if (v == 0)
               {
                   v = ply.Angle;
               }
               double D = 0;
               if (c1 < 0)
               {
                   D = -(Math.Abs(c1) + Math.Abs(v)) / (ply.Length + ply1.Length);
               }
               else
               {
                   D = (Math.Abs(c1) + Math.Abs(v)) / (ply.Length + ply1.Length);
               }
               if (Math.Abs(D) > teamValue)
               {
                   InitialCValuesSetted.Add(i + 1, D);
               }
               v = c1;
           }           
           return InitialCValuesSetted;
       }
        
      static public double GetCurvature(IPoint firstPt, IPoint secondPt, IPoint thirdPt)
       {
           IPolyline ply = new PolylineClass();
           ply.FromPoint = firstPt;
           ply.ToPoint = secondPt;
           IPolyline ply1 = new PolylineClass();
           ply1.FromPoint = secondPt;
           ply1.ToPoint = thirdPt;
           IConstructPoint contructionPoint = new PointClass();
           contructionPoint.ConstructAlong(ply, esriSegmentExtension.esriExtendAtTo, 5, true);
           IPoint extendPt = contructionPoint as IPoint;
           ILine line = new LineClass();
           line.FromPoint = secondPt;
           line.ToPoint = extendPt;
           double beforeAnge = line.Angle;
           if (beforeAnge < 0) 
           {
               beforeAnge = beforeAnge + 2 * Math.PI;
           }
           ILine afterLine = new LineClass();
           afterLine.FromPoint = secondPt;
           afterLine.ToPoint = thirdPt;
           double afterAngle = afterLine.Angle;
           if (afterAngle < 0) 
           {
               afterAngle = afterAngle + 2 * Math.PI;
           }
           double diferenceAngle = afterAngle - beforeAnge;
           if (Math.Abs(diferenceAngle) > Math.PI) 
           {
               if (diferenceAngle < 0) 
               {
                   diferenceAngle = diferenceAngle + 2 * Math.PI;
               }
               else 
               {
                   diferenceAngle = diferenceAngle - 2 * Math.PI;
               }
           }              
           return diferenceAngle;
       }
    }
}

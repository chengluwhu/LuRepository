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
  public  class PublicFunctionClass
    {
      static public double intervalValue = 10;
      
        
        static public List<IPolyline> CreatePerpendicularLine(FeaPtInform fptClass, IFeatureLayer plyFeatureLyr, bool isValOrRige)
        {
            IPoint pt1 = CreateAngleBisectorPt(fptClass, plyFeatureLyr, isValOrRige);
            ILine AngleBisectorLine = new LineClass();
            AngleBisectorLine.FromPoint = fptClass.PtCoord;
            AngleBisectorLine.ToPoint = pt1;
            
            IConstructPoint constructPt = new PointClass();
            double m = 100;
            if (isValOrRige == true)
            {
                m = 150;
            }
            constructPt.ConstructPerpendicular(AngleBisectorLine as ISegment, esriSegmentExtension.esriNoExtension, AngleBisectorLine.FromPoint, m, false);
            IPoint newpt = constructPt as IPoint;
            ILine beforeLine = new LineClass();
            beforeLine.FromPoint = fptClass.PtCoord;
            beforeLine.ToPoint = newpt;
            IPolyline leftPly = new PolylineClass();
            leftPly.FromPoint = fptClass.PtCoord;
            leftPly.ToPoint = newpt;
            
            IPolyline temperply = new PolylineClass();
            temperply = ExtendLineFun(beforeLine.FromPoint, beforeLine.Angle - Math.PI);

            IPolyline rightPly = new PolylineClass();
            rightPly.FromPoint = fptClass.PtCoord;
            rightPly.ToPoint = temperply.ToPoint;
            List<IPolyline> buildPerpendicularLine = new List<IPolyline>();
            buildPerpendicularLine.Add(leftPly);
            buildPerpendicularLine.Add(rightPly);
            return buildPerpendicularLine;

        }
        
        static public IPolyline ExtendLineFun(IPoint pt1, double angle)
        {
            IConstructPoint extendPt = new PointClass();
            extendPt.ConstructAngleDistance(pt1, angle, 150);
            IPoint newPt = extendPt as IPoint;
            newPt.Z = 10;
            IPolyline extendPly = new PolylineClass();
            extendPly.FromPoint = pt1;
            extendPly.ToPoint = newPt;
            return extendPly;
        }
        
        static public IPoint CreateAngleBisectorPt(FeaPtInform fptClass, IFeatureLayer flyr, bool isValOrRige)
        {
            IConstructPoint constructPt = new PointClass();
            IPointCollection ptCOl = flyr.FeatureClass.GetFeature(fptClass.PtAtPlyOid).Shape as IPointCollection;
            if (isValOrRige == true)
            {
                constructPt.ConstructAngleBisector(ptCOl.get_Point(fptClass.PtAtNumber - 1), ptCOl.get_Point(fptClass.PtAtNumber), ptCOl.get_Point(fptClass.PtAtNumber + 1),7.8, false);
            }
            else
            {
                constructPt.ConstructAngleBisector(ptCOl.get_Point(fptClass.PtAtNumber + 1), ptCOl.get_Point(fptClass.PtAtNumber), ptCOl.get_Point(fptClass.PtAtNumber - 1),7.8, true);
            }
            IPoint newpt = new PointClass();
            newpt = constructPt as IPoint;
            newpt.Z = 10;
            return newpt;
        }
        static public  IPolyline CreatePly(List<IPoint> ptList)
        {
            ISegmentCollection sc = new PathClass();
            for (int i = 0; i < ptList.Count - 1; i++)
            {
                ILine newline = new LineClass();                             
                newline.PutCoords(ptList[i], ptList[i + 1]);
                sc.AddSegment(newline as ISegment, Type.Missing, Type.Missing);
            }
            IPolyline pline = new PolylineClass();
            IGeometryCollection gPolyline = pline as IGeometryCollection;
            gPolyline.AddGeometry(sc as IGeometry, Type.Missing, Type.Missing);
            pline = gPolyline as IPolyline;
            return pline;
        }
        
        static public void CreateStructPly(IFeatureLayer featurePtLyr, IPolyline structPly, int cd,bool isValOrRidge)
        {
            int mark = featurePtLyr.FeatureClass.FindField("Mark");
            int code = featurePtLyr.FeatureClass.FindField("Code");
            IFeatureBuffer featureBuffer = featurePtLyr.FeatureClass.CreateFeatureBuffer();
            IFeatureCursor featureCursor = featurePtLyr.FeatureClass.Insert(true);
            IZAware pZAware = (IZAware)structPly;
            pZAware.ZAware = true;
            featureBuffer.Shape = structPly;
            if (isValOrRidge == true)
            {
                featureBuffer.set_Value(mark, -1);
                featureBuffer.set_Value(code, cd);
            }
            else
            {
                featureBuffer.set_Value(mark, 1);
                featureBuffer.set_Value(code, cd);
            }
            featureCursor.InsertFeature(featureBuffer);
            featureCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
        }       

    }
}

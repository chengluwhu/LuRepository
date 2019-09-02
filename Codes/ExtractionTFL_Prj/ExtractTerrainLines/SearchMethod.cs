using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ExtractFeaturePts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTerrainLines
{
    public class SearchMethod
    {

        static double K = 0;
        static ILine newLine = new LineClass();
        static double leNew = 0;
        static double leOld = 0;
        static IPoint fristPt = new PointClass();
        static IPoint secondPt = new PointClass();
        static List<IPoint> ptList = new List<IPoint>();
        static int startStep = 0;
        static double adjustiveAngle = 5;
        static int direction = 0;
      
        static public IPolyline intelligentSearchMethod(IFeatureLayer terlkFyr, FeaPtInform fptInfo, IPolyline leftPly, IPolyline rightPly, IGeometry geometry, int isSaddOrGeneral)
        {
            ptList = new List<IPoint>();
            FeaPtInform newFptClass = new FeaPtInform();
            fristPt = fptInfo.PtCoord;
            if (isSaddOrGeneral == 0) 
            {
                ptList.Add(fptInfo.PtCoord);
                IPointCollection terlkPtc = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid).Shape as IPointCollection;
                IConstructPoint constructPt = new PointClass();
                constructPt.ConstructAngleBisector(terlkPtc.get_Point(fptInfo.PtAtNumber - 1), fptInfo.PtCoord, terlkPtc.get_Point(fptInfo.PtAtNumber + 1), 6.45, false);
                IPoint newPt = new PointClass();
                newPt = constructPt as IPoint;
                secondPt = newPt;
                newPt.Z = 10;
                ptList.Add(newPt);
                newFptClass.PtCoord = newPt;
                newFptClass.Elev = fptInfo.Elev;
                fptInfo = newFptClass;
            }
            else 
            {
                ptList.Add(fptInfo.PtCoord);
               
                IPointCollection leftPtCol = leftPly as IPointCollection;
                IPointCollection rightPtCol = rightPly as IPointCollection;
                for (int i = 0; i < leftPtCol.PointCount - 3; i++)
                {
                    int p = 0;
                    IPolyline ply1 = CreateAngleBisector(leftPtCol.get_Point(i), leftPtCol.get_Point(i + 1), leftPtCol.get_Point(i + 2));
                    ITopologicalOperator topo = ply1 as ITopologicalOperator;
                    for (int j = i; j < rightPtCol.PointCount - 3; j++)
                    {
                        IPolyline ply2 = CreateAngleBisector(rightPtCol.get_Point(j), rightPtCol.get_Point(j + 1), rightPtCol.get_Point(j + 2));
                        IGeometry pGeo = topo.Intersect(ply2, esriGeometryDimension.esriGeometry0Dimension);
                        if (pGeo.IsEmpty == false)
                        {
                            IPointCollection ptc = pGeo as IPointCollection;
                            IPoint pt = ptc.get_Point(0);
                            double midLen = Length(pt, fptInfo.PtCoord);
                            double leftLen = Length( pt, leftPtCol.get_Point(i + 1));
                            double rightLen = Length(pt, rightPtCol.get_Point(j + 1));
                            if (Math.Abs(midLen- leftLen)<1 &&Math.Abs(midLen- rightLen)<1)
                            {
                                pt.Z = 10;
                                ptList.Add(pt);
                                secondPt = pt;
                                newFptClass.PtCoord = pt;
                                newFptClass.Elev = fptInfo.Elev;
                                fptInfo = newFptClass;
                                p = 1;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (p == 1)
                    {
                        break;

                    }
                }
            }
            newLine.FromPoint = fristPt;
            newLine.ToPoint = secondPt ;
                     
            double l = 100; double r = 100;                            
            Dictionary<IPoint, double> leftDic = ReturnMinDistancePt(leftPly, secondPt);
            Dictionary<IPoint, double> rightDic = ReturnMinDistancePt(rightPly, secondPt);
            l = leftDic.Values.ElementAt(0);
            r = rightDic.Values.ElementAt(0);
            double h = (l + r) / 6.0;
            IConstructPoint extendPt = new PointClass();
            extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
            IPoint nePt = new PointClass();
            nePt = extendPt as IPoint;
            int m = 0;
            newLine.FromPoint = secondPt ;
            newLine.ToPoint = nePt;
            Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
            while (true)
            {
                if (m == 0)
                {
                    fristPt = secondPt;
                    secondPt = nePt;
                }
                else
                {
                    fristPt = newLine.FromPoint;
                    secondPt = newLine.ToPoint;
                }
                if (m != 0)
                {
                    if (isSaddOrGeneral == 0 && geometry.GeometryType == esriGeometryType.esriGeometryPolyline) 
                    {
                        IPolyline ppy = new PolylineClass();
                        ppy.FromPoint = newLine.FromPoint;
                        ppy.ToPoint = newLine.ToPoint;
                        ITopologicalOperator topo = ppy as ITopologicalOperator;
                        IGeometry pgeo = topo.Intersect(geometry, esriGeometryDimension.esriGeometry0Dimension);
                        if (pgeo.IsEmpty == false)
                        {
                            IPointCollection ptcol = pgeo as IPointCollection;
                            IPoint pt = ptcol.get_Point(0);
                            pt.Z = 10;
                            ptList.Add(pt);
                            IPolyline ply = CreatePly(ptList);
                            return ply;
                        }
                    }
                    else if (isSaddOrGeneral == 0 && geometry.GeometryType == esriGeometryType.esriGeometryPoint) 
                    {
                        ITopologicalOperator pTopoOper = newLine.ToPoint as ITopologicalOperator;
                        IPolygon pBufferPoly = pTopoOper.Buffer(30) as IPolygon;
                        IRelationalOperator relatOper = pBufferPoly as IRelationalOperator;
                        if (relatOper.Contains(geometry) == true)
                        {
                            IPoint ppt = geometry as IPoint;
                            ptList.Add(ppt);
                            IPolyline ply = CreatePly(ptList);
                            return ply;
                        }
                    }
                    else if (isSaddOrGeneral == 1 && geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        IPolyline ppy = new PolylineClass();
                        ppy.FromPoint = newLine.FromPoint;
                        ppy.ToPoint = newLine.ToPoint;
                        ITopologicalOperator topo = ppy as ITopologicalOperator;
                        IGeometry pgeo = topo.Intersect(geometry, esriGeometryDimension.esriGeometry0Dimension);
                        if (pgeo.IsEmpty == false)
                        {
                            IPointCollection ptcol = pgeo as IPointCollection;
                            IPoint pt = ptcol.get_Point(0);
                            pt.Z = 10;
                            ptList.Add(pt);
                            IPolyline ply = CreatePly(ptList);
                            return ply;
                        }
                    }
                }
                K = newLine.Angle;
                l = 100; r = 100;                              
                leftDic = ReturnMinDistancePt(leftPly, secondPt);
                rightDic = ReturnMinDistancePt(rightPly, secondPt);
                l = leftDic.Values.ElementAt(0);
                r = rightDic.Values.ElementAt(0);
                double diff = Math.Abs(l - r);
                leNew = diff;
                if (dic.ContainsKey(secondPt)==false )
                {
                    dic.Add(secondPt, diff);
                }
              
                if (diff < 0.03||m>15)
                {
                    if (m>15)
                    { 
                        var dicSd = from objDic in dic orderby objDic.Value ascending select objDic;
                         
                        foreach (KeyValuePair<IPoint,double> keyv in dicSd)
                        {
                            secondPt = keyv.Key; break;
                        }                        
                    }
                    h = (l + r) / 6.0;
                    secondPt.Z = 10;
                    ptList.Add(secondPt);
                    newFptClass = new FeaPtInform();
                    newFptClass.PtCoord = secondPt;
                     
                    extendPt = new PointClass();
                    extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                    IPoint newPt = extendPt as IPoint;
                    newLine.FromPoint = secondPt; ;
                    newLine.ToPoint = newPt;
                    fptInfo = newFptClass;
                    K = newLine.Angle;
                    leOld = 0;
                    leNew = 0;
                    startStep = 0;
                    adjustiveAngle = 5;
                    dic = new Dictionary<IPoint, double>();
                    m=1;                   
                }
                else
                {
                    if (startStep == 0)
                    {
                        if (l > r)
                        {
                            adjustiveAngle = -adjustiveAngle;
                            direction = 0;
                        }
                        else
                        {
                            adjustiveAngle = 5;
                            direction = 1;
                        }
                        IPoint adjustivePt = new PointClass(); 
                        adjustivePt.X = newLine.FromPoint.X + h * Math.Cos(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                        adjustivePt.Y = newLine.FromPoint.Y + h * Math.Sin(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                        leOld = leNew;
                        newLine.FromPoint = fristPt; ;
                        newLine.ToPoint = adjustivePt;
                        newFptClass = new FeaPtInform();
                        newFptClass.PtCoord = fristPt;
                        newFptClass.Elev = fptInfo.Elev;
                        fptInfo = newFptClass;
                        startStep = 1;
                        m++;
                    }
                    else if (startStep == 1)
                    {
                        if (leNew < leOld) 
                        {
                            if (direction == 0) 
                            {
                                if (adjustiveAngle > 0)
                                {
                                    adjustiveAngle = -adjustiveAngle;
                                }
                                double a = adjustiveAngle;
                                adjustiveAngle = a; direction = 0;
                            }
                            else if (direction == 1) 
                            {
                                if (adjustiveAngle < 0)
                                {
                                    adjustiveAngle = -adjustiveAngle;
                                }
                                double a = adjustiveAngle;
                                adjustiveAngle = a; direction = 1;
                            }
                            IPoint adjustivePt = new PointClass(); 
                            adjustivePt.X = newLine.FromPoint.X + h * Math.Cos(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                            adjustivePt.Y = newLine.FromPoint.Y + h * Math.Sin(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                            leOld = leNew;
                            newLine.FromPoint = fristPt; ;
                            newLine.ToPoint = adjustivePt;
                            newFptClass = new FeaPtInform();
                            newFptClass.PtCoord = fristPt;
                            newFptClass.Elev = fptInfo.Elev;
                            fptInfo = newFptClass;
                            startStep = 1;
                        }
                        else if (leNew > leOld) 
                        {
                            if (direction == 0) 
                            {
                                if (adjustiveAngle < 0)
                                {
                                    adjustiveAngle = -adjustiveAngle;
                                }
                                adjustiveAngle = adjustiveAngle / 2;
                                direction = 1;
                            }
                            else if (direction == 1) 
                            {
                                if (adjustiveAngle > 0)
                                {
                                    adjustiveAngle = -adjustiveAngle;
                                }
                                adjustiveAngle = adjustiveAngle / 2;
                                direction = 0;
                            }
                            IPoint adjustivePt = new PointClass(); 
                            adjustivePt.X = newLine.FromPoint.X + h * Math.Cos(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                            adjustivePt.Y = newLine.FromPoint.Y + h * Math.Sin(newLine.Angle + (Math.PI / 180) * adjustiveAngle);
                            leOld = leNew;
                            newLine.FromPoint = fristPt; ;
                            newLine.ToPoint = adjustivePt;
                            newFptClass = new FeaPtInform();
                            newFptClass.PtCoord = fristPt;
                            newFptClass.Elev = fptInfo.Elev;
                            fptInfo = newFptClass;
                            startStep = 1;

                        }
                        else if (leNew == leOld)
                        {
                            
                            extendPt = new PointClass();
                            extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                            IPoint newPt = extendPt as IPoint;
                            newLine.FromPoint = secondPt; ;
                            newLine.ToPoint = newPt;
                            newFptClass = new FeaPtInform();
                            newFptClass.PtCoord = secondPt;
                            newFptClass.Elev = fptInfo.Elev;
                            fptInfo = newFptClass;
                            K = newLine.Angle;
                            leOld = 0;
                            leNew = 0;
                            startStep = 0;
                            adjustiveAngle = 5;
                        }
                    }
                    m++;
                }
            }
        }
        static private double Length(IPoint pt1, IPoint pt2)
        {
            ILine line = new LineClass();
            line.FromPoint = pt1;
            line.ToPoint = pt2;
            ////pt1.Z = 100;
            ////pt2.Z = 100;
            //IPolyline py= new PolylineClass();
            //py.FromPoint = pt1;
            //py.ToPoint = pt2;
            //PublicFunctionClass.CreateStructPly(terlk, py, 0, true);
            return line.Length;
        }
        static private IPolyline CreateAngleBisector(IPoint pt1, IPoint pt2, IPoint pt3)
        {
            IConstructPoint constructPt = new PointClass();
            constructPt.ConstructAngleBisector(pt1, pt2, pt3, 100, true);
            IPoint newPt = new PointClass();
            newPt = constructPt as IPoint;
            IPolyline newPly = new PolylineClass();
            newPly.FromPoint = pt2;
            newPly.ToPoint = newPt;
            return newPly;
        }
        static private Dictionary<IPoint, double> ReturnMinDistancePt(IPolyline line, IPoint point)
        {
            Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
            double distanceAlongCurve = 0;        
            double distanceFromCurve = 0;              
            bool bRightSide = false;
            IPoint outPt = new PointClass();
            line.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, false, outPt, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
            dic.Add(outPt, distanceFromCurve);
            return dic;

        }
        static private IPolyline CreatePly(List<IPoint> ptList)
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
    }
}

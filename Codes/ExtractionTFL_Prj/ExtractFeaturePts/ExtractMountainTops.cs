using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PieceLineSmoothing_3OrderPolyno;
using CreateBoundary;

namespace ExtractFeaturePts
{
   public  class ExtractMountainTops
    {
        static List<PixelPosition_T> newPiexlList = null;
        static private List<IFeature> hellollFea = new List<IFeature>(); 
        static private List<IFeature> bottomFea = new List<IFeature>();   
        static List<FeaInfoClass> closedline = new List<FeaInfoClass>(); 
        static List<FeaInfoClass> openline = new List<FeaInfoClass>(); 
        static public IDictionary<IPoint ,IFeature> mounTPtList = new  Dictionary<IPoint,IFeature> (); 
        public static double intervalValue; 
        static public void GetMouAndBotPts(IFeatureLayer conFyr)
        {
            GetMouOrBotFun(conFyr);
            for (int i = 0; i < hellollFea.Count; i++)
            {
                if (hellollFea[i].Shape.IsEmpty == false)
                {
                    GetRealPt(conFyr, hellollFea[i],  true, hellollFea[i].OID, 2);
                }
            }
            for (int j = 0; j < bottomFea.Count; j++)
            {
                if (bottomFea[j].Shape.IsEmpty == false)
                {
                    GetRealPt(conFyr, bottomFea[j], false, bottomFea[j].OID, -2);
                }

            }
        }
       
      
       static private void GetRealPt(IFeatureLayer conFyr, IFeature pFeature, bool isPeakOrBottom, int fid, int sMark)//true表示山包线，false表示山洼线
        {
            Dictionary<IPoint, IFeature> feaAndCenPt = RaterPtToVectorPt(pFeature);
            for (int i = 0; i < feaAndCenPt.Count; i++)
            {
                IPoint newPeakOrBotPt = feaAndCenPt.Keys.ElementAt(i);
                IPointCollection ptC = feaAndCenPt.Values.ElementAt(i).Shape as IPointCollection;
                double Z = Math.Round (ptC.get_Point(0).Z,0);
                IPolyline mainLine = GetMainDirection(pFeature);    
                IPolyline sectionPly = GetaSectionPly(mainLine, newPeakOrBotPt);                
                IPoint leftPtOfMouPt = new PointClass();
                IPoint rightPtOfMouPt = new PointClass();
                ITopologicalOperator sTopo = sectionPly as ITopologicalOperator;
                IGeometry pGeo = sTopo.Intersect(pFeature.Shape, esriGeometryDimension.esriGeometry0Dimension);
                IPointCollection ptCol =new MultipointClass ();
                if (pGeo.IsEmpty==false)
                {
                    ptCol = pGeo as IPointCollection;                     
                    IPoint pt1 = ptCol.get_Point(0);
                    IPoint pt2 = ptCol.get_Point(1);
                    if (pt1.X>pt2.X)
                    {
                        rightPtOfMouPt = pt1;
                        leftPtOfMouPt = pt2;
                        
                    }
                    else
                    {
                        rightPtOfMouPt = pt2;
                        leftPtOfMouPt = pt1;
                    }
                  
                }
                
                IPoint leftPtOfAdjPt = new PointClass();
                IPoint rightPtOfAdjPt = new PointClass();
                IFeature adjacentF = GetAdjacentFea(closedline, Z,newPeakOrBotPt, isPeakOrBottom);
                if (adjacentF==null)
                {
                    adjacentF = GetAdjacentFea(openline, Z, newPeakOrBotPt, isPeakOrBottom);
                }                
                pGeo = sTopo.Intersect(adjacentF.Shape ,esriGeometryDimension.esriGeometry0Dimension);
                if (pGeo.IsEmpty==false)
                {
                    ptCol = pGeo as IPointCollection;
                    if (ptCol.PointCount==2)
                    {
                        IPoint pt1 = ptCol.get_Point(0);
                       IPoint pt2 = ptCol.get_Point(1);
                       if (pt1.X>pt2.X)
                       {
                          rightPtOfAdjPt=pt1;
                          leftPtOfAdjPt = pt2;
                       }
                       else
                       {
                           rightPtOfAdjPt = pt2;
                           leftPtOfAdjPt = pt1;
                      }
                    }
                    else if (ptCol.PointCount>2)
                    {                         
                        Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
                        for (int i_1 = 0; i_1 < ptCol.PointCount; i_1++)
		                {
                            IPoint pt=ptCol.get_Point(i_1);
                            double dis=Math.Pow(newPeakOrBotPt.X-pt.X,2)+Math.Pow(newPeakOrBotPt.Y-pt.Y,2);
                            dic.Add(pt, dis);
                        }
                        var disCd = from objDic in dic orderby objDic.Value ascending select objDic;
                        List<IPoint> ptList = new List<IPoint>();
                        foreach (KeyValuePair <IPoint ,double > key in disCd)
                        {
                            ptList.Add(key.Key);
                            if (ptList.Count==2)
                            {
                                break;
                            }
                        }
                        IPoint pt1=ptList[0];
                        IPoint pt2=ptList[1];
                        if (pt1.X>pt2.X)
                        {
                            rightPtOfAdjPt = pt1;
                            leftPtOfAdjPt = pt2;
                        }
                        else
                        {
                            rightPtOfAdjPt = pt2;
                            leftPtOfAdjPt = pt1;
                        }

                    }
                    else if (ptCol.PointCount ==1) 
                    {
                        IPoint pt1=ptCol.get_Point(0);
                        IPoint pt2 = new PointClass();
                        Dictionary<IPolyline, int> boundD = GetBounds.GetNewBoundAndPtsDataFun(conFyr);
                        for (int j = 0; j < boundD.Count; j++)
                        {
                            IPolyline ply = boundD.Keys.ElementAt(j);
                            IGeometry pg = sTopo.Intersect(ply,esriGeometryDimension.esriGeometry0Dimension);
                            if (pg.IsEmpty==false )
                            {
                                IPointCollection otherPtc = pg as IPointCollection;
                                pt2 = otherPtc.get_Point(0);
                                break;
                            }                            
                        }
                        if (pt1.X > pt2.X)
                        {
                            rightPtOfAdjPt = pt1;
                            leftPtOfAdjPt = pt2;
                        }
                        else
                        {
                            rightPtOfAdjPt = pt2;
                            leftPtOfAdjPt = pt1;
                        }
                    }
                }
                ILine newLine = new LineClass();
                newLine.FromPoint = sectionPly.FromPoint;
                newLine.ToPoint = sectionPly.ToPoint;
                IPoint newPt1 = new PointClass ();
                IPoint newPt2 = new PointClass();
                newPt1 = RotateCoordinate(leftPtOfMouPt, newLine.Angle);
                newPt2 = RotateCoordinate(rightPtOfMouPt, newLine.Angle);
                double angle1 = 0;
                double angle2 = 0;                 
                double  rightDis= Math.Sqrt(Math.Pow(rightPtOfMouPt.X - rightPtOfAdjPt.X, 2) + Math.Pow(rightPtOfMouPt.Y - Math.Abs(rightPtOfAdjPt.Y), 2));//右边
                double  leftDis =Math.Sqrt(Math.Pow(leftPtOfAdjPt.X - leftPtOfMouPt.X, 2) + Math.Pow(leftPtOfAdjPt.Y - Math.Abs(leftPtOfMouPt.Y), 2)); //左边                            
                if (isPeakOrBottom==true )
                {
                    if (leftDis < rightDis) 
                    {
                        angle1 = Math.Atan(intervalValue / leftDis);
                        angle2 = 2 * Math.PI - Math.Atan(intervalValue / rightDis);
                    }
                    else if (leftDis > rightDis) 
                    {
                        angle1 = Math.PI - Math.Atan(intervalValue / rightDis);
                        angle2 = Math.PI + Math.Atan(intervalValue / leftDis);
                    }    
                }
                else
                {
                    if (leftDis < rightDis) 
                    {
                        angle1 = 2 * Math.PI - Math.Atan(intervalValue / leftDis);
                        angle2 = Math.Atan(intervalValue / rightDis);
                    }
                    else if (leftDis > rightDis) 
                    {
                        angle1 = Math.PI + Math.Atan(intervalValue / rightDis);
                        angle2 = Math.PI - Math.Atan(intervalValue / leftDis);
                    }    
                }        
                
                Dictionary<double, double> ptXZDic = new Dictionary<double, double>();
                GetCalculatedPoints.dTightness= 2.5;
                for (int j = 0; j < 1; j++)
                {

                    ptXZDic = GetCalculatedPoints.CreateNewPoint(newPt1.X, Z, newPt2.X, Z, angle1 , angle2, 20);
                    ptXZDic = SortedByZValue(ptXZDic, isPeakOrBottom);
                    double diffZ = ptXZDic.Values.ElementAt(0);
                    IPoint ppts = RotateCoordinate(newPeakOrBotPt, newLine.Angle);
                    if ((diffZ < Z + intervalValue && diffZ > Z) || (diffZ > Z - intervalValue && diffZ < Z))
                    {
                        IPoint pt = new PointClass();
                        if (ptXZDic.Keys.ElementAt(0) > ppts.X)
                        {
                            pt.X = ppts.X + Math.Abs(ptXZDic.Keys.ElementAt(0) - ppts.X);
                        }
                        else
                        {
                            pt.X = ppts.X - Math.Abs(ptXZDic.Keys.ElementAt(0) - ppts.X);
                        }
                        pt.Y = ((pt.X - newPt2.X) * (newPt1.Y - newPt2.Y) / (newPt1.X - newPt2.X)) + newPt2.Y;
                        IPoint nPt = RotateCoordinate(pt, -newLine.Angle);                                                
                        nPt.Z = ptXZDic.Values.ElementAt(0);
                        mounTPtList.Add(nPt, feaAndCenPt.Values.ElementAt(i));
                        break;
                    }
                    else
                    {
                        GetCalculatedPoints.dTightness-= 0.1;
                        j = -1;
                    }
                }                                                 
            }
        }     
       static private Dictionary<double, double> SortedByZValue(Dictionary<double, double> paramDic,bool isPeakOrBot)
        {
            Dictionary<double, double> newDic = new Dictionary<double, double>();
            
            if (isPeakOrBot==true) 
            {
                var dicSd = from objDic in paramDic orderby objDic.Value descending select objDic;
                foreach (KeyValuePair <double ,double > keyv in dicSd)
                {
                    newDic.Add(keyv.Key, keyv.Value);
                }
            }
            else
            {
                var dicSd = from objDic in paramDic orderby objDic.Value ascending select objDic;
                foreach (KeyValuePair<double, double> keyv in dicSd)
                {
                    newDic.Add(keyv.Key, keyv.Value);
                }
            }                      
            return newDic;
        
        }
       
        static private IFeature  GetAdjacentFea(List<FeaInfoClass> feaList, double Z,IPoint newPt, bool isPeakOrBottom)
        {
            IFeature feaL = null; IPolygon polygon = null; IRelationalOperator relTopo = null;
            for (int i = 0; i < feaList.Count; i++)
            {
                double elev = feaList[i].Z;
                if ((Math.Abs(elev - Z) == intervalValue && Z > elev && isPeakOrBottom == true) || (Math.Abs(elev - Z) == intervalValue && Z < elev && isPeakOrBottom == false))
                {
                      
                     polygon = ConstructPolygonFromPolyline(feaList[i].ConFea.Shape as IPolyline );
                     relTopo = polygon as IRelationalOperator;
                     if (relTopo.Contains(newPt)==true )
                     {
                       feaL=feaList[i].ConFea;
                     }
                }                                 
            }
            return feaL;
        }       
       
        static private IPolyline GetaSectionPly(IPolyline ply, IPoint point)
        {
            IPolyline radioPly = new PolylineClass();
            double distanceAlongCurve = 0;        
            double distanceFromCurve = 0;          
            bool bRightSide = false;
            IPoint outPt = new PointClass();
            ply.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, false, outPt, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
            ILine beforeLine = new LineClass();
            beforeLine.FromPoint = point;
            beforeLine.ToPoint = outPt;
            IPolyline temperply1 = ExtendLineFun(point, beforeLine.Angle);
            IPolyline temperply2 = ExtendLineFun(point, beforeLine.Angle - Math.PI);
            radioPly.FromPoint = temperply1.ToPoint;
            radioPly.ToPoint = temperply2.ToPoint;
            return radioPly;
        }
        static private IPoint RotateCoordinate(IPoint originalPt, double angle)
        {
            IPoint newPt = new PointClass();
            newPt.X = originalPt.X * Math.Cos(angle) + originalPt.Y * Math.Sin(angle);
            newPt.Y = originalPt.X * (-Math.Sin(angle)) + originalPt.Y * Math.Cos(angle);
            return newPt;
        }               
        
        static private IPolyline ExtendLineFun(IPoint pt1, double angle)
        {
            IConstructPoint extendPt = new PointClass();
            extendPt.ConstructAngleDistance(pt1, angle, 150);
            IPoint newPt = extendPt as IPoint;
            IPolyline extendPly = new PolylineClass();
            extendPly.FromPoint = pt1;
            extendPly.ToPoint = newPt;
            return extendPly;
        }
        
        static private IPolyline GetMainDirection(IFeature pFeature)
        {
            IPointCollection ptCol = pFeature.Shape as IPointCollection;
            IPolyline nPly = new PolylineClass();
            double dis = 0;
            int a = 0; int b = 0;
            for (int i = 0; i < ptCol.PointCount; i++)
            {
                IPolyline newPly = new PolylineClass();
                newPly.FromPoint = ptCol.get_Point(i);
                for (int j = i + 1; j < ptCol.PointCount; j++)
                {
                    newPly.ToPoint = ptCol.get_Point(j);
                    if (newPly.Length > dis)
                    {
                        a = i; b = j;
                        dis = newPly.Length;
                    }
                }

            }
            nPly.FromPoint = ptCol.get_Point(a);
            nPly.ToPoint = ptCol.get_Point(b);
            return nPly;

        }
        
        static private Dictionary<IPoint, IFeature> RaterPtToVectorPt(IFeature pFeature)
        {
            Dictionary<IPoint, IFeature> feaAndCenterRaterPt = new Dictionary<IPoint, IFeature>();
            newPiexlList = new List<PixelPosition_T>();
            IPoint opt = new PointClass();
            opt.X = 0;
            opt.Y = 0;
            double pixelsize = 1;
            IPolyline pFeaPly = pFeature.Shape as IPolyline;
            IEnvelope envelop = pFeaPly.Envelope;
            double xMin = envelop.XMin;
            double xMax = envelop.XMax;
            double yMin = envelop.YMin;
            double yMax = envelop.YMax;

            int rs = (int)((yMax - yMin) / pixelsize);
            int cs = (int)((xMax - xMin) / pixelsize);
            Raster_T mapras = new Raster_T(rs, cs, pixelsize, opt); 
            mapras.Originpt.Y = yMax;
            mapras.Originpt.X = xMin;
            List<PixelPosition_T> pypixel = RasterationPolygon(pFeature, mapras, xMin, xMax); 
            PixelPosition_T pp1 = pypixel[pypixel.Count - 1];
            List<PixelPosition_T> newPiexl = new List<PixelPosition_T>();
            
            Bitmap bmp = new Bitmap(mapras.Cols + 3, mapras.Rows + 2);
            foreach (PixelPosition_T pp in pypixel)
            {
                PixelPosition_T pix_T = new PixelPosition_T();
                pix_T.Code = 1;
                pix_T.Col = Math.Abs(pp.Col) + 1;
                pix_T.Row = Math.Abs(pp.Row) + 1;
                newPiexl.Add(pix_T);
            }
            newPiexl.Reverse();
            newPiexlList = ArrangePixel(newPiexl);
            PixelPosition_T[,] p1 = OneDemiConvertToTwo(rs + 2, cs + 3);
            PixelPosition_T[,] p2 = CalOne(p1);
            Dictionary<int, PixelPosition_T[,]> p3 = CalTwo(p2);
            List<PixelPosition_T> test = TestDistanceSkeletonThinning(p3);
            IPoint newPeakPt = new PointClass();
            if (test.Count == 1)
            {
                newPeakPt.X = xMin + test[0].Col;
                newPeakPt.Y = yMax - test[0].Row;
                newPeakPt.Z = 100;
                feaAndCenterRaterPt.Add(newPeakPt, pFeature);
            }
            else
            {
                double X = 0; double Y = 0;
                for (int i = 0; i < test.Count; i++)
                {
                    X += xMin + test[i].Col;
                    Y += yMax - test[i].Row;
                }
                newPeakPt.X = X / test.Count;
                newPeakPt.Y = Y / test.Count;
                newPeakPt.Z = 100;
                feaAndCenterRaterPt.Add(newPeakPt, pFeature);
            }
            return feaAndCenterRaterPt;
        }
        #region raster closed conour lines
        static private List<PixelPosition_T> RasterationPolygon(IFeature polyFea, Raster_T ras, double xmin, double xMax)
        {
            List<PixelPosition_T> pypixel = new List<PixelPosition_T>();
            IPointCollection pygonPtCol = polyFea.Shape as IPointCollection;
            Dictionary<IPoint, double> ptDic = new Dictionary<IPoint, double>();
            for (int i = 0; i < pygonPtCol.PointCount; i++)
            {
                //IPoint ppt = GetGeo( axMapControl1.ActiveView,pygonPtCol.get_Point(i).X, pygonPtCol.get_Point(i).Y);
                IPoint ppt = pygonPtCol.get_Point(i);
                ptDic.Add(ppt, ppt.Y);
            }
            
            Dictionary<IPoint, double> dicYValue = SortByXYvalue(ptDic);
            PixelPosition_T ppmax = RasterationPoint(dicYValue.Keys.ElementAt(dicYValue.Count - 1), ras);
            PixelPosition_T ppmin = RasterationPoint(dicYValue.Keys.ElementAt(0), ras);
            for (int i = 0; i <= (ppmax.Row - ppmin.Row); i++) 
            {
                double y = (ppmin.Row + i + 0.5) * ras.Pixelsize + ras.Originpt.Y;
                IPoint pt1 = new PointClass();
                pt1.X = xmin;// ras.Originpt.X;
                pt1.Y = y;
                pt1.Z = 100;
                IPoint pt2 = new PointClass();
                pt2.X = xMax;//  ras.Originpt.X + ras.Cols * ras.Pixelsize;
                pt2.Y = y;
                pt2.Z = 100;
                IPolyline plyLine = new PolylineClass();
                plyLine.FromPoint = pt1;
                plyLine.ToPoint = pt2;                
                ITopologicalOperator topoLyr = plyLine as ITopologicalOperator;
                Dictionary<IPoint, double> interpts = new Dictionary<IPoint, double>(); 
                for (int j = 0; j < pygonPtCol.PointCount - 1; j++)
                {
                    IPolyline partLy = new PolylineClass();
                    partLy.FromPoint = pygonPtCol.get_Point(j);
                    partLy.ToPoint = pygonPtCol.get_Point(j + 1);
                    IGeometry pgeo = topoLyr.Intersect(partLy as IGeometry, esriGeometryDimension.esriGeometry0Dimension);
                    if (pgeo.IsEmpty == false)
                    {
                        IPointCollection ptC = pgeo as IPointCollection;
                        interpts.Add(ptC.get_Point(0), ptC.get_Point(0).X);
                    }
                }
                Dictionary<IPoint, double> newPts = SortByXYvalue(interpts);   
                for (int j = 0; j < newPts.Count - 1; j = j + 2) 
                {
                    IPolyline pline = new PolylineClass();
                    pline.FromPoint = newPts.Keys.ElementAt(j);
                    pline.ToPoint = newPts.Keys.ElementAt(j + 1);
                    List<PixelPosition_T> cpixel = RasterationHonLine(pline, ras); 
                    pypixel.AddRange(cpixel);
                }
            }
            return pypixel;
        }      
        static private List<PixelPosition_T> RasterationHonLine(IPolyline line, Raster_T ras)
        {
            List<PixelPosition_T> pixels = new List<PixelPosition_T>();

            PixelPosition_T ppmax = RasterationPoint(line.ToPoint, ras);

            PixelPosition_T ppmin = RasterationPoint(line.FromPoint, ras);
            pixels.Add(ppmin);
            for (int i = 1; i < ppmax.Col - ppmin.Col; i++)//
            {
                PixelPosition_T cpp = new PixelPosition_T(ppmin.Row, ppmin.Col + i);
                pixels.Add(cpp);
            }
            pixels.Add(ppmax);
            return pixels;
        }
        static private PixelPosition_T RasterationPoint(IPoint pt, Raster_T ras)
        {
            int row = (int)((pt.Y - ras.Originpt.Y) / ras.Pixelsize);
            int col = (int)((pt.X - ras.Originpt.X) / ras.Pixelsize);
            PixelPosition_T pp = new PixelPosition_T(row, col);
            return pp;
        }
        static private Dictionary<IPoint, double> SortByXYvalue(Dictionary<IPoint, double> paramDic)
        {
            var dicSd = from objDic in paramDic orderby objDic.Value ascending select objDic;
            Dictionary<IPoint, double> paramTemper = new Dictionary<IPoint, double>();
            foreach (KeyValuePair<IPoint, double> keyv in dicSd)
            {
                paramTemper.Add(keyv.Key, keyv.Value);
            }
            return paramTemper;
        }
        static private List<PixelPosition_T> ArrangePixel(List<PixelPosition_T> newPiexl)
        {
            List<PixelPosition_T> piexlList = new List<PixelPosition_T>();
            for (int i = 0; i < newPiexl.Count - 1; i++)
            {
                PixelPosition_T pixel = new PixelPosition_T();
                for (int j = 0; j < newPiexl.Count - 1; j++)
                {
                    if (newPiexl[j + 1].Row == newPiexl[j].Row && newPiexl[j].Col > newPiexl[j + 1].Col)
                    {
                        pixel = newPiexl[j + 1];
                        newPiexl[j + 1] = newPiexl[j];
                        newPiexl[j] = pixel;
                    }

                }
            }
            return piexlList = newPiexl;

        }
        #endregion
        #region Skeleton thining algorithm
        static private PixelPosition_T[,] OneDemiConvertToTwo(int rs, int col)
        {
            PixelPosition_T[,] pixelArrange = new PixelPosition_T[rs, col];
            for (int i = 0; i < newPiexlList.Count; i++)
            {
                PixelPosition_T pix_T = newPiexlList[i];
                pixelArrange[pix_T.Row, pix_T.Col] = new PixelPosition_T();
                pixelArrange[pix_T.Row, pix_T.Col].Row = pix_T.Row;
                pixelArrange[pix_T.Row, pix_T.Col].Col = pix_T.Col;
                pixelArrange[pix_T.Row, pix_T.Col].Code = pix_T.Code;
            }
            return pixelArrange;

        }
        static private PixelPosition_T[,] CalOne(PixelPosition_T[,] pixelArrange)//第一次计算
        {
            for (int i = 0; i < pixelArrange.GetLength(0); i++)
            {
                for (int j = 0; j < pixelArrange.GetLength(1); j++)
                {
                    if (pixelArrange[i, j] != null)
                    {
                        if (pixelArrange[i, j - 1] == null)
                        {
                            pixelArrange[i, j - 1] = new PixelPosition_T();
                        }
                        if (pixelArrange[i - 1, j] == null)
                        {
                            pixelArrange[i - 1, j] = new PixelPosition_T();
                        }
                        if (pixelArrange[i, j - 1].Code < pixelArrange[i - 1, j].Code)
                        {
                            pixelArrange[i, j].Code = pixelArrange[i, j - 1].Code + 1;
                        }
                        else
                        {
                            pixelArrange[i, j].Code = pixelArrange[i - 1, j].Code + 1;
                        }
                    }
                }
            }
            return pixelArrange;
        }
        static private Dictionary<int, PixelPosition_T[,]> CalTwo(PixelPosition_T[,] pixelArrange)//第二次计算
        {

            Dictionary<int, PixelPosition_T[,]> dic = new Dictionary<int, PixelPosition_T[,]>();
            int maxCode = 1;
            for (int i = pixelArrange.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = pixelArrange.GetLength(1) - 1; j >= 0; j--)
                {
                    if (pixelArrange[i, j] != null && pixelArrange[i, j].Code != 0)
                    {
                        if (pixelArrange[i, j + 1] == null)
                        {
                            pixelArrange[i, j + 1] = new PixelPosition_T();
                        }
                        if (pixelArrange[i + 1, j] == null)
                        {
                            pixelArrange[i + 1, j] = new PixelPosition_T();
                        }
                        if (pixelArrange[i, j].Code <= pixelArrange[i, j + 1].Code + 1 && pixelArrange[i, j].Code <= pixelArrange[i + 1, j].Code + 1)
                        {
                            pixelArrange[i, j].Code = pixelArrange[i, j].Code;
                        }
                        else if (pixelArrange[i, j + 1].Code + 1 <= pixelArrange[i, j].Code && pixelArrange[i, j + 1].Code + 1 <= pixelArrange[i + 1, j].Code + 1)
                        {
                            pixelArrange[i, j].Code = pixelArrange[i, j + 1].Code + 1;
                        }
                        else if (pixelArrange[i + 1, j].Code + 1 <= pixelArrange[i, j + 1].Code + 1 && pixelArrange[i + 1, j].Code + 1 <= pixelArrange[i, j].Code)
                        {
                            pixelArrange[i, j].Code = pixelArrange[i + 1, j].Code + 1;
                        }
                        if (pixelArrange[i, j].Code > maxCode)
                        {
                            maxCode = pixelArrange[i, j].Code;
                        }

                    }
                }

            }
            dic.Add(maxCode, pixelArrange);
            return dic;
        }
        static private List<PixelPosition_T> TestDistanceSkeletonThinning(Dictionary<int, PixelPosition_T[,]> dic)
        {
            PixelPosition_T[,] pixelArrange = dic.Values.ElementAt(0);
            int maxCode = dic.Keys.ElementAt(0);
            List<PixelPosition_T> pixelList = new List<PixelPosition_T>();
            for (int i = 0; i < pixelArrange.GetLength(0); i++)
            {
                for (int j = 0; j < pixelArrange.GetLength(1); j++)
                {
                    if (pixelArrange[i, j] != null && pixelArrange[i, j].Code == maxCode)
                    {
                        PixelPosition_T p_T = new PixelPosition_T();
                        p_T.Code = maxCode;
                        p_T.Row = pixelArrange[i, j].Row;
                        p_T.Col = pixelArrange[i, j].Col;
                        pixelList.Add(p_T);

                    }
                }
            }
            List<PixelPosition_T> pList = new List<PixelPosition_T>();//总的栅格点存储           
            if (pixelList.Count == 1)
            {
                pList.Add(pixelList[0]);
            }
            else
            {
                Dictionary<int, List<PixelPosition_T>> rowPP = new Dictionary<int, List<PixelPosition_T>>();
                List<PixelPosition_T> ppList = null;
                for (int i = 0; i < pixelList.Count; i++)
                {
                    ppList = new List<PixelPosition_T>();
                    PixelPosition_T p_T1 = pixelList[i];
                    ppList.Add(p_T1);
                    pixelList.Remove(p_T1);
                    for (int j = 0; j < pixelList.Count; j++)
                    {
                        PixelPosition_T p_T2 = pixelList[j];
                        if (p_T1.Row == p_T2.Row)
                        {
                            ppList.Add(p_T2); pixelList.Remove(p_T2); j = -1;
                        }
                    }
                    i = -1;
                    rowPP.Add(p_T1.Row, ppList);
                }
                if (rowPP.Count == 1)
                {
                    pList.Add(rowPP.Values.ElementAt(0)[(int)(rowPP.Values.ElementAt(0).Count / 2)]);
                }
                else
                {
                    bool isLinkRow = true;
                    for (int i = 0; i < rowPP.Count - 1; i++)
                    {
                        int row1 = rowPP.Keys.ElementAt(i);
                        List<PixelPosition_T> p_T1 = rowPP.Values.ElementAt(i);
                        List<PixelPosition_T> p_T2 = rowPP.Values.ElementAt(i + 1);
                        int row2 = rowPP.Keys.ElementAt(i + 1);
                        if (Math.Abs(row1 - row2) == 1)
                        {
                            if (isLinkRow == false)
                            {
                                pList.Clear();
                                for (int m = 0; m < p_T2.Count; m++)
                                {
                                    pList.Add(p_T2[m]);
                                }

                            }
                            if (isLinkRow == true)
                            {
                                if (i == 0)
                                {
                                    for (int m = 0; m < p_T1.Count; m++)
                                    {
                                        pList.Add(p_T1[m]);
                                    }

                                }
                                for (int m = 0; m < p_T2.Count; m++)
                                {
                                    pList.Add(p_T2[m]);
                                }

                            }
                            isLinkRow = true;
                        }
                        else
                        {
                            isLinkRow = false;
                        }
                    }
                    if (isLinkRow == false)
                    {
                        List<PixelPosition_T> p_T2 = rowPP.Values.ElementAt((int)(rowPP.Count / 2));
                        for (int m = 0; m < p_T2.Count; m++)
                        {
                            pList.Add(p_T2[m]);
                        }
                    }
                }

            }
            return pList;
        }
        #endregion 
         
        static private void GetMouOrBotFun(IFeatureLayer conFyr)
        {
            closedline = OrderedOfClosedCon(conFyr);
            List<int> idContain = new List<int>();
            for (int i = 0; i < closedline.Count; i++)
            {
                if (idContain.Count != 0)
                {
                    if (idContain.Contains(closedline[i].ConFea.OID))
                    {
                        continue;
                    }
                }
                IPointCollection ptCol = closedline[i].ConFea.Shape as IPointCollection;
                IPoint pt1 = ptCol.get_Point(0);
                double Z1 = closedline[i].Z;
                IPolygon pPolygon1 = GetPolygon(closedline[i].ConFea.Shape as IPolyline);
                IRelationalOperator topo1 = pPolygon1 as IRelationalOperator;
                int m = 0;
                List<FeaInfoClass> bottomFlist = new List<FeaInfoClass>();
                for (int j = i + 1; j < closedline.Count; j++)
                {
                    IPointCollection ptC = closedline[j].ConFea.Shape as IPointCollection;
                    IPoint pt2 = ptC.get_Point(0);
                    double Z2 = closedline[j].Z;
                    IPolygon pPolygon2 = GetPolygon(closedline[j].ConFea.Shape as IPolyline);
                    IRelationalOperator topo2 = pPolygon2 as IRelationalOperator;
                    if (topo2.Contains(pt1 as IGeometry))
                    {
                        if (m == 0)
                        {
                            hellollFea.Add(closedline[i].ConFea);
                            idContain.Add(closedline[i].ConFea.OID);
                        }
                        idContain.Add(closedline[j].ConFea.OID);
                        m++;
                    }
                    else if (topo1.Contains(pt2 as IGeometry))
                    {
                        bottomFlist.Add(closedline[j]);
                    }
                }
                if (bottomFlist.Count != 0)
                {
                    idContain.Add(closedline[i].ConFea.OID);
                    for (int n = 0; n < bottomFlist.Count; n++)
                    {
                        idContain.Add(bottomFlist[n].ConFea.OID);
                    }
                    bottomFea.Add(bottomFlist[bottomFlist.Count - 1].ConFea);
                }
                if (m == 0 && bottomFlist.Count == 0)//从开曲线
                {
                    IPointCollection ring = new RingClass() as IPointCollection;
                    IRing rng = ring as IRing;
                    for (int j = 0; j < ptCol.PointCount; j++)
                    {
                        ring.AddPoint(ptCol.get_Point(j));
                    }
                    bool isExterior = rng.IsExterior;
                    if (isExterior == true) 
                    {
                        bottomFea.Add(closedline[i].ConFea);
                    }
                    else 
                    {
                        hellollFea.Add(closedline[i].ConFea);
                    }
                    idContain.Add(closedline[i].ConFea.OID);
                }
            }
        }        
        static private List<FeaInfoClass> OrderedOfClosedCon(IFeatureLayer conFyr)
        {
            List<FeaInfoClass> getClosedLine = GetClosedContour(conFyr);
            List<FeaInfoClass> temperlist = new List<FeaInfoClass>();
            for (int i = 0; i < getClosedLine.Count - 1; i++)
            {
                FeaInfoClass fClass = new FeaInfoClass();
                for (int j = 0; j < getClosedLine.Count - 1; j++)
                {
                    if (getClosedLine[j + 1].Z >= getClosedLine[j].Z)
                    {
                        fClass = getClosedLine[j + 1];
                        getClosedLine[j + 1] = getClosedLine[j];
                        getClosedLine[j] = fClass;
                    }
                }
            }
            return temperlist = getClosedLine;
        }
        static private List<FeaInfoClass> GetClosedContour(IFeatureLayer conFyr)
        {
            List<FeaInfoClass> temperClosedline = new List<FeaInfoClass>();
            IFeatureCursor pFeaCursor = conFyr.FeatureClass.Search(null, false);
            IFeature pFeature = pFeaCursor.NextFeature();
            while (pFeature != null)
            {
                IPointCollection ptCol = pFeature.Shape as IPointCollection;
                if (Math.Abs(ptCol.get_Point(0).X - ptCol.get_Point(ptCol.PointCount - 1).X) < 0.01 && Math.Abs(ptCol.get_Point(0).Y - ptCol.get_Point(ptCol.PointCount - 1).Y) < 0.01)
                {
                    FeaInfoClass fInClass = new FeaInfoClass();
                    fInClass.ConFea = pFeature;
                    fInClass.Z = Math.Round(ptCol.get_Point(0).Z,0);
                    temperClosedline.Add(fInClass);

                }
                else
                {
                    FeaInfoClass fInClass = new FeaInfoClass();
                    fInClass.ConFea = pFeature;
                    fInClass.Z = Math.Round(ptCol.get_Point(0).Z, 0); 
                    openline.Add(fInClass);
                }
                pFeature = pFeaCursor.NextFeature();
            }
            return temperClosedline;
        }      
        static private IPolygon ConstructPolygonFromPolyline(IPolyline pPolyline)
        {
            IGeometryCollection pPolygonGeoCol = new PolygonClass();
            if ((pPolyline != null) && (!pPolyline.IsEmpty))
            {
                IGeometryCollection pPolylineGeoCol = pPolyline as IGeometryCollection;
                ISegmentCollection pSegCol = new RingClass();
                ISegment pSegment = null;
                object missing = Type.Missing;

                for (int i = 0; i < pPolylineGeoCol.GeometryCount; i++)
                {
                    ISegmentCollection pPolylineSegCol = pPolylineGeoCol.get_Geometry(i) as ISegmentCollection;
                    for (int j = 0; j < pPolylineSegCol.SegmentCount; j++)
                    {
                        pSegment = pPolylineSegCol.get_Segment(j);
                        pSegCol.AddSegment(pSegment, ref missing, ref missing);
                    }
                    pPolygonGeoCol.AddGeometry(pSegCol as IGeometry, ref missing, ref missing);
                }
            }
            return pPolygonGeoCol as IPolygon;
        }
        static IPolygon GetPolygon(IPolyline pPlyine)
        {
            IPolygon pPolygon = ConstructPolygonFromPolyline(pPlyine);
            if ((pPolygon != null) && (!pPolygon.IsEmpty))
            {
                if (!pPolygon.IsClosed)
                {
                    pPolygon.Close();
                }
            }
            return pPolygon;
        }      
   }
}

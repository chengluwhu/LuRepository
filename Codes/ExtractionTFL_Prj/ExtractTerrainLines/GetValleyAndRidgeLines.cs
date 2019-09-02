using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ExtractFeaturePts;
//using IntelligentSearchMethod;
using DataStructure;
using PieceLineSmoothing_3OrderPolyno;
using CreateBoundary;
namespace ExtractTerrainLines
{
    public class GetValleyAndRidgeLines
    {
        //// local variable        
        static List<List<List<FeaPtInform>>> regionOfGroupPts = new List<List<List<FeaPtInform>>>(); 
        static Dictionary<IPolyline, int> terrainFeaLines = new Dictionary<IPolyline, int>();                                                                                                                
        static List<List<FeaPtInform>> valleyOrRidgeExtensiveStore = new List<List<FeaPtInform>>();             
        static Dictionary<List<FeaPtInform>,double > saveClosedPts = new Dictionary<List<FeaPtInform>,double> (); 
        static List<List<FeaPtInform>> allValleyFeaPtGroups = new List<List<FeaPtInform>>(); 
        static List<List<FeaPtInform>> allRidgeFeaPtGroups = new List<List<FeaPtInform>>(); 
        static Dictionary<IPolyline, int> boundsPly = new Dictionary<IPolyline, int>(); 
        static Dictionary<List<FeaPtInform>, Dictionary<List<FeaPtInform>, IPoint>> sameZPtsAndSaddPtDic = new Dictionary<List<FeaPtInform>, Dictionary<List<FeaPtInform>, IPoint>>(); 
        static List<IPoint> valleyOrRdigePts = new List<IPoint>(); 
        static List<FeaPtInform> fristPtList = new List<FeaPtInform>();               
        //Extract terrain lines function
        static public void TerrainFeaLinesFun(IFeatureLayer terlkFyr, IFeatureLayer feaPtFyr, IFeatureLayer newTerrainFyr)
        {
            GetFeaturePtGroups.GetValleyOrRidgeFeaPtGroups(feaPtFyr);
            SortFeaturePtsAndTerlks.GetTerlkFun(terlkFyr);
            boundsPly = GetBounds.GetNewBoundAndPtsDataFun(terlkFyr);
            allValleyFeaPtGroups = GetFeaturePtGroups.allValleyFeaPtGroups;
            for (int i = 0; i < allValleyFeaPtGroups.Count; i++)
            {
                valleyOrRidgeExtensiveStore = new List<List<FeaPtInform>>();
                regionOfGroupPts = new List<List<List<FeaPtInform>>>();
                List<FeaPtInform> feaPtGroup = allValleyFeaPtGroups[i];
                valleyOrRidgeExtensiveStore.Add(feaPtGroup);
                RecTreeStructSearch(terlkFyr, feaPtGroup, true);
                GetFeaturePtGroups.allValleyFeaPtGroups.Remove(feaPtGroup);
                Dictionary<List<List<FeaPtInform>>, double> averageSlopeDic = GetMainOrBranchStream(terlkFyr, true);
                if (averageSlopeDic == null)
                {
                    i = i - 1; continue;
                }
                GetTerrainFeatureLines(terlkFyr, newTerrainFyr, averageSlopeDic, true);
                i = i - 1;

            }
            for (int i = 0; i < terrainFeaLines.Count; i++)
            {
                PublicFunctionClass.CreateStructPly(newTerrainFyr, terrainFeaLines.Keys.ElementAt(i), terrainFeaLines.Values.ElementAt(i), true);
            }
            allRidgeFeaPtGroups = GetFeaturePtGroups.allRidgeFeaPtGroups;
            terrainFeaLines = new Dictionary<IPolyline, int>();
            for (int i = 0; i < allRidgeFeaPtGroups.Count; i++)
            {
                valleyOrRidgeExtensiveStore = new List<List<FeaPtInform>>();
                regionOfGroupPts = new List<List<List<FeaPtInform>>>();
                List<FeaPtInform> feaPtGroup = allRidgeFeaPtGroups[i];
                sameZPtsAndSaddPtDic = new Dictionary<List<FeaPtInform>, Dictionary<List<FeaPtInform>, IPoint>>();
                valleyOrRidgeExtensiveStore.Add(feaPtGroup);
                RecTreeStructSearch(terlkFyr, feaPtGroup, false);
                GetFeaturePtGroups.allRidgeFeaPtGroups.Remove(feaPtGroup);
                Dictionary<List<List<FeaPtInform>>, double> averageSlopeDic = GetMainOrBranchStream(terlkFyr, false);
                if (averageSlopeDic == null)
                {
                    i = i - 1;
                    continue;
                }
                GetTerrainFeatureLines(terlkFyr, newTerrainFyr, averageSlopeDic, false);
                i = i - 1;
            }
            Dictionary<IPolyline, int> newTerrainDic = new Dictionary<IPolyline, int>();
            for (int i = 0; i < saddleFyr.Count; i++)
            {
                newTerrainDic.Add(saddleFyr.Keys.ElementAt(i), saddleFyr.Values.ElementAt(i));
                ITopologicalOperator topSaddlePly = saddleFyr.Keys.ElementAt(i) as ITopologicalOperator;
                IRelationalOperator ppp = saddleFyr.Keys.ElementAt(i) as IRelationalOperator;
                for (int p = 0; p < terrainFeaLines.Count; p++)
                {
                    IPolyline ppy = terrainFeaLines.Keys.ElementAt(p);
                    int code = terrainFeaLines.Values.ElementAt(p);
                    if (code == 1)
                    {
                      bool isOverLap=  ppp.Overlaps(ppy);
                        IGeometry pgeo = topSaddlePly.Intersect(ppy, esriGeometryDimension.esriGeometry0Dimension);
                        if (pgeo.IsEmpty == false&&isOverLap==true)
                        {
                            IPointCollection ptPgeo = pgeo as IPointCollection;
                            IPoint pt = ptPgeo.get_Point(0);                                                       
                             terrainFeaLines.Remove(ppy);
                              break;                            
                        }
                    }

                }
            }
            int kk = 0;
            for (int i = 0; i < terrainFeaLines.Count; i++)
            {
                IPolyline ply = terrainFeaLines.Keys.ElementAt(i);
                int code = terrainFeaLines.Values.ElementAt(i);
                if (code==4)
                {
                    ITopologicalOperator topo = ply as ITopologicalOperator;
                    for (int j = 0; j < saddleFyr.Count; j++)
                    {
                        IPolyline saddlePly = saddleFyr.Keys.ElementAt(j);
                        IGeometry pgeo = topo.Intersect(saddlePly,esriGeometryDimension.esriGeometry0Dimension);
                        if (pgeo.IsEmpty==false)
                       {                            
                            newTerrainDic.Add(terrainFeaLines.Keys.ElementAt(i), 1);
                            terrainFeaLines.Remove(terrainFeaLines.Keys.ElementAt(i));
                            kk = 1;
                            break;
                        }                       
                    }
                }
                if (kk==1)
                {
                    break;
                }          
            }
            for (int i = 0; i < terrainFeaLines.Count; i++)
            {
                newTerrainDic.Add(terrainFeaLines.Keys.ElementAt(i), terrainFeaLines.Values.ElementAt(i));                   
             }    
            for (int i = 0; i < newTerrainDic.Count; i++)
            {
                PublicFunctionClass.CreateStructPly(newTerrainFyr, newTerrainDic.Keys.ElementAt(i), newTerrainDic.Values.ElementAt(i), false);
            }
           
        }

        static int Count = 0; static int indexRecord = 0; 
        static Dictionary<FeaPtInform, double> temperStoreRemovedFeaPt = new Dictionary<FeaPtInform, double>();
        static Dictionary<IPolyline, int> saddleFyr = new Dictionary<IPolyline, int>();
        static  private void GetTerrainFeatureLines(IFeatureLayer terlkFyr,IFeatureLayer newTerain, Dictionary<List<List<FeaPtInform>>, double> averageSlopeDic, bool isValOrRidge)
        {
            Count = averageSlopeDic.Count;
           
            Dictionary<List<List<FeaPtInform>>, double> newSlopeDic = new Dictionary<List<List<FeaPtInform>>, double>();
            if (isValOrRidge==false)
            {
                for (int i = 0; i < averageSlopeDic.Count; i++)
                {
                    List<List<FeaPtInform>> feaPtGroups = averageSlopeDic.Keys.ElementAt(i);
                    List<FeaPtInform> fptClass_1 = feaPtGroups[feaPtGroups.Count - 1];
                    ITopologicalOperator pTopoOper = fptClass_1[fptClass_1.Count / 2].PtCoord as ITopologicalOperator;
                    IPolygon pBufferPoly = pTopoOper.Buffer(70) as IPolygon;
                    List<IPoint> saddlePtList = GetSaddleOrPeakPts(pBufferPoly, SortFeaturePtsAndTerlks.sortedSaddlePts);
                    if (saddlePtList.Count != 0)
                    {
                        newSlopeDic.Add(feaPtGroups, averageSlopeDic.Values.ElementAt(i));
                        averageSlopeDic.Remove(feaPtGroups);
                        break;

                    }
                }
                for (int i = 0; i < averageSlopeDic.Count; i++)
                {
                    newSlopeDic.Add(averageSlopeDic.Keys.ElementAt(i), averageSlopeDic.Values.ElementAt(i));
                }
                averageSlopeDic = newSlopeDic;
            }           
            for (int j = 0; j < averageSlopeDic.Count; j++)
             {
                 
                indexRecord = j;
                valleyOrRdigePts = new List<IPoint>(); 
                temperStoreRemovedFeaPt = new Dictionary<FeaPtInform, double>();
                List<List<FeaPtInform>> feaPtGroups = averageSlopeDic.Keys.ElementAt(j);               
                List<FeaPtInform> fptClass_1 = feaPtGroups[feaPtGroups.Count-1];                 
                FeaPtInform  currentOptiumPt = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, fptClass_1, null, null, isValOrRidge);               
                CirculationFun(terlkFyr, newTerain,feaPtGroups, currentOptiumPt, feaPtGroups.Count - 2, isValOrRidge);
                
             }
            
        }
        static private void CirculationFun(IFeatureLayer terlkFyr, IFeatureLayer newTerain, List<List<FeaPtInform>> feaPtGroups, FeaPtInform fptInfo, int index, bool isValOrRidge)
        {
            #region local variable
            ITopologicalOperator pTopoOper = null;
            IPolygon pBufferPoly = new PolygonClass();
            bool isContainSaddPt_Ridge = false;
            IPolyline midPly = new PolylineClass();
            IPointCollection midPtCol = new MultipointClass();
            IPolyline terrainFly = new PolylineClass();
            IPolyline stepPly = new PolylineClass();
            List<IPoint> saddlist = new List<IPoint>();                
            double distance = 0;
            List<FeaPtInform> nextFptInfoList = new List<FeaPtInform>();
            ILine newLine = new LineClass();
            double leNew = 0;
            double leOld = 0;
            IPoint fristPt = new PointClass();
            IPoint secondPt = new PointClass();
            int startStep = 0;
            double adjustiveAngle = 5;
            int direction = 0;         
            IPolyline leftPly = new PolylineClass();
            IPolyline rightPly = new PolylineClass();
            double l = 0; double r = 0;
            Dictionary<IPoint, double> leftDic = new Dictionary<IPoint, double>();
            Dictionary<IPoint, double> rightDic = new Dictionary<IPoint, double>();
            double h = 0;
            int isRunin = 0;
            bool isBranch = false;
            int isEncounter = 0;
            List<IPolyline> closePlyList = new List<IPolyline>();
            List<IPolyline> ownPlyList = new List<IPolyline>();
            IPolyline ownLeftPly = new PolylineClass();
            IPolyline ownRightPly = new PolylineClass();
            IPolyline closeLeftPly = new PolylineClass();
            IPolyline closeRightPly = new PolylineClass();
            FeaPtInform newFptClass = new FeaPtInform();
            Dictionary<IPoint, double> temperDownAndUpDic = new Dictionary<IPoint, double>();
            int isCrossMidLine = 0;
            double Z=0;            
            #endregion
            #region Judging saddle points 
            if (index == feaPtGroups.Count - 2) 
            {
                pTopoOper = fptInfo.PtCoord as ITopologicalOperator;
                pBufferPoly = pTopoOper.Buffer(70) as IPolygon;
                List<IPoint> saddlePtList = GetSaddleOrPeakPts(pBufferPoly, SortFeaturePtsAndTerlks.sortedSaddlePts);
                if (saddlePtList.Count != 0)
                {
                   
                    IPoint upPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 0, 3);
                    upPt.Z = 10;
                    if (isValOrRidge == true)
                    {
                        saddlist.Add(saddlePtList[0]);
                        saddlist.Add(upPt);
                    }
                    else
                    {
                        if (sameZPtsAndSaddPtDic.Count == 0)
                        {
                            saddlist.Add(saddlePtList[0]);
                            saddlist.Add(upPt);
                        }
                    }
                    isContainSaddPt_Ridge = true;
                }
                valleyOrRdigePts.Add(fptInfo.PtCoord);
                temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                #region method
                if (isValOrRidge == false)
                {
                    for (int i = 0; i < sameZPtsAndSaddPtDic.Count; i++)
                    {
                        List<FeaPtInform> feaPtInfo = sameZPtsAndSaddPtDic.Keys.ElementAt(i); 
                        if (feaPtInfo[0].PtAtPlyOid == fptInfo.PtAtPlyOid && isContainSaddPt_Ridge ==true)
                        {
                            Dictionary<List<FeaPtInform>, IPoint> saddleArea = sameZPtsAndSaddPtDic.Values.ElementAt(i); 
                            if (saddleArea.Keys.ElementAt(0).Count!=0)
                            {
                                                                                               
                                List<IPolyline> perpLines = PublicFunctionClass.CreatePerpendicularLine(fptInfo, terlkFyr, isValOrRidge);
                              
                                List<IPolyline> closePly = GetCloseTerlkFea(terlkFyr, perpLines[0], perpLines[1], fptInfo.Elev, isValOrRidge);                               
                                saddlist.Add(saddleArea.Values.ElementAt(0));
                                saddlist.Add(fptInfo.PtCoord);
                                if (temperStoreRemovedFeaPt.ContainsKey(fptInfo) == false)
                                {
                                    temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                                }
                                isContainSaddPt_Ridge = true;
                                terrainFly = PublicFunctionClass.CreatePly(saddlist);
                                saddleFyr.Add(terrainFly, 1);
                                
                               
                                List<IPoint> otherSaddlist = new List<IPoint>();
                                FeaPtInform fpt = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, saddleArea.Keys.ElementAt(0), null, null, isValOrRidge);
                                double d1 = Math.Sqrt(Math.Pow(fpt.PtCoord.X - saddleArea.Values.ElementAt(0).X, 2) + Math.Pow(fpt.PtCoord.Y - saddleArea.Values.ElementAt(0).Y, 2));
                                double Z_1 = fpt.Elev;
                                if (d1 > 70)
                                {

                                    midPly = SearchMethod.intelligentSearchMethod(terlkFyr, fpt, closePly[0], closePly[1], saddleArea.Values.ElementAt(0), 0);
                                    midPtCol = midPly as IPointCollection;
                                    otherSaddlist.Add(saddleArea.Values.ElementAt(0));
                                    for (int i_1 = midPtCol.PointCount - 1; i_1 > 0; i_1--)
                                    {
                                        otherSaddlist.Add(midPtCol.get_Point(i_1));
                                    }
                                }
                                else
                                {
                                    saddlist.Add(saddleArea.Values.ElementAt(0));
                                }
                                otherSaddlist.Add(fpt.PtCoord);
                                IFeature closeTerlk = terlkFyr.FeatureClass.GetFeature(fpt.PtAtPlyOid);
                                int markIndex = closeTerlk.Fields.FindField("Mark");
                                short mmk = (short)closeTerlk.get_Value(markIndex);
                                #region  
                                if (mmk == 0)
                                {
                                     
                                    IPointCollection ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                                    IPolygon polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                                    IRelationalOperator relation = polygon as IRelationalOperator;
                                    List<List<FeaPtInform>> feaList = new List<List<FeaPtInform>>();
                                    for (int i_j = 0; i_j < 1; i_j++)
                                    {
                                        closeTerlk = terlkFyr.FeatureClass.GetFeature(fpt.PtAtPlyOid);
                                        ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                                        polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                                        relation = polygon as IRelationalOperator;
                                        feaList = new List<List<FeaPtInform>>();
                                        for (int i_1 = 0; i_1 < saveClosedPts.Count; i_1++)
                                        {
                                            double z = saveClosedPts.Values.ElementAt(i_1);
                                            List<FeaPtInform> fList = saveClosedPts.Keys.ElementAt(i_1);
                                            if (Math.Abs(z - Z_1) == PublicFunctionClass.intervalValue && z > Z_1 && relation.Contains(fList[0].PtCoord) == true)
                                            {
                                                List<FeaPtInform> ptList = saveClosedPts.Keys.ElementAt(i_1);
                                                feaList.Add(ptList);
                                            }
                                        }
                                        if (feaList.Count == 0) 
                                        {
                                            for (int p = 0; p < SortFeaturePtsAndTerlks.sortedPeakPts.Count; p++)
                                            {
                                                FeaPtInform fpt_1 = SortFeaturePtsAndTerlks.sortedPeakPts[p];
                                                if (Math.Abs(fpt.Elev - fpt_1.Elev) <= PublicFunctionClass.intervalValue && fpt_1.PtAtPlyOid == fpt.PtAtPlyOid)
                                                {
                                                    otherSaddlist.Add(fpt_1.PtCoord); break;
                                                }
                                            }
                                            otherSaddlist.Insert(0, saddleArea.Values.ElementAt(0));
                                            terrainFly = PublicFunctionClass.CreatePly(otherSaddlist);
                                            saddleFyr.Add(terrainFly, 1);
                                                        
                                        }
                                        else
                                        {
                                            FeaPtInform fpt_1 = new FeaPtInform();
                                            
                                            if (feaList.Count == 1)
                                            {
                                                List<FeaPtInform> list = feaList[0];
                                                 
                                                double dis_1 = 1000000000;
                                                for (int i_1 = 0; i_1 < list.Count; i_1++)
                                                {
                                                    double d = Math.Sqrt(Math.Pow(fpt.PtCoord.X - list[i_1].PtCoord.X, 2) + Math.Pow(fpt.PtCoord.Y - list[i_1].PtCoord.Y, 2));
                                                    if (d < dis_1)
                                                    {
                                                        dis_1 = d;
                                                        fpt_1 = list[i_1];
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                
                                                double dis_1 = 1000000000;
                                                List<FeaPtInform> shortDList = new List<FeaPtInform>();
                                                for (int i_1 = 0; i_1 < feaList.Count; i_1++)
                                                {
                                                    List<FeaPtInform> list = feaList[i_1];
                                                    double d = Math.Sqrt(Math.Pow(fpt.PtCoord.X - list[0].PtCoord.X, 2) + Math.Pow(fpt.PtCoord.Y - list[0].PtCoord.Y, 2));
                                                    if (d < dis_1)
                                                    {
                                                        dis_1 = d;
                                                        shortDList = list;
                                                    }
                                                }
                                                
                                                dis_1 = 1000000000;
                                                for (int i_1 = 0; i_1 < shortDList.Count; i_1++)
                                                {
                                                    double d = Math.Sqrt(Math.Pow(fpt.PtCoord.X - shortDList[i_1].PtCoord.X, 2) + Math.Pow(fpt.PtCoord.Y - shortDList[i_1].PtCoord.Y, 2));
                                                    if (d < dis_1)
                                                    {
                                                        dis_1 = d;
                                                        fpt_1 = shortDList[i_1];
                                                    }
                                                }
                                                otherSaddlist.Add(fpt_1.PtCoord);
                                                fpt = fpt_1;
                                                Z = fpt_1.Elev;
                                            }
                                             
                                            IPoint downPt = UpAndDownAngleBisector(terlkFyr, fpt, isValOrRidge, 1, 3);
                                            ILine downLine = new LineClass();
                                            downLine.FromPoint = fpt.PtCoord;
                                            downLine.ToPoint = downPt;
                                            otherSaddlist.Add(downPt);
                                             
                                            IPoint upPt = UpAndDownAngleBisector(terlkFyr, fpt_1, isValOrRidge, 0, 3);
                                            ILine upLine = new LineClass();
                                            upLine.FromPoint = upPt;
                                            upLine.ToPoint = fpt_1.PtCoord;
                                                  
                                            GetCalculatedPoints.dTightness = 0.3;
                                            Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(downPt.X, downPt.Y, upPt.X, upPt.Y, downLine.Angle, upLine.Angle, 5);
                                            for (int ii = 0; ii < ptDic.Count; ii++)
                                            {
                                                IPoint newPt = new PointClass();
                                                newPt.X = ptDic.Keys.ElementAt(ii);
                                                newPt.Y = ptDic.Values.ElementAt(ii);
                                                newPt.Z = 10;
                                                otherSaddlist.Add(newPt);
                                            }
                                            otherSaddlist.Add(fpt_1.PtCoord);
                                            fpt = fpt_1;
                                            Z = fpt_1.Elev;
                                            i_j = -1;
                                            feaList = new List<List<FeaPtInform>>();
                                        }
                                    }
                                    break;
                                }
                                #endregion
                                #region  
                                else
                                {
                                    otherSaddlist.Insert(0,saddleArea.Values.ElementAt(0));
                                    terrainFly = PublicFunctionClass.CreatePly(otherSaddlist);
                                    saddleFyr.Add(terrainFly, 1);
                                    
                                   
                                    IPointCollection ptc = closeTerlk.Shape as IPointCollection;
                                    IPolygon polyGonGeo = GetPolygonFun(closeTerlk, 0, ptc.PointCount);
                                    List<IPoint> peakPtList = GetSaddleOrPeakPts(polyGonGeo, SortFeaturePtsAndTerlks.sortedPeakPts);
                                    if (peakPtList.Count==0)  
                                    {
                                       
                                        double dis = 100000;
                                        int boundIndex = 0;
                                        for (int j = 0; j < boundsPly.Count; j++)
                                        {
                                            IPolyline boundPly = boundsPly.Keys.ElementAt(j);
                                            int indexBound = boundsPly.Values.ElementAt(j);
                                            Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
                                            double distanceAlongCurve = 0;          
                                            double distanceFromCurve = 0;              
                                            bool bRightSide = false;
                                            IPoint outPt = new PointClass();
                                            boundPly.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ptc.get_Point(0), false, outPt, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                                            if (distanceFromCurve<dis)
                                            {
                                                dis = distanceFromCurve;
                                                boundIndex = j;                                                
                                            }
                                        }
                                                       
                                        List<IPolyline> pyList = LeftAndRightPlyFun(terlkFyr, fpt, isValOrRidge);
                                                            
                                        intelligentMethod.fptList = saddleArea.Keys.ElementAt(0);
                                        IPolyline ply = intelligentMethod.intelligentSearchMethod(terlkFyr, fpt, pyList[0], pyList[1], boundsPly.Keys.ElementAt(boundIndex), isValOrRidge);
                                        saddleFyr.Add(ply, 1);
                                        
                                    }
                                   
                                }
                                #endregion
                            }                        
                        }
                    }
                }
                #endregion
            }
            #endregion 
            while (true)
            {              
                if (isRunin == 0 && index >= 0)
                {
                    nextFptInfoList = feaPtGroups[index];                    
                    distance = GetDistanceBetweenPts(fptInfo, nextFptInfoList);
                }
                #region   
                if (index<0)
                {
                    
                    if ((isValOrRidge == true && Count >= 2 && indexRecord == 0) || (isValOrRidge == true && Count >= 2 && fptInfo.Elev==feaPtGroups[0][0].Elev))//走中线,判断与边界是否相交
                    {
                        isCrossMidLine = 1;
                    }
                    else if ((isValOrRidge == true && Count == 1 && feaPtGroups.Count > 2) || (isValOrRidge == false && Count == 1 && feaPtGroups.Count > 2 ))//走中线，判断是否与主支流相交，即
                    {
                        isCrossMidLine=2;
                    }
                    else if ((isValOrRidge == false && Count > 1 && isCrossMidLine != 3 && isCrossMidLine != 1))//|| (isValOrRidge == false && Count == 1 && mmk_1 == 0)
                    {
                        IFeature closeTerlk = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid);
                        int markIndex = closeTerlk.Fields.FindField("Mark");
                        short mmk_1 = (short)closeTerlk.get_Value(markIndex);
                        Z = fptInfo.Elev;
                        #region  
                        IPointCollection ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                        IPolygon polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                        IRelationalOperator relation = polygon as IRelationalOperator;
                        List<List<FeaPtInform>> feaList = new List<List<FeaPtInform>>();
                        int mark = 0;
                        for (int i_j = 0; i_j < 1; i_j++)
                        {
                            closeTerlk = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid);
                            ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                            polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                            relation = polygon as IRelationalOperator;                            
                            for (int i_1 = 0; i_1 < saveClosedPts.Count; i_1++)
                            {
                                double z = saveClosedPts.Values.ElementAt(i_1);
                                List<FeaPtInform> fList = saveClosedPts.Keys.ElementAt(i_1);
                                if (Math.Abs(z - Z) == PublicFunctionClass.intervalValue && z > Z && relation.Contains(fList[0].PtCoord) == true)
                                {
                                    List<FeaPtInform> ptList = saveClosedPts.Keys.ElementAt(i_1);
                                    feaList.Add(ptList);
                                }
                            }
                            if (feaList.Count == 0) 
                            {
                               
                                for (int p = 0; p < SortFeaturePtsAndTerlks.sortedPeakPts.Count; p++)
                                {
                                    FeaPtInform fpt_1 = SortFeaturePtsAndTerlks.sortedPeakPts[p];
                                    if (Math.Abs(fptInfo.Elev - fpt_1.Elev) <= PublicFunctionClass.intervalValue && fpt_1.PtAtPlyOid == fptInfo.PtAtPlyOid)
                                    {
                                        mark = 1;
                                        valleyOrRdigePts.Add(fpt_1.PtCoord);
                                        break;
                                    }
                                }
                                if (mark == 1)
                                {
                                    terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                    if (isContainSaddPt_Ridge == true||Count==3)
                                    {
                                        terrainFeaLines.Add(terrainFly, 1);
                                    }
                                    else
                                    {
                                        terrainFeaLines.Add(terrainFly, 2);
                                    }                                    
                                }
                                else
                                {

                                    IPointCollection ptCC = closeTerlk.Shape as IPointCollection;
                                    IPolyline ply=new PolylineClass ();
                                    ply.FromPoint=ptCC.get_Point(0);
                                    ply.ToPoint=ptCC.get_Point(ptCC.PointCount-1);
                                     ITopologicalOperator topo=ply  as ITopologicalOperator ;
                                     
                                     int interset = 0;
                                    for (int i = 0; i < SortFeaturePtsAndTerlks.openTerlkStored.Count; i++)
                                    {
                                        IPolyline  fea = SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i).Shape as IPolyline ;
                                        double zz = SortFeaturePtsAndTerlks.openTerlkStored.Values.ElementAt(i);
                                        if (zz>Z )
                                        {
                                            IGeometry pgo = topo.Intersect(fea, esriGeometryDimension.esriGeometry0Dimension);
                                            if (pgo.IsEmpty==false)
                                            {
                                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                                isEncounter = 1;
                                                break;
                                            }
                                            else
                                            {
                                                interset = 0;
                                            }
                                        }
                                    }
                                    if (isContainSaddPt_Ridge == true || interset == 0)
                                    {
                                         isCrossMidLine = 1;
                                    }
                                    else    
                                    {
                                        isCrossMidLine = 3;
                                    }
                                   
                                }
                            }
                            else
                            {
                                FeaPtInform fpt_1 = new FeaPtInform();
                                
                                if (feaList.Count == 1)
                                {
                                    List<FeaPtInform> list = feaList[0];
                                    
                                    double dis_1 = 1000000000;
                                    for (int i_1 = 0; i_1 < list.Count; i_1++)
                                    {
                                        double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - list[i_1].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - list[i_1].PtCoord.Y, 2));
                                        if (d < dis_1)
                                        {
                                            dis_1 = d;
                                            fpt_1 = list[i_1];
                                        }
                                    }
                                }
                                else
                                {
                                    
                                    double dis_1 = 1000000000;
                                    List<FeaPtInform> shortDList = new List<FeaPtInform>();
                                    for (int i_1 = 0; i_1 < feaList.Count; i_1++)
                                    {
                                        List<FeaPtInform> list = feaList[i_1];
                                        double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - list[0].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - list[0].PtCoord.Y, 2));
                                        if (d < dis_1)
                                        {
                                            dis_1 = d;
                                            shortDList = list;
                                        }
                                    }
                                     
                                    dis_1 = 1000000000;
                                    for (int i_1 = 0; i_1 < shortDList.Count; i_1++)
                                    {
                                        double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - shortDList[i_1].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - shortDList[i_1].PtCoord.Y, 2));
                                        if (d < dis_1)
                                        {
                                            dis_1 = d;
                                            fpt_1 = shortDList[i_1];
                                        }
                                    }
                                     
                                }
                                
                                IPoint downPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 1, 3);
                                ILine downLine = new LineClass();
                                downLine.FromPoint = fptInfo.PtCoord;
                                downLine.ToPoint = downPt;
                                valleyOrRdigePts.Add(downPt);
                                 
                                IPoint upPt = UpAndDownAngleBisector(terlkFyr, fpt_1, isValOrRidge, 0, 3);
                                ILine upLine = new LineClass();
                                upLine.FromPoint = upPt;
                                upLine.ToPoint = fpt_1.PtCoord;
                                    
                                GetCalculatedPoints.dTightness = 0.3;
                                Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(downPt.X, downPt.Y, upPt.X, upPt.Y, downLine.Angle, upLine.Angle,5);
                                for (int ii = 0; ii < ptDic.Count; ii++)
                                {
                                    IPoint newPt = new PointClass();
                                    newPt.X = ptDic.Keys.ElementAt(ii);
                                    newPt.Y = ptDic.Values.ElementAt(ii);
                                    newPt.Z = 10;
                                    valleyOrRdigePts.Add(newPt);
                                }
                                valleyOrRdigePts.Add(fpt_1.PtCoord);
                                fptInfo = fpt_1;
                                Z = fpt_1.Elev;
                                i_j = -1;
                                feaList = new List<List<FeaPtInform>>();
                            }
                        }
                        if (mark==1||isEncounter==1)
                        {
                            break;
                        }
                        #endregion                         
                    }
                    else if ((Count == 1 && feaPtGroups.Count == 2)) 
                    {
                        IFeature closeTerlk = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid);
                        int markIndex = closeTerlk.Fields.FindField("Mark");
                        short mmk_1 = (short)closeTerlk.get_Value(markIndex);
                        Z = fptInfo.Elev;
                        if (mmk_1==0)
                        {
                            IPointCollection ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                            IPolygon polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                            IRelationalOperator relation = polygon as IRelationalOperator;
                            List<List<FeaPtInform>> feaList = new List<List<FeaPtInform>>();                             
                            for (int i_j = 0; i_j < 1; i_j++)
                            {
                                closeTerlk = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid);
                                ptOfCloseTer = closeTerlk.Shape as IPointCollection;
                                polygon = GetPolygonFun(closeTerlk, 0, ptOfCloseTer.PointCount - 2);
                                relation = polygon as IRelationalOperator;
                                for (int i_1 = 0; i_1 < saveClosedPts.Count; i_1++)
                                {
                                    double z = saveClosedPts.Values.ElementAt(i_1);
                                    List<FeaPtInform> fList = saveClosedPts.Keys.ElementAt(i_1);
                                    if (Math.Abs(z - Z) == PublicFunctionClass.intervalValue && z > Z && relation.Contains(fList[0].PtCoord) == true)
                                    {
                                        List<FeaPtInform> ptList = saveClosedPts.Keys.ElementAt(i_1);
                                        feaList.Add(ptList);
                                    }
                                }
                                if (feaList.Count == 0) 
                                {

                                    for (int p = 0; p < SortFeaturePtsAndTerlks.sortedPeakPts.Count; p++)
                                    {
                                        FeaPtInform fpt_1 = SortFeaturePtsAndTerlks.sortedPeakPts[p];
                                        if (Math.Abs(fptInfo.Elev - fpt_1.Elev) <= PublicFunctionClass.intervalValue && fpt_1.PtAtPlyOid == fptInfo.PtAtPlyOid)
                                        {
                                            
                                            valleyOrRdigePts.Add(fpt_1.PtCoord);
                                            terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                            terrainFeaLines.Add(terrainFly, 1);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    FeaPtInform fpt_1 = new FeaPtInform();
                                    
                                    if (feaList.Count == 1)
                                    {
                                        List<FeaPtInform> list = feaList[0];
                                       
                                        double dis_1 = 1000000000;
                                        for (int i_1 = 0; i_1 < list.Count; i_1++)
                                        {
                                            double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - list[i_1].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - list[i_1].PtCoord.Y, 2));
                                            if (d < dis_1)
                                            {
                                                dis_1 = d;
                                                fpt_1 = list[i_1];
                                            }
                                        }
                                    }
                                    else
                                    {
                                       
                                        double dis_1 = 1000000000;
                                        List<FeaPtInform> shortDList = new List<FeaPtInform>();
                                        for (int i_1 = 0; i_1 < feaList.Count; i_1++)
                                        {
                                            List<FeaPtInform> list = feaList[i_1];
                                            double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - list[0].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - list[0].PtCoord.Y, 2));
                                            if (d < dis_1)
                                            {
                                                dis_1 = d;
                                                shortDList = list;
                                            }
                                        }
                                       
                                        dis_1 = 1000000000;
                                        for (int i_1 = 0; i_1 < shortDList.Count; i_1++)
                                        {
                                            double d = Math.Sqrt(Math.Pow(fptInfo.PtCoord.X - shortDList[i_1].PtCoord.X, 2) + Math.Pow(fptInfo.PtCoord.Y - shortDList[i_1].PtCoord.Y, 2));
                                            if (d < dis_1)
                                            {
                                                dis_1 = d;
                                                fpt_1 = shortDList[i_1];
                                            }
                                        }                                                                                 
                                    }
                                   
                                    IPoint downPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 1, 3);
                                    ILine downLine = new LineClass();
                                    downLine.FromPoint = fptInfo.PtCoord;
                                    downLine.ToPoint = downPt;
                                    valleyOrRdigePts.Add(downPt);
                                    
                                    IPoint upPt = UpAndDownAngleBisector(terlkFyr, fpt_1, isValOrRidge, 0, 3);
                                    ILine upLine = new LineClass();
                                    upLine.FromPoint = upPt;
                                    upLine.ToPoint = fpt_1.PtCoord;
                                           
                                    GetCalculatedPoints.dTightness = 0.3;
                                    Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(downPt.X, downPt.Y, upPt.X, upPt.Y, downLine.Angle, upLine.Angle, 5);
                                    for (int ii = 0; ii < ptDic.Count; ii++)
                                    {
                                        IPoint newPt = new PointClass();
                                        newPt.X = ptDic.Keys.ElementAt(ii);
                                        newPt.Y = ptDic.Values.ElementAt(ii);
                                        newPt.Z = 10;
                                        valleyOrRdigePts.Add(newPt);
                                    }
                                    valleyOrRdigePts.Add(fpt_1.PtCoord);
                                    fptInfo = fpt_1;
                                    Z = fpt_1.Elev;
                                    i_j = -1;
                                    feaList = new List<List<FeaPtInform>>();
                                }
                            }
                           
                        }
                        else
                        {
                            terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                            CreateFeatureLines(terrainFly, isValOrRidge);
                                                  
                        }
                        break; 
                        
                    }                   
                    if (isCrossMidLine!=0)
                    {
                        #region  
                        if (fptInfo.PtAtNumber != 0)
                        {
                            Z=fptInfo.Elev;
                            isBranch = fptInfo.IsAffluent;
                            fristPt = fptInfo.PtCoord;
                            
                            ownPlyList = LeftAndRightPlyFun(terlkFyr, fptInfo, isValOrRidge);
                            ownLeftPly = ownPlyList[0];
                            ownRightPly = ownPlyList[1];
                            leftPly = ownLeftPly;
                            rightPly = ownRightPly;
                            IPointCollection leftPtCol = leftPly as IPointCollection;
                            IPointCollection rightPtCol = rightPly as IPointCollection;
                            IPoint angPt = PublicFunctionClass.CreateAngleBisectorPt(fptInfo, terlkFyr, isValOrRidge);
                            valleyOrRdigePts.Add(angPt);
                            secondPt = angPt;
                            newLine.FromPoint = fristPt;
                            newLine.ToPoint = secondPt;
                                                                                           
                            leftDic = ReturnMinDistancePt(leftPly, secondPt);
                            rightDic = ReturnMinDistancePt(rightPly, secondPt);
                            l = leftDic.Values.ElementAt(0);
                            r = rightDic.Values.ElementAt(0);
                            h = (l + r) / 5.0;
                            IConstructPoint extendPt = new PointClass();
                            extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                            IPoint nePt = new PointClass();
                            nePt = extendPt as IPoint;
                            newLine.FromPoint = secondPt;
                            newLine.ToPoint = nePt;
                            if (startStep == 0)
                            {
                                fristPt = secondPt;
                                secondPt = nePt;
                            }
                           
                        }
                        #endregion
                        if (startStep != 0)
                        {
                            fristPt = newLine.FromPoint;
                            secondPt = newLine.ToPoint;
                        }
                        leftDic = ReturnMinDistancePt(leftPly, secondPt);
                        rightDic = ReturnMinDistancePt(rightPly, secondPt);
                        l = leftDic.Values.ElementAt(0);
                        r = rightDic.Values.ElementAt(0);
                        double diff = Math.Abs(l - r);
                        if (temperDownAndUpDic.ContainsKey(secondPt) == false)
                        {
                            temperDownAndUpDic.Add(secondPt, diff);
                        }
                        leNew = diff;                        
                        #region  
                        if (diff < 0.03 || startStep > 15)
                        {
                            if (startStep > 15)
                            { 
                                var dicSd = from objDic in temperDownAndUpDic orderby objDic.Value ascending select objDic;

                                foreach (KeyValuePair<IPoint, double> keyv in dicSd)
                                {
                                    secondPt = keyv.Key; break;
                                }
                            }
                            h = (l + r) / 5.0;
                            secondPt.Z = 10;
                            valleyOrRdigePts.Add(secondPt);                       
                           
                            IConstructPoint extendPt = new PointClass();
                            extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                            IPoint newPt = extendPt as IPoint;
                            newLine.FromPoint = secondPt; ;
                            newLine.ToPoint = newPt;
                            newFptClass.PtCoord = fristPt;
                            newFptClass.Elev = fptInfo.Elev;
                            fptInfo = newFptClass;
                            leOld = 0;
                            leNew = 0;
                            startStep = 1;
                            adjustiveAngle = 5;
                            temperDownAndUpDic = new Dictionary<IPoint, double>();
                           IPolyline ply = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                            //PublicFunctionClass.CreateStructPly(newTerain, ply, 0, isValOrRidge);
                            if (valleyOrRdigePts.Count<2)
                            {
                                stepPly.FromPoint = fristPt;
                                stepPly.ToPoint = secondPt;
                                newLine.FromPoint = fristPt;
                                newLine.ToPoint = secondPt;
                            }
                            else
                            {
                                stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count-2];
                                stepPly.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];                               
                            }
                            pTopoOper = stepPly as ITopologicalOperator;
                            #region  
                            if (isCrossMidLine == 1)
                            {                              
                                for (int i = 0; i < boundsPly.Count; i++)
                                {
                                    IGeometry pgo = pTopoOper.Intersect(boundsPly.Keys.ElementAt(i), esriGeometryDimension.esriGeometry0Dimension);
                                    if (pgo.IsEmpty == false)
                                    {
                                        IPointCollection ptcc = pgo as IPointCollection;
                                        IPoint pt = ptcc.get_Point(0);
                                        pt.Z = 10;
                                        valleyOrRdigePts.Add(pt);
                                        if (isContainSaddPt_Ridge == true)
                                        {
                                            valleyOrRdigePts.Insert(0, saddlist[0]);
                                            valleyOrRdigePts.Insert(1, saddlist[1]);
                                        }                                                                                
                                        terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                        if (isValOrRidge==true)
                                        {
                                            if ((Count == 2 && indexRecord == 0) || (Count >2 && indexRecord != 0))
                                            {
                                                terrainFeaLines.Add(terrainFly, -2);
                                            }
                                            else
                                            {
                                                terrainFeaLines.Add(terrainFly, -1);
                                            }          
                                        }
                                        else
                                        {                                           
                                          terrainFeaLines.Add(terrainFly, 1);                                                                                                                                   
                                        }
                                        ////PublicFunctionClass.CreateStructPly(newTerain, terrainFly, 0, isValOrRidge);                                                                                                           
                                        isEncounter = 1; 
                                        break;
                                    }
                                }
                               
                                ITopologicalOperator topo = pTopoOper as ITopologicalOperator;
                                for (int i_1 = 0; i_1 < terrainFeaLines.Count; i_1++)
                                {
                                    IPolyline terrPly = terrainFeaLines.Keys.ElementAt(i_1);
                                    IGeometry pgeo_1 = topo.Intersect(terrPly, esriGeometryDimension.esriGeometry0Dimension);//判断与主支流是否相交                                                                                                              
                                    if (pgeo_1.IsEmpty == false) 
                                    {
                                            IPointCollection ptCol = pgeo_1 as IPointCollection;
                                            IPoint pt = ptCol.get_Point(0);
                                            pt.Z = 10;
                                            valleyOrRdigePts.RemoveAt(valleyOrRdigePts.Count-1);
                                            valleyOrRdigePts.Add(pt);
                                            if (isValOrRidge == false && isContainSaddPt_Ridge == true && sameZPtsAndSaddPtDic.Count == 0)
                                            {
                                                valleyOrRdigePts.Insert(0, saddlist[0]);
                                            }
                                            terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                            if (isValOrRidge == false && isContainSaddPt_Ridge == true)
                                            {
                                                terrainFeaLines.Add(terrainFly,1);
                                            }
                                            else
                                            {                                               
                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                            }                                            
                                            isEncounter = 1;
                                            break;
                                   }
                                    
                                }
                            }
                            #endregion 
                            #region  
                            else if (isCrossMidLine == 2 || isCrossMidLine==3)
                            {
                               
                                List<IPolyline> plyList = new List<IPolyline>();
                                for (int i = 0; i < SortFeaturePtsAndTerlks.openTerlkStored.Count; i++)
                                {
                                    IFeature  feature = SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i);
                                    double elev = SortFeaturePtsAndTerlks.openTerlkStored.Values.ElementAt(i);
                                    if (isValOrRidge==true)
                                    {
                                        if (elev < Z && Math.Abs(elev - Z) == PublicFunctionClass.intervalValue)
                                        {
                                            plyList.Add(feature.Shape as IPolyline);
                                        }
                                    }
                                    else
                                    {
                                        if (elev > Z && Math.Abs(elev - Z) == PublicFunctionClass.intervalValue)
                                        {
                                            plyList.Add(feature.Shape as IPolyline);
                                        }
                                    }
                                }
                                if (plyList.Count!=0)
                                {
                                    for (int i = 0; i < plyList.Count; i++)
                                    {
                                        IGeometry pgeo = pTopoOper.Intersect(plyList[i], esriGeometryDimension.esriGeometry0Dimension);
                                        if (pgeo.IsEmpty==false) 
                                        {
                                            extendPt = new PointClass();
                                            extendPt.ConstructAngleDistance(secondPt, newLine.Angle,70);
                                            IPoint newPt_1 = extendPt as IPoint;
                                            IPolyline ply_1 = new PolylineClass();
                                            ply_1.FromPoint = secondPt;
                                            ply_1.ToPoint = newPt_1;
                                            ITopologicalOperator topo = ply_1 as ITopologicalOperator;
                                            for (int i_1 = 0; i_1 < terrainFeaLines.Count; i_1++)
                                            {
                                                IPolyline terrPly = terrainFeaLines.Keys.ElementAt(i_1);
                                                IGeometry pgeo_1 = topo.Intersect(terrPly, esriGeometryDimension.esriGeometry0Dimension); 
                                                if (isCrossMidLine==2)
                                                {
                                                    #region 
                                                    if (pgeo_1.IsEmpty == false) 
                                                    {
                                                        IPointCollection ptCol = pgeo_1 as IPointCollection;
                                                        IPoint pt = ptCol.get_Point(0);
                                                        pt.Z = 10;
                                                        valleyOrRdigePts.Add(pt);
                                                        terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                        CreateFeatureLines(terrainFly, isValOrRidge);
                                                        isEncounter = 1;
                                                        break;
                                                    }
                                                    else 
                                                    {
                                                        for (int i_2 = valleyOrRdigePts.Count - 1; i_2 >= 0; i_2--)
                                                        {

                                                            IPoint ptt = valleyOrRdigePts[i_2];
                                                            if (ptt.Z == 10)
                                                            {
                                                                valleyOrRdigePts.Remove(ptt);
                                                            }
                                                            else
                                                            {
                                                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                                                isEncounter = 1;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    #endregion 
                                                }
                                                else if (isCrossMidLine == 3)
                                                {
                                                    #region
                                                    if (pgeo_1.IsEmpty == false) 
                                                    {
                                                        for (int i_2 = valleyOrRdigePts.Count - 1; i_2 >= 0; i_2--)
                                                        {

                                                            IPoint ptt = valleyOrRdigePts[i_2];
                                                            if (ptt.Z == 10)
                                                            {
                                                                valleyOrRdigePts.Remove(ptt);
                                                            }
                                                            else
                                                            {
                                                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                                                isEncounter = 1;
                                                                break;
                                                            }
                                                        }                                                    
                                                    }
                                                    else
                                                    {
                                                        isCrossMidLine = 1;
                                                    }
                                                    #endregion
                                                }
                                                if (isEncounter == 1)
                                                {
                                                    break;
                                                }
                                            }
                                            if (isEncounter==1)
                                            {
                                                break;
                                            }
                                        }
                                        else  
                                        {
                                            for (int j = 0; j < boundsPly.Count; j++)
                                            {
                                                IGeometry pgo = pTopoOper.Intersect(boundsPly.Keys.ElementAt(j), esriGeometryDimension.esriGeometry0Dimension);
                                                if (pgo.IsEmpty==false)
                                                {
                                                    for (int i_2 = valleyOrRdigePts.Count - 1; i_2 >= 0; i_2--)
                                                    {

                                                        IPoint ptt = valleyOrRdigePts[i_2];
                                                        if (ptt.Z == 10)
                                                        {
                                                            valleyOrRdigePts.Remove(ptt);
                                                        }
                                                        else
                                                        {
                                                            terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                            CreateFeatureLines(terrainFly, isValOrRidge);
                                                            isEncounter = 1;
                                                            break;
                                                        }
                                                    }
                                                    if (isEncounter==1)
                                                    {
                                                        break;
                                                    } 
                                                }                                               
                                            }
                                            if (isEncounter == 1)
                                            {
                                                break;
                                            }
                                        }
                                    }                                    
                                }
                                else
                                {
                                    isEncounter = 1;
                                }
                            }
                            #endregion 
                            if (isEncounter==1)
                            {
                                break;
                            }
                        }
                         #endregion 
                        #region  
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
                                newFptClass.PtCoord = secondPt;
                                newFptClass.Elev = fptInfo.Elev;
                                fptInfo = newFptClass;
                                startStep = 1;
                            }
                            else 
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
                                    newFptClass.PtCoord = fristPt;
                                    newFptClass.Elev = fptInfo.Elev;
                                    fptInfo = newFptClass;
                                    startStep++;
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
                                    newFptClass.PtCoord = fristPt;
                                    newFptClass.Elev = fptInfo.Elev;
                                    fptInfo = newFptClass;
                                    startStep++;
                                }
                                else if (leNew == leOld)
                                {
                                    
                                    IConstructPoint    extendPt = new PointClass();
                                    extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                    IPoint newPt = extendPt as IPoint;
                                    newLine.FromPoint = secondPt; ;
                                    newLine.ToPoint = newPt;
                                    newFptClass.PtCoord = newPt;
                                    newFptClass.Elev = fptInfo.Elev;
                                    fptInfo = newFptClass;
                                    leOld = 0;
                                    leNew = 0;
                                    startStep = 0;
                                    adjustiveAngle = 5;
                                }
                            }                            
                        }
                        #endregion
                    }                           
                }
                #endregion 
                else
                {
                    #region  original values
                    if (fptInfo.PtAtNumber != 0) 
                    {
                        fristPt = fptInfo.PtCoord;
                        secondPt = PublicFunctionClass.CreateAngleBisectorPt(fptInfo, terlkFyr, isValOrRidge);
                    }
                    else
                    {
                        fristPt = newLine.FromPoint;
                        secondPt = newLine.ToPoint;
                    }
                    #endregion
                    #region The processing in dense contour lines regions
                    if (distance > 0 && distance < 70 && index >= 0)
                    {
                        ////////////////// 
                        IPoint downPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 1, 3);
                        ILine downLine = new LineClass();
                        downLine.FromPoint = fptInfo.PtCoord;
                        downLine.ToPoint = downPt;
                        valleyOrRdigePts.Add(downPt);
                        //////////////////// 
                        fptInfo = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, nextFptInfoList, fptInfo, null, false);
                        // 
                        IPoint upPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 0, 3);
                        ILine upLine = new LineClass();
                        upLine.FromPoint = upPt;
                        upLine.ToPoint = fptInfo.PtCoord;
                        //       
                        GetCalculatedPoints.dTightness = 0.3;
                        Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(downPt.X, downPt.Y, upPt.X, upPt.Y, downLine.Angle, upLine.Angle, 5);
                        for (int ii = 0; ii < ptDic.Count; ii++)
                        {
                            IPoint newPt = new PointClass();
                            newPt.X = ptDic.Keys.ElementAt(ii);
                            newPt.Y = ptDic.Values.ElementAt(ii);
                            newPt.Z = 10;
                            valleyOrRdigePts.Add(newPt);
                        }
                        valleyOrRdigePts.Add(fptInfo.PtCoord);
                        temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                        stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                        stepPly.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                        bool isIntersect = false; 
                        IProximityOperator pp = stepPly as IProximityOperator;                        
                        pTopoOper = stepPly as ITopologicalOperator;
                        for (int i = 0; i < terrainFeaLines.Count; i++)
                        {
                            double dis = pp.ReturnDistance(terrainFeaLines.Keys.ElementAt(i));                             
                            if (dis==0)
                            {
                                isIntersect = true;
                                break;
                            }
                        }                        
                        if (isIntersect == true)
                        {
                            if (isContainSaddPt_Ridge == true)
                            {
                                if (isValOrRidge == false)
                                {
                                    IFeature closeTerlk = terlkFyr.FeatureClass.GetFeature(fptInfo.PtAtPlyOid);
                                    int markIndex = closeTerlk.Fields.FindField("Mark");
                                    short mmk_1 = (short)closeTerlk.get_Value(markIndex);
                                    if (mmk_1 == 1 && sameZPtsAndSaddPtDic.Count==0)
                                    {
                                        downPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 1, 3);
                                         
                                        ILine line_1 = new LineClass();
                                        line_1.FromPoint = fptInfo.PtCoord;
                                        line_1.ToPoint = downPt;
                                        
                                        IPointCollection ptCol = closeTerlk.Shape as IPointCollection;
                                        IPoint pt_1 = ptCol.get_Point(0);
                                        IPoint pt_2 = ptCol.get_Point(ptCol.PointCount - 1);
                                        IPoint midPt = new PointClass();
                                        midPt.X = (pt_1.X + pt_2.X) / 2;
                                        midPt.Y = (pt_1.Y + pt_2.Y) / 2;
                                        IConstructPoint constructPt = new PointClass();
                                        constructPt.ConstructAngleBisector(pt_1, midPt, pt_2, 7.8, true);
                                        IPoint newpt = new PointClass();
                                        newpt = constructPt as IPoint;
                                        ILine line_2 = new LineClass();
                                        line_2.FromPoint = midPt;
                                        line_2.ToPoint = newpt;
                                        GetCalculatedPoints.dTightness = 0.3;
                                        ptDic = GetCalculatedPoints.CreateNewPoint(line_1.ToPoint.X, line_1.ToPoint.Y, line_2.ToPoint.X, line_2.ToPoint.Y, line_1.Angle, line_2.Angle, 5);
                                        for (int ii = 0; ii < ptDic.Count; ii++)
                                        {
                                            IPoint newPt = new PointClass();
                                            newPt.X = ptDic.Keys.ElementAt(ii);
                                            newPt.Y = ptDic.Values.ElementAt(ii);
                                            newPt.Z = 10;
                                            valleyOrRdigePts.Add(newPt);
                                        }
                                    }                                     
                                    valleyOrRdigePts.Insert(0, saddlist[0]);
                                    valleyOrRdigePts.Insert(1, saddlist[1]);                                     
                                    terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                    terrainFeaLines.Add(terrainFly, 1);                                 
                                    isEncounter = 1;
                                }
                                else
                                {
                                    valleyOrRdigePts.Insert(0, saddlist[0]);
                                    valleyOrRdigePts.Insert(1, saddlist[1]);
                                    terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                    CreateFeatureLines(terrainFly, isValOrRidge);
                                }
                            }
                            else
                            {
                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                CreateFeatureLines(terrainFly, isValOrRidge);
                                
                            }
                            break;
                        }
                        else if (isIntersect == false && index >= 0)
                        {
                            index = index - 1;
                        }
                        isRunin = 0;
                         
                    }
                    #endregion
                    #region  The processing in gentle contour lines regions
                    else if (distance > 70 && index >= 0)
                    {
                        if (fptInfo.PtAtNumber != 0)
                        {
                            #region  
                            ownPlyList = LeftAndRightPlyFun(terlkFyr, fptInfo, isValOrRidge);
                            ownLeftPly = ownPlyList[0];
                            ownRightPly = ownPlyList[1];
                            if (index == feaPtGroups.Count - 2)
                            {
                                List<IPolyline> perpendicularLines = PublicFunctionClass.CreatePerpendicularLine(fptInfo, terlkFyr, isValOrRidge);
                            
                                closePlyList = GetCloseTerlkFea(terlkFyr, perpendicularLines[0], perpendicularLines[1], fptInfo.Elev, isValOrRidge);
                                if (closePlyList.Count != 0)
                                {
                                    closeLeftPly = closePlyList[0];
                                    closeRightPly = closePlyList[1];
                                }
                            }
                            else
                            {
                                FeaPtInform upFeaPt = new FeaPtInform();
                                for (int f = 0; f < temperStoreRemovedFeaPt.Count; f++)
                                {
                                    double z = temperStoreRemovedFeaPt.Values.ElementAt(f);
                                    FeaPtInform ptFeaList = temperStoreRemovedFeaPt.Keys.ElementAt(f);
                                    if (isValOrRidge == true)
                                    {
                                        if (fptInfo.Elev < z && Math.Abs(fptInfo.Elev - z) == PublicFunctionClass.intervalValue)
                                        {
                                            upFeaPt = ptFeaList;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (fptInfo.Elev > z && Math.Abs(fptInfo.Elev - z) == PublicFunctionClass.intervalValue)
                                        {
                                            upFeaPt = ptFeaList;
                                            break;
                                        }
                                    }
                                }
                                closePlyList = LeftAndRightPlyFun(terlkFyr, upFeaPt, isValOrRidge);
                                if (closePlyList.Count != 0)
                                {
                                    closeLeftPly = closePlyList[0];
                                    closeRightPly = closePlyList[1];
                                }
                            }
                            #endregion
                        } 
                        #region  When meetting branches of feature points groups
                        if ((fptInfo.IsAffluent == true && indexRecord != 0) || closePlyList.Count == 0)
                        {
                            for (int i = 0; i < 1; i++)
                            {
                                #region  
                                if (fptInfo.PtAtNumber != 0)
                                {
                                    isBranch = fptInfo.IsAffluent;
                                    fristPt = fptInfo.PtCoord;
                                    
                                    leftPly = ownLeftPly;
                                    rightPly = ownRightPly;
                                    IPointCollection leftPtCol = leftPly as IPointCollection;
                                    IPointCollection rightPtCol = rightPly as IPointCollection;
                                    IPoint angPt = PublicFunctionClass.CreateAngleBisectorPt(fptInfo, terlkFyr, isValOrRidge);
                                    valleyOrRdigePts.Add(angPt);
                                    secondPt = angPt;
                                    newLine.FromPoint = fristPt;
                                    newLine.ToPoint = secondPt;
                                                                                                     
                                    leftDic = ReturnMinDistancePt(leftPly, secondPt);
                                    rightDic = ReturnMinDistancePt(rightPly, secondPt);
                                    l = leftDic.Values.ElementAt(0);
                                    r = rightDic.Values.ElementAt(0);
                                    h = (l + r) / 8.0;
                                    IConstructPoint extendPt = new PointClass();
                                    extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                    IPoint nePt = new PointClass();
                                    nePt = extendPt as IPoint;
                                    newLine.FromPoint = secondPt;
                                    newLine.ToPoint = nePt;
                                    if (startStep == 0)
                                    {
                                        fristPt = secondPt;
                                        secondPt = nePt;
                                    }
                                }
                                #endregion
                                if (startStep != 0)
                                {
                                    fristPt = newLine.FromPoint;
                                    secondPt = newLine.ToPoint;
                                }
                                leftDic = ReturnMinDistancePt(leftPly, secondPt);
                                rightDic = ReturnMinDistancePt(rightPly, secondPt);
                                l = leftDic.Values.ElementAt(0);
                                r = rightDic.Values.ElementAt(0);
                                double diff = Math.Abs(l - r);
                                if (temperDownAndUpDic.ContainsKey(secondPt) == false)
                                {
                                    temperDownAndUpDic.Add(secondPt, diff);
                                }
                                leNew = diff;
                                #region 
                                if (diff < 0.03 || startStep > 15)
                                {
                                    if (startStep > 15)
                                    { 
                                        var dicSd = from objDic in temperDownAndUpDic orderby objDic.Value ascending select objDic;

                                        foreach (KeyValuePair<IPoint, double> keyv in dicSd)
                                        {
                                            secondPt = keyv.Key; break;
                                        }
                                    }
                                    h = (l + r) /5.0;
                                    secondPt.Z = 10;
                                    valleyOrRdigePts.Add(secondPt);
                                  
                                    IConstructPoint extendPt = new PointClass();
                                    extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                    IPoint newPt = extendPt as IPoint;
                                    newLine.FromPoint = secondPt; ;
                                    newLine.ToPoint = newPt;
                                    newFptClass.PtCoord = fristPt;
                                    newFptClass.Elev = fptInfo.Elev;
                                    fptInfo = newFptClass;
                                    leOld = 0;
                                    leNew = 0;
                                    startStep = 1;
                                    adjustiveAngle = 5;
                                    temperDownAndUpDic = new Dictionary<IPoint, double>();
                                    #region  
                                    int   mark = 0;
                                    stepPly.FromPoint=valleyOrRdigePts[valleyOrRdigePts.Count-2];
                                    stepPly.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];                               
                                    ITopologicalOperator topo = stepPly as ITopologicalOperator;
                                    for (int i_1 = 0; i_1 < terrainFeaLines.Count; i_1++)
                                    {                                                                             
                                        IGeometry  pGeo = topo.Intersect(terrainFeaLines.Keys.ElementAt(i_1), esriGeometryDimension.esriGeometry0Dimension);
                                        if (pGeo.IsEmpty==false )
                                        {
                                            IPointCollection ptC = pGeo as IPointCollection;
                                            IPoint intersectPt = ptC.get_Point(0);
                                            intersectPt.Z = 10;
                                            valleyOrRdigePts.RemoveAt(valleyOrRdigePts.Count-1);
                                            valleyOrRdigePts.Add(intersectPt);                                            
                                            if (isContainSaddPt_Ridge==true)
                                            {
                                                 valleyOrRdigePts.Insert(0, saddlist[0]);
                                                valleyOrRdigePts.Insert(1, saddlist[1]);                                    
                                            }                                                                                           
                                            terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                            CreateFeatureLines(terrainFly, isValOrRidge);
                                            mark = 1;
                                            isEncounter = 1;
                                            break;
                                        }
                                    }
                                    #endregion
                                    if (mark==0)
                                    {
                                        #region  
                                        pTopoOper = secondPt as ITopologicalOperator;
                                        pBufferPoly = pTopoOper.Buffer(50) as IPolygon;
                                        IRelationalOperator relationOper = pBufferPoly as IRelationalOperator;
                                        int mark_1 = 0;
                                                               
                                        IPoint containedPt = new PointClass();
                                        for (int b = 0; b < nextFptInfoList.Count; b++)
                                        {
                                            if (relationOper.Contains(nextFptInfoList[b].PtCoord) == true)
                                            {
                                                containedPt = nextFptInfoList[b].PtCoord;
                                                mark_1 = 1;
                                                break;
                                            }
                                        }
                                        #endregion
                                         
                                        if (mark_1 == 1)
                                        {
                                            stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                                            stepPly.ToPoint = containedPt;                                            
                                            topo = stepPly as ITopologicalOperator;
                                            for (int i_1 = 0; i_1 < terrainFeaLines.Count; i_1++)
                                            {                                                 
                                                IGeometry pGeo = topo.Intersect(terrainFeaLines.Keys.ElementAt(i_1), esriGeometryDimension.esriGeometry0Dimension);
                                                if (pGeo.IsEmpty == false)
                                                {
                                                    IPointCollection ptC = pGeo as IPointCollection;
                                                    IPoint intersectPt = ptC.get_Point(0);
                                                    intersectPt.Z = 10;                                                    
                                                    valleyOrRdigePts.Add(intersectPt);
                                                    if (isContainSaddPt_Ridge == true && saddlist.Count!=0)
                                                    {
                                                        valleyOrRdigePts.Insert(0, saddlist[0]);
                                                        valleyOrRdigePts.Insert(1, saddlist[1]);
                                                    }    
                                                    terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                    CreateFeatureLines(terrainFly, isValOrRidge);
                                                    mark_1 = 1;
                                                    isEncounter = 1;
                                                    break;
                                                }
                                                else
                                                {
                                                    mark_1 = 0;
                                                }
                                            }
                                            #region   
                                            if (mark_1 == 0)
                                            {
                                                #region  
                                             
                                                if (valleyOrRdigePts.Count < 2)
                                                {
                                                    newLine.FromPoint = fristPt;
                                                    newLine.ToPoint = secondPt;
                                                }
                                                else
                                                {
                                                    newLine.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                                                    newLine.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                                                }
                                                 
                                                double maxC = ChooseOptimalFeaPt.MaxCvalue(nextFptInfoList);
                                               
                                                double maxDis = ChooseOptimalFeaPt.MaxDistance(nextFptInfoList, newLine.ToPoint);
                                                
                                                List<double> middList = ChooseOptimalFeaPt.MiddlenessValue(nextFptInfoList);
                                                
                                                double maxAngle = ChooseOptimalFeaPt.MaxDeflecAngle(nextFptInfoList, newLine.ToPoint, newLine);
                                                 
                                                double maxG = 0;
                                                for (int k = 0; k < nextFptInfoList.Count; k++)
                                                {
                                                    double mid = 0; 
                                                    if (k == 0 || k == nextFptInfoList.Count - 1)
                                                    {
                                                        mid = 0;
                                                    }
                                                    else
                                                    {
                                                        mid = middList[k];
                                                    }
                                                    FeaPtInform fpt_1 = nextFptInfoList[k];
                                                    double cRate = ChooseOptimalFeaPt.CalCurveOfRate(Math.Abs(fpt_1.CurvValues), maxC);//曲率比率
                                                                                                  
                                                    ILine linkLine = new LineClass();
                                                    linkLine.FromPoint = newLine.ToPoint;
                                                    linkLine.ToPoint = fpt_1.PtCoord;
                                                    double angle = ChooseOptimalFeaPt.IntersectionAngle(fpt_1, newLine.ToPoint, newLine);
                                                    
                                                    double InnerAngle = ChooseOptimalFeaPt.CalAngleOfRate(angle, maxAngle);
                                                   
                                                    double disR = ChooseOptimalFeaPt.CalShortRate(linkLine.Length, maxDis);
                                                    double maxGravitation = ChooseOptimalFeaPt.GetExpectedStream(cRate, disR, InnerAngle, mid);
                                                    if (maxGravitation > maxG)
                                                    {
                                                        maxG = maxGravitation;
                                                        fptInfo = fpt_1;
                                                    }
                                                }
                                                
                                                IPoint upPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 0, 3);
                                                ILine upLine = new LineClass();
                                                upLine.FromPoint = upPt;
                                                upLine.ToPoint = fptInfo.PtCoord;
                                                      
                                                GetCalculatedPoints.dTightness = 0.3;
                                                Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(newLine.ToPoint.X, newLine.ToPoint.Y, upPt.X, upPt.Y, newLine.Angle, upLine.Angle, 5);
                                                for (int ii = 0; ii < ptDic.Count; ii++)
                                                {
                                                    IPoint newPt_1 = new PointClass();
                                                    newPt_1.X = ptDic.Keys.ElementAt(ii);
                                                    newPt_1.Y = ptDic.Values.ElementAt(ii);
                                                    newPt_1.Z = 10;
                                                    valleyOrRdigePts.Add(newPt_1);
                                                }
                                                valleyOrRdigePts.Add(fptInfo.PtCoord);
                                                temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                                                 
                                                stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 3];
                                                stepPly.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1]; ;
                                                topo = stepPly as ITopologicalOperator;
                                                for (int i_1 = 0; i_1 < terrainFeaLines.Count; i_1++)
                                                {
                                                    IGeometry pGeo = topo.Intersect(terrainFeaLines.Keys.ElementAt(i_1), esriGeometryDimension.esriGeometry0Dimension);
                                                    if (pGeo.IsEmpty == false)
                                                    {
                                                        IPointCollection ptC = pGeo as IPointCollection;
                                                        IPoint intersectPt = ptC.get_Point(0);
                                                        intersectPt.Z = 10;
                                                        valleyOrRdigePts.Add(intersectPt);
                                                        if (isContainSaddPt_Ridge == true)
                                                        {
                                                            valleyOrRdigePts.Insert(0, saddlist[0]);
                                                            valleyOrRdigePts.Insert(1, saddlist[1]);
                                                        }
                                                        terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                        CreateFeatureLines(terrainFly, isValOrRidge);
                                                        mark_1 = 1;
                                                        isEncounter = 1;
                                                        break;
                                                    }                                                    
                                                }
                                                if (isEncounter==0)
                                                {
                                                    index = index - 1;
                                                }                                                
                                                #endregion
                                            }                                            
                                            #endregion                                           
                                        }
                                        else
                                        {
                                            i = -1;
                                        }                                        
                                    }                                    
                                }
                                #endregion
                                #region  
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
                                        newFptClass.PtCoord = secondPt;
                                        newFptClass.Elev = fptInfo.Elev;
                                        fptInfo = newFptClass;
                                        startStep = 1;
                                    }
                                    else 
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
                                            newFptClass.PtCoord = fristPt;
                                            newFptClass.Elev = fptInfo.Elev;
                                            fptInfo = newFptClass;
                                            startStep++;
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
                                            newFptClass.PtCoord = fristPt;
                                            newFptClass.Elev = fptInfo.Elev;
                                            fptInfo = newFptClass;
                                            startStep++;
                                        }
                                        else if (leNew == leOld)
                                        {
                                            
                                            IConstructPoint extendPt = new PointClass();
                                            extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                            IPoint newPt = extendPt as IPoint;
                                            newLine.FromPoint = secondPt; ;
                                            newLine.ToPoint = newPt;
                                            newFptClass.PtCoord = newPt;
                                            newFptClass.Elev = fptInfo.Elev;
                                            fptInfo = newFptClass;
                                            leOld = 0;
                                            leNew = 0;
                                            startStep = 0;
                                            adjustiveAngle = 5;
                                        }
                                    }
                                    i = -1;
                                }
                                #endregion
                            }
                            if ( isEncounter ==1)
                            {
                                break;
                            }

                        }
                        #endregion
                        #region  there are no branches
                        else
                        {
                            IPoint upPt_1 = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 1, 4);
                            upPt_1.Z = 10;
                            valleyOrRdigePts.Add(upPt_1);
                            List<IPoint> temperList = new List<IPoint>();
                            intelligentMethod.fptList = nextFptInfoList;
                            IPolyline midPlyWithIntelliMthod = intelligentMethod.intelligentSearchMethod(terlkFyr, fptInfo, ownLeftPly, ownRightPly, nextFptInfoList[nextFptInfoList.Count / 2].PtCoord, isValOrRidge);                            
                            IPointCollection midPlyTotalPts = midPlyWithIntelliMthod as IPointCollection;                                                        
                            #region  
                            int mark = 0;                     
                            for (int i = 1; i <= midPlyTotalPts.PointCount - 3; i++)
                            {
                                IPoint pt_1 = midPlyTotalPts.get_Point(i);
                                IPoint pt_2 = midPlyTotalPts.get_Point(i+1);
                                IPoint pt_3 = midPlyTotalPts.get_Point(i + 2);
                                #region  
                                IConstructPoint constructPt = new PointClass();
                                constructPt.ConstructAngleBisector(pt_1, pt_2, pt_3, 150, false);
                                IPoint newpt = new PointClass();
                                newpt = constructPt as IPoint;
                                ILine l_line = new LineClass();
                                l_line.FromPoint = pt_2;
                                l_line.ToPoint = newpt;
                                IPolyline l_sectionPly = new PolylineClass();
                                l_sectionPly.FromPoint = pt_2;
                                l_sectionPly.ToPoint = newpt;
                                IPolyline r_sectionPly = PublicFunctionClass.ExtendLineFun(pt_2, l_line.Angle - Math.PI);
                                IPoint ptLeft = GetIntersectPoint(l_sectionPly, ownLeftPly);
                                IPoint ptRight = GetIntersectPoint(r_sectionPly, ownRightPly);
                                if (ptLeft.IsEmpty == true || ptRight.IsEmpty == true)
                                {
                                    valleyOrRdigePts.Add(pt_2);
                                    pTopoOper = pt_2 as ITopologicalOperator;
                                    pBufferPoly = pTopoOper.Buffer(50) as IPolygon;
                                    IRelationalOperator relationOper = pBufferPoly as IRelationalOperator;
                                    #region 
                                    for (int b = 0; b < nextFptInfoList.Count; b++)
                                    {
                                        if (relationOper.Contains(nextFptInfoList[b].PtCoord) == true)
                                        {
                                            mark = 1; break;
                                        }
                                    }
                                    if (mark == 1)
                                    {
                                        
                                        if (valleyOrRdigePts.Count < 2)
                                        {
                                            newLine.FromPoint = fristPt;
                                            newLine.ToPoint = secondPt;
                                        }
                                        else
                                        {
                                            newLine.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                                            newLine.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                                        }
                                        
                                        double maxC = ChooseOptimalFeaPt.MaxCvalue(nextFptInfoList);
                                        
                                        double maxDis = ChooseOptimalFeaPt.MaxDistance(nextFptInfoList, newLine.ToPoint);
                                        
                                        List<double> middList = ChooseOptimalFeaPt.MiddlenessValue(nextFptInfoList);
                                        
                                        double maxAngle = ChooseOptimalFeaPt.MaxDeflecAngle(nextFptInfoList, newLine.ToPoint, newLine);
                                         
                                        double maxG = 0;
                                        for (int k = 0; k < nextFptInfoList.Count; k++)
                                        {
                                            double mid = 0; 
                                            if (k == 0 || k == nextFptInfoList.Count - 1)
                                            {
                                                mid = 0;
                                            }
                                            else
                                            {
                                                mid = middList[k];
                                            }
                                            FeaPtInform fpt_1 = nextFptInfoList[k];
                                            double cRate = ChooseOptimalFeaPt.CalCurveOfRate(Math.Abs(fpt_1.CurvValues), maxC);//曲率比率
                                                                                 
                                            ILine linkLine = new LineClass();
                                            linkLine.FromPoint = newLine.ToPoint;
                                            linkLine.ToPoint = fpt_1.PtCoord;
                                            double angle = ChooseOptimalFeaPt.IntersectionAngle(fpt_1, newLine.ToPoint, newLine);
                                            
                                            double InnerAngle = ChooseOptimalFeaPt.CalAngleOfRate(angle, maxAngle);
                                          
                                            double disR = ChooseOptimalFeaPt.CalShortRate(linkLine.Length, maxDis);
                                            double maxGravitation = ChooseOptimalFeaPt.GetExpectedStream(cRate, disR, InnerAngle, mid);
                                            if (maxGravitation > maxG)
                                            {
                                                maxG = maxGravitation;
                                                fptInfo = fpt_1;
                                            }
                                        }
                                      
                                        IPoint upPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 0, 3);
                                        ILine upLine = new LineClass();
                                        upLine.FromPoint = upPt;
                                        upLine.ToPoint = fptInfo.PtCoord;
                                           
                                        GetCalculatedPoints.dTightness = 0.3;
                                        Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(newLine.ToPoint.X, newLine.ToPoint.Y, upPt.X, upPt.Y, newLine.Angle, upLine.Angle, 5);
                                        for (int ii = 0; ii < ptDic.Count; ii++)
                                        {
                                            IPoint newPt_1 = new PointClass();
                                            newPt_1.X = ptDic.Keys.ElementAt(ii);
                                            newPt_1.Y = ptDic.Values.ElementAt(ii);
                                            newPt_1.Z = 10;
                                            valleyOrRdigePts.Add(newPt_1);
                                        }
                                        valleyOrRdigePts.Add(fptInfo.PtCoord);
                                        temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                                        index = index - 1;
                                        break;
                                    }
                                    else  if (mark == 0) 
                                    {
                                        if (valleyOrRdigePts.Count < 2)
                                        {
                                            stepPly.FromPoint = fristPt;
                                            stepPly.ToPoint = secondPt;
                                        }
                                        else
                                        {
                                            stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                                            stepPly.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                                        }
                                        ITopologicalOperator topo = stepPly as ITopologicalOperator;
                                        for (int k = 0; k < terrainFeaLines.Count; k++)
                                        {
                                            IGeometry pGeo = topo.Intersect(terrainFeaLines.Keys.ElementAt(k), esriGeometryDimension.esriGeometry0Dimension);
                                            if (pGeo.IsEmpty == false)
                                            {
                                                IPointCollection ptC = pGeo as IPointCollection;
                                                IPoint intersectPt = ptC.get_Point(0);
                                                intersectPt.Z = 10;
                                                valleyOrRdigePts.Add(intersectPt);
                                                if (isContainSaddPt_Ridge == true)
                                                {
                                                    valleyOrRdigePts.Insert(0, saddlist[0]);
                                                    valleyOrRdigePts.Insert(1, saddlist[1]);
                                                }
                                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                                isRunin = 0;
                                                mark = 1;
                                                isEncounter = 1;
                                                break;
                                            }
                                        }
                                        
                                    }
                                    #endregion
                                    continue;
                                }
                                IPoint midPoint = midPlyTotalPts.get_Point(i + 1);
                                for (double i_1 = 0; i_1 < 1; i_1++)
                                {
                                    Dictionary<IPoint, double> ownLeft = ReturnMinDistancePt(ownLeftPly, midPoint);
                                    Dictionary<IPoint, double> ownRight = ReturnMinDistancePt(ownRightPly, midPoint);
                                    Dictionary<IPoint, double> nearLeft = ReturnMinDistancePt(closeLeftPly, midPoint);
                                    Dictionary<IPoint, double> nearRight = ReturnMinDistancePt(closeRightPly, midPoint);
                                    if (ownLeft.Count == 0 || ownRight.Count == 0 || nearLeft.Count == 0 || nearRight.Count == 0)
                                    {
                                        valleyOrRdigePts.Add(midPoint);
                                        break;
                                    }
                                    l = Math.Sqrt(ownLeft.Values.ElementAt(0)) / Math.Sqrt(nearLeft.Values.ElementAt(0));
                                    r = Math.Sqrt(ownRight.Values.ElementAt(0)) / Math.Sqrt(nearRight.Values.ElementAt(0));
                                    double c = Math.Abs(l - r);
                                    leNew = c;
                                    if (temperDownAndUpDic.ContainsKey(midPoint) == false)
                                    {
                                        temperDownAndUpDic.Add(midPoint, c);
                                    }
                                    #region  
                                    if (c < 0.03 || startStep > 15)
                                    {
                                        if (startStep > 15)
                                        { 
                                            var dicSd = from objDic in temperDownAndUpDic orderby objDic.Value ascending select objDic;
                                            foreach (KeyValuePair<IPoint, double> keyv in dicSd)
                                            {
                                                midPoint = keyv.Key; break;
                                            }
                                        }
                                        midPoint.Z = 10;
                                        temperList.Add(midPoint);
                                         
                                        if (valleyOrRdigePts.Count < 2)
                                        {
                                            stepPly.FromPoint = fristPt;
                                            stepPly.ToPoint = secondPt;
                                        }
                                        else
                                        {
                                            stepPly.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                                            stepPly.ToPoint = midPoint;
                                        }
                                        ITopologicalOperator topo = stepPly as ITopologicalOperator;
                                        for (int k = 0; k < terrainFeaLines.Count; k++)
                                        {
                                            IGeometry pGeo = topo.Intersect(terrainFeaLines.Keys.ElementAt(k), esriGeometryDimension.esriGeometry0Dimension);
                                            if (pGeo.IsEmpty == false)
                                            {
                                                IPointCollection ptC = pGeo as IPointCollection;
                                                IPoint intersectPt = ptC.get_Point(0);
                                                intersectPt.Z = 10;
                                                valleyOrRdigePts.Add(intersectPt);
                                                if (isContainSaddPt_Ridge == true)
                                                {
                                                    valleyOrRdigePts.Insert(0, saddlist[0]);
                                                    valleyOrRdigePts.Insert(1, saddlist[1]);
                                                }
                                                terrainFly = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                                CreateFeatureLines(terrainFly, isValOrRidge);
                                                isRunin = 0;
                                                mark = 1;
                                                isEncounter = 1;
                                                break;
                                            }
                                            //IPolyline pl = PublicFunctionClass.CreatePly(valleyOrRdigePts);
                                            //PublicFunctionClass.CreateStructPly(newTerain, pl, 0, isValOrRidge);
                                        }
                                        if (isEncounter==0)
                                        {
                                            pTopoOper = midPoint as ITopologicalOperator;
                                            pBufferPoly = pTopoOper.Buffer(53) as IPolygon;
                                            IRelationalOperator relationOper = pBufferPoly as IRelationalOperator;
                                            #region  
                                            for (int b = 0; b < nextFptInfoList.Count; b++)
                                            {
                                                if (relationOper.Contains(nextFptInfoList[b].PtCoord) == true)
                                                {
                                                    mark = 1; break;
                                                }
                                            }
                                            if (mark == 1)
                                            {
                                                 
                                                if (valleyOrRdigePts.Count < 2)
                                                {
                                                    newLine.FromPoint = fristPt;
                                                    newLine.ToPoint = secondPt;
                                                }
                                                else
                                                {
                                                    newLine.FromPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 2];
                                                    newLine.ToPoint = valleyOrRdigePts[valleyOrRdigePts.Count - 1];
                                                }
                                                 
                                                double maxC = ChooseOptimalFeaPt.MaxCvalue(nextFptInfoList);
                                                
                                                double maxDis = ChooseOptimalFeaPt.MaxDistance(nextFptInfoList, newLine.ToPoint);
                                                
                                                List<double> middList = ChooseOptimalFeaPt.MiddlenessValue(nextFptInfoList);
                                                 
                                                double maxAngle = ChooseOptimalFeaPt.MaxDeflecAngle(nextFptInfoList, newLine.ToPoint, newLine);
                                                 
                                                double maxG = 0;
                                                for (int k = 0; k < nextFptInfoList.Count; k++)
                                                {
                                                    double mid = 0; 
                                                    if (k == 0 || k == nextFptInfoList.Count - 1)
                                                    {
                                                        mid = 0;
                                                    }
                                                    else
                                                    {
                                                        mid = middList[k];
                                                    }
                                                    FeaPtInform fpt_1 = nextFptInfoList[k];
                                                    double cRate = ChooseOptimalFeaPt.CalCurveOfRate(Math.Abs(fpt_1.CurvValues), maxC);//曲率比率
                                                                                                   
                                                    ILine linkLine = new LineClass();
                                                    linkLine.FromPoint = newLine.ToPoint;
                                                    linkLine.ToPoint = fpt_1.PtCoord;
                                                    double angle = ChooseOptimalFeaPt.IntersectionAngle(fpt_1, newLine.ToPoint, newLine);
                                                   
                                                    double InnerAngle = ChooseOptimalFeaPt.CalAngleOfRate(angle, maxAngle);
                                                    
                                                    double disR = ChooseOptimalFeaPt.CalShortRate(linkLine.Length, maxDis);
                                                    double maxGravitation = ChooseOptimalFeaPt.GetExpectedStream(cRate, disR, InnerAngle, mid);
                                                    if (maxGravitation > maxG)
                                                    {
                                                        maxG = maxGravitation;
                                                        fptInfo = fpt_1;
                                                    }
                                                }
                                             
                                                IPoint upPt = UpAndDownAngleBisector(terlkFyr, fptInfo, isValOrRidge, 0, 3);
                                                ILine upLine = new LineClass();
                                                upLine.FromPoint = upPt;
                                                upLine.ToPoint = fptInfo.PtCoord;
                                                
                                                upPt.Z = 10;
                                                temperList.Add(upPt);
                                                
                                                int odd = 0;
                                                if (temperList.Count % 2 == 0)
                                                {
                                                    valleyOrRdigePts.Add(temperList[0]);
                                                    odd = 1;
                                                }
                                                for (int ii = odd; ii <= temperList.Count - 3; ii += 2)
                                                {
                                                    ILine upLine_1 = new LineClass();
                                                    upLine_1.FromPoint = temperList[ii];
                                                    upLine_1.ToPoint = temperList[ii + 1];
                                                    ILine downLine = new LineClass();
                                                    downLine.FromPoint = temperList[ii + 1];
                                                    downLine.ToPoint = temperList[ii + 2];
                                                    GetCalculatedPoints.dTightness = 0.5;
                                                    int seg = 5;
                                                    double length = upLine_1.Length + downLine.Length;
                                                    Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(upLine_1.FromPoint.X, upLine_1.FromPoint.Y, downLine.ToPoint.X, downLine.ToPoint.Y, upLine_1.Angle, downLine.Angle, seg);
                                                    for (int i_2 = 0; i_2 < ptDic.Count; i_2++)
                                                    {
                                                        IPoint newPt = new PointClass();
                                                        newPt.X = ptDic.Keys.ElementAt(i_2);
                                                        newPt.Y = ptDic.Values.ElementAt(i_2);
                                                        newPt.Z = 10;
                                                        valleyOrRdigePts.Add(newPt);
                                                    }
                                                }
                                                 
                                                valleyOrRdigePts.Add(fptInfo.PtCoord);
                                                if (temperStoreRemovedFeaPt.ContainsKey(fptInfo) == false)
                                                {
                                                    temperStoreRemovedFeaPt.Add(fptInfo, fptInfo.Elev);
                                                }

                                                 
                                                index = index - 1;
                                            }
                                            else if (mark == 0) 
                                            {
                                                 
                                            }
                                            #endregion
                                        }                                         
                                        startStep = 0;
                                        leNew = 0;
                                        leOld = 0;
                                        direction = 0;
                                        temperDownAndUpDic = new Dictionary<IPoint, double>();
                                    }
                                    #endregion
                                    #region  
                                    else
                                    {
                                        if (startStep == 0)
                                        {
                                            if (l > r) 
                                            {
                                                ptRight = midPoint;
                                                midPoint = CreatePt(ptLeft, ptRight, 1);
                                                direction = 0;
                                            }
                                            else  
                                            {
                                                ptLeft = midPoint;
                                                midPoint = CreatePt(ptLeft, ptRight, 1);
                                                direction = 1;
                                            }
                                            i_1 = -1;
                                            startStep = 1;
                                        }
                                        else
                                        {
                                            if (leNew < leOld) 
                                            {
                                                if (direction == 0) 
                                                {
                                                    ptRight = midPoint;
                                                    midPoint = CreatePt(ptLeft, ptRight, 1);
                                                    direction = 0;
                                                }
                                                else  
                                                {
                                                    ptLeft = midPoint;
                                                    midPoint = CreatePt(ptLeft, ptRight, 1);
                                                    direction = 1;
                                                }
                                            }
                                            else if (leNew > leOld) 
                                            {
                                                if (direction == 0)
                                                {
                                                    ptLeft = midPoint;
                                                    midPoint = CreatePt(ptLeft, ptRight, 1);
                                                    direction = 1;
                                                }
                                                else
                                                {
                                                    ptRight = midPoint;
                                                    midPoint = CreatePt(ptLeft, ptRight, 1);
                                                    direction = 0;

                                                }
                                            }
                                            startStep++;
                                            leOld = leNew;
                                            i_1 = -1;
                                        }
                                    }
                                    #endregion
                                }
                                if (mark == 1)
                                {
                                    break;
                                }
                                #endregion
                            }
                            if (isEncounter == 1)
                            {
                                break;
                            }
                            #endregion
                        }
                        #endregion

                    }
                    #endregion 
                }
                
           }
        }

        //Define the structure trees of feature points groups
        static private Dictionary<List<List<FeaPtInform>>, double> GetMainOrBranchStream(IFeatureLayer terlkFyr, bool isValOrRidge)       
        {
            
            if (regionOfGroupPts.Count==0)
            {
                return null;
            }
            #region  
            double sameDeepZ = 100000000;           
           
            if (isValOrRidge == true)
            {
                for (int j = 0; j < regionOfGroupPts.Count; j++)
                {
                    List<List<FeaPtInform>> valleyDic = regionOfGroupPts[j];
                    double z = valleyDic[valleyDic.Count - 1][0].Elev;
                    if (z < sameDeepZ)
                    {
                        sameDeepZ = z;
                    }
                }
            }
            else
            {
                sameDeepZ = 0;
                for (int j = 0; j < regionOfGroupPts.Count; j++)
                {
                    List<List<FeaPtInform>> valleyDic = regionOfGroupPts[j];
                    double z = valleyDic[valleyDic.Count - 1][0].Elev;
                    if (z >sameDeepZ)
                    {
                        sameDeepZ = z;
                    }
                }
            }
           #endregion 
            #region 
            Dictionary<List<List<FeaPtInform>>, double> lengthDic = new Dictionary<List<List<FeaPtInform>>, double>();
            for (int j = 0; j < regionOfGroupPts.Count; j++)
            {
                List<List<FeaPtInform>> valleyDic = regionOfGroupPts[j];
                double length = 0;
                for (int m = 0; m < valleyDic.Count - 1; m++)
                {
                    List<FeaPtInform> fpt1 = valleyDic[m];
                    List<FeaPtInform> fpt2 = valleyDic[m + 1];
                    length += Math.Sqrt(Math.Pow(fpt1[fpt1.Count / 2].PtCoord.X - fpt2[fpt2.Count / 2].PtCoord.X, 2) + Math.Pow(fpt1[fpt1.Count / 2].PtCoord.Y - fpt2[fpt2.Count / 2].PtCoord.Y, 2));
                }

                lengthDic.Add(valleyDic, length);
            }
            if (lengthDic.Count != 0)
            {
                lengthDic = SortByValueOfLength(lengthDic);
            }

            #endregion
            #region  
            List<List<FeaPtInform>> longestGroupPts = lengthDic.Keys.ElementAt(0);
            int count=0;
            for (int i = longestGroupPts.Count - 1; i >=0 ; i--)
            {
                List<FeaPtInform> ptg = longestGroupPts[i];
                if (ptg[0].Elev==sameDeepZ )
	            {
                    count = i; break;
	             }
                 
            }
            List<IPoint> totalPtList = new List<IPoint>();
            FeaPtInform temperFeaPt_Up = new FeaPtInform();
            List<FeaPtInform> ptG_1 = longestGroupPts[count];
            FeaPtInform optimalFeaPt_Up =ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, ptG_1, null,null, isValOrRidge);
            totalPtList.Add(optimalFeaPt_Up.PtCoord);
            for (int i = count - 1; i >= 0; i--)
            {
                List<FeaPtInform> ptG_2 = longestGroupPts[i];
                double dis = GetDistanceBetweenPts(optimalFeaPt_Up, ptG_2);
                FeaPtInform optFea = new FeaPtInform();
                if (dis < 70)
                {                                  
                    optFea = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, ptG_2, optimalFeaPt_Up,null, isValOrRidge);//结合fptClass_2和上一个点fptClass_1，找到fptClass_2的最佳特征点                    
                    totalPtList.Add(optFea.PtCoord);
                    optimalFeaPt_Up = optFea;                    
                }
                else
                {
                              
                    List<IPolyline> pyList = LeftAndRightPlyFun(terlkFyr, optimalFeaPt_Up, isValOrRidge);
                                         
                    intelligentMethod.fptList = ptG_2;
                    IPolyline ply = intelligentMethod.intelligentSearchMethod(terlkFyr, optimalFeaPt_Up, pyList[0], pyList[1], ptG_2[ptG_2.Count/2].PtCoord, isValOrRidge);
                  
                    IPointCollection ptC = ply as IPointCollection;
                    int odd = 0;
                    if (ptC.PointCount % 2 == 0)
                    {
                        totalPtList.Add(ptC.get_Point(0));
                        odd = 1;
                    }
                    for (int ii = odd; ii <= ptC.PointCount - 3; ii += 2)
                    {
                        ILine upLine = new LineClass();
                        upLine.FromPoint = ptC.get_Point(ii);
                        upLine.ToPoint = ptC.get_Point(ii+1);
                        ILine downLine = new LineClass();
                        downLine.FromPoint = ptC.get_Point(ii+1);
                        downLine.ToPoint = ptC.get_Point(ii + 2);
                        GetCalculatedPoints.dTightness = 0.3;
                        double length = upLine.Length + downLine.Length;
                        int seg = 5;                        
                        Dictionary<double, double> ptDic = GetCalculatedPoints.CreateNewPoint(upLine.FromPoint.X, upLine.FromPoint.Y, downLine.ToPoint.X, downLine.ToPoint.Y, upLine.Angle, downLine.Angle, seg);
                        for (int ii_1 = 0; ii_1 < ptDic.Count; ii_1++)
                        {
                            IPoint newPt = new PointClass();
                            newPt.X = ptDic.Keys.ElementAt(ii_1);
                            newPt.Y = ptDic.Values.ElementAt(ii_1);
                            newPt.Z = 10;
                            totalPtList.Add(newPt);
                        }                        
                    }                   
                    optimalFeaPt_Up = intelligentMethod.feapt;                                       
                }
               
            }
            IPolyline ply_1 = PublicFunctionClass.CreatePly(totalPtList);            
            List<IPolyline> lineList = new List<IPolyline>();
            IPolyline branchPly = new PolylineClass();
            lineList.Add(ply_1);
            Dictionary<List<List<FeaPtInform>>, double> totalDic = new  Dictionary<List<List<FeaPtInform>>,double> ();
            double slope = Math.Abs(longestGroupPts[count][0].Elev - longestGroupPts[0][0].Elev) / ply_1.Length;            
            totalDic.Add(longestGroupPts, slope);
            #endregion 
            #region  
            for (int i = 1; i < lengthDic.Count; i++)
            {
                totalPtList = new List<IPoint>();
                List<List<FeaPtInform>> groupPts = lengthDic.Keys.ElementAt(i);                 
                ptG_1 = groupPts[count];
                optimalFeaPt_Up = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, ptG_1, null, null, isValOrRidge);                
                int stop = 0;
                for (int j = count - 1; j >= 0; j--)
                {
                    List<FeaPtInform> ptG_2 = groupPts[j];                    
                    double dis = GetDistanceBetweenPts(optimalFeaPt_Up, ptG_2);
                    FeaPtInform optFea = new FeaPtInform();
                    if (dis < 70)
                    {                         
                        optFea = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, ptG_2, optimalFeaPt_Up, null, false);
                        totalPtList.Add(optFea.PtCoord);
                        optimalFeaPt_Up = optFea;  
                    }
                    else
                    {
                         List<IPolyline> pyList = LeftAndRightPlyFun(terlkFyr, optimalFeaPt_Up, isValOrRidge);
                         ILine newLine = new LineClass();
                         double leNew = 0;
                         double leOld = 0;
                         IPoint fristPt = new PointClass();
                         IPoint secondPt = new PointClass();
                         int startStep = 0;
                         double adjustiveAngle = 5;
                         int direction = 0;
                         int isZearo = 0;
                         #region  
                         FeaPtInform newFptClass = new FeaPtInform();
                         fristPt = optimalFeaPt_Up.PtCoord;
                         totalPtList.Add(optimalFeaPt_Up.PtCoord);
                         
                         IPolyline leftPly = pyList[0];
                         IPolyline rightPly = pyList[1];
                         IPointCollection leftPtCol = leftPly as IPointCollection;
                         IPointCollection rightPtCol = pyList[1] as IPointCollection;
                         IPoint angPt = PublicFunctionClass.CreateAngleBisectorPt(optimalFeaPt_Up, terlkFyr, isValOrRidge);
                         totalPtList.Add(angPt);
                         secondPt = angPt;
                         newLine.FromPoint = fristPt;
                         newLine.ToPoint = secondPt;
                                  
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
                         newLine.FromPoint = secondPt;
                         newLine.ToPoint = nePt;
                         Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
                          while (true)
                         {
                             if (isZearo == 0)
                             {
                                 fristPt = secondPt;
                                 secondPt = nePt;
                             }
                             else
                             {
                                 fristPt = newLine.FromPoint;
                                 secondPt = newLine.ToPoint;
                             }
                             if (isZearo != 0)
                             {
                                 int f = 0;
                                 #region  
                                 ITopologicalOperator pTopoOper = newLine.ToPoint as ITopologicalOperator;
                                 IPolygon pBufferPoly = pTopoOper.Buffer(50) as IPolygon;
                                 IRelationalOperator relatOper = pBufferPoly as IRelationalOperator;
                                 for (int g = 0; g < ptG_2.Count; g++)
                                 {
                                     FeaPtInform fp = ptG_2[g];
                                     if (relatOper.Contains(ptG_2[ptG_2.Count / 2].PtCoord) == true)
                                     {
                                          
                                         FeaPtInform fpt = new FeaPtInform();
                                         fpt.PtCoord = newLine.ToPoint;
                                         FeaPtInform feapt = ChooseOptimalFeaPt.GetOptiumPt(terlkFyr, ptG_2, fpt, newLine, isValOrRidge);
                                         
                                         for (int j_2 = 0; j_2 < lineList.Count; j_2++)
                                         {
                                             IPolyline intersectPly = lineList[j_2];
                                             IPolyline ppy = new PolylineClass();
                                             ppy.FromPoint = newLine.FromPoint;
                                             ppy.ToPoint = feapt.PtCoord;
                                             ITopologicalOperator topo = ppy as ITopologicalOperator;
                                             IGeometry pgeo = topo.Intersect(intersectPly, esriGeometryDimension.esriGeometry0Dimension);
                                             if (pgeo.IsEmpty == false)
                                             {
                                                 IPointCollection ptcol = pgeo as IPointCollection;
                                                 IPoint pt = ptcol.get_Point(0);
                                                 pt.Z = 10;
                                                 totalPtList.Add(pt);
                                                 IPolyline ply =PublicFunctionClass. CreatePly(totalPtList);
                                                 
                                                 topo = ply as ITopologicalOperator;
                                                 IPointCollection ptCol = intersectPly as IPointCollection;
                                                 int jj = 0;
                                                 IPoint intersectPt = new PointClass();
                                                 for (int j_3 = 0; j_3 < ptCol.PointCount - 1; j_3++)
                                                 {
                                                     IPolyline py = new PolylineClass();
                                                     py.FromPoint = ptCol.get_Point(j_3);
                                                     py.ToPoint = ptCol.get_Point(j_3 + 1);
                                                     IGeometry pgeo_1 = topo.Intersect(py, esriGeometryDimension.esriGeometry0Dimension);
                                                     if (pgeo_1.IsEmpty == false)
                                                     {
                                                         IPointCollection pptCol = pgeo_1 as IPointCollection;
                                                         intersectPt = pptCol.get_Point(0);
                                                         intersectPt.Z = -10;
                                                         totalPtList.Add(intersectPt);
                                                         jj = j_3;
                                                         break;
                                                     }
                                                 }
                                                 for (int j_3 = jj + 1; j_3 < ptCol.PointCount; j_3++)
                                                 {
                                                     IPoint pt_1 = ptCol.get_Point(j_3);
                                                     pt_1.Z = 10;
                                                     totalPtList.Add(pt_1);
                                                 }
                                                 stop = 1;
                                                 break;
                                             }

                                         }
                                         if (stop == 0)
                                         {
                                             optimalFeaPt_Up = feapt;
                                         }
                                         f = 1;
                                         break;
                                     }
                                 }
                                 #endregion
                                 if (f == 0)
                                 {
                                     #region 
                                     for (int j_2 = 0; j_2 < lineList.Count; j_2++)
                                     {
                                         IPolyline intersectPly = lineList[j_2];
                                         IPolyline ppy = new PolylineClass();
                                         ppy.FromPoint = newLine.FromPoint;
                                         ppy.ToPoint = newLine.ToPoint;
                                         ITopologicalOperator topo = ppy as ITopologicalOperator;
                                         IGeometry pgeo = topo.Intersect(intersectPly, esriGeometryDimension.esriGeometry0Dimension);
                                         if (pgeo.IsEmpty == false)
                                         {
                                             IPointCollection ptcol = pgeo as IPointCollection;
                                             IPoint pt = ptcol.get_Point(0);
                                             pt.Z = 10;
                                             totalPtList.Add(pt);
                                             IPolyline ply =PublicFunctionClass.CreatePly(totalPtList);
                                             
                                             topo = ply as ITopologicalOperator;
                                             IPointCollection ptCol = intersectPly as IPointCollection;
                                             int jj = 0;
                                             IPoint intersectPt = new PointClass();
                                             for (int j_3 = 0; j_3 < ptCol.PointCount - 1; j_3++)
                                             {
                                                 IPolyline py = new PolylineClass();
                                                 py.FromPoint = ptCol.get_Point(j_3);
                                                 py.ToPoint = ptCol.get_Point(j_3 + 1);
                                                 IGeometry pgeo_1 = topo.Intersect(py, esriGeometryDimension.esriGeometry0Dimension);
                                                 if (pgeo_1.IsEmpty == false)
                                                 {
                                                     IPointCollection pptCol = pgeo_1 as IPointCollection;
                                                     intersectPt = pptCol.get_Point(0);
                                                     intersectPt.Z = -10;
                                                     totalPtList.Add(intersectPt);
                                                     jj = j_3;
                                                     break;
                                                 }
                                             }
                                             if (totalPtList.Count <= 4)
                                             {
                                                 totalPtList.Clear();
                                             }
                                             for (int j_3 = jj + 1; j_3 < ptCol.PointCount; j_3++)
                                             {
                                                 IPoint pt_1 = ptCol.get_Point(j_3);
                                                 pt_1.Z = 10;
                                                 totalPtList.Add(pt_1);
                                             }
                                             stop = 1;
                                             break;
                                         }
                                     }
                                     #endregion 
                                 }
                                 if (stop == 1 || f == 1)
                                 {
                                     break;
                                 }

                             }
                             if (stop == 0)
                             {
                                 #region  
                                 l = 100; r = 100;                              
                                 leftDic = ReturnMinDistancePt(leftPly, secondPt);
                                 rightDic = ReturnMinDistancePt(rightPly, secondPt);
                                 l = leftDic.Values.ElementAt(0);
                                 r = rightDic.Values.ElementAt(0);
                                 double diff = Math.Abs(l - r);
                                 leNew = diff;
                                 if (dic.ContainsKey(secondPt) == false)
                                 {
                                     dic.Add(secondPt, diff);
                                 }
                                 if (diff < 0.03||isZearo > 15)
                                 {
                                     if (isZearo > 15)
                                     { 
                                         var dicSd = from objDic in dic orderby objDic.Value ascending select objDic;

                                         foreach (KeyValuePair<IPoint, double> keyv in dicSd)
                                         {
                                             secondPt = keyv.Key; break;
                                         }
                                     }
                                     h = (l + r) / 6.0;
                                     secondPt.Z = 10;
                                     totalPtList.Add(secondPt);
                                    
                                     extendPt = new PointClass();
                                     extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                     IPoint newPt = extendPt as IPoint;
                                     newLine.FromPoint = secondPt; ;
                                     newLine.ToPoint = newPt;
                                     leOld = 0;
                                     leNew = 0;
                                     startStep = 0;
                                     adjustiveAngle = 5;
                                     isZearo=1;
                                     dic = new Dictionary<IPoint, double>();
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
                                         startStep = 1;
                                         isZearo++;
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
                                             startStep = 1;

                                         }
                                         else if (leNew == leOld)
                                         {
                                         
                                             extendPt = new PointClass();
                                             extendPt.ConstructAngleDistance(secondPt, newLine.Angle, h);
                                             IPoint newPt = extendPt as IPoint;
                                             newLine.FromPoint = secondPt; ;
                                             newLine.ToPoint = newPt;
                                             leOld = 0;
                                             leNew = 0;
                                             startStep = 0;
                                             adjustiveAngle = 5;
                                         }
                                     }
                                     isZearo++;
                                 }
                                 #endregion 
                             }
                         }
                         #endregion                        
                    }
                    if (stop == 1)
                    {
                        break;
                    }
                }
                branchPly = PublicFunctionClass.CreatePly(totalPtList);                
                lineList.Add(branchPly);
                slope = Math.Abs(groupPts[count][0].Elev - groupPts[0][0].Elev) / branchPly.Length;               
                totalDic.Add(groupPts, slope);
            }
            Dictionary<List<List<FeaPtInform>>, double > dic1 = SortByValueOfSlope(totalDic);//按平均坡度排序
            return dic1;             
            #endregion
       
        
        }
        //Construct hierarchical terrain feature lines
        static private void CreateFeatureLines(IPolyline terrainFly, bool isValOrRidge)
        {
            bool isIntersect = false;
            IProximityOperator pTopoOper = terrainFly as IProximityOperator;
            for (int w = 0; w < terrainFeaLines.Count; w++)
            {
                IPolyline branchPly = terrainFeaLines.Keys.ElementAt(w);
                int code = terrainFeaLines.Values.ElementAt(w);
                double dis = pTopoOper.ReturnDistance(branchPly);                
                if (dis==0)
                {
                    isIntersect = true;
                    if (isValOrRidge == true)
                    {
                        if (code == -1)
                        {
                            if (terrainFeaLines.ContainsKey(terrainFly)==false)
                            {
                                terrainFeaLines.Add(terrainFly, -2);         
                            }
                                             
                            
                        }
                        else if (code == -2)
                        {
                            if (terrainFeaLines.ContainsKey(terrainFly) == false)
                            {
                                terrainFeaLines.Add(terrainFly, -3);
                            }
                            
                        }
                        else if (code == -3)
                        {
                            if (terrainFeaLines.ContainsKey(terrainFly) == false)
                            {
                                terrainFeaLines.Add(terrainFly, -4);
                            }
                           
                        }
                    }
                    else
                    {
                        if (code == 1)
                        {                            	                        
                           terrainFeaLines.Add(terrainFly, 2);	                         
                           
                        }
                        else if (code == 2)
                        {
                            terrainFeaLines.Add(terrainFly, 4);
                        }
                        else
                        {                                                        
                          terrainFeaLines.Add(terrainFly, 4);                                                       
                        }
                    }
                    break;
                }                
            }
            if (isIntersect==false&&Count==1)
            {
                if (isValOrRidge == true)
                {
                    if (terrainFeaLines.ContainsKey(terrainFly)==false)
                    {
                        terrainFeaLines.Add(terrainFly, -4);
                    }
                   
                }
                else
                {
                    if (terrainFeaLines.ContainsKey(terrainFly) == false)
                    {
                        terrainFeaLines.Add(terrainFly, 4);
                    }                    
                }
            }
            else if (isIntersect == false && Count >=2)
            {
                if (isValOrRidge == true)
                {
                    if (terrainFeaLines.ContainsKey(terrainFly) == false)
                    {
                        terrainFeaLines.Add(terrainFly, -2);
                    }

                }
                else
                {
                    if (terrainFeaLines.ContainsKey(terrainFly) == false)
                    {
                        terrainFeaLines.Add(terrainFly, 2);
                    }
                }
            }             
        }
         
        static private List<IPolyline > GetCloseTerlkFea(IFeatureLayer terlkFyr,IPolyline leftPly,IPolyline rightPly,double elev,bool isValOrRidge)
        {
            List<IPolyline> plyList = new List<IPolyline>();
            SortFeaturePtsAndTerlks.GetTerlkFun(terlkFyr);
            List<IFeature> feaList = new List<IFeature>();
            IFeature sameFea = null;
            IFeature leftFeature = null;
            IFeature rightFeature = null;
            for (int i = 0; i < SortFeaturePtsAndTerlks.openTerlkStored.Count; i++)
            {
                double z = SortFeaturePtsAndTerlks.openTerlkStored.Values.ElementAt(i);
                if (isValOrRidge == true)
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z > elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i));
                    }
                }
                else
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z < elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i));
                    }
                }
            }
            if (isValOrRidge==false)
            {
                for (int i = 0; i < SortFeaturePtsAndTerlks.closedTerlkStored.Count; i++)
                {
                    double z = SortFeaturePtsAndTerlks.closedTerlkStored.Values.ElementAt(i);
                    if (isValOrRidge == true)
                    {
                        if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z > elev)
                        {
                            feaList.Add(SortFeaturePtsAndTerlks.closedTerlkStored.Keys.ElementAt(i));
                        }
                    }
                    else
                    {
                        if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z < elev)
                        {
                            feaList.Add(SortFeaturePtsAndTerlks.closedTerlkStored.Keys.ElementAt(i));
                        }
                    }
                }
                   
            }
             ITopologicalOperator topoOperate=null ;
            IGeometry pGeo_Left=null ;
            IGeometry pGeo_Rigth =null ;
            for (int i = 0; i < feaList.Count; i++)
            {
                topoOperate = feaList[i].Shape as ITopologicalOperator;
                pGeo_Left = topoOperate.Intersect(leftPly, esriGeometryDimension.esriGeometry0Dimension);
                pGeo_Rigth = topoOperate.Intersect(rightPly, esriGeometryDimension.esriGeometry0Dimension);
                if (pGeo_Left.IsEmpty == false && pGeo_Rigth.IsEmpty == false) 
                {
                    sameFea = feaList[i];
                    break;
                }
                else if (pGeo_Left.IsEmpty == false && pGeo_Rigth.IsEmpty == true) 
                {
                    leftFeature = feaList[i];
                }
                else if (pGeo_Left.IsEmpty == true && pGeo_Rigth.IsEmpty == false) 
                {
                    rightFeature = feaList[i];
                }
            }
            if (sameFea!=null )
            {
                IPointCollection ptCol = sameFea.Shape as IPointCollection;
                int left = 0; 
                int right = 0; 
                IPoint leftPt = new PointClass(); 
                IPoint rightPt = new PointClass();
                for (int i = 0; i < ptCol.PointCount-1; i++)
                {                    
                    IPolyline ply = new PolylineClass();
                    ply.FromPoint = ptCol.get_Point(i);
                    ply.ToPoint = ptCol.get_Point(i+1);
                    topoOperate = ply as ITopologicalOperator;
                    pGeo_Left = topoOperate.Intersect(leftPly, esriGeometryDimension.esriGeometry0Dimension);
                    pGeo_Rigth = topoOperate.Intersect(rightPly, esriGeometryDimension.esriGeometry0Dimension);
                    if (pGeo_Left.IsEmpty==false)
                    {
                        IPointCollection ptc = pGeo_Left as IPointCollection;
                        leftPt = ptc.get_Point(0);
                        left = i;
                    }
                    if (pGeo_Rigth.IsEmpty==false )
                    {
                        IPointCollection ptc = pGeo_Rigth as IPointCollection;
                        rightPt = ptc.get_Point(0);
                        right = i;
                    }
                }
                int min = 0; int max = 0;
                if (left<right)
                {
                    min = left;
                    max = right;
                }
                else
                {
                    min = right;
                    max = left;
                }
                IPolyline line = new PolylineClass();
                line.FromPoint = leftPt;
                line.ToPoint = rightPt;
                ptCol = sameFea.Shape as IPointCollection;
                double dis = 0; int mark = 0;
                for (int i = min; i < max; i++)
                {
                    IPoint pt = ptCol.get_Point(i);
                    Dictionary<IPoint, double> dic = ReturnMinDistancePt(line,pt);
                    double d = dic.Values.ElementAt(0);
                    if (d>dis)
                    {
                        dis = d;
                        mark = i;                        
                    }
                }
                FeaPtInform fpt=new FeaPtInform();
                fpt.PtAtPlyOid=sameFea.OID;
                fpt.PtAtNumber = mark;
                plyList = LeftAndRightPlyFun(terlkFyr, fpt, isValOrRidge);
            }
            else if (sameFea == null && leftFeature != null && rightFeature!=null)
            {
                plyList.Add(leftFeature.Shape as IPolyline );
                plyList.Add(rightFeature.Shape as IPolyline);
            }
            return plyList;
        }
         
        static private IPoint CreatePt(IPoint pt1, IPoint pt2, double r)
        {
            double x1 = pt1.X; double x2 = pt2.X;
            double y1 = pt1.Y; double y2 = pt2.Y;
            double x = (x1 + r * x2) / (1 + r);
            double y = (y1 + r * y2) / (1 + r);
            IPoint newPt = new PointClass();
            newPt.X = x;
            newPt.Y = y;
            newPt.Z = 10;
            return newPt;
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
         
        static private IPoint GetIntersectPoint(IPolyline secply_1,IPolyline ply_2)
        {
            IPoint newPt = new PointClass();
            ITopologicalOperator topoOperate = secply_1 as ITopologicalOperator;
            IGeometry pGeo = topoOperate.Intersect(ply_2, esriGeometryDimension.esriGeometry0Dimension);
            if (pGeo .IsEmpty==false )
            {
                IPointCollection ptCol = pGeo as IPointCollection;
                if (ptCol.PointCount ==1)
                {
                    newPt = ptCol.get_Point(0);                    
                }
                else
                {
                    IPoint pt = secply_1.FromPoint; double dis = 10000000;                    
                    for (int i = 0; i < ptCol.PointCount; i++)
                    {
                        double d = Math.Sqrt(Math.Pow(pt.X - ptCol.get_Point(i).X, 2) + Math.Pow(pt.Y - ptCol.get_Point(i).Y, 2));
                        if (d<dis)
                        {
                            dis = d;
                            newPt = ptCol.get_Point(i);
                        }
                    }
                }
            }
            return newPt;
        
        
        
        }
         
        static private IPoint UpAndDownAngleBisector(IFeatureLayer flyr, FeaPtInform temperIform, bool isValOrRidge, int p, double length)
        {
             
            IFeature pFea = flyr.FeatureClass.GetFeature(temperIform.PtAtPlyOid);
            IPointCollection PTcoL = pFea.Shape as IPointCollection;
            IConstructPoint constructPt = new PointClass();
            IPoint newpt = new PointClass();
            if (p == 0) 
            {
                if (isValOrRidge == true)
                {
                    constructPt.ConstructAngleBisector(PTcoL.get_Point(temperIform.PtAtNumber + 1), PTcoL.get_Point(temperIform.PtAtNumber), PTcoL.get_Point(temperIform.PtAtNumber - 1), length, false);

                }
                else
                {
                    constructPt.ConstructAngleBisector(PTcoL.get_Point(temperIform.PtAtNumber - 1), PTcoL.get_Point(temperIform.PtAtNumber), PTcoL.get_Point(temperIform.PtAtNumber + 1), length, false);
                }
            }
            else  
            {
                constructPt.ConstructAngleBisector(PTcoL.get_Point(temperIform.PtAtNumber - 1), PTcoL.get_Point(temperIform.PtAtNumber), PTcoL.get_Point(temperIform.PtAtNumber + 1), length, true);
            }
            newpt = constructPt as IPoint;
            newpt.Z = 10;
            return newpt;

        }
         
        static private double GetDistanceBetweenPts(FeaPtInform fptClass,List<FeaPtInform> fptList)
        {
            double dis = 10000000000;
            for (int i = 0; i < fptList.Count; i++)
            {
                double d = Math.Sqrt(Math.Pow(fptClass.PtCoord.X - fptList[i].PtCoord.X, 2) + Math.Pow(fptClass.PtCoord.Y - fptList[i].PtCoord.Y, 2));
                if (d<dis)
                {
                    dis = d;
                }
            }
            return dis;
        }
         
        static private Dictionary<FeaPtInform, double> GetFeaPtWithShortDistance(FeaPtInform fpt,List<FeaPtInform> fptList)
        {
            Dictionary<FeaPtInform, double> dic = new Dictionary<FeaPtInform, double>();
            FeaPtInform newFpt=new FeaPtInform ();double dis=1000000000;
            for (int i = 0; i < fptList.Count; i++)
            {
                FeaPtInform fpt_1=fptList[i];
                double d = Math.Sqrt(Math.Pow(fpt_1.PtCoord.X - fpt.PtCoord.X, 2) + Math.Pow(fpt_1.PtCoord.Y - fpt.PtCoord.Y, 2));
                if (d<dis)
                {
                    dis = d;
                    newFpt = fpt_1;
                }
            }
            dic.Add(newFpt,dis);
            return dic;
        }        
         
        static private Dictionary<List<List<FeaPtInform>>, double> SortByValueOfLength(Dictionary<List<List<FeaPtInform>>, double> paramDic)
        {
            var dicSd = from objDic in paramDic orderby objDic.Value descending select objDic;
            Dictionary<List<List<FeaPtInform>>, double> paramTemper = new Dictionary<List<List<FeaPtInform>>, double>();
            foreach (KeyValuePair<List<List<FeaPtInform>>, double> keyv in dicSd)
            {
                paramTemper.Add(keyv.Key, keyv.Value);
            }
            return paramTemper;
        }      
         
        static private List<IPolyline> LeftAndRightPlyFun(IFeatureLayer terlkFyr, FeaPtInform midFpfOfgroup, bool isValOrRidge)
        {
            List<IPolyline> plyList = new List<IPolyline>();
            List<IPoint> ptList = new List<IPoint>();
            IPointCollection ptc = terlkFyr.FeatureClass.GetFeature(midFpfOfgroup.PtAtPlyOid).Shape as IPointCollection;
            IPolyline leftPly = new PolylineClass();
            IPolyline rightPly = new PolylineClass();
            for (int ii =midFpfOfgroup.PtAtNumber  ; ii>=0 ; ii--)
            {
                ptList.Add(ptc.get_Point(ii));
            }
            IPolyline ply1 = PublicFunctionClass.CreatePly(ptList);
            ptList = new List<IPoint>();
            for (int ii = midFpfOfgroup.PtAtNumber; ii < ptc.PointCount; ii++)
            {
                ptList.Add(ptc.get_Point(ii));
            }
            IPolyline ply2 = PublicFunctionClass.CreatePly(ptList);
            if (isValOrRidge == true)
            {
                leftPly = ply1;
                rightPly = ply2;
            }
            else
            {
                leftPly = ply2;
                rightPly = ply1;
            }
            plyList.Add(leftPly);
            plyList.Add(rightPly);
            return plyList;
        }
         
        static private Dictionary<List<List<FeaPtInform>>, double> SortByValueOfSlope(Dictionary<List<List<FeaPtInform>>, double> paramDic)
        {
            var dicSd = from objDic in paramDic orderby objDic.Value ascending select objDic;
            Dictionary<List<List<FeaPtInform>>, double> paramTemper = new  Dictionary<List<List<FeaPtInform>>,double> ();
            foreach (KeyValuePair<List<List<FeaPtInform>>, double> keyv in dicSd)
            {
                paramTemper.Add(keyv.Key, keyv.Value);
            }
            return paramTemper;
        
        }                             
        


           
        static List<List<FeaPtInform>> allBranchFeaPts = new  List<List<FeaPtInform>> (); 
        static List<FeaPtInform> branchFeaPtOfOneStream = new List<FeaPtInform>();
        static int isWrongBranch = 0;
        static List<List<List<FeaPtInform>>> temperValleyOrRidge = new List<List<List<FeaPtInform>>>();
        //Finding structure trees according to the relationship of feautre points groups
        static private void RecTreeStructSearch(IFeatureLayer terlkFyr, List<FeaPtInform> originList, bool isValOrRidge)
        {
            for (int k = 0; k < 1; k++)
            {
                if (isValOrRidge == false) 
                {
                    IFeature fea = terlkFyr.FeatureClass.GetFeature(originList[0].PtAtPlyOid);
                    int markIndex = fea.Fields.FindField("Mark");
                    short mark = (short)fea.get_Value(markIndex);
                    if (mark == 0)
                    {
                        if (saveClosedPts.ContainsKey(originList)==false)
                        {
                            saveClosedPts.Add(originList, originList[0].Elev);
                        }                                                 
                    }
                }               
               
                List<IPolyline> perpendicularLine = PublicFunctionClass.CreatePerpendicularLine(originList[originList.Count / 2], terlkFyr, isValOrRidge);
                List<List<FeaPtInform>> newArrangeList = GetArrangeFeaPtGroups(perpendicularLine, originList, originList[0].Elev, isValOrRidge);
            
                if (newArrangeList.Count == 0)
                {
                    if (isWrongBranch != 0)
                    {
                        if (valleyOrRidgeExtensiveStore.Count > 1)
                        {
                            regionOfGroupPts.Add(valleyOrRidgeExtensiveStore);
                        }
                    }
                    valleyOrRidgeExtensiveStore = new List<List<FeaPtInform>>();                    
                    for (int i = 0; i < temperValleyOrRidge.Count; i++)
                    {
                        List<List<FeaPtInform>> dic = temperValleyOrRidge[i];
                        valleyOrRidgeExtensiveStore = dic;
                        List<FeaPtInform> list_1 = valleyOrRidgeExtensiveStore[valleyOrRidgeExtensiveStore.Count - 1];
                        originList = list_1;
                        k = -1;
                        isWrongBranch = 0;
                        temperValleyOrRidge.Remove(dic);
                        if (k == -1)
                        {
                            break;
                        }
                    }
                    continue;
                }
                #region 
                List<FeaPtInform> newList = new List<FeaPtInform>();
                List<FeaPtInform> teamOfFeaPt = new List<FeaPtInform>();
                if (newArrangeList.Count == 1)
                {
                    teamOfFeaPt = newArrangeList[0];
                    FeaPtInform midFeaPtInform = teamOfFeaPt[teamOfFeaPt.Count / 2];
                    newList = ReNewFeaPtForm(teamOfFeaPt);
                    valleyOrRidgeExtensiveStore.Add(newList);
                    if (isValOrRidge==true)
                    {
                        DeleteDealedFeaturePtsFun(teamOfFeaPt, allValleyFeaPtGroups);
                        
                    }
                    else
                    {
                        DeleteDealedFeaturePtsFun(teamOfFeaPt, allRidgeFeaPtGroups);
                        
                    }                     
                }
              #endregion
              #region   
              else if (newArrangeList.Count > 1)
              {                    
                    double shortDis = 100000000;
                    int shortTeam = 0;
                    FeaPtInform middleFeaPt = new FeaPtInform();
                    
                    for (int i = 0; i < newArrangeList.Count; i++)
                    {
                        teamOfFeaPt = newArrangeList[i];
                        middleFeaPt = teamOfFeaPt[teamOfFeaPt.Count / 2];
                        double d = Math.Sqrt(Math.Pow(originList[originList.Count / 2].PtCoord.X - middleFeaPt.PtCoord.X, 2) + Math.Pow(originList[originList.Count / 2].PtCoord.Y - middleFeaPt.PtCoord.Y, 2));
                        if (d < shortDis)
                        {
                            shortTeam = i;
                            shortDis = d;
                        }
                    }
                    double longDis = 0;
                    int longTeam = 0;
                    for (int i = 0; i < newArrangeList.Count; i++)
                    {
                        teamOfFeaPt = newArrangeList[i];
                        middleFeaPt = teamOfFeaPt[teamOfFeaPt.Count / 2];
                        double d = Math.Sqrt(Math.Pow(originList[originList.Count / 2].PtCoord.X - middleFeaPt.PtCoord.X, 2) + Math.Pow(originList[originList.Count / 2].PtCoord.Y - middleFeaPt.PtCoord.Y, 2));
                        if (d < longDis)
                        {
                            longTeam = i;
                            longDis = d;
                        }
                    }
                    if (shortDis<40&&longDis>100) 
                    {
                        newList = ReNewFeaPtForm(newArrangeList[shortTeam]);
                    }
                    else
                    {
                         
                        newList = ReNewFeaPtForm(newArrangeList[shortTeam]);
                        int kk = 0;
                        for (int i = 0; i < allBranchFeaPts.Count; i++)
                        {
                            List<FeaPtInform> pttList = allBranchFeaPts[i];
                            if (pttList[0].PtAtNumber == newList[0].PtAtNumber && pttList[0].PtAtPlyOid == newList[0].PtAtPlyOid)
                            {
                                kk = 1;
                            }
                        }
                        if (kk == 0)
                        {
                            allBranchFeaPts.Add(newList);
                        }
                     
                        newArrangeList.Remove(newArrangeList[shortTeam]);
                        List<List<FeaPtInform>> branchList = new List<List<FeaPtInform>>();
                       
                        for (int i = 0; i < newArrangeList.Count; i++)
                        {
                            List<FeaPtInform> fptlist = newArrangeList[i];
                            MarkOtherSurplusPts(terlkFyr, fptlist, isValOrRidge);
                        }
                        if (newList[0].IsAffluent == false)
                        {
                            for (int i = 0; i < newList.Count; i++)//标记为支流
                            {
                                newList[i].IsAffluent = true;
                            }
                        }
                        if (shortDis > 30)
                        {
                            for (int i = 0; i < valleyOrRidgeExtensiveStore.Count; i++)
                            {
                                branchList.Add(valleyOrRidgeExtensiveStore[i]);
                            }
                            temperValleyOrRidge.Add(branchList);
                        }
                    }                   
                    valleyOrRidgeExtensiveStore.Add(newList);                      
                    if (isValOrRidge == true) 
                    {
                        DeleteDealedFeaturePtsFun(newList, allValleyFeaPtGroups);
                         
                    }
                    else
                    {
                        DeleteDealedFeaturePtsFun(newList, allRidgeFeaPtGroups);
                         
                    }                                                                            
                }
                originList = newList;
                k = -1;
                isWrongBranch++;
                #endregion
            }                  
        }
         
        static private void DeleteDealedFeaturePtsFun(List<FeaPtInform> cDic, List<List<FeaPtInform>> group)
        {

            for (int i = 0; i < group.Count; i++)
            {
                List<FeaPtInform> grp = group[i];
                if (grp[0].PtAtNumber == cDic[0].PtAtNumber && grp[0].PtAtPlyOid == cDic[0].PtAtPlyOid)
                {
                    group.Remove(grp);
                    break;
                }
            }

        }
         
        static private List<List<FeaPtInform>> GetArrangeFeaPtGroups(List<IPolyline> perpendicularLine, List<FeaPtInform> fptClass, double elev, bool isValOrRidge)
        {
            List<List<FeaPtInform>> arrangeList = new List<List<FeaPtInform>>();           
            
            IPolyline leftPly = perpendicularLine[0];
            IPolyline rightPly = perpendicularLine[1];
            
            Dictionary<int, Dictionary<IPoint, IPolyline>> plyLeftDic = new Dictionary<int, Dictionary<IPoint, IPolyline>>();
            Dictionary<int, Dictionary<IPoint, IPolyline>> plyRightDic = new Dictionary<int, Dictionary<IPoint, IPolyline>>();
            IFeature sameFea = null;
            IFeature leftFeature = null;
            IFeature rightFeature = null;
            List<IFeature> feaList = new List<IFeature>();
            #region  
            for (int i = 0; i < SortFeaturePtsAndTerlks.openTerlkStored.Count; i++)
            {
                double z = SortFeaturePtsAndTerlks.openTerlkStored.Values.ElementAt(i);
                if (isValOrRidge == true)
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z > elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i));
                    }
                }
                else
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z < elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.openTerlkStored.Keys.ElementAt(i));
                    }
                }
            }
            for (int i = 0; i < SortFeaturePtsAndTerlks.closedTerlkStored.Count; i++)
            {
                double z = SortFeaturePtsAndTerlks.closedTerlkStored.Values.ElementAt(i);
                if (isValOrRidge == true)
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z > elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.closedTerlkStored.Keys.ElementAt(i));
                    }
                }
                else
                {
                    if (Math.Abs(z - elev) == PublicFunctionClass.intervalValue && z < elev)
                    {
                        feaList.Add(SortFeaturePtsAndTerlks.closedTerlkStored.Keys.ElementAt(i));
                    }
                }
            }
            for (int i = 0; i < feaList.Count; i++)
            {
                ITopologicalOperator topoOperate = feaList[i].Shape as ITopologicalOperator;
                IGeometry pGeo_Left = topoOperate.Intersect(leftPly, esriGeometryDimension.esriGeometry0Dimension);
                IGeometry pGeo_Rigth = topoOperate.Intersect(rightPly, esriGeometryDimension.esriGeometry0Dimension);
                if (pGeo_Left.IsEmpty == false && pGeo_Rigth.IsEmpty == false) 
                {
                    sameFea = feaList[i];
                    break;
                }
                else if (pGeo_Left.IsEmpty == false && pGeo_Rigth.IsEmpty == true) 
                {
                    leftFeature = feaList[i];
                }
                else if (pGeo_Left.IsEmpty == true && pGeo_Rigth.IsEmpty == false) 
                {
                    rightFeature = feaList[i];
                }
            }
            #endregion
            int indexLeft = 0; int indexRight = 0;
            #region  
            if (sameFea != null)
            {
                IPointCollection samePtCol = sameFea.Shape as IPointCollection;                
                plyLeftDic = GetIndexAndPtDis(samePtCol, leftPly, fptClass[fptClass.Count / 2].PtCoord);
                plyRightDic = GetIndexAndPtDis(samePtCol, rightPly, fptClass[fptClass.Count / 2].PtCoord);
                indexLeft = plyLeftDic.Keys.ElementAt(0);
                indexRight = plyRightDic.Keys.ElementAt(0);
                arrangeList = SearchFPts(fptClass , sameFea, indexLeft, indexRight, isValOrRidge);                                
            }  
            #endregion 
            #region  
            else if (sameFea == null && leftFeature != null && rightFeature != null && leftFeature.OID != rightFeature.OID)
            {
                IPointCollection ptCol_left = leftFeature.Shape as IPointCollection;
                IPoint fristPt_Left = ptCol_left.get_Point(0);
                IPoint endPt_Left = ptCol_left.get_Point(ptCol_left.PointCount - 1);
                IPointCollection ptCol_right = rightFeature.Shape as IPointCollection;
                IPoint fristPt_Right = ptCol_right.get_Point(0);
                IPoint endPt_Right = ptCol_right.get_Point(ptCol_right.PointCount - 1);
                plyLeftDic = GetIndexAndPtDis(ptCol_left, leftPly, fptClass[fptClass.Count / 2].PtCoord);
                plyRightDic = GetIndexAndPtDis(ptCol_right, rightPly, fptClass[fptClass.Count / 2].PtCoord);
                indexLeft = plyLeftDic.Keys.ElementAt(0);
                indexRight = plyRightDic.Keys.ElementAt(0);
                 
                ITopologicalOperator pTopoOper = fptClass[fptClass.Count / 2].PtCoord as ITopologicalOperator;
                IPolygon pBufferPoly = pTopoOper.Buffer(70) as IPolygon;
                List<IPoint > isContanSaddlePts = GetSaddleOrPeakPts(pBufferPoly, SortFeaturePtsAndTerlks.sortedSaddlePts);
                #region  
                if (isContanSaddlePts.Count==0)
                {             
                    int min = 0; int mid = 0; int max = 0; int m = 0; List<FeaPtInform> newList = new List<FeaPtInform>();
                    
                    if (isValOrRidge==true ) 
                    {
                        for (int i = 0; i < allValleyFeaPtGroups.Count; i++)
                        {
                            List<FeaPtInform> info = allValleyFeaPtGroups[i];
                            min = info[0].PtAtNumber;
                            mid = info[info.Count / 2].PtAtNumber;
                            max = info[info.Count - 1].PtAtNumber;
                            if (info[0].PtAtPlyOid == leftFeature.OID)
                            {
                                if ((min > indexLeft && min < ptCol_left.PointCount) || (mid > indexLeft && mid < ptCol_left.PointCount) || (max > indexLeft && max < ptCol_left.PointCount))
                                {
                                    newList = ReNewFeaPtForm(info);
                                    arrangeList.Add(newList);                                                                         
                                    m = 1;
                                }
                            }
                            if (info[0].PtAtPlyOid == rightFeature.OID)
                            {
                                if ((min > 0 && min < indexRight) || (mid > 0 && mid < indexRight) || (max > 0 && max < indexRight))
                                {
                                    newList = ReNewFeaPtForm(info);
                                    arrangeList.Add(newList);                                                                         
                                    m = 2;
                                }
                            }
                            if (m == 1)
                            {
                                if (info[0].PtAtPlyOid != leftFeature.OID)
                                    break;

                            }
                            if (m == 2)
                            {
                                if (info[0].PtAtPlyOid != rightFeature.OID)
                                    break;
                            }
                        }
                        
                    }
                    else 
                    {
                        for (int i = 0; i < allRidgeFeaPtGroups.Count; i++)
                        {
                            List<FeaPtInform> info = allRidgeFeaPtGroups[i];
                            min = info[0].PtAtNumber;
                            mid = info[info.Count / 2].PtAtNumber;
                            max = info[info.Count - 1].PtAtNumber;
                            if (info[0].PtAtPlyOid == leftFeature.OID)
                            {
                                if ((min > 0 && min < indexLeft) || (mid > 0 && mid < indexLeft) || (max > 0 && max < indexLeft))
                                {
                                    newList = ReNewFeaPtForm(info);
                                    arrangeList.Add(newList);                                     
                                    m = 1;
                                }
                            }
                            if (info[0].PtAtPlyOid == rightFeature.OID)
                            {
                                if ((min > indexRight && min < ptCol_right.PointCount - 1) || (mid > indexRight && mid < ptCol_right.PointCount - 1) || (max > indexRight && max < ptCol_right.PointCount - 1))
                                {
                                    newList = ReNewFeaPtForm(info);
                                    arrangeList.Add(newList);                                                                       
                                    m = 2;
                                }
                            }
                            if (m == 1)
                            {
                                if (info[0].PtAtPlyOid != leftFeature.OID)
                                    break;

                            }
                            if (m == 2)
                            {
                                if (info[0].PtAtPlyOid != rightFeature.OID)
                                    break;
                            }
                        }
                    }
                }
                #endregion 
                #region  
                else if (isContanSaddlePts.Count == 1)
                {
                       Ring ring1 = new RingClass();
                    object missing = Type.Missing;
                    for (int i = 0; i <ptCol_right.PointCount; i++)
                    {
                        ring1.AddPoint(ptCol_right.get_Point(i), ref missing, ref missing);  
                    }
                    for (int i = 0; i < ptCol_left.PointCount; i++)
                    {
                        ring1.AddPoint(ptCol_left.get_Point(i), ref missing, ref missing);  
                    }
                    ring1.AddPoint(ptCol_right.get_Point(0), ref missing, ref missing);  
                    IGeometryCollection pointPolygon = new PolygonClass();
                    pointPolygon.AddGeometry(ring1 as IGeometry, ref missing, ref missing);
                    IPolygon polyGonGeo = pointPolygon as IPolygon;
                    polyGonGeo.SimplifyPreserveFromTo();
                    IRelationalOperator relationOperate = polyGonGeo as IRelationalOperator;
                    List<List<FeaPtInform>> same_1 = new List<List<FeaPtInform>>();
                    for (int i = 0; i < allRidgeFeaPtGroups.Count; i++)
                    {
                        List<FeaPtInform> info = allRidgeFeaPtGroups[i];
                        if (relationOperate.Contains(info[0].PtCoord) == true && info[0].Elev == fptClass[0].Elev && info[0].PtAtPlyOid != fptClass[0].PtAtPlyOid)
                        {
                            same_1.Add(info);
                        }
                    }
                    List<FeaPtInform> newList = new List<FeaPtInform>();
                    Dictionary<List<FeaPtInform>, IPoint > dic = new  Dictionary<List<FeaPtInform>,IPoint> ();
                    if (same_1.Count==1)
                    {
                        newList = ReNewFeaPtForm(same_1[0]);
                        dic.Add(newList, isContanSaddlePts[0]);
                        sameZPtsAndSaddPtDic.Add(fptClass,dic);
                        
                    }
                    else if (same_1.Count>1)
                    {
                       
                        double dis = 100000000; List<FeaPtInform> newLi = new List<FeaPtInform>();
                        for (int i = 0; i < same_1.Count; i++)
                        {
                           List<FeaPtInform> list = same_1[i];
                           FeaPtInform md = list[list.Count / 2];
                           double d = Math.Sqrt(Math.Pow(md.PtCoord.X - fptClass[0].PtCoord.X, 2) + Math.Pow(md.PtCoord.Y - fptClass[0].PtCoord.Y, 2));
                           if (d < dis)
                           {
                              dis = d;
                              newLi = list;
                           }
                       }
                        newList = ReNewFeaPtForm(newLi);
                        IPoint pt1 = newList[0].PtCoord;
                        IPoint pt2 = isContanSaddlePts[0];
                        if (Math.Sqrt(Math.Pow(pt1.X-pt2.X,2)+Math.Pow(pt1.Y-pt2.Y,2))<100)
                        {
                            dic.Add(newList, isContanSaddlePts[0]);
                            sameZPtsAndSaddPtDic.Add(fptClass, dic);   
                        }
                                              
                         
                    }
                }
                #endregion 
                else  
                {
                        
                }
            }
            #endregion 
            #region 
            else if(sameFea==null )
            {                 
                IPolyline rotateLeftPly = new PolylineClass();
                IPolyline rotateRightPly = new PolylineClass();
                if (sameFea == null && leftFeature == null && rightFeature == null)
                {
                    ITransform2D transLeft = leftPly as ITransform2D;
                    transLeft.Rotate(leftPly.FromPoint, -Math.PI / 4);
                    rotateLeftPly = transLeft as IPolyline;
                    ITransform2D transRight = rightPly as ITransform2D;
                    transRight.Rotate(rightPly.FromPoint, Math.PI / 4);
                    rotateRightPly = transRight as IPolyline;
                    for (int i = 0; i < feaList.Count; i++)
                    {
                        ITopologicalOperator topoOperate = feaList[i].Shape as ITopologicalOperator;
                        IGeometry pGeo_Left = topoOperate.Intersect(rotateLeftPly, esriGeometryDimension.esriGeometry0Dimension);
                        IGeometry pGeo_Rigth = topoOperate.Intersect(rotateRightPly, esriGeometryDimension.esriGeometry0Dimension);
                        if (pGeo_Left.IsEmpty == false && pGeo_Rigth.IsEmpty == false) 
                        {
                            sameFea = feaList[i];
                            break;
                        }
                    }
                }
                else if (sameFea == null && leftFeature == null && rightFeature != null)
                {
                    ITransform2D transLeft = leftPly as ITransform2D;                   
                    for (int i =0; i < 8; i++)
                    {
                        transLeft.Rotate(leftPly.FromPoint, -Math.PI/18);
                        rotateLeftPly = transLeft as IPolyline;
                        for (int j = 0; j < feaList.Count; j++)
                        {
                            ITopologicalOperator topoOperate = feaList[j].Shape as ITopologicalOperator;                                                     
                            IGeometry pGeo_Left = topoOperate.Intersect(rotateLeftPly, esriGeometryDimension.esriGeometry0Dimension);
                            if (pGeo_Left.IsEmpty == false && feaList[j].OID != fptClass[0].PtAtPlyOid) 
                            {
                                leftFeature = feaList[j];
                                break;
                            }
                        }
                        if (leftFeature != null)
                        {
                            break;
                        }
                    }
                                                    
                }
                else if (sameFea == null && leftFeature != null && rightFeature == null)
                {
                    ITransform2D transRight = rightPly as ITransform2D;                   
                    for (int i = 0; i < 8 ; i++)
                    {
                        transRight.Rotate(rightPly.FromPoint, Math.PI / 18);
                        rotateRightPly = transRight as IPolyline;
                        for (int j = 0; j < feaList.Count; j++)
                        {
                            ITopologicalOperator topoOperate = feaList[j].Shape as ITopologicalOperator;                            
                            IGeometry pGeo_Rigth = topoOperate.Intersect(rotateRightPly, esriGeometryDimension.esriGeometry0Dimension);
                            if (pGeo_Rigth.IsEmpty == false && feaList[j].OID != fptClass[0].PtAtPlyOid) 
                            {
                                rightFeature = feaList[j];
                                break;
                            }
                        }
                        if (rightFeature != null)
                        {
                            break;
                        }
                    }
                }

                if (sameFea != null )
                {

                    IPointCollection samePtCol = sameFea.Shape as IPointCollection;
                    plyLeftDic = GetIndexAndPtDis(samePtCol, leftPly, fptClass[fptClass.Count / 2].PtCoord);
                    plyRightDic = GetIndexAndPtDis(samePtCol, rightPly, fptClass[fptClass.Count / 2].PtCoord);
                    indexLeft = plyLeftDic.Keys.ElementAt(0);
                    indexRight = plyRightDic.Keys.ElementAt(0);
                    arrangeList = SearchFPts(fptClass, sameFea, indexLeft, indexRight, isValOrRidge);                    
                }
                else if (rightFeature != null && leftFeature != null && leftFeature.OID==rightFeature.OID)
                {
                    sameFea = rightFeature;
                    IPointCollection samePtCol = sameFea.Shape as IPointCollection;
                    plyLeftDic = GetIndexAndPtDis(samePtCol, leftPly, fptClass[fptClass.Count / 2].PtCoord);
                    plyRightDic = GetIndexAndPtDis(samePtCol, rightPly, fptClass[fptClass.Count / 2].PtCoord);
                    indexLeft = plyLeftDic.Keys.ElementAt(0);
                    indexRight = plyRightDic.Keys.ElementAt(0);
                    arrangeList = SearchFPts(fptClass, sameFea, indexLeft, indexRight, isValOrRidge);
                }
                
            }
           #endregion 
            return arrangeList;
          
        }      
         
        static private List<List<FeaPtInform>>  SearchFPts(List<FeaPtInform> fptClass, IFeature sameFea, int indexLeft, int indexRight, bool isValOrRidge)
        {
            List<List<FeaPtInform>> arrangeList = new List<List<FeaPtInform>>();
            int m = 0;
            int min = 0; int mid = 0; int max = 0; List<FeaPtInform> newList = new List<FeaPtInform>();
            #region  
            if (isValOrRidge == true)
            {
                for (int i = 0; i < allValleyFeaPtGroups.Count; i++)
                {
                    List<FeaPtInform> info = allValleyFeaPtGroups[i];
                    if (info[0].PtAtPlyOid == sameFea.OID)
                    {
                        min = info[0].PtAtNumber;
                        mid = info[info.Count / 2].PtAtNumber;
                        max = info[info.Count - 1].PtAtNumber;
                        
                        if ((min >= indexLeft && min <= indexRight) || (max >= indexLeft && max <= indexRight) || (mid >= indexLeft && mid <= indexRight))
                        {
                            newList = ReNewFeaPtForm(info);
                            arrangeList.Add(newList);
                            m = 1;                                                         
                        }
                    }
                    if (m == 1)
                    {
                        if (info[0].PtAtPlyOid != sameFea.OID)
                            break;
                    }
                }
            }
            #endregion 
            #region  
            else
            {
                 IPolygon polyGonGeo = GetPolygonFun(sameFea, indexRight, indexLeft);                               
                
                List<IPoint> saddlePtList = GetSaddleOrPeakPts(polyGonGeo,SortFeaturePtsAndTerlks.sortedSaddlePts);                
                if (saddlePtList.Count==0) 
                {
                    for (int i = 0; i <allRidgeFeaPtGroups.Count;i++)
                    {
                        List<FeaPtInform> info = allRidgeFeaPtGroups[i];                        
                        if (info[0].PtAtPlyOid == sameFea.OID)
                        {
                            min = info[0].PtAtNumber;
                            mid = info[info.Count / 2].PtAtNumber;
                            max = info[info.Count - 1].PtAtNumber;                          
                           
                            if ((min >= indexRight && min <= indexLeft) || (max >= indexRight && max <= indexLeft) || (mid >= indexRight && mid <= indexLeft))
                            {
                                newList = ReNewFeaPtForm(info);
                                arrangeList.Add(newList);
                                m = 1;
                            }
                           
                        }
                        if (m == 1)
                        {
                            if (info[0].PtAtPlyOid != sameFea.OID) break;
                        }
                    }
                }
                else 
                {
                    List<List<FeaPtInform>> same_1 = new List<List<FeaPtInform>>();
                    IRelationalOperator relationOperate = polyGonGeo as IRelationalOperator;
                    for (int i = 0; i < allRidgeFeaPtGroups.Count; i++)
                    {
                        List<FeaPtInform> info = allRidgeFeaPtGroups[i];
                        newList = ReNewFeaPtForm(info);
                        if (relationOperate.Contains(info[0].PtCoord) == true && info[0].Elev == fptClass[0].Elev && info[0].PtAtPlyOid != fptClass[0].PtAtPlyOid)
                        {                                                                                          
                             same_1.Add(newList);
                             saveClosedPts.Add(newList, newList[0].Elev);
                                                                                         

                        }
                        if (relationOperate.Contains(info[0].PtCoord) && info[0].Elev > fptClass[0].Elev && Math.Abs(info[0].Elev - fptClass[0].Elev) == PublicFunctionClass.intervalValue)
                        {
                            saveClosedPts.Add(newList, newList[0].Elev);
                             
                        }
                    }
                    Dictionary<List<FeaPtInform>, IPoint> dic = new Dictionary<List<FeaPtInform>, IPoint>();
                     if (saddlePtList.Count ==1) 
	                 {
                        if (same_1.Count == 1) 
                        {
                            dic.Add(same_1[0], saddlePtList[0]);
                            sameZPtsAndSaddPtDic.Add(fptClass, dic);
                        }
                        else 
                        {
                            double dis = 10000000;
                            List<FeaPtInform> ptGroup = new List<FeaPtInform>();
                            for (int i = 0; i < same_1.Count; i++)
                            {
                                List<FeaPtInform> li = same_1[i];
                                double d = Math.Sqrt(Math.Pow(li[li.Count / 2].PtCoord.X - fptClass[fptClass.Count / 2].PtCoord.X, 2) + Math.Pow(li[li.Count / 2].PtCoord.Y - fptClass[fptClass.Count / 2].PtCoord.Y, 2));
                                if (d < dis)
                                {
                                    dis = d;
                                    ptGroup = li;
                                }
                            }
                            dic.Add(ptGroup, saddlePtList[0]);
                            sameZPtsAndSaddPtDic.Add(fptClass, dic);

                        }     
	                 }
                     else
                     {
                         
                         IPoint pt = fptClass[0].PtCoord;
                         double dis = 10000000000000000;
                         IPoint newSaddlePt = new PointClass();
                         for (int i = 0; i < saddlePtList.Count; i++)
                         {
                             double d = Math.Sqrt(Math.Pow(pt.X - saddlePtList[i].X, 2) + Math.Pow(pt.Y - saddlePtList[i].Y, 2));
                             if (d<dis)
                             {
                                 dis = d;
                                 newSaddlePt = saddlePtList[i];                                 
                             }                             
                         }
                         dis = 10000000000000000;
                          
                         List<FeaPtInform> ptGroup = new List<FeaPtInform>();
                         for (int i = 0; i < same_1.Count; i++)
                         {
                             List<FeaPtInform> li = same_1[i];
                             double d = Math.Sqrt(Math.Pow(li[li.Count / 2].PtCoord.X - newSaddlePt.X, 2) + Math.Pow(li[li.Count / 2].PtCoord.Y - newSaddlePt.Y, 2));
                             if (d < dis)
                             {
                                 dis = d;
                                 ptGroup = li;
                             }
                         }
                         dic.Add(ptGroup, newSaddlePt);
                         sameZPtsAndSaddPtDic.Add(fptClass, dic);
                     }               
                }                                
            }
            #endregion
            return arrangeList;        
        
        }
         
        static private void MarkOtherSurplusPts(IFeatureLayer plyFeatureLyr, List<FeaPtInform> cDic, bool isValOrRidge)
        {
            List<List<FeaPtInform>> group = new List<List<FeaPtInform>>();
            if (isValOrRidge == true)
            {
                group = allValleyFeaPtGroups;
            }
            else
            {
                group = allRidgeFeaPtGroups;
            }
            for (int i = 0; i < group.Count; i++)
            {
                List<FeaPtInform> part = group[i];
                if (part[0].PtAtNumber == cDic[0].PtAtNumber && part[0].PtAtPlyOid == cDic[0].PtAtPlyOid && cDic[0].IsAffluent == false)
                {
                    List<FeaPtInform> newPartList = new List<FeaPtInform>();
                    for (int j = 0; j < part.Count; j++)
                    {
                        FeaPtInform pft = new FeaPtInform();
                        pft.Elev = part[j].Elev;
                        pft.CurvValues = part[j].CurvValues;
                        pft.PtAtNumber = part[j].PtAtNumber;
                        pft.PtAtPlyOid = part[j].PtAtPlyOid;
                        pft.PtCoord = part[j].PtCoord;
                        pft.IsAffluent = true;
                        newPartList.Add(pft);
                    }
                    group.Remove(part);
                    group.Insert(i, newPartList);
                    if (isValOrRidge == false) 
                    {
                        IFeature fea = plyFeatureLyr.FeatureClass.GetFeature(newPartList[0].PtAtPlyOid);
                        int markIndex = fea.Fields.FindField("Mark");
                        short mark = (short)fea.get_Value(markIndex);
                        if (mark == 0)
                        {
                            saveClosedPts.Add(newPartList, newPartList[0].Elev);
                        }
                    }
                    allBranchFeaPts.Add(newPartList);
                    break;
                }

            }
        }
         
        static private IPolygon GetPolygonFun(IFeature sameFea, int min, int max)
        {
            IPointCollection samePtCol = sameFea.Shape as IPointCollection;
             
            Ring ring1 = new RingClass();
            object missing = Type.Missing;
            int kk = 0;
            for (int i = min; i <= max; i++)
            {
                if (i>=samePtCol.PointCount)
                {
                    i = i - 1;
                    kk = 1;
                }
                ring1.AddPoint(samePtCol.get_Point(i), ref missing, ref missing);
                if (kk==1)
                {
                    break;
                }
            }
            IGeometryCollection pointPolygon = new PolygonClass();
            pointPolygon.AddGeometry(ring1 as IGeometry, ref missing, ref missing);
            IPolygon polyGonGeo = pointPolygon as IPolygon;
            polyGonGeo.SimplifyPreserveFromTo();
            return polyGonGeo;

        }
        
        static private List<IPoint >GetSaddleOrPeakPts(IPolygon searchPolygon, List<FeaPtInform> saddleOrPeaks)
        {
            List<IPoint> newPtList = new List<IPoint> ();
            IRelationalOperator relationalOperator = searchPolygon as IRelationalOperator;
            for (int i = 0; i < saddleOrPeaks.Count; i++)
            {
                if (relationalOperator.Contains(saddleOrPeaks[i].PtCoord as IGeometry))
                {
                    newPtList.Add(saddleOrPeaks[i].PtCoord);
                }
            }
            return newPtList;
        }  
         
        static private Dictionary<int, Dictionary<IPoint, IPolyline>> GetIndexAndPtDis(IPointCollection ptCol, IPolyline ply, IPoint upPoint)
        {
            Dictionary<int, Dictionary<IPoint, IPolyline>> plyDic = new Dictionary<int, Dictionary<IPoint, IPolyline>>();
            for (int i = 0; i < ptCol.PointCount - 1; i++)
            {
                IPolyline newPly = new PolylineClass();
                newPly.FromPoint = ptCol.get_Point(i);
                newPly.ToPoint = ptCol.get_Point(i + 1);
               
                ITopologicalOperator topoOperate = newPly as ITopologicalOperator;
                
                IGeometry pG = topoOperate.Intersect(ply as IGeometry, esriGeometryDimension.esriGeometry0Dimension);
                IPointCollection ptc_1 = new MultipointClass();
                Dictionary<IPoint, IPolyline> dic = new Dictionary<IPoint, IPolyline>();
                if (pG.IsEmpty == false)
                {
                    ptc_1 = pG as IPointCollection;
                    dic.Add(ptc_1.get_Point(0), ply);
                    plyDic.Add(i, dic);
                }
            }
            double d = 10000000000;
            Dictionary<int, Dictionary<IPoint, IPolyline>> newDic = new Dictionary<int, Dictionary<IPoint, IPolyline>>();
            if (plyDic.Count > 1)
            {
                int mm = 0;
                for (int i = 0; i < plyDic.Count; i++)
                {
                    int index = plyDic.Keys.ElementAt(i);
                    Dictionary<IPoint, IPolyline> dic = plyDic.Values.ElementAt(i);
                    IPoint pt1 = dic.Keys.ElementAt(0);
                    double dis = Math.Sqrt(Math.Pow(pt1.X - upPoint.X, 2) + Math.Pow(pt1.Y - upPoint.Y, 2));
                    if (dis < d)
                    {
                        d = dis;
                        mm = i;
                    }
                }
                newDic.Add(plyDic.Keys.ElementAt(mm), plyDic.Values.ElementAt(mm));
            }
            else
            {
                newDic = plyDic;
            }

            return newDic;
        }
         
        static private List<FeaPtInform> ReNewFeaPtForm(List<FeaPtInform> list)
        {
            List<FeaPtInform> newList = new List<FeaPtInform>();
            for (int i = 0; i < list.Count; i++)
            {
                FeaPtInform feaIn = new FeaPtInform();
                feaIn.CurvValues = list[i].CurvValues;
                feaIn.Elev = list[i].Elev;
                feaIn.IsAffluent = list[i].IsAffluent;
                feaIn.PtAtNumber = list[i].PtAtNumber;
                feaIn.PtCoord = list[i].PtCoord;
                feaIn.PtAtPlyOid = list[i].PtAtPlyOid;
                newList.Add(feaIn);
            }
            return newList;
        }

        
    }
}

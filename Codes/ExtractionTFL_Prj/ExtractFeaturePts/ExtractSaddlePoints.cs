using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreateBoundary;
using ESRI.ArcGIS.Geometry;
using PieceLineSmoothing_3OrderPolyno;
namespace ExtractFeaturePts
{
   public class ExtractSaddlePoints
   {
        
       static public IList<IPoint> sPtsList = new List<IPoint>();
       static public double intervalValue; 
       static Dictionary<IFeature, double> allFeaList = new Dictionary<IFeature, double>();
       static List<conFeaInfor> listOne = new List<conFeaInfor>();
       static List<List<conFeaInfor>> saddlePtsInfor = new List<List<conFeaInfor>>();
       static List<conFeaInfor> conPElev = new List<conFeaInfor>(); 
       
       static public void DealWithSaddles(IFeatureLayer conFyr, IFeatureLayer boundFyr, IFeatureLayer newFeaPtLyr)
       {
           GetClosedAndOpenStoreInfo(conFyr, boundFyr);
           conPElev = sameElevConStoreFun();
           GetSaddleRegions();
           for (int i = 0; i < saddlePtsInfor.Count; i++)
           {
               List<conFeaInfor> conFeaList = saddlePtsInfor[i];
               for (int l = 0; l < conFeaList.Count; l++)
               {
                   IFeature feaOne = conFeaList[l].ConFea;
                   IPointCollection ptColOne = feaOne.Shape as IPointCollection;
                   for (int q = conFeaList.Count - 1; q > l; q--)
                   {
                       IFeature feaTwo = conFeaList[q].ConFea;
                       IPointCollection ptColTwo = feaTwo.Shape as IPointCollection;
                       
                       List<int> oneList = GetFeaturePtsFun(feaOne);
                       List<int> TwoList = GetFeaturePtsFun(feaTwo);
                       if (oneList.Count == 0 || TwoList.Count == 0)
                       {
                           continue;
                       }
                       
                       double distance = 10000000000;
                       IPolyline newPolyline = null;
                       int mark1 = 0; int mark2 = 0;
                       for (int j = 0; j < oneList.Count; j++)
                       {
                           IPoint fristPt = ptColOne.get_Point(oneList[j]);
                           for (int k = 0; k < TwoList.Count; k++)
                           {
                               IPoint secondPt = ptColTwo.get_Point(TwoList[k]);
                               IPolyline ply = GetBoundPly(fristPt, secondPt);
                               double d = ply.Length;
                               if (d < distance)
                               {
                                   mark1 = oneList[j];
                                   mark2 = TwoList[k];
                                   distance = d;
                                   newPolyline = ply;
                               }
                           }
                       }
                       ITopologicalOperator topoOperate = newPolyline as ITopologicalOperator;
                       int record = 0;
                       for (int m = 0; m < allFeaList.Count; m++)
                       {
                           if (allFeaList.Keys.ElementAt(m).OID == feaOne.OID || allFeaList.Keys.ElementAt(m).OID == feaTwo.OID)
                           {
                               continue;
                           }
                           IGeometry pGeo = topoOperate.Intersect(allFeaList.Keys.ElementAt(m).Shape as IGeometry, esriGeometryDimension.esriGeometry0Dimension);
                           if (pGeo.IsEmpty == false)
                           {
                               record++;
                           }
                           if (record > 1)
                           {
                               break;
                           }
                       }
                       if (record == 0) 
                       {
                           IPoint sPt = ptColTwo.get_Point(mark2);
                                                
                           #region  
                           IPolyline lengSectionPly = new PolylineClass();
                           IPoint pt1 = ptColOne.get_Point(mark1);
                           IPoint pt2 = ptColOne.get_Point(mark1 + 1);
                           ILine line = new LineClass();
                           line.FromPoint = pt1;
                           line.ToPoint = pt2;
                           IConstructPoint extendPt = new PointClass();
                           extendPt.ConstructAngleDistance(pt2, line.Angle, 150);
                           IPolyline sectionPly_1 = new PolylineClass();
                           sectionPly_1.FromPoint = pt1;
                           sectionPly_1.ToPoint = extendPt as IPoint;
                           lengSectionPly.FromPoint = sectionPly_1.ToPoint;
                           Dictionary<IFeature, double> closeFeature1 = JudgeIntersectTerlk(sectionPly_1, pt1, Math.Round(pt1.Z, 0));
                           IConstructPoint extendPt_1 = new PointClass();
                           extendPt_1.ConstructAngleDistance(pt1, line.Angle - Math.PI, 150);
                           IPolyline sectionPly_2 = new PolylineClass();
                           sectionPly_2.FromPoint = pt2;
                           sectionPly_2.ToPoint = extendPt_1 as IPoint;
                           lengSectionPly.ToPoint = sectionPly_2.ToPoint;
                           Dictionary<IFeature, double> closeFeature2 = JudgeIntersectTerlk(sectionPly_2, pt2, Math.Round(pt2.Z, 0));
                           #endregion
                           double angle1 = 0;
                           double angle2 = 0;
                           List<IPoint> ptList = new List<IPoint>();
                           IPolyline rightPly = new PolylineClass();
                           IPolyline leftPly = new PolylineClass();
                           IPoint leftPtOfMostCLPt_1 = new PointClass(); 
                           IPoint rightPtOfMostCLPt_1 = new PointClass(); 
                           IPoint leftPtOfClosLPt_1 = new PointClass(); 
                           IPoint rightPtOfClosLPt_1 = new PointClass(); 
                           IPoint agentPoint = new PointClass();
                           FeaPtInform fptIn = new FeaPtInform();
                           fptIn.PtAtNumber = mark1;
                           fptIn.PtCoord = pt1;
                           fptIn.Elev = pt1.Z;
                           fptIn.PtAtPlyOid = feaOne.OID;
                           IPolyline midPly = new PolylineClass();
                           var disCd = from objDic in closeFeature1 orderby objDic.Value descending select objDic;
                           
                           Dictionary<IFeature, double> cF_1 = new Dictionary<IFeature, double>();
                           foreach (KeyValuePair<IFeature , double> key in disCd)
                           {
                               cF_1.Add(key.Key, key.Value);
                               
                           }
                           var disCd1 = from objDic in closeFeature2 orderby objDic.Value descending select objDic;
                           Dictionary<IFeature, double> cF_2 = new Dictionary<IFeature, double>();
                           foreach (KeyValuePair<IFeature, double> key in disCd1)
                           {

                               cF_2.Add(key.Key, key.Value);
                           }
                           #region   
                           IConstructPoint constructPt_1 = new PointClass();
                           constructPt_1.ConstructAngleBisector(ptColOne.get_Point(mark1 - 1), ptColOne.get_Point(mark1), ptColOne.get_Point(mark1 + 1), 4, false);
                           IPoint markPt_1 = new PointClass();
                           markPt_1 = constructPt_1 as IPoint;
                           ILine markLine_1 = new LineClass();
                           markLine_1.FromPoint = ptColOne.get_Point(mark1);
                           markLine_1.ToPoint = markPt_1;
                           IConstructPoint constructPt_2 = new PointClass();
                           constructPt_2.ConstructAngleBisector(ptColTwo.get_Point(mark2 - 1), ptColTwo.get_Point(mark2), ptColTwo.get_Point(mark2 + 1), 4, true);
                           IPoint markPt_2 = new PointClass();
                           markPt_2 = constructPt_2 as IPoint;
                           ILine markLine_2 = new LineClass();
                           markLine_2.FromPoint = ptColTwo.get_Point(mark2);
                           markLine_2.ToPoint = markPt_2;
                           GetCalculatedPoints.dTightness = 0.1;
                           Dictionary<double, double> ptXYDic = GetCalculatedPoints.CreateNewPoint(ptColOne.get_Point(mark1).X, ptColOne.get_Point(mark1).Y, ptColTwo.get_Point(mark2).X, ptColTwo.get_Point(mark2).Y, markLine_1.Angle, markLine_2.Angle, 10);
                           List<IPoint> ptlist = new List<IPoint>();
                           for (int w = 0; w < ptXYDic.Count; w++)
                           {
                               IPoint pt = new PointClass();
                               pt.X = ptXYDic.Keys.ElementAt(w);
                               pt.Y = ptXYDic.Values.ElementAt(w);
                               pt.Z = 10;
                               ptlist.Add(pt);
                           }
                           midPly = CreatePolyline(ptlist);
                           #endregion 
                           if (cF_1.Count == 2 && cF_2.Count == 2)
                           {
                               if (cF_1.Keys.ElementAt(0).OID == cF_2.Keys.ElementAt(0).OID) 
                               {
                                   
                                   IFeature closePly = cF_1.Keys.ElementAt(0);
                                   IPointCollection closePtc = closePly.Shape as IPointCollection;
                                   #region 
                                   ITopologicalOperator topo = lengSectionPly as ITopologicalOperator;
                                   int j_1 = 0; int j_2 = 0; int k = 0;
                                   for (int j = 0; j < closePtc.PointCount - 1; j++)
                                   {
                                       IPolyline ply = new PolylineClass();
                                       ply.FromPoint = closePtc.get_Point(j);
                                       ply.ToPoint = closePtc.get_Point(j + 1);
                                       IGeometry geometry = topo.Intersect(ply as IGeometry, esriGeometryDimension.esriGeometry0Dimension);
                                       if (geometry.IsEmpty == false)
                                       {
                                           if (k == 0)
                                           {
                                               j_1 = j;
                                           }
                                           else
                                           {
                                               j_2 = j;
                                               break;
                                           }
                                           k++;
                                       }
                                   }
                                   double dis = 0; int divide = 0;
                                   for (int a = j_1; a <= j_2; a++)
                                   {
                                       double d = ReturnMinDistance(lengSectionPly, closePtc.get_Point(a));
                                       if (d > dis)
                                       {
                                           dis = d;
                                           divide = a;
                                       }
                                   }
                                   
                                   for (int k1 = 0; k1 <= divide; k1++)
                                   {
                                       ptList.Add(closePtc.get_Point(k1));
                                   }
                                   rightPly = CreatePolyline(ptList);
                                   ptList = new List<IPoint>();
                                   for (int k1 = divide; k1 < closePtc.PointCount; k1++)
                                   {
                                       ptList.Add(closePtc.get_Point(k1));
                                   }
                                   leftPly = CreatePolyline(ptList);
                                   #endregion
                                    
                                   IPointCollection midPtCol = midPly as IPointCollection;
                                   double shortDis = 10000000000;
                                   int shortIndex = 0;
                                   for (int k1 = 1; k1 < midPtCol.PointCount - 1; k1++)
                                   {
                                       double d1 = ReturnMinDistance(leftPly, midPtCol.get_Point(k1));
                                       double d2 = ReturnMinDistance(rightPly, midPtCol.get_Point(k1));
                                       if (d1 + d2 < shortDis)
                                       {
                                           shortDis = d1 + d2; shortIndex = k1;
                                       }
                                   }                                    
                                                                     
                                       agentPoint = midPtCol.get_Point(shortIndex);
                                                                                                           
                                       IPolyline saddleSectionPly = SaddleSectionPly(midPtCol, shortIndex);
                                       
                                       IPolyline mosClPly = closeFeature1.Keys.ElementAt(0).Shape as IPolyline;
                                       List<IPoint> ptMos = IntersectLeftOrRightPt(saddleSectionPly, mosClPly);
                                       leftPtOfMostCLPt_1 = ptMos[0];
                                       rightPtOfMostCLPt_1 = ptMos[1];
                                       if (cF_1.Keys.ElementAt(1).OID == cF_2.Keys.ElementAt(1).OID)
                                       {
                                           IPolyline clPly = cF_1.Keys.ElementAt(1).Shape as IPolyline;
                                           List<IPoint> ptClose = IntersectLeftOrRightPt(saddleSectionPly, clPly);
                                           leftPtOfClosLPt_1 = ptClose[0];
                                           rightPtOfClosLPt_1 = ptClose[1];
                                       }
                                       else
                                       {
                                           IPolyline clPly = cF_1.Keys.ElementAt(1).Shape as IPolyline;
                                           List<IPoint> ptClose = IntersectLeftOrRightPt(saddleSectionPly, clPly);
                                           leftPtOfClosLPt_1 = ptClose[0];
                                           clPly = cF_2.Keys.ElementAt(1).Shape as IPolyline;
                                           ptClose = IntersectLeftOrRightPt(saddleSectionPly, clPly);
                                           rightPtOfClosLPt_1 = ptClose[0];
                                       }
                               }
                               else
                               {
                                   IPolyline mosClPly_1 = cF_1.Keys.ElementAt(0).Shape as IPolyline;
                                   IPolyline mosClPly_2 = cF_2.Keys.ElementAt(0).Shape as IPolyline;
                                   leftPly = mosClPly_1;
                                   rightPly = mosClPly_2;
                                  
                                   IPointCollection midPtCol = midPly as IPointCollection;
                                   double shortDis = 10000000000;
                                   int shortIndex = 0;
                                   for (int k1 = 1; k1 < midPtCol.PointCount - 1; k1++)
                                   {
                                       double d1 = ReturnMinDistance(leftPly, midPtCol.get_Point(k1));
                                       double d2 = ReturnMinDistance(rightPly, midPtCol.get_Point(k1));
                                       if (d1 + d2 < shortDis)
                                       {
                                           shortDis = d1 + d2; shortIndex = k1;
                                       }
                                   }
                                   if (shortIndex == 1 || shortIndex == midPtCol.PointCount - 2)
                                   {
                                       shortIndex = midPtCol.PointCount / 2;
                                   }
                                   agentPoint = midPtCol.get_Point(shortIndex);
                                   
                                   IPolyline saddleSectionPly = SaddleSectionPly(midPtCol, shortIndex);
                                   List<IPoint> ptMos_1 = IntersectLeftOrRightPt(saddleSectionPly, mosClPly_1);
                                   List<IPoint> ptMos_2 = IntersectLeftOrRightPt(saddleSectionPly, mosClPly_2);
                                   if (ptMos_1[0].X < ptMos_2[0].X)
                                   {
                                       leftPtOfMostCLPt_1 = ptMos_1[0]; rightPtOfMostCLPt_1 = ptMos_2[0];
                                   }
                                   else
                                   {
                                       leftPtOfMostCLPt_1 = ptMos_2[0]; rightPtOfMostCLPt_1 = ptMos_1[0];
                                   }
                                   IPolyline clPly_1 = closeFeature1.Keys.ElementAt(1).Shape as IPolyline;
                                   IPolyline clPly_2 = closeFeature2.Keys.ElementAt(1).Shape as IPolyline;
                                   List<IPoint> ptClose_1 = IntersectLeftOrRightPt(saddleSectionPly, clPly_1);
                                   List<IPoint> ptClose_2 = IntersectLeftOrRightPt(saddleSectionPly, clPly_2);
                                   if (ptClose_1.Count==0&&ptClose_2.Count!=0)
                                   {
                                       if (ptClose_2[0].X>agentPoint.X&&ptClose_2[0].X<saddleSectionPly.ToPoint.X)
                                       {
                                           ptClose_1.Add(saddleSectionPly.FromPoint);
                                       }
                                       else if (ptClose_2[0].X <agentPoint.X && ptClose_2[0].X > saddleSectionPly.FromPoint.X)
                                       {
                                           ptClose_1.Add(saddleSectionPly.ToPoint);
                                       }
                                   }
                                   else if (ptClose_1.Count!=0&&ptClose_2.Count==0)
                                   {
                                       if (ptClose_1[0].X > agentPoint.X && ptClose_1[0].X < saddleSectionPly.ToPoint.X)
                                       {
                                           ptClose_2.Add(saddleSectionPly.FromPoint);
                                       }
                                       else if (ptClose_1[0].X < agentPoint.X && ptClose_1[0].X > saddleSectionPly.FromPoint.X)
                                       {
                                           ptClose_2.Add(saddleSectionPly.ToPoint);
                                       }
                                   }
                                   if (ptClose_1[0].X < ptClose_2[0].X)
                                   {
                                       leftPtOfClosLPt_1 = ptClose_1[0]; rightPtOfClosLPt_1 = ptClose_2[0];
                                   }
                                   else
                                   {
                                       leftPtOfClosLPt_1 = ptClose_2[0]; rightPtOfClosLPt_1 = ptClose_1[0];
                                   }
                               }
                               double Z = closeFeature1.Values.ElementAt(0);
                               ILine newLine = new LineClass();
                               newLine.FromPoint = sectionPly_1.FromPoint;
                               newLine.ToPoint = sectionPly_1.ToPoint;
                               IPoint newPt1 = new PointClass();
                               IPoint newPt2 = new PointClass();
                               newPt1 = RotateCoordinate(leftPtOfMostCLPt_1, newLine.Angle);
                               newPt2 = RotateCoordinate(rightPtOfMostCLPt_1, newLine.Angle);
                               double rightDis = Math.Sqrt(Math.Pow(rightPtOfClosLPt_1.X - rightPtOfMostCLPt_1.X, 2) + Math.Pow(rightPtOfClosLPt_1.Y - rightPtOfMostCLPt_1.Y, 2));//右边
                               double leftDis = Math.Sqrt(Math.Pow(leftPtOfClosLPt_1.X - leftPtOfMostCLPt_1.X, 2) + Math.Pow(leftPtOfClosLPt_1.Y - leftPtOfMostCLPt_1.Y, 2)); //左边                                                           
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
                                
                               Dictionary<double, double> ptXZDic = new Dictionary<double, double>();
                               GetCalculatedPoints.dTightness = 2;
                               for (int j = 0; j < 1; j++)
                               {
                                   ptXZDic = GetCalculatedPoints.CreateNewPoint(newPt1.X, Z, newPt2.X, Z, angle1, angle2, 20);
                                   ptXZDic = SortedByZValue(ptXZDic);
                                   double diffZ = ptXZDic.Values.ElementAt(0);
                                   IPoint ppts = RotateCoordinate(agentPoint, newLine.Angle);
                                   if (diffZ < Z + intervalValue && diffZ > Z)
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
                                       IPoint SaddlePoint = RotateCoordinate(pt, -newLine.Angle);
                                       SaddlePoint.Z = ptXZDic.Values.ElementAt(0);
                                       AttainFpts.CreateFPtByDictionary(newFeaPtLyr, 0, SaddlePoint, 2, 2);
                                       //sPtsList.Add(SaddlePoint);
                                       break;
                                   }
                                   else
                                   {
                                       GetCalculatedPoints.dTightness -= 0.1;
                                       j = -1;
                                   }
                               }
                           }
                           else if ((closeFeature1.Count == 0 && closeFeature2.Count == 2) || (closeFeature1.Count == 2 && closeFeature2.Count == 0))     //若只有一侧有邻近等高线，直接用三点光滑法求得直线，然后取得中点为鞍部点          
                           {
                               GetCalculatedPoints.dTightness = 2;
                               IConstructPoint constructPt = new PointClass();
                               constructPt.ConstructAngleBisector(ptColOne.get_Point(mark1 - 1), ptColOne.get_Point(mark1), ptColOne.get_Point(mark1 + 1), 10, false);
                               IPoint newpt = new PointClass();
                               newpt = constructPt as IPoint;
                               IPoint newPt1 = ptColOne.get_Point(mark1);
                               IPoint newPt2 = ptColTwo.get_Point(mark2);
                               angle1 = Angle(newPt1, newpt as IPoint);
                               constructPt = new PointClass();
                               constructPt.ConstructAngleBisector(ptColTwo.get_Point(mark2 - 1), ptColTwo.get_Point(mark2), ptColTwo.get_Point(mark2 + 1), 10, true);
                               newpt = new PointClass();
                               newpt = constructPt as IPoint;
                               angle2 = Angle(newPt2, newpt as IPoint);
                               Dictionary<double, double> ptXZDic = new Dictionary<double, double>();
                               double Z = 0;
                               if (closeFeature1.Count != 0)
                               {
                                   Z = closeFeature1.Values.ElementAt(0);
                               }
                               else
                               {
                                   Z = closeFeature2.Values.ElementAt(0);
                               }
                               ptXZDic = GetCalculatedPoints.CreateNewPoint(ptColOne.get_Point(mark1).X, ptColOne.get_Point(mark1).Y, ptColTwo.get_Point(mark2).X, ptColTwo.get_Point(mark2).Y, angle1, angle2, 20);
                               ptXZDic = SortedByZValue(ptXZDic);
                               IPoint SaddlePoint = new PointClass();
                               SaddlePoint.X = ptXZDic.Keys.ElementAt(ptXZDic.Count / 2);
                               SaddlePoint.Y = ptXZDic.Values.ElementAt(ptXZDic.Count / 2);
                               SaddlePoint.Z = (Z + (Z + intervalValue)) / 2;
                               AttainFpts.CreateFPtByDictionary(newFeaPtLyr, 0, SaddlePoint, 2, 2);
                               //sPtsList.Add(SaddlePoint);
                           }

                       }
                   }

               }

           }

       }       
       static public void CreateStructPly(IFeatureLayer featurePtLyr, IPolyline structPly)
       {
           int mark = featurePtLyr.FeatureClass.FindField("Mark");
           IFeatureBuffer featureBuffer = featurePtLyr.FeatureClass.CreateFeatureBuffer();
           IFeatureCursor featureCursor = featurePtLyr.FeatureClass.Insert(true);
           IZAware pZAware = (IZAware)structPly;
           pZAware.ZAware = true;
           featureBuffer.Shape = structPly;
           featureCursor.InsertFeature(featureBuffer);
           featureCursor.Flush();
           System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
       }  
       static private List<IPoint> IntersectLeftOrRightPt(IPolyline saddleSectionPly,IPolyline clPly)
       {
           List<IPoint> ptlist = new List<IPoint>(); 
           ITopologicalOperator sTopo = saddleSectionPly as ITopologicalOperator;
           IGeometry pGeo = sTopo.Intersect(clPly, esriGeometryDimension.esriGeometry0Dimension);
           IPointCollection ptCol = new MultipointClass();
           if (pGeo.IsEmpty == false)
           {
               ptCol = pGeo as IPointCollection;
               if (ptCol.PointCount==2)
               {
                   IPoint pt_1 = ptCol.get_Point(0);
                   IPoint pt_2 = ptCol.get_Point(1);
                   if (pt_1.X > pt_2.X)
                   {
                       ptlist.Add(pt_2);
                       ptlist.Add(pt_1);
                   }
                   else
                   {
                       ptlist.Add(pt_1);
                       ptlist.Add(pt_2);
                   }
               }
               else if (ptCol.PointCount == 1)
               {
                   ptlist.Add(ptCol.get_Point(0));
               }
               else if (ptCol.PointCount>2)
               {
                    
                   
                   IPoint midPt = new PointClass();
                   midPt.X = (saddleSectionPly.FromPoint.X + saddleSectionPly.ToPoint.X) / 2;
                   midPt.Y = (saddleSectionPly.FromPoint.Y + saddleSectionPly.ToPoint.Y) / 2;
                   Dictionary<IPoint, double> dic = new Dictionary<IPoint, double>();
                   for (int i = 0; i < ptCol.PointCount; i++)
                   {
                       double d = Math.Pow(midPt.X - ptCol.get_Point(i).X, 2) + Math.Pow(midPt.Y - ptCol.get_Point(i).Y, 2);
                       dic.Add(ptCol.get_Point(i), d);
                   }
                   var disCd = from objDic in dic orderby objDic.Value ascending select objDic;
                   List<IPoint> pList = new List<IPoint>();
                   foreach (KeyValuePair<IPoint, double> key in disCd)
                   {
                       pList.Add(key.Key);
                       if (pList.Count == 2)
                       {
                           break;
                       }
                   }
                   IPoint pt_1 = pList[0];
                   IPoint pt_2 = pList[1];
                   if (pt_1.X > pt_2.X)
                   {
                       ptlist.Add(pt_2);
                       ptlist.Add(pt_1);
                   }
                   else
                   {
                       ptlist.Add(pt_1);
                       ptlist.Add(pt_2);
                   }
               }
           }
           return ptlist;
       }
       static private IPolyline SaddleSectionPly(IPointCollection midPtCol, int shortIndex)
       {
            
           IConstructPoint angleBesiPt = new PointClass();           
           IPoint pt1 = midPtCol.get_Point(shortIndex - 1);
           IPoint pt2 = midPtCol.get_Point(shortIndex);
           IPoint pt3 = midPtCol.get_Point(shortIndex+1);
           angleBesiPt.ConstructAngleBisector(pt1, pt2, pt3, 150, true);
           ILine angleBesiLine = new LineClass();
           angleBesiLine.FromPoint = midPtCol.get_Point(shortIndex);
           angleBesiLine.ToPoint = angleBesiPt as IPoint;
         
          
           IPolyline temperply = new PolylineClass();
           temperply = ExtendLineFun(angleBesiLine.FromPoint, angleBesiLine.Angle - Math.PI);
           IPolyline saddleSectionPly = new PolylineClass();
           saddleSectionPly.FromPoint = angleBesiLine.ToPoint;
           saddleSectionPly.ToPoint = temperply.ToPoint;
           return saddleSectionPly;
       
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
       static private IPoint RotateCoordinate(IPoint originalPt, double angle)
       {
           IPoint newPt = new PointClass();
           newPt.X = originalPt.X * Math.Cos(angle) + originalPt.Y * Math.Sin(angle);
           newPt.Y = originalPt.X * (-Math.Sin(angle)) + originalPt.Y * Math.Cos(angle);
           return newPt;
       }               
       static private Dictionary<double, double> SortedByZValue(Dictionary<double, double> paramDic)
       {
           Dictionary<double, double> newDic = new Dictionary<double, double>();            
           var dicSd = from objDic in paramDic orderby objDic.Value descending select objDic;
           foreach (KeyValuePair<double, double> keyv in dicSd)
          {
             newDic.Add(keyv.Key, keyv.Value);
          }            
           return newDic;

       }
        
       static private void GetSaddleRegions()
       {
           if (conPElev.Count == 1)
           {
               return;
           }
           if (listOne.Count == 0)
           {
               for (int i = 0; i < conPElev.Count - 1; i++)
               {
                   if (conPElev[i].Elev == conPElev[i + 1].Elev)
                   {
                       listOne.Add(conPElev[i]);
                   }
                   else
                   {
                       listOne.Add(conPElev[i]);
                       break;
                   }
               }
               for (int m = 0; m < listOne.Count; m++)
               {
                   conPElev.Remove(listOne[m]);
               }
           }

           List<conFeaInfor> listTwo = new List<conFeaInfor>();
           for (int i = 0; i < conPElev.Count - 1; i++)
           {
               if (conPElev[i].Elev == conPElev[i + 1].Elev)
               {
                   listTwo.Add(conPElev[i]);
               }
               else
               {
                   listTwo.Add(conPElev[i]);
                   break;
               }

           }
           for (int m = 0; m < listTwo.Count; m++)
           {
               conPElev.Remove(listTwo[m]);
           }
           List<conFeaInfor> temperList = null;

          
           for (int j = 0; j < listTwo.Count; j++)
           {
               IPolygon polygon = listTwo[j].PolygonOfCon;
               temperList = new List<conFeaInfor>();
               for (int i = 0; i < listOne.Count; i++)
               {
                   IPointCollection ptc = listOne[i].ConFea.Shape as IPointCollection;
                   bool isContain = IsContainPtInTerlk(polygon, ptc.get_Point(ptc.PointCount / 2));
                   if (isContain == true)
                   {
                       temperList.Add(listOne[i]);
                   }
               }
               if (temperList.Count == 1)
               {
                   listOne.Remove(temperList[0]);
               }
               else if (temperList.Count > 1)
               {
                   List<conFeaInfor> temList = new List<conFeaInfor>();
                   for (int i = 0; i < temperList.Count; i++)
                   {
                       temList.Add(temperList[i]);
                       listOne.Remove(temperList[i]);
                   }
                   saddlePtsInfor.Add(temList);
               }
           }
           listOne = listTwo;
           GetSaddleRegions();

       }
        
       static private Dictionary<IFeature, double> JudgeIntersectTerlk(IPolyline sectionPly, IPoint shortDisPt,double z)
        {
            Dictionary<IFeature, double> featureDic = new Dictionary<IFeature, double>();
            Dictionary<Dictionary<IFeature, double>,double > featureList = new Dictionary<Dictionary<IFeature,double>,double>();
            ITopologicalOperator topoOperator = sectionPly as ITopologicalOperator;
            for (int i = 0; i < allFeaList.Count; i++)
            {
                IFeature feature = allFeaList.Keys.ElementAt(i);                
                double elev = allFeaList.Values.ElementAt(i);
                if ((Math.Abs(elev -z)==intervalValue ||Math.Abs(elev -z)==2*intervalValue)&&elev<z)
                {
                    IGeometry pgeo = topoOperator.Intersect(feature.Shape as IGeometry, esriGeometryDimension.esriGeometry0Dimension);
                    if (pgeo.IsEmpty==false ) 
                    {
                        Dictionary<IFeature, double> dic = new Dictionary<IFeature, double>();
                        IPointCollection ptCol = pgeo as IPointCollection;
                        IPoint pt1 = ptCol.get_Point(0);
                        double dis = Math.Pow(pt1.X - shortDisPt.X, 2) + Math.Pow(pt1.Y - shortDisPt.Y, 2);
                        dic.Add(feature, elev);
                        featureList.Add(dic, dis);
                    }
                }                                 
            }
            if (featureList.Count==2)
            {
                if (featureList.Values.ElementAt(0)<featureList.Values.ElementAt(1))
                {
                    featureDic.Add(featureList.Keys.ElementAt(0).Keys.ElementAt(0), featureList.Keys.ElementAt(0).Values.ElementAt(0));
                    featureDic.Add(featureList.Keys.ElementAt(1).Keys.ElementAt(0), featureList.Keys.ElementAt(1).Values.ElementAt(0));
                }
                else
                {
                    featureDic.Add(featureList.Keys.ElementAt(1).Keys.ElementAt(0), featureList.Keys.ElementAt(1).Values.ElementAt(0));
                    featureDic.Add(featureList.Keys.ElementAt(0).Keys.ElementAt(0), featureList.Keys.ElementAt(0).Values.ElementAt(0));
                }
            }
            return featureDic;
        }            
        
       static private List<conFeaInfor> sameElevConStoreFun()
       {
           List<conFeaInfor> fList = new List<conFeaInfor>();
           for (int i = 0; i < conPElev.Count - 1; i++)
           {
               conFeaInfor fClass = new conFeaInfor();
               for (int j = 0; j < conPElev.Count - 1; j++)
               {
                   if (conPElev[j + 1].Elev == conPElev[j].Elev && conPElev[j].Area > conPElev[j + 1].Area)
                   {
                       fClass = conPElev[j + 1];
                       conPElev[j + 1] = conPElev[j];
                       conPElev[j] = fClass;

                   }
                   else if (conPElev[j + 1].Elev > conPElev[j].Elev)
                   {

                       fClass = conPElev[j + 1];
                       conPElev[j + 1] = conPElev[j];
                       conPElev[j] = fClass;
                   }

               }
           }
           fList = conPElev;
           return fList;
       }
        
       static private void GetClosedAndOpenStoreInfo(IFeatureLayer conFyr, IFeatureLayer boundFyr)
       {

           if (boundFyr != null)
           {
               CreateBoundary.MakeBounds.GetFourBoundarys(boundFyr);
           }
           else
           {
               CreateBoundary.GetBounds.GetNewBoundAndPtsDataFun(conFyr);
           }
            
           List<IPoint> temperXY = CreateBoundary.BoundsAndPtsOfXY.minAndMaxOfXY; //获取四边界点，有序,从左上角，逆时针一直到左上角                   ；
           Dictionary<IPolyline, int> boundry = CreateBoundary.BoundsAndPtsOfXY.boundry;
                     
           IFeatureClass pFeatureClass = conFyr.FeatureClass;
           IFeatureCursor pFcursor = pFeatureClass.Search(null, false);
           IFeature pFeature = pFcursor.NextFeature();
           conFeaInfor closedPolyongAndElev = null;
           while (pFeature != null)
           {
               closedPolyongAndElev = new conFeaInfor();
               int elevIndex = pFeature.Fields.FindField("Elev");              
               int markIndex = pFeature.Fields.FindField("Mark");  
               double elev = (double)pFeature.get_Value(elevIndex);
               short mark = (short)pFeature.get_Value(markIndex);
               IPolygon polygon = null; IPolyline pPolyline = null;
               if (mark == 0)
               {
                   pPolyline = pFeature.Shape as IPolyline;
                   polygon = ConstructPolygonFromPolyline(pPolyline);
               }
               else
               {
                  
                   ITopologicalOperator topoOperate = pFeature.Shape as ITopologicalOperator;
                   IPointCollection ptCol = pFeature.Shape as IPointCollection;
                   IPoint fristPt = ptCol.get_Point(0);
                   IPoint endPt = ptCol.get_Point(ptCol.PointCount - 1);
                   Dictionary<int, int> boundIndex = new Dictionary<int, int>(); 
                   for (int i = 0; i < boundry.Count; i++)
                   {
                       IPolyline boundPly = boundry.Keys.ElementAt(i);
                       int boundNO = boundry.Values.ElementAt(i);
                       double dis1 = ReturnMinDistance(boundPly, fristPt);
                       double dis2 = ReturnMinDistance(boundPly, endPt);
                       if (dis1 < 0.5 && dis2 < 0.5) 
                       {
                           boundIndex.Add(boundNO, 0); break;
                       }
                       else
                       {
                           if (dis1 < 0.5)
                           {
                               boundIndex.Add(boundNO, 0);
                           }
                           if (dis2 < 0.5)
                           {
                               boundIndex.Add(boundNO, 1);
                           }
                       }
                   }
                   List<IPoint> ptList = new List<IPoint>();
                   #region  
                   if (boundIndex.Count == 1) 
                   {
                                     
                       IPointCollection ring = new RingClass() as IPointCollection;
                       IRing rng = ring as IRing;
                       for (int p = 0; p < ptCol.PointCount; p++)
                       {
                           ring.AddPoint(ptCol.get_Point(p));
                           ptList.Add(ptCol.get_Point(p));
                       }
                       ring.AddPoint(fristPt);
                       ptList.Add(fristPt);
                       bool isClockWise = rng.IsExterior;
                       if (isClockWise == false) 
                       {
                           pPolyline = CreatePolyline(ptList);
                           polygon = ConstructPolygonFromPolyline(pPolyline);

                       }
                       else  
                       {
                           ring.RemovePoints(ring.PointCount - 1, 1);
                           ptList.RemoveAt(ptList.Count - 1);
                           BoundsAndPtsOfXY.minAndMaxOfXY[0].Z = BoundsAndPtsOfXY.minAndMaxOfXY[1].Z = BoundsAndPtsOfXY.minAndMaxOfXY[2].Z = BoundsAndPtsOfXY.minAndMaxOfXY[3].Z = elev;
                           if (boundIndex.Keys.ElementAt(0) == 0) 
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(fristPt);
                           }
                           else if (boundIndex.Keys.ElementAt(0) == 1) 
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(fristPt);
                           }
                           else if (boundIndex.Keys.ElementAt(0) == 2) 
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(fristPt);
                           }

                           else 
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(fristPt);
                           }
                           pPolyline = CreatePolyline(ptList);
                           polygon = ConstructPolygonFromPolyline(pPolyline);
                       }
                   }
                   #endregion
                   #region  
                   else
                   {
                       for (int p = 0; p < ptCol.PointCount; p++)
                       {
                           ptList.Add(ptCol.get_Point(p));
                       }
                       int boundOne = boundIndex.Keys.ElementAt(0);
                       int sOrEndPtOne = boundIndex.Values.ElementAt(0);
                       int boundTwo = boundIndex.Keys.ElementAt(1);
                       int sOrEndPtTwo = boundIndex.Values.ElementAt(1);
                       if (boundOne == 0 && boundTwo == 1)
                       {
                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(fristPt);
                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(fristPt);
                           }
                       }
                       else if (boundOne == 0 && boundTwo == 2)
                       {

                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(fristPt);
                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(fristPt);
                           }
                       }
                       else if (boundOne == 0 && boundTwo == 3)
                       {
                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(fristPt);

                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(fristPt);
                           }
                       }
                       else if (boundOne == 1 && boundTwo == 2)
                       {
                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(fristPt);
                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(fristPt);
                           }
                       }
                       else if (boundOne == 1 && boundTwo == 3)
                       {
                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {

                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(fristPt);
                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(fristPt);
                           }
                       }
                       else if (boundOne == 2 && boundTwo == 3)
                       {
                           if (sOrEndPtOne == 0 && sOrEndPtTwo == 1)
                           {

                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[0]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[1]);
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[2]);
                               ptList.Add(fristPt);

                           }
                           else
                           {
                               ptList.Add(BoundsAndPtsOfXY.minAndMaxOfXY[3]);
                               ptList.Add(fristPt);
                           }
                       }

                       pPolyline = CreatePolyline(ptList);
                       polygon = ConstructPolygonFromPolyline(pPolyline);
                   }
                   #endregion
               }
               IArea areaOfPolygon = polygon as IArea;
               closedPolyongAndElev.Elev = elev;
               closedPolyongAndElev.ConFea = pFeature;
               closedPolyongAndElev.PolygonOfCon = polygon;
               closedPolyongAndElev.Area = Math.Abs(areaOfPolygon.Area);
               conPElev.Add(closedPolyongAndElev);
               allFeaList.Add(pFeature, elev);
               pFeature = pFcursor.NextFeature();
           }

       }
        
       static private List<int> GetFeaturePtsFun(IFeature fea)
       {
           CalCurvature.teamValue = 0.004;
           Dictionary<int, double> feaDic = CalCurvature.GetInitialCValues(fea);
           List<int> feaPtIndexList = new List<int>();
           for (int j = 0; j < feaDic.Count; j++)
           {
               if (feaDic.Values.ElementAt(j) > 0)
               {
                   feaPtIndexList.Add(feaDic.Keys.ElementAt(j));
               }
           }
           return feaPtIndexList;
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
        
       static private IPolyline CreatePolyline(List<IPoint> ptList)
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
           IConstructCurve constructCurve = new PolylineClass();
           constructCurve.ConstructOffset(pline, 0, Type.Missing, Type.Missing);
           return constructCurve as IPolyline;
       }
        
       static private bool IsContainPtInTerlk(IPolygon pPolygon, IPoint inTerlkPt)
       {
           IGeometry pGeo = pPolygon as IGeometry;
           IRelationalOperator relations = pGeo as IRelationalOperator;
           if (relations.Contains(inTerlkPt))
           {
               return true;
           }
           else
           {
               return false;
           }
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
        
       static private double ReturnMinDistance(IPolyline ply, IPoint point)
       {
           double distanceAlongCurve = 0;       
           double distanceFromCurve = 0;            
           bool bRightSide = false;
           IPoint outPt = new PointClass();
           ply.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, false, outPt, ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
           return distanceFromCurve;
       }
        
       static private IPolyline GetBoundPly(IPoint pt1, IPoint pt2)
       {
           IPolyline ply = new PolylineClass();
           ply.FromPoint = pt1;
           ply.ToPoint = pt2;
           return ply;
       }
        
       static private double Length(IPoint pt1, IPoint pt2)
       {

           ILine line = new LineClass();
           line.FromPoint = pt1;
           line.ToPoint = pt2;
           return line.Length;
       }
        
       static private double Angle(IPoint pt1, IPoint pt2)
       {
           ILine line = new LineClass();
           line.FromPoint = pt1;
           line.ToPoint = pt2;
           return line.Angle;
       }
    }
}

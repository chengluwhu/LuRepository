using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ExtractFeaturePts;
using DataStructure;
namespace ExtractTerrainLines
{
   public class ChooseOptimalFeaPt
    {
         
        static public FeaPtInform GetOptiumPt(IFeatureLayer terlkFyr, List<FeaPtInform> list, FeaPtInform upPoint,ILine nline, bool isValOrRidge)
        {
            FeaPtInform temperFeaPt = new FeaPtInform();
            if (list.Count == 1)
            {
                temperFeaPt = list[0];
            }
            else if (list.Count == 2)
            {
                if (Math.Abs(list[0].CurvValues) > Math.Abs(list[1].CurvValues))
                {
                    temperFeaPt = list[0];
                }
                else
                {
                    temperFeaPt = list[1];
                }
            }
            else
            {
                 
                double maxC = MaxCvalue(list);
               
                List<double> midRateList = MiddlenessValue(list);
                double maxG = 0;
                
                if (upPoint != null)
                {
                    
                    IPoint ppt = new PointClass();
                    ILine angleLine = new LineClass();
                    if (upPoint.PtAtNumber!=0&&nline==null)
                    {
                        if (isValOrRidge == true)
                        {
                            ppt = PublicFunctionClass.CreateAngleBisectorPt(upPoint, terlkFyr, true);
                        }
                        else
                        {
                            ppt = PublicFunctionClass.CreateAngleBisectorPt(upPoint, terlkFyr, false);
                        }
                        
                       angleLine.FromPoint = upPoint.PtCoord;
                        angleLine.ToPoint = ppt;
                    }
                    else if (upPoint.PtAtNumber==0&&nline!=null)
                    {
                        angleLine.FromPoint = nline.FromPoint;
                        angleLine.ToPoint = nline.ToPoint;
                    }                                
                    
                    double maxAngle = MaxDeflecAngle(list, upPoint.PtCoord, angleLine);
                    double maxDis = MaxDistance(list, upPoint.PtCoord);
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        double mid = 0; 
                        if (i == 0 || i == list.Count - 1)
                        {
                            mid = 0;
                        }
                        else
                        {
                            mid = midRateList[i];
                        }
                        FeaPtInform fpt_1 = list[i];
                        double cRate = CalCurveOfRate(Math.Abs(fpt_1.CurvValues), maxC); 
                         
                        FeaPtInform fpt = list[i];
                        ILine linkLine = new LineClass();
                        linkLine.FromPoint = upPoint.PtCoord;
                        linkLine.ToPoint = fpt.PtCoord;
                        double angle = IntersectionAngle(fpt, upPoint.PtCoord, angleLine);                       
                       
                        double InnerAngle = CalAngleOfRate(angle, maxAngle);
                         
                        double disR = CalShortRate(linkLine.Length, maxDis);
                        double maxGravitation = GetExpectedStream(cRate, disR, InnerAngle, mid);
                        if (maxGravitation > maxG)
                        {
                            maxG = maxGravitation;
                            temperFeaPt = fpt_1;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        double mid = 0; 
                        if (i == 0 || i == list.Count - 1)
                        {
                            mid = 0;
                        }
                        else
                        {
                            mid = midRateList[i];
                        }
                        FeaPtInform fpt_1 = list[i];
                        double cRate = CalCurveOfRate(Math.Abs(fpt_1.CurvValues), maxC);//曲率比率
                        double maxGravitation = GetExpectedStream(cRate, 0, 0, mid);
                        if (maxGravitation > maxG)
                        {
                            maxG = maxGravitation;
                            temperFeaPt = fpt_1;
                        }
                    }
                }
            }
            return temperFeaPt;
        }
         
        static public double GetExpectedStream(double curveR, double shortDisR, double angleR, double midR)
        {
            //double K1 = 0.2; double K2 = 0.7; double K3 = 0.1; double K4 =0;
            //double K1 = 0.15; double K2 = 0.7; double K3 = 0.05; double K4 = 0.1;
            double K1 = 0.15; double K2 = 0.7; double K3 = 0.05; double K4 = 0.1;
            if (shortDisR == 0 && angleR == 0)
            {
                K1 = 0.3; K2 = 0; K3 = 0; K4 = 0.7;
            }
            double T = K1 * Math.Abs(curveR) + K2 * shortDisR + K3 * angleR + K4 * midR;
            return T;
        }
         
        static public double IntersectionAngle(FeaPtInform fpt, IPoint upPoint, ILine angleLine)
        {

            ILine linkLine = new LineClass();
            linkLine.FromPoint = upPoint;
            linkLine.ToPoint = fpt.PtCoord;
            Point_T fristFpt = new Point_T(linkLine.FromPoint.X, linkLine.FromPoint.Y);
            Point_T secondFpt = new Point_T(linkLine.ToPoint.X, linkLine.ToPoint.Y);
            Vector_T vec1 = new Vector_T(fristFpt, secondFpt);
            Point_T frp = new Point_T(angleLine.FromPoint.X, angleLine.FromPoint.Y);
            Point_T tp = new Point_T(angleLine.ToPoint.X, angleLine.ToPoint.Y);
            Vector_T vec2 = new Vector_T(frp, tp);
           
            double angle = Math.Abs(vec1.Angle(vec2));          
            return angle;


        }
         
        static public double CalMidRate(double currentLength, double average, double totalLength)
        {

            double l = 1 - (Math.Abs(currentLength - average) / (0.5 * totalLength));
            return l;
        }
         
        static public double CalShortRate(double currentLength, double maxLength)
        {

            double l = 1 - currentLength / maxLength;
            return l;
        }
         
        static public double CalAngleOfRate(double AddCurve, double maxAnge)
        {
            double c = 1 - AddCurve / maxAnge;
           
            return c;
        }
         
        static public double CalCurveOfRate(double currentCurve, double maxCurve)
        {
            return Math.Abs(currentCurve / maxCurve);
        }
         
        static public double MaxCvalue(List<FeaPtInform> list)
        {
            double maxC = 0;
            for (int i = 0; i < list.Count; i++)
            {
                FeaPtInform fpt_1 = list[i];
                if (Math.Abs(fpt_1.CurvValues) > maxC)
                {
                    maxC = Math.Abs(fpt_1.CurvValues);
                }
            }
            return maxC;

        }
         
        static public List<double> MiddlenessValue(List<FeaPtInform> list)
        {

            //计算总长度
            double totalLength = 0;
            for (int i = 0; i < list.Count - 1; i++)
            {
                FeaPtInform fpt_1 = list[i];
                FeaPtInform fpt_2 = list[i + 1];
                totalLength += Math.Sqrt(Math.Pow(fpt_1.PtCoord.X - fpt_2.PtCoord.X, 2) + Math.Pow(fpt_1.PtCoord.Y - fpt_2.PtCoord.Y, 2));
            }
            double averageLength = totalLength / 2;//中间长度
            double len = 0;
            List<double> midRateList = new List<double>();
            for (int i = 0; i < list.Count - 1; i++)
            {
                FeaPtInform fpt_1 = list[i];
                FeaPtInform fpt_2 = list[i + 1];
                len += Math.Sqrt(Math.Pow(fpt_1.PtCoord.X - fpt_2.PtCoord.X, 2) + Math.Pow(fpt_1.PtCoord.Y - fpt_2.PtCoord.Y, 2));
                /////////中间率
                double midRate = CalMidRate(len, averageLength, totalLength);
                midRateList.Add(midRate);

            }
            midRateList.Insert(0, 0);
            return midRateList;
        }
         
        static public double MaxDistance(List<FeaPtInform> list, IPoint upPt)
        {
            double maxDis = 0;
            for (int i = 0; i < list.Count; i++)
            {
                FeaPtInform fpt = list[i];
                ILine linkLine = new LineClass();
                linkLine.FromPoint = upPt;
                linkLine.ToPoint = fpt.PtCoord;
                if (linkLine.Length > maxDis)
                {
                    maxDis = linkLine.Length;
                }
            }
            return maxDis;
        }
         
        static public double MaxDeflecAngle(List<FeaPtInform> list, IPoint upPoint, ILine angleLine)
        {

            double maxAngle = 0;
            for (int i = 0; i < list.Count; i++)
            {
                FeaPtInform fpt = list[i];
                ILine linkLine = new LineClass();
                linkLine.FromPoint = upPoint;
                linkLine.ToPoint = fpt.PtCoord;
                ////两向量的夹角
                double angle = IntersectionAngle(fpt, upPoint, angleLine);
                if (angle > maxAngle)
                {
                    maxAngle =angle;
                }
            }
            return maxAngle;
        }



    }
}

using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtractFeaturePts;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
namespace ExtractionTFL_Prj
{
    public partial class ExtractionFpts : Form
    {
        IMap _mapControl;
        public ExtractionFpts(IMap mapControl)
        {
            InitializeComponent();
            _mapControl = mapControl;
        }
        IFeatureLayer conFyr;
        IFeatureLayer newFeaPtLyr;
        IFeatureLayer boundaryFyr;
        private void contourOpenButton_Click(object sender, EventArgs e)
        {
            conFyr = publicClass.OpenShapeData(_mapControl, openFileDialog1);
            if (conFyr.FeatureClass.ShapeType==esriGeometryType.esriGeometryPoint)
            {
                MessageBox.Show("Please input contour lines");
                return;
            }
            ILayer pLayer = conFyr as ILayer;
            _mapControl.AddLayer(pLayer);
            contourTextBox.Text = publicClass.InputFileOfName;  
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string savePath = publicClass.Savefunction(saveFileDialog1);
            saveTextBox.Text = savePath;
        }

        private void oKButton_Click(object sender, EventArgs e)
        {
            newFeaPtLyr = publicClass.CreateShapefileLayer(_mapControl, publicClass.FilePath, publicClass.GdbName, 0);
            /////////Extract mountain tops
            ExtractFeaturePts.ExtractMountainTops.intervalValue = 10;//contour lines interval;
            ExtractFeaturePts.ExtractMountainTops.GetMouAndBotPts(conFyr);
            for (int i = 0; i < ExtractMountainTops.mounTPtList.Count; i++)
            {
                ExtractFeaturePts.AttainFpts.CreateFPtByDictionary(newFeaPtLyr, ExtractMountainTops.mounTPtList.Values.ElementAt(i).OID, ExtractMountainTops.mounTPtList.Keys.ElementAt(i), 0, 0);
            }
            ////////Extract saddle points             
            ExtractSaddlePoints.intervalValue = 10;//contour lines interval;
            ExtractSaddlePoints.DealWithSaddles(conFyr, boundaryFyr, newFeaPtLyr);           
           ///////Extract valley\ridge points        
            IFeatureClass fClass = conFyr.FeatureClass;
            IFeatureCursor pFCursor = fClass.Search(null, false);
            IFeature pFeature = pFCursor.NextFeature();
            while (pFeature != null)
            {
                IPointCollection ptCol = pFeature.Shape as IPointCollection;
                CalCurvature.teamValue = 0.004;
                Dictionary<int, double> cVDic = CalCurvature.GetInitialCValues(pFeature);
                for (int i = 0; i < cVDic.Count; i++)
                {
                    int loc = cVDic.Keys.ElementAt(i);
                    double cValue = cVDic.Values.ElementAt(i);
                    IPoint feaPt = ptCol.get_Point(loc);
                    AttainFpts.CreateFPtByDictionary(newFeaPtLyr, pFeature.OID, feaPt, loc, cValue);
                }
                pFeature = pFCursor.NextFeature();
            }
            ILayer lyr = newFeaPtLyr as ILayer;
            _mapControl.AddLayer(lyr);            
            this.Close();
        
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOpenBoundary_Click(object sender, EventArgs e)
        {
            boundaryFyr = publicClass.OpenShapeData(_mapControl, openFileDialog2);
            if (boundaryFyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                MessageBox.Show("Please input boundaries");
                return;
            }
            ILayer pLayer = boundaryFyr as ILayer;
            _mapControl.AddLayer(pLayer);
            tbBoundary.Text = publicClass.InputFileOfName; 
        }
    }
}

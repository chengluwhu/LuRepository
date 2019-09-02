using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ExtractTerrainLines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtractionTFL_Prj
{
    public partial class GetTerrainFlines : Form
    {
        IMap _mapControl;
        public GetTerrainFlines(IMap mapControl)
        {
            InitializeComponent();
            _mapControl = mapControl;
        }
        IFeatureLayer conFyr;
        IFeatureLayer newFeaPtLyr;
        IFeatureLayer newTerrainFlyrs;
        private void fptOpenButton_Click(object sender, EventArgs e)
        {
            newFeaPtLyr = publicClass.OpenShapeData(_mapControl, openFileDialog2);
            if (newFeaPtLyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("Please input filtered feature points");
                return;
            }
            ILayer pLayer = newFeaPtLyr as ILayer;
            _mapControl.AddLayer(pLayer);
            tbFpts.Text = publicClass.InputFileOfName;  
        }

        private void contourOpenButton_Click(object sender, EventArgs e)
        {
            conFyr = publicClass.OpenShapeData(_mapControl, openFileDialog1);
            if (conFyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
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
            saveTerrainFlines.Text = savePath;
        }

        private void oKButton_Click(object sender, EventArgs e)
        {
            newTerrainFlyrs = publicClass.CreateShapefileLayer(_mapControl, publicClass.FilePath, publicClass.GdbName,1);
            GetValleyAndRidgeLines.TerrainFeaLinesFun(conFyr,newFeaPtLyr,newTerrainFlyrs);
            ILayer lyr = newTerrainFlyrs as ILayer;
            _mapControl.AddLayer(lyr);
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

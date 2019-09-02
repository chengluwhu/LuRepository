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
namespace ExtractionTFL_Prj
{
    public partial class FeaPtsFiltration : Form
    {
        IMap _mapControl;
        public FeaPtsFiltration(IMap mapControl)
        {
            InitializeComponent();
            _mapControl = mapControl;
        }
        IFeatureLayer originalFPtsFyr;
        IFeatureLayer conFyr;
        IFeatureLayer newFeaPtLyr;
        private void fptOpenButton_Click(object sender, EventArgs e)
        {
            originalFPtsFyr = publicClass.OpenShapeData(_mapControl, openFileDialog1);
            if (originalFPtsFyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                MessageBox.Show("Please input original extracted feature points for filtration");
                return;
            }
            ILayer pLayer = originalFPtsFyr as ILayer;
            _mapControl.AddLayer(pLayer);
            tbFpts.Text = publicClass.InputFileOfName;  
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string savePath = publicClass.Savefunction(saveFileDialog1);
            savefpts.Text = savePath;
        }

        private void oKButton_Click(object sender, EventArgs e)
        {
            newFeaPtLyr = publicClass.CreateShapefileLayer(_mapControl, publicClass.FilePath, publicClass.GdbName, 0);
            DeleteNoisePtByDCE.SurplesFptsByDCE(originalFPtsFyr, conFyr, newFeaPtLyr);
            ILayer lyr = newFeaPtLyr as ILayer;
            _mapControl.AddLayer(lyr);            
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contourOpenButton_Click(object sender, EventArgs e)
        {
            conFyr = publicClass.OpenShapeData(_mapControl, openFileDialog2);
            if (conFyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
            {
                MessageBox.Show("Please input contour lines");
                return;
            }
            ILayer pLayer = conFyr as ILayer;
            _mapControl.AddLayer(pLayer);
            contourTextBox.Text = publicClass.InputFileOfName;  
        }
    }
}

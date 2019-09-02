using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtractTerrainLines;
using ESRI.ArcGIS.Carto;
namespace ExtractionTFL_Prj
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
   
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ExtractionFpts fptsFrm = new ExtractionFpts(axMapControl1.Map);
            fptsFrm.ShowDialog();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FeaPtsFiltration frm = new FeaPtsFiltration(axMapControl1.Map);
            frm.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            GetTerrainFlines getTFLines = new GetTerrainFlines(axMapControl1.Map);
            getTFLines.ShowDialog();
        }
    }
}

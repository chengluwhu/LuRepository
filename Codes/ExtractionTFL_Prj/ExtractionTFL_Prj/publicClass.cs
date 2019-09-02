using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using System.Windows.Forms;
using System.IO;

namespace ExtractionTFL_Prj
{
    public class publicClass
    {
        static private string inputFileOfName;

        public static string InputFileOfName
        {
            get { return publicClass.inputFileOfName; }
            set { publicClass.inputFileOfName = value; }
        }
        static public IFeatureLayer OpenShapeData(IMap mapControl, OpenFileDialog OpenFdlg)
        {

            OpenFdlg.Title = "choose shape file";
            OpenFdlg.Filter = "Shape file（*.shp）|*.shp";
            OpenFdlg.ShowDialog();
            string strFileName = OpenFdlg.FileName;
            if (strFileName == string.Empty)
            {
                return null;
            }
            string pathName = System.IO.Path.GetDirectoryName(strFileName);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(strFileName);
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pathName, 0);
            IFeatureWorkspace pFeatureWorkspace;
            pFeatureWorkspace = pWorkspace as IFeatureWorkspace;
            IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(fileName);
            IDataset pDataset = pFeatureClass as IDataset;
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureClass;
            pFeatureLayer.Name = pDataset.Name;
            //ILayer pLayer = pFeatureLayer as ILayer;
            //mapControl.AddLayer(pLayer);
            InputFileOfName = strFileName;
            return pFeatureLayer;
        }

        static private string filePath;

        static public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        static private string gdbName;

        static public string GdbName
        {
            get { return gdbName; }
            set { gdbName = value; }
        }
        static public string Savefunction(SaveFileDialog saveFileDialog)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                string file = saveFileDialog.FileName;
                string filePath;
                string fileName;
                int index = 0;
                index = file.LastIndexOf("\\");
                filePath = file.Substring(0, index);
                fileName = file.Substring(index + 1, file.Length - index - 1);
                GdbName = @saveFileDialog.FileName + ".gdb";
                bool b = System.IO.Directory.Exists(GdbName);
                if (b)
                {
                    if (MessageBox.Show("There is a same file in the folder,do you want it to be replaced?", "Ask", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        DeleteFolder(GdbName);
                    }
                    else
                        return null;
                }

                FilePath = filePath;
                gdbName = fileName;
                string File = file;
                return File;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="dir"></param>
        static private void DeleteFolder(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    foreach (string d in Directory.GetFileSystemEntries(dir))
                    {
                        if (File.Exists(d))
                            File.Delete(d);
                        else
                            DeleteFolder(d);
                    }
                    Directory.Delete(dir, true);
                }
            }
            catch (Exception ee)
            {

                MessageBox.Show(ee.ToString());
            }

        }

        static public IFeatureLayer CreateShapefileLayer(IMap mapCtrl, string strFolder, string strName, int app)//app表示 0-点要素，1-线要素
        {
            const string strShapeFieldName = "Shape";
            //Open the folder to contain the shapefile as a workspace
            IFeatureWorkspace pFWS;
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFWS = pWorkspaceFactory.OpenFromFile(strFolder, 0) as IFeatureWorkspace;
            //Set up a simple fields collection
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            //Make the shape field 
            //it will need a geometry definition, with a spatial reference
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = strShapeFieldName;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDef pGeometryDef = new GeometryDef();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            if (app == 0)
            {
                pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            }
            else
            {
                pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            }
            pGeometryDefEdit.SpatialReference_2 = mapCtrl.SpatialReference;
            pGeometryDefEdit.HasZ_2 = true;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "Name";
            pFieldEdit.AliasName_2 = "AliasName";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "Elev";
            pFieldEdit.AliasName_2 = "Elev";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "TerlkFID";
            pFieldEdit.AliasName_2 = "TerlkFID";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "WhereAtcou";
            pFieldEdit.AliasName_2 = "WhereAtcou";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "Mark";
            pFieldEdit.AliasName_2 = "Mark";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);
            //Add another field
            pField = new FieldClass();
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Length_2 = 30;
            pFieldEdit.Name_2 = "Curve";
            pFieldEdit.AliasName_2 = "Curve";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            pFieldsEdit.AddField(pField);
            //Add another field
            if (app==1)
            {
                pField = new FieldClass();
                pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Length_2 = 30;
                pFieldEdit.Name_2 = "Code";
                pFieldEdit.AliasName_2 = "Code";
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                pFieldsEdit.AddField(pField);
            }            
            //Create the shapefile
            IFeatureClass featureClass = pFWS.CreateFeatureClass(strName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            IFeatureLayer featurelayer = new FeatureLayerClass();
            featurelayer.Name = strName;
            featurelayer.FeatureClass = featureClass;
            //ILayer layer = featurelayer as ILayer;
            return featurelayer;
            //mapCtrl.AddLayer((ILayer)featurelayer);
        }

    }
}

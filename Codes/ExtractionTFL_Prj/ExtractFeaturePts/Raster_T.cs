using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractFeaturePts
{
    public class PixelPosition_T
    {
        private int row;

        public int Row
        {
            get { return row; }
            set { row = value; }
        }
        private int col;

        public int Col
        {
            get { return col; }
            set { col = value; }
        }
        private int code;

        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        public PixelPosition_T()
        {
        }
        public PixelPosition_T(int r, int c)
        {
            row = r;
            col = c;
        }
        public PixelPosition_T(int r, int c, int cd)
        {
            row = r;
            col = c;
            code = cd;
        }

    }
    public class Raster_T
    {
        private int rows;

        public int Rows
        {
            get { return rows; }
            set { rows = value; }
        }
        private int cols;

        public int Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        private double pixelsize;

        public double Pixelsize
        {
            get { return pixelsize; }
            set { pixelsize = value; }
        }
        private IPoint originpt;

        public IPoint Originpt
        {
            get { return originpt; }
            set { originpt = value; }
        }
        public Raster_T()
        { }
        public Raster_T(int rs, int cs, double ps, IPoint opt)
        {
            rows = rs;
            cols = cs;
            pixelsize = ps;
            originpt = opt;
        }
    }
}
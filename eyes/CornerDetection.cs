using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;

namespace eyes
{
    class CornerDetection
    {
        Image<Gray, byte> img;
        Image<Gray, byte> CornerL;
        Image<Gray, byte> CornerR;
        
        int row;
        int col;
        double[] colMean, rowMean;
        double[] colBlack,rowBlack;
        

        public CornerDetection () { }
        public CornerDetection (Image<Bgr, byte> EyeROI) {
            img = EyeROI.Convert<Gray, byte>();
            
            this.row = EyeROI.Height;
            this.col = EyeROI.Width;
            rowMean = new double[row];
            colMean = new double[col];
            rowBlack = new double[row];
            colBlack = new double[col];
            
            col_row_Mean_Black();

            // Create directory to save image 000
            if (!Directory.Exists("R")){ Directory.CreateDirectory("R"); }
            if (!Directory.Exists("L")){ Directory.CreateDirectory("L"); }
            Console.Write("testc");
        }

        private void col_row_Mean_Black() {
            //Column Mean
            double sum = 0;
            for (int x = 0; x < col; ++x)
            {
                for (int y = 0; y < row; ++y)
                {
                    sum += img[y, x].Intensity;
                    if (img[y, x].Intensity <= 70) {
                        colBlack[x]++;
                        
                    }
                }
                colMean[x] = sum / row;
                sum = 0;
            }
            //Row Mean
            for (int y = 0; y < row; ++y)
            {
                for (int x = 0; x < col; ++x)
                {
                    sum += img[y, x].Intensity;
                    if (img[y, x].Intensity <= 70)
                    {
                        rowBlack[y]++;
                    }
                }
                rowMean[y] = sum / col;
                sum = 0;
            }
        }
        public void VPF(String LorR ,out Rectangle IrisROI, out Image<Bgr,byte> Iris) {

            //Draw the variance for visualization
            Image<Gray, byte> variance = img.Clone();
            Image<Gray, byte> varianceV = img.Clone();
            Image<Gray, byte> varianceH = img.Clone();

            // Vertical part
            // Calculate vertical VPF
            Dictionary<int, Double> vVPF = new Dictionary<int, double>();
            calculateVPF(vVPF, "Vertical");


            // Calculate the largest diffrient between the point of variance
            Point[] p = new Point[col];
            Dictionary<int, int> diffMax = new Dictionary<int, int>();
            int CornerROI_Lx, CornerROI_Rx;
            VerticalDiffMax(p, out CornerROI_Lx,out CornerROI_Rx, vVPF,10);

            // Draw the vertical line to segment the CORNER area
            variance.Draw(new LineSegment2D(new Point(CornerROI_Lx, 0), new Point(CornerROI_Lx, variance.Height)), new Gray(255), 1);
            variance.Draw(new LineSegment2D(new Point(CornerROI_Rx, 0), new Point(CornerROI_Rx, variance.Height)), new Gray(255), 1);

            foreach (var dot in p)
            {
                varianceV.Draw(new CircleF(dot, 0), new Gray(255), 0);
            }

            //varianceV.DrawPolyline(p, false, new Gray(255));
            varianceV.Save(LorR + "\\varianceV.jpg");
            
            // Horizontal part
            // Calculate horizontal VPF
            Dictionary<int, Double> hVPF = new Dictionary<int, double>();
            calculateVPF(hVPF, "Horizontal");


            // Calculate the diffrient between the point of variance
            p = new Point[row];
            diffMax = new Dictionary<int, int>();
            int CornerROI_Dy, CornerROI_Uy;
            HorizontalDiffMax(p, out CornerROI_Uy,out CornerROI_Dy, hVPF,10);

            

            // Draw the horizontal line to segment the CORNER area
            variance.Draw(new LineSegment2D(new Point(0, CornerROI_Uy), new Point(variance.Width, CornerROI_Uy)), new Gray(255), 1);
            variance.Draw(new LineSegment2D(new Point(0, CornerROI_Dy), new Point(variance.Width, CornerROI_Dy)), new Gray(255), 1);
            variance.Save(LorR + "\\varianceSegment.jpg");

            foreach (var dot in p)
            {
                varianceH.Draw(new CircleF(dot, 0), new Gray(255), 0);
            }
            //varianceH.DrawPolyline(p, false, new Gray(255));
            varianceH.Save(LorR + "\\varianceH.jpg");

            varianceV.DrawPolyline(p, false, new Gray(255));
            varianceV.Save(LorR + "\\varianceWave.jpg");


            // Set Iris ROI
            IrisROI = new Rectangle();
            IrisROI.X = CornerROI_Lx;
            IrisROI.Y = CornerROI_Uy;
            IrisROI.Width = CornerROI_Rx - CornerROI_Lx;
            IrisROI.Height = CornerROI_Dy - CornerROI_Uy;

            Iris = img.Convert<Bgr,byte>();
            Iris.ROI = IrisROI;
            Iris.Save(LorR+ "\\Iris.jpg");

            // Set Corner ROI
            int offset = 30;
            Image<Gray, byte> Corner = img.Clone();
            int CornerL_width = (CornerROI_Rx + offset) < img.Width ? offset : img.Width - CornerROI_Rx;
            int CornerR_x = (CornerROI_Lx - offset) > 0 ? CornerROI_Lx - offset : 0;

           // if (LorR == "R") CornerR_x -= 80;

           // if(LorR == "L") CornerROI_Rx += 70;


            CornerR = img.Clone();
            CornerR.ROI = new Rectangle(CornerR_x, CornerROI_Uy, offset, CornerROI_Dy - CornerROI_Uy);
            CornerR.Save(LorR + @"\\CornerR.jpg");

            CornerL = img.Clone();
            CornerL.ROI = new Rectangle(CornerROI_Rx, CornerROI_Uy, CornerL_width, CornerROI_Dy - CornerROI_Uy);
            CornerL.Save(LorR + @"\\CornerL.jpg");

        }


        // input ImgName ,the First charector indicate Left or Right eye
        // return upperEyelid's y position
        public int VPF_eyelidsDetect(String ImgName)
        {
            //Draw the variance for visualization
            Image<Gray, byte> variance = img.Clone();
            Image<Gray, byte> varianceH = img.Clone();

            // Horizontal part
            // Calculate horizontal VPF
            Dictionary<int, Double> hVPF = new Dictionary<int, double>();
            calculateVPF(hVPF, "Horizontal");

            // Calculate the diffrient between the point of variance
            Point[] p = new Point[row];
            int CornerROI_Dy, CornerROI_Uy;
            HorizontalDiffMax(p, out CornerROI_Uy, out CornerROI_Dy, hVPF,0);

            // Draw the horizontal line to segment the CORNER area
            variance.Draw(new LineSegment2D(new Point(0, CornerROI_Uy), new Point(variance.Width, CornerROI_Uy)), new Gray(255), 1);
            variance.Draw(new LineSegment2D(new Point(0, CornerROI_Dy), new Point(variance.Width, CornerROI_Dy)), new Gray(255), 1);

            variance.Save(ImgName.First() + "\\" + ImgName + "Segment.jpg");
            

            foreach (var dot in p)
            {
                varianceH.Draw(new CircleF(dot, 0), new Gray(255), 0);
            }
            varianceH.Save(ImgName.First() + "\\" + ImgName + "H.jpg");

            return CornerROI_Uy;
        }

        public Point WVPF(String Identifier) {

            #region setting
            if (Identifier.Last().ToString() == "R")
            {
                img = CornerR.Clone();
                this.row = CornerR.Height;
                this.col = CornerR.Width;
                rowMean = new double[row];
                colMean = new double[col];
                col_row_Mean_Black();
            }
            if (Identifier.Last().ToString() == "L")
            {
                img = CornerL.Clone();
                this.row = CornerL.Height;
                this.col = CornerL.Width;
                rowMean = new double[row];
                colMean = new double[col];
                col_row_Mean_Black();
            }


            #endregion

            Dictionary<int, Double> vVPF = new Dictionary<int, double>();
            Dictionary<int, Double> hVPF = new Dictionary<int, double>();

            #region harris

            Image<Gray, float> CornerL_harris = img.Convert<Gray,float>().CopyBlank();
            Image<Gray, float> CornerR_harris = img.Convert<Gray, float>().CopyBlank();

            double min = 0, max = 0;
            Point minP = new Point();
            Point maxP = new Point();
            if (Identifier.First().ToString() == "R" && Identifier.Last().ToString() == "L")// Patient's right eye inner corner
            {
                CornerL_harris = CornerL.Convert<Gray, float>();
                CvInvoke.CornerHarris(CornerL, CornerL_harris, 3);
                CvInvoke.Normalize(CornerL_harris, CornerL_harris, 0, 255, NormType.MinMax, DepthType.Cv32F);
                CornerL_harris = CornerL_harris.AbsDiff(new Gray(0));
                CvInvoke.MinMaxLoc(CornerL_harris, ref min, ref max, ref minP, ref maxP);

                vVPF = calculateVPF(vVPF, "Vertical",CornerR_harris);
                hVPF = calculateVPF(hVPF, "Horizontal", CornerR_harris);
            }
            else if (Identifier.First().ToString() == "L" && Identifier.Last().ToString() == "R")// Patient's left eye  inner corner
            {
                CornerR_harris = CornerR.Convert<Gray, float>();
                CvInvoke.CornerHarris(CornerR, CornerR_harris, 3);
                CvInvoke.Normalize(CornerR_harris, CornerR_harris, 0, 255, NormType.MinMax, DepthType.Cv32F);
                CornerR_harris = CornerR_harris.AbsDiff(new Gray(0));
                CvInvoke.MinMaxLoc(CornerR_harris, ref min, ref max, ref minP, ref maxP);

                vVPF = calculateVPF(vVPF, "Vertical", CornerL_harris);
                hVPF = calculateVPF(hVPF, "Horizontal", CornerL_harris);
            }
            else
            {
                vVPF = calculateVPF(vVPF, "Vertical");
                hVPF = calculateVPF(hVPF, "Horizontal");
            }
            #endregion

            //Draw the variance for visualization
            Image<Gray, byte> variance = img.Clone();

            // Calculate the diffrience between the point of variance
            Point[] p = new Point[col];
            Dictionary<int, int> diffMax = new Dictionary<int, int>();
            diffMax = VerticalDiffMax(p, diffMax, vVPF);

    
            var dicSort = from objDic in diffMax orderby objDic.Value descending select objDic;
            int CornerX = 0;

            if (Identifier.Last().ToString() == "L")
            {
                CornerX = dicSort.ElementAt(0).Key + CornerL.ROI.X;
            }
            if (Identifier.Last().ToString() == "R")
            {
                CornerX = dicSort.ElementAt(0).Key + CornerR.ROI.X;
            }

            variance.Draw(new LineSegment2D(new Point(dicSort.ElementAt(0).Key, 0), new Point(dicSort.ElementAt(0).Key, variance.Height)), new Gray(0), 1);
            variance.DrawPolyline(p, false, new Gray(255));
            variance.Save(Identifier.First().ToString() + "\\varianceVertical" + Identifier.Last().ToString() + ".jpg");




            // Calculate the diffrience between the point of variance
            p = new Point[row];
            diffMax = new Dictionary<int, int>();
            diffMax = HorizontalDiffMax(p, diffMax, hVPF);

            dicSort = from objDic in diffMax orderby objDic.Value descending select objDic;

            int CornerY = 0;

            if (Identifier.Last().ToString() == "L")
            {
                CornerY = dicSort.ElementAt(0).Key + CornerL.ROI.Y;
            }
            else if (Identifier.Last().ToString() == "R")
            {
                CornerY = dicSort.ElementAt(0).Key + CornerR.ROI.Y;
            }
            
            
            // Draw a line to segment the CORNER area
            variance.Draw(new LineSegment2D(new Point(0, dicSort.ElementAt(0).Key), new Point(variance.Width, dicSort.ElementAt(0).Key)), new Gray(0), 1);
            variance.DrawPolyline(p, false, new Gray(255));
            variance.Save(Identifier.First().ToString() + "\\varianceCross" + Identifier.Last().ToString() + ".jpg");

            variance = img.Clone();
            variance.Draw(new LineSegment2D(new Point(0, dicSort.ElementAt(0).Key), new Point(variance.Width, dicSort.ElementAt(0).Key)), new Gray(0), 1);
            variance.DrawPolyline(p, false, new Gray(255));
            variance.Save(Identifier.First().ToString() + "\\varianceHorizontal" + Identifier.Last().ToString() + ".jpg");


            if (Identifier.First().ToString() == "R" && Identifier.Last().ToString() == "L")// Patient's right eye inner corner
            {
                return maxP;
            }
            else if (Identifier.First().ToString() == "L" && Identifier.Last().ToString() == "R")// Patient's left eye inner corner
            {
                return maxP;
            }
            else
            {
                return new Point(CornerX, CornerY);
            }

        }

        // Calculate Weighted VPF
        private Dictionary<int, double> calculateVPF(Dictionary<int ,double> VPF, String direction, Image<Gray,float> harris)
        {
            double sum = 0;
            
            if (direction == "Vertical")
            {
                for (int x = 0; x < col; ++x)
                {
                    for (int y = 0; y < row; ++y)
                    {
                        sum += harris[y,x].Intensity * Math.Pow(img[y, x].Intensity - colMean[x], 2);
                    }
                    VPF.Add(x, sum / row);
                    sum = 0;
                }
            }
            if (direction == "Horizontal")
            {
                for (int y = 0; y < row; ++y)
                {
                    for (int x = 0; x < col; ++x)
                    {
                        sum += harris[y, x].Intensity * Math.Pow(img[y, x].Intensity - rowMean[y], 2);
                    }
                    VPF.Add(y, sum / col);
                    sum = 0;
                }
            }

            return VPF;
        }
        // Calculate Unweighted VPF
        private Dictionary<int, double> calculateVPF(Dictionary<int, double> VPF, String direction)
        {
            double sum = 0;
            if (direction == "Vertical")
            {
                for (int x = 0; x < col; ++x)
                {
                    for (int y = 0; y < row; ++y)
                    {
                        sum += Math.Pow(img[y, x].Intensity - colMean[x], 2);
                    }
                    sum *= colBlack[x];
                    VPF.Add(x, sum / row);
                    sum = 0;
                }
            }
            if (direction == "Horizontal")
            {
                for (int y = 0; y < row; ++y)
                {
                    for (int x = 0; x < col; ++x)
                    {
                        sum += Math.Pow(img[y, x].Intensity - rowMean[y], 2);
                    }
                    sum *= rowBlack[y];
                    VPF.Add(y, sum / col);
                    sum = 0;
                }
            }

            return VPF;
        }

        // Rerange the variance to the height of image
        // Calculate the largest diffrient between the point of variance
        private Dictionary<int, int> VerticalDiffMax(Point[] p, Dictionary<int, int> diffMax, Dictionary<int, double> VPF)
        {
            p[0].X = 0;
            p[0].Y = (int)((VPF[0] - VPF.Values.Min()) * row / (VPF.Values.Max() - VPF.Values.Min())); // Shift
            for (int x = 1; x < col; ++x)
            {
                p[x].X = x;
                p[x].Y = (int)((VPF[x] - VPF.Values.Min()) * row / (VPF.Values.Max() - VPF.Values.Min()));

                diffMax.Add(p[x].X, p[x].Y);
            }
            if (col == 1)
            {
                diffMax.Add(0, p[0].Y);
            }
            return diffMax;
        }

        private Dictionary<int, int> HorizontalDiffMax(Point[] p, Dictionary<int, int> diffMax, Dictionary<int, double> VPF)
        {
            p[0].X = 0;
            p[0].Y = (int)((VPF[0] - VPF.Values.Min()) * col / (VPF.Values.Max() - VPF.Values.Min())); // Shift
            for (int y = 1; y < row; ++y)
            {
                p[y].Y = y;
                p[y].X = (int)((VPF[y] - VPF.Values.Min()) * col / (VPF.Values.Max() - VPF.Values.Min())); // Shiht

                diffMax.Add(p[y].Y, p[y].X);
            }
            if (row == 1)
            {
                diffMax.Add(0, p[0].X);
            }
            return diffMax;
        }


        private void VerticalDiffMax(Point[] p, out int CornerROI_Lx, out int CornerROI_Rx, Dictionary<int, double> VPF,int SegementOffset)
        {
            int consecutive =0;
            int longest = 0;
            int tempLx = 0;
            int tempRx = 0;
            CornerROI_Lx = 0;
            CornerROI_Rx = 0;
            for (int x = 0; x < col; ++x)
            {
                p[x].X = x;
                p[x].Y = (int)((VPF[x] - VPF.Values.Min()) * row / (VPF.Values.Max() - VPF.Values.Min())); // Shift

                if ( p[x].Y >= row / 3)
                {
                    if (consecutive == 0)
                    {
                        tempLx = x;
                    }
                    ++consecutive;
                }
                else
                {
                    tempRx = x;
                    if (consecutive >= longest)
                    {
                        longest = consecutive;
                        CornerROI_Lx = tempLx - SegementOffset;
                        CornerROI_Rx = tempRx + SegementOffset;
                        if (CornerROI_Lx < 0) {
                            CornerROI_Lx = 0;
                        }
                        if(CornerROI_Rx > col) {
                            CornerROI_Rx = col -1 ;
                        }
                    }
                    consecutive = 0;
                }

            }
            if (consecutive != 0 && consecutive >= longest)
            {
                CornerROI_Lx = tempLx;
                CornerROI_Rx = col - 1;
            }
        }

        private void HorizontalDiffMax(Point[] p, out int CornerROI_Uy, out int CornerROI_Dy, Dictionary<int, double> VPF,int SegementOffset)
        {
            int consecutive = 0;
            int longest = 0;
            int tempUy = 0;
            int tempDy = 0;
            CornerROI_Uy = 0;
            CornerROI_Dy = 0;

            for (int y = 0; y < row; ++y)
            {
                p[y].Y = y;
                p[y].X = (int)((VPF[y] - VPF.Values.Min()) * col / (VPF.Values.Max() - VPF.Values.Min())); // Shiht

                if (p[y].X >= col *4/ 10)
                {
                    if (consecutive == 0)
                    {
                        tempUy = y;
                    }
                    ++consecutive;
                }
                else
                {
                    tempDy = y;
                    if (consecutive >= longest)
                    {
                        longest = consecutive;
                        CornerROI_Uy = tempUy - SegementOffset;
                        CornerROI_Dy = tempDy + SegementOffset;
                        if (CornerROI_Uy < 0) {
                            CornerROI_Uy = 0;
                        }
                        if (CornerROI_Dy >= row) {
                            CornerROI_Dy = row -1 ;
                        }
                    }
                    consecutive = 0;
                }

            }

            if (consecutive != 0 && consecutive >= longest)
            {
                CornerROI_Uy = tempUy;
                CornerROI_Dy = row - 1;
            }
        }

    }
}

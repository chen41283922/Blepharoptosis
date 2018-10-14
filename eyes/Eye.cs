using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SomeCalibrations
{
    class Eye
    {
        
        PointF Left_endPoint;
        PointF Right_endPoint;
        public Curve above;
        public Curve below;

        Image<Gray, float> sobelX;
        Image<Gray, float> sobelY;
        Matrix<double> S_frst;

        public Eye() { }
        public Eye(Eye copy)
        {
            this.Left_endPoint = copy.Left_endPoint;
            this.Right_endPoint = copy.Right_endPoint;
            this.above = copy.above;
            this.below = copy.below;
        }     
        public Eye(List<List<PointF>> CtrlPoints, PointF CornerL, PointF CornerR,String LorR)
        {
            initpoint(CtrlPoints,CornerL, CornerR, LorR);
        }

        public PointF get_Left_endPoint() { return this.Left_endPoint; }
        public PointF get_Right_endPoint(){  return this.Right_endPoint; }

        private void initpoint(List<List<PointF>> CtrlPoints,PointF CornerL, PointF CornerR, String LorR)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            
            int offset = 2;
            
            if (LorR == "R")// R_eye
            {
                Left_endPoint = new PointF(CornerR.X + rand.Next(-offset, offset), CornerR.Y + rand.Next(-offset, offset));
                Right_endPoint = new PointF(CornerL.X, CornerL.Y); // left corner
            }
            else // LorR == "L" L_eye
            {
                Left_endPoint = new PointF(CornerR.X, CornerR.Y); // right corner
                Right_endPoint = new PointF(CornerL.X + rand.Next(-offset, offset), CornerL.Y + rand.Next(-offset, offset));
            }

            // Upper Eyelid Curve
            List<PointF> points = new List<PointF>(CtrlPoints[0]);
            points.Add(Left_endPoint);
            points.Add(Right_endPoint);
            above = new Curve(points,3);

            // Lower Eyelid Curve
            points = new List<PointF>(CtrlPoints[1]);
            points.Add(Left_endPoint);
            points.Add(Right_endPoint);
            below = new Curve(points,3);
            
        }
        

        public double Gradient(Image<Gray,byte> img)
        {
            if (sobelX == null && sobelY == null) {
                sobelX = img.Sobel(1, 0, 3);
                sobelY = img.Sobel(0, 1, 3);
            }

            double D_grad = 0;
            int counter = 0;
            for (int x = (int)Left_endPoint.X; x < Right_endPoint.X; ++x)
            {
                float y =  (float)above.FY(x);
                float y2 = (float)below.FY(x);
                float dy = (float)above.DifferentialFY(x);
                float dy2 = (float)below.DifferentialFY(x);


                if (y > 0 && y < img.Height)
                {
                    double gx, gy;
                    double theta1;
                    double theta2;
                    double deltheta;
                    double cs2;
                    gx = sobelX[(int)y, x].Intensity;
                    gy = sobelY[(int)y, x].Intensity;
                    theta1 = Math.Atan2(gy, gx);
                    theta2 = Math.Atan(dy);
                    deltheta = (theta1 - theta2);
                    cs2 = Math.Cos(deltheta);
                    cs2 *= cs2;
                    D_grad += cs2;
                    counter++;
                }

                if (y2 > 0 && y2 < img.Height)
                {
                    double gx, gy;
                    double theta1;
                    double theta2;
                    double deltheta;
                    double cs2;
                    gx = sobelX[(int)y2, x].Intensity;
                    gy = sobelY[(int)y2, x].Intensity;
                    theta1 = Math.Atan2(gy, gx);
                    theta2 = Math.Atan(dy2);
                    deltheta = (theta1 - theta2);
                    cs2 = Math.Cos(deltheta);
                    cs2 *= cs2;
                    D_grad += cs2;
                    counter++;
                }


            }
            return (D_grad / counter);
        }
        public void DrawEyelid(ref Image<Bgr, Byte> pic)
        {
            for (int x = ((int)Left_endPoint.X > 0) ? (int)Left_endPoint.X : 0; x < Right_endPoint.X && x < pic.Width; x++)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);
                pic.Draw(
                    new CircleF(new PointF(x*16, y*16), 4), new Bgr(Color.Red), 1,LineType.AntiAlias,4
                 );
                
                pic.Draw(
                    new CircleF(new PointF(x*16, y2*16), 4), new Bgr(Color.Blue), 1, LineType.AntiAlias, 4
                 );
            }
        }


        public void RunFRST(Image<Bgr, byte> img) {
            Image<Gray, byte> grayimg = img.Convert<Gray, byte>();
            Mat input = grayimg.Mat;

            sobelX = grayimg.Sobel(1, 0, 3);
            sobelY = grayimg.Sobel(0, 1, 3);
            int alpha_symm = 1;
            fullfrst(ref input, alpha_symm, 1);
        }
        private void fullfrst(ref Mat inputImage, double alpha, double stdFactor)
        {
            int symm_rad_range = 5;//100D : 66  //indu 20  //hospi 35
            int rad = symm_rad_range;
            int radlit = symm_rad_range + 10;
            Matrix<double> S = new Matrix<double>(1, 1);
            Matrix<double> temp;
            double thisSmax;
            for (; rad < radlit; rad++)
            {
                frstsimple(ref inputImage, out temp, 2 * rad - 1, alpha, stdFactor, out thisSmax);
                if (S.Cols < temp.Cols && S.Rows < temp.Rows)
                {
                    S = temp.Clone();
                }
                else
                {
                    S = S.Add(temp);
                }

            }
            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();
            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            S._Mul((1d / max));

            S_frst = S.Clone();


        }
        private void frstsimple(ref Mat inputImage, out Matrix<double> S, int radii, double alpha, double stdFactor, out double Smax)
        {

            int width = inputImage.Cols;
            int height = inputImage.Rows;
            Matrix<double> S_temp = new Matrix<double>(inputImage.Rows + 2 * radii, inputImage.Cols + 2 * radii, inputImage.NumberOfChannels);
            Matrix<double> O_n = new Matrix<double>(S_temp.Rows, S_temp.Cols, S_temp.NumberOfChannels);
            Matrix<double> M_n = new Matrix<double>(S_temp.Rows, S_temp.Cols, S_temp.NumberOfChannels);



            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    PointF p = new PointF(j, i);

                    double[] g = new double[2];
                    g[0] = sobelX[i, j].Intensity;
                    g[1] = sobelY[i, j].Intensity;

                    double gnorm = Math.Sqrt(g[0] * g[0] + g[1] * g[1]);

                    if (gnorm > 0)
                    {
                        int[] gp = new int[2];
                        gp[0] = (int)Math.Round((g[0] / gnorm) * radii);
                        gp[1] = (int)Math.Round((g[1] / gnorm) * radii);

                        PointF pnve = new PointF(j - gp[0] + radii, i - gp[1] + radii);
                        O_n[(int)pnve.Y, (int)pnve.X] -= 1;
                        M_n[(int)pnve.Y, (int)pnve.X] -= gnorm;

                    }
                }
            }

            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();

            Matrix<double> temp_0 = O_n.CopyBlank();
            Matrix<double> abs_n = O_n.CopyBlank();
            temp_0.SetZero();
            CvInvoke.AbsDiff(O_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            O_n = abs_n.Mul(1d / max);
            CvInvoke.MinMaxLoc(O_n, ref min, ref max, ref minloc, ref maxloc);

            temp_0.SetZero();
            abs_n.SetZero();
            min = max = 0;

            CvInvoke.AbsDiff(M_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            M_n = abs_n.Mul(1d / max);
            CvInvoke.MinMaxLoc(M_n, ref min, ref max, ref minloc, ref maxloc);

            CvInvoke.Pow(O_n, alpha, S_temp);
            S_temp._Mul(M_n);

            int kSize = (int)Math.Ceiling(radii / 2d);
            if (kSize % 2 == 0)
                kSize++;

            CvInvoke.MinMaxLoc(S_temp, ref min, ref max, ref minloc, ref maxloc);

            CvInvoke.GaussianBlur(S_temp, S_temp, new Size(kSize, kSize), radii * stdFactor);
            CvInvoke.MinMaxLoc(S_temp, ref min, ref max, ref minloc, ref maxloc);

            S = new Matrix<double>(S_temp.Rows - 2 * radii, S_temp.Cols - 2 * radii);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    S[i, j] = S_temp[i + radii, j + radii];
                }
            }
            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            Smax = max;
        }

        public Point FindEyeROIbyFRST(int BlockWidth, int BlockHeight, String LorR = "")
        {

            if (S_frst != null)
            {
                Dictionary<Point, double> Candidate = new Dictionary<Point, double>();

                int padX = BlockWidth - S_frst.Width % BlockWidth;
                int padY = BlockHeight - S_frst.Height % BlockHeight;
                int S_frstH = S_frst.Height;
                int S_frstW = S_frst.Width;

                Matrix<double> data = new Matrix<double>(S_frstH + padY, S_frstW + padX, 1);

                if (LorR == "")
                {
                    for (int i = 0; i < S_frstH; ++i)
                    {
                        for (int j = 0; j < S_frstW; ++j)
                        {
                            data[i, j] = S_frst[i, j];
                        }
                    }
                }
                else if (LorR == "R")
                {
                    for (int i = 0; i < S_frstH; ++i)
                    {
                        for (int j = 0; j < S_frstW / 2; ++j)
                        {
                            data[i, j] = S_frst[i, j];
                        }
                    }
                }
                else if (LorR == "L")
                {
                    for (int i = 0; i < S_frstH; ++i)
                    {
                        for (int j = S_frstW / 2; j < S_frstW; ++j)
                        {
                            data[i, j] = S_frst[i, j];
                        }
                    }
                }

                double min = 0, max = 0;
                Point pMin = new Point();
                Point pMax = new Point();

                data.MinMax(out min, out max, out pMin, out pMax);

                return new Point(pMax.X - BlockWidth / 2, pMax.Y - BlockHeight / 2);
            }
            else
            {
                return new Point(0, 0);
            }


        }

    }
    
}

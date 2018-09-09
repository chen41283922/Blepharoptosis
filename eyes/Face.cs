using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using bearing;
using EyeDection;
using SomeCalibrations;

namespace eyes
{
    class Face
    {

        public class MyCV
        {
            public static Bitmap BoundingBoxeyebrow(Image<Gray, Byte> gray, Image<Bgr, byte> draw)
            {
                // 使用 VectorOfVectorOfPoint 類別一次取得多個輪廓。
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    // 在這版本請使用FindContours，早期版本有cvFindContours等等，在這版都無法使用，
                    // 由於這邊是要取得最外層的輪廓，所以第三個參數給 null，第四個參數則用 RetrType.External。
                    CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    Bitmap tempimg = new Bitmap(gray.Bitmap);
                    Bitmap tempcolorimg = new Bitmap(draw.Bitmap);

                    int left = 2147483647, right = 0, top = 2147483647, button = 0;
                    int lefttop = 2147483647, leftdown = 0, righttop = 2147483647, rightdown = 0;
                    int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0, lefttoppoint = 0, leftdownpoint = 0, righttoppoint = 0, rightdownpoint = 0;
                    int count = contours.Size;



                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        {

                            // 使用 BoundingRectangle 取得框選矩形
                            Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                            if ((BoundingBox.Width / BoundingBox.Height) > 1.0 && (BoundingBox.Width * BoundingBox.Height) > 500 && (BoundingBox.Width * BoundingBox.Height) < 7000 && BoundingBox.Y < gray.Height / 2)
                            {




                            }


                        }
                    }



                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        {

                            // 使用 BoundingRectangle 取得框選矩形
                            Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                            if ((BoundingBox.Width / BoundingBox.Height) > 1.5 && (BoundingBox.Width * BoundingBox.Height) > 1000 && (BoundingBox.Width * BoundingBox.Height) < 7000 && BoundingBox.Y < gray.Height / 3)//過濾長寬比太小和面積太小的box
                            //CvInvoke.DrawContours(draw, contours,i, new MCvScalar(255, 0, 255, 255),2);
                            {
                                PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                                PointF[] pts = CvInvoke.ConvexHull(temp, true);
                                Point[] points = new Point[temp.Length];



                                for (int j = 0; j < temp.Length; j++)//找上下左右端點
                                {
                                    points[j] = Point.Round(temp[j]);//PointF2Point


                                    if (j > 1 && points[j].X < left)
                                    {
                                        left = points[j].X;
                                        leftpoint = j;
                                    }

                                    if (j > 1 && points[j].X > right)
                                    {
                                        right = points[j].X;
                                        rightpoint = j;
                                    }
                                    if (j > 1 && points[j].Y < top)
                                    {
                                        top = points[j].Y;
                                        toppoint = j;
                                    }
                                    if (j > 1 && points[j].Y > button)
                                    {
                                        button = points[j].Y;
                                        buttonpoint = j;
                                    }
                                }




                                for (int j = 0; j < temp.Length; j++)
                                {
                                    if (j > 1 && points[j].X == (points[leftpoint].X + points[toppoint].X) / 2 && points[j].Y < lefttop)
                                    {
                                        lefttop = points[j].Y;
                                        lefttoppoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[leftpoint].X + points[buttonpoint].X) / 2 && points[j].Y > leftdown)
                                    {
                                        leftdown = points[j].Y;
                                        leftdownpoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[rightpoint].X + points[toppoint].X) / 2 && points[j].Y < righttop)
                                    {
                                        righttop = points[j].Y;
                                        righttoppoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[rightpoint].X + points[buttonpoint].X) / 2 && points[j].Y > rightdown)
                                    {
                                        rightdown = points[j].Y;
                                        rightdownpoint = j;
                                    }
                                }

                                Point[] pointlefttopright = { points[leftpoint], points[lefttoppoint], points[toppoint], points[righttoppoint], points[rightpoint] };
                                Point[] pointleftbuttonright = { points[leftpoint], points[leftdownpoint], points[buttonpoint], points[rightdownpoint], points[rightpoint] };








                                Point[] subsampling = new Point[points.Length / 15 + 1];
                                for (int j = 0; j < temp.Length; j = j + 15)
                                {
                                    subsampling[j / 15] = points[j];
                                }
                                subsampling[points.Length / 15] = subsampling[0];





                                Graphics g = Graphics.FromImage(tempimg);//畫上曲線
                                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 3);
                                g.DrawCurve(pen, subsampling, 0.3f);
                                g.Dispose();

                                Graphics g1 = Graphics.FromImage(tempimg);//畫下曲線
                                Pen pen1 = new Pen(Color.Yellow, 4);
                                //g1.DrawCurve(pen1, pointleftbuttonright, 0.3f);
                                g1.Dispose();

                                Graphics g6 = Graphics.FromImage(tempcolorimg);//畫上曲線
                                g6.DrawCurve(pen, subsampling, 0.4f);
                                g6.Dispose();

                                Graphics g7 = Graphics.FromImage(tempcolorimg);//畫下曲線
                                //g7.DrawCurve(pen1, pointleftbuttonright, 0.4f);
                                g7.Dispose();

                                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                Graphics g2 = Graphics.FromImage(tempimg);//畫左點
                                SolidBrush drawBrush = new SolidBrush(Color.LightGreen);
                                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                                g2.Dispose();

                                Graphics g3 = Graphics.FromImage(tempimg);//畫右點
                                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                                g3.Dispose();


                                Graphics g4 = Graphics.FromImage(tempcolorimg);//畫左點
                                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                                g4.Dispose();

                                Graphics g5 = Graphics.FromImage(tempcolorimg);//畫右點
                                g5.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                                g5.Dispose();
                            }


                        }
                        left = 2147483647; right = 0; top = 2147483647; button = 0; lefttop = 2147483647; leftdown = 0; righttop = 2147483647; rightdown = 0;
                        leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0; lefttoppoint = 0; leftdownpoint = 0; righttoppoint = 0; rightdownpoint = 0;
                    }

                    gray.Bitmap = tempimg;
                    gray.ROI = Rectangle.Empty;
                    draw.Bitmap = tempcolorimg;
                    draw.ROI = Rectangle.Empty;
                    return tempimg;
                }

            }



            private static PointF Point2PointF(Point P)//Point轉PointF
            {
                PointF PF = new PointF
                {
                    X = P.X,
                    Y = P.Y
                };
                return PF;
            }
        }
        private static PointF Point2PointF(Point P)//Point轉PointF
        {
            PointF PF = new PointF
            {
                X = P.X,
                Y = P.Y
            };
            return PF;
        }

        public struct eyebrowpoint//眉毛點的Struct
        {
            public Point leftleftpoint;//left 4 左眉
            public Point leftrightpoint;
            public Point lefttoppoint;

            public Point rightleftpoint;//right 4 右眉
            public Point rightrightpoint;
            public Point righttoppoint;

            public Point centerpoint;//眉毛中間點

            public Point[] lefteyebrowcontour;
            public Point[] righteyebrowcontour;
        }
        public struct noseshape//鼻子形狀
        {
            
        }
        public struct lipspoint//嘴唇點的Struct
        {
            public Point leftlipspoint;
            public Point rightlipspoint;

        }
        public struct faceupleftrightdownpoint//臉高 臉寬 髮際線
        {
            public Point FaceTopPoint;
            public Point FaceDownPoint;
            public Point FaceLeftPoint;
            public Point FaceRightPoint;
        }
        public static Point NoseCenterPoint(Image<Bgr, byte> My_Image2, Image<Gray, byte> My_Image1, Rectangle[] faces)//鼻子中心點
        {
            Point centernose;

            //鼻子
            Image<Bgr, byte> nose = My_Image2.Clone();
            Image<Gray, byte> nosegray = My_Image1.Clone();
            //nosegray.ROI = faces[0];

            nosegray = nosegray.ThresholdBinaryInv(new Gray(70), new Gray(255));
            Image<Bgr, byte> nosegraytoBgr = nosegray.Convert<Bgr, byte>();
            //faces[0].X+faces[0].Width/2,faces[0].Y+faces[0].Height/2 中心點座標
            Rectangle noserange = new Rectangle(faces[0].X + (faces[0].Width / 5) * 2, faces[0].Y + faces[0].Height / 2, faces[0].Width / 5, faces[0].Height / 7);//取鼻子範圍
            nose.ROI = noserange;
            nosegray.ROI = noserange;
            nosegraytoBgr.ROI = noserange;



            VectorOfVectorOfPoint contoursnose = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(nosegray, contoursnose, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int countnose = contoursnose.Size;


            Rectangle first = Rectangle.Empty, second = Rectangle.Empty;//最大跟第二大物件的座標
            for (int i = 0; i < countnose; i++)//找最大
            {
                using (VectorOfPoint contour = contoursnose[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0) { first = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > first.Width * first.Height) { first = BoundingBox; }
                }
            }

            for (int i = 0; i < countnose; i++)//找第二大
            {
                using (VectorOfPoint contour = contoursnose[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0 && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { }
                    else if (i == 0) { second = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { second = BoundingBox; }
                }
            }

            if (first.X > second.X)//如果最大在右邊 交換
            {
                Rectangle temprect = first;
                first = second;
                second = temprect;
            }
            //鼻中心座標
            centernose = new Point((first.X + first.Width / 2 + second.X + second.Width / 2) / 2 + (faces[0].X + (faces[0].Width / 5) * 2), (first.Y + first.Height / 2 + second.Y + second.Height / 2) / 2 + (faces[0].Y + faces[0].Height / 2));//左右鼻孔位置平均

            return centernose;


        }
        public static Bitmap Temperature(Image<Bgr, Byte> My_Image2)//色溫調整 輸入Image 輸出Bitmap
        {
            Bitmap Source = new Bitmap(My_Image2.Bitmap);
            Bitmap Result = (Bitmap)Source.Clone();
            double xD = 0, yD = 0;
            double CCT_x = 0, CCT_y = 0, CCT_n = 0;
            double Tc = 3999;//這裡改色溫
            double Xiw = 0, Yiw = 0, Ziw = 0;
            double Xtw = 0, Ytw = 0, Ztw = 0;
            double Riw = 0, Giw = 0, Biw = 0, Rtw = 0, Gtw = 0, Btw = 0;

            //計算目標色溫(5800K)與 Rtw、Gtw、Btw
            //if (textBox4.Text != "")
            //    Tc = int.Parse(textBox4.Text);// 9000;// 校正出來色溫比預定值(Tc)還要高約400
            //else
            //    Tc = 0;
            if (Tc >= 1677 && Tc < 4000)
                xD = 0 - (0.2661239 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) - (0.2343580 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.8776956 * Math.Pow(10, 3) / Tc) + 0.179910;
            else if (Tc >= 4000 && Tc <= 25000)
                xD = 0 - (3.0258469 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) + (2.1070379 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.2226347 * Math.Pow(10, 3) / Tc) + 0.24039;

            if (xD <= 0.38405)
                yD = 3.0817580 * Math.Pow(xD, 3) - 5.8733867 * Math.Pow(xD, 2) + 3.75112997 * xD - 0.37001483;
            else if (xD <= 0.50338)
                yD = 0 - 0.9549476 * Math.Pow(xD, 3) - 1.37418593 * Math.Pow(xD, 2) + 2.09137015 * xD - 0.16748867;
            else
                yD = 0 - 1.1063814 * Math.Pow(xD, 3) - 1.34811020 * Math.Pow(xD, 2) + 2.18555832 * xD - 0.20219683;

            Xtw = xD / yD;
            Ytw = yD / yD;
            Ztw = (1 - xD - yD) / yD;

            Rtw = 0.8951 * Xtw / Ytw + 0.2664 * Ytw / Ytw - 0.1614 * Ztw / Ytw;
            Gtw = -0.7502 * Xtw / Ytw + 1.7135 * Ytw / Ytw + 0.0367 * Ztw / Ytw;
            Btw = 0.0389 * Xtw / Ytw - 0.0685 * Ytw / Ytw + 1.0296 * Ztw / Ytw;

            //計算來源(i,j)色溫與 Rtw、Gtw、Btw 並執行 Bradford CAT
            double[] R = new double[Source.Height * Source.Width];
            double[] G = new double[Source.Height * Source.Width];
            double[] B = new double[Source.Height * Source.Width];
            int RTotal = 0, GTotal = 0, BTotal = 0;
            double RAverage = 0, GAverage = 0, BAverage = 0;
            for (int h = 0; h < Source.Height; h++)
            {
                for (int w = 0; w < Source.Width; w++)
                {
                    R[h * Source.Width + w] = (double)Source.GetPixel(w, h).R;
                    RTotal += Source.GetPixel(w, h).R;
                    G[h * Source.Width + w] = (double)Source.GetPixel(w, h).G;
                    GTotal += Source.GetPixel(w, h).G;
                    B[h * Source.Width + w] = (double)Source.GetPixel(w, h).B;
                    BTotal += Source.GetPixel(w, h).B;
                }
            }
            RAverage = RTotal / (Source.Height * Source.Width);
            GAverage = GTotal / (Source.Height * Source.Width);
            BAverage = BTotal / (Source.Height * Source.Width);

            CCT_x = (0.2469 * RAverage + 0.2707 * GAverage + -0.1473 * BAverage) / (0.2558 * RAverage + 0.836 * GAverage + 0.0694 * BAverage);
            CCT_y = (0.1735 * RAverage + 0.3443 * GAverage + -0.1143 * BAverage) / (0.2558 * RAverage + 0.836 * GAverage + 0.0694 * BAverage);
            CCT_n = (CCT_x - 0.3320) / (CCT_y - 0.1858);
            Tc = 3525 * Math.Pow(CCT_n, 2) - 6823.3 * CCT_n - 449 * Math.Pow(CCT_n, 3) + 5520.33;

            //Color Temperature Conversion Coefficients
            xD = 0;
            xD = 0;
            yD = 0;
            if (Tc >= 1677 && Tc < 4000)
                xD = 0 - (0.2661239 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) - (0.2343580 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.8776956 * Math.Pow(10, 3) / Tc) + 0.179910;
            else if (Tc >= 4000 && Tc <= 25000)
                xD = 0 - (3.0258469 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) + (2.1070379 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.2226347 * Math.Pow(10, 3) / Tc) + 0.24039;

            if (xD <= 0.38405)
                yD = 3.0817580 * Math.Pow(xD, 3) - 5.8733867 * Math.Pow(xD, 2) + 3.75112997 * xD - 0.37001483;
            else if (xD <= 0.50338)
                yD = 0 - 0.9549476 * Math.Pow(xD, 3) - 1.37418593 * Math.Pow(xD, 2) + 2.09137015 * xD - 0.16748867;
            else
                yD = 0 - 1.1063814 * Math.Pow(xD, 3) - 1.34811020 * Math.Pow(xD, 2) + 2.18555832 * xD - 0.20219683;

            Xiw = xD / yD;
            Yiw = yD / yD;
            Ziw = (1 - xD - yD) / yD;

            Riw = 0.8951 * Xiw / Yiw + 0.2664 * Yiw / Yiw - 0.1614 * Ziw / Yiw;
            Giw = -0.7502 * Xiw / Yiw + 1.7135 * Yiw / Yiw + 0.0367 * Ziw / Yiw;
            Biw = 0.0389 * Xiw / Yiw - 0.0685 * Yiw / Yiw + 1.0296 * Ziw / Yiw;


            //計算原始色溫 -> Calibration method for Correlated Color Temperature (CCT) measurement using RGB color sensors
            //0.2469  0.2707  -0.1473
            //0.1735  0.3443  -0.1143
            //-0.1646  0.2210   0.3310
            int t = 0;
            double Mc_mul_X = 0, Mc_mul_Y = 0, Mc_mul_Z = 0;
            double Rlinear = 0, Glinear = 0, Blinear = 0;
            double Pixel_x = 0, Pixel_y = 0, Pixel_z = 0;
            double Gamma_R = 0, Gamma_G = 0, Gamma_B = 0;
            
            int Rout = 0, Gout = 0, Bout = 0;
            double[,] MBFDneg_mul_D;
            double[,] D_mul_MBFD;
            double[,] MBFD;
            double[] Clinear;
            double A = 0.055;//gamma校正係數
            Color[] ResuleColor = new Color[Source.Height * Source.Width];
            for (int h = 0; h < Source.Height; h++)
            {
                for (int w = 0; w < Source.Width; w++)
                {
                    t = h * Source.Width + w;
                    CCT_n = 0;
                    Mc_mul_X = 0;
                    Mc_mul_Y = 0;
                    Mc_mul_Z = 0;
                    Rlinear = 0;
                    Glinear = 0;
                    Blinear = 0;

                    //CCT_x = (0.2469 * R[t] + 0.2707 * G[t] + -0.1473 * B[t]) / (0.2558 * R[t] + 0.836 * G[t] + 0.0694 * B[t]);
                    //CCT_y = (0.1735 * R[t] + 0.3443 * G[t] + -0.1143 * B[t]) / (0.2558 * R[t] + 0.836 * G[t] + 0.0694 * B[t]);
                    //CCT_n = (CCT_x - 0.3320) / (CCT_y - 0.1858);
                    //Tc = 3525 * Math.Pow(CCT_n, 2) - 6823.3 * CCT_n - 449 * Math.Pow(CCT_n, 3) + 5520.33 - 600;

                    ////Color Temperature Conversion Coefficients
                    //xD = 0;
                    //yD = 0;
                    //if (Tc >= 1677 && Tc < 4000)
                    //    xD = 0 - (0.2661239 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) - (0.2343580 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.8776956 * Math.Pow(10, 3) / Tc) + 0.179910;
                    //else if (Tc >= 4000 && Tc <= 25000)
                    //    xD = 0 - (3.0258469 * Math.Pow(10, 9) / Math.Pow(Tc, 3)) + (2.1070379 * Math.Pow(10, 6) / Math.Pow(Tc, 2)) + (0.2226347 * Math.Pow(10, 3) / Tc) + 0.24039;

                    //if (xD <= 0.38405)
                    //    yD = 3.0817580 * Math.Pow(xD, 3) - 5.8733867 * Math.Pow(xD, 2) + 3.75112997 * xD - 0.37001483;
                    //else if (xD <= 0.50338)
                    //    yD = 0 - 0.9549476 * Math.Pow(xD, 3) - 1.37418593 * Math.Pow(xD, 2) + 2.09137015 * xD - 0.16748867;
                    //else
                    //    yD = 0 - 1.1063814 * Math.Pow(xD, 3) - 1.34811020 * Math.Pow(xD, 2) + 2.18555832 * xD - 0.20219683;

                    //Xiw = xD / yD;
                    //Yiw = yD / yD;
                    //Ziw = (1 - xD - yD) / yD;

                    Riw = 0.8951 * Xiw / Yiw + 0.2664 * Yiw / Yiw - 0.1614 * Ziw / Yiw;
                    Giw = -0.7502 * Xiw / Yiw + 1.7135 * Yiw / Yiw + 0.0367 * Ziw / Yiw;
                    Biw = 0.0389 * Xiw / Yiw - 0.0685 * Yiw / Yiw + 1.0296 * Ziw / Yiw;


                    MBFDneg_mul_D = new double[3, 3] {{ 0.9870* Rtw / Riw,-0.1471* Gtw / Giw, 0.1600* Btw / Biw},
                                                      { 0.4323* Rtw / Riw, 0.5184* Gtw / Giw, 0.0493* Btw / Biw},
                                                      {-0.0085* Rtw / Riw, 0.0400* Gtw / Giw, 0.9685* Btw / Biw}};

                    MBFD = new double[3, 3]{{ 0.8951, 0.2664, -0.1614},
                                            {-0.7502, 1.7135,  0.0367},
                                            { 0.0389,-0.0685,  1.0296}};

                    D_mul_MBFD = new double[3, 3];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            D_mul_MBFD[i, j] = MBFDneg_mul_D[i, 0] * MBFD[0, j] + MBFDneg_mul_D[i, 1] * MBFD[1, j] + MBFDneg_mul_D[i, 2] * MBFD[2, j];
                        }
                    }

                    if (R[t] / 255 > 0.04045)
                        Gamma_R = Math.Pow(((R[t] / 255 + 0.055) / (1 + 0.055)), 2.4);
                    else
                        Gamma_R = R[t] / 255 / 12.92;

                    if (G[t] / 255 > 0.04045)
                        Gamma_G = Math.Pow(((G[t] / 255 + 0.055) / (1 + 0.055)), 2.4);
                    else
                        Gamma_G = G[t] / 255 / 12.92;

                    if (G[t] / 255 > 0.04045)
                        Gamma_B = Math.Pow(((B[t] / 255 + 0.055) / (1 + 0.055)), 2.4);
                    else
                        Gamma_B = B[t] / 255 / 12.92;

                    Pixel_x = 0.412424 * Gamma_R + 0.357579 * Gamma_G + 0.180464 * Gamma_B;
                    Pixel_y = 0.212656 * Gamma_R + 0.715158 * Gamma_G + 0.072186 * Gamma_B;
                    Pixel_z = 0.019332 * Gamma_R + 0.119193 * Gamma_G + 0.950444 * Gamma_B;

                    Mc_mul_X = D_mul_MBFD[0, 0] * Pixel_x + D_mul_MBFD[0, 1] * Pixel_y + D_mul_MBFD[0, 2] * Pixel_z;
                    Mc_mul_Y = D_mul_MBFD[1, 0] * Pixel_x + D_mul_MBFD[1, 1] * Pixel_y + D_mul_MBFD[1, 2] * Pixel_z;
                    Mc_mul_Z = D_mul_MBFD[2, 0] * Pixel_x + D_mul_MBFD[2, 1] * Pixel_y + D_mul_MBFD[2, 2] * Pixel_z;

                    Rlinear = 3.2410 * Mc_mul_X - 1.5374 * Mc_mul_Y - 0.4986 * Mc_mul_Z;
                    Glinear = 0 - 0.9692 * Mc_mul_X + 1.8760 * Mc_mul_Y + 0.0416 * Mc_mul_Z;
                    Blinear = 0.0556 * Mc_mul_X - 0.2040 * Mc_mul_Y + 1.0570 * Mc_mul_Z;

                    if (Rlinear > 1)
                        Rlinear = 1;
                    else if (Rlinear < 0)
                        Rlinear = 0;

                    if (Glinear > 1)
                        Glinear = 1;
                    else if (Glinear < 0)
                        Glinear = 0;

                    if (Blinear > 1)
                        Blinear = 1;
                    else if (Blinear < 0)
                        Blinear = 0;

                    Clinear = new double[3] { Rlinear, Glinear, Blinear };
                    for (int i = 0; i < 3; i++)
                    {
                        if (Clinear[i] <= 0.00304)
                        {
                            switch (i)
                            {
                                case 0:
                                    Rout = (int)(12.92 * Clinear[i] * 255);
                                    break;
                                case 1:
                                    Gout = (int)(12.92 * Clinear[i] * 255);
                                    break;
                                case 2:
                                    Bout = (int)(12.92 * Clinear[i] * 255);
                                    break;
                            }
                        }
                        else if (Clinear[i] > 0.00304)
                        {
                            switch (i)
                            {
                                case 0:
                                    Rout = (int)(((1 + A) * Math.Pow(Clinear[i], 0.4166) - A) * 255);
                                    break;
                                case 1:
                                    Gout = (int)(((1 + A) * Math.Pow(Clinear[i], 0.4166) - A) * 255);
                                    break;
                                case 2:
                                    Bout = (int)(((1 + A) * Math.Pow(Clinear[i], 0.4166) - A) * 255);
                                    break;
                            }
                        }
                    }
                    Result.SetPixel(w, h, Color.FromArgb(Rout, Gout, Bout));

                }
            }
            return Result;
        }
        public static eyebrowpoint eyebrowfindpoint(Image<Bgr, Byte> My_Image2, Rectangle[] faces)//眉毛點
        {
            eyebrowpoint ebp = new eyebrowpoint();

            Rectangle righteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 40) * 9, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取右眉毛範圍
            Rectangle lefteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 9) * 5, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取左眉毛範圍

            Image<Bgr, byte> righteyebrowuseimg = new Image<Bgr, Byte>(Face.Temperature(My_Image2));
            Image<Bgr, byte> lefteyebrowuseimg = new Image<Bgr, Byte>(Face.Temperature(My_Image2));

            righteyebrowuseimg.ROI = righteyebrowrange;
            lefteyebrowuseimg.ROI = lefteyebrowrange;

            Image<Bgr, byte> righteyebrowuseimgroi = new Image<Bgr, byte>(righteyebrowuseimg.Bitmap);
            Image<Bgr, byte> lefteyebrowuseimgroi = new Image<Bgr, byte>(lefteyebrowuseimg.Bitmap);


            Bitmap righteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//取出眉毛bitmap
            Bitmap lefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            Bitmap setrighteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//Bitmap輸出
            Bitmap setlefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////前處理 嚴格找一次
            int righteyebroww = righteyebrowbitmap.Width;
            int righteyebrowh = righteyebrowbitmap.Height;

            for (int y = 0; y < righteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < righteyebroww; x++)
                {
                    Color color = righteyebrowbitmap.GetPixel(x, y);
                    int Red = color.R;
                    int Gre = color.G;
                    int Bul = color.B;
                    float hue = color.GetHue();
                    float saturation = color.GetSaturation();
                    //int A = color.A;

                    int range = 30;
                    if (Math.Abs(Red - Gre) <= range && Math.Abs(Gre - Bul) <= range && Math.Abs(Red - Bul) <= range && (Red + Gre + Bul) / 3 <= 150)//&& (hue > 60 || hue == 0))
                    {
                        setrighteyebrowbitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { setrighteyebrowbitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }
                }
            }

            int lefteyebroww = lefteyebrowbitmap.Width;
            int lefteyebrowh = lefteyebrowbitmap.Height;

            for (int y = 0; y < lefteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < lefteyebroww; x++)
                {
                    Color color = lefteyebrowbitmap.GetPixel(x, y);
                    int Red = color.R;
                    int Gre = color.G;
                    int Blu = color.B;
                    float hue = color.GetHue();
                    float saturation = color.GetSaturation();
                    //int A = color.A;

                    int range = 30;
                    if (Math.Abs(Red - Gre) <= range && Math.Abs(Gre - Blu) <= range && Math.Abs(Red - Blu) <= range && (Red + Gre + Blu) / 3 <= 150)//&& (hue > 60 || hue == 0))
                    {
                        setlefteyebrowbitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { setlefteyebrowbitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////找生長起點
            int temp = 2147483647;
            Point righteyebrowxy = new Point(0, 0);
            for (int y = 0; y < righteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < righteyebroww; x++)
                {
                    Color color = setrighteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(righteyebroww / 2 - x) + Math.Abs(righteyebrowh / 2 - y)) < temp)
                    {
                        temp = Math.Abs(righteyebroww / 2 - x) + Math.Abs(righteyebrowh / 2 - y);
                        righteyebrowxy.X = x;
                        righteyebrowxy.Y = y;
                    }
                }
            }

            temp = 2147483647;
            Point lefteyebrowxy = new Point(0, 0);
            for (int y = 0; y < lefteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < lefteyebroww; x++)
                {
                    Color color = setlefteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y)) < temp)
                    {
                        temp = Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y);
                        lefteyebrowxy.X = x;
                        lefteyebrowxy.Y = y;
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////生長

            Stack<Point> stack = new Stack<Point>();
            Stack<Color> Colorstack = new Stack<Color>();
            Bitmap image = new Bitmap(righteyebrowbitmap);
            Bitmap image2 = new Bitmap(righteyebroww, righteyebrowh);

            stack.Push(new Point(righteyebrowxy.X, righteyebrowxy.Y));
            Colorstack.Push(image.GetPixel(righteyebrowxy.X, righteyebrowxy.Y));


            // 領域の開始点の色を領域条件とする
            Color colour;


            if (righteyebrowxy.X + 1 < image.Width && righteyebrowxy.X - 1 >= 0 && righteyebrowxy.Y + 1 < image.Height && righteyebrowxy.Y - 1 >= 0)
            {
                int Red = (righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y - 1).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y + 1).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y - 1).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y + 1).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y - 1).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y).R + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y + 1).R) / 9;
                int Gre = (righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y - 1).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y + 1).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y - 1).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y + 1).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y - 1).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y).G + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y + 1).G) / 9;
                int Blu = (righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y - 1).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X - 1, righteyebrowxy.Y + 1).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y - 1).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y + 1).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y - 1).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y).B + righteyebrowbitmap.GetPixel(righteyebrowxy.X + 1, righteyebrowxy.Y + 1).B) / 9;
                colour = Color.FromArgb(Red, Gre, Blu);
            }
            else
            {
                colour = righteyebrowbitmap.GetPixel(righteyebrowxy.X, righteyebrowxy.Y);
            }
            // 開始点をあらかじめStackに収めておく

            while (stack.Count != 0)
            {
                // 注目点を取り出す
                Point p = stack.Pop();

                // 注目点にまだマーカが付いていない場合
                if (image2.GetPixel(p.X, p.Y) != Color.FromArgb(255, 255, 255))
                {

                    // マーカを付ける
                    image2.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255));
                    
                    int range2 = 30;
                    int light = 180;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if ((p.X + 1 < image.Width && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 1, p.Y).G) + Math.Abs(image.GetPixel(p.X + 1, p.Y).G - image.GetPixel(p.X + 1, p.Y).B) + Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 1, p.Y).B))) <= range2 && (image.GetPixel(p.X + 1, p.Y).R + image.GetPixel(p.X + 1, p.Y).G + image.GetPixel(p.X + 1, p.Y).B) / 3 <= light) || (p.X + 2 < image.Width && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X + 2, p.Y).R - image.GetPixel(p.X + 2, p.Y).G) + Math.Abs(image.GetPixel(p.X + 2, p.Y).G - image.GetPixel(p.X + 2, p.Y).B) + Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 2, p.Y).B))) <= range2 && (image.GetPixel(p.X + 2, p.Y).R + image.GetPixel(p.X + 2, p.Y).G + image.GetPixel(p.X + 2, p.Y).B) / 3 <= light) || (p.X + 3 < image.Width && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X + 3, p.Y).R - image.GetPixel(p.X + 3, p.Y).G) + Math.Abs(image.GetPixel(p.X + 3, p.Y).G - image.GetPixel(p.X + 3, p.Y).B) + Math.Abs(image.GetPixel(p.X + 3, p.Y).R - image.GetPixel(p.X + 3, p.Y).B))) <= range2 && (image.GetPixel(p.X + 3, p.Y).R + image.GetPixel(p.X + 3, p.Y).G + image.GetPixel(p.X + 3, p.Y).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if ((p.X - 1 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 1, p.Y).R - image.GetPixel(p.X - 1, p.Y).G) + Math.Abs(image.GetPixel(p.X - 1, p.Y).G - image.GetPixel(p.X - 1, p.Y).B) + Math.Abs(image.GetPixel(p.X - 1, p.Y).R - image.GetPixel(p.X - 1, p.Y).B))) <= range2 && (image.GetPixel(p.X - 1, p.Y).R + image.GetPixel(p.X - 1, p.Y).G + image.GetPixel(p.X - 1, p.Y).B) / 3 <= light) || (p.X - 2 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 2, p.Y).R - image.GetPixel(p.X - 2, p.Y).G) + Math.Abs(image.GetPixel(p.X - 2, p.Y).G - image.GetPixel(p.X - 2, p.Y).B) + Math.Abs(image.GetPixel(p.X - 2, p.Y).R - image.GetPixel(p.X - 2, p.Y).B))) <= range2 && (image.GetPixel(p.X - 2, p.Y).R + image.GetPixel(p.X - 2, p.Y).G + image.GetPixel(p.X - 2, p.Y).B) / 3 <= light) || (p.X - 3 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 3, p.Y).R - image.GetPixel(p.X - 3, p.Y).G) + Math.Abs(image.GetPixel(p.X - 3, p.Y).G - image.GetPixel(p.X - 3, p.Y).B) + Math.Abs(image.GetPixel(p.X - 3, p.Y).R - image.GetPixel(p.X - 3, p.Y).B))) <= range2 && (image.GetPixel(p.X - 3, p.Y).R + image.GetPixel(p.X - 3, p.Y).G + image.GetPixel(p.X - 3, p.Y).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if ((p.Y + 1 < image.Height && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y + 1).R - image.GetPixel(p.X, p.Y + 1).G) + Math.Abs(image.GetPixel(p.X, p.Y + 1).G - image.GetPixel(p.X, p.Y + 1).B) + Math.Abs(image.GetPixel(p.X, p.Y + 1).R - image.GetPixel(p.X, p.Y + 1).B))) <= range2 && (image.GetPixel(p.X, p.Y + 1).R + image.GetPixel(p.X, p.Y + 1).G + image.GetPixel(p.X, p.Y + 1).B) / 3 <= light) || (p.Y + 2 < image.Height && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y + 2).R - image.GetPixel(p.X, p.Y + 2).G) + Math.Abs(image.GetPixel(p.X, p.Y + 2).G - image.GetPixel(p.X, p.Y + 2).B) + Math.Abs(image.GetPixel(p.X, p.Y + 2).R - image.GetPixel(p.X, p.Y + 2).B))) <= range2 && (image.GetPixel(p.X, p.Y + 2).R + image.GetPixel(p.X, p.Y + 2).G + image.GetPixel(p.X, p.Y + 2).B) / 3 <= light) || (p.Y + 3 < image.Height && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y + 3).R - image.GetPixel(p.X, p.Y + 3).G) + Math.Abs(image.GetPixel(p.X, p.Y + 3).G - image.GetPixel(p.X, p.Y + 3).B) + Math.Abs(image.GetPixel(p.X, p.Y + 3).R - image.GetPixel(p.X, p.Y + 3).B))) <= range2 && (image.GetPixel(p.X, p.Y + 3).R + image.GetPixel(p.X, p.Y + 3).G + image.GetPixel(p.X, p.Y + 3).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if ((p.Y - 1 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 1).R - image.GetPixel(p.X, p.Y - 1).G) + Math.Abs(image.GetPixel(p.X, p.Y - 1).G - image.GetPixel(p.X, p.Y - 1).B) + Math.Abs(image.GetPixel(p.X, p.Y - 1).R - image.GetPixel(p.X, p.Y - 1).B))) <= range2 && (image.GetPixel(p.X, p.Y - 1).R + image.GetPixel(p.X, p.Y - 1).G + image.GetPixel(p.X, p.Y - 1).B) / 3 <= light) || (p.Y - 2 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 2).R - image.GetPixel(p.X, p.Y - 2).G) + Math.Abs(image.GetPixel(p.X, p.Y - 2).G - image.GetPixel(p.X, p.Y - 2).B) + Math.Abs(image.GetPixel(p.X, p.Y - 2).R - image.GetPixel(p.X, p.Y - 2).B))) <= range2 && (image.GetPixel(p.X, p.Y - 2).R + image.GetPixel(p.X, p.Y - 2).G + image.GetPixel(p.X, p.Y - 2).B) / 3 <= light) || (p.Y - 3 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 3).R - image.GetPixel(p.X, p.Y - 3).G) + Math.Abs(image.GetPixel(p.X, p.Y - 3).G - image.GetPixel(p.X, p.Y - 3).B) + Math.Abs(image.GetPixel(p.X, p.Y - 3).R - image.GetPixel(p.X, p.Y - 3).B))) <= range2 && (image.GetPixel(p.X, p.Y - 3).R + image.GetPixel(p.X, p.Y - 3).G + image.GetPixel(p.X, p.Y - 3).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////左眉

            Stack<Point> stackleft = new Stack<Point>();
            stackleft.Push(new Point(lefteyebrowxy.X, lefteyebrowxy.Y));
            Bitmap leftimage = new Bitmap(lefteyebrowbitmap);
            Bitmap leftimage2 = new Bitmap(lefteyebroww, lefteyebrowh);
            Color leftcolour;
            // 領域の開始点の色を領域条件とする
            if (lefteyebrowxy.X + 1 < leftimage.Width && lefteyebrowxy.X - 1 >= 0 && lefteyebrowxy.Y + 1 < leftimage.Height && lefteyebrowxy.Y - 1 >= 0)
            {
                int Red = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).R) / 9;
                int Gre = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).G) / 9;
                int Blu = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).B) / 9;
                leftcolour = Color.FromArgb(Red, Gre, Blu);
            }
            else
            {
                leftcolour = lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y);
            }

            // 開始点をあらかじめStackに収めておく

            while (stackleft.Count != 0)
            {
                // 注目点を取り出す
                Point p = stackleft.Pop();

                // 注目点にまだマーカが付いていない場合
                if (leftimage2.GetPixel(p.X, p.Y) != Color.FromArgb(255, 255, 255))
                {

                    // マーカを付ける
                    leftimage2.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255));
                    int range2 = 30;
                    int light = 180;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if ((p.X + 1 < leftimage.Width && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X + 1, p.Y).R - leftimage.GetPixel(p.X + 1, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X + 1, p.Y).G - leftimage.GetPixel(p.X + 1, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X + 1, p.Y).R - leftimage.GetPixel(p.X + 1, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X + 1, p.Y).R + leftimage.GetPixel(p.X + 1, p.Y).G + leftimage.GetPixel(p.X + 1, p.Y).B) / 3 <= light) || (p.X + 2 < leftimage.Width && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X + 2, p.Y).R - leftimage.GetPixel(p.X + 2, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X + 2, p.Y).G - leftimage.GetPixel(p.X + 2, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X + 1, p.Y).R - leftimage.GetPixel(p.X + 2, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X + 2, p.Y).R + leftimage.GetPixel(p.X + 2, p.Y).G + leftimage.GetPixel(p.X + 2, p.Y).B) / 3 <= light) || (p.X + 3 < leftimage.Width && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X + 3, p.Y).R - leftimage.GetPixel(p.X + 3, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X + 3, p.Y).G - leftimage.GetPixel(p.X + 3, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X + 3, p.Y).R - leftimage.GetPixel(p.X + 3, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X + 3, p.Y).R + leftimage.GetPixel(p.X + 3, p.Y).G + leftimage.GetPixel(p.X + 3, p.Y).B) / 3 <= light))
                    {
                        stackleft.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if ((p.X - 1 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X - 1, p.Y).R - leftimage.GetPixel(p.X - 1, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X - 1, p.Y).G - leftimage.GetPixel(p.X - 1, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X - 1, p.Y).R - leftimage.GetPixel(p.X - 1, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X - 1, p.Y).R + leftimage.GetPixel(p.X - 1, p.Y).G + leftimage.GetPixel(p.X - 1, p.Y).B) / 3 <= light) || (p.X - 2 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X - 2, p.Y).R - leftimage.GetPixel(p.X - 2, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X - 2, p.Y).G - leftimage.GetPixel(p.X - 2, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X - 2, p.Y).R - leftimage.GetPixel(p.X - 2, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X - 2, p.Y).R + leftimage.GetPixel(p.X - 2, p.Y).G + leftimage.GetPixel(p.X - 2, p.Y).B) / 3 <= light) || (p.X - 3 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X - 3, p.Y).R - leftimage.GetPixel(p.X - 3, p.Y).G) + Math.Abs(leftimage.GetPixel(p.X - 3, p.Y).G - leftimage.GetPixel(p.X - 3, p.Y).B) + Math.Abs(leftimage.GetPixel(p.X - 3, p.Y).R - leftimage.GetPixel(p.X - 3, p.Y).B))) <= range2 && (leftimage.GetPixel(p.X - 3, p.Y).R + leftimage.GetPixel(p.X - 3, p.Y).G + leftimage.GetPixel(p.X - 3, p.Y).B) / 3 <= light))
                    {
                        stackleft.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if ((p.Y + 1 < leftimage.Height && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y + 1).R - leftimage.GetPixel(p.X, p.Y + 1).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 1).G - leftimage.GetPixel(p.X, p.Y + 1).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 1).R - leftimage.GetPixel(p.X, p.Y + 1).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y + 1).R + leftimage.GetPixel(p.X, p.Y + 1).G + leftimage.GetPixel(p.X, p.Y + 1).B) / 3 <= light) || (p.Y + 2 < leftimage.Height && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y + 2).R - leftimage.GetPixel(p.X, p.Y + 2).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 2).G - leftimage.GetPixel(p.X, p.Y + 2).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 2).R - leftimage.GetPixel(p.X, p.Y + 2).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y + 2).R + leftimage.GetPixel(p.X, p.Y + 2).G + leftimage.GetPixel(p.X, p.Y + 2).B) / 3 <= light) || (p.Y + 3 < leftimage.Height && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y + 3).R - leftimage.GetPixel(p.X, p.Y + 3).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 3).G - leftimage.GetPixel(p.X, p.Y + 3).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y + 3).R - leftimage.GetPixel(p.X, p.Y + 3).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y + 3).R + leftimage.GetPixel(p.X, p.Y + 3).G + leftimage.GetPixel(p.X, p.Y + 3).B) / 3 <= light))
                    {
                        stackleft.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if ((p.Y - 1 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y - 1).R - leftimage.GetPixel(p.X, p.Y - 1).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 1).G - leftimage.GetPixel(p.X, p.Y - 1).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 1).R - leftimage.GetPixel(p.X, p.Y - 1).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y - 1).R + leftimage.GetPixel(p.X, p.Y - 1).G + leftimage.GetPixel(p.X, p.Y - 1).B) / 3 <= light) || (p.Y - 2 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y - 2).R - leftimage.GetPixel(p.X, p.Y - 2).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 2).G - leftimage.GetPixel(p.X, p.Y - 2).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 2).R - leftimage.GetPixel(p.X, p.Y - 2).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y - 2).R + leftimage.GetPixel(p.X, p.Y - 2).G + leftimage.GetPixel(p.X, p.Y - 2).B) / 3 <= light) || (p.Y - 3 >= 0 && Math.Abs((Math.Abs(leftcolour.R - leftcolour.G) + Math.Abs(leftcolour.G - leftcolour.B) + Math.Abs(leftcolour.R - leftcolour.B)) - (Math.Abs(leftimage.GetPixel(p.X, p.Y - 3).R - leftimage.GetPixel(p.X, p.Y - 3).G) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 3).G - leftimage.GetPixel(p.X, p.Y - 3).B) + Math.Abs(leftimage.GetPixel(p.X, p.Y - 3).R - leftimage.GetPixel(p.X, p.Y - 3).B))) <= range2 && (leftimage.GetPixel(p.X, p.Y - 3).R + leftimage.GetPixel(p.X, p.Y - 3).G + leftimage.GetPixel(p.X, p.Y - 3).B) / 3 <= light))
                    {
                        stackleft.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing


            /////////////////////////////////////////////////////////////////////////////////////////////////find contour
            Image<Gray, byte> righteyebrowcont = new Image<Gray, byte>(image2);//右眼生長後圖
            Image<Gray, byte> lefteyebrowcont = new Image<Gray, byte>(leftimage2);//左眼生長後圖
            VectorOfVectorOfPoint contoursright = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contoursleft = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(righteyebrowcont, contoursright, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            CvInvoke.FindContours(lefteyebrowcont, contoursleft, null, RetrType.External, ChainApproxMethod.ChainApproxNone);


            Rectangle Boundboxrighteyebrow = CvInvoke.BoundingRectangle(contoursright[0]);
            Rectangle Boundboxlefteyebrow = CvInvoke.BoundingRectangle(contoursleft[0]);
            Point eyebrowcenterpoint = Point.Empty;
            eyebrowcenterpoint = new Point(((Boundboxrighteyebrow.X + righteyebrowrange.X + Boundboxrighteyebrow.Width / 2) + (Boundboxlefteyebrow.X + lefteyebrowrange.X + Boundboxlefteyebrow.Width / 2)) / 2, ((Boundboxrighteyebrow.Y + righteyebrowrange.Y + Boundboxrighteyebrow.Height / 2) + (Boundboxlefteyebrow.Y + lefteyebrowrange.Y + Boundboxlefteyebrow.Height / 2)) / 2);
            ebp.centerpoint = eyebrowcenterpoint;

            int righteyebrowleft = 2147483647, righteyebrowright = 0, righteyebrowtop = 2147483647, righteyebrowbutton = 0;
            int righteyebrowleftpoint = 0, righteyebrowrightpoint = 0, righteyebrowtoppoint = 0, righteyebrowbuttonpoint = 0;

            using (VectorOfPoint contour = contoursright[0])
            {

                PointF[] righteyebrowtemp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                PointF[] pts = CvInvoke.ConvexHull(righteyebrowtemp, true);
                Point[] points = new Point[righteyebrowtemp.Length];

                for (int j = 0; j < righteyebrowtemp.Length; j++)//找上下左右端點
                {
                    points[j] = Point.Round(righteyebrowtemp[j]);//PointF2Point


                    if (j > 1 && points[j].X < righteyebrowleft)
                    {
                        righteyebrowleft = points[j].X;
                        righteyebrowleftpoint = j;
                    }

                    if (j > 1 && points[j].X > righteyebrowright)
                    {
                        righteyebrowright = points[j].X;
                        righteyebrowrightpoint = j;
                    }
                    if (j > 1 && points[j].Y < righteyebrowtop)
                    {
                        righteyebrowtop = points[j].Y;
                        righteyebrowtoppoint = j;
                    }
                    if (j > 1 && points[j].Y > righteyebrowbutton)
                    {
                        righteyebrowbutton = points[j].Y;
                        righteyebrowbuttonpoint = j;
                    }
                }


                Point[] subsampling = new Point[points.Length / 15 + 1];
                for (int j = 0; j < righteyebrowtemp.Length; j = j + 15)
                {
                    subsampling[j / 15] = points[j];
                }
                subsampling[points.Length / 15] = subsampling[0];

                ebp.righteyebrowcontour = subsampling;

                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                SolidBrush drawBrush = new SolidBrush(Color.Green);
                Graphics g = Graphics.FromImage(righteyebrowbitmap);//畫曲線
                if (points.Length > 1)
                    g.DrawCurve(pen, subsampling, 0.3f);
                g.Dispose();



                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;




                Graphics g4 = Graphics.FromImage(righteyebrowbitmap);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowleftpoint].X, points[righteyebrowleftpoint].Y, sf);//畫左點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowrightpoint].X, points[righteyebrowrightpoint].Y, sf);//畫右點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowtoppoint].X, points[righteyebrowtoppoint].Y, sf);//畫上點
                g4.Dispose();

                ebp.rightleftpoint = new Point(points[righteyebrowleftpoint].X, points[righteyebrowleftpoint].Y);
                ebp.rightrightpoint = new Point(points[righteyebrowrightpoint].X, points[righteyebrowrightpoint].Y);
                ebp.righttoppoint = new Point(points[righteyebrowtoppoint].X, points[righteyebrowtoppoint].Y);

            }
            righteyebrowleft = 2147483647; righteyebrowright = 0; righteyebrowtop = 2147483647; righteyebrowbutton = 0;
            righteyebrowleftpoint = 0; righteyebrowrightpoint = 0; righteyebrowtoppoint = 0; righteyebrowbuttonpoint = 0;


            int lefteyebrowleft = 2147483647, lefteyebrowright = 0, lefteyebrowtop = 2147483647, lefteyebrowbutton = 0;
            int lefteyebrowleftpoint = 0, lefteyebrowrightpoint = 0, lefteyebrowtoppoint = 0, lefteyebrowbuttonpoint = 0;

            using (VectorOfPoint contour = contoursleft[0])
            {

                PointF[] lefteyebrowtemp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                PointF[] pts = CvInvoke.ConvexHull(lefteyebrowtemp, true);
                Point[] points = new Point[lefteyebrowtemp.Length];

                for (int j = 0; j < lefteyebrowtemp.Length; j++)//找上下左右端點
                {
                    points[j] = Point.Round(lefteyebrowtemp[j]);//PointF2Point


                    if (j > 1 && points[j].X < lefteyebrowleft)
                    {
                        lefteyebrowleft = points[j].X;
                        lefteyebrowleftpoint = j;
                    }

                    if (j > 1 && points[j].X > lefteyebrowright)
                    {
                        lefteyebrowright = points[j].X;
                        lefteyebrowrightpoint = j;
                    }
                    if (j > 1 && points[j].Y < lefteyebrowtop)
                    {
                        lefteyebrowtop = points[j].Y;
                        lefteyebrowtoppoint = j;
                    }
                    if (j > 1 && points[j].Y > lefteyebrowbutton)
                    {
                        lefteyebrowbutton = points[j].Y;
                        lefteyebrowbuttonpoint = j;
                    }
                }


                Point[] subsampling = new Point[points.Length / 15 + 1];
                for (int j = 0; j < lefteyebrowtemp.Length; j = j + 15)
                {
                    subsampling[j / 15] = points[j];
                }
                subsampling[points.Length / 15] = subsampling[0];

                ebp.lefteyebrowcontour = subsampling;

                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                SolidBrush drawBrush = new SolidBrush(Color.Green);
                Graphics g = Graphics.FromImage(lefteyebrowbitmap);//畫曲線
                if (points.Length > 1)
                    g.DrawCurve(pen, subsampling, 0.3f);
                g.Dispose();



                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;




                Graphics g4 = Graphics.FromImage(lefteyebrowbitmap);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowleftpoint].X, points[lefteyebrowleftpoint].Y, sf);//畫左點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowrightpoint].X, points[lefteyebrowrightpoint].Y, sf);//畫右點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowtoppoint].X, points[lefteyebrowtoppoint].Y, sf);//畫上點
                g4.Dispose();

                ebp.leftleftpoint = new Point(points[lefteyebrowleftpoint].X, points[lefteyebrowleftpoint].Y);
                ebp.leftrightpoint = new Point(points[lefteyebrowrightpoint].X, points[lefteyebrowrightpoint].Y);
                ebp.lefttoppoint = new Point(points[lefteyebrowtoppoint].X, points[lefteyebrowtoppoint].Y);

            }


            return ebp;
        }
        public static lipspoint lip(Image<Bgr, Byte> My_Image2, Point centernose, Rectangle[] faces)//嘴唇
        {
            Image<Bgr, Byte> lips = My_Image2.Clone();
            //lips = lips.SmoothGaussian(11);
            Rectangle liprange = new Rectangle(centernose.X - 75, centernose.Y + 20, 150, faces[0].Height / 5);//用鼻中心取嘴唇範圍
            lips.ROI = liprange;

            Bitmap lipsHSV = new Bitmap(lips.Bitmap);

            int w_hsv = lipsHSV.Width;
            int h_hsv = lipsHSV.Height;

            Bitmap hsvlipsbinary = new Bitmap(w_hsv, h_hsv);

            for (int y = 0; y < h_hsv; y++)
            {
                for (int x = 0; x < w_hsv; x++)
                {
                    Color color = lipsHSV.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;

                    if (color.GetHue() <= 15 || color.GetHue() >= 334)
                    {
                        hsvlipsbinary.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { hsvlipsbinary.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }
                }
            }

            Image<Gray, byte> LipshsvforFindContours = new Image<Gray, byte>(hsvlipsbinary);

            VectorOfVectorOfPoint lipshsvVVP = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(LipshsvforFindContours, lipshsvVVP, null, RetrType.External, ChainApproxMethod.ChainApproxNone);

            Rectangle biggestliphsv = Rectangle.Empty;
            int biggestcontourliphsv = 0;
            for (int i = 0; i < lipshsvVVP.Size; i++)//找最大
            {
                using (VectorOfPoint contourbig = lipshsvVVP[i])
                {

                    Rectangle BoundingBoxlip = CvInvoke.BoundingRectangle(contourbig);
                    if (i == 0) { biggestliphsv = BoundingBoxlip; }
                    else if (BoundingBoxlip.Width * BoundingBoxlip.Height > biggestliphsv.Width * biggestliphsv.Height) { biggestliphsv = BoundingBoxlip; biggestcontourliphsv = i; }
                }
            }




            Bitmap lipbitmap = new Bitmap(lips.Bitmap);

            int w = lipbitmap.Width;
            int h = lipbitmap.Height;
            Bitmap imagelip = new Bitmap(lipbitmap);
            Bitmap imagelip2 = new Bitmap(w, h);

            ///////////////////////////////////////////////////找鼻中心往下最黑的點
            int darkpoint = 2147483647;
            Point dark = new Point(0, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = lipbitmap.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;

                    if (R + G + B < darkpoint && x == w / 2 && y > biggestliphsv.Top && y < biggestliphsv.Bottom)
                    {
                        dark = new Point(x, y);
                        darkpoint = R + G + B;
                    }
                }
            }




            Stack<Point> stacklip = new Stack<Point>();
            stacklip.Push(new Point(dark.X, dark.Y));


            // 領域の開始点の色を領域条件とする
            Color colourlip = lipbitmap.GetPixel(dark.X, dark.Y);

            // 開始点をあらかじめStackに収めておく

            while (stacklip.Count != 0)
            {
                // 注目点を取り出す
                Point p = stacklip.Pop();

                // 注目点にまだマーカが付いていない場合
                if (imagelip2.GetPixel(p.X, p.Y) != Color.FromArgb(255, 255, 255))
                {

                    // マーカを付ける
                    imagelip2.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255));
                    int range = 60;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < imagelip.Width && Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).R - colourlip.R) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).G - colourlip.G) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).B - colourlip.B) < range)
                    {
                        stacklip.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).R - colourlip.R) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).G - colourlip.G) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).B - colourlip.B) < range)
                    {
                        stacklip.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < imagelip.Height && Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).R - colourlip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).G - colourlip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).B - colourlip.B) < range)
                    {
                        stacklip.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).R - colourlip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).G - colourlip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).B - colourlip.B) < range)
                    {
                        stacklip.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing

            int lipleft = 2147483647, lipright = 0;
            int lipleftpoint1 = 0, liprightpoint1 = 0;

            Image<Gray, byte> lipsline = new Image<Gray, byte>(imagelip2);//找嘴巴線的輪廓
            VectorOfVectorOfPoint contourslipsline = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(lipsline, contourslipsline, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int countlipline = contourslipsline.Size;

            Rectangle biggest = Rectangle.Empty;
            int biggestcontour = 0;
            for (int i = 0; i < countlipline; i++)//找最大
            {
                using (VectorOfPoint contourbig = contourslipsline[i])
                {

                    Rectangle BoundingBoxbig = CvInvoke.BoundingRectangle(contourbig);
                    if (i == 0) { biggest = BoundingBoxbig; }
                    else if (BoundingBoxbig.Width * BoundingBoxbig.Height > biggest.Width * biggest.Height) { biggest = BoundingBoxbig; biggestcontour = i; }
                }
            }

            VectorOfPoint contourlip = contourslipsline[biggestcontour];

            PointF[] temp1 = Array.ConvertAll(contourlip.ToArray(), new Converter<Point, PointF>(Point2PointF));
            PointF[] pts1 = CvInvoke.ConvexHull(temp1, true);
            Point[] points1 = new Point[temp1.Length];

            if (contourlip != null)
            {
                for (int j = 0; j < temp1.Length; j++)//找左右端點
                {
                    points1[j] = Point.Round(temp1[j]);//PointF2Point


                    if (j > 1 && points1[j].X < lipleft)
                    {
                        lipleft = points1[j].X;
                        lipleftpoint1 = j;
                    }

                    if (j > 1 && points1[j].X > lipright)
                    {
                        lipright = points1[j].X;
                        liprightpoint1 = j;
                    }
                }

                StringFormat sf1 = new StringFormat();//設定string置中，drawString才不會錯位
                sf1.Alignment = StringAlignment.Center;
                sf1.LineAlignment = StringAlignment.Center;

                Graphics gg = Graphics.FromImage(imagelip2);
                SolidBrush drawBrush1 = new SolidBrush(Color.Green);
                Pen pengreen = new Pen(Color.Green, 3);
                gg.DrawString("+", new Font("Arial", 25), drawBrush1, points1[lipleftpoint1].X, points1[lipleftpoint1].Y, sf1);//畫左點
                gg.DrawString("+", new Font("Arial", 25), drawBrush1, points1[liprightpoint1].X, points1[liprightpoint1].Y, sf1);//畫右點
                gg.Dispose();
            }




            Point lipsleftpoint = new Point(points1[lipleftpoint1].X, points1[lipleftpoint1].Y);
            Point lipsrightpoint = new Point(points1[liprightpoint1].X, points1[liprightpoint1].Y);

            Point lipsleftpointori = new Point(points1[lipleftpoint1].X + liprange.X, points1[lipleftpoint1].Y + liprange.Y);
            Point lipsrightpointori = new Point(points1[liprightpoint1].X + liprange.X, points1[liprightpoint1].Y + liprange.Y);

            lipleft = 2147483647; lipright = 0;
            lipleftpoint1 = 0; liprightpoint1 = 0;


            Point[] lipscurve = { new Point(0, lipsleftpoint.Y), lipsleftpoint, dark, lipsrightpoint, new Point(w, lipsrightpoint.Y) };

            Image<Bgr, byte> smooth = new Image<Bgr, byte>(lipbitmap);
            smooth = smooth.SmoothGaussian(11);
            lipbitmap = smooth.Bitmap;

            Graphics gate = Graphics.FromImage(lipbitmap);//畫線區隔上下唇
            Pen Penred = new Pen(Color.White, 1);
            gate.DrawCurve(Penred, lipscurve);
            gate.Dispose();





            Stack<Point> stackuplip = new Stack<Point>();
            stackuplip.Push(new Point(dark.X, dark.Y - 5));


            // 領域の開始点の色を領域条件とする
            Color colouruplip = lipbitmap.GetPixel(dark.X, dark.Y - 5);
            Bitmap imageuplip = new Bitmap(w, h);
            imagelip = new Bitmap(lipbitmap);
            // 開始点をあらかじめStackに収めておく

            while (stackuplip.Count != 0)
            {
                // 注目点を取り出す
                Point p = stackuplip.Pop();

                // 注目点にまだマーカが付いていない場合
                if (imageuplip.GetPixel(p.X, p.Y) != Color.FromArgb(255, 0, 0))
                {

                    // マーカを付ける
                    imageuplip.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, 0));
                    int range = 200; int huerange = 10;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < imagelip.Width && Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X + 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X + 1, p.Y).GetHue() >= 300))
                    {
                        stackuplip.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X - 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X - 1, p.Y).GetHue() >= 300))
                    {
                        stackuplip.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < imagelip.Height && Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y + 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y + 1).GetHue() >= 300))
                    {
                        stackuplip.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y - 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y - 1).GetHue() >= 300))
                    {
                        stackuplip.Push(new Point(p.X, p.Y - 1));
                    }


                }
            }//Region growing



            Stack<Point> stackdownlip = new Stack<Point>();
            stackdownlip.Push(new Point(dark.X, dark.Y ));


            // 領域の開始点の色を領域条件とする
            Color colourdownlip = lipbitmap.GetPixel(dark.X, dark.Y );
            imagelip = new Bitmap(lipbitmap);
            // 開始点をあらかじめStackに収めておく

            while (stackdownlip.Count != 0)
            {
                // 注目点を取り出す
                Point p = stackdownlip.Pop();

                // 注目点にまだマーカが付いていない場合
                if (imageuplip.GetPixel(p.X, p.Y) != Color.FromArgb(0, 255, 0))
                {

                    // マーカを付ける
                    imageuplip.SetPixel(p.X, p.Y, Color.FromArgb(0, 255, 0));
                    int range = 200; int huerange = 10;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < imagelip.Width && Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).R - colourdownlip.R) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).G - colourdownlip.G) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).B - colourdownlip.B) < range && (imagelip.GetPixel(p.X + 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X + 1, p.Y).GetHue() >= 300))
                    {
                        stackdownlip.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).R - colourdownlip.R) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).G - colourdownlip.G) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).B - colourdownlip.B) < range && (imagelip.GetPixel(p.X - 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X - 1, p.Y).GetHue() >= 300))
                    {
                        stackdownlip.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < imagelip.Height && Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).R - colourdownlip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).G - colourdownlip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).B - colourdownlip.B) < range && (imagelip.GetPixel(p.X, p.Y + 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y + 1).GetHue() >= 300))
                    {
                        stackdownlip.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).R - colourdownlip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).G - colourdownlip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).B - colourdownlip.B) < range && (imagelip.GetPixel(p.X, p.Y - 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y - 1).GetHue() >= 300))
                    {
                        stackdownlip.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing





            Graphics g = Graphics.FromImage(lipbitmap);
            SolidBrush drawBrushlip = new SolidBrush(Color.Red);
            SolidBrush drawBrushgreen = new SolidBrush(Color.Green);
            StringFormat sflip = new StringFormat();//設定string置中，drawString才不會錯位
            sflip.Alignment = StringAlignment.Center;
            sflip.LineAlignment = StringAlignment.Center;

            g.DrawString("+", new Font("Arial", 25), drawBrushlip, dark.X, dark.Y, sflip);
            g.DrawString("+", new Font("Arial", 25), drawBrushgreen, lipsleftpoint.X, lipsleftpoint.Y, sflip);
            g.DrawString("+", new Font("Arial", 25), drawBrushgreen, lipsrightpoint.X, lipsrightpoint.Y, sflip);
            g.DrawCurve(Penred, lipscurve);
            g.Dispose();

            lips.Bitmap = lipbitmap;




            lips.ROI = Rectangle.Empty;
            //imageBox1.Image = lips;

            //imageBox2.Image = new Image<Bgr, byte>(imageuplip);






            //for (int y = liprange.Y; y < liprange.Bottom; y++)//畫嘴唇
            //{
            //    for (int x = liprange.X; x < liprange.Right; x++)
            //    {
            //        Color color = imageuplip.GetPixel(x - liprange.X, y - liprange.Y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;

            //        if (color.R != 0 || color.G != 0 || color.B != 0)
            //        {
            //            tempcolorimg.SetPixel(x, y, color);
            //        }
            //    }
            //}
            lipspoint lp = new lipspoint();
            lp.leftlipspoint = new Point(liprange.X + lipsleftpoint.X, liprange.Y + lipsleftpoint.Y);
            lp.rightlipspoint = new Point(liprange.X + lipsrightpoint.X, liprange.Y + lipsrightpoint.Y);
            return lp;
        }
        public static faceupleftrightdownpoint faceulrdpoint(Image<Bgr, byte> My_Image2, Image<Gray, byte> My_Image1, Rectangle[] faces, Point centernose, Image<Bgr, Byte> facecutori, Image<Gray, Byte> facecutorigray)//臉的寬高
        {
            Image<Bgr, byte> hair = My_Image2.Clone();
            Image<Gray, byte> hairgray = new Image<Gray, byte>(My_Image1.Bitmap);
            //hairgray.ROI = facezoom;
            hairgray = hairgray.ThresholdBinaryInv(new Gray(80), new Gray(255));

            Bitmap imagetophair = new Bitmap(hairgray.Bitmap);

            int w = hairgray.Bitmap.Width;
            int h = hairgray.Bitmap.Height;
            int K = 0;

            Color colortophair = imagetophair.GetPixel(centernose.X, faces[0].Y + faces[0].Height / 3);

            while (colortophair.R == 0)//從鼻中心點往上找
            {
                colortophair = imagetophair.GetPixel(centernose.X, faces[0].Y + faces[0].Height / 3 - K);
                K++;
            }

            Point facetop = new Point(centernose.X, faces[0].Y + faces[0].Height / 3 - K);//髮際點

            Point facebutton, faceright, faceleft;
            //for (int y = 0; y < h; y++)//find skin RGB
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;

            //        if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

            //    }
            //}
            Image<Gray, byte> facewhgray = new Image<Gray, byte>(My_Image1.Bitmap);

            int w1 = My_Image2.Bitmap.Width;
            int h1 = My_Image2.Bitmap.Height;
            Bitmap image11 = new Bitmap(facecutori.Bitmap);
            Bitmap skin1 = new Bitmap(facecutori.Bitmap);
            for (int y = 0; y < h1; y++)//find skin  YCbCr
            {
                for (int x = 0; x < w1; x++)
                {
                    Color color = image11.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
                    double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
                    double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
                    if (Cb > 85 && Cb < 135 && Cr > 135 && Cr < 180 && Y > 80)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
                    {
                        skin1.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { skin1.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

                }
            }


            Image<Gray, byte> skinimage = new Image<Gray, byte>(skin1);
            skinimage = skinimage.Dilate(2);
            skinimage = skinimage.Erode(2);
            Image<Bgr, byte> skincolorimage = new Image<Bgr, byte>(skinimage.Bitmap);


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(skinimage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Bitmap tempimg = new Bitmap(skinimage.Bitmap);
            //Bitmap tempcolorimg = new Bitmap(facewh.Bitmap);
            Bitmap tempeyebrowimg = new Bitmap(MyCV.BoundingBoxeyebrow(facecutorigray, facecutori));

            int left = 2147483647, right = 0, top = 2147483647, button = 0;
            int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0;
            int count = contours.Size;



            int prearea = 0, maxarea = 0;
            for (int i = 0; i < count; i++)
            {
                using (VectorOfPoint contour = contours[i])//找最大面積
                {
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    int area = BoundingBox.Width * BoundingBox.Height;
                    if (area > prearea)
                    {
                        maxarea = i;
                        prearea = area;
                    }

                }
            }
            
            using (VectorOfPoint contour = contours[maxarea])//找端點畫+號
            {

                // 使用 BoundingRectangle 取得框選矩形
                Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);

                PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                PointF[] pts = CvInvoke.ConvexHull(temp, true);
                Point[] points = new Point[temp.Length];


                for (int j = 0; j < temp.Length; j++)//找上下左右端點
                {
                    points[j] = Point.Round(temp[j]);//PointF2Point


                    if (j > 1 && points[j].X < left && points[j].Y == centernose.Y - 50)
                    {
                        left = points[j].X;
                        leftpoint = j;
                    }

                    if (j > 1 && points[j].X > right && points[j].Y == centernose.Y - 50)
                    {
                        right = points[j].X;
                        rightpoint = j;
                    }
                    if (j > 1 && points[j].Y < top && points[j].X == centernose.X)
                    {
                        top = points[j].Y;
                        toppoint = j;
                    }
                    if (j > 1 && points[j].Y > button && points[j].X == centernose.X)
                    {
                        button = points[j].Y;
                        buttonpoint = j;
                    }
                }

                facebutton = new Point(points[buttonpoint].X, points[buttonpoint].Y);//下巴點
                faceright = new Point(points[rightpoint].X, points[rightpoint].Y);//臉右點
                faceleft = new Point(points[leftpoint].X, points[leftpoint].Y);//臉左點


            }
            faceupleftrightdownpoint faceulrd = new faceupleftrightdownpoint();
            faceulrd.FaceDownPoint = facebutton;
            faceulrd.FaceLeftPoint = faceleft;
            faceulrd.FaceRightPoint = faceright;
            faceulrd.FaceTopPoint = facetop;
            return faceulrd;
        }
        public static noseshape Noseshape(Image<Bgr, byte> My_Image2)
        {




            noseshape nshape;
            return nshape;
        }

    }
}

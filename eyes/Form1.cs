using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bearing;
using EyeDection;
using SomeCalibrations;
using eyes;
using iTextSharp.text.pdf;
using System.IO;

namespace eyes
{
    public partial class Form1 : Form
    {
        

        public Image<Gray, Byte> My_Image1;
        public Image<Bgr, Byte> My_Image2;
        Image<Bgr, Byte> Ori_Image;

        public Image<Bgr, Byte> facecut;
        public Image<Bgr, Byte> facecutori;
        
        public Image<Gray, Byte> facecutorigray;

        
        public Bitmap camera;
        
        /// ////////////////////////////柏洧
        List<CircleF> fortestcyl = new List<CircleF>();
        List<CircleF> fortestcyr = new List<CircleF>();
        Dictionary<CircleF, double> lvalver = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> rvalver = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> lvalbla = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> rvalbla = new Dictionary<CircleF, double>();
        List<Image<Bgr, Byte>> forproshow = new List<Image<Bgr, Byte>>();
        List<string> leyeinfo = new List<string>();
        List<string> reyeinfo = new List<string>();

        public Form1()
        {
            InitializeComponent();

            float sel_w = Screen.PrimaryScreen.Bounds.Width;
            float sel_h = Screen.PrimaryScreen.Bounds.Height;
            foreach (Control c in this.Controls)
            {
                //c.Scale(sel_w / 1920, sel_h / 1080);
                c.Scale(new SizeF(sel_w / 1920, sel_h / 1080));
                Single currentSize = c.Font.Size * (sel_h / 1080);
                c.Font = new Font("微軟正黑體", currentSize);
            }

            //foreach (Control c1 in panel2.Controls)
            //{
            //    //c.Scale(sel_w / 1920, sel_h / 1080);
            //    c1.Scale(new SizeF(sel_w / 1920, sel_h / 1080));
            //    Single currentSize = c1.Font.Size * (sel_h / 1080);
            //    c1.Font = new Font("微軟正黑體", currentSize);
            //}
        }
        public struct eyebrowpoint
        {
            public Point leftleftpoint;//left 4 左眉
            public Point leftrightpoint;
            public Point lefttoppoint;

            public Point rightleftpoint;//right 4 右眉
            public Point rightrightpoint;
            public Point righttoppoint;

            public Point centerpoint;//眉毛中間點
        }
        public struct lipspoint
        {
            public Point leftlipspoint;
            public Point rightlipspoint;

        }
        public struct faceupleftrightdownpoint//臉高 臉寬 髮際線
        {
            public Point facetoppoint;
            public Point facedownpoint;
            public Point faceleftpoint;
            public Point facerightpoint;
        }






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
                            if ((BoundingBox.Width / BoundingBox.Height) > 1.5 && (BoundingBox.Width * BoundingBox.Height) > 1000 && (BoundingBox.Width * BoundingBox.Height) < 7000&& BoundingBox.Y<gray.Height/3)//過濾長寬比太小和面積太小的box
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








                                Point[] subsampling = new Point[points.Length/15+1];
                                for (int j = 0; j < temp.Length; j=j+15)
                                {
                                    subsampling[j / 15] = points[j];
                                }
                                subsampling[points.Length / 15] = subsampling[0];
                                




                                Graphics g = Graphics.FromImage(tempimg);//畫上曲線
                                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0),3);
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
                                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y,sf);
                                g2.Dispose();

                                Graphics g3 = Graphics.FromImage(tempimg);//畫右點
                                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y,sf);
                                g3.Dispose();
                                

                                Graphics g4 = Graphics.FromImage(tempcolorimg);//畫左點
                                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y,sf);
                                g4.Dispose();

                                Graphics g5 = Graphics.FromImage(tempcolorimg);//畫右點
                                g5.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y,sf);
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

        public class MyFace
        {
            public static Point nosepoint(Image<Bgr, byte> My_Image2, Image<Gray, byte> My_Image1, Rectangle[] faces)//鼻子中心點
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
                int biggestnose = 0;//紀錄最大的面積在哪個contours



                Rectangle first = new Rectangle(0, 0, 0, 0), second = new Rectangle(0, 0, 0, 0);//最大跟第二大物件的座標
                for (int i = 0; i < countnose; i++)//找最大
                {
                    using (VectorOfPoint contour = contoursnose[i])
                    {
                        Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                        if (BoundingBox.Width * BoundingBox.Height > first.Width * first.Height) { first = BoundingBox; biggestnose = i; }
                    }
                }

                for (int i = 0; i < countnose; i++)//找第二大
                {
                    using (VectorOfPoint contour = contoursnose[i])
                    {
                        if (i != biggestnose)
                        {
                            Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                            if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height) { second = BoundingBox; }
                        }
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
            public static eyebrowpoint eyebrowfindpoint(Image<Bgr, Byte> My_Image2,Rectangle[]faces)//眉毛點
            {
                eyebrowpoint ebp = new eyebrowpoint();
               
                Rectangle righteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 40) * 9, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取右眉毛範圍
                Rectangle lefteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 9) * 5, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取左眉毛範圍

                Image<Bgr, byte> righteyebrowuseimg = new Image<Bgr, Byte>(MyFace.Temperature(My_Image2));
                Image<Bgr, byte> lefteyebrowuseimg = new Image<Bgr, Byte>(MyFace.Temperature(My_Image2));

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
                stackdownlip.Push(new Point(dark.X, dark.Y));


                // 領域の開始点の色を領域条件とする
                Color colourdownlip = lipbitmap.GetPixel(dark.X, dark.Y);
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
                faceulrd.facedownpoint = facebutton;
                faceulrd.faceleftpoint = faceleft;
                faceulrd.facerightpoint = faceright;
                faceulrd.facetoppoint = facetop;
                return faceulrd;
            }


            
        }

        VideoCapture webCam = null;

        public Rectangle[] faces;
        public Rectangle facesori;
        Rectangle facezoom;
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Width = (int)sel_w;
            //this.Height = (int)sel_h;

            //Form3 form3 = new Form3();
            //form3.Tag = this;
            //form3.TopMost = true;
            //form3.ShowDialog();//鎖定


        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image != null)
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files(*.BMP)|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    webCam.QueryFrame().ToImage<Bgr, Byte>().Save(saveFileDialog.FileName);
                }
            }
        }
        

        private void toolStripMenuItem5_Click(object sender, EventArgs e)//眉毛
        {
            
            imageBox1.Image = My_Image2;
            
            
            imageBox1.Image = new Image<Bgr, Byte>(MyFace.Temperature(My_Image2));

            Rectangle righteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 40) * 9, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取右眉毛範圍
            Rectangle lefteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 9) * 5, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取左眉毛範圍

            Image<Bgr, byte> righteyebrowuseimg = new Image<Bgr, Byte>(MyFace.Temperature(My_Image2)); 
            Image<Bgr, byte> lefteyebrowuseimg = new Image<Bgr, Byte>(MyFace.Temperature(My_Image2)); 

            
            righteyebrowuseimg.ROI = righteyebrowrange;//擷取眼睛區域
            lefteyebrowuseimg.ROI = lefteyebrowrange;

            

            Image<Bgr, byte> righteyebrowuseimgroi = new Image<Bgr, byte>(righteyebrowuseimg.Bitmap);
            Image<Bgr, byte> lefteyebrowuseimgroi = new Image<Bgr, byte>(lefteyebrowuseimg.Bitmap);
            


            Bitmap righteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//取出眉毛bitmap
            Bitmap lefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            Bitmap setrighteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//Bitmap輸出
            Bitmap setlefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////前處理
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
                    if (Math.Abs(Red - Gre) <= range && Math.Abs(Gre - Blu) <= range && Math.Abs(Red - Blu) <= range && (Red + Gre+ Blu) / 3 <= 150)//&& (hue > 60 || hue == 0))
                    {
                        setlefteyebrowbitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { setlefteyebrowbitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////找生長起點
            int tempcolse = 2147483647;
            Point righteyebrowxy = new Point(0,0);
            for (int y = 0; y < righteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < righteyebroww; x++)
                {
                    Color color = setrighteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(righteyebroww/2-x) + Math.Abs(righteyebrowh/2 - y))< tempcolse)//找離圖片中心最接近的眉毛當起始點
                    {
                        tempcolse = Math.Abs(righteyebroww / 2 - x) + Math.Abs(righteyebrowh / 2 - y);
                        righteyebrowxy.X = x;
                        righteyebrowxy.Y = y;
                    }
                }
            }

            tempcolse = 2147483647;
            Point lefteyebrowxy = new Point(0, 0);
            for (int y = 0; y < lefteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < lefteyebroww; x++)
                {
                    Color color = setlefteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y)) < tempcolse)
                    {
                        tempcolse = Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y);
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
                    if ((p.X + 1 < image.Width && Math.Abs((Math.Abs(colour.R- colour.G)+ Math.Abs(colour.G - colour.B)+ Math.Abs(colour.R - colour.B))-(Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 1, p.Y).G) + Math.Abs(image.GetPixel(p.X + 1, p.Y).G - image.GetPixel(p.X + 1, p.Y).B) + Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 1, p.Y).B))) <= range2  && (image.GetPixel(p.X + 1, p.Y).R + image.GetPixel(p.X + 1, p.Y).G + image.GetPixel(p.X + 1, p.Y).B) / 3 <= light) || (p.X + 2 < image.Width  && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X + 2, p.Y).R - image.GetPixel(p.X + 2, p.Y).G) + Math.Abs(image.GetPixel(p.X + 2, p.Y).G - image.GetPixel(p.X + 2, p.Y).B) + Math.Abs(image.GetPixel(p.X + 1, p.Y).R - image.GetPixel(p.X + 2, p.Y).B))) <= range2 && (image.GetPixel(p.X + 2, p.Y).R + image.GetPixel(p.X + 2, p.Y).G + image.GetPixel(p.X + 2, p.Y).B) / 3 <= light) || (p.X + 3 < image.Width && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X + 3, p.Y).R - image.GetPixel(p.X + 3, p.Y).G) + Math.Abs(image.GetPixel(p.X + 3, p.Y).G - image.GetPixel(p.X + 3, p.Y).B) + Math.Abs(image.GetPixel(p.X + 3, p.Y).R - image.GetPixel(p.X + 3, p.Y).B))) <= range2 && (image.GetPixel(p.X + 3, p.Y).R + image.GetPixel(p.X + 3, p.Y).G + image.GetPixel(p.X + 3, p.Y).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if ((p.X - 1 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 1, p.Y).R - image.GetPixel(p.X - 1, p.Y).G) + Math.Abs(image.GetPixel(p.X - 1, p.Y).G - image.GetPixel(p.X - 1, p.Y).B) + Math.Abs(image.GetPixel(p.X - 1, p.Y).R - image.GetPixel(p.X - 1, p.Y).B))) <= range2 && (image.GetPixel(p.X - 1, p.Y).R + image.GetPixel(p.X - 1, p.Y).G + image.GetPixel(p.X - 1, p.Y).B) / 3 <= light) || (p.X - 2 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 2, p.Y).R - image.GetPixel(p.X - 2, p.Y).G) + Math.Abs(image.GetPixel(p.X - 2, p.Y).G - image.GetPixel(p.X - 2, p.Y).B) + Math.Abs(image.GetPixel(p.X - 2, p.Y).R - image.GetPixel(p.X - 2, p.Y).B))) <= range2 && (image.GetPixel(p.X - 2, p.Y).R + image.GetPixel(p.X - 2, p.Y).G + image.GetPixel(p.X - 2, p.Y).B) / 3 <= light) || (p.X - 3 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X - 3, p.Y).R - image.GetPixel(p.X - 3, p.Y).G) + Math.Abs(image.GetPixel(p.X - 3, p.Y).G - image.GetPixel(p.X - 3, p.Y).B) + Math.Abs(image.GetPixel(p.X - 3, p.Y).R - image.GetPixel(p.X - 3, p.Y).B))) <= range2 && (image.GetPixel(p.X - 3, p.Y).R + image.GetPixel(p.X - 3, p.Y).G + image.GetPixel(p.X - 3, p.Y).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if ((p.Y + 1 < image.Height  && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X , p.Y + 1).R - image.GetPixel(p.X, p.Y + 1).G) + Math.Abs(image.GetPixel(p.X , p.Y + 1).G - image.GetPixel(p.X , p.Y + 1).B) + Math.Abs(image.GetPixel(p.X , p.Y + 1).R - image.GetPixel(p.X , p.Y + 1).B))) <= range2 && (image.GetPixel(p.X, p.Y + 1).R + image.GetPixel(p.X, p.Y + 1).G + image.GetPixel(p.X, p.Y + 1).B) / 3 <= light) || (p.Y + 2 < image.Height  && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y + 2).R - image.GetPixel(p.X, p.Y + 2).G) + Math.Abs(image.GetPixel(p.X, p.Y + 2).G - image.GetPixel(p.X, p.Y + 2).B) + Math.Abs(image.GetPixel(p.X, p.Y + 2).R - image.GetPixel(p.X, p.Y + 2).B))) <= range2 && (image.GetPixel(p.X, p.Y + 2).R + image.GetPixel(p.X, p.Y + 2).G + image.GetPixel(p.X, p.Y + 2).B) / 3 <= light) || (p.Y + 3 < image.Height && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y + 3).R - image.GetPixel(p.X, p.Y + 3).G) + Math.Abs(image.GetPixel(p.X, p.Y + 3).G - image.GetPixel(p.X, p.Y + 3).B) + Math.Abs(image.GetPixel(p.X, p.Y + 3).R - image.GetPixel(p.X, p.Y + 3).B))) <= range2 && (image.GetPixel(p.X, p.Y + 3).R + image.GetPixel(p.X, p.Y + 3).G + image.GetPixel(p.X, p.Y + 3).B) / 3 <= light))
                    {
                        stack.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if ((p.Y - 1 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 1).R - image.GetPixel(p.X, p.Y - 1).G) + Math.Abs(image.GetPixel(p.X, p.Y - 1).G - image.GetPixel(p.X, p.Y - 1).B) + Math.Abs(image.GetPixel(p.X, p.Y - 1).R - image.GetPixel(p.X, p.Y - 1).B))) <= range2 && (image.GetPixel(p.X, p.Y - 1).R + image.GetPixel(p.X, p.Y - 1).G + image.GetPixel(p.X, p.Y - 1).B) / 3 <= light) || (p.Y - 2 >= 0  && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 2).R - image.GetPixel(p.X, p.Y - 2).G) + Math.Abs(image.GetPixel(p.X, p.Y - 2).G - image.GetPixel(p.X, p.Y - 2).B) + Math.Abs(image.GetPixel(p.X, p.Y - 2).R - image.GetPixel(p.X, p.Y - 2).B))) <= range2 && (image.GetPixel(p.X, p.Y - 2).R + image.GetPixel(p.X, p.Y - 2).G + image.GetPixel(p.X, p.Y - 2).B) / 3 <= light) || (p.Y - 3 >= 0 && Math.Abs((Math.Abs(colour.R - colour.G) + Math.Abs(colour.G - colour.B) + Math.Abs(colour.R - colour.B)) - (Math.Abs(image.GetPixel(p.X, p.Y - 3).R - image.GetPixel(p.X, p.Y - 3).G) + Math.Abs(image.GetPixel(p.X, p.Y - 3).G - image.GetPixel(p.X, p.Y - 3).B) + Math.Abs(image.GetPixel(p.X, p.Y - 3).R - image.GetPixel(p.X, p.Y - 3).B))) <= range2 && (image.GetPixel(p.X, p.Y - 3).R + image.GetPixel(p.X, p.Y - 3).G + image.GetPixel(p.X, p.Y - 3).B) / 3 <= light))
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
            if (lefteyebrowxy.X + 1 < leftimage.Width && lefteyebrowxy.X -1 >= 0 && lefteyebrowxy.Y + 1 < leftimage.Height && lefteyebrowxy.Y - 1 >= 0)
            {
                int Red = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).R + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).R) / 9;
                int Gre = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).G + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).G) / 9;
                int Blu = (lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X - 1, lefteyebrowxy.Y + 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X, lefteyebrowxy.Y + 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y - 1).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y).B + lefteyebrowbitmap.GetPixel(lefteyebrowxy.X + 1, lefteyebrowxy.Y + 1).B) / 9;
                leftcolour = Color.FromArgb(Red,Gre,Blu);
            }
            else{
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

            int righteyebrowleft = 2147483647, righteyebrowright = 0, righteyebrowtop = 2147483647, righteyebrowbutton = 0;
            int righteyebrowleftpoint = 0, righteyebrowrightpoint = 0, righteyebrowtoppoint = 0, righteyebrowbuttonpoint = 0;
            Bitmap tempcolorimg1 = new Bitmap(My_Image2.Bitmap);
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
                        subsampling[j / 15] = new Point(points[j].X+righteyebrowrange.X,points[j].Y + righteyebrowrange.Y);
                    }
                    subsampling[points.Length / 15] = subsampling[0];


                    Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                    SolidBrush drawBrush = new SolidBrush(Color.Green);
                    Graphics g = Graphics.FromImage(tempcolorimg1);//畫曲線
                    if (points.Length > 1)
                        g.DrawCurve(pen, subsampling,0.3f);
                    g.Dispose();



                    StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                



                    Graphics g4 = Graphics.FromImage(tempcolorimg1);
                    g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowleftpoint].X+righteyebrowrange.X, points[righteyebrowleftpoint].Y+ righteyebrowrange.Y, sf);//畫左點
                    g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowrightpoint].X + righteyebrowrange.X, points[righteyebrowrightpoint].Y+righteyebrowrange.Y, sf);//畫右點
                    g4.DrawString("+", new Font("Arial", 25), drawBrush, points[righteyebrowtoppoint].X + righteyebrowrange.X, points[righteyebrowtoppoint].Y + righteyebrowrange.Y, sf);//畫上點
                    g4.Dispose();
                
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
                    subsampling[j / 15] = new Point(points[j].X+ lefteyebrowrange.X,points[j].Y+ lefteyebrowrange.Y);
                }
                subsampling[points.Length / 15] = subsampling[0];


                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                SolidBrush drawBrush = new SolidBrush(Color.Green);
                Graphics g = Graphics.FromImage(tempcolorimg1);//畫曲線
                if (points.Length > 1)
                    g.DrawCurve(pen, subsampling, 0.3f);
                g.Dispose();



                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;




                Graphics g4 = Graphics.FromImage(tempcolorimg1);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowleftpoint].X+ lefteyebrowrange.X, points[lefteyebrowleftpoint].Y+ lefteyebrowrange.Y, sf);//畫左點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowrightpoint].X+ lefteyebrowrange.X, points[lefteyebrowrightpoint].Y+ lefteyebrowrange.Y, sf);//畫右點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[lefteyebrowtoppoint].X+ lefteyebrowrange.X, points[lefteyebrowtoppoint].Y+ lefteyebrowrange.Y, sf);//畫上點
                g4.Dispose();

            }








            /////////////////////////////////////////////////////////////////////////////////////////////////畫出起始點
            //StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
            //sf.Alignment = StringAlignment.Center;
            //sf.LineAlignment = StringAlignment.Center;

            //Graphics g2 = Graphics.FromImage(setrighteyebrowbitmap);
            //SolidBrush drawBrush = new SolidBrush(Color.Green);
            //Pen pengreen = new Pen(Color.Green, 3);
            //g2.DrawString("+", new Font("Arial", 25), drawBrush, righteyebrowxy.X, righteyebrowxy.Y, sf);//畫左點
            //g2.Dispose();

            //Graphics g3 = Graphics.FromImage(setlefteyebrowbitmap);
            //g3.DrawString("+", new Font("Arial", 25), drawBrush, lefteyebrowxy.X, lefteyebrowxy.Y, sf);//畫左點
            //g3.Dispose();

            //timer3.Start();

            imageBox2.Image = new Image<Bgr, byte>(tempcolorimg1);

        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)//拍照
        {
            
        }

        
        double mmperpixel=0.5;//每個像素幾毫米
        private void toolStripMenuItem8_Click(object sender, EventArgs e)//比例校正
        {
            label5.Text = null;
            label6.Text = null;
            label7.Text = null;
            imageBox5.Image = null;
            imageBox6.Image = null;


            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    My_Image1 = new Image<Gray, byte>(Openfile.FileName);
                    My_Image2 = new Image<Bgr, byte>(Openfile.FileName);

                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }

            if (My_Image2 != null)
            {

                Image<Bgr, Byte> img = new Image<Bgr, Byte>(My_Image2.Bitmap);
                //img.ROI = new Rectangle(2 * img.Width / 5, img.Height / 2, img.Width / 4, img.Height / 2);
                Image<Bgr, Byte> result = img.Copy();
                CircleF[] colsecircle = scale(img, ref result);
                label1.Text = "硬幣半徑 : " + colsecircle[0].Radius.ToString("#0.00") + " pixels";
                label2.Text = "硬幣直徑 : " + (colsecircle[0].Radius * 2).ToString("#0.00") + " pixels";
                label3.Text = "硬幣真實直徑(26 mm)/硬幣直徑 : " + (26 / (colsecircle[0].Radius * 2)).ToString("#0.00") + "mm /pixels";
                mmperpixel = 26 / (colsecircle[0].Radius * 2);
                imageBox1.Image = result;
            }
            else
                MessageBox.Show("No image!");
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;

        }
        SectionDetection sectionDetection = new SectionDetection();
        private CircleF[] scale(Image<Bgr, Byte> src, ref Image<Bgr, Byte> dest)
        {
            CircleF[] closeCircle;
            double[] std;
            //example of GeometryDetection 350,300 should be variable!!
            sectionDetection.GeometryDetection(src, dest, 350, 350, out closeCircle, out std);
            //example of DrewAllCircle
            sectionDetection.DrewAllCircle(ref dest, closeCircle);
            int width = 10;
            //sectionDetection.ALLDefectDetection(src, dest, closeCircle);
            System.Drawing.Rectangle ROIrectangle = new System.Drawing.Rectangle((int)(closeCircle[0].Center.X - closeCircle[0].Radius - width), (int)(closeCircle[0].Center.Y - closeCircle[0].Radius - width), (int)(closeCircle[0].Radius + width) * 2, (int)(closeCircle[0].Radius + width) * 2);
            //src.ROI = ROIrectangle;
            //dest.ROI = ROIrectangle;
            //比例因子
            //label10.Visible = true;
            //Factor_Value = 25 / closeCircle[2].Radius;
            //factor.Left = label10.Right + 5;
            //factor.Text = Factor_Value.ToString("f3") + "mm/pixel";
            return closeCircle;
        }


        public static Bitmap BoundingBox(Image<Gray, Byte> gray, Image<Bgr, byte> draw)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                Bitmap tempimg = new Bitmap(gray.Bitmap);
                Bitmap tempcolorimg = new Bitmap(draw.Bitmap);

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


                    StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    //facewidth = Math.Sqrt(Math.Pow((points[leftpoint].X - points[rightpoint].X), 2) + Math.Pow((points[leftpoint].Y - points[rightpoint].Y), 2));

                    Graphics g2 = Graphics.FromImage(tempimg);//畫左點
                    SolidBrush drawBrush = new SolidBrush(Color.Red);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                    g2.Dispose();
                    


                    Graphics g3 = Graphics.FromImage(tempcolorimg);//畫左點
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                    g3.Dispose();
                    

                }
                left = 2147483647; right = 0; top = 2147483647; button = 0;
                leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0;


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

        private void toolStripMenuItem9_Click(object sender, EventArgs e)//ALL
        {
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Value = 1;
            toolStripProgressBar1.Maximum = 7;

            while (My_Image2 == null)
                toolStripMenuItem20_Click(sender, e);

            this.Cursor = Cursors.AppStarting;

            imageBox2.Visible = false;
            imageBox3.Visible = false;
            imageBox4.Visible = false;
            imageBox5.Visible = false;
            imageBox6.Visible = false;
            pictureBox1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            label5.Visible = false;
            label6.Visible = false;

            My_Image2.ROI = Rectangle.Empty;
            imageBox1.Image = My_Image2;
            this.Refresh();

            imageBox3.Visible = true;
            Point centernose;
            Point eyescenter;
            Point Lefteyebrowinnerpoint, Lefteyebrowouterpoint, Lefteyebrowtoppoint;//左眉內側點、左眉外側點、左眉頂點
            Point Righteyebrowinnerpoint, Righteyebrowouterpoint, Righteyebrowtoppoint;
            toolStripProgressBar1.Value++;
            #region nosecenter
            centernose = Face.NoseCenterPoint(My_Image2, My_Image1, faces);
            #endregion
            toolStripProgressBar1.Value++;
            #region eyebrow 眉毛

            Point eyebrowcenterpoint = Point.Empty;
            facecutori.ROI = Rectangle.Empty;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////眉毛new

            Bitmap tempcolorimg1 = new Bitmap(My_Image2.Bitmap);


            Rectangle righteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 40) * 9, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取右眉毛範圍
            Rectangle lefteyebrowrange = new Rectangle(faces[0].X + (faces[0].Width / 9) * 5, faces[0].Y + (faces[0].Height * 2) / 7, faces[0].Width / 4, faces[0].Height / 8);//取左眉毛範圍
            Bitmap Temperatureimg = MyFace.Temperature(My_Image2);
            Image<Bgr, byte> righteyebrowuseimg = new Image<Bgr, Byte>(Temperatureimg);
            Image<Bgr, byte> lefteyebrowuseimg = new Image<Bgr, Byte>(Temperatureimg);

            imageBox3.Image = righteyebrowuseimg;//顯示
            this.Refresh();

            righteyebrowuseimg.ROI = righteyebrowrange;//擷取眉毛區域
            lefteyebrowuseimg.ROI = lefteyebrowrange;

            Image<Bgr, byte> righteyebrowuseimgroi = new Image<Bgr, byte>(righteyebrowuseimg.Bitmap);
            Image<Bgr, byte> lefteyebrowuseimgroi = new Image<Bgr, byte>(lefteyebrowuseimg.Bitmap);



            Bitmap righteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//取出眉毛bitmap
            Bitmap lefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            Bitmap setrighteyebrowbitmap = new Bitmap(righteyebrowuseimg.Bitmap);//Bitmap輸出
            Bitmap setlefteyebrowbitmap = new Bitmap(lefteyebrowuseimg.Bitmap);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////前處理
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
            int tempcolse = 2147483647;
            Point righteyebrowxy = new Point(0, 0);
            for (int y = 0; y < righteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < righteyebroww; x++)
                {
                    Color color = setrighteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(righteyebroww / 2 - x) + Math.Abs(righteyebrowh / 2 - y)) < tempcolse)//找離圖片中心最接近的眉毛當起始點
                    {
                        tempcolse = Math.Abs(righteyebroww / 2 - x) + Math.Abs(righteyebrowh / 2 - y);
                        righteyebrowxy.X = x;
                        righteyebrowxy.Y = y;
                    }
                }
            }

            tempcolse = 2147483647;
            Point lefteyebrowxy = new Point(0, 0);
            for (int y = 0; y < lefteyebrowh; y++)//find eyebrow
            {
                for (int x = 0; x < lefteyebroww; x++)
                {
                    Color color = setlefteyebrowbitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) && (Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y)) < tempcolse)
                    {
                        tempcolse = Math.Abs(lefteyebroww / 2 - x) + Math.Abs(lefteyebrowh / 2 - y);
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


            if (righteyebrowxy.X + 1 < image.Width && righteyebrowxy.X - 1 >= 0 && righteyebrowxy.Y + 1 < image.Height && righteyebrowxy.Y - 1 >= 0)//起始點跟附近8個點平均當作起始點顏色
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
            Image<Bgr, byte> facewh = new Image<Bgr, byte>(facecutori.Bitmap);
            Bitmap tempcolorimg = new Bitmap(facewh.Bitmap);

            CvInvoke.FindContours(righteyebrowcont, contoursright, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            CvInvoke.FindContours(lefteyebrowcont, contoursleft, null, RetrType.External, ChainApproxMethod.ChainApproxNone);


            Rectangle Boundboxrighteyebrow = CvInvoke.BoundingRectangle(contoursright[0]);
            Rectangle Boundboxlefteyebrow = CvInvoke.BoundingRectangle(contoursleft[0]);



            int righteyebrowleft = 2147483647, righteyebrowright = 0, righteyebrowtop = 2147483647, righteyebrowbutton = 0;
            int righteyebrowleftpoint = 0, righteyebrowrightpoint = 0, righteyebrowtoppoint = 0, righteyebrowbuttonpoint = 0;
            //Bitmap tempcolorimg1 = new Bitmap(My_Image2.Bitmap);
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
                    subsampling[j / 15] = new Point(points[j].X + righteyebrowrange.X, points[j].Y + righteyebrowrange.Y);
                }
                subsampling[points.Length / 15] = subsampling[0];


                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                SolidBrush drawBrush = new SolidBrush(Color.Green);
                Graphics g = Graphics.FromImage(tempcolorimg);//畫曲線
                if (points.Length > 1)
                    g.DrawCurve(pen, subsampling, 0.3f);
                g.Dispose();



                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                Righteyebrowinnerpoint = new Point(points[righteyebrowrightpoint].X + righteyebrowrange.X, points[righteyebrowrightpoint].Y + righteyebrowrange.Y); //右眉內側點
                Righteyebrowouterpoint = new Point(points[righteyebrowleftpoint].X + righteyebrowrange.X, points[righteyebrowleftpoint].Y + righteyebrowrange.Y); //右眉外側點
                Righteyebrowtoppoint = new Point(points[righteyebrowtoppoint].X + righteyebrowrange.X, points[righteyebrowtoppoint].Y + righteyebrowrange.Y);//右眉頂點


                Graphics g4 = Graphics.FromImage(tempcolorimg);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Righteyebrowouterpoint.X, Righteyebrowouterpoint.Y, sf);//畫右眉外側點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Righteyebrowinnerpoint.X, Righteyebrowinnerpoint.Y, sf);//畫右眉內側點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Righteyebrowtoppoint.X, Righteyebrowtoppoint.Y, sf);//畫右眉頂點
                g4.Dispose();

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
                    subsampling[j / 15] = new Point(points[j].X + lefteyebrowrange.X, points[j].Y + lefteyebrowrange.Y);
                }
                subsampling[points.Length / 15] = subsampling[0];


                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
                SolidBrush drawBrush = new SolidBrush(Color.Green);
                Graphics g = Graphics.FromImage(tempcolorimg);//畫曲線
                if (points.Length > 1)
                    g.DrawCurve(pen, subsampling, 0.3f);
                g.Dispose();



                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                Lefteyebrowinnerpoint = new Point(points[lefteyebrowleftpoint].X + lefteyebrowrange.X, points[lefteyebrowleftpoint].Y + lefteyebrowrange.Y);//左眉內側點
                Lefteyebrowouterpoint = new Point(points[lefteyebrowrightpoint].X + lefteyebrowrange.X, points[lefteyebrowrightpoint].Y + lefteyebrowrange.Y);//左眉外側點
                Lefteyebrowtoppoint = new Point(points[lefteyebrowtoppoint].X + lefteyebrowrange.X, points[lefteyebrowtoppoint].Y + lefteyebrowrange.Y);//左眉頂點
                Graphics g4 = Graphics.FromImage(tempcolorimg);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Lefteyebrowinnerpoint.X, Lefteyebrowinnerpoint.Y, sf);//畫左眉內側點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Lefteyebrowouterpoint.X, Lefteyebrowouterpoint.Y, sf);//畫左眉外側點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, Lefteyebrowtoppoint.X, Lefteyebrowtoppoint.Y, sf);//畫左眉頂點
                g4.Dispose();

            }
            //eyebrowcenterpoint = new Point(((Boundboxrighteyebrow.X + righteyebrowrange.X + Boundboxrighteyebrow.Width / 2) + (Boundboxlefteyebrow.X + lefteyebrowrange.X + Boundboxlefteyebrow.Width / 2)) / 2, ((Boundboxrighteyebrow.Y + righteyebrowrange.Y + Boundboxrighteyebrow.Height / 2) + (Boundboxlefteyebrow.Y + lefteyebrowrange.Y + Boundboxlefteyebrow.Height / 2)) / 2);
            eyebrowcenterpoint = new Point((Righteyebrowinnerpoint.X + Lefteyebrowinnerpoint.X) / 2, (Righteyebrowinnerpoint.Y + Lefteyebrowinnerpoint.Y) / 2);
            eyescenter = new Point(eyebrowcenterpoint.X, eyebrowcenterpoint.Y + faces[0].Height / 15);
            eyebrowpoint ebp = MyFace.eyebrowfindpoint(My_Image2, faces);


            #endregion
            imageBox3.Image = new Image<Bgr, byte>(tempcolorimg);

            toolStripProgressBar1.Value++;
            #region 髮際點
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////找髮際點
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
            ///////////////////////////////////////////////////////////////////////////////
            Point facebutton, faceright, faceleft;


            /////////////////////////////////////////////////////////////////////////////找臉左右下端點
            //Image<Bgr, byte> facewh = new Image<Bgr, byte>(facecutori.Bitmap);
            Image<Gray, byte> facewhgray = new Image<Gray, byte>(My_Image1.Bitmap);

            int w1 = My_Image2.Bitmap.Width;
            int h1 = My_Image2.Bitmap.Height;
            Bitmap image11 = new Bitmap(facecutori.Bitmap);
            Bitmap skin1 = new Bitmap(facecutori.Bitmap);
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
            double facewidth = 0;
            double faceheight = 0;
            double eyebrowtochin = 0;

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



                #endregion
                toolStripProgressBar1.Value++;
                imageBox3.Image = new Image<Bgr, byte>(tempcolorimg);
                this.Refresh();
                #region 嘴唇

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////嘴唇

                Image<Bgr, Byte> lips = My_Image2.Clone();
                //lips = lips.SmoothGaussian(11);
                //Rectangle liprange = new Rectangle(centernose.X - 75, centernose.Y + 20, 150, faces[0].Height / 5);//用鼻中心取嘴唇範圍
                Rectangle liprange = new Rectangle(faces[0].X + faces[0].Width / 3, faces[0].Y + faces[0].Height * 7 / 12, faces[0].Width / 3, faces[0].Height / 5);//用鼻中心取嘴唇範圍
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

                w = lipbitmap.Width;
                h = lipbitmap.Height;
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
                smooth = smooth.SmoothMedian(11);
                lipbitmap = smooth.Bitmap;

                Graphics gate = Graphics.FromImage(lipbitmap);//畫線區隔上下唇
                Pen Penred = new Pen(Color.White, 3);
                gate.DrawCurve(Penred, lipscurve);
                gate.Dispose();





                Stack<Point> stackuplip = new Stack<Point>();
                stackuplip.Push(new Point(dark.X, dark.Y - 10));


                // 領域の開始点の色を領域条件とする
                Color colouruplip = lipbitmap.GetPixel(dark.X, dark.Y - 10);

                Bitmap ImageUpLip = new Bitmap(w, h);//上唇
                Bitmap DownLipBitmap = new Bitmap(w, h);//下唇
                Bitmap ImageLip = new Bitmap(w, h);//上下唇

                imagelip = new Bitmap(lipbitmap);
                // 開始点をあらかじめStackに収めておく

                while (stackuplip.Count != 0)
                {
                    // 注目点を取り出す
                    Point p = stackuplip.Pop();

                    // 注目点にまだマーカが付いていない場合
                    if (ImageLip.GetPixel(p.X, p.Y) != Color.FromArgb(255, 0, 0))
                    {

                        // マーカを付ける
                        ImageLip.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, 0));
                        int range = 200; int huerange = 15;
                        // 右隣を見て、領域条件に合えばStackに収める
                        if ((p.X + 1 < imagelip.Width && Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X + 1, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X + 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X + 1, p.Y).GetHue() >= 300)) || (p.X + 2 < imagelip.Width && Math.Abs(imagelip.GetPixel(p.X + 2, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X + 2, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X + 2, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X + 2, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X + 2, p.Y).GetHue() >= 300)))
                        {
                            stackuplip.Push(new Point(p.X + 1, p.Y));
                        }

                        // 左隣を見て、領域条件に合えばStackに収める
                        if ((p.X - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X - 1, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X - 1, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X - 1, p.Y).GetHue() >= 300)) || (p.X - 2 >= 0 && Math.Abs(imagelip.GetPixel(p.X - 2, p.Y).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X - 2, p.Y).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X - 2, p.Y).B - colouruplip.B) < range && (imagelip.GetPixel(p.X - 2, p.Y).GetHue() <= huerange || imagelip.GetPixel(p.X - 2, p.Y).GetHue() >= 300)))
                        {
                            stackuplip.Push(new Point(p.X - 1, p.Y));
                        }

                        // 下を見て、領域条件に合えばStackに収める
                        if ((p.Y + 1 < imagelip.Height && Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 1).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y + 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y + 1).GetHue() >= 300)) || (p.Y + 2 < imagelip.Height && Math.Abs(imagelip.GetPixel(p.X, p.Y + 2).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 2).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y + 2).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y + 2).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y + 2).GetHue() >= 300)))
                        {
                            stackuplip.Push(new Point(p.X, p.Y + 1));
                        }

                        // 上を見て、領域条件に合えばStackに収める
                        if ((p.Y - 1 >= 0 && Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 1).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y - 1).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y - 1).GetHue() >= 300)) || (p.Y - 2 >= 0 && Math.Abs(imagelip.GetPixel(p.X, p.Y - 2).R - colouruplip.R) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 2).G - colouruplip.G) + Math.Abs(imagelip.GetPixel(p.X, p.Y - 2).B - colouruplip.B) < range && (imagelip.GetPixel(p.X, p.Y - 2).GetHue() <= huerange || imagelip.GetPixel(p.X, p.Y - 2).GetHue() >= 300)))
                        {
                            stackuplip.Push(new Point(p.X, p.Y - 1));
                        }


                    }
                }//Region growing



                Stack<Point> stackdownlip = new Stack<Point>();
                stackdownlip.Push(new Point(dark.X, dark.Y + 10));


                // 領域の開始点の色を領域条件とする
                Color colourdownlip = lipbitmap.GetPixel(dark.X, dark.Y + 10);
                imagelip = new Bitmap(lipbitmap);
                // 開始点をあらかじめStackに収めておく

                while (stackdownlip.Count != 0)
                {
                    // 注目点を取り出す
                    Point p = stackdownlip.Pop();

                    // 注目点にまだマーカが付いていない場合
                    if (ImageLip.GetPixel(p.X, p.Y) != Color.FromArgb(0, 255, 0))
                    {

                        // マーカを付ける
                        ImageLip.SetPixel(p.X, p.Y, Color.FromArgb(0, 255, 0));//畫下唇
                        DownLipBitmap.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255));//取出下唇 找輪廓用
                        int range = 150; int huerange = 10;
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


                Image<Gray, byte> DownLipImage = new Image<Gray, byte>(DownLipBitmap);
                VectorOfVectorOfPoint DownLipContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(DownLipImage, DownLipContours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);//找下唇輪廓

                int DownLipLeft = 2147483647, DownLipRight = 0, DownLipButton = 0;
                int DownLipLeftTemp = 0, DownLipRightTemp = 0, DownLipButtonTemp = 0;
                Point DownLipLeftPoint, DownLipRightPoint, DownLipButtonPoint;
                using (VectorOfPoint Contour = DownLipContours[0])
                {

                    PointF[] DownLipContoursTemp = Array.ConvertAll(Contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                    //PointF[] DownLipConvexHull = CvInvoke.ConvexHull(DownLipContoursTemp, true);
                    Point[] DownLipPoint = new Point[DownLipContoursTemp.Length];

                    for (int j = 0; j < DownLipContoursTemp.Length; j++)//找上下左右端點
                    {
                        DownLipPoint[j] = Point.Round(DownLipContoursTemp[j]);//PointF2Point


                        if (j > 1 && DownLipPoint[j].X < DownLipLeft)
                        {
                            DownLipLeft = DownLipPoint[j].X;
                            DownLipLeftTemp = j;
                        }

                        if (j > 1 && DownLipPoint[j].X > DownLipRight)
                        {
                            DownLipRight = DownLipPoint[j].X;
                            DownLipRightTemp = j;
                        }


                    }

                    for (int j = 0; j < DownLipContoursTemp.Length; j++)//找上下左右端點
                    {
                        DownLipPoint[j] = Point.Round(DownLipContoursTemp[j]);//PointF2Point


                        if (j > 1 && DownLipPoint[j].Y > DownLipButton && DownLipPoint[j].X == ((DownLipPoint[DownLipLeftTemp].X + DownLipPoint[DownLipRightTemp].X) / 2))
                        {
                            DownLipButton = DownLipPoint[j].Y;
                            DownLipButtonTemp = j;
                        }
                    }

                    DownLipLeftPoint = new Point(DownLipPoint[DownLipLeftTemp].X, DownLipPoint[DownLipLeftTemp].Y);
                    DownLipRightPoint = new Point(DownLipPoint[DownLipRightTemp].X, DownLipPoint[DownLipRightTemp].Y);
                    DownLipButtonPoint = new Point(DownLipPoint[DownLipButtonTemp].X, DownLipPoint[DownLipButtonTemp].Y);
                    Point[] DownlipCurve = { DownLipLeftPoint, DownLipButtonPoint, DownLipRightPoint };
                    SolidBrush DrawBrush = new SolidBrush(Color.Red);
                    StringFormat Sflip = new StringFormat();//設定string置中，drawString才不會錯位
                    Sflip.Alignment = StringAlignment.Center;
                    Sflip.LineAlignment = StringAlignment.Center;
                    Pen RenPen = new Pen(Color.Red, 3);

                    Graphics g1 = Graphics.FromImage(DownLipBitmap);
                    g1.DrawString("+", new Font("Arial", 25), DrawBrush, DownLipPoint[DownLipLeftTemp].X, DownLipPoint[DownLipLeftTemp].Y, Sflip);
                    g1.DrawString("+", new Font("Arial", 25), DrawBrush, DownLipPoint[DownLipRightTemp].X, DownLipPoint[DownLipRightTemp].Y, Sflip);
                    g1.DrawString("+", new Font("Arial", 25), DrawBrush, DownLipPoint[DownLipButtonTemp].X, DownLipPoint[DownLipButtonTemp].Y, Sflip);
                    g1.DrawCurve(RenPen, DownlipCurve, (float)1.1);
                    g1.Dispose();

                }


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

                imageBox2.Image = new Image<Bgr, byte>(ImageLip);

                imageBox6.Visible = true;
                imageBox6.Image = new Image<Bgr, byte>(DownLipBitmap);




                for (int y = liprange.Y; y < liprange.Bottom; y++)//畫嘴唇
                {
                    for (int x = liprange.X; x < liprange.Right; x++)
                    {
                        Color color = ImageLip.GetPixel(x - liprange.X, y - liprange.Y);
                        int R = color.R;
                        int G = color.G;
                        int B = color.B;

                        if (color.R != 0 || color.G != 0 || color.B != 0)
                        {
                            tempcolorimg.SetPixel(x, y, color);
                        }
                    }
                }





                lipspoint lp = new lipspoint();
                lp = MyFace.lip(My_Image2, centernose, faces);
                Face.lip(My_Image2, centernose, faces);

                faceupleftrightdownpoint faceulrd = new faceupleftrightdownpoint();
                faceulrd = MyFace.faceulrdpoint(My_Image2, My_Image1, faces, centernose, facecutori, facecutorigray);

                #endregion
                toolStripProgressBar1.Value++;
                imageBox3.Image = new Image<Bgr, byte>(tempcolorimg);
                this.Refresh();
                #region 鼻子輪廓
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////鼻子輪廓

                Image<Bgr, byte> noseshape = My_Image2.Clone();
                Image<Gray, byte> noseshapegray = My_Image1.Clone();
                noseshapegray = noseshapegray.SmoothMedian(3);
                Rectangle noseshaperange = new Rectangle(faces[0].X + (faces[0].Width / 6) * 2, faces[0].Y + faces[0].Height * 50 / 100, faces[0].Width / 3, faces[0].Height / 7);//取鼻子範圍
                Image<Gray, float> sobelX = noseshapegray.Sobel(1, 0, 3);
                Image<Gray, float> sobelY = noseshapegray.Sobel(0, 1, 3);
                Image<Gray, Byte> sobelXByte = sobelX.Convert<Gray, Byte>();
                Image<Gray, Byte> sobelYByte = sobelY.Convert<Gray, Byte>();
                sobelX = sobelX.AbsDiff(new Gray(0));
                sobelY = sobelY.AbsDiff(new Gray(0));
                Image<Gray, float> sobel = sobelX + sobelY;
                double[] mins, maxs;
                Point[] minLoc, maxLoc;
                sobel.MinMax(out mins, out maxs, out minLoc, out maxLoc);
                Image<Gray, Byte> sobelImage = sobel.ConvertScale<byte>(255 / maxs[0], 0);
                sobelImage._ThresholdBinary(new Gray(30), new Gray(255));
                sobelImage.ROI = noseshaperange;
                noseshape.ROI = noseshaperange;
                Image<Bgr, Byte> sobelImagetoBgr = sobelImage.Convert<Bgr, Byte>();//Sobel影像轉Bgr
                Image<Bgr, Byte> tempcolorimgBgrbyte = new Image<Bgr, Byte>(tempcolorimg);
                tempcolorimgBgrbyte.ROI = noseshaperange;

                Bitmap sobelBitmap = sobelImagetoBgr.Bitmap;
                Bitmap noseshapeBitmap = noseshape.Bitmap;
                Bitmap tempcolorimgBgrbyteBitmap = tempcolorimgBgrbyte.Bitmap;
                int noseshapew = noseshapeBitmap.Width, noseshapeh = noseshapeBitmap.Height;

                for (int y = 0; y < noseshapeh; y++)//find eyebrow
                {
                    for (int x = 0; x < noseshapew; x++)
                    {
                        Color color = sobelBitmap.GetPixel(x, y);

                        if (color == Color.FromArgb(255, 255, 255))
                        {
                            tempcolorimgBgrbyteBitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0));
                        }
                    }
                }

                tempcolorimgBgrbyte.Bitmap = tempcolorimgBgrbyteBitmap;
                tempcolorimgBgrbyte.ROI = Rectangle.Empty;
                tempcolorimg = tempcolorimgBgrbyte.Bitmap;

                #endregion
                toolStripProgressBar1.Value++;
                imageBox3.Image = new Image<Bgr, byte>(tempcolorimg);
                this.Refresh();


                //this.Refresh();介面刷新

                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                facewidth = Math.Sqrt(Math.Pow((points[leftpoint].X - points[rightpoint].X), 2) + Math.Pow((points[leftpoint].Y - points[rightpoint].Y), 2));//臉寬度
                faceheight = Math.Sqrt(Math.Pow((facetop.X - points[buttonpoint].X), 2) + Math.Pow((facetop.Y - points[buttonpoint].Y), 2));//臉高度
                eyebrowtochin = Math.Abs(points[buttonpoint].Y - eyebrowcenterpoint.Y);//眉中心至下巴距離




                SolidBrush drawBrush = new SolidBrush(Color.Green);
                SolidBrush drawBrushY = new SolidBrush(Color.Green);
                SolidBrush drawBrushRed = new SolidBrush(Color.Red);
                Pen greenPen = new Pen(Color.Green, 3);
                Pen redPen = new Pen(Color.Red, 3);
                Pen bluePen = new Pen(Color.Blue, 3);
                Graphics g3 = Graphics.FromImage(tempcolorimg);
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                g3.DrawString("+", new Font("Arial", 25), drawBrush, facetop.X, facetop.Y, sf);//髮際點
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);//下巴點
                g3.DrawString("+", new Font("Arial", 50), drawBrush, L_CornerL.X + 50, L_CornerL.Y + 50, sf);//左眼左角點
                //g3.DrawString("x", new Font("Arial", 25), drawBrush, eyescenter.X, eyescenter.Y, sf); //兩眼中心點

                g3.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                g3.DrawLine(redPen, facetop.X, facetop.Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                g3.DrawLine(redPen, centernose.X - 200, centernose.Y, centernose.X + 200, centernose.Y);//畫鼻子線
                g3.DrawLine(redPen, faceulrd.facetoppoint.X - 200, faceulrd.facetoppoint.Y, faceulrd.facetoppoint.X + 200, faceulrd.facetoppoint.Y);//畫髮際線
                g3.DrawLine(redPen, points[buttonpoint].X - 200, points[buttonpoint].Y, points[buttonpoint].X + 200, points[buttonpoint].Y);//畫下巴線
                g3.DrawString("+", new Font("Arial", 25), drawBrushY, centernose.X, centernose.Y, sf);//鼻中心點
                g3.DrawLine(bluePen, eyescenter.X, eyescenter.Y, eyescenter.X, centernose.Y - faces[0].Height / 30);//畫鼻樑
                if (eyebrowcenterpoint.X != 0)
                {
                    g3.DrawString("+", new Font("Arial", 25), drawBrushY, ebp.centerpoint.X, ebp.centerpoint.Y, sf);//眉中心點
                    g3.DrawLine(redPen, centernose.X - 200, eyebrowcenterpoint.Y, centernose.X + 200, eyebrowcenterpoint.Y);//畫眉毛線
                }
                g3.DrawString("+", new Font("Arial", 25), drawBrushY, lp.leftlipspoint.X, lp.leftlipspoint.Y, sf);//嘴唇左點
                g3.DrawString("+", new Font("Arial", 25), drawBrushY, lp.rightlipspoint.X, lp.rightlipspoint.Y, sf);//嘴唇右點
                g3.Dispose();




            }
            left = 2147483647; right = 0; top = 2147483647; button = 0;
            leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0;


            skinimage.Bitmap = tempimg;
            skinimage.ROI = Rectangle.Empty;
            facewh.Bitmap = tempcolorimg;



            //facewh.ROI = Rectangle.Empty;

            //facewh.ROI = facewhRange;

            //Form5 form5 = new Form5();
            //form5.Tag = this;
            //form5.Location = new System.Drawing.Point(this.Location.X + this.Size.Width/2, this.Location.Y);
            //form5.Show();

            Rectangle facecutbywh = new Rectangle(faceleft.X - 4, facetop.Y - 2, (int)facewidth + 8, (int)faceheight + 4);

            facewh.ROI = faces[0];
            My_Image2.ROI = faces[0];
            imageBox3.Image = facewh;
            imageBox2.Image = facewh;
            imageBox1.Image = My_Image2;

            label40.Text = (faceheight * mmperpixel).ToString("#0.00") + " mm";//臉的高度
            label41.Text = (facewidth * mmperpixel).ToString("#0.00") + " mm";//臉的寬度
            label42.Text = (faceheight / facewidth).ToString("#0.00") + " : 1";//臉的高寬比
            label43.Text = (facewidth / eyebrowtochin).ToString("#0.00") + " : 1";//臉寬除以眉毛至下巴的距離
            label32.Text = ((double)(eyebrowcenterpoint.Y - facetop.Y) / (double)(facebutton.Y - facetop.Y)).ToString("#0.00");
            label33.Text = ((double)(centernose.Y - eyebrowcenterpoint.Y) / (double)(facebutton.Y - facetop.Y)).ToString("#0.00");
            label34.Text = ((double)(facebutton.Y - centernose.Y) / (double)(facebutton.Y - facetop.Y)).ToString("#0.00");

            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            pictureBox1.Visible = true;
            panel2.Visible = true;
            panel3.Visible = true;
            panel4.Visible = true;
            toolStripProgressBar1.Visible = false;
            //========================頭髮==============================//
            try
            {
                Image<Bgr, byte> hair2 = My_Image2.Clone();
                Rectangle rh = new Rectangle(facesori.X, facesori.Y - 130, facesori.Width, 170);
                hair.ROI = rh;

                //取得灰階影像
                Image<Gray, byte> grayImage = new Image<Gray, byte>(hair.Bitmap);
                //二值化的閥值
                Gray thresholdValue = new Gray(200);
                //取得二值化影像
                Image<Gray, byte> thresholdImage = grayImage.ThresholdBinary(thresholdValue, new Gray(255));
                Image<Bgr, byte> thresholdImagebgr = thresholdImage.Convert<Bgr, Byte>();
                Point kkk = new Point((rh.X + rh.Width + rh.X) / 2, rh.Y);

                Bitmap thresholdImagebm = thresholdImagebgr.Bitmap;
                int nw;
                int nh;
                int pointy = 0;
                int pointy2 = 0;
                nw = thresholdImagebm.Width;
                nh = thresholdImagebm.Height;
                Bitmap abc = new Bitmap(nw, nh, thresholdImagebm.PixelFormat);
                for (int y = nh - 1; y > 0; y--)
                {

                    Color sColor = thresholdImagebm.GetPixel(nw / 2, y);
                    if (sColor.R == 0)
                    {
                        pointy = y;
                    }

                }

                for (int y = 0; y < nh; y++)
                {

                    Color sColor = thresholdImagebm.GetPixel(nw / 2, y);
                    if (sColor.R == 0)
                    {
                        pointy2 = y;
                    }

                }

                Point bbb = new Point(nw / 2, pointy);
                Point ccc = new Point(nw / 2, pointy2);
                Graphics gaa = Graphics.FromImage(thresholdImagebm);
                SolidBrush db = new SolidBrush(Color.Red);
                SolidBrush dbg = new SolidBrush(Color.Green);
                StringFormat sf3 = new StringFormat();//設定string置中，drawString才不會錯位
                sf3.Alignment = StringAlignment.Center;
                sf3.LineAlignment = StringAlignment.Center;
                gaa.DrawString("+", new Font("Arial", 25), db, bbb.X, bbb.Y, sf3);
                gaa.DrawString("+", new Font("Arial", 25), dbg, ccc.X, ccc.Y, sf3);
                gaa.Dispose();
                thresholdImagebgr.Bitmap = thresholdImagebm;

                int dy = pointy2 - pointy;
                double fwdy = dy / faceheight;//頭髮與臉的比例
                //imageBox2.Image = thresholdImagebgr;
                //============================================================================================================================================//

                Bitmap drawface = new Bitmap(pictureBox1.Image = Image.FromFile(Application.StartupPath + "\\square_face1.jpg"));
                StringFormat ds = new StringFormat();//設定string置中，drawString才不會錯位
                ds.Alignment = StringAlignment.Center;
                ds.LineAlignment = StringAlignment.Center;
                SolidBrush drawBrushGreen = new SolidBrush(Color.Green);
                SolidBrush drawBrushRed = new SolidBrush(Color.Red);

                Graphics gr = Graphics.FromImage(drawface);//畫上曲線
                Pen pr = new Pen(Color.FromArgb(255, 255, 0, 0), 3);
                Pen pg = new Pen(Color.FromArgb(255, 0, 255, 0), 3);
                //int Xsize = 22;//X記號大小

                //g.DrawLine(pen, 50, 180, 400, 180);
                //gr.DrawLine(pg, 224, 180, 224, 595);//臉高
                //                                    //三庭
                //gr.DrawLine(pg, 30, 180, 550, 180);//髮際線
                //gr.DrawLine(pg, 30, 318, 550, 318);//眉中心
                //gr.DrawLine(pg, 30, 449, 550, 449);//鼻中心
                //gr.DrawLine(pg, 30, 595, 550, 595);//下巴
                //gr.DrawString("1/3", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 525, 250, ds);//上臉1/3
                //gr.DrawString("1/3", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 525, 385, ds);//中臉1/3
                //gr.DrawString("1/3", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 525, 525, ds);//下臉1/3
                //                                                                                             //五眼
                //gr.DrawLine(pr, 48, 50, 48, 685);//1/5
                //gr.DrawLine(pr, 107, 50, 107, 685);
                //gr.DrawLine(pr, 187, 50, 187, 685);
                //gr.DrawLine(pr, 267, 50, 267, 685);
                //gr.DrawLine(pr, 342, 50, 342, 685);
                //gr.DrawLine(pr, 399, 50, 399, 685);
                //gr.DrawString("1/5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 80, 667, ds);
                //gr.DrawString("1/5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 148, 667, ds);
                //gr.DrawString("1/5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 224, 667, ds);
                //gr.DrawString("1/5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 305, 667, ds);
                //gr.DrawString("1/5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 374, 667, ds);
                //gr.DrawString("5", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 187, 357, ds);//右眼眼頭
                //gr.DrawString("6", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 267, 357, ds);//左眼眼頭
                //gr.DrawString("7", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 107, 346, ds);//右眼眼尾
                //gr.DrawString("8", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 48, 347, ds);//右眼髮際線
                //gr.DrawString("9", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 342, 349, ds);//左眼眼尾
                //gr.DrawString("10", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 399, 347, ds);//左眼髮際線

                //gr.DrawString("1", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 224, 180, ds);//髮際
                //gr.DrawString("11", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 192, 318, ds);//右眉眉頭
                //gr.DrawString("12", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 258, 318, ds);//左眉眉頭
                //gr.DrawString("2", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 225, 318, ds);//兩眉中心
                //gr.DrawString("13", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 84, 309, ds);//右眉眉尾
                //gr.DrawString("14", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 365, 312, ds);//左眉眉尾
                //gr.DrawString("15", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 110, 290, ds);//右眉眉峰
                //gr.DrawString("16", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 342, 296, ds);//左眉眉峰
                //gr.DrawString("3", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 224, 449, ds);//鼻中心
                //gr.DrawString("17", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 188, 440, ds);//右鼻翼
                //gr.DrawString("18", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 263, 440, ds);//左鼻翼
                //gr.DrawString("4", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 224, 595, ds);//下巴中心
                //gr.DrawString("19", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 205, 482, ds);//右唇峰
                //gr.DrawString("20", new Font("Arial", Xsize, FontStyle.Bold), drawBrushRed, 246, 482, ds);//左唇峰
                //gr.DrawString("20", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 163, 492, ds);//右唇角
                //gr.DrawString("21", new Font("Arial", Xsize, FontStyle.Bold), drawBrushGreen, 291, 493, ds);//左唇角





                gr.Dispose();



                ///////////////////////////////////////////全新處//////////////////////////////////////////////////////////////////////
                Image<Bgr, byte> img = new Image<Bgr, Byte>(drawface);//整圖
                Image<Bgr, byte> upspace = new Image<Bgr, Byte>(drawface);//頭髮上面空白處
                Image<Bgr, byte> imgupup = new Image<Bgr, Byte>(drawface);//頭髮               
                Image<Bgr, byte> imgupface = new Image<Bgr, Byte>(drawface);//上臉
                Image<Bgr, byte> imgmidface = new Image<Bgr, Byte>(drawface);//中臉
                Image<Bgr, byte> imgdownface = new Image<Bgr, Byte>(drawface);//下臉
                Image<Bgr, byte> imgdownspace = new Image<Bgr, Byte>(drawface);//下面空白處
                Image<Bgr, byte> upfacespace = new Image<Bgr, Byte>(drawface);//左耳上臉空白處
                Image<Bgr, byte> leftear = new Image<Bgr, Byte>(drawface);//左耳
                Image<Bgr, byte> downfacespace = new Image<Bgr, Byte>(drawface);//左耳下臉空白處
                Image<Bgr, byte> upfacespace2 = new Image<Bgr, Byte>(drawface);//右耳上臉空白處
                Image<Bgr, byte> rightear = new Image<Bgr, Byte>(drawface);//右耳
                Image<Bgr, byte> downfacespace2 = new Image<Bgr, Byte>(drawface);//右耳下臉空白處
                Image<Bgr, byte> spacefree = new Image<Bgr, Byte>(drawface);//空白
                Image<Bgr, byte> spacefree2 = new Image<Bgr, Byte>(drawface);//空白
                Image<Bgr, byte> dnspace = new Image<Bgr, Byte>(drawface);//空白

                int upfaceratio = (int)(((double)(eyebrowcenterpoint.Y - facetop.Y) / (double)(facebutton.Y - facetop.Y)) * 100);//114 406 28
                int midfaceratio = (int)(((double)(centernose.Y - eyebrowcenterpoint.Y) / (double)(facebutton.Y - facetop.Y)) * 100);
                int downfaceratio = (int)(((double)(facebutton.Y - centernose.Y) / (double)(facebutton.Y - facetop.Y)) * 100);

                int nnnnnn = 415 * upfaceratio / 10; int nnnnnn1 = nnnnnn % 10; if (nnnnnn1 >= 5) { nnnnnn = nnnnnn / 10 + 1; } else { nnnnnn = nnnnnn / 10; }
                int mmmmmm = 415 * midfaceratio / 10; int mmmmmm1 = mmmmmm % 10; if (mmmmmm1 >= 5) { mmmmmm = mmmmmm / 10 + 1; } else { mmmmmm = mmmmmm / 10; }
                int llllll = 415 * downfaceratio / 10; int llllll1 = llllll % 10; if (llllll1 >= 5) { llllll = llllll / 10 + 1; } else { llllll = llllll / 10; }

                upspace.ROI = new Rectangle(48, 0, 351, 70);//頭髮上面空白處
                imgupup.ROI = new Rectangle(48, 70, 351, 110);//頭髮                
                imgupface.ROI = new Rectangle(0, 180, 447, 138);//上臉
                imgmidface.ROI = new Rectangle(0, 318, 447, 131);//中臉
                imgdownface.ROI = new Rectangle(0, 449, 447, 146);//下臉
                imgdownspace.ROI = new Rectangle(0, 595, 447, 20);//下面空白處
                upfacespace.ROI = new Rectangle(0, 180, 48, 138);//上臉空白處
                leftear.ROI = new Rectangle(0, 318, 48, 131);//左耳
                downfacespace.ROI = new Rectangle(0, 449, 48, 146);//下臉空白處
                spacefree.ROI = new Rectangle(0, 0, 48, 110);//空白
                spacefree2.ROI = new Rectangle(0, 0, 48, 90);//空白
                upfacespace2.ROI = new Rectangle(399, 180, 48, 138);//上臉空白處
                rightear.ROI = new Rectangle(399, 318, 48, 131);//左耳
                downfacespace2.ROI = new Rectangle(399, 449, 48, 146);//下臉空白處
                dnspace.ROI = new Rectangle(48, 0, 351, 70);//下臉空白處


                imgupup = imgupup.Resize(imgupup.Width, dy, Inter.Linear);
                imgupface = imgupface.Resize(imgupface.Width, nnnnnn, Inter.Linear);//116
                imgmidface = imgmidface.Resize(imgmidface.Width, mmmmmm, Inter.Linear);//141
                imgdownface = imgdownface.Resize(imgdownface.Width, llllll, Inter.Linear);//154
                upfacespace = upfacespace.Resize(upfacespace.Width, nnnnnn, Inter.Linear);
                leftear = leftear.Resize(leftear.Width, mmmmmm, Inter.Linear);
                downfacespace = downfacespace.Resize(downfacespace.Width, llllll, Inter.Linear);
                upfacespace2 = upfacespace2.Resize(upfacespace2.Width, nnnnnn, Inter.Linear);
                rightear = rightear.Resize(rightear.Width, mmmmmm, Inter.Linear);
                downfacespace2 = downfacespace2.Resize(downfacespace2.Width, llllll, Inter.Linear);
                spacefree = spacefree.Resize(spacefree.Width, dy, Inter.Linear);


                Bitmap rrrr = imgdownspace.ToBitmap();
                pictureBox1.Image = rrrr;
                /////////////////////////////////////////////////////////////////////////上臉+中臉
                Image MergedImage100 = default(Image);
                Int32 Wide100 = 0;
                Int32 High100 = 0;
                High100 = imgupface.Height + imgmidface.Height;//設定高度          
                if (imgupface.Width >= imgmidface.Width)
                {
                    Wide100 = imgupface.Width;
                }
                else
                {
                    Wide100 = imgmidface.Width;
                }
                Bitmap mybmp100 = new Bitmap(Wide100, High100);
                Graphics gw100 = Graphics.FromImage(mybmp100);
                //處理第一張圖片
                Bitmap imgmidfacebmm = imgmidface.ToBitmap();
                Bitmap imgupfacebmm = imgupface.ToBitmap();
                gw100.DrawImage(imgupfacebmm, 0, 0);
                //處理第二張圖片

                gw100.DrawImage(imgmidfacebmm, 0, imgupfacebmm.Height);
                MergedImage100 = mybmp100;
                gw100.Dispose();
                //pictureBox1.Image = MergedImage100;
                /////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////+下臉
                Image MergedImage101 = default(Image);
                Int32 Wide101 = 0;
                Int32 High101 = 0;
                High101 = mybmp100.Height + imgdownface.Height;//設定高度          
                if (mybmp100.Width >= imgdownface.Width)
                {
                    Wide101 = mybmp100.Width;
                }
                else
                {
                    Wide101 = imgdownface.Width;
                }
                Bitmap mybmp101 = new Bitmap(Wide101, High101);
                Graphics gw101 = Graphics.FromImage(mybmp101);
                //處理第一張圖片

                Bitmap imgdownfacebmm = imgdownface.ToBitmap();
                gw101.DrawImage(mybmp100, 0, 0);
                //處理第二張圖片

                gw101.DrawImage(imgdownfacebmm, 0, mybmp100.Height);
                MergedImage101 = mybmp101;
                gw101.Dispose();
                //pictureBox1.Image = MergedImage101;
                /////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////+下面空白處
                Image MergedImage102 = default(Image);
                Int32 Wide102 = 0;
                Int32 High102 = 0;
                High102 = mybmp101.Height + imgdownspace.Height;//設定高度          
                if (mybmp101.Width >= imgdownspace.Width)
                {
                    Wide102 = mybmp101.Width;
                }
                else
                {
                    Wide102 = imgdownspace.Width;
                }
                Bitmap mybmp102 = new Bitmap(Wide102, High102);
                Graphics gw102 = Graphics.FromImage(mybmp102);
                //處理第一張圖片

                Bitmap imgdownspacebmm = imgdownspace.ToBitmap();
                gw102.DrawImage(mybmp101, 0, 0);
                //處理第二張圖片

                gw102.DrawImage(imgdownspacebmm, 0, mybmp101.Height);
                MergedImage102 = mybmp102;
                gw102.Dispose();
                // pictureBox1.Image = mybmp102;
                /////////////////////////////////////////////////////////////////////////上耳+中耳(左)
                Image MergedImage108 = default(Image);
                Int32 Wide108 = 0;
                Int32 High108 = 0;
                High108 = upfacespace.Height + leftear.Height;//設定高度          
                if (upfacespace.Width >= leftear.Width)
                {
                    Wide108 = upfacespace.Width;
                }
                else
                {
                    Wide108 = leftear.Width;
                }
                Bitmap mybmp108 = new Bitmap(Wide108, High108);
                Graphics gw108 = Graphics.FromImage(mybmp108);
                //處理第一張圖片

                Bitmap upfacespacebmm = upfacespace.ToBitmap();
                Bitmap leftearbmm = leftear.ToBitmap();
                gw108.DrawImage(upfacespacebmm, 0, 0);
                //處理第二張圖片

                gw108.DrawImage(leftearbmm, 0, upfacespacebmm.Height);
                MergedImage108 = mybmp108;
                gw108.Dispose();
                //pictureBox1.Image = mybmp108;
                /////////////////////////////////////////////////////////////////////////上耳+中耳+下耳(左)
                Image MergedImage109 = default(Image);
                Int32 Wide109 = 0;
                Int32 High109 = 0;
                High109 = mybmp108.Height + downfacespace.Height;//設定高度          
                if (mybmp108.Width >= downfacespace.Width)
                {
                    Wide109 = mybmp108.Width;
                }
                else
                {
                    Wide109 = downfacespace.Width;
                }
                Bitmap mybmp109 = new Bitmap(Wide109, High109);
                Graphics gw109 = Graphics.FromImage(mybmp109);
                //處理第一張圖片


                Bitmap downfacespacebmm = downfacespace.ToBitmap();
                gw109.DrawImage(mybmp108, 0, 0);
                //處理第二張圖片

                gw109.DrawImage(downfacespacebmm, 0, mybmp108.Height);
                MergedImage109 = mybmp109;
                gw109.Dispose();
                //pictureBox1.Image = mybmp109;
                /////////////////////////////////////////////////////////////////////////上耳+中耳(右)
                //Image MergedImage112 = default(Image);
                Int32 Wide112 = 0;
                Int32 High112 = 0;
                High112 = upfacespace2.Height + rightear.Height;//設定高度          
                if (upfacespace2.Width >= rightear.Width)
                {
                    Wide112 = upfacespace2.Width;
                }
                else
                {
                    Wide112 = rightear.Width;
                }
                Bitmap mybmp112 = new Bitmap(Wide112, High112);
                Graphics gw112 = Graphics.FromImage(mybmp112);
                //處理第一張圖片

                Bitmap upfacespace2bmm = upfacespace2.ToBitmap();
                Bitmap rightearbmm = rightear.ToBitmap();
                gw112.DrawImage(upfacespace2bmm, 0, 0);
                //處理第二張圖片

                gw112.DrawImage(rightearbmm, 0, upfacespace2bmm.Height);

                gw112.Dispose();
                //pictureBox1.Image = mybmp112;
                /////////////////////////////////////////////////////////////////////////上耳+中耳+下耳(右)
                //Image MergedImage113 = default(Image);
                Int32 Wide113 = 0;
                Int32 High113 = 0;
                High113 = mybmp112.Height + downfacespace2.Height;//設定高度          
                if (mybmp112.Width >= downfacespace2.Width)
                {
                    Wide113 = mybmp112.Width;
                }
                else
                {
                    Wide113 = downfacespace2.Width;
                }
                Bitmap mybmp113 = new Bitmap(Wide113, High113);
                Graphics gw113 = Graphics.FromImage(mybmp113);
                //處理第一張圖片


                Bitmap downfacespace2bmm = downfacespace2.ToBitmap();
                gw113.DrawImage(mybmp112, 0, 0);
                //處理第二張圖片

                gw113.DrawImage(downfacespace2bmm, 0, mybmp112.Height);

                gw113.Dispose();
                // pictureBox1.Image = mybmp113;
                /////////////////////////////////////////////////////////////////////////
                //Image<Bgr, byte> bgrmy102 = new Image<Bgr, Byte>(mybmp102);
                //bgrmy102 = bgrmy102.Resize(bgrmy102.Width, 435, Inter.Linear);
                //Bitmap mybmp102bmm = bgrmy102.ToBitmap();
                /////////////////////////////////////////////////////////////////////////
                Image<Bgr, byte> eyebrow1 = new Image<Bgr, Byte>(mybmp102);//左眉1
                Image<Bgr, byte> eyebrow2 = new Image<Bgr, Byte>(mybmp102);//左眉2
                Image<Bgr, byte> eyebrow3 = new Image<Bgr, Byte>(mybmp102);//左眉3
                Image<Bgr, byte> eyebrow4 = new Image<Bgr, Byte>(mybmp102);//右眉1
                Image<Bgr, byte> eyebrow5 = new Image<Bgr, Byte>(mybmp102);//右眉2
                Image<Bgr, byte> eyebrow6 = new Image<Bgr, Byte>(mybmp102);//右眉3

                Image<Bgr, byte> nose2 = new Image<Bgr, Byte>(mybmp102);//左鼻
                Image<Bgr, byte> nose3 = new Image<Bgr, Byte>(mybmp102);//右鼻

                Image<Bgr, byte> lip1 = new Image<Bgr, Byte>(mybmp102);//左嘴
                Image<Bgr, byte> lip2 = new Image<Bgr, Byte>(mybmp102);//右嘴

                eyebrow1.ROI = new Rectangle(48, 0, 36, imgupface.Height);//左眉1
                eyebrow2.ROI = new Rectangle(84, 0, 104, imgupface.Height);//左眉2
                eyebrow3.ROI = new Rectangle(188, 0, 36, imgupface.Height);//左眉3
                eyebrow4.ROI = new Rectangle(224, 0, 36, imgupface.Height);//右眉1
                eyebrow5.ROI = new Rectangle(260, 0, 104, imgupface.Height);//右眉2
                eyebrow6.ROI = new Rectangle(364, 0, 36, imgupface.Height);//右眉3

                nose2.ROI = new Rectangle(48, imgupface.Height, 176, imgmidface.Height);//左鼻
                nose3.ROI = new Rectangle(224, imgupface.Height, 176, imgmidface.Height);//右鼻

                lip1.ROI = new Rectangle(48, imgupface.Height + imgmidface.Height, 176, imgdownface.Height);//左嘴
                lip2.ROI = new Rectangle(224, imgupface.Height + imgmidface.Height, 176, imgdownface.Height);//右嘴

                lipspoint lp = new lipspoint();
                lp = MyFace.lip(My_Image2, centernose, faces);
                Face.lip(My_Image2, centernose, faces);

                int lefteyebrowratio1 = (int)(((double)(Righteyebrowouterpoint.X - faceleft.X) / (double)(ebp.centerpoint.X - faceleft.X)) * 100);//18
                int lefteyebrowratio2 = (int)(((double)(Righteyebrowinnerpoint.X - Righteyebrowouterpoint.X) / (double)(ebp.centerpoint.X - faceleft.X)) * 100);//87 138
                int lefteyebrowratio3 = (int)(((double)(ebp.centerpoint.X - Righteyebrowinnerpoint.X) / (double)(ebp.centerpoint.X - faceleft.X)) * 100);//33
                int righteyebrowratio1 = (int)(((double)(Lefteyebrowinnerpoint.X - ebp.centerpoint.X) / (double)(faceright.X - ebp.centerpoint.X)) * 100);//56
                int righteyebrowratio2 = (int)(((double)(Lefteyebrowouterpoint.X - Lefteyebrowinnerpoint.X) / (double)(faceright.X - ebp.centerpoint.X)) * 100);//39
                int righteyebrowratio3 = (int)(((double)(faceright.X - Lefteyebrowouterpoint.X) / (double)(faceright.X - ebp.centerpoint.X)) * 100);//64

                int leftnoseratio = (int)(((double)(centernose.X - faceleft.X) / (double)(faceright.X - faceleft.X)) * 100);//鼻子左半邊比例
                int rightnoseratio = (int)(((double)(faceright.X - centernose.X) / (double)(faceright.X - faceleft.X)) * 100);//鼻子右半邊比例

                int leftlipratio = (int)(((double)((lp.leftlipspoint.X + lp.rightlipspoint.X) / 2 - faceleft.X) / (double)(faceright.X - faceleft.X)) * 100);//嘴唇左半邊比例
                int rightlipratio = (int)(((double)(faceright.X - (lp.leftlipspoint.X + lp.rightlipspoint.X) / 2) / (double)(faceright.X - faceleft.X)) * 100);//嘴唇右半邊比例

                int ffff = 176 * lefteyebrowratio1 / 10; int ffff1 = ffff % 10; if (ffff1 >= 5) { ffff = ffff / 10 + 1; } else { ffff = ffff / 10; }//23
                int gggggg = 176 * lefteyebrowratio2 / 10; int gggggg1 = gggggg % 10; if (gggggg1 >= 5) { gggggg = gggggg / 10 + 1; } else { gggggg = gggggg / 10; }//110
                int jjjjjjjj = 176 * lefteyebrowratio3 / 10; int jjjjjjjj1 = jjjjjjjj % 10; if (jjjjjjjj1 >= 5) { jjjjjjjj = jjjjjjjj / 10 + 1; } else { jjjjjjjj = jjjjjjjj / 10; }///40
                int uuuuu = 176 * righteyebrowratio1 / 10; int uuuuu1 = uuuuu % 10; if (uuuuu1 >= 5) { uuuuu = uuuuu / 10 + 1; } else { uuuuu = uuuuu / 10; }//62
                int kkkkk = 176 * righteyebrowratio2 / 10; int kkkkk1 = kkkkk % 10; if (kkkkk1 >= 5) { kkkkk = kkkkk / 10 + 1; } else { kkkkk = kkkkk / 10; }//42
                int ooooo = 176 * righteyebrowratio3 / 10; int ooooo1 = ooooo % 10; if (ooooo1 >= 5) { ooooo = ooooo / 10 + 1; } else { ooooo = ooooo / 10; }//70

                int aaaaaa = 352 * leftnoseratio / 10; int aaaaaa1 = aaaaaa % 10; if (aaaaaa1 >= 5) { aaaaaa = aaaaaa / 10 + 1; } else { aaaaaa = aaaaaa / 10; }//70
                int bbbbbb = 352 * rightnoseratio / 10; int bbbbbb1 = bbbbbb % 10; if (bbbbbb1 >= 5) { bbbbbb = bbbbbb / 10 + 1; } else { bbbbbb = bbbbbb / 10; }//70

                int cccccc = 352 * leftlipratio / 10; int cccccc1 = cccccc % 10; if (cccccc1 >= 5) { cccccc = cccccc / 10 + 1; } else { cccccc = cccccc / 10; }//70
                int dddddd = 352 * rightlipratio / 10; int dddddd1 = dddddd % 10; if (dddddd1 >= 5) { dddddd = dddddd / 10 + 1; } else { dddddd = dddddd / 10; }//70

                eyebrow1 = eyebrow1.Resize(ffff, eyebrow1.Height, Inter.Linear);//22
                eyebrow2 = eyebrow2.Resize(gggggg, eyebrow2.Height, Inter.Linear);//110
                eyebrow3 = eyebrow3.Resize(jjjjjjjj, eyebrow3.Height, Inter.Linear);//40
                eyebrow4 = eyebrow4.Resize(uuuuu, eyebrow4.Height, Inter.Linear);
                eyebrow5 = eyebrow5.Resize(kkkkk, eyebrow5.Height, Inter.Linear);
                eyebrow6 = eyebrow6.Resize(ooooo, eyebrow6.Height, Inter.Linear);

                nose2 = nose2.Resize(aaaaaa, nose2.Height, Inter.Linear);
                nose3 = nose3.Resize(bbbbbb, nose3.Height, Inter.Linear);

                lip1 = lip1.Resize(cccccc, lip1.Height, Inter.Linear);
                lip2 = lip2.Resize(dddddd, lip2.Height, Inter.Linear);

                Bitmap sss = nose3.ToBitmap();
                //pictureBox1.Image = sss;

                ///////////////////////////////////////////////////////////////////////////////////////////////////合左眉
                Image MR200 = default(Image);
                Int32 WD200 = 0;
                Int32 HH200 = 0;
                WD200 = eyebrow1.Width + eyebrow2.Width;//設定寬度           
                if (eyebrow1.Height >= eyebrow2.Height)
                {
                    HH200 = eyebrow1.Height;
                }
                else
                {
                    HH200 = eyebrow2.Height;
                }
                Bitmap bi200 = new Bitmap(WD200, HH200);
                Graphics gs200 = Graphics.FromImage(bi200);
                //處理第一張圖片

                Bitmap eyebrow1bmm = eyebrow1.ToBitmap();
                Bitmap eyebrow2bmm = eyebrow2.ToBitmap();


                gs200.DrawImage(eyebrow1bmm, 0, 0);
                //處理第二張圖片
                gs200.DrawImage(eyebrow2bmm, eyebrow1bmm.Width, 0);
                MR200 = bi200;
                gs200.Dispose();
                //pictureBox1.Image = bi200;
                ///////////////////////////////////////////////////////////////////////////////////////////////////合左眉
                Image MR201 = default(Image);
                Int32 WD201 = 0;
                Int32 HH201 = 0;
                WD201 = bi200.Width + eyebrow3.Width;//設定寬度           
                if (bi200.Height >= eyebrow3.Height)
                {
                    HH201 = bi200.Height;
                }
                else
                {
                    HH201 = eyebrow3.Height;
                }
                Bitmap bi201 = new Bitmap(WD201, HH201);
                Graphics gs201 = Graphics.FromImage(bi201);
                //處理第一張圖片


                Bitmap eyebrow3bmm = eyebrow3.ToBitmap();


                gs201.DrawImage(bi200, 0, 0);
                //處理第二張圖片
                gs201.DrawImage(eyebrow3bmm, bi200.Width, 0);
                MR201 = bi201;
                gs201.Dispose();
                //pictureBox1.Image = bi201;
                ///////////////////////////////////////////////////////////////////////////////////////////////////合右眉
                Image MR202 = default(Image);
                Int32 WD202 = 0;
                Int32 HH202 = 0;
                WD202 = bi201.Width + eyebrow4.Width;//設定寬度           
                if (bi201.Height >= eyebrow4.Height)
                {
                    HH202 = bi201.Height;
                }
                else
                {
                    HH202 = eyebrow4.Height;
                }
                Bitmap bi202 = new Bitmap(WD202, HH202);
                Graphics gs202 = Graphics.FromImage(bi202);
                //處理第一張圖片


                Bitmap eyebrow4bmm = eyebrow4.ToBitmap();


                gs202.DrawImage(bi201, 0, 0);
                //處理第二張圖片
                gs202.DrawImage(eyebrow4bmm, bi201.Width, 0);
                MR202 = bi202;
                gs202.Dispose();
                //pictureBox1.Image = bi202;
                ///////////////////////////////////////////////////////////////////////////////////////////////////合右眉
                Image MR203 = default(Image);
                Int32 WD203 = 0;
                Int32 HH203 = 0;
                WD203 = bi202.Width + eyebrow5.Width;//設定寬度           
                if (bi202.Height >= eyebrow5.Height)
                {
                    HH203 = bi202.Height;
                }
                else
                {
                    HH203 = eyebrow5.Height;
                }
                Bitmap bi203 = new Bitmap(WD203, HH203);
                Graphics gs203 = Graphics.FromImage(bi203);
                //處理第一張圖片


                Bitmap eyebrow5bmm = eyebrow5.ToBitmap();


                gs203.DrawImage(bi202, 0, 0);
                //處理第二張圖片
                gs203.DrawImage(eyebrow5bmm, bi202.Width, 0);
                MR203 = bi203;
                gs203.Dispose();
                // pictureBox1.Image = bi203;
                ///////////////////////////////////////////////////////////////////////////////////////////////////合右眉
                Image MR204 = default(Image);
                Int32 WD204 = 0;
                Int32 HH204 = 0;
                WD204 = bi203.Width + eyebrow6.Width;//設定寬度           
                if (bi203.Height >= eyebrow6.Height)
                {
                    HH204 = bi203.Height;
                }
                else
                {
                    HH204 = eyebrow6.Height;
                }
                Bitmap bi204 = new Bitmap(WD204, HH204);
                Graphics gs204 = Graphics.FromImage(bi204);
                //處理第一張圖片


                Bitmap eyebrow6bmm = eyebrow6.ToBitmap();


                gs204.DrawImage(bi203, 0, 0);
                //處理第二張圖片
                gs204.DrawImage(eyebrow6bmm, bi203.Width, 0);
                MR204 = bi204;
                gs204.Dispose();

                //Image<Bgr, byte> bi204bgr = new Image<Bgr, Byte>(bi204);
                //bi204bgr = bi204bgr.Resize(352, bi204bgr.Height, Inter.Linear);
                //Bitmap bi204bgrbmm = bi204bgr.ToBitmap();
                //pictureBox1.Image = bi204;
                label41.Text = (WD204).ToString("#0.00") + " mm";//臉的寬度
                ///////////////////////////////////////////////////////////////////////////////////////////////////合鼻子
                Image MR205 = default(Image);
                Int32 WD205 = 0;
                Int32 HH205 = 0;
                WD205 = nose2.Width + nose3.Width;//設定寬度           
                if (nose2.Height >= nose3.Height)
                {
                    HH205 = nose2.Height;
                }
                else
                {
                    HH205 = nose3.Height;
                }
                Bitmap bi205 = new Bitmap(WD205, HH205);
                Graphics gs205 = Graphics.FromImage(bi205);
                //處理第一張圖片


                Bitmap nose2bmm = nose2.ToBitmap();
                Bitmap nose3bmm = nose3.ToBitmap();


                gs205.DrawImage(nose2bmm, 0, 0);
                //處理第二張圖片
                gs205.DrawImage(nose3bmm, nose2bmm.Width, 0);
                MR205 = bi205;
                gs205.Dispose();

                //Image<Bgr, byte> bi205bgr = new Image<Bgr, Byte>(bi205);
                //bi205bgr = bi205bgr.Resize(352, bi205bgr.Height, Inter.Linear);
                //Bitmap bi205bgrbmm = bi205bgr.ToBitmap();
                //pictureBox1.Image = bi205;
                ///////////////////////////////////////////////////////////////////////////////////////////////////合嘴巴
                Image MR206 = default(Image);
                Int32 WD206 = 0;
                Int32 HH206 = 0;
                WD206 = lip1.Width + lip2.Width;//設定寬度           
                if (lip1.Height >= lip2.Height)
                {
                    HH206 = lip1.Height;
                }
                else
                {
                    HH206 = lip2.Height;
                }
                Bitmap bi206 = new Bitmap(WD206, HH206);
                Graphics gs206 = Graphics.FromImage(bi206);
                //處理第一張圖片


                Bitmap lip1bmm = lip1.ToBitmap();
                Bitmap lip2bmm = lip2.ToBitmap();


                gs206.DrawImage(lip1bmm, 0, 0);
                //處理第二張圖片
                gs206.DrawImage(lip2bmm, lip1bmm.Width, 0);
                MR206 = bi206;
                gs206.Dispose();
                //pictureBox1.Image = bi206;
                /////////////////////////////////////////////////////////////////////////上臉+中臉
                Image MergedImage103 = default(Image);
                Int32 Wide103 = 0;
                Int32 High103 = 0;
                High103 = bi204.Height + bi205.Height;//設定高度          
                if (bi204.Width >= bi205.Width)
                {
                    Wide103 = bi204.Width;
                }
                else
                {
                    Wide103 = bi205.Width;
                }


                Bitmap mybmp103 = new Bitmap(Wide103, High103);
                Graphics gw103 = Graphics.FromImage(mybmp103);
                //處理第一張圖片                
                gw103.DrawImage(bi204, 0, 0);
                //處理第二張圖片

                gw103.DrawImage(bi205, 0, bi204.Height);
                MergedImage103 = mybmp103;
                gw103.Dispose();
                //pictureBox1.Image = mybmp103;
                /////////////////////////////////////////////////////////////////////////上中臉+下臉
                Image MergedImage104 = default(Image);
                Int32 Wide104 = 0;
                Int32 High104 = 0;
                High104 = mybmp103.Height + bi206.Height;//設定高度          
                if (mybmp103.Width >= bi206.Width)
                {
                    Wide104 = mybmp103.Width;
                }
                else
                {
                    Wide104 = bi206.Width;
                }


                Bitmap mybmp104 = new Bitmap(Wide104, High104);
                Graphics gw104 = Graphics.FromImage(mybmp104);
                //處理第一張圖片                
                gw104.DrawImage(mybmp103, 0, 0);
                //處理第二張圖片

                gw104.DrawImage(bi206, 0, mybmp103.Height);
                MergedImage104 = mybmp104;
                gw104.Dispose();
                //pictureBox1.Image = mybmp104;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////
                Image<Bgr, byte> mybmp104bgr = new Image<Bgr, Byte>(mybmp104);
                int rightleftx = faceright.X - faceleft.X;
                int buttontopy = facebutton.Y - facetop.Y;
                mybmp104bgr = mybmp104bgr.Resize(rightleftx, buttontopy, Inter.Linear);
                Bitmap mybmp104bgrbmm = mybmp104bgr.ToBitmap();
                //pictureBox1.Image = mybmp104bgrbmm;
                label32.Text = (imgupup.Height).ToString("#0.00");//406 411
                imgupup = imgupup.Resize(mybmp104bgrbmm.Width, imgupup.Height, Inter.Linear);
                upspace = upspace.Resize(mybmp104bgrbmm.Width, upspace.Height, Inter.Linear);

                label33.Text = (imgupup.Height).ToString("#0.00");//297 348
                label34.Text = (spacefree.Height).ToString("#0.00");





                /////////////////////////////////////////////////////////////////////////頭髮+整臉
                Image MergedImage105 = default(Image);
                Int32 Wide105 = 0;
                Int32 High105 = 0;
                High105 = imgupup.Height + mybmp104bgrbmm.Height;//設定高度          
                if (imgupup.Width >= mybmp104bgrbmm.Width)
                {
                    Wide105 = imgupup.Width;
                }
                else
                {
                    Wide105 = mybmp104bgrbmm.Width;
                }
                Bitmap mybmp105 = new Bitmap(Wide105, High105);
                Graphics gw105 = Graphics.FromImage(mybmp105);
                //處理第一張圖片   
                Bitmap imgupupbmm = imgupup.ToBitmap();
                gw105.DrawImage(imgupupbmm, 0, 0);
                //處理第二張圖片

                gw105.DrawImage(mybmp104bgrbmm, 0, imgupupbmm.Height);
                MergedImage105 = mybmp105;
                gw105.Dispose();
                //pictureBox1.Image = mybmp105;
                /////////////////////////////////////////////////////////////////////////上面空白+頭髮+整臉
                Image MergedImage106 = default(Image);
                Int32 Wide106 = 0;
                Int32 High106 = 0;
                High106 = upspace.Height + mybmp105.Height;//設定高度          
                if (upspace.Width >= mybmp105.Width)
                {
                    Wide106 = upspace.Width;
                }
                else
                {
                    Wide106 = mybmp105.Width;
                }
                Bitmap mybmp106 = new Bitmap(Wide106, High106);
                Graphics gw106 = Graphics.FromImage(mybmp106);
                //處理第一張圖片   
                Bitmap upspacebmm = upspace.ToBitmap();
                gw106.DrawImage(upspacebmm, 0, 0);
                //處理第二張圖片

                gw106.DrawImage(mybmp105, 0, upspacebmm.Height);
                MergedImage106 = mybmp106;
                gw106.Dispose();
                // pictureBox1.Image = mybmp106;
                /////////////////////////////////////////////////////////////////////////+下面空白
                //Image MergedImage107 = default(Image);
                Int32 Wide107 = 0;
                Int32 High107 = 0;
                High107 = mybmp106.Height + upspace.Height;//設定高度          
                if (mybmp106.Width >= upspace.Width)
                {
                    Wide107 = mybmp106.Width;
                }
                else
                {
                    Wide107 = upspace.Width;
                }
                Bitmap mybmp107 = new Bitmap(Wide107, High107);
                Graphics gw107 = Graphics.FromImage(mybmp107);
                //處理第一張圖片   

                gw107.DrawImage(mybmp106, 0, 0);
                //處理第二張圖片

                gw107.DrawImage(upspacebmm, 0, mybmp106.Height);

                gw107.Dispose();
                pictureBox1.Image = mybmp107;
                /////////////////////////////////////////////////////////////////////////上空白處+上耳+中耳+下耳(左)
                Image MergedImage110 = default(Image);
                Int32 Wide110 = 0;
                Int32 High110 = 0;
                High110 = spacefree.Height + mybmp109.Height;//設定高度          
                if (spacefree.Width >= mybmp109.Width)
                {
                    Wide110 = spacefree.Width;
                }
                else
                {
                    Wide110 = mybmp109.Width;
                }
                Bitmap mybmp110 = new Bitmap(Wide110, High110);
                Graphics gw110 = Graphics.FromImage(mybmp110);
                //處理第一張圖片


                Bitmap spacefreebmm = spacefree.ToBitmap();
                gw110.DrawImage(spacefreebmm, 0, 0);
                //處理第二張圖片

                gw110.DrawImage(mybmp109, 0, spacefreebmm.Height);
                MergedImage110 = mybmp110;
                gw110.Dispose();
                //pictureBox1.Image = mybmp110;
                /////////////////////////////////////////////////////////////////////////上空白處+上耳+中耳+下耳+下空白處(左)
                Image MergedImage111 = default(Image);
                Int32 Wide111 = 0;
                Int32 High111 = 0;
                High111 = mybmp110.Height + spacefree2.Height;//設定高度          
                if (mybmp110.Width >= spacefree2.Width)
                {
                    Wide111 = mybmp110.Width;
                }
                else
                {
                    Wide111 = spacefree2.Width;
                }
                Bitmap mybmp111 = new Bitmap(Wide111, High111);
                Graphics gw111 = Graphics.FromImage(mybmp111);
                //處理第一張圖片


                Bitmap spacefree2bmm = spacefree2.ToBitmap();
                gw111.DrawImage(mybmp110, 0, 0);
                //處理第二張圖片

                gw111.DrawImage(spacefree2bmm, 0, mybmp110.Height);
                MergedImage111 = mybmp111;
                gw111.Dispose();
                //pictureBox1.Image = mybmp111;
                ////////////////////////////////////////////////////////////////////////////左耳+整圖
                Image MR210 = default(Image);
                Int32 WD210 = 0;
                Int32 HH210 = 0;
                WD210 = mybmp111.Width + mybmp107.Width;//設定寬度           
                if (mybmp111.Height >= mybmp107.Height)
                {
                    HH210 = mybmp111.Height;
                }
                else
                {
                    HH210 = mybmp107.Height;
                }
                Bitmap bi210 = new Bitmap(WD210, HH210);
                Graphics gs210 = Graphics.FromImage(bi210);
                //處理第一張圖片            
                gs210.DrawImage(mybmp111, 0, 0);
                //處理第二張圖片
                gs210.DrawImage(mybmp107, mybmp111.Width, 0);
                MR210 = bi210;
                gs210.Dispose();
                //pictureBox1.Image = bi210;
                /////////////////////////////////////////////////////////////////////////上空白處+上耳+中耳+下耳(右)
                //Image MergedImage114 = default(Image);
                Int32 Wide114 = 0;
                Int32 High114 = 0;
                High114 = spacefree.Height + mybmp113.Height;//設定高度          
                if (spacefree.Width >= mybmp113.Width)
                {
                    Wide114 = spacefree.Width;
                }
                else
                {
                    Wide114 = mybmp113.Width;
                }
                Bitmap mybmp114 = new Bitmap(Wide114, High114);
                Graphics gw114 = Graphics.FromImage(mybmp114);
                //處理第一張圖片



                gw114.DrawImage(spacefreebmm, 0, 0);
                //處理第二張圖片

                gw114.DrawImage(mybmp113, 0, spacefreebmm.Height);

                gw114.Dispose();
                //pictureBox1.Image = mybmp114;
                /////////////////////////////////////////////////////////////////////////上空白處+上耳+中耳+下耳+下空白處(右)
                //Image MergedImage115 = default(Image);
                Int32 Wide115 = 0;
                Int32 High115 = 0;
                High115 = mybmp114.Height + spacefree2.Height;//設定高度          
                if (mybmp114.Width >= spacefree2.Width)
                {
                    Wide115 = mybmp114.Width;
                }
                else
                {
                    Wide115 = spacefree2.Width;
                }
                Bitmap mybmp115 = new Bitmap(Wide115, High115);
                Graphics gw115 = Graphics.FromImage(mybmp115);
                //處理第一張圖片



                gw115.DrawImage(mybmp114, 0, 0);
                //處理第二張圖片

                gw115.DrawImage(spacefree2bmm, 0, mybmp114.Height);

                gw115.Dispose();
                //pictureBox1.Image = mybmp115;
                ////////////////////////////////////////////////////////////////////////////右耳+整圖
                //Image MR216 = default(Image);
                Int32 WD216 = 0;
                Int32 HH216 = 0;
                WD216 = bi210.Width + mybmp115.Width;//設定寬度           
                if (bi210.Height >= mybmp115.Height)
                {
                    HH216 = bi210.Height;
                }
                else
                {
                    HH216 = mybmp115.Height;
                }
                Bitmap bi216 = new Bitmap(WD216, HH216);
                Graphics gs216 = Graphics.FromImage(bi216);
                //處理第一張圖片            
                gs216.DrawImage(bi210, 0, 0);
                //處理第二張圖片
                gs216.DrawImage(mybmp115, bi210.Width, 0);

                gs216.Dispose();
                //pictureBox1.Image = bi216;
                ////////////////////////////////////////////////////////////////////////////test1
                //Image MR217 = default(Image);
                Int32 WD217 = 0;
                Int32 HH217 = 0;
                WD217 = mybmp109.Width + mybmp104.Width;//設定寬度           
                if (mybmp109.Height >= mybmp104.Height)
                {
                    HH217 = mybmp109.Height;
                }
                else
                {
                    HH217 = mybmp104.Height;
                }
                Bitmap bi217 = new Bitmap(WD217, HH217);
                Graphics gs217 = Graphics.FromImage(bi217);
                //處理第一張圖片            
                gs217.DrawImage(mybmp109, 0, 0);
                //處理第二張圖片
                gs217.DrawImage(mybmp104, mybmp109.Width, 0);

                gs217.Dispose();
                //pictureBox1.Image = bi217;

                ///////////////////////////////////////////全新處//////////////////////////////////////////////////////////////////////


            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            this.Cursor = Cursors.Arrow;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////


        private Image<Bgr, byte> grayimgtobgr(Image<Gray, byte> img)
        {
            Image<Bgr, byte> bgrimg = new Image<Bgr, byte>(img.Width, img.Height);
            for (int i = 0; i < img.Width; ++i)
            {
                for (int j = 0; j < img.Height; ++j)
                {
                    bgrimg[j, i] = new Bgr(img[j, i].Intensity, img[j, i].Intensity, img[j, i].Intensity);
                }
            }

            return bgrimg;
        }
        

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> nose = My_Image2.Clone();
            Image<Gray, byte> nosegray = My_Image1.Clone();
            
            //nosegray.ROI = faces[0];

            nosegray = nosegray.ThresholdBinaryInv(new Gray(80),new Gray(255));
            Image<Bgr, byte> nosegraytoBgr = nosegray.Convert<Bgr, byte>();
            //faces[0].X+faces[0].Width/2,faces[0].Y+faces[0].Height/2 中心點座標
            Rectangle noserange = new Rectangle(faces[0].X+ (faces[0].Width/5)*2, faces[0].Y+ faces[0].Height/2, faces[0].Width/5, faces[0].Height/7);//取鼻子範圍
            nose.ROI = noserange;
            nosegray.ROI = noserange;
            nosegraytoBgr.ROI = noserange;


            Bitmap tempimg = new Bitmap(nosegray.Bitmap);
            Bitmap tempcolorimg = new Bitmap(nose.Bitmap);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(nosegray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int count = contours.Size;
            int biggest = 0;//紀錄最大的面積在哪個contours

            Rectangle first = new Rectangle(0, 0, 0, 0), second = new Rectangle(0,0,0,0);//最大跟第二大物件的座標
            for (int i = 0; i < count; i++)//找最大
            {
                using (VectorOfPoint contour = contours[i])
                {
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if(BoundingBox.Width* BoundingBox.Height>first.Width*first.Height) { first = BoundingBox; biggest = i; }
                }
            }

            for (int i = 0; i < count; i++)//找第二大
            {
                using (VectorOfPoint contour = contours[i])
                {
                    if (i != biggest)
                    {
                        Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                        if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height) { second = BoundingBox; }
                    }
                }
            }

            //if (first.X > second.X)//如果最大在右邊 交換
            //{
            //    Rectangle temprect = first;
            //    first = second;
            //    second = temprect;
            //}

            Point centernose = new Point((first.X + first.Width / 2 + second.X + second.Width / 2)/2, (first.Y + first.Height / 2 + second.Y + second.Height / 2) / 2);//左右鼻孔位置平均

            StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            Graphics g = Graphics.FromImage(tempimg);//畫左鼻孔中心點
            SolidBrush drawBrush = new SolidBrush(Color.Red);
            SolidBrush drawBrushG = new SolidBrush(Color.Green);
            g.DrawString("+", new Font("Arial", 25), drawBrush, first.X + first.Width / 2, first.Y + first.Height / 2, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrushG, second.X + second.Width / 2, second.Y + second.Height / 2, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrush, centernose.X, centernose.Y, sf);

            g.Dispose();

            Graphics g1 = Graphics.FromImage(tempcolorimg);//畫右鼻孔中心點
            g1.DrawString("+", new Font("Arial", 25), drawBrush, first.X + first.Width / 2, first.Y + first.Height / 2, sf);
            g1.DrawString("+", new Font("Arial", 25), drawBrushG, second.X + second.Width / 2, second.Y + second.Height / 2, sf);
            g1.DrawString("+", new Font("Arial", 25), drawBrush, centernose.X, centernose.Y, sf);
            g1.Dispose();

            
            


            nosegray.Bitmap = tempimg;
            nosegray.ROI = Rectangle.Empty;
            nose.Bitmap = tempcolorimg;
            nose.ROI = Rectangle.Empty;
            nosegraytoBgr.Bitmap = tempimg;

            //nosegraytoBgr.ROI = Rectangle.Empty;

            imageBox1.Image = nose;
            imageBox2.Image = nosegraytoBgr;
        }

        bool doppff;
        int timercounter = 0;
        int loopcounter = 0;
        int numofpar = 30;
        Parcitle maxPar,maxParRight;
        double maxw = -100;
        double maxd1 = -100;
        double maxd2 = -100;
        double maxd3 = -100;
        double maxd4 = -100;

        private void toolStripMenuItem12_Click(object sender, EventArgs e)//1000次
        {
            //LevatorFaceDown = null;
            //LevatorFaceUp = null;
            //My_Image2 = null;

            #region imgLoading & UI setting
            if (My_Image2 == null || LevatorFaceDown == null || LevatorFaceUp == null )
            {
                toolStripMenuItem20_Click(sender, e);
                // If user click 'Cancel' ,return
                if (My_Image2 == null || LevatorFaceDown == null || LevatorFaceUp == null)
                {
                    MessageBox.Show("取消執行");
                    return;
                }
            }
            pictureBox1.Visible = false;
            imageBox2.Image = null;
            imageBox4.Visible = true;
            imageBox5.Visible = true;
            imageBox5.Image = null;
            imageBox6.Visible = true;
            imageBox6.Image = null;
            label1.Text = null;
            label2.Text = null;
            label3.Text = null;
            pictureBox1.Visible = false;
            imageBox3.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            imageBox2.Visible = true;
            panel1.Visible = false;
            label5.Text = null;
            label6.Text = null;
            label7.Text = null;

            #endregion

            this.Refresh();
            timer2.Stop();
            MRD1 = new List<double>();
            MRD2 = new List<double>();
            PFH = new List<double>();
            PFW = new List<double>();
            OSA = new List<double>();
            PtosisSeverity = new List<double>();
            Levetor = new List<double>();

            loopcounter = 0;

            Image<Hsv, Byte> HSVimg;
            // Seperate Eye Region by relative position
            if (facecutori != null)
            {
                facecutori.ROI = facesori;
                Ori_Image = facecutori.Clone();
                int Eye_X = facecutori.ROI.Width * 15 / 100;
                int Eye_Y = facecutori.ROI.Height * 25 / 100;
                int Eye_Width = facecutori.ROI.Width * 70 / 100;
                int Eye_Height = facecutori.ROI.Height * 20 / 100;
                Ori_Image.ROI = new Rectangle(Eye_X + facecutori.ROI.X, Eye_Y + facecutori.ROI.Y, Eye_Width, Eye_Height);

                HSVimg = new Image<Hsv, byte>(Ori_Image.Width, Ori_Image.Height); //HSV類型要定義為float類型，因為BGR轉化後數值非整數
                Image<Bgr, float> oriBgr_float = new Image<Bgr, float>(Ori_Image.Width, Ori_Image.Height);
                CvInvoke.cvConvertScale(Ori_Image, oriBgr_float, 1.0, 0); //將原圖轉化為float類型的數據
                CvInvoke.CvtColor(Ori_Image, HSVimg, ColorConversion.Bgr2HsvFull); //根據圖像的類型選擇轉換方式BGR2HSV，還有RGB2HSV
                Image<Gray, byte> saturation = HSVimg[1];
                CvInvoke.Normalize(saturation, saturation, 0, 255, NormType.MinMax);
                saturation.Save("HSVimg0.jpg");

                LevatorFaceDown.ROI = new Rectangle(Eye_X + facecutori.ROI.X, Eye_Y + facecutori.ROI.Y, Eye_Width, Eye_Height);
                LevatorFaceUp.ROI = new Rectangle(Eye_X + facecutori.ROI.X, Eye_Y + facecutori.ROI.Y, Eye_Width, Eye_Height);

                LevatorFaceDown.Save("LevatorFaceDown.jpg");
                LevatorFaceUp.Save("LevatorFaceUp.jpg");

            }
            else
            {
                MessageBox.Show("臉部偵測失敗，請重新拍攝");
                return;
            }


            // Particle Filter method
            doppff = true;
            LorR_flag = 0;
            maxw = double.MinValue;
            timercounter = 0;
            loopcounter = 0;
            maxPar = null;
            maxParRight = null;
            sum[0] = 0;
            sum[1] = 0;
            sum[2] = 0;
            sum[3] = 0;

            ppff = new Particle_parameter_for_fullimg(Ori_Image);

            #region EyeROI
            // Find EyeROI base on faceROI and FRST outcome
            int BlockWidth = 100;
            int BlockHeight = 60;
            Point EyeRoi_R = ppff.FindEyeROIbyFRST(BlockWidth, BlockHeight);
            Point EyeRoi_L;
            if (EyeRoi_R.X >= (Ori_Image.Width / 2)){
                //RightEye
                EyeRoi_L = ppff.FindEyeROIbyFRST(BlockWidth, BlockHeight, "R");
            }
            else{
                //LeftEye
                EyeRoi_L = ppff.FindEyeROIbyFRST(BlockWidth, BlockHeight, "L");
            }

            if (EyeRoi_R.X > EyeRoi_L.X){
                Point temp = EyeRoi_R;
                EyeRoi_R = EyeRoi_L;
                EyeRoi_L = temp;
            }

            // Set L_eye, R_eye, R_LevatorDown,L_LevatorDown,R_LevatorUp,L_LevatorUp ROI
            R_eye = Ori_Image.Copy();
            R_eye.ROI = new Rectangle(EyeRoi_R, new Size(BlockWidth, BlockHeight));
            R_eyeParticle = R_eye.Copy();
            L_eye = Ori_Image.Copy();
            L_eye.ROI = new Rectangle(EyeRoi_L, new Size(BlockWidth, BlockHeight));
            L_eyeParticle = L_eye.Copy();

            R_LevatorDown = LevatorFaceDown.Copy();
            L_LevatorDown = LevatorFaceDown.Copy();
            R_LevatorUp = LevatorFaceUp.Copy();
            L_LevatorUp = LevatorFaceUp.Copy();

            R_LevatorDown.ROI = R_eye.ROI;
            R_LevatorUp.ROI = R_eye.ROI;
            L_LevatorDown.ROI = L_eye.ROI;
            L_LevatorUp.ROI = L_eye.ROI;

            R_LevatorDown.Save("R_LevatorDown.jpg");
            R_LevatorUp.Save("R_LevatorUp.jpg");
            L_LevatorDown.Save("L_LevatorDown.jpg");
            L_LevatorUp.Save("L_LevatorUp.jpg");

            #endregion

            #region LevatorFunction

            int Down, Up;
            CornerDetection levator = new CornerDetection(R_LevatorDown);
            Down = levator.VPF_eyelidsDetect("R_LevatorDown");
            levator = new CornerDetection(R_LevatorUp);
            Up = levator.VPF_eyelidsDetect("R_LevatorUp");
            Levetor.Add((Down - Up)* mmperpixel);

            levator = new CornerDetection(L_LevatorDown);
            Down = levator.VPF_eyelidsDetect("L_LevatorDown");
            levator = new CornerDetection(L_LevatorUp);
            Up = levator.VPF_eyelidsDetect("L_LevatorUp");
            Levetor.Add((Down - Up)* mmperpixel);

            Console.WriteLine("Levetor R :" + Levetor[0]+ " Levetor L : "+ Levetor[1]);



            #endregion

            #region Corner


            Image<Bgr, byte> R_Pupil, L_Pupil;

            CornerDetection CornerDetector = new CornerDetection(R_eye);
            CornerDetector.VPF("R", out R_PupilROI, out R_CornerL_ROI, out R_CornerR_ROI, out R_Pupil);
            // Get the Corner (PointF)
            R_CornerL = CornerDetector.WVPF("R_eye_CornerL", ref ppff);
            R_CornerR = CornerDetector.WVPF("R_eye_CornerR", ref ppff);
            R_CornerL.X += R_PupilROI.Right;
            R_CornerL.Y += R_PupilROI.Top;


            // Draw R_eye Corner
            //R_eye.Draw(new Cross2DF(R_CornerL, 5, 5), new Bgr(0, 0, 255), 1);
            //R_eye.Draw(new Cross2DF(R_CornerR, 5, 5), new Bgr(0, 0, 255), 1);

            // Boundary Check
            // Size of Corner_ROI
            int cor_offset = 5;
            int cor_width = 10;
            // Outer Corner
            R_CornerR_ROI.X = ((int)R_CornerR.X - cor_offset) < 0 ? (int)R_CornerR.X : ((int)R_CornerR.X - cor_offset);
            R_CornerR_ROI.Width = cor_width;
            // Inner Corner
            R_CornerL_ROI.X = (int)R_CornerL.X - cor_offset;
            R_CornerL_ROI.Y = ((int)R_CornerL.Y - cor_offset) < 0 ? (int)R_CornerL.Y : (int)R_CornerL.Y - cor_offset;
            R_CornerL_ROI.Width = ((int)R_CornerL.X + cor_offset) < R_eye.Width ? cor_width : (R_eye.Width - R_CornerL_ROI.X - 1);
            R_CornerL_ROI.Height = ((int)R_CornerL.Y + cor_offset) < R_eye.Height ? cor_width : (R_eye.Height - R_CornerL_ROI.Y - 1);

            //R_eye.Draw(new Rectangle((int)R_CornerL.X - 5, (int)R_CornerL.Y - 5, 10, 10), new Bgr(0, 0, 255), 1);
            //R_eye.Draw(new Rectangle((int)R_CornerR.X - 5, (int)R_CornerR_ROI.Y, 10, R_CornerR_ROI.Height), new Bgr(0, 0, 255), 1);
            //R_eye.Draw(R_CornerR_ROI, new Bgr(0, 255, 0), 1);
            //R_eye.Draw(R_CornerL_ROI, new Bgr(0, 255, 0), 1);



            //---------------------------------------------------------------------------------------

            CornerDetector = new CornerDetection(L_eye);
            CornerDetector.VPF("L", out L_PupilROI, out L_CornerL_ROI, out L_CornerR_ROI, out L_Pupil);
            // Get the Corner (PointF)
            L_CornerL = CornerDetector.WVPF("L_eye_CornerL", ref ppff);
            L_CornerR = CornerDetector.WVPF("L_eye_CornerR", ref ppff);

            L_CornerR.Y += L_PupilROI.Y;

            // Draw L_eye Corner
            //L_eye.Draw(new Cross2DF(L_CornerL, 5, 5), new Bgr(0, 0, 255), 1);
            //L_eye.Draw(new Cross2DF(L_CornerR, 5, 5), new Bgr(0, 0, 255), 1);


            // Boundary Check
            // Outer Corner
            L_CornerL_ROI.X = (int)L_CornerL.X - cor_offset;
            L_CornerL_ROI.Width = ((int)L_CornerL.X + cor_offset) < L_eye.Width ? cor_width : L_eye.Width - (int)L_CornerL_ROI.X - 1;
            // Inner Corner
            L_CornerR_ROI.X = ((int)L_CornerR.X - cor_offset) < 0 ? (int)L_CornerR.X : (int)L_CornerR.X - cor_offset;
            L_CornerR_ROI.Y = ((int)L_CornerR.Y - cor_offset) < 0 ? (int)L_CornerR.Y : (int)L_CornerR.Y - cor_offset;
            L_CornerR_ROI.Width = cor_width;
            L_CornerR_ROI.Height = ((int)L_CornerR.Y + cor_offset) < L_eye.Height ? cor_width : (L_eye.Height - L_CornerR_ROI.Y - 1);

            //L_eye.Draw(new Rectangle((int)L_CornerL.X - 5, (int)L_CornerL_ROI.Y, 10, L_CornerL_ROI.Height), new Bgr(0, 0, 255), 1);
            //L_eye.Draw(L_CornerL_ROI, new Bgr(0, 255, 0), 1);
            //L_eye.Draw(L_CornerR_ROI, new Bgr(0, 255, 0), 1);
            //L_eye.Draw(new Rectangle((int)L_CornerR.X - 5, (int)L_CornerR.Y - 5, 10, 10), new Bgr(0, 255, 0), 1);


            #endregion

            #region PupilMark

            // HoughCircle outcome
            List<CircleF> list_LeftPupil = new List<CircleF>();
            List<CircleF> list_RightPupil = new List<CircleF>();

            // For debuging 
            List<Image<Bgr, byte>> list_draw = new List<Image<Bgr, byte>>();

            // Parameter for HoughCircle
            double dp = 0.5;
            double minDist = 0.05;
            double CannyThres = 80;
            double HoughThres = 80;
            int minRadius = 12; // hospital : 29
            int maxRadius = 18; // hospital : 38

            // Right Pupil Detect
            //R_Pupil._SmoothGaussian(3);
            pupilDection_R = new PupilDetection();
            list_RightPupil = pupilDection_R.HoughCircles(R_Pupil, dp, minDist, CannyThres, HoughThres, minRadius, maxRadius);
            if (list_RightPupil.Count != 0)
            {
                pupilDection_R.SavePreprocess(list_RightPupil, "R");
            }
            else
            {
                timer2.Enabled = false;
                return;
            }

            // Left Pupil Detect
            //L_Pupil._SmoothGaussian(3);
            pupilDection_L = new PupilDetection();
            list_LeftPupil = pupilDection_L.HoughCircles(L_Pupil, dp, minDist, CannyThres, HoughThres, minRadius, maxRadius);
            if (list_LeftPupil.Count != 0)
            {
                pupilDection_L.SavePreprocess(list_LeftPupil, "L");
            }
            else
            {
                timer2.Enabled = false;
                return;
            }

            // Visualization
            Image<Bgr, byte> draw = Ori_Image.Clone();
            pupilDection_L.DrawEyeRoi(list_LeftPupil, list_RightPupil, ref draw, ref L_eye, ref R_eye, R_PupilROI, L_PupilROI);
            if (list_LeftPupil.Count != 0 && list_RightPupil.Count != 0)
            {
                L_eye_Pupil = list_LeftPupil[0];
                R_eye_Pupil = list_RightPupil[0];
            }

            #endregion
            Ori_Image.Save("Ori_Image.jpg");
            Ori_Image.Draw(new Rectangle(EyeRoi_R, new Size(BlockWidth, BlockHeight)), new Bgr(0, 0, 255), 1);
            Ori_Image.Draw(new Rectangle(EyeRoi_L, new Size(BlockWidth, BlockHeight)), new Bgr(0, 0, 255), 1);
            imageBox1.Image = Ori_Image;
            imageBox5.Image = R_eye;
            imageBox6.Image = L_eye;

            R_eye_SobelY = R_eye.Convert<Gray,byte>().Sobel(0, 1, 3);
            R_eye_SobelY = R_eye_SobelY.AbsDiff(new Gray(0));
            R_eye_SobelY.Save("R_eye_SobelY.jpg");
            L_eye_SobelY = L_eye.Convert<Gray, byte>().Sobel(0, 1, 3);
            L_eye_SobelY = L_eye_SobelY.AbsDiff(new Gray(0));
            L_eye_SobelY.Save("L_eye_SobelY.jpg");
            
            timer2.Enabled = true;
            timer2.Start();
        }
        double[] sum = new double[4];
        Particle_parameter_for_fullimg ppff;
        PupilDetection pupilDection_R, pupilDection_L;
        CircleF L_eye_Pupil, R_eye_Pupil;
        Image<Bgr, Byte> turn;
        Parcitle par;
        Image<Bgr, byte> L_eye, R_eye;
        Image<Gray, float> L_eye_SobelY, R_eye_SobelY;
        Image<Bgr, byte> L_eyeParticle, R_eyeParticle;
        Image<Bgr, byte> ParticleDraw;
        PointF R_CornerL, R_CornerR;
        PointF L_CornerL, L_CornerR;
        System.Drawing.Rectangle L_PupilROI, R_PupilROI;
        Rectangle L_CornerR_ROI, L_CornerL_ROI;
        Rectangle R_CornerR_ROI, R_CornerL_ROI;
        List<List<PointF>> CtrlPoints = new List<List<PointF>>();

        Image<Bgr, byte> R_LevatorDown, L_LevatorDown;
        Image<Bgr, byte> R_LevatorUp, L_LevatorUp;
        public Image<Bgr, byte> LevatorFaceUp, LevatorFaceDown;

        // The first item is R_eye , the second item is L_eye
        List<double> MRD1;
        List<double> MRD2;
        List<double> PFH;
        List<double> PFW;
        List<double> OSA;
        List<double> PtosisSeverity;
        List<double> Levetor;

        public String name = String.Empty;
        public String AgeSex = String.Empty;
        public String NoChart = String.Empty;
        public String Address = String.Empty;
        public String Phone = String.Empty;
        public String Date = String.Empty;


        int parcount;
        int LorR_flag = 0;

        private void timer2_Tick(object sender, EventArgs e)
        {
            label4.Text = timercounter.ToString();

            if (timercounter == numofpar)
            {
                doppff = true;
                LorR_flag++;
                timercounter = 0;
                loopcounter++;
                Console.WriteLine("next loop" + loopcounter.ToString());
                sum[0] = 0;
                sum[1] = 0;
                sum[2] = 0;
                sum[3] = 0;
                this.imageBox2.Image = ParticleDraw;

                #region The last step
                if (loopcounter == 2)
                {
                    maxPar.Graddraw(ref L_eye);
                    maxPar.drawtest(ref L_eye);
                    Console.WriteLine("end");

                    L_eye.Save("L_eyeROI.jpg");
                    R_eye.Save("R_eyeROI.jpg");

                    panel1.Visible = true;
                    label6.Visible = true;
                    label7.Visible = true;
                    imageBox2.Visible = false;
                    imageBox5.Visible = true;
                    imageBox6.Visible = true;

                    imageBox1.Image = My_Image2;
                    imageBox5.Image = R_eye;
                    imageBox6.Image = L_eye;


                    // Calculate L_eye measurement
                    measurementCalculate(L_PupilROI, L_eye_Pupil, maxPar);

                    // Output result
                    // R_eye measurement
                    label6.Text = "\n\n" + MRD1[0].ToString("#0.#0") +
                                  "\n" + MRD2[0].ToString("#0.#0") +
                                  "\n" + PFH[0].ToString("#0.#0") +
                                  "\n" + PFW[0].ToString("#0.#0") +
                                  "\n" + PtosisSeverity[0].ToString("#0.#0") +
                                  "\n" + OSA[0].ToString("#0.#0") +
                                  "\n" + Levetor[0].ToString("#0.#0");
                    // L_eye measurement
                    label7.Text = "\n\n" + MRD1[1].ToString("#0.#0") +
                                  "\n" + MRD2[1].ToString("#0.#0") +
                                  "\n" + PFH[1].ToString("#0.#0") +
                                  "\n" + PFW[1].ToString("#0.#0") +
                                  "\n" + PtosisSeverity[1].ToString("##0.#0") +
                                  "\n" + OSA[1].ToString("#0.#0") +
                                  "\n" + Levetor[1].ToString("#0.#0");

                    timer2.Stop();

                    #region PDF report
                    SaveFileDialog Savefile = new SaveFileDialog();
                    Savefile.DefaultExt = "pdf";
                    Savefile.Filter = "PDF 檔案(.pdf)|*.pdf";


                    if (Savefile.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            PdfReader reader = new PdfReader("sample.pdf");
                            int n = reader.NumberOfPages;
                            PdfStamper stamp = new PdfStamper(reader, new FileStream(Savefile.FileName, FileMode.Create));
                            PdfContentByte under;
                            //iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("D:\\51863897_p0.jpg");
                            //img.SetAbsolutePosition(0, 0);
                            BaseFont bfHei = BaseFont.CreateFont(@"arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);


                            int i = 0;
                            while (i < n)
                            {
                                i++;
                                under = stamp.GetOverContent(i);
                                PdfTemplate template;

                                #region slit_pre
                                template = under.CreateTemplate(60, 20);
                                //template.AddImage(img);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PFH[0].ToString("#0.#0"));//右眼
                                template.EndText();
                                under.AddTemplate(template, 55, 695);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PFH[1].ToString("#0.#0"));//左眼
                                template.EndText();
                                under.AddTemplate(template, 238, 695);//左下0,0
                                #endregion

                                #region slit_post
                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(leyeinfo[0]);
                                //template.EndText();
                                //under.AddTemplate(template, 143, 695);//左下0,0

                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(reyeinfo[0]);
                                //template.EndText();
                                //under.AddTemplate(template, 325, 695);//左下0,0
                                #endregion

                                #region mrd_pre
                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PFH[0].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 55, 657);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PFH[1].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 238, 657);//左下0,0
                                #endregion

                                #region mrd_post
                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(leyeinfo[1]);
                                //template.EndText();
                                //under.AddTemplate(template, 143, 657);//左下0,0

                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(reyeinfo[1]);
                                //template.EndText();
                                //under.AddTemplate(template, 325, 657);//左下0,0
                                #endregion

                                #region severity_pre
                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PtosisSeverity[0].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 55, 620);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(PtosisSeverity[1].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 238, 620);//左下0,0
                                #endregion

                                #region severity_pre v
                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 8, 10);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 12);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText("V");
                                template.EndText();
                                if (PtosisSeverity[0] >= 1 && PtosisSeverity[0] <= 2)
                                    under.AddTemplate(template, 33, 604);//左下0,0
                                else if (PtosisSeverity[0] > 2 && PtosisSeverity[0] <= 3)
                                    under.AddTemplate(template, 33, 586);//左下0,0
                                else if (PtosisSeverity[0] > 3 && PtosisSeverity[0] <= 4)
                                    under.AddTemplate(template, 33, 568);//左下0,0
                                else if (PtosisSeverity[0] > 4)
                                    under.AddTemplate(template, 33, 550);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 8, 10);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 12);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText("V");
                                template.EndText();
                                if (PtosisSeverity[1] >= 1 && PtosisSeverity[1] <= 2)
                                    under.AddTemplate(template, 216, 604);//左下0,0
                                else if (PtosisSeverity[1] > 2 && PtosisSeverity[1] <= 3)
                                    under.AddTemplate(template, 216, 586);//左下0,0
                                else if (PtosisSeverity[1] > 3 && PtosisSeverity[1] <= 4)
                                    under.AddTemplate(template, 216, 568);//左下0,0
                                else if (PtosisSeverity[1] > 4)
                                    under.AddTemplate(template, 216, 550);//左下0,0
                                #endregion

                                #region severity_post
                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(leyeinfo[2]);
                                //template.EndText();
                                //under.AddTemplate(template, 143, 620);//左下0,0

                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN);
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText(reyeinfo[2]);
                                //template.EndText();
                                //under.AddTemplate(template, 325, 620);//左下0,0
                                #endregion

                                #region levator_pre
                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(Levetor[0].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 55, 464);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                template.Rectangle(0, 0, 50, 15);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 18);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText(Levetor[1].ToString("#0.#0"));
                                template.EndText();
                                under.AddTemplate(template, 238, 464);//左下0,0
                                #endregion

                                #region levator_pre v
                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 8, 10);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 12);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText("V");
                                template.EndText();
                                if (Levetor[0] >= 12)
                                    under.AddTemplate(template, 33, 448);//左下0,0
                                else if (Levetor[0] >= 8 && Levetor[0] < 12)
                                    under.AddTemplate(template, 33, 430);//左下0,0
                                else if (Levetor[0] >= 5 && Levetor[0] < 8)
                                    under.AddTemplate(template, 34, 412);//左下0,0
                                else if (Levetor[0] <= 4)
                                    under.AddTemplate(template, 34, 394);//左下0,0

                                template = under.CreateTemplate(60, 20);
                                template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker());
                                template.Rectangle(0, 0, 8, 10);
                                template.Fill();
                                template.BeginText();
                                template.SetFontAndSize(bfHei, 12);
                                template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                template.ShowText("V");
                                template.EndText();
                                if (Levetor[1] >= 12)
                                    under.AddTemplate(template, 216, 448);//左下0,0
                                else if (Levetor[1] >= 8 && Levetor[1] < 12)
                                    under.AddTemplate(template, 216, 430);//左下0,0
                                else if (Levetor[1] >= 5 && Levetor[1] < 8)
                                    under.AddTemplate(template, 217, 412);//左下0,0
                                else if (Levetor[1] <= 4)
                                    under.AddTemplate(template, 217, 394);//左下0,0
                                #endregion

                                #region levator_post
                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN.Darker());
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText("13");
                                //template.EndText();
                                //under.AddTemplate(template, 143, 464);//左下0,0

                                //template = under.CreateTemplate(60, 20);
                                //template.SetColorFill(iTextSharp.text.BaseColor.GREEN.Darker());
                                //template.Rectangle(0, 0, 50, 15);
                                //template.Fill();
                                //template.BeginText();
                                //template.SetFontAndSize(bfHei, 18);
                                //template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                //template.ShowText("13");
                                //template.EndText();
                                //under.AddTemplate(template, 325, 464);//左下0,0
                                #endregion

                                #region name
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(name);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 758);//左下0,0
                                }
                                #endregion

                                #region AgeSex
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(AgeSex);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 741);//左下0,0
                                }
                                #endregion

                                #region NoChart
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(NoChart);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 724);//左下0,0
                                }
                                #endregion

                                #region Address
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(Address);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 707);//左下0,0
                                }
                                #endregion

                                #region Phone
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(Phone);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 690);//左下0,0
                                }
                                #endregion

                                #region Date
                                if (name != String.Empty)
                                {
                                    template = under.CreateTemplate(60, 20);
                                    template.SetColorFill(iTextSharp.text.BaseColor.YELLOW.Darker().Darker());
                                    template.Rectangle(0, 0, 100, 12);
                                    template.Fill();
                                    template.BeginText();
                                    template.SetFontAndSize(bfHei, 12);
                                    template.SetColorFill(iTextSharp.text.BaseColor.BLACK);
                                    template.ShowText(Date);
                                    template.EndText();
                                    under.AddTemplate(template, 477, 672);//左下0,0
                                }
                                #endregion

                            }
                            stamp.Close();


                            //Document document = new Document();
                            //PdfWriter.GetInstance(document, new FileStream(Savefile.FileName, FileMode.Create));
                            //document.Open();
                            //document.Add(new Paragraph("Hello World"));
                            //document.Close();

                            //var form = new Form6();
                            //form.showpdf(Savefile.FileName);
                            //form.Show(this);
                        }
                        catch
                        {

                        }

                    }
                    #region position
                    //左下0,0
                    //x 55 143 238 325
                    //eyeslit y 695
                    //MRD y 657
                    //serverity y<620
                    //eyelid crease y 501
                    //levator func y 464
                    #endregion
                    #endregion
                }



                #endregion
            }


            if (doppff && LorR_flag == 0)
            {
                ppff = new Particle_parameter_for_fullimg(R_eyeParticle);
                turn = R_eyeParticle.Clone();
                doppff = false;

                CtrlPoints = ContourSampling(getContour(R_eyeParticle),10,"R");

                foreach (var p in CtrlPoints[1])
                {
                    R_eyeParticle.Draw(new CircleF(p, 1), new Bgr(0, 0, 255), 0);
                }
                foreach (var p in CtrlPoints[0])
                {
                    R_eyeParticle.Draw(new CircleF(p, 1), new Bgr(255, 0, 0), 0);
                }
                R_eyeParticle.Save("R_Ctrl.jpg");
            }
            if (doppff && LorR_flag == 1)
            {
                maxParRight = new Parcitle(maxPar);
                maxParRight.Graddraw(ref R_eye);
                maxParRight.drawtest(ref R_eye);
                // Calculate R_eye measurement
                measurementCalculate(R_PupilROI, R_eye_Pupil, maxParRight);

                ppff = new Particle_parameter_for_fullimg(L_eyeParticle);
                turn = L_eyeParticle.Clone();
                doppff = false;
                
                CtrlPoints =  ContourSampling(getContour(L_eyeParticle),10,"L");

                foreach (var p in CtrlPoints[1])
                {
                    L_eyeParticle.Draw(new CircleF(p, 1), new Bgr(0, 0, 255), 0);
                }
                foreach (var p in CtrlPoints[0])
                {
                    L_eyeParticle.Draw(new CircleF(p, 1), new Bgr(255, 0, 0), 0);
                }
                L_eyeParticle.Save("L_Ctrl.jpg");

                maxPar = null;
                maxw = -100;

            }
            ParticleDraw = turn.Clone();


            if (loopcounter == 0)// R_eye
            {
                par = new Parcitle(ParticleDraw,CtrlPoints, ppff.ContoursPoint, R_eye_Pupil, R_PupilROI, R_CornerL, R_CornerR, "R");
                ParticleDraw.Draw(R_PupilROI, new Bgr(0, 0, 255), 1);
            }
            else if (loopcounter == 1)// L_eye
            {
                par = new Parcitle(ParticleDraw, CtrlPoints, ppff.ContoursPoint, L_eye_Pupil, L_PupilROI, L_CornerL, L_CornerR, "L");
                ParticleDraw.Draw(L_PupilROI, new Bgr(0, 0, 255), 1);
            }
            
            
            double d1 = par.Gradient(ref ppff);
            double d2 = par.Saturation(ref ppff);
            double d3 = par.symmetric(ref ppff);
            double d4 = par.corner(ref ppff);
            double weightnow = par.getweight(d1, sum[0] / parcount, d2, sum[1] / parcount, d3, sum[2] / parcount, d4, sum[3] / parcount, loopcounter);
            sum[0] += d1;
            sum[1] += d2;
            sum[2] += d3;
            sum[3] += d4;
            parcount++;

            par.Graddraw(ref ParticleDraw);
            par.drawtest(ref ParticleDraw);

            if (maxPar != null)// ma : max(eye)
            {
                maxPar.Graddraw(ref ParticleDraw);
            }
            this.imageBox2.Image = ParticleDraw;

            if (timercounter != 0)
                label7.Text = "\n" + d1.ToString("##0.####") +
                              "\n" + weightnow.ToString("##0.########");

            if (maxPar != null)
                maxw = maxPar.getweight(maxd1, sum[0] / parcount, maxd2, sum[1] / parcount, maxd3, sum[2] / parcount, maxd4, sum[3] / parcount, loopcounter);
            if (timercounter != 0)
            {
                label5.Text = "\ngradient     : ";
                              
                label6.Text = "\n" + maxd1.ToString("##0.####") +
                              "\n" + maxw.ToString("##0.########");
            }

            if (loopcounter == 0)
            {
                if (d3 > maxd3 || (d3 == maxd3 && weightnow < maxw) || maxPar == null)
                {
                    maxw = weightnow;
                    maxPar = new Parcitle(CtrlPoints,par.getbasic(), par.getPoint(), par.left, par.right, "R");
                    maxd1 = d1;
                    maxd2 = d2;
                    maxd3 = d3;
                    maxd4 = d4;
                }
            }
            else
            {
                if (weightnow < maxw || maxPar == null)
                {
                    maxw = weightnow;
                    maxPar = new Parcitle(CtrlPoints,par.getbasic(), par.getPoint(), par.left, par.right, "L");
                    maxd1 = d1;
                    maxd2 = d2;
                    maxd3 = d3;
                    maxd4 = d4;
                }
            }
            timercounter++;
        }

        int counter_picchange = 1;
        private void imageBox4_Click(object sender, EventArgs e)
        {
            if (ppff != null)
            {
                if (counter_picchange % 5 == 0)
                {
                    imageBox4.Image = Ori_Image;
                    counter_picchange++;
                }
                else if (counter_picchange % 5 == 1)
                {
                    imageBox4.Image = ppff.Gradpic();
                    counter_picchange++;
                }
                else if (counter_picchange % 5 == 2)
                {
                    imageBox4.Image = ppff.saturation;
                    counter_picchange++;
                }
                else if (counter_picchange % 5 == 3)
                {
                    imageBox4.Image = ppff.getfrstdraw3();
                    counter_picchange++;
                }
                else if (counter_picchange % 5 == 4)
                {
                    imageBox4.Image = ppff.getcornordraw();
                    counter_picchange++;
                }
            }
        }


        private void toolStripMenuItem14_Click(object sender, EventArgs e)//嘴唇
        {
            //鼻子
            Image<Bgr, byte> nose = My_Image2.Clone();
            Image<Gray, byte> nosegray = My_Image1.Clone();

            //nosegray.ROI = faces[0];

            nosegray = nosegray.ThresholdBinaryInv(new Gray(60), new Gray(255));
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
                    if (i == 0 && BoundingBox.X != first.X && BoundingBox.Y != first.Y && Math.Abs(BoundingBox.Y - first.Y) < 5) { second = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height && BoundingBox.X != first.X && BoundingBox.Y != first.Y && Math.Abs(BoundingBox.Y - first.Y) < 5) { second = BoundingBox; }
                }
            }

            if (first.X > second.X)//如果最大在右邊 交換
            {
                Rectangle temprect = first;
                first = second;
                second = temprect;
            }
            //鼻中心座標
            Point centernose = new Point((first.X + first.Width / 2 + second.X + second.Width / 2) / 2 + (faces[0].X + (faces[0].Width / 5) * 2), (first.Y + first.Height / 2 + second.Y + second.Height / 2) / 2 + (faces[0].Y + faces[0].Height / 2));//左右鼻孔位置平均

            centernose = MyFace.nosepoint(My_Image2, My_Image1, faces);

























            Image<Bgr, Byte> lips = My_Image2.Clone();
            //lips = lips.SmoothGaussian(11);
            Rectangle liprange = new Rectangle(faces[0].X+faces[0].Width/3, faces[0].Y+faces[0].Height*7 /12, faces[0].Width/3, faces[0].Height / 4);//用鼻中心取嘴唇範圍
            lips.ROI = liprange;
            imageBox2.Image = lips;
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

                    if (color.GetHue() <= 10 || color.GetHue() >= 334)
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

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contourbig);
                    if (i == 0) { biggestliphsv = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > biggestliphsv.Width * biggestliphsv.Height) { biggestliphsv = BoundingBox; biggestcontourliphsv = i; }
                }
            }




            Bitmap lipbitmap = new Bitmap(lips.Bitmap);

            int w = lipbitmap.Width;
            int h = lipbitmap.Height;
            Bitmap image1 = new Bitmap(lipbitmap);
            Bitmap image2 = new Bitmap(w, h);

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




            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(dark.X, dark.Y));


            // 領域の開始点の色を領域条件とする
            Color colour = lipbitmap.GetPixel(dark.X, dark.Y);

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
                    int range = 60;
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < image1.Width && Math.Abs(image1.GetPixel(p.X + 1, p.Y).R - colour.R) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).G - colour.G) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).B - colour.B) < range)
                    {
                        stack.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && Math.Abs(image1.GetPixel(p.X - 1, p.Y).R - colour.R) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).G - colour.G) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).B - colour.B) < range)
                    {
                        stack.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < image1.Height && Math.Abs(image1.GetPixel(p.X, p.Y + 1).R - colour.R) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).G - colour.G) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).B - colour.B) < range)
                    {
                        stack.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && Math.Abs(image1.GetPixel(p.X, p.Y - 1).R - colour.R) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).G - colour.G) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).B - colour.B) < range)
                    {
                        stack.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing

            int lipleft = 2147483647, lipright = 0;
            int leftpoint1 = 0, rightpoint1 = 0;

            Image<Gray, byte> lipsline = new Image<Gray, byte>(image2);//找嘴巴線的輪廓
            VectorOfVectorOfPoint contourslipsline = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(lipsline, contourslipsline, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int countlipline = contourslipsline.Size;

            Rectangle biggest = Rectangle.Empty;
            int biggestcontour = 0;
            for (int i = 0; i < countlipline; i++)//找最大
            {
                using (VectorOfPoint contourbig = contourslipsline[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contourbig);
                    if (i == 0) { biggest = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > biggest.Width * biggest.Height) { biggest = BoundingBox; biggestcontour = i; }
                }
            }

            VectorOfPoint contourlip = contourslipsline[biggestcontour];

            PointF[] temp = Array.ConvertAll(contourlip.ToArray(), new Converter<Point, PointF>(Point2PointF));
            PointF[] pts = CvInvoke.ConvexHull(temp, true);
            Point[] points = new Point[temp.Length];

            if (contourlip != null)
            {
                for (int j = 0; j < temp.Length; j++)//找左右端點
                {
                    points[j] = Point.Round(temp[j]);//PointF2Point


                    if (j > 1 && points[j].X < lipleft)
                    {
                        lipleft = points[j].X;
                        leftpoint1 = j;
                    }

                    if (j > 1 && points[j].X > lipright)
                    {
                        lipright = points[j].X;
                        rightpoint1 = j;
                    }
                }

                StringFormat sf1 = new StringFormat();//設定string置中，drawString才不會錯位
                sf1.Alignment = StringAlignment.Center;
                sf1.LineAlignment = StringAlignment.Center;

                Graphics g2 = Graphics.FromImage(image2);
                SolidBrush drawBrush1 = new SolidBrush(Color.Green);
                Pen pengreen = new Pen(Color.Green, 3);
                g2.DrawString("+", new Font("Arial", 25), drawBrush1, points[leftpoint1].X, points[leftpoint1].Y, sf1);//畫左點
                g2.DrawString("+", new Font("Arial", 25), drawBrush1, points[rightpoint1].X, points[rightpoint1].Y, sf1);//畫右點
                g2.Dispose();
            }




            Point lipsleftpoint = new Point(points[leftpoint1].X, points[leftpoint1].Y);
            Point lipsrightpoint = new Point(points[rightpoint1].X, points[rightpoint1].Y);

            lipleft = 2147483647; lipright = 0;
            leftpoint1 = 0; rightpoint1 = 0;


            Point[] lipscurve = { new Point(0, lipsleftpoint.Y), lipsleftpoint, dark, lipsrightpoint, new Point(w, lipsrightpoint.Y) };

            Image<Bgr, byte> smooth = new Image<Bgr, byte>(lipbitmap);
            smooth = smooth.SmoothMedian(9);
            lipbitmap = smooth.Bitmap;

            Graphics gate = Graphics.FromImage(lipbitmap);//畫線區隔上下唇
            Pen Penred = new Pen(Color.White, 1);
            gate.DrawCurve(Penred, lipscurve);
            gate.Dispose();





            Stack<Point> stackuplip = new Stack<Point>();
            stackuplip.Push(new Point(dark.X, dark.Y - 10));


            // 領域の開始点の色を領域条件とする
            Color colouruplip = lipbitmap.GetPixel(dark.X, dark.Y - 10);
            Color skincolor = lipbitmap.GetPixel(dark.X, dark.Y - 40);//取皮膚顏色來比較
            Bitmap imageuplip = new Bitmap(w, h);
            image1 = new Bitmap(lipbitmap);
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
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < image1.Width && (Math.Abs(image1.GetPixel(p.X + 1, p.Y).R - colouruplip.R) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).G - colouruplip.G) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).B - colouruplip.B) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetHue() - colouruplip.GetHue()) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetSaturation() - colouruplip.GetSaturation())) < (Math.Abs(image1.GetPixel(p.X + 1, p.Y).R - skincolor.R) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).G - skincolor.G) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).B - skincolor.B) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetHue() - skincolor.GetHue()) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetSaturation() - skincolor.GetSaturation())))
                    {
                        stackuplip.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && (Math.Abs(image1.GetPixel(p.X - 1, p.Y).R - colouruplip.R) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).G - colouruplip.G) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).B - colouruplip.B) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetHue() - colouruplip.GetHue()) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetSaturation() - colouruplip.GetSaturation())) < (Math.Abs(image1.GetPixel(p.X - 1, p.Y).R - skincolor.R) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).G - skincolor.G) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).B - skincolor.B) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetHue() - skincolor.GetHue()) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetSaturation() - skincolor.GetSaturation())))
                    {
                        stackuplip.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < image1.Height && (Math.Abs(image1.GetPixel(p.X, p.Y + 1).R - colouruplip.R) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).G - colouruplip.G) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).B - colouruplip.B) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetHue() - colouruplip.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetSaturation() - colouruplip.GetSaturation())) < (Math.Abs(image1.GetPixel(p.X, p.Y + 1).R - skincolor.R) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).G - skincolor.G) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).B - skincolor.B) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetHue() - skincolor.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetSaturation() - skincolor.GetSaturation())))
                    {
                        stackuplip.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && (Math.Abs(image1.GetPixel(p.X, p.Y - 1).R - colouruplip.R) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).G - colouruplip.G) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).B - colouruplip.B) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetHue() - colouruplip.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetSaturation() - colouruplip.GetSaturation())) < (Math.Abs(image1.GetPixel(p.X, p.Y - 1).R - skincolor.R) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).G - skincolor.G) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).B - skincolor.B) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetHue() - skincolor.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetSaturation() - skincolor.GetSaturation())))
                    {
                        stackuplip.Push(new Point(p.X, p.Y - 1));
                    }


                }
            }//Region growing



            Stack<Point> stackdownlip = new Stack<Point>();
            stackdownlip.Push(new Point(dark.X, dark.Y + 15));


            // 領域の開始点の色を領域条件とする
            Color colourdownlip = lipbitmap.GetPixel(dark.X, dark.Y + 15);
            Color skincolordown = lipbitmap.GetPixel(dark.X , dark.Y + 40);//取皮膚顏色來比較
            Bitmap imagelip = new Bitmap(w, h);
            image1 = new Bitmap(lipbitmap);
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
                    // 右隣を見て、領域条件に合えばStackに収める
                    if (p.X + 1 < image1.Width && Math.Abs(image1.GetPixel(p.X + 1, p.Y).R - colourdownlip.R) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).G - colourdownlip.G) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).B - colourdownlip.B) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetHue() - colourdownlip.GetHue()) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetSaturation() - colourdownlip.GetSaturation()) < Math.Abs(image1.GetPixel(p.X + 1, p.Y).R - skincolordown.R) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).G - skincolordown.G) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).B - skincolordown.B) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetHue() - skincolordown.GetHue()) + Math.Abs(image1.GetPixel(p.X + 1, p.Y).GetSaturation() - skincolordown.GetSaturation()))
                    {
                        stackdownlip.Push(new Point(p.X + 1, p.Y));
                    }

                    // 左隣を見て、領域条件に合えばStackに収める
                    if (p.X - 1 >= 0 && Math.Abs(image1.GetPixel(p.X - 1, p.Y).R - colourdownlip.R) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).G - colourdownlip.G) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).B - colourdownlip.B) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetHue() - colourdownlip.GetHue()) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetSaturation() - colourdownlip.GetSaturation()) < Math.Abs(image1.GetPixel(p.X - 1, p.Y).R - skincolordown.R) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).G - skincolordown.G) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).B - skincolordown.B) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetHue() - skincolordown.GetHue()) + Math.Abs(image1.GetPixel(p.X - 1, p.Y).GetSaturation() - skincolordown.GetSaturation()))
                    {
                        stackdownlip.Push(new Point(p.X - 1, p.Y));
                    }

                    // 下を見て、領域条件に合えばStackに収める
                    if (p.Y + 1 < image1.Height && Math.Abs(image1.GetPixel(p.X, p.Y + 1).R - colourdownlip.R) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).G - colourdownlip.G) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).B - colourdownlip.B) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetHue() - colourdownlip.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetSaturation() - colourdownlip.GetSaturation()) < Math.Abs(image1.GetPixel(p.X, p.Y + 1).R - skincolordown.R) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).G - skincolordown.G) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).B - skincolordown.B) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetHue() - skincolordown.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y + 1).GetSaturation() - skincolordown.GetSaturation()))
                    {
                        stackdownlip.Push(new Point(p.X, p.Y + 1));
                    }

                    // 上を見て、領域条件に合えばStackに収める
                    if (p.Y - 1 >= 0 && Math.Abs(image1.GetPixel(p.X, p.Y - 1).R - colourdownlip.R) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).G - colourdownlip.G) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).B - colourdownlip.B) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetHue() - colourdownlip.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetSaturation() - colourdownlip.GetSaturation()) < Math.Abs(image1.GetPixel(p.X, p.Y - 1).R - skincolordown.R) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).G - skincolordown.G) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).B - skincolordown.B) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetHue() - skincolordown.GetHue()) + Math.Abs(image1.GetPixel(p.X, p.Y - 1).GetSaturation() - skincolordown.GetSaturation()))
                    {
                        stackdownlip.Push(new Point(p.X, p.Y - 1));
                    }
                }
            }//Region growing





            Graphics g = Graphics.FromImage(lipbitmap);
            SolidBrush drawBrush = new SolidBrush(Color.Red);
            SolidBrush drawBrushgreen = new SolidBrush(Color.Green);
            StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            g.DrawString("+", new Font("Arial", 25), drawBrush, dark.X, dark.Y, sf);
            g.DrawString("*", new Font("Arial", 10), drawBrush, dark.X , dark.Y - 40, sf);
            g.DrawString("*", new Font("Arial", 10), drawBrush, dark.X , dark.Y + 40, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrushgreen, lipsleftpoint.X, lipsleftpoint.Y, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrushgreen, lipsrightpoint.X, lipsrightpoint.Y, sf);
            g.DrawCurve(Penred, lipscurve);
            g.Dispose();

            lips.Bitmap = lipbitmap;




            lips.ROI = Rectangle.Empty;
            imageBox1.Image = lips;

            imageBox2.Image = new Image<Bgr, byte>(imageuplip);
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)//開檔
        {
            imageBox2.Visible = false;
            imageBox3.Visible = false;
            imageBox4.Visible = false;
            imageBox5.Visible = false;
            imageBox6.Visible = false;

            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;

            panel1.Visible = false;

            pictureBox1.Visible = false;

            //第一次選取 正常直視
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Openfile.Title = "請選取正常直視圖";
                    My_Image1 = new Image<Gray, byte>(Openfile.FileName);
                    My_Image2 = new Image<Bgr, byte>(Openfile.FileName);

                    MessageBox.Show("請選取往下看照片");
                    #region 眼睛
                    CascadeClassifier frontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
                    faces = frontalface.DetectMultiScale(My_Image1, 1.1, 5, new Size(200, 200), Size.Empty);

                    List<Rectangle> face = new List<Rectangle>();
                    face.AddRange(faces);

                    //眼睛
                    if (faces.Length != 0)
                    {
                        facecutori = new Image<Bgr, Byte>(My_Image2.Bitmap);
                        facecutori.ROI = faces[0];//切出臉的部分
                        facecutorigray = new Image<Gray, Byte>(My_Image1.Bitmap);
                        facecutorigray.ROI = faces[0];


                        facesori = new Rectangle(faces[0].X, faces[0].Y, faces[0].Width, faces[0].Height);//擷取出的臉部範圍

                        int zoomface = 60;
                        for (int i = 0; i < faces.Length; i++)//調整臉範圍大小
                        {
                            faces[i].X = faces[i].X - zoomface;
                            faces[i].Y = faces[i].Y - zoomface * 2;
                            faces[i].Width = faces[i].Width + zoomface * 2;
                            faces[i].Height = faces[i].Height + zoomface * 4;
                        }

                        facezoom = new Rectangle(faces[0].X, faces[0].Y, faces[0].Width, faces[0].Height);
                        facecut = new Image<Bgr, Byte>(My_Image2.Bitmap);

                        facecut.ROI = faces[0];
                        imageBox1.Image = My_Image2;
                        //imageBox2.Image = facecut;
                        toolStripMenuItem1.Enabled = true;

                    }
                    #endregion

                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }
            else
            {
                My_Image2 = null;
            }

            //第二次選取 往下看圖
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Openfile.Title = "請選取往下看圖";
                    LevatorFaceDown = new Image<Bgr, byte>(Openfile.FileName);
                    MessageBox.Show("請選取往上看照片");
                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }
            else
            {
                LevatorFaceDown = null;
            }

            //第三次選取 往上看圖
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Openfile.Title = "請選取往上看圖";
                    LevatorFaceUp = new Image<Bgr, byte>(Openfile.FileName);
                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }
            else
            {
                LevatorFaceUp = null;
            }
        }

        
        int LineLength = -30;
        int LineThickness = 1;
        Bgr LineColor = new Bgr(0, 0, 255);
        FontFace fontface = FontFace.HersheySimplex;
        double fontscale = 0.3;
        MCvScalar textColor = new MCvScalar(0,0,255);
        // result radioButton
        private void radioButtonOri_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */  
            imageBox5.Image = R_eye;
            /* L_eye */ 
            imageBox6.Image = L_eye;


        }
        // MRD1 radioButton
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            Point pupil = new Point((int)pupil_x, (int)pupil_y);
            Point upperEyelid = new Point((int)pupil_x, (int)maxParRight.above.FY(pupil_x));
            imageBox5.Image = measurementVisualize(MeasurementImage,upperEyelid,pupil, MRD1[0]);

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;

            pupil = new Point((int)pupil_x, (int)pupil_y);
            upperEyelid = new Point((int)pupil_x, (int)maxPar.above.FY(pupil_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, pupil, MRD1[1]);
        }
        // MRD2 radioButton
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            Point pupil = new Point((int)pupil_x, (int)pupil_y);
            Point lowerEyelid = new Point((int)pupil_x, (int)maxParRight.below.FY(pupil_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, pupil, lowerEyelid, MRD2[0]);

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;
            
            pupil = new Point((int)pupil_x, (int)pupil_y);
            lowerEyelid = new Point((int)pupil_x, (int)maxPar.below.FY(pupil_x));
            
            imageBox6.Image = measurementVisualize(MeasurementImage, pupil, lowerEyelid, MRD2[1]);
        }
        // PFH radioButton
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            Point upperEyelid = new Point((int)pupil_x, (int)maxParRight.above.FY(pupil_x));
            Point lowerEyelid = new Point((int)pupil_x, (int)maxParRight.below.FY(pupil_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, upperEyelid, lowerEyelid, PFH[0]);

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;

            upperEyelid = new Point((int)pupil_x, (int)maxPar.above.FY(pupil_x));
            lowerEyelid = new Point((int)pupil_x, (int)maxPar.below.FY(pupil_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, lowerEyelid, PFH[1]);
        }
        // PFW radioButton
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();

            Point right = Point.Round(maxParRight.right);
            Point right_end = new Point(right.X, right.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(right, right_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(right_end,2), LineColor, LineThickness);

            Point left = new Point((int)maxParRight.left.X, right.Y);
            Point left_end = new Point(left.X, left.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(left, left_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(left_end, 2), LineColor, LineThickness);

            String Text = PFW[0].ToString("#0.#0") + "mm";
            CvInvoke.PutText(MeasurementImage, Text, left_end,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);
            imageBox5.Image = MeasurementImage;

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();

            left = Point.Round(maxPar.left);
            left_end = new Point(left.X, left.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(left, left_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(left_end, 2), LineColor, LineThickness);

            right = new Point((int)maxPar.right.X, left.Y);
            right_end = new Point(right.X, right.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(right, right_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(right_end, 2), LineColor, LineThickness);

            Text = PFW[1].ToString("#0.#0") + "mm";
            CvInvoke.PutText(MeasurementImage, Text, left_end,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);

            imageBox6.Image = MeasurementImage;
        }
        // LEVATOR radioButton
        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            imageBox7.Visible = true;
            imageBox8.Visible = true;
            imageBox9.Visible = true;
            imageBox10.Visible = true;

            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            Point pupil = new Point((int)pupil_x, (int)pupil_y);
            Point upperEyelid = new Point((int)pupil_x, (int)maxParRight.above.FY(pupil_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, upperEyelid, upperEyelid, Levetor[0]);

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;

            pupil = new Point((int)pupil_x, (int)pupil_y);
            upperEyelid = new Point((int)pupil_x, (int)maxPar.above.FY(pupil_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, upperEyelid, Levetor[1]);

            imageBox7.Image = R_LevatorUp.Clone();
            imageBox8.Image = L_LevatorUp.Clone();
            imageBox9.Image = R_LevatorDown.Clone();
            imageBox10.Image = L_LevatorDown.Clone();
        }

        // PS radioButton
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            Point pupil = new Point((int)pupil_x, (int)(pupil_y - R_eye_Pupil.Radius));
            Point upperEyelid = new Point((int)pupil_x, (int)maxParRight.above.FY(pupil_x));

            MeasurementImage = measurementVisualize(MeasurementImage, pupil, upperEyelid, PtosisSeverity[0]);
            MeasurementImage.Draw(new CircleF(new PointF(pupil_x*16, pupil_y*16), (R_eye_Pupil.Radius + 1)*16), new Bgr(0, 0, 255), 1,LineType.AntiAlias,4);

            imageBox5.Image = MeasurementImage;

            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;

            pupil = new Point((int)pupil_x, (int)(pupil_y - L_eye_Pupil.Radius));
            upperEyelid = new Point((int)pupil_x, (int)maxPar.above.FY(pupil_x));

            MeasurementImage = measurementVisualize(MeasurementImage, pupil, upperEyelid, PtosisSeverity[1]);
            MeasurementImage.Draw(new CircleF(new PointF(pupil_x*16, pupil_y*16), (L_eye_Pupil.Radius + 1)*16), new Bgr(0, 0, 255), 1,LineType.AntiAlias,4);

            imageBox6.Image = MeasurementImage;
        }
        // OSA radioButton
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = R_eyeParticle.Clone();
            float pupil_x = R_PupilROI.X + R_eye_Pupil.Center.X;
            float pupil_y = R_PupilROI.Y + R_eye_Pupil.Center.Y;

            // Calculate OSA : Pupil Within Eyelids Area
            for (float i = pupil_x - R_eye_Pupil.Radius; i <= pupil_x + R_eye_Pupil.Radius; i += 0.1f)
            {
                for (float j = pupil_y - R_eye_Pupil.Radius; j <= pupil_y + R_eye_Pupil.Radius; j += 0.1f)
                {
                    if ((Math.Pow(pupil_x - i, 2) + Math.Pow(pupil_y - j, 2) < Math.Pow(R_eye_Pupil.Radius, 2)) &&
                         pupil_y - j < pupil_y - maxParRight.above.FY(i))
                    {
                        MeasurementImage.Draw(new CircleF(new PointF(i, j), 0), LineColor, 0);
                    }
                }
            }
            Point TextPoint = new Point((int)(pupil_x - R_eye_Pupil.Radius*2), (int)(pupil_y - R_eye_Pupil.Radius));
            String Text = OSA[0].ToString("#0.#0") + "mm^2";
            CvInvoke.PutText(MeasurementImage, Text, TextPoint,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);
            imageBox5.Image = MeasurementImage;


            /* L_eye */
            MeasurementImage = L_eyeParticle.Clone();
            pupil_x = L_PupilROI.X + L_eye_Pupil.Center.X;
            pupil_y = L_PupilROI.Y + L_eye_Pupil.Center.Y;

            // Calculate OSA : Pupil Within Eyelids Area
            for (float i = pupil_x - L_eye_Pupil.Radius; i <= pupil_x + L_eye_Pupil.Radius; i += 0.1f)
            {
                for (float j = pupil_y - L_eye_Pupil.Radius; j <= pupil_y + L_eye_Pupil.Radius; j += 0.1f)
                {
                    if ((Math.Pow(pupil_x - i, 2) + Math.Pow(pupil_y - j, 2) < Math.Pow(L_eye_Pupil.Radius, 2)) &&
                         pupil_y - j < pupil_y - maxPar.above.FY(i))
                    {
                        MeasurementImage.Draw(new CircleF(new PointF(i, j), 0), LineColor, 0);
                    }
                }
            }
            TextPoint = new Point((int)(pupil_x - L_eye_Pupil.Radius*2), (int)(pupil_y - L_eye_Pupil.Radius));
            Text = OSA[1].ToString("#0.#0") + "mm^2";
            CvInvoke.PutText(MeasurementImage, Text, TextPoint,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);
            imageBox6.Image = MeasurementImage;


        }

        private Image<Bgr, byte> measurementVisualize(Image<Bgr,byte> img_measurement,Point upAnchor,Point downAnchor, double value) {
            Point upAnchor_end = new Point(upAnchor.X + LineLength, upAnchor.Y);
            Point downAnchor_end = new Point(downAnchor.X + LineLength, downAnchor.Y);
            Point upMark = new Point(upAnchor.X + LineLength, upAnchor.Y - (upAnchor.Y/6));
            

            // upper line
            img_measurement.Draw(new LineSegment2D(upAnchor, upAnchor_end), LineColor, LineThickness);
            img_measurement.Draw(new CircleF(upAnchor_end, 2), LineColor, LineThickness);
            // lower line
            img_measurement.Draw(new LineSegment2D(downAnchor, downAnchor_end), LineColor, LineThickness);
            img_measurement.Draw(new CircleF(downAnchor_end, 2), LineColor, LineThickness);

            // Print measurement
            String Text = value.ToString("#0.#0") + "mm"; 
            CvInvoke.PutText(img_measurement, Text, upMark,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);

            return img_measurement;
        }

        private void measurementCalculate(Rectangle PupilROI, CircleF Pupil, Parcitle maxPar)
        {
            // Calculate MRD1, MRD2, PFH ,PFW ,PtosisSeverity
            float pupil_x = PupilROI.X + Pupil.Center.X;
            float pupil_y = PupilROI.Y + Pupil.Center.Y;
            double mrd1 = (pupil_y - maxPar.above.FY(pupil_x)) * mmperpixel;
            double mrd2 = (maxPar.below.FY(pupil_x) - pupil_y) * mmperpixel;
            if (mrd1 < 0) mrd1 = 0;
            if (mrd2 < 0) mrd2 = 0;
            double ps = Pupil.Radius * mmperpixel - mrd1;
            if (ps < 0) ps = 0;
            MRD1.Add(mrd1);
            MRD2.Add(mrd2);
            PFH.Add(MRD1[MRD1.Count-1] + MRD2[MRD2.Count-1]);
            PFW.Add((maxPar.right.X - maxPar.left.X) * mmperpixel);
            PtosisSeverity.Add(ps);

            double pupil_Area = 0.0;
            // Calculate OSA : Pupil Within Eyelids Area
            for (float i = pupil_x - Pupil.Radius; i <= pupil_x + Pupil.Radius; i += 0.1f)
            {
                for (float j = pupil_y - Pupil.Radius; j <= pupil_y + Pupil.Radius; j += 0.1f)
                {
                    if ((Math.Pow(pupil_x - i, 2) + Math.Pow(pupil_y - j, 2) < Math.Pow(Pupil.Radius, 2)) &&
                         pupil_y - j < pupil_y - maxPar.above.FY(i))
                    {
                        pupil_Area += 0.1;
                    }
                }
            }
            OSA.Add(pupil_Area * mmperpixel);

        }

        /// <summary>
        /// Sampling the point by 'Div' pixels from VectorOfPoint, collect as two List<PointF>, upper curve and lower curve
        /// </summary>
        /// <param name="Contour">The contour information</param>
        /// <param name="Div">The distance of each sampling point, a lower value indicate the point would be dense </param>
        /// <returns></returns>
        private List<List<PointF>> ContourSampling(VectorOfPoint Contour,int Div,String LorR) {
            
            int len = Contour.ToArray().Length;
            PointF[] ContourArray = new PointF[len];
            
            // Convert Point to PointF
            for (int i = 0; i < len; i++) {
                ContourArray[i] = Point2PointF(Contour[i]);
            }

            // The first item is Upper Curve CtrlPoints, the second item is Lower Curve CtrlPoints
            List<List<PointF>> CtrlPoints = new List<List<PointF>>(2);
            CtrlPoints.Add(new List<PointF>());
            CtrlPoints.Add(new List<PointF>());

            Rectangle rect = CvInvoke.BoundingRectangle(Contour);
            int Middle = rect.Top + rect.Height / 2;

            // Sort the point in ContourArray from left to right
            var ContourArraySort = from p in ContourArray orderby p.X ascending select p;
            ContourArray = ContourArraySort.ToArray();

            // To avoid Adding the same X coordinate point
            float CurX = 0, NextX = 0;
            bool isUpAdd = false, isDownAdd = false;

            for (int i = len/10; i < len*9/10 -1; i++) {
                if (CurX != NextX) {
                    isUpAdd = false;
                    isDownAdd = false;
                }
                PointF CurP = ContourArray[i];
                CurX = ContourArray[i].X;
                NextX = ContourArray[i + 1].X;

                // Each point is seperated by 'Div' pixels
                // Upper Curve
                if ((CurP.X % Div == Div - 1) && CurP.Y < Middle && !isUpAdd) {
                    CtrlPoints[0].Add(CurP);
                    isUpAdd = true;
                }// Lower Curve
                else if ((CurP.X % Div == Div - 1) && CurP.Y > Middle && !isDownAdd){
                    CtrlPoints[1].Add(CurP);
                    isDownAdd = true;
                }
            }
            //Console.WriteLine("CtrlPoints[0].Count : " + CtrlPoints[0].Count + " CtrlPoints[1].Count : " + CtrlPoints[1].Count);
            return CtrlPoints;
        }

        // A series of pre-process methods to get the 'main Contour' ( the largest one )
        private VectorOfPoint getContour(Image<Bgr, byte> img) {
            
            Image<Gray, byte> colorSubtract = img[2] - img[0]; // R - B
            CvInvoke.Normalize(colorSubtract, colorSubtract, 0, 255, NormType.MinMax);
            colorSubtract.Save("L_eyeRG.jpg");
            CvInvoke.Threshold(colorSubtract, colorSubtract, 10, 255, ThresholdType.Binary);
            colorSubtract.Save("L_eyeBin.jpg");
            colorSubtract = colorSubtract.Erode(1);
            colorSubtract = colorSubtract.Dilate(1);
            colorSubtract.Save("L_eyeMor.jpg");
            colorSubtract._Not();
            VectorOfVectorOfPoint Contours = new VectorOfVectorOfPoint();

            double maxArea = 0;
            int Inx = 0;

            CvInvoke.FindContours(colorSubtract, Contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            if (Contours.Size > 0)
            {
                for (int i = 0; i < Contours.Size; i++)
                {
                    double area = CvInvoke.ArcLength(Contours[i], true);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        Inx = i;
                    }
                }
                //CvInvoke.DrawContours(img, Contours, Inx, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
            }
            img.Save("L_eye.jpg");
            
            return Contours[Inx];
        }

        private void toolStripMenuItem6_Click_1(object sender, EventArgs e)
        {
            //imageBox3.Visible = true;
            //Image<Gray, Byte> m_SourceImage = My_Image1.Clone();

            //Image<Gray, Byte> m_ThresholdImage = new Image<Gray, Byte>(m_SourceImage.Size);

            //CvInvoke.Threshold(m_SourceImage, m_ThresholdImage, 0.0001,255.0, Emgu.CV.CvEnum.ThresholdType.Otsu);

            //Image<Gray, float> m_CornerImage = new Image<Gray, float>(m_SourceImage.Size);

            //CvInvoke.CornerHarris(m_SourceImage, m_CornerImage, 7, 3, 0.04);
            //CvInvoke.Normalize(m_CornerImage, m_CornerImage, 0, 255, NormType.MinMax, DepthType.Cv32F);  //标准化处理
            //double min = 0, max = 0;
            //Point minp = new Point(0, 0);
            //Point maxp = new Point(0, 0);
            //CvInvoke.MinMaxLoc(m_CornerImage, ref min, ref max, ref minp, ref maxp);
            //double scale = 255 / (max - min);
            //double shift = min * scale;
            //CvInvoke.ConvertScaleAbs(m_CornerImage, m_SourceImage, scale, shift);//进行缩放，转化为byte类型


            //Bitmap bmpCorner = m_SourceImage.ToBitmap();

            //Bitmap bmpThreshold = m_ThresholdImage.ToBitmap();
            //imageBox1.Image = My_Image1;
            //imageBox3.Image = m_SourceImage;
            double Y = 0;
            double Cb = 0;
            double Cr = 0;


            double Cr2 = 0;//Cr平方
            double Cr_Cb = 0;//Cr除Cb

            try
            {
                Bitmap image1 = new Bitmap(My_Image2.Bitmap);
                int w = image1.Width;
                int h = image1.Height;
                int pixelcount = 0;
                double MouthMap = 0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color color = image1.GetPixel(x, y);
                        Y = (int)((0.257 * color.R) + (0.504 * color.G) + (0.098 * color.B) + 16);
                        Cb = (int)(-(0.148 * color.R) - (0.291 * color.G) + (0.439 * color.B) + 128);
                        Cr = (int)((0.439 * color.R) - (0.368 * color.G) - (0.071 * color.B) + 128);
                        
                        Cr2 = Cr2 + Cr * Cr;
                        Cr_Cb = Cr_Cb + (Cr / Cb);
                        pixelcount = pixelcount+1;
                    }
                }
                MessageBox.Show(Cr2.ToString() + "," + Cr_Cb.ToString() + "," + pixelcount.ToString());
                double n = 0.95 * ((Cr2 / pixelcount) / (Cr_Cb / pixelcount));
                MessageBox.Show(n.ToString());

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color color = image1.GetPixel(x, y);
                        Y = (int)((0.257 * color.R) + (0.504 * color.G) + (0.098 * color.B) + 16);
                        Cb = (int)(-(0.148 * color.R) - (0.291 * color.G) + (0.439 * color.B) + 128);
                        Cr = (int)((0.439 * color.R) - (0.368 * color.G) - (0.071 * color.B) + 128);
                        
                        Cr2 = Cr * Cr;
                        Cr_Cb = Cr / Cb;

                        MouthMap = Cr2 * (Cr2 - n * Cr_Cb) * (Cr2 - n * Cr_Cb);
                        MessageBox.Show(MouthMap.ToString());
                        image1.SetPixel(x, y, Color.FromArgb(color.R, color.G, color.B));
                    }
                }
                MessageBox.Show(Cb.ToString());
                imageBox3.Image = new Image<Bgr,byte>(image1);
                imageBox3.Visible = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (form4 == null || form4.IsDisposed)
            {
                form4 = new Form4();
                form4.Tag = this;
                form4.Show();//鎖定置頂
            }
        }

        

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)//拍照
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            imageBox3.Visible = false;
            imageBox5.Visible = false;
            imageBox6.Visible = false;
            pictureBox1.Visible = false;


            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;

            Form3 form3 = new Form3();
            form3.Tag = this;
            form3.TopMost = true;
            form3.ShowDialog();//鎖定置頂
        }
        Form4 form4;
        private void toolStripMenuItem18_Click(object sender, EventArgs e)//關於...
        {
            
        }
        

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
                Bitmap Source = new Bitmap(My_Image2.Bitmap);
                Bitmap Result = (Bitmap)Source.Clone();
                double xD = 0, yD = 0;
                double CCT_x = 0, CCT_y = 0, CCT_n = 0;
                double Tc = 3999;
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
            //Result.Save(pic_path + Picture.Text + "_色溫校正" + comboBox1.Text);
            imageBox1.Image = new Image<Bgr,Byte>(Result);
            //Source.Save(pic_path + Picture.Text + " (色溫校正結果)" + comboBox1.Text);   
            My_Image2 = new Image<Bgr, byte>(Result);
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> noseshape = My_Image2.Clone();
            Image<Gray, byte> noseshapegray = My_Image1.Clone();
            noseshapegray = noseshapegray.SmoothMedian(3);
            Rectangle noserange = new Rectangle(faces[0].X + (faces[0].Width / 6) * 2, faces[0].Y + faces[0].Height*50 / 100, faces[0].Width / 3, faces[0].Height / 7);//取鼻子範圍
            Image<Gray, float> sobelX = noseshapegray.Sobel(1, 0, 3);
            Image<Gray, float> sobelY = noseshapegray.Sobel(0, 1, 3);
            Image<Gray, Byte> sobelXByte = sobelX.Convert<Gray, Byte>();
            Image<Gray, Byte> sobelYByte = sobelY.Convert<Gray, Byte>();
            sobelX = sobelX.AbsDiff(new Gray(0));
            sobelY = sobelY.AbsDiff(new Gray(0));
            Image<Gray, float> sobel = sobelX + sobelY;
            double[] mins, maxs;
            Point[] minLoc, maxLoc;
            sobel.MinMax(out mins, out maxs, out minLoc, out maxLoc);
            Image<Gray, Byte> sobelImage = sobel.ConvertScale<byte>(255 / maxs[0], 0);
            sobelImage._ThresholdBinary(new Gray(30), new Gray(255));
            sobelImage.ROI = noserange;
            noseshape.ROI = noserange;
            Image<Bgr, Byte> sobelImagetoBgr = sobelImage.Convert<Bgr, Byte>();//Sobel影像轉Bgr
            imageBox2.Image = sobelImagetoBgr;
            Bitmap sobelBitmap = sobelImagetoBgr.Bitmap;
            Bitmap noseshapeBitmap = noseshape.Bitmap;

            int noseshapew = noseshapeBitmap.Width, noseshapeh = noseshapeBitmap.Height;

            for (int y = 0; y < noseshapeh; y++)//find eyebrow
            {
                for (int x = 0; x < noseshapew; x++)
                {
                    Color color = sobelBitmap.GetPixel(x, y);

                    if (color == Color.FromArgb(255, 255, 255) )
                    {
                        noseshapeBitmap.SetPixel(x,y, Color.FromArgb(255, 0, 0));
                    }
                }
            }
            Image<Gray, byte> noseshapecont = new Image<Gray, byte>(sobelBitmap);
            VectorOfVectorOfPoint contournoseshape = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(noseshapecont, contournoseshape, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Rectangle Boundboxcontournoseshape = Rectangle.Empty, CurrentLeftnosecontourshape = Rectangle.Empty;
            VectorOfPoint Leftnosecontour = new VectorOfPoint(), Rightnosecontour = new VectorOfPoint();
            for (int i = 0; i < contournoseshape.Size; i++)
            {
                CurrentLeftnosecontourshape = CvInvoke.BoundingRectangle(Leftnosecontour);
                Boundboxcontournoseshape = CvInvoke.BoundingRectangle(contournoseshape[i]);
                if (i == 0)
                {
                    Leftnosecontour = contournoseshape[i];
                }
                else if(Boundboxcontournoseshape.X< CurrentLeftnosecontourshape.X)
                {
                    Leftnosecontour = contournoseshape[i];
                }

            }
            SolidBrush drawBrush = new SolidBrush(Color.Green);
            StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            Graphics g = Graphics.FromImage(noseshapeBitmap);
            g.DrawString("+", new Font("Arial", 25), drawBrush, CurrentLeftnosecontourshape.X, CurrentLeftnosecontourshape.Y, sf);
            g.Dispose();


            noseshape.Bitmap = noseshapeBitmap;
            //noseshape.ROI = Rectangle.Empty;
            //imageBox2.Image = noseshape;
        }


    }
}

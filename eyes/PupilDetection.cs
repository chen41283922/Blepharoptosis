using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace eyes
{
    class PupilDetection
    {
        Image<Gray, byte> img_Gray;
        Image<Bgr, byte> img_Bgr;
        Image<Gray, byte> img_Threshold;
        Image<Gray, float> img_SobelX;
        Image<Gray, float> img_SobelY;
        Image<Gray, float> img_Sobel;
        Image<Gray, float> img_laplace;
        Image<Gray, byte> img_laplaceByte;
        public Image<Gray, Byte> sobelImage;
        Image<Gray, byte> img_Edge;
        Image<Gray, byte> img_Ada3;
        Image<Gray, byte> img_Ada5;
        Image<Gray, byte> img_Ada7;
        Image<Gray, byte> img_Ada9;
        Image<Gray, byte> img_Ada11;
        Image<Gray, byte> img_Ada13;
        Image<Gray, byte> img_Ada15;
        Image<Gray, byte> img_Ada17;
        Image<Gray, byte> img_Ada19;
        Image<Gray, byte> img_Ada21;
        Image<Gray, byte> img_Ada23;
        Image<Gray, byte> img_Ada35;
        Image<Bgr, byte> img_EdgeText;
        Image<Gray, byte> img_overlap;

        CircleF[] Circles;
        List<CircleF> list_Pupil;
        List<Image<Bgr, byte>> list_draw;
        
        int cyble_thickness = 0;
        public PupilDetection()
        {
            list_draw = new List<Image<Bgr, byte>>();

        }

        public List<CircleF> HoughCircles(Image<Bgr, byte> EyeRoi,// Input image
                                           double dp,// Resolution of the accumulator used to detect centers of the circles
                                           double minDist,//min detected circles distance 
                                           double CannyThres,// Canny high threshold
                                           double HoughThres,// Hough counting threshold
                                           int minRadius, // Circle Radius
                                           int maxRadius)
        {
            //轉灰階
            img_Gray = EyeRoi.Convert<Gray, Byte>();
            img_Gray._GammaCorrect(0.6);
            //CvInvoke.Normalize(img_Gray, img_Gray, 0, 255, NormType.MinMax);
            img_Bgr = EyeRoi.Clone();
            //img_Bgr._EqualizeHist();
            // Laplace 邊緣強化
            img_laplace = img_Gray.Convert<Gray, float>();
            //img_laplace = img_laplace.Laplace(1);
            //Convert to 8-bit image
            Point[] minLoc, maxLoc;
            double[] mins, maxs;
            img_laplace.MinMax(out mins, out maxs, out minLoc, out maxLoc);
            Console.WriteLine("mins : " + mins[0] + "maxs : " + maxs[0]);
            img_laplaceByte = img_laplace.ConvertScale<byte>(255 / maxs[0], 0);
            


            //二值化
            #region Adaptive Threshold
            img_Ada3 = img_Gray.Clone();
            img_Ada5 = img_Gray.Clone();
            img_Ada7 = img_Gray.Clone();
            img_Ada9 = img_Gray.Clone();
            img_Ada11 = img_Gray.Clone();
            img_Ada13 = img_Gray.Clone();
            img_Ada15 = img_Gray.Clone();
            img_Ada17 = img_Gray.Clone();
            img_Ada19 = img_Gray.Clone();
            img_Ada21 = img_Gray.Clone();
            img_Ada23 = img_Gray.Clone();
            img_Ada35 = img_Gray.Clone();
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada3, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 3, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada5, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 5, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada7, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 7, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada9, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 9, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada11, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 11, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada13, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 13, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada15, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 15, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada17, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 17, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada19, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 19, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada21, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 21, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada23, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 23, 0);
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada35, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 35, 0);
            #endregion
            
            img_Threshold = img_Gray.CopyBlank();

            img_Threshold = img_Ada23.Clone();

            //Median Filter 去除雜訊
            CvInvoke.MedianBlur(img_Threshold, img_Threshold, 3);

            // Canny 取得邊緣
            img_Edge = img_Gray.CopyBlank();
            img_Edge = img_Threshold.Canny(30, 90);

            VectorOfVectorOfPoint Contours = new VectorOfVectorOfPoint();
            List<VectorOfPoint> C = new List<VectorOfPoint>();

            CvInvoke.FindContours(img_Edge, Contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            //Image<Gray, Byte> blackImage = img_Gray.CopyBlank();
            img_Edge = img_Gray.CopyBlank();
            img_EdgeText = EyeRoi.CopyBlank();

            Console.WriteLine("(Pupil) img_Edge Contours.size : " + Contours.Size);
            double maxArea = 0;
            int Inx = 0;
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
                    //if ((area > 250) || (area < 150)) continue;
                    //if (area < 50)
                    //{
                    //    continue;
                    //}
                    Console.WriteLine("n : " + i + " area : " + area);
                    CvInvoke.DrawContours(img_Edge, Contours, i, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
                    CvInvoke.DrawContours(img_EdgeText, Contours, i, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
                    Rectangle rect = CvInvoke.BoundingRectangle(Contours[i]);
                    CvInvoke.PutText(img_EdgeText, area.ToString("###.#"), new Point(rect.X, rect.Y + rect.Height), Emgu.CV.CvEnum.FontFace.HersheyDuplex, 0.2, new Bgr(Color.Red).MCvScalar);
                    C.Add(Contours[i]);
                }
                //CvInvoke.DrawContours(img_Edge, Contours, Inx, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);


                try
                {
                    do
                    {
                        Circles = CvInvoke.HoughCircles(img_Edge, // Input image
                                    HoughType.Gradient,// Method
                                    dp,//Resolution of the accumulator used to detect centers of the circles
                                    minDist,//min distance 
                                    CannyThres,// Canny high threshold
                                    HoughThres,// Hough counting threshold
                                    minRadius,//min radius
                                    maxRadius//max radius
                                    );

                        list_Pupil = new List<CircleF>();

                        HoughThres -= 5;
                        if (HoughThres <= 0)
                        {
                            Console.WriteLine("cythred=0");
                            //Only save the first 30th  Circles
                            int limit = 0;
                            foreach (CircleF cy in Circles)
                            {
                                list_Pupil.Add(cy);
                                limit++;
                                if (limit == 30 || limit > Circles.Length) break;
                            }

                        }

                    } while (list_Pupil.Count <= 5);
                }
                catch (Emgu.CV.Util.CvException ex) { Console.WriteLine(ex); }

                //list_Pupil = CircleVerify(list_Pupil, img_Edge, img_Threshold

                // by  gradient 
                list_Pupil = CircleVerify(list_Pupil, img_Edge, img_Threshold, img_laplaceByte);

            }
            else
            {
                MessageBox.Show("無法偵測瞳孔，請重新拍攝 \r\n (Contours.size = 0)");
                return null;
            }

            img_Sobel = img_laplaceByte.Convert<Gray, float>();
            img_SobelX = img_laplaceByte.Sobel(1, 0, 3);
            img_SobelY = img_laplaceByte.Sobel(0, 1, 3);
            
            img_SobelX = img_SobelX.AbsDiff(new Gray(0));
            img_SobelY = img_SobelY.AbsDiff(new Gray(0));
            img_Sobel = img_SobelX+ img_SobelY;
            //Find sobel min or max value
            
            //Find sobel min or max value position
            img_Sobel.MinMax(out mins, out maxs, out minLoc, out maxLoc);
            //Conversion to 8-bit image
            sobelImage = img_Sobel.ConvertScale<byte>(255 / maxs[0], 0);
            
            //CvInvoke.Threshold(sobelImage, sobelImage, 100, 255, ThresholdType.Binary);
            //sobelImage = sobelImage.Canny(400, 500);
            //Get binary image
            //sobelImage = img_Gray.Copy();



            //CvInvoke.MedianBlur(sobelImage, sobelImage,3);



            return list_Pupil;


        }
        
        private List<CircleF> CircleVerify(List<CircleF> Circles, Image<Gray, byte> edge, Image<Gray, byte> black, Image<Gray, byte> gray)
        {
            if (Circles.Count < 1)
            {
                Console.WriteLine("No cy to verify");
                return Circles;
            }
            else
            {
                Console.WriteLine("cy num:" + Circles.Count);
            }

            List<CircleF> result = new List<CircleF>();

            Dictionary<CircleF, double> eyeEdge = new Dictionary<CircleF, double>();
            Dictionary<CircleF, double> eyeBlack = new Dictionary<CircleF, double>();


            foreach (CircleF cy in Circles)
            {
                //Dictionary initialization
                eyeEdge.Add(cy, 0);
                eyeBlack.Add(cy, 0);
                
                //int cyedg_thickness = 2;
                Image<Gray, byte> cyedg = edge.CopyBlank();
                //cyedg.Draw(cy, new Gray(255), cyedg_thickness);
                for (double theta = 0.0; theta < 2.0; theta += 0.01)
                {
                    //if ((theta > 0.25 && theta < 0.75) || theta > 1.0 && theta < 1.99) continue;
                    if (theta > 1.0 && theta < 1.99) continue;
                    double rx = cy.Radius * Math.Cos(theta * Math.PI);
                    double ry = cy.Radius * Math.Sin(theta * Math.PI);
                    cyedg.Draw(new CircleF(new PointF(cy.Center.X + (float)rx, cy.Center.Y + (float)ry), 0), new Gray(255), 0);

                    rx = (cy.Radius - 1) * Math.Cos(theta * Math.PI);
                    ry = (cy.Radius - 1) * Math.Sin(theta * Math.PI);
                    cyedg.Draw(new CircleF(new PointF(cy.Center.X + (float)rx, cy.Center.Y + (float)ry), 0), new Gray(255), 0);

                }

                double Out = 0;
                double In = 0;

                for (double theta = 0.0; theta < 2.0; theta += 0.01)
                {
                    //if ((theta > 0.25 && theta < 0.75) || theta > 1.0 && theta < 1.99) continue;
                    if (theta > 1.0 && theta < 1.99) continue;
                    double rx_out = (cy.Radius + 1) * Math.Cos(theta * Math.PI);
                    double ry_out = (cy.Radius + 1) * Math.Sin(theta * Math.PI);

                    double rx_in = (cy.Radius - 1) * Math.Cos(theta * Math.PI);
                    double ry_in = (cy.Radius - 1) * Math.Sin(theta * Math.PI);

                    //if (i + rx_out < 0 || i + rx_out > gray.Width) continue;
                    if (cy.Center.Y + ry_out < 0 || cy.Center.Y + ry_out > gray.Height) continue;
                    if (cy.Center.X + rx_out < 0 || cy.Center.X + rx_out > gray.Width) continue;
                    Out = gray[(int)(cy.Center.Y + ry_out), (int)(cy.Center.X + rx_out)].Intensity;
                    In = gray[(int)(cy.Center.Y + ry_in), (int)(cy.Center.X + rx_in)].Intensity;

                    if ((Out - In) > 30)
                    {
                        eyeEdge[cy] += 1;
                    }
                }


                Image<Gray, byte> cybla = edge.CopyBlank();
                cybla.Draw(cy, new Gray(255), cyble_thickness);


                //Calculate the Similarity based on the edge and black 
                for (int i = (int)(cy.Center.X - cy.Radius + 1); i <= (int)(cy.Center.X + cy.Radius + 1) && i < edge.Width; ++i)
                {
                    for (int j = (int)(cy.Center.Y - cy.Radius + 1); j <= (int)(cy.Center.Y + cy.Radius + 1) && j < edge.Height; ++j)
                    {
                        if (i < 0) i = 0; if (j < 0) j = 0;

                        //if (cyedg[j, i].Intensity == 255 && edge[j, i].Intensity == 255)
                        //{
                        //    eyeEdge[cy] += 1;
                        //}
                        if (cybla[j, i].Intensity == 255 && black[j, i].Intensity == 0)
                        {
                            eyeBlack[cy] += 1;
                        }
                    }
                }


            }

            Dictionary<CircleF, double> Eye_evaluate = new Dictionary<CircleF, double>();

            foreach (CircleF cy in Circles)
            {
                // black ratio = black pixels of cy / Area of cy
                eyeBlack[cy] /= cy.Area;
                Eye_evaluate[cy] = eyeEdge[cy] * eyeBlack[cy];

            }

            // Sort the Eye_evaluate from highest to lowest
            var dicSort = from objDic in Eye_evaluate orderby objDic.Value descending select objDic;

            foreach (var item in dicSort)
            {
                result.Add(item.Key);
                //Console.WriteLine("Key : " + count++ + " Value : " + item.Value.ToString("0.###"));

                Console.WriteLine("r : " + item.Key.Radius.ToString("0.###") +
                 " edg[" + Circles.IndexOf(item.Key) + "] : " + eyeEdge[item.Key].ToString() +
                 " bla[" + Circles.IndexOf(item.Key) + "] : " + eyeBlack[item.Key].ToString("0.###") +
                 " Eye_evaluate : " + Eye_evaluate[item.Key].ToString("0.###"));

            }


            return result;
        }

        
        public void DrawEyeRoi(List<CircleF> list_LeftPupil, List<CircleF> list_RightPupil, ref Image<Bgr, byte> draw, ref Image<Bgr, byte> L_eye, ref Image<Bgr, byte> R_eye, Rectangle R_PupilROI, Rectangle L_PupilROI)
        {
            Bgr color = new Bgr(Color.Red);

            // Draw and save left circle of all list_LeftPupil
            foreach (CircleF cy in list_LeftPupil)
            {
                Image<Bgr, byte> drawCircle = draw.Clone();
                float X = cy.Center.X + L_eye.ROI.X + L_PupilROI.X;
                float Y = cy.Center.Y + L_eye.ROI.Y + L_PupilROI.Y;
                //Circle
                drawCircle.Draw(new CircleF(new PointF(X, Y), cy.Radius+1), color, 1);
                //Center
                drawCircle.Draw(new CircleF(new PointF(X, Y), 0), color, 5);
                list_draw.Add(drawCircle);
            }
            // Draw right circle
            int count = 0;
            foreach (CircleF cy in list_RightPupil)
            {
                if (list_RightPupil.IndexOf(cy) < list_LeftPupil.Count)
                {
                    Image<Bgr, byte> drawCircle = draw.Clone();
                    float X = cy.Center.X + R_eye.ROI.X + R_PupilROI.X;
                    float Y = cy.Center.Y + R_eye.ROI.Y + R_PupilROI.Y;
                    //Circle
                    list_draw[list_RightPupil.IndexOf(cy)].Draw(new CircleF(new PointF(X, Y), cy.Radius+1), color, 1);
                    //Center
                    list_draw[list_RightPupil.IndexOf(cy)].Draw(new CircleF(new PointF(X, Y), 0), color, 5);
                    list_draw[list_RightPupil.IndexOf(cy)].Save(count++ + ".jpg");
                }
            }

            //Filter by Center.Y and Radius
            for (int i = 0; i < 20 && list_LeftPupil.Count > 0 && list_RightPupil.Count > 0; i++)
            {

                if (list_RightPupil[0].Center.Y - list_LeftPupil[0].Center.Y > 40)
                {
                    list_LeftPupil.RemoveAt(0);
                    Console.WriteLine("L-R Center.Y >40");
                }
                else if (list_LeftPupil[0].Center.Y - list_RightPupil[0].Center.Y > 40)
                {
                    list_RightPupil.RemoveAt(0);
                    Console.WriteLine("L-R Center.Y >40");
                }
                else if (list_LeftPupil[0].Radius - list_RightPupil[0].Radius > 1.5)
                {
                    list_LeftPupil.RemoveAt(0);
                    Console.WriteLine("L-R Radius >1.5");
                }
                else if (list_RightPupil[0].Radius - list_LeftPupil[0].Radius > 1.5)
                {
                    list_RightPupil.RemoveAt(0);
                    Console.WriteLine("L-R Radius >1.5");
                }
                else
                    break;
            }


            float Circle_X, Circle_Y;
            float Circle_radius;
            // Show the best result
            if (list_LeftPupil.Count != 0)
            {
                Circle_X = list_LeftPupil[0].Center.X + L_PupilROI.X;
                Circle_Y = list_LeftPupil[0].Center.Y + L_PupilROI.Y;
                Circle_radius = list_LeftPupil[0].Radius + 1;

                // Draw Circle
                L_eye.Draw(new CircleF(new PointF(Circle_X*16, Circle_Y*16), Circle_radius*16), color, 1, LineType.AntiAlias, 4);
                //Draw center
                L_eye.Draw(new CircleF(new PointF(Circle_X, Circle_Y), 0), color, 3);
            }
            if (list_RightPupil.Count != 0)
            {
                Circle_X = list_RightPupil[0].Center.X + R_PupilROI.X;
                Circle_Y = list_RightPupil[0].Center.Y + R_PupilROI.Y;
                Circle_radius = list_RightPupil[0].Radius + 1;

                // Draw Circle
                R_eye.Draw(new CircleF(new PointF(Circle_X*16, Circle_Y*16), Circle_radius*16), color, 1,LineType.AntiAlias,4);
                //Draw center
                R_eye.Draw(new CircleF(new PointF(Circle_X, Circle_Y), 0), color, 3);
            }


        }

        public void SavePreprocess(List<CircleF> list_Pupil, String LorR)
        {

            if (img_Gray != null && img_Threshold != null && img_Edge != null)
            {
                img_Gray.Save(LorR + "\\" + "img_Gray_" + LorR + ".jpg");
                img_Bgr.Save(LorR + "\\" + "img_Bgr_" + LorR + ".jpg");
                img_Threshold.Save(LorR + "\\" + "img_Threshold" + LorR + ".jpg");
                img_Edge.Save(LorR + "\\" + "img_Edge" + LorR + ".jpg");
                img_Ada3.Save(LorR + "\\" + "img_Ada3" + LorR + ".jpg");
                img_Ada5.Save(LorR + "\\" + "img_Ada5" + LorR + ".jpg");
                img_Ada7.Save(LorR + "\\" + "img_Ada7" + LorR + ".jpg");
                img_Ada9.Save(LorR + "\\" + "img_Ada9" + LorR + ".jpg");
                img_Ada11.Save(LorR + "\\" + "img_Ada11" + LorR + ".jpg");
                img_Ada13.Save(LorR + "\\" + "img_Ada13" + LorR + ".jpg");
                img_Ada15.Save(LorR + "\\" + "img_Ada15" + LorR + ".jpg");
                img_Ada17.Save(LorR + "\\" + "img_Ada17" + LorR + ".jpg");
                img_Ada19.Save(LorR + "\\" + "img_Ada19" + LorR + ".jpg");
                img_Ada21.Save(LorR + "\\" + "img_Ada21" + LorR + ".jpg");
                img_Ada23.Save(LorR + "\\" + "img_Ada23" + LorR + ".jpg");
                img_Ada35.Save(LorR + "\\" + "img_Ada35" + LorR + ".jpg");
                img_Sobel.Save(LorR + "\\" + "img_Sobel" + LorR + ".jpg");
                img_SobelX.Save(LorR + "\\" + "img_SobelX" + LorR + ".jpg");
                img_SobelY.Save(LorR + "\\" + "img_SobelY" + LorR + ".jpg");
                
                img_laplace.Save(LorR + "\\" + "img_laplace" + LorR + ".jpg");
                img_laplaceByte.Save(LorR + "\\" + "img_laplaceByte" + LorR + ".jpg");

                sobelImage.Save(LorR + "\\" + "sobelImage" + LorR + ".jpg");

                img_EdgeText.Save(LorR + "\\" + "img_EdgeText" + LorR + ".jpg");

                Mat kernelOp = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));
                img_Ada35._MorphologyEx(MorphOp.Open, kernelOp, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));
                img_Ada35.Save(LorR + "\\" + "img_Ada35Morph" + LorR + ".jpg");


                img_overlap = img_Gray.CopyBlank();
                CvInvoke.AddWeighted(img_Edge, 0.3, img_Gray, 0.7, 0, img_overlap);
                img_overlap.Save(LorR + "\\" + "img_overlap" + LorR + ".jpg");


            }

            Draw_cyedg_cybla(list_Pupil, LorR + "\\", img_Edge, img_Threshold);

        }

        //Save the circle & eye overlapping image for debugging purpose , Optional
        private void Draw_cyedg_cybla(List<CircleF> list_Pupil, String Directory, Image<Gray, byte> Edge, Image<Gray, byte> Black)
        {
            int count = 0;
            foreach (CircleF cy in list_Pupil)
            {
                Image<Gray, byte> cyedg = new Image<Gray, byte>(img_Edge.Width, img_Edge.Height, new Gray(255));
                //cyedg.Draw(cy, new Gray(0), cyedg_thickness);
                
                for (double theta = 0.0; theta < 2.0; theta += 0.01)
                {
                    //if ((theta > 0.25 && theta < 0.75) || theta > 1.0 && theta < 1.99) continue;
                    if ( theta > 1.0 && theta < 1.99) continue;
                    double rx = cy.Radius * Math.Cos(theta * Math.PI);
                    double ry = cy.Radius * Math.Sin(theta * Math.PI);
                    cyedg.Draw(new CircleF(new PointF(cy.Center.X + (float)rx, cy.Center.Y + (float)ry), 0), new Gray(0), 0);

                    rx = (cy.Radius - 1) * Math.Cos(theta * Math.PI);
                    ry = (cy.Radius - 1) * Math.Sin(theta * Math.PI);
                    cyedg.Draw(new CircleF(new PointF(cy.Center.X + (float)rx, cy.Center.Y + (float)ry), 0), new Gray(0), 0);

                    //rx = (cy.Radius + 1) * Math.Cos(theta * Math.PI);
                    //ry = (cy.Radius + 1) * Math.Sin(theta * Math.PI);
                    //cyedg.Draw(new CircleF(new PointF(cy.Center.X + (float)rx, cy.Center.Y + (float)ry), 0), new Gray(0), 0);
                }



                CvInvoke.AddWeighted(Edge, 0.5, cyedg, 0.5, 0, cyedg);
                cyedg.Save(Directory + "cyedg" + count + ".jpg");

                Image<Gray, byte> cybla = new Image<Gray, byte>(img_Edge.Width, img_Edge.Height, new Gray(255));
                cybla.Draw(cy, new Gray(0), cyble_thickness);
                CvInvoke.AddWeighted(Black, 0.5, cybla, 0.5, 0, cybla);
                cybla.Save(Directory + "cybla" + count + ".jpg");

                count++;
            }

        }

        public Image<Gray,byte> Get_imgThreshold() {
            if (img_Threshold != null)
                return img_Threshold;
            else
                return null;
        }
        public Image<Gray, byte> Get_imgOverlap()
        {
            if (img_overlap != null)
                return img_overlap;
            else
                return null;
        }

    }
}

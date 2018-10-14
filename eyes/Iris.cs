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
    class Iris
    {
        CircleF iris;

        Image<Gray, byte> img_Gray;
        Image<Bgr, byte> img_Bgr;
        Image<Gray, byte> img_Threshold;
        Image<Gray, float> img_SobelX;
        Image<Gray, float> img_SobelY;
        Image<Gray, float> img_Sobel;
        Image<Gray, float> img_laplace;
        Image<Gray, byte> img_laplaceByte;
        Image<Gray, byte> sobelImage;
        Image<Gray, byte> img_Edge;
        Image<Gray, byte> img_Ada;

        Image<Bgr, byte> img_EdgeText;
        Image<Gray, byte> img_overlap;

        CircleF[] Circles;
        List<CircleF> list_Iris;
        List<Image<Bgr, byte>> list_draw;
        
        int cyble_thickness = 0;
        public Iris()
        {
            list_draw = new List<Image<Bgr, byte>>();
            iris = new CircleF();
        }

        public CircleF get_Iris() { return this.iris; }

        public List<CircleF> HoughCircles(Image<Bgr, byte> EyeRoi,// Input image
                                           int minRadius, // Circle Radius
                                           int maxRadius,
                                           double dp = 0.1,// Resolution of the accumulator used to detect centers of the circles
                                           double minDist = 0.01,//min detected circles distance 
                                           double CannyThres = 80,// Canny high threshold
                                           double HoughThres = 80// Hough counting threshold
                                           )
        {
            //轉灰階
            img_Gray = EyeRoi.Convert<Gray, Byte>();
            img_Gray._GammaCorrect(0.6);
            img_Bgr = EyeRoi.Clone();
            // Laplace 邊緣強化
            img_laplace = img_Gray.Convert<Gray, float>();
            //Convert to 8-bit image
            Point[] minLoc, maxLoc;
            double[] mins, maxs;
            img_laplace.MinMax(out mins, out maxs, out minLoc, out maxLoc);
            img_laplaceByte = img_laplace.ConvertScale<byte>(255 / maxs[0], 0);

            img_Sobel = img_laplaceByte.Convert<Gray, float>();
            img_SobelX = img_laplaceByte.Sobel(1, 0, 3);
            img_SobelY = img_laplaceByte.Sobel(0, 1, 3);

            img_SobelX = img_SobelX.AbsDiff(new Gray(0));
            img_SobelY = img_SobelY.AbsDiff(new Gray(0));
            img_Sobel = img_SobelX + img_SobelY;
            //Find sobel min or max value

            //Find sobel min or max value position
            img_Sobel.MinMax(out mins, out maxs, out minLoc, out maxLoc);
            //Conversion to 8-bit image
            sobelImage = img_Sobel.ConvertScale<byte>(255 / maxs[0], 0);


            //Adaptive Threshold
            img_Ada = img_Gray.Clone();
            CvInvoke.AdaptiveThreshold(img_laplaceByte, img_Ada, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 7, 0);
            img_Threshold = img_Ada.Clone();

            //Median Filter
            CvInvoke.MedianBlur(img_Threshold, img_Threshold, 3);

            // Canny
            img_Edge = img_Gray.CopyBlank();
            img_Edge = img_Threshold.Canny(30, 90);

            VectorOfVectorOfPoint Contours = new VectorOfVectorOfPoint();
            List<VectorOfPoint> C = new List<VectorOfPoint>();

            CvInvoke.FindContours(img_Edge, Contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            img_Edge = img_Gray.CopyBlank();
            img_EdgeText = EyeRoi.CopyBlank();

            double maxLen = 0;
            int Inx = 0;
            if (Contours.Size > 0)
            {
                for (int i = 0; i < Contours.Size; i++)
                {
                    double len = CvInvoke.ArcLength(Contours[i], true);
                    if (len > maxLen)
                    {
                        maxLen = len;
                        Inx = i;
                    }
                    CvInvoke.DrawContours(img_Edge, Contours, i, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
                    CvInvoke.DrawContours(img_EdgeText, Contours, i, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
                    Rectangle rect = CvInvoke.BoundingRectangle(Contours[i]);
                    CvInvoke.PutText(img_EdgeText, len.ToString("###.#"), new Point(rect.X, rect.Y + rect.Height), Emgu.CV.CvEnum.FontFace.HersheyDuplex, 0.2, new Bgr(Color.Red).MCvScalar);
                    C.Add(Contours[i]);
                }

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

                        list_Iris = new List<CircleF>();

                        HoughThres -= 5;
                        if (HoughThres <= 0)
                        {
                            Console.WriteLine("cythred=0");
                            //Only save the first 30th  Circles
                            int limit = 0;
                            foreach (CircleF cy in Circles)
                            {
                                list_Iris.Add(cy);
                                limit++;
                                if (limit == 60 || limit > Circles.Length) break;
                            }

                        }

                    } while (list_Iris.Count <= 5);
                }
                catch (Emgu.CV.Util.CvException ex) { Console.WriteLine(ex); }

                // by  gradient 
                list_Iris = CircleVerify(list_Iris);

            }
            else
            {
                MessageBox.Show("無法偵測瞳孔，請重新拍攝 \r\n (Contours.size = 0)");
                return null;
            }

            if (list_Iris.Count != 0) {
                this.iris = list_Iris[0];
            }

            return list_Iris;
        }

        private List<CircleF> CircleVerify(List<CircleF> Circles)
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

            Dictionary<CircleF, double> WeightSum = new Dictionary<CircleF, double>();

            foreach (CircleF cy in Circles)
            {
                // initialization
                WeightSum.Add(cy, 0);

                for (double theta = 0.0; theta < 2.0; theta += 0.01)
                {
                    
                    double rx = cy.Radius * Math.Cos(theta * Math.PI);
                    double ry = cy.Radius * Math.Sin(theta * Math.PI);
                    double Circle_X = cy.Center.X + rx + 0.5;
                    double Circle_Y = cy.Center.Y + ry + 0.5;

                    if (Circle_Y < 0 || Circle_Y > img_SobelX.Height) continue;
                    if (Circle_X < 0 || Circle_X > img_SobelX.Width) continue;
                    WeightSum[cy] += img_SobelX[(int)Circle_Y, (int)Circle_X].Intensity;

                    rx = (cy.Radius + 1) * Math.Cos(theta * Math.PI);
                    ry = (cy.Radius + 1) * Math.Sin(theta * Math.PI);
                    Circle_X = cy.Center.X + rx + 0.5;
                    Circle_Y = cy.Center.Y + ry + 0.5;
                    if (Circle_Y < 0 || Circle_Y > img_SobelX.Height) continue;
                    if (Circle_X < 0 || Circle_X > img_SobelX.Width) continue;
                    WeightSum[cy] += img_SobelX[(int)Circle_Y, (int)Circle_X].Intensity;

                    //rx = (2) * Math.Cos(theta * Math.PI);
                    //ry = (2) * Math.Sin(theta * Math.PI);
                    //Circle_X = cy.Center.X + rx + 0.5;
                    //Circle_Y = cy.Center.Y + ry + 0.5;
                    //if (Circle_Y < 0 || Circle_Y > img_SobelX.Height) continue;
                    //if (Circle_X < 0 || Circle_X > img_SobelX.Width) continue;
                    //WeightSum[cy] += img_SobelX[(int)Circle_Y, (int)Circle_X].Intensity;

                    //rx = (cy.Radius + 2) * Math.Cos(theta * Math.PI);
                    //ry = (cy.Radius + 2) * Math.Sin(theta * Math.PI);
                    //if (cy.Center.Y + ry + 0.5 < 0 || cy.Center.Y + ry + 0.5 > img_SobelX.Height) continue;
                    //if (cy.Center.X + rx + 0.5 < 0 || cy.Center.X + rx + 0.5 > img_SobelX.Width) continue;
                    //WeightSum[cy] -= img_SobelX[(int)(cy.Center.Y + ry + 0.5), (int)(cy.Center.X + rx + 0.5)].Intensity;
                }
            }

            // Sort the Eye_evaluate from highest to lowest
            var dicSort = from objDic in WeightSum orderby objDic.Value descending select objDic;

            List<CircleF> result = new List<CircleF>();

            foreach (var item in dicSort)
            {
                result.Add(item.Key);
                //Console.WriteLine("Key : " + count++ + " Value : " + item.Value.ToString("0.###"));

                Console.WriteLine("r : " + item.Key.Radius.ToString("0.###") +
                 " WeightSum : " + WeightSum[item.Key].ToString("0.###"));

            }

            return result;
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

        
        public void DrawEyeRoi(List<CircleF> list_LeftPupil, List<CircleF> list_RightPupil, ref Image<Bgr, byte> draw, Image<Bgr, byte> L_eye, Image<Bgr, byte> R_eye, Rectangle R_PupilROI, Rectangle L_PupilROI)
        {
            Bgr color = new Bgr(Color.Red);

            // Draw and save left circle of all list_LeftPupil
            foreach (CircleF cy in list_LeftPupil)
            {
                Image<Bgr, byte> drawCircle = draw.Clone();
                float X = cy.Center.X + L_eye.ROI.X + L_PupilROI.X + 0.5f;
                float Y = cy.Center.Y + L_eye.ROI.Y + L_PupilROI.Y + 0.5f;
                //Circle
                drawCircle.Draw(new CircleF(new PointF((int)X, (int)Y), (cy.Radius + 1)), color, 1);
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
                    float X = cy.Center.X + R_eye.ROI.X + R_PupilROI.X+0.5f;
                    float Y = cy.Center.Y + R_eye.ROI.Y + R_PupilROI.Y+0.5f;
                    //Circle
                    list_draw[list_RightPupil.IndexOf(cy)].Draw(new CircleF(new PointF((int)X, (int)Y), (cy.Radius + 1)), color, 1);
                    //Center
                    list_draw[list_RightPupil.IndexOf(cy)].Draw(new CircleF(new PointF(X, Y), 0), color, 5);
                    list_draw[list_RightPupil.IndexOf(cy)].Save(count++ + ".jpg");
                }
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
                img_Ada.Save(LorR + "\\" + "img_Ada7" + LorR + ".jpg");
                
                img_Sobel.Save(LorR + "\\" + "img_Sobel" + LorR + ".jpg");
                img_SobelX.Save(LorR + "\\" + "img_SobelX" + LorR + ".jpg");
                img_SobelY.Save(LorR + "\\" + "img_SobelY" + LorR + ".jpg");
                
                img_laplace.Save(LorR + "\\" + "img_laplace" + LorR + ".jpg");
                img_laplaceByte.Save(LorR + "\\" + "img_laplaceByte" + LorR + ".jpg");

                sobelImage.Save(LorR + "\\" + "sobelImage" + LorR + ".jpg");

                img_EdgeText.Save(LorR + "\\" + "img_EdgeText" + LorR + ".jpg");

                
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
                    //if ( theta > 1.0 && theta < 1.99) continue;
                    double rx = cy.Center.X + cy.Radius * Math.Cos(theta * Math.PI)+0.5;
                    double ry = cy.Center.Y + cy.Radius * Math.Sin(theta * Math.PI)+0.5;
                    cyedg.Draw(new CircleF(new PointF((int)rx, (int)ry), 0), new Gray(0), 0);

                    rx = cy.Center.X + (cy.Radius + 1) * Math.Cos(theta * Math.PI)+0.5;
                    ry = cy.Center.Y + (cy.Radius + 1) * Math.Sin(theta * Math.PI)+0.5;
                    cyedg.Draw(new CircleF(new PointF((int)rx, (int)ry), 0), new Gray(0), 0);

                    rx = cy.Center.X + (2) * Math.Cos(theta * Math.PI) + 0.5;
                    ry = cy.Center.Y + (2) * Math.Sin(theta * Math.PI) + 0.5;
                    cyedg.Draw(new CircleF(new PointF((int)rx, (int)ry), 0), new Gray(0), 0);

                }



                CvInvoke.AddWeighted(img_SobelX.Convert<Gray,byte>(), 0.5, cyedg, 0.5, 0, cyedg);
                cyedg.Save(Directory + "cyedg" + count + ".jpg");

                Image<Gray, byte> cybla = new Image<Gray, byte>(img_Edge.Width, img_Edge.Height, new Gray(255));
                cybla.Draw(cy, new Gray(0), cyble_thickness);
                CvInvoke.AddWeighted(Black, 0.5, cybla, 0.5, 0, cybla);
                cybla.Save(Directory + "cybla" + count + ".jpg");

                count++;
            }

        }
        

    }
}

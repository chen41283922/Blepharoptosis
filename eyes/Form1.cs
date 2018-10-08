using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using bearing;
using SomeCalibrations;
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
        }
   
        VideoCapture webCam = null;

        public Rectangle[] faces;
        public Rectangle facesori;
        Rectangle facezoom;
  
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

        private static PointF Point2PointF(Point P)//Point轉PointF
        {
            PointF PF = new PointF
            {
                X = P.X,
                Y = P.Y
            };
            return PF;
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
            
            imageBox2.Image = null;
            imageBox5.Visible = true;
            imageBox5.Image = null;
            imageBox6.Visible = true;
            imageBox6.Image = null;
            label1.Text = null;
            label2.Text = null;
            label3.Text = null;
            imageBox3.Visible = false;
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
            CornerDetector.VPF("R", out R_PupilROI, out R_Pupil);
            // Get the Corner (PointF)
            R_CornerInner = CornerDetector.WVPF("R_eye_CornerL", ref ppff);
            R_CornerOuter = CornerDetector.WVPF("R_eye_CornerR", ref ppff);
            R_CornerInner.X += R_PupilROI.Right;
            R_CornerInner.Y += R_PupilROI.Top;


            // Draw R_eye Corner
            R_eye.Draw(new Cross2DF(R_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);
            R_eye.Draw(new Cross2DF(R_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);


            //---------------------------------------------------------------------------------------

            CornerDetector = new CornerDetection(L_eye);
            CornerDetector.VPF("L", out L_PupilROI, out L_Pupil);
            // Get the Corner (PointF)
            L_CornerOuter = CornerDetector.WVPF("L_eye_CornerL", ref ppff);
            L_CornerInner = CornerDetector.WVPF("L_eye_CornerR", ref ppff);

            L_CornerInner.Y += L_PupilROI.Y;

            // Draw L_eye Corner
            L_eye.Draw(new Cross2DF(L_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);
            L_eye.Draw(new Cross2DF(L_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);

            #endregion

            #region PupilMark

            // HoughCircle outcome
            List<CircleF> list_LeftPupil = new List<CircleF>();
            List<CircleF> list_RightPupil = new List<CircleF>();

            // For debuging 
            List<Image<Bgr, byte>> list_draw = new List<Image<Bgr, byte>>();

            // Parameter for HoughCircle
            int minRadius = 12; // hospital : 29
            int maxRadius = 16; // hospital : 38

            // Right Pupil Detect
            pupilDection_R = new PupilDetection();
            list_RightPupil = pupilDection_R.HoughCircles(R_Pupil, minRadius, maxRadius,0.1);
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
            pupilDection_L = new PupilDetection();
            list_LeftPupil = pupilDection_L.HoughCircles(L_Pupil, minRadius, maxRadius,0.1);
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

            timer2.Enabled = true;
            timer2.Start();

        }
        double[] sum = new double[4];
        Particle_parameter_for_fullimg ppff;
        PupilDetection pupilDection_R, pupilDection_L;
        CircleF L_eye_Pupil, R_eye_Pupil;
        Image<Bgr, Byte> turn;
        Parcitle par;
        Image<Bgr, byte> L_eye;/* Patient's left eye */
        Image<Bgr, byte> R_eye;/* Patient's right eye */
        Image<Bgr, byte> L_eyeParticle, R_eyeParticle;
        Image<Bgr, byte> ParticleDraw;
        PointF R_CornerInner, R_CornerOuter;/*  Patient's right eye corner*/
        PointF L_CornerInner, L_CornerOuter;/*  Patient's left eye corner */
        Rectangle L_PupilROI, R_PupilROI;/* Patient's Pupil ROI */
        List<List<PointF>> CtrlPoints = new List<List<PointF>>();

        Image<Bgr, byte> R_LevatorDown, L_LevatorDown;
        Image<Bgr, byte> R_LevatorUp, L_LevatorUp;
        public Image<Bgr, byte> LevatorFaceUp, LevatorFaceDown;

        // The first item is for R_eye , the second item is for L_eye
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
                par = new Parcitle(ParticleDraw,CtrlPoints, ppff.ContoursPoint, R_eye_Pupil, R_PupilROI, R_CornerInner, R_CornerOuter, "R");
                ParticleDraw.Draw(R_PupilROI, new Bgr(0, 0, 255), 1);
            }
            else if (loopcounter == 1)// L_eye
            {
                par = new Parcitle(ParticleDraw, CtrlPoints, ppff.ContoursPoint, L_eye_Pupil, L_PupilROI, L_CornerOuter, L_CornerInner, "L");
                ParticleDraw.Draw(L_PupilROI, new Bgr(0, 0, 255), 1);
            }
            
            
            double d1 = par.Gradient(ref ppff);
            double d2 = par.Saturation(ref ppff);
            double d3 = par.symmetric(ref ppff);
            double d4 = par.corner(ref ppff);
            double weightnow = par.getweight(d1, sum[0] / parcount, d2, sum[1] / parcount, d3, sum[2] / parcount, d4, sum[3] / parcount, loopcounter);
            sum[0] += d1;
            
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
                    
                }
            }
            else
            {
                if (weightnow < maxw || maxPar == null)
                {
                    maxw = weightnow;
                    maxPar = new Parcitle(CtrlPoints,par.getbasic(), par.getPoint(), par.left, par.right, "L");
                    maxd1 = d1;
                    
                }
            }
            timercounter++;
        }


        private void toolStripMenuItem20_Click(object sender, EventArgs e)//開檔
        {
            imageBox2.Visible = false;
            imageBox3.Visible = false;            
            imageBox5.Visible = false;
            imageBox6.Visible = false;


            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;

            panel1.Visible = false;


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

            //Image<Ycc, byte> eye_Ycc = img.Convert<Ycc, byte>();
            //eye_Ycc.Save("L_eyeYcc.jpg");
            //Image<Gray, byte> Ycc_Bin = img.Convert<Gray, byte>().CopyBlank();
            //eye_Ycc[2].Save("YCC2.jpg");
            //CvInvoke.Threshold(eye_Ycc[2], Ycc_Bin, 127, 255, ThresholdType.BinaryInv);
            //Ycc_Bin.Save("L_eye_Bin.jpg");

            img[0].Save("B.jpg");
            img[1].Save("G.jpg");
            img[2].Save("R.jpg");
            Image<Gray, byte> his = img[2].CopyBlank();
            CvInvoke.CLAHE(img[2], 5, new Size(2, 2), his);
            his.Save("R_his.jpg");
            Image <Gray, byte> colorSubtract = img[2] - img[0]; // R - B
            //CvInvoke.Normalize(colorSubtract, colorSubtract, 0, 255, NormType.MinMax);
            colorSubtract.Save("L_eyeRG.jpg");
            CvInvoke.Threshold(colorSubtract, colorSubtract, 10, 255, ThresholdType.Binary);
            colorSubtract.Save("L_eyeBin.jpg");
            colorSubtract = colorSubtract.Erode(1);
            colorSubtract = colorSubtract.Dilate(1);
            colorSubtract.Save("L_eye_Mor.jpg");
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
            imageBox3.Visible = false;
            imageBox5.Visible = false;
            imageBox6.Visible = false;


            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;

            Form3 form3 = new Form3();
            form3.Tag = this;
            form3.TopMost = true;
            form3.ShowDialog();//鎖定置頂
        }
        Form4 form4;
        
    }
}

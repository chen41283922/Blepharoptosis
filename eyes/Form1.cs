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
        

        public Image<Bgr, Byte> facecut;
        public Image<Bgr, Byte> facecutori;
        public Image<Gray, Byte> facecutorigray;
        public string filename,faceupfilename,facedownfilename;
        public Bitmap camera;

        public Point offset;
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

        // For timer procedure control
        /****************************************************************************/
        bool isEyesInit; /* Flag of eyes initialization */
        int timer_current = 0; /* Counter of timer */
        int timer_end = 30; /* Determine the times of the loop for each eye */
        int timer_stage = 0;/* 0 : R_eye
                               1 : L_eye
                               2 : Show the result */
        /****************************************************************************/
        double minWeight = double.MinValue; /* The minimum gradient score */

        Eye Eye_Left; /* Patient's left eye */
        Eye Eye_Right;/* Patient's right eye */
        Eye Eye_temp;
        Iris Iris_Left; /* Patient's left iris */
        Iris Iris_Right; /* Patient's right iris */
        

        public Image<Bgr, byte> img_LevatorFaceUp, img_LevatorFaceDown;
        public Image<Bgr, byte> img_Periocular; /* Both eye ROI */

        Image<Bgr, byte> img_L_eye;/* Patient's left eye ROI */
        Image<Bgr, byte> img_R_eye;/* Patient's right eye ROI */
        Image<Bgr, byte> img_Draw; /* For visualization propose */
        Image<Bgr, byte> img_temp;
        Image<Bgr, byte> img_L_CtrlPoints, img_R_CtrlPoints;/* For visualization propose */
        Image<Bgr, byte> img_L_LevatorDown, img_R_LevatorDown;/* Patient's eye look down */
        Image<Bgr, byte> img_L_LevatorUp, img_R_LevatorUp; /* Patient's eye look up */

        PointF R_CornerInner, R_CornerOuter;/*  Patient's right eye corner*/
        PointF L_CornerInner, L_CornerOuter;/*  Patient's left eye corner */
        Rectangle L_IrisROI, R_IrisROI;/* Patient's Iris ROI */

        /* CtrlPoints[0] : upper eyelid
         * CtrlPoints[1] : lower eyelid  */
        List<List<PointF>> CtrlPoints = new List<List<PointF>>();

        /* Measurement */
        /* The first item is for R_eye
         * The second item is for L_eye*/
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


        private void toolStripMenuItem12_Click(object sender, EventArgs e)//眼睛 Ctrl + V
        {
            
            #region imgLoading & UI setting
            if (My_Image2 == null || img_LevatorFaceDown == null || img_LevatorFaceUp == null )
            {
                toolStripMenuItem20_Click(sender, e);
                // If user click 'Cancel' ,return
                if (My_Image2 == null || img_LevatorFaceDown == null || img_LevatorFaceUp == null)
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

            // Set periocular ROI by relative position of CascadeClassifier outcome
            if (facecutori != null)
            {
                /*
                facecutori.ROI = facesori;
                img_Periocular = facecutori.Clone();
                int X = facecutori.ROI.Width * 15 / 100 + +facecutori.ROI.X;
                int Y = facecutori.ROI.Height * 25 / 100 + facecutori.ROI.Y;
                int Width = facecutori.ROI.Width * 70 / 100;
                int Height = facecutori.ROI.Height * 20 / 100;
                img_Periocular.Save("Img_P_test_1.jpg");
                img_Periocular.ROI = new Rectangle(X , Y , Width, Height);
                Console.WriteLine("img_Per.X={0}, img_Per.Y={1}, img_Per.Width={2}, img_Per.Height={3}"
                            , X, Y,Width, Height);
                offset.X += X;
                offset.Y += Y;
                img_LevatorFaceDown.ROI = new Rectangle(X, Y, Width, Height);
                img_LevatorFaceUp.ROI = new Rectangle(X, Y, Width, Height);*/

                //12.01 update
                /*
                facesori = new Rectangle(250, 190, 1300, 230);
                facecutori.ROI = facesori;
                img_Periocular = facecutori.Clone();

                img_LevatorFaceDown.ROI = new Rectangle(250, 240, 1300, 180);
                img_LevatorFaceUp.ROI = new Rectangle(250, 240, 1300, 180);

                facecutori.Save("Facecutori_test.jpg");
                img_Periocular.Save("Img_P_test_2.jpg");
                img_LevatorFaceDown.Save("LevatorFaceDown.jpg");
                img_LevatorFaceUp.Save("LevatorFaceUp.jpg");*/

                //12.10 update
                AICornerDetection aICornerDetection = new AICornerDetection(filename);
                aICornerDetection.findEyeROI(out facesori);
                facecutori.ROI = facesori;
                img_Periocular = facecutori.Clone();

                aICornerDetection.setfilename(facedownfilename);
                aICornerDetection.findEyeROI(out facesori);
                img_LevatorFaceDown.ROI = facesori;

                aICornerDetection.setfilename(faceupfilename);
                aICornerDetection.findEyeROI(out facesori);
                img_LevatorFaceUp.ROI = facesori;

            }
            else
            {
                MessageBox.Show("臉部偵測失敗，請重新拍攝");
                return;
            }


            // timer procedure control
            isEyesInit = true;
            timer_current = 0;
            timer_stage = 0;
            Eye_Left = null;
            Eye_Right = null;


            #region EyeROI

            Eye_temp = new Eye();
            Eye_temp.RunFRST(img_Periocular);
            
            // Find eye ROI base on FRST
            int BlockWidth = 350;
            int BlockHeight = 150;
            Point EyeRoi_R = Eye_temp.FindEyeROIbyFRST(BlockWidth, BlockHeight);
            Point EyeRoi_L;
            if (EyeRoi_R.X >= (img_Periocular.Width / 2)){
                //RightEye
                EyeRoi_L = Eye_temp.FindEyeROIbyFRST(BlockWidth, BlockHeight, "R");
            }
            else{
                //LeftEye
                EyeRoi_L = Eye_temp.FindEyeROIbyFRST(BlockWidth, BlockHeight, "L");
            }

            
            if (EyeRoi_R.X > EyeRoi_L.X){
                Point temp = EyeRoi_R;
                EyeRoi_R = EyeRoi_L;
                EyeRoi_L = temp;
            }
            EyeRoi_R.X -= 150;
            EyeRoi_L.X += 150;
            // Set L_eye, R_eye, R_LevatorDown,L_LevatorDown,R_LevatorUp,L_LevatorUp ROI
            img_R_eye = img_Periocular.Copy();
            img_R_eye.ROI = new Rectangle(EyeRoi_R, new Size(BlockWidth, BlockHeight));

            Console.WriteLine("img_R ={0},img_R_W = {1},img_R_H ={2}"
                            , EyeRoi_R, BlockWidth, BlockHeight);
            img_R_eye.Save("img_R_eye_test.jpg");

            img_R_CtrlPoints = img_R_eye.Copy();
            img_L_eye = img_Periocular.Copy();
            img_L_eye.ROI = new Rectangle(EyeRoi_L, new Size(BlockWidth, BlockHeight));

            Console.WriteLine("img_L ={0},img_L_W = {1},img_L_H ={2}"
                            , EyeRoi_L, BlockWidth, BlockHeight);
            img_L_eye.Save("img_L_eye_test.jpg");

            img_L_CtrlPoints = img_L_eye.Copy();

            img_R_LevatorDown = img_LevatorFaceDown.Copy();
            img_L_LevatorDown = img_LevatorFaceDown.Copy();
            img_R_LevatorUp = img_LevatorFaceUp.Copy();
            img_L_LevatorUp = img_LevatorFaceUp.Copy();

            img_R_LevatorDown.ROI = img_R_eye.ROI;
            img_R_LevatorUp.ROI = img_R_eye.ROI;
            img_L_LevatorDown.ROI = img_L_eye.ROI;
            img_L_LevatorUp.ROI = img_L_eye.ROI;

            img_R_LevatorDown.Save("R_LevatorDown.jpg");
            img_R_LevatorUp.Save("R_LevatorUp.jpg");
            img_L_LevatorDown.Save("L_LevatorDown.jpg");
            img_L_LevatorUp.Save("L_LevatorUp.jpg");

            #endregion

            #region LevatorFunction

            int Down, Up;
            CornerDetection levator = new CornerDetection(img_R_LevatorDown);
            Down = levator.VPF_eyelidsDetect("R_LevatorDown");
            levator = new CornerDetection(img_R_LevatorUp);
            Up = levator.VPF_eyelidsDetect("R_LevatorUp");
            Levetor.Add((Down - Up)* mmperpixel);

            levator = new CornerDetection(img_L_LevatorDown);
            Down = levator.VPF_eyelidsDetect("L_LevatorDown");
            levator = new CornerDetection(img_L_LevatorUp);
            Up = levator.VPF_eyelidsDetect("L_LevatorUp");
            Levetor.Add((Down - Up)* mmperpixel);

            #endregion

            #region Corner


            

            Image<Bgr, byte> R_Iris, L_Iris;
            
            CornerDetection CornerDetector = new CornerDetection(img_R_eye);
            CornerDetector.VPF("R", out R_IrisROI, out R_Iris);
            
            // Get the Corner (PointF)
            R_CornerInner = CornerDetector.WVPF("R_eye_CornerL");
            R_CornerOuter = CornerDetector.WVPF("R_eye_CornerR");
            R_CornerInner.X += R_IrisROI.Right;
            R_CornerInner.Y += R_IrisROI.Top;
            
            

            // Draw R_eye Corner
            img_R_eye.Draw(new Cross2DF(R_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);
            img_R_eye.Draw(new Cross2DF(R_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);


            //---------------------------------------------------------------------------------------

            CornerDetector = new CornerDetection(img_L_eye);
            CornerDetector.VPF("L", out L_IrisROI, out L_Iris);
            // Get the Corner (PointF)
            L_CornerOuter = CornerDetector.WVPF("L_eye_CornerL");
            L_CornerInner = CornerDetector.WVPF("L_eye_CornerR");

            //L_CornerInner.Y += L_IrisROI.Y;
            
            // Draw L_eye Corner
            img_L_eye.Draw(new Cross2DF(L_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);
            img_L_eye.Draw(new Cross2DF(L_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);
            /*AICornerDetection aICornerDetector = new AICornerDetection(filename);
            aICornerDetector.findCorner(out R_CornerOuter, out R_CornerInner, out L_CornerOuter, out L_CornerInner);
            Console.WriteLine("R_cornerinner:{0}  EyeROI_R:{1}", R_CornerInner, EyeRoi_R);
            R_CornerOuter.X -= (EyeRoi_R.X + offset.X);
            R_CornerOuter.Y -= (EyeRoi_R.Y + offset.Y);
            R_CornerInner.X -= (EyeRoi_R.X + offset.X);
            R_CornerInner.Y -= (EyeRoi_R.Y + offset.Y);
            L_CornerOuter.X -= (EyeRoi_L.X + offset.X);
            L_CornerOuter.Y -= (EyeRoi_L.Y + offset.Y);
            L_CornerInner.X -= (EyeRoi_L.X + offset.X);
            L_CornerInner.Y -= (EyeRoi_L.Y + offset.Y);
            Console.WriteLine("R_cornerinner:{0}  R_cornerouter:{1}", R_CornerInner, R_CornerOuter);
            Console.WriteLine("L_cornerinner:{0}  L_cornerouter:{1}", L_CornerInner, L_CornerOuter);*/
            // Draw R_eye Corner
            img_R_eye.Draw(new Cross2DF(R_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);
            img_R_eye.Draw(new Cross2DF(R_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);
            // Draw L_eye Corner
            img_L_eye.Draw(new Cross2DF(L_CornerOuter, 5, 5), new Bgr(0, 0, 255), 1);
            img_L_eye.Draw(new Cross2DF(L_CornerInner, 5, 5), new Bgr(0, 0, 255), 1);

            #endregion

            #region IrisMark

            // HoughCircle outcome
            List<CircleF> list_LeftIris = new List<CircleF>();
            List<CircleF> list_RightIris = new List<CircleF>();

            // For debuging 
            List<Image<Bgr, byte>> list_draw = new List<Image<Bgr, byte>>();

            // Parameter for HoughCircle
            int minRadius = 60; // hospital : 29 12
            int maxRadius = 78; // hospital : 38 16

            // Right Iris Detect
            Iris_Right = new Iris();
            list_RightIris = Iris_Right.HoughCircles(R_Iris, minRadius, maxRadius,0.1);
            if (list_RightIris.Count != 0)
            {
                Iris_Right.SavePreprocess(list_RightIris, "R");
            }
            else
            {
                timer2.Enabled = false;
                return;
            }

            // Left Iris Detect
            Iris_Left = new Iris();
            list_LeftIris = Iris_Left.HoughCircles(L_Iris, minRadius, maxRadius,0.1);
            if (list_LeftIris.Count != 0)
            {
                Iris_Left.SavePreprocess(list_LeftIris, "L");
            }
            else
            {
                timer2.Enabled = false;
                return;
            }

            // Visualization
            Image<Bgr, byte> draw = img_Periocular.Clone();
            Iris_Left.DrawEyeRoi(list_LeftIris, list_RightIris, ref draw, img_L_eye, img_R_eye, R_IrisROI, L_IrisROI);
            
            #endregion

            img_Periocular.Save("img_Periocular.jpg");
            img_Periocular.Draw(new Rectangle(EyeRoi_R, new Size(BlockWidth, BlockHeight)), new Bgr(0, 0, 255), 1);
            img_Periocular.Draw(new Rectangle(EyeRoi_L, new Size(BlockWidth, BlockHeight)), new Bgr(0, 0, 255), 1);
            imageBox1.Image = img_Periocular;

            timer2.Enabled = true;
            timer2.Start();

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // If true, go on to the next stage
            if (timer_current == timer_end)
            {
                isEyesInit = true;
                timer_current = 0;
                timer_stage++;
                Console.WriteLine("next loop" + timer_stage.ToString());

                #region The last step
                if (timer_stage == 2)
                {
                    Eye_Left.DrawEyelid(ref img_L_eye);
                    Eye_Right.DrawEyelid(ref img_R_eye);
                    
                    // Draw Iris
                    float iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
                    float iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;
                    img_L_eye.Draw(new CircleF(new PointF(iris_x * 16, iris_y * 16), (Iris_Left.get_Iris().Radius + 1) * 16), new Bgr(0, 0, 255), 1, LineType.AntiAlias, 4);

                    iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
                    iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;
                    img_R_eye.Draw(new CircleF(new PointF(iris_x * 16, iris_y * 16), (Iris_Right.get_Iris().Radius + 1) * 16), new Bgr(0, 0, 255), 1, LineType.AntiAlias, 4);
                    Console.WriteLine("end");

                    img_L_eye.Save("L_eyeROI.jpg");
                    img_R_eye.Save("R_eyeROI.jpg");

                    panel1.Visible = true;
                    label6.Visible = true;
                    label7.Visible = true;
                    imageBox2.Visible = false;
                    imageBox5.Visible = true;
                    imageBox6.Visible = true;

                    imageBox1.Image = My_Image2;
                    imageBox5.Image = img_R_eye;
                    imageBox6.Image = img_L_eye;

                    // Calculate measurement
                    measurementCalculate(L_IrisROI, Iris_Left.get_Iris(), Eye_Left);
                    measurementCalculate(R_IrisROI, Iris_Right.get_Iris(), Eye_Right);

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


            if (isEyesInit && timer_stage == 0)
            {
               
                img_temp = img_R_CtrlPoints.Clone();
                isEyesInit = false;

                CtrlPoints = ContourSampling(getContour(img_R_CtrlPoints),10,"R");

                // Draw Control Points
                foreach (var p in CtrlPoints[0])
                {
                    img_R_CtrlPoints.Draw(new CircleF(p, 1), new Bgr(0, 0, 255), 0);
                }
                foreach (var p in CtrlPoints[1])
                {
                    img_R_CtrlPoints.Draw(new CircleF(p, 1), new Bgr(255, 0, 0), 0);
                }
                img_R_CtrlPoints.Save("R_Ctrl.jpg");
            }
            if (isEyesInit && timer_stage == 1)
            {
                Eye_Right = new Eye(Eye_Left);
                Eye_Right.DrawEyelid(ref img_R_eye);
                

                img_temp = img_L_CtrlPoints.Clone();
                isEyesInit = false;
                
                CtrlPoints =  ContourSampling(getContour(img_L_CtrlPoints),10,"L");

                // Draw Control Points
                foreach (var p in CtrlPoints[0])
                {
                    img_L_CtrlPoints.Draw(new CircleF(p, 1), new Bgr(0, 0, 255), 0);
                }
                foreach (var p in CtrlPoints[1])
                {
                    img_L_CtrlPoints.Draw(new CircleF(p, 1), new Bgr(255, 0, 0), 0);
                }
                img_L_CtrlPoints.Save("L_Ctrl.jpg");

                Eye_Left = null;
            }

            img_Draw = img_temp.Clone();

            double weight = 0;
            if (timer_stage == 0)// R_eye
            {
                Eye_temp = new Eye(CtrlPoints, R_CornerInner, R_CornerOuter, "R");
                weight = Eye_temp.Gradient(img_R_eye.Convert<Gray, byte>());
            }
            else if (timer_stage == 1)// L_eye
            {
                Eye_temp = new Eye( CtrlPoints, L_CornerOuter, L_CornerInner, "L");
                weight = Eye_temp.Gradient(img_L_eye.Convert<Gray, byte>());
            }

            Eye_temp.DrawEyelid(ref img_Draw);
            
            if (Eye_Left != null)
            {
                Eye_Left.DrawEyelid(ref img_Draw);
            }
            this.imageBox2.Image = img_Draw;



            if (weight < minWeight || Eye_Left == null)
            {
                minWeight = weight;
                Eye_Left = Eye_temp;
            }

            timer_current++;
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
                    filename = Openfile.FileName;
                    Console.WriteLine("{0}    {1}", filename, Openfile.FileName);
                    MessageBox.Show("請選取往下看照片");
                    #region 眼睛
                    /*
                    Console.WriteLine("{0}", System.Environment.CurrentDirectory);
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

                        Console.WriteLine("faces[0].X={0}, faces[0].Y={1}, faces[0].Width={2}, faces[0].Height={3}"
                            , faces[0].X, faces[0].Y, faces[0].Width, faces[0].Height);
                        //offset.X += face[0].X;
                        //offset.Y += face[0].Y;
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
                    */
                    #endregion
                    facecutori = new Image<Bgr, byte>(filename);
                    img_Periocular = new Image<Bgr, byte>(filename);

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
                    img_LevatorFaceDown = new Image<Bgr, byte>(Openfile.FileName);
                    facedownfilename = Openfile.FileName;
                    MessageBox.Show("請選取往上看照片");
                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }
            else
            {
                img_LevatorFaceDown = null;
            }

            //第三次選取 往上看圖
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Openfile.Title = "請選取往上看圖";
                    img_LevatorFaceUp = new Image<Bgr, byte>(Openfile.FileName);
                    faceupfilename = Openfile.FileName;
                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
            }
            else
            {
                img_LevatorFaceUp = null;
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
            imageBox5.Image = img_R_eye;
            /* L_eye */ 
            imageBox6.Image = img_L_eye;


        }
        // MRD1 radioButton
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            Point Iris = new Point((int)Iris_x, (int)Iris_y);
            Point upperEyelid = new Point((int)Iris_x, (int)Eye_Right.above.FY(Iris_x));
            imageBox5.Image = measurementVisualize(MeasurementImage,upperEyelid,Iris, MRD1[0]);
            
            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;

            Iris = new Point((int)Iris_x, (int)Iris_y);
            upperEyelid = new Point((int)Iris_x, (int)Eye_Left.above.FY(Iris_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, Iris, MRD1[1]);
        }
        // MRD2 radioButton
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            Point Iris = new Point((int)Iris_x, (int)Iris_y);
            Point lowerEyelid = new Point((int)Iris_x, (int)Eye_Right.below.FY(Iris_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, Iris, lowerEyelid, MRD2[0]);

            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;
            
            Iris = new Point((int)Iris_x, (int)Iris_y);
            lowerEyelid = new Point((int)Iris_x, (int)Eye_Left.below.FY(Iris_x));
            
            imageBox6.Image = measurementVisualize(MeasurementImage, Iris, lowerEyelid, MRD2[1]);
        }
        // PFH radioButton
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            Point upperEyelid = new Point((int)Iris_x, (int)Eye_Right.above.FY(Iris_x));
            Point lowerEyelid = new Point((int)Iris_x, (int)Eye_Right.below.FY(Iris_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, upperEyelid, lowerEyelid, PFH[0]);

            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;

            upperEyelid = new Point((int)Iris_x, (int)Eye_Left.above.FY(Iris_x));
            lowerEyelid = new Point((int)Iris_x, (int)Eye_Left.below.FY(Iris_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, lowerEyelid, PFH[1]);
        }
        // PFW radioButton
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();

            Point right = Point.Round(Eye_Right.get_Right_endPoint());
            Point right_end = new Point(right.X, right.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(right, right_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(right_end,2), LineColor, LineThickness);

            Point left = new Point((int)Eye_Right.get_Left_endPoint().X, right.Y);
            Point left_end = new Point(left.X, left.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(left, left_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(left_end, 2), LineColor, LineThickness);

            String Text = PFW[0].ToString("#0.#0") + "mm";
            CvInvoke.PutText(MeasurementImage, Text, left_end,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);
            imageBox5.Image = MeasurementImage;

            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();

            left = Point.Round(Eye_Left.get_Left_endPoint());
            left_end = new Point(left.X, left.Y + LineLength);
            MeasurementImage.Draw(new LineSegment2D(left, left_end), LineColor, LineThickness);
            MeasurementImage.Draw(new CircleF(left_end, 2), LineColor, LineThickness);

            right = new Point((int)Eye_Left.get_Right_endPoint().X, left.Y);
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
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            Point Iris = new Point((int)Iris_x, (int)Iris_y);
            Point upperEyelid = new Point((int)Iris_x, (int)Eye_Right.above.FY(Iris_x));
            imageBox5.Image = measurementVisualize(MeasurementImage, upperEyelid, upperEyelid, Levetor[0]);

            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;

            Iris = new Point((int)Iris_x, (int)Iris_y);
            upperEyelid = new Point((int)Iris_x, (int)Eye_Left.above.FY(Iris_x));

            imageBox6.Image = measurementVisualize(MeasurementImage, upperEyelid, upperEyelid, Levetor[1]);

            imageBox7.Image = img_R_LevatorUp.Clone();
            imageBox8.Image = img_L_LevatorUp.Clone();
            imageBox9.Image = img_R_LevatorDown.Clone();
            imageBox10.Image = img_L_LevatorDown.Clone();
        }

        // PS radioButton
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            Point Iris = new Point((int)Iris_x, (int)(Iris_y - Iris_Right.get_Iris().Radius));
            Point upperEyelid = new Point((int)Iris_x, (int)Eye_Right.above.FY(Iris_x));

            MeasurementImage = measurementVisualize(MeasurementImage, Iris, upperEyelid, PtosisSeverity[0]);
            MeasurementImage.Draw(new CircleF(new PointF(Iris_x*16, Iris_y*16), (Iris_Right.get_Iris().Radius + 1)*16), new Bgr(0, 0, 255), 1,LineType.AntiAlias,4);

            imageBox5.Image = MeasurementImage;

            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;

            Iris = new Point((int)Iris_x, (int)(Iris_y - Iris_Left.get_Iris().Radius));
            upperEyelid = new Point((int)Iris_x, (int)Eye_Left.above.FY(Iris_x));

            MeasurementImage = measurementVisualize(MeasurementImage, Iris, upperEyelid, PtosisSeverity[1]);
            MeasurementImage.Draw(new CircleF(new PointF(Iris_x*16, Iris_y*16), (Iris_Left.get_Iris().Radius + 1)*16), new Bgr(0, 0, 255), 1,LineType.AntiAlias,4);

            imageBox6.Image = MeasurementImage;
        }
        // OSA radioButton
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            /* R_eye */
            Image<Bgr, byte> MeasurementImage = img_R_CtrlPoints.Clone();
            float Iris_x = R_IrisROI.X + Iris_Right.get_Iris().Center.X;
            float Iris_y = R_IrisROI.Y + Iris_Right.get_Iris().Center.Y;

            // Calculate OSA : Iris Within Eyelids Area
            for (float i = Iris_x - Iris_Right.get_Iris().Radius; i <= Iris_x + Iris_Right.get_Iris().Radius; i += 0.1f)
            {
                for (float j = Iris_y - Iris_Right.get_Iris().Radius; j <= Iris_y + Iris_Right.get_Iris().Radius; j += 0.1f)
                {
                    if ((Math.Pow(Iris_x - i, 2) + Math.Pow(Iris_y - j, 2) < Math.Pow(Iris_Right.get_Iris().Radius, 2)) &&
                         Iris_y - j < Iris_y - Eye_Right.above.FY(i))
                    {
                        MeasurementImage.Draw(new CircleF(new PointF(i, j), 0), LineColor, 0);
                    }
                }
            }
            Point TextPoint = new Point((int)(Iris_x - Iris_Right.get_Iris().Radius*2), (int)(Iris_y - Iris_Right.get_Iris().Radius));
            String Text = OSA[0].ToString("#0.#0") + "mm^2";
            CvInvoke.PutText(MeasurementImage, Text, TextPoint,
                fontface, fontscale, textColor, 1, LineType.AntiAlias);
            imageBox5.Image = MeasurementImage;


            /* L_eye */
            MeasurementImage = img_L_CtrlPoints.Clone();
            Iris_x = L_IrisROI.X + Iris_Left.get_Iris().Center.X;
            Iris_y = L_IrisROI.Y + Iris_Left.get_Iris().Center.Y;

            // Calculate OSA : Iris Within Eyelids Area
            for (float i = Iris_x - Iris_Left.get_Iris().Radius; i <= Iris_x + Iris_Left.get_Iris().Radius; i += 0.1f)
            {
                for (float j = Iris_y - Iris_Left.get_Iris().Radius; j <= Iris_y + Iris_Left.get_Iris().Radius; j += 0.1f)
                {
                    if ((Math.Pow(Iris_x - i, 2) + Math.Pow(Iris_y - j, 2) < Math.Pow(Iris_Left.get_Iris().Radius, 2)) &&
                         Iris_y - j < Iris_y - Eye_Left.above.FY(i))
                    {
                        MeasurementImage.Draw(new CircleF(new PointF(i, j), 0), LineColor, 0);
                    }
                }
            }
            TextPoint = new Point((int)(Iris_x - Iris_Left.get_Iris().Radius*2), (int)(Iris_y - Iris_Left.get_Iris().Radius));
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

        private void measurementCalculate(Rectangle IrisROI, CircleF Iris, Eye eye)
        {
            // Calculate MRD1, MRD2, PFH ,PFW ,PtosisSeverity
            float Iris_x = IrisROI.X + Iris.Center.X;
            float Iris_y = IrisROI.Y + Iris.Center.Y;
            double mrd1 = (Iris_y - eye.above.FY(Iris_x)) * mmperpixel;
            double mrd2 = (eye.below.FY(Iris_x) - Iris_y) * mmperpixel;
            if (mrd1 < 0) mrd1 = 0;
            if (mrd2 < 0) mrd2 = 0;
            double ps = Iris.Radius * mmperpixel - mrd1;
            if (ps < 0) ps = 0;
            MRD1.Add(mrd1);
            MRD2.Add(mrd2);
            PFH.Add(MRD1[MRD1.Count-1] + MRD2[MRD2.Count-1]);
            PFW.Add((eye.get_Right_endPoint().X - eye.get_Left_endPoint().X) * mmperpixel);
            PtosisSeverity.Add(ps);

            double Iris_Area = 0.0;
            // Calculate OSA : Iris Within Eyelids Area
            for (float i = Iris_x - Iris.Radius; i <= Iris_x + Iris.Radius; i += 0.1f)
            {
                for (float j = Iris_y - Iris.Radius; j <= Iris_y + Iris.Radius; j += 0.1f)
                {
                    if ((Math.Pow(Iris_x - i, 2) + Math.Pow(Iris_y - j, 2) < Math.Pow(Iris.Radius, 2)) &&
                         Iris_y - j < Iris_y - eye.above.FY(i))
                    {
                        Iris_Area += 0.1;
                    }
                }
            }
            OSA.Add(Iris_Area * mmperpixel);

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
            Image <Gray, byte> colorSubtract = img[2] - img[1]; // R - G
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
                CvInvoke.DrawContours(img, Contours, Inx, new MCvScalar(255, 255, 255), 1, LineType.EightConnected, null);
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

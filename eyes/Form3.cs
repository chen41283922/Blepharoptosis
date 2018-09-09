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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bearing;

namespace eyes
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            float sel_w = Screen.PrimaryScreen.Bounds.Width;
            float sel_h = Screen.PrimaryScreen.Bounds.Height;
            foreach (Control c in this.Controls)
            {
                //c.Scale(sel_w / 1920, sel_h / 1080);
                c.Scale(new SizeF(sel_w / 1280, sel_h / 720));
                Single currentSize = c.Font.Size * (sel_h / 720);
                c.Font = new Font("微軟正黑體", currentSize);
            }
           
        }

        VideoCapture webCam;
        VideoCapture webCamRight;
        Image<Gray, Byte> My_Image1;
        Image<Bgr, Byte> My_Image2;
        Image<Bgr, Byte> LevatorFaceDown;
        Image<Bgr, Byte> LevatorFaceUp;
        int turn = 0;

        private void Form3_Load(object sender, EventArgs e)
        {
            

            webCam = new VideoCapture(0);
            webCamRight = new VideoCapture(1);
            ////////////////////////////////////////////////////////相機設定
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 900);//900*675是4:3最大，解析度再上去就會強制16:9
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 675);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 160);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, 20);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.XiAutoWb, 0);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, 100);

            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 900);//900*675是4:3最大，解析度再上去就會強制16:9
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 675);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 160);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, 20);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.XiAutoWb, 0);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
            webCamRight.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, 100);
            Application.Idle += Application_Idle;
        }
        Image<Bgr, Byte> camImage;
        Image<Bgr, Byte> RightcamImage;
        int count = 0;
        int countdelay = 0;
        void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                camImage = webCam.QueryFrame().ToImage<Bgr, Byte>();
                RightcamImage = webCamRight.QueryFrame().ToImage<Bgr, Byte>();
                Rectangle box = new Rectangle(175,50,450,525);//偵測人臉框框
                float[] dashValues = { 6, 6 };//設定需線pattern
                float[] dashValues2 = { 1, 6, 5 };//設定需線pattern
                float[] dashValues3 = { 2, 6, 4 };//設定需線pattern
                float[] dashValues4 = { 3, 6 ,3};//設定需線pattern
                float[] dashValues5 = { 4, 6, 2 };//設定需線pattern
                float[] dashValues6 = { 5, 6, 1 };//設定需線pattern
                Pen redpen = new Pen(Color.Red, 2);
                Pen greenpen = new Pen(Color.Green, 2);
                
                Bitmap drawellipse = new Bitmap(camImage.Bitmap);//畫橢圓用bitmap
                Bitmap drawrightellipse = new Bitmap(RightcamImage.Bitmap);//畫橢圓用bitmap

                Image<Bgr, Byte> img = new Image<Bgr, Byte>(drawellipse);
                
                Rectangle CorrectionPoint = new Rectangle(400, 300, 3, 3);//校正用點
                img.ROI = box;
                CascadeClassifier frontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
                Rectangle[] faces = frontalface.DetectMultiScale(img, 1.1, 5, new Size(200, 200), Size.Empty);
                img.ROI = Rectangle.Empty;

                
                countdelay++;
                if (countdelay % 3 == 0)
                {
                    count++;
                }

                #region 虛線
                if (count % 6 == 0)
                {
                    redpen.DashPattern = dashValues;//設定虛線
                    greenpen.DashPattern = dashValues;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                else if(count % 6 == 1)
                {
                    redpen.DashPattern = dashValues2;//設定虛線
                    greenpen.DashPattern = dashValues2;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                else if (count % 6 == 2)
                {
                    redpen.DashPattern = dashValues3;//設定虛線
                    greenpen.DashPattern = dashValues3;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                else if (count % 6 == 3)
                {
                    redpen.DashPattern = dashValues4;//設定虛線
                    greenpen.DashPattern = dashValues4;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                else if (count % 6 == 4)
                {
                    redpen.DashPattern = dashValues5;//設定虛線
                    greenpen.DashPattern = dashValues5;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                else if (count % 6 == 5)
                {
                    redpen.DashPattern = dashValues6;//設定虛線
                    greenpen.DashPattern = dashValues6;//設定虛線
                    if (faces.Length != 0)
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(greenpen, box);
                        g.Dispose();
                    }
                    else
                    {
                        Graphics g = Graphics.FromImage(drawellipse);
                        g.DrawEllipse(redpen, box);
                        g.Dispose();
                    }
                }
                #endregion

                img = new Image<Bgr, Byte>(drawellipse);
                img.Draw(CorrectionPoint, new Bgr(Color.Green), 2);
                Image<Bgr, Byte> result = img.Copy();
                imageBox1.Image = result;


                
                Image<Bgr, Byte> Rightimg = new Image<Bgr, Byte>(drawrightellipse);
                Rightimg.ROI = box;
                CascadeClassifier Rightfrontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
                Rectangle[] Rightfaces = Rightfrontalface.DetectMultiScale(Rightimg, 1.1, 5, new Size(200, 200), Size.Empty);
                Rightimg.ROI = Rectangle.Empty;
                if (Rightfaces.Length != 0)
                {
                    Graphics g = Graphics.FromImage(drawrightellipse);
                    g.DrawEllipse(greenpen, box);
                    g.Dispose();
                }
                else
                {
                    Graphics g = Graphics.FromImage(drawrightellipse);
                    g.DrawEllipse(redpen, box);
                    g.Dispose();
                }
                Rightimg = new Image<Bgr, Byte>(drawrightellipse);
                Rightimg.Draw(CorrectionPoint, new Bgr(Color.Green), 2);
                Image<Bgr, Byte> Rightresult = Rightimg.Copy();
                imageBox2.Image = RightcamImage;//Rightresult;
            }
            catch { }
        }

        SectionDetection sectionDetection = new SectionDetection();
        private void scale(Image<Bgr, Byte> src, ref Image<Bgr, Byte> dest)
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 Form1 = (Form1)this.Tag;
            if (turn == 0)
            {
                string strPicFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".bmp";
                imageBox1.Image = camImage;
                imageBox1.Image.Save(Application.StartupPath + @"\image\" + strPicFile);//照日期存檔

                Form1.camera = imageBox1.Image.Bitmap;
                My_Image1 = new Image<Gray, byte>(Application.StartupPath + @"\image\" + strPicFile);
                My_Image2 = new Image<Bgr, byte>(Application.StartupPath + @"\image\" + strPicFile);
                Form1.My_Image1 = My_Image1;
                Form1.My_Image2 = My_Image2;

                #region 眼睛
                CascadeClassifier frontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
                Rectangle[] faces = frontalface.DetectMultiScale(My_Image1, 1.1, 5, new Size(200, 200), Size.Empty);

                List<Rectangle> face = new List<Rectangle>();
                face.AddRange(faces);

                //眼睛
                if (faces.Length != 0)
                {
                    Form1.facecutori = new Image<Bgr, Byte>(My_Image2.Bitmap);
                    Form1.facecutori.ROI = faces[0];
                    Form1.facecutorigray = new Image<Gray, Byte>(My_Image1.Bitmap);
                    Form1.facecutorigray.ROI = faces[0];
                    Form1.facesori = new Rectangle(faces[0].X, faces[0].Y, faces[0].Width, faces[0].Height);//擷取出的臉部範圍
                    int zoomface = 60;
                    for (int i = 0; i < faces.Length; i++)//調整臉範圍大小
                    {
                        faces[i].X = faces[i].X - zoomface;
                        faces[i].Y = faces[i].Y - zoomface * 2;
                        faces[i].Width = faces[i].Width + zoomface * 2;
                        faces[i].Height = faces[i].Height + zoomface * 4;
                    }
                    Form1.facecut = new Image<Bgr, Byte>(My_Image2.Bitmap);
                    Form1.facecut.ROI = faces[0];
                    Form1.imageBox1.Image = My_Image2;
                    Form1.faces = faces;
                    Form1.toolStripMenuItem1.Enabled = true;
                }
                #endregion

                MessageBox.Show("拍攝完成！接著進行提眼肌測試，請用力往下看");
            }
            else if (turn == 1)// Take the picture of LevatorFaceDown
            {
                string strPicFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".bmp";
                imageBox1.Image = camImage;
                imageBox1.Image.Save(Application.StartupPath + @"\image\" + strPicFile);//照日期存檔

                Form1.LevatorFaceDown = new Image<Bgr, byte>(Application.StartupPath + @"\image\" + strPicFile);
                MessageBox.Show("拍攝完成！接著請用力往上看");
            }
            else if (turn == 2)// Take the picture of LevatorFaceUp
            {
                string strPicFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".bmp";
                imageBox1.Image = camImage;
                imageBox1.Image.Save(Application.StartupPath + @"\image\" + strPicFile);//照日期存檔

                Form1.LevatorFaceUp = new Image<Bgr, byte>(Application.StartupPath + @"\image\" + strPicFile);
            }


            turn++;


            //Application.Idle -= Application_Idle;
            if (turn == 3)
            {
                this.Close();
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            webCam.Dispose();//視窗關掉釋放相機資源
            webCamRight.Dispose();
        }

        
    }
}

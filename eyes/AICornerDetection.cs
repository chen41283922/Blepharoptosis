using System;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace eyes
{
    class AICornerDetection
    {
        //Image<Bgr, byte>inputImage;
        public string imgPath ;
        public AICornerDetection() { }
        //public AICornerDetection(Image<Bgr, byte> face) { inputImage = face; }
        public AICornerDetection(string str) { imgPath = str; }

        public void setfilename(string str) {  imgPath = str; }

        public void findCorner(out PointF ro, out PointF ri, out PointF lo, out PointF li)
        {
            string python = @"C:\Users\jason\Anaconda3\python.exe";
            //string imgPath = "test.bmp";
            // python app to call 
            string myPythonApp = "face_test.py";
            //Console.WriteLine("{0}", System.Environment.CurrentDirectory);
            // dummy parameters to send Python script 
            ro = new PointF(0, 0);
            ri = new PointF(0, 0);
            lo = new PointF(0, 0);
            li = new PointF(0, 0);
            // Create new process start info 
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);

            // make sure we can read the output from stdout 
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;

            // start python app with 3 arguments  
            // 1st arguments is pointer to itself,  
            // 2nd and 3rd are actual arguments we want to send 
            myProcessStartInfo.Arguments = myPythonApp + " " + imgPath;

            Process myProcess = new Process();
            // assign start information to the process 
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo = myProcessStartInfo;

            //Console.WriteLine("Calling Python script with arguments {0} and {1}", x, y);
            // start the process 
            myProcess.Start();

            // Read the standard output of the app we called.  
            // in order to avoid deadlock we will read output first 
            // and then wait for process terminate: 
            StreamReader myStreamReader = myProcess.StandardOutput;
            //string[] myString0 = new string[10];
            string myString1 = myStreamReader.ReadLine();
            string myString2 = myStreamReader.ReadLine();
            string myString3 = myStreamReader.ReadLine();
            string myString4 = myStreamReader.ReadLine();
            string myString5 = myStreamReader.ReadLine();
            string myString6 = myStreamReader.ReadLine();
            string myString7 = myStreamReader.ReadLine();
            string myString8 = myStreamReader.ReadLine();
            //for (int i = 0; i < 8; i++)
            //    myString0[i] = myStreamReader.ReadLine();
            /*if you need to read multiple lines, you might use: 
                string myString = myStreamReader.ReadToEnd() */

            ro.X = float.Parse(myString1);
            ro.Y = float.Parse(myString2);
            ri.X = float.Parse(myString3);
            ri.Y = float.Parse(myString4);
            lo.X = float.Parse(myString5);
            lo.Y = float.Parse(myString6);
            li.X = float.Parse(myString7);
            li.Y = float.Parse(myString8);

            // wait exit signal from the app we called and then close it. 
            myProcess.WaitForExit();
            myProcess.Close();            
        }

        public void findEyeROI(out Rectangle output)
        {
            string python = @"C:\Users\jason\Anaconda3\python.exe";
            string myPythonApp = "eyeRoi.py";
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + imgPath;
            Process myProcess = new Process();
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();

            StreamReader myStreamReader = myProcess.StandardOutput;
            int x, y, width, height;
            x = int.Parse(myStreamReader.ReadLine());
            y = int.Parse(myStreamReader.ReadLine());
            width = int.Parse(myStreamReader.ReadLine()) - x;
            height = int.Parse(myStreamReader.ReadLine()) - y;
            Console.WriteLine("x:{0},y:{1},w:{2},h:{3}", x, y, width, height);
            Rectangle temprect = new Rectangle(x, y, width, height);
            output = temprect;
        }

    }
}
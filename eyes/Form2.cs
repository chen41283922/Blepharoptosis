using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eyes
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            timer1.Interval = 2000;
            timer1.Start();
            timer_Initial.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Close();
        }
        int times = 0;
        private void timer_Initial_Tick(object sender, EventArgs e)
        {
            if (times == 0)
                Thread.Sleep(100);

            times++;
            if (times <= 60)
            {
                label_Initial.Location = new Point(label_Initial.Location.X, label_Initial.Location.Y - 6);
                label_Loading.Location = new Point(label_Loading.Location.X, label_Initial.Location.Y + 100);
            }
        }
    }
}

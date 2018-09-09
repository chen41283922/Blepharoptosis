using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eyes
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://image.cse.nsysu.edu.tw/index1.html");
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

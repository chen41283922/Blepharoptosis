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
    public partial class Form7 : Form
    {
        Form1 form1;
        public Form7()
        {
            form1 = new Form1();
            InitializeComponent();
        }

        private void button_Apply_Click(object sender, EventArgs e)
        {
            form1.name = textBox_Name.Text;
            form1.AgeSex = textBox_AgeSex.Text;
            form1.NoChart = textBox_NoChart.Text;
            form1.Address = textBox_Address.Text;
            form1.Phone = textBox_Phone.Text;
            form1.Date = textBox_Date.Text;

            form1.Visible = true;
            this.Visible = false;

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            form1.Visible = true;
            this.Visible = false;
        }
    }
}

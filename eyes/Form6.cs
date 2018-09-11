using PdfiumViewer;
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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }
        public void showpdf(String filepath)
        {
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                pdfViewer1.Document = PdfDocument.Load(@filepath);

                //var stream = new FileStream(filepath, FileMode.Open);
                //// Create PDF Document
                //var pdfDocument = PdfDocument.Load(stream);
                //// Load PDF Document into WinForms Control
                //pdfRenderer1.Load(pdfDocument);

                //webBrowser1.Navigate(@filepath);
            }
        }

    }
}

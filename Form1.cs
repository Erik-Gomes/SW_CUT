using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SW_CUT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("CRIAR PROJETO - IMPORTAR ARQUIVO DXF");
        }

        private void button2_Click(object sender, EventArgs e)
        {
          //MessageBox.Show("ABRIR PROJETO - SELECIONE ARQUIVO");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = new LinkLabel();
            linkLabel.Text = "Clique aqui para acessar o Google";
            linkLabel.LinkArea = new LinkArea(10, 6); // "Google" como link
        }
    }
}

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
            var leitor = new DxfBasicReader();
            var formas = leitor.LerArquivo("meuarquivo.dxf");

            foreach (var forma in formas)
            {
                Console.WriteLine($"Forma: {forma.Tipo}");
                foreach (var ponto in forma.Pontos)
                {
                    Console.WriteLine($"  Ponto: ({ponto.X}, {ponto.Y})");
                }
                if (forma.Raio > 0)
                {
                    Console.WriteLine($"  Raio: {forma.Raio}");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
          //MessageBox.Show("ABRIR PROJETO - SELECIONE ARQUIVO");
        }
        private void linkLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Site oficial: sw_cut.com.br\nSoftware 100% offline para uso em CPU", 
                    "Informações", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    
    }
}

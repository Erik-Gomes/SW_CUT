using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace SW_CUT
{
    public class FuncionalPrincipal : Form
    {
        private List<Forma> formas;
        private Panel canvas;
        private Button btnExportar;
        private Button btnAjustarChapa;
        private NumericUpDown numLarguraChapa;
        private NumericUpDown numAlturaChapa;
        private NumericUpDown numDistanciamento;
        private NumericUpDown numQtdChapas;

        private float larguraChapa = 3000;
        private float alturaChapa = 1200;
        private float distanciamento = 5;
        private int qtdChapas = 1;

        private string pastaExportacao;

        public FuncionalPrincipal(List<Forma> formasAprovadas)
        {
            this.formas = formasAprovadas;
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Funcional Principal - Nesting e Exportação";
            this.Size = new Size(1000, 700);

            canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            btnExportar = new Button
            {
                Text = "Exportar",
                Image = SystemIcons.Information.ToBitmap(),
                TextAlign = ContentAlignment.MiddleRight,
                ImageAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnExportar.Click += BtnExportar_Click;

            btnAjustarChapa = new Button
            {
                Text = "Aplicar Configurações",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnAjustarChapa.Click += BtnAjustarChapa_Click;

            numLarguraChapa = new NumericUpDown { Minimum = 500, Maximum = 6000, Value = 3000, Dock = DockStyle.Top };
            numAlturaChapa = new NumericUpDown { Minimum = 500, Maximum = 3000, Value = 1200, Dock = DockStyle.Top };
            numDistanciamento = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 5, Dock = DockStyle.Top };
            numQtdChapas = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 1, Dock = DockStyle.Top };

            var configPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                Padding = new Padding(10)
            };

            configPanel.Controls.Add(new Label { Text = "Qtd. Chapas", Dock = DockStyle.Top });
            configPanel.Controls.Add(numQtdChapas);
            configPanel.Controls.Add(new Label { Text = "Distanciamento entre peças (mm)", Dock = DockStyle.Top });
            configPanel.Controls.Add(numDistanciamento);
            configPanel.Controls.Add(new Label { Text = "Altura Chapa (mm)", Dock = DockStyle.Top });
            configPanel.Controls.Add(numAlturaChapa);
            configPanel.Controls.Add(new Label { Text = "Largura Chapa (mm)", Dock = DockStyle.Top });
            configPanel.Controls.Add(numLarguraChapa);
            configPanel.Controls.Add(btnAjustarChapa);
            configPanel.Controls.Add(btnExportar);

            this.Controls.Add(canvas);
            this.Controls.Add(configPanel);
        }

        private void BtnAjustarChapa_Click(object sender, EventArgs e)
        {
            larguraChapa = (float)numLarguraChapa.Value;
            alturaChapa = (float)numAlturaChapa.Value;
            distanciamento = (float)numDistanciamento.Value;
            qtdChapas = (int)numQtdChapas.Value;

            MessageBox.Show(
                $"Configurações aplicadas:\nChapa: {larguraChapa}x{alturaChapa}mm\nDistanciamento entre peças: {distanciamento}mm\nQtd: {qtdChapas}",
                "Atualizado");
        }

        private void BtnExportar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(pastaExportacao))
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Selecione a pasta para salvar os arquivos .NRP";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        pastaExportacao = fbd.SelectedPath;
                        File.WriteAllText("config_path.txt", pastaExportacao);
                    }
                    else
                    {
                        MessageBox.Show("Exportação cancelada.");
                        return;
                    }
                }
            }

            // Posicionamento automático das peças (Nesting básico)
            AplicarNesting();

            var exportData = GerarDadosExportacao();
            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

            string arquivo = Path.Combine(pastaExportacao, $"ProjetoLaser_{DateTime.Now:yyyyMMdd_HHmmss}.nrp");

            File.WriteAllText(arquivo, json);
            MessageBox.Show($"Arquivo exportado com sucesso!\n\nLocal: {arquivo}", "Exportação");
        }

        private void AplicarNesting()
        {
            float offsetX = 0;
            float offsetY = 0;
            float linhaMaxAltura = 0;

            foreach (var forma in formas)
            {
                if (forma.Status != FormaStatus.Reprovada)
                {
                    if (offsetX + forma.Largura > larguraChapa)
                    {
                        offsetX = 0;
                        offsetY += linhaMaxAltura + distanciamento;
                        linhaMaxAltura = 0;
                    }

                    // Verifica se a peça cabe na altura restante
                    if (offsetY + forma.Altura > alturaChapa)
                    {
                        // Aqui podemos implementar lógica para usar a próxima chapa, se houver
                        offsetX = 0;
                        offsetY = 0;
                        linhaMaxAltura = 0;
                    }

                    forma.Posicao = new PointF(offsetX, offsetY);
                    offsetX += forma.Largura + distanciamento;
                    if (forma.Altura > linhaMaxAltura) linhaMaxAltura = forma.Altura;
                }
            }
        }

        private object GerarDadosExportacao()
        {
            List<object> pecasExport = new List<object>();
            foreach (var forma in formas)
            {
                if (forma.Status == FormaStatus.Aprovada || forma.Status == FormaStatus.ComDobra)
                {
                    pecasExport.Add(new
                    {
                        forma.Nome,
                        forma.Quantidade,
                        forma.Largura,
                        forma.Altura,
                        PosicaoX = forma.Posicao.X,
                        PosicaoY = forma.Posicao.Y,
                        Status = forma.Status.ToString()
                    });
                }
            }

            return new
            {
                Projeto = "Projeto de Corte a Laser",
                Data = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                Chapa = new
                {
                    Largura = larguraChapa,
                    Altura = alturaChapa,
                    DistanciamentoEntrePecas = distanciamento,
                    Quantidade = qtdChapas
                },
                Pecas = pecasExport
            };
        }
    }

    public enum FormaStatus { Aprovada, ComDobra, Reprovada }

    public class Forma
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public float Largura { get; set; }
        public float Altura { get; set; }
        public PointF Posicao { get; set; }
        public FormaStatus Status { get; set; }
    }
}

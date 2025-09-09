using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SW_CUT
{
    public partial class ProjectForm : Form
    {
        private FlowLayoutPanel flowPreviews;
        private Button btnImportarDXF;

        public ProjectForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // FlowLayoutPanel para pré-visualizações
            this.flowPreviews = new FlowLayoutPanel();
            this.flowPreviews.Location = new Point(20, 60);
            this.flowPreviews.Size = new Size(760, 400);
            this.flowPreviews.AutoScroll = true;
            this.flowPreviews.WrapContents = true;
            this.flowPreviews.FlowDirection = FlowDirection.LeftToRight;
            this.Controls.Add(this.flowPreviews);

            // Botão de importar DXF
            this.btnImportarDXF = new Button();
            this.btnImportarDXF.Text = "Importar DXF";
            this.btnImportarDXF.Location = new Point(20, 20);
            this.btnImportarDXF.Size = new Size(120, 30);
            this.btnImportarDXF.Click += btnImportarDXF_Click;
            this.Controls.Add(this.btnImportarDXF);

            this.ClientSize = new Size(800, 500);
            this.Text = "SW_CUT - Projeto";
        }

        private void btnImportarDXF_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Arquivos DXF (*.dxf)|*.dxf";
            openFile.Title = "Selecione arquivos DXF";
            openFile.Multiselect = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                flowPreviews.Controls.Clear(); // limpa prévias anteriores

                foreach (var arquivo in openFile.FileNames)
                {
                    var leitor = new DxfReader();
                    var formas = leitor.LerArquivo(arquivo);

                    // Container para PictureBox + Label
                    Panel container = new Panel();
                    container.Size = new Size(200, 180);
                    container.Margin = new Padding(5);

                    // PictureBox para pré-visualização
                    PictureBox preview = new PictureBox();
                    preview.Size = new Size(200, 150);
                    preview.BackColor = Color.White;
                    preview.BorderStyle = BorderStyle.FixedSingle;
                    preview.Paint += (s, pe) => DrawPreview(pe.Graphics, formas, preview.Size);

                    // Clique para excluir linhas
                    preview.MouseClick += (s, me) => HandleMouseClickOnPreview(me, preview, formas);

                    // Label com nome do arquivo
                    Label lblNome = new Label();
                    lblNome.Text = System.IO.Path.GetFileName(arquivo);
                    lblNome.Dock = DockStyle.Bottom;
                    lblNome.TextAlign = ContentAlignment.MiddleCenter;

                    container.Controls.Add(preview);
                    container.Controls.Add(lblNome);

                    flowPreviews.Controls.Add(container);
                    preview.Refresh();
                }
            }
        }

        private void DrawPreview(Graphics g, List<Forma> formas, Size size)
        {
            if (formas == null || formas.Count == 0) return;

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var f in formas)
            {
                foreach (var p in f.Pontos)
                {
                    if (p.X < minX) minX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }

            float dx = maxX - minX;
            float dy = maxY - minY;
            float scale = Math.Min(size.Width / dx * 0.9f, size.Height / dy * 0.9f);

            float offsetX = (size.Width - dx * scale) / 2 - minX * scale;
            float offsetY = (size.Height - dy * scale) / 2 - minY * scale;

            foreach (var f in formas)
            {
                if (f.Tipo == "Linha")
                {
                    var p1 = f.Pontos[0];
                    var p2 = f.Pontos[1];

                    Pen pen = Pens.Black;
                    switch (f.LinhaTipo)
                    {
                        case LinhaTipo.Contorno:
                            pen = Pens.Green;
                            break;
                        case LinhaTipo.Dobra:
                            pen = Pens.Yellow;
                            break;
                        case LinhaTipo.Solta:
                            pen = Pens.Red;
                            break;
                    }

                    g.DrawLine(pen,
                               p1.X * scale + offsetX, p1.Y * scale + offsetY,
                               p2.X * scale + offsetX, p2.Y * scale + offsetY);
                }
                else if (f.Tipo == "Circulo" && f.Raio > 0)
                {
                    var centro = f.Pontos[0];
                    float raio = f.Raio * scale;
                    g.DrawEllipse(Pens.Green,
                                  centro.X * scale + offsetX - raio,
                                  centro.Y * scale + offsetY - raio,
                                  raio * 2, raio * 2);
                }
            }
        }

        private void HandleMouseClickOnPreview(MouseEventArgs e, PictureBox preview, List<Forma> formas)
        {
            var clickedPoint = new Ponto { X = e.X, Y = e.Y };
            float threshold = 5f;

            Forma linhaSelecionada = null;
            foreach (var f in formas)
            {
                if (f.Tipo != "Linha") continue;
                var p1 = f.Pontos[0];
                var p2 = f.Pontos[1];

                if (DistanceToLine(clickedPoint, p1, p2) <= threshold)
                {
                    linhaSelecionada = f;
                    break;
                }
            }

            if (linhaSelecionada != null)
            {
                var result = MessageBox.Show("Deseja excluir esta linha?", "Excluir Linha", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    formas.Remove(linhaSelecionada);
                    preview.Invalidate();
                }
            }
        }

        private float DistanceToLine(Ponto p, Ponto a, Ponto b)
        {
            float A = p.X - a.X;
            float B = p.Y - a.Y;
            float C = b.X - a.X;
            float D = b.Y - a.Y;

            float dot = A * C + B * D;
            float len_sq = C * C + D * D;
            float param = (len_sq != 0) ? dot / len_sq : -1;

            float xx, yy;

            if (param < 0)
            {
                xx = a.X;
                yy = a.Y;
            }
            else if (param > 1)
            {
                xx = b.X;
                yy = b.Y;
            }
            else
            {
                xx = a.X + param * C;
                yy = a.Y + param * D;
            }

            float dx = p.X - xx;
            float dy = p.Y - yy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }

    // Estrutura sugerida para as formas
    public class Forma
    {
        public string Tipo { get; set; } // "Linha" ou "Circulo"
        public List<Ponto> Pontos { get; set; }
        public float Raio { get; set; } // Para círculos
        public LinhaTipo LinhaTipo { get; set; } // Contorno, Dobra, Solta
    }

    public enum LinhaTipo
    {
        Contorno,
        Dobra,
        Solta
    }

    public class Ponto
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}




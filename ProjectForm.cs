using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SW_CUT
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel flowPreviews;
        private Button btnImportarDXF;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.flowPreviews = new FlowLayoutPanel();
            this.flowPreviews.Location = new Point(20, 60);
            this.flowPreviews.Size = new Size(760, 400);
            this.flowPreviews.AutoScroll = true;
            this.flowPreviews.WrapContents = true;
            this.flowPreviews.FlowDirection = FlowDirection.LeftToRight;
            this.Controls.Add(this.flowPreviews);

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
                flowPreviews.Controls.Clear();

                foreach (var arquivo in openFile.FileNames)
                {
                    var leitor = new DxfReader();
                    var formas = leitor.LerArquivo(arquivo);

                    Panel container = new Panel();
                    container.Size = new Size(200, 180);
                    container.Margin = new Padding(5);

                    PictureBox preview = new PictureBox();
                    preview.Size = new Size(200, 150);
                    preview.BackColor = Color.White;
                    preview.BorderStyle = BorderStyle.FixedSingle;
                    preview.Paint += (s, pe) => DrawPreview(pe.Graphics, formas, preview.Size);

                    // Clique abre PreviewForm
                    preview.Click += (s, me) =>
                    {
                        PreviewForm pf = new PreviewForm(formas);
                        pf.ShowDialog();
                    };

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
            }
        }
    }
}

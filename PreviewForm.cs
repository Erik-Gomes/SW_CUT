using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SW_CUT
{
    public class PreviewForm : Form
    {
        private List<Forma> formas;
        private Panel canvas;

        private float zoom = 1.0f;
        private Point pan = new Point(0, 0);
        private Point lastMousePos;

        public PreviewForm(List<Forma> formas)
        {
            this.formas = formas;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.canvas = new Panel();
            this.canvas.Dock = DockStyle.Fill;
            this.canvas.BackColor = Color.White;
            this.canvas.Paint += Canvas_Paint;
            this.canvas.MouseWheel += Canvas_MouseWheel;
            this.canvas.MouseDown += Canvas_MouseDown;
            this.canvas.MouseMove += Canvas_MouseMove;
            this.canvas.MouseClick += Canvas_MouseClick;
            this.Controls.Add(this.canvas);

            this.ClientSize = new Size(800, 600);
            this.Text = "Visualização Ampliada";
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            DrawPreview(e.Graphics, formas, canvas.ClientSize);
        }

        private void DrawPreview(Graphics g, List<Forma> formas, Size size)
        {
            if (formas == null || formas.Count == 0) return;

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Calcula bounding box
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
            float scale = Math.Min(size.Width / dx * 0.9f, size.Height / dy * 0.9f) * zoom;

            float offsetX = (size.Width - dx * scale) / 2 - minX * scale + pan.X;
            float offsetY = (size.Height - dy * scale) / 2 - minY * scale + pan.Y;

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

        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += (e.Delta > 0) ? 0.1f : -0.1f;
            if (zoom < 0.1f) zoom = 0.1f;
            canvas.Invalidate();
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                pan.X += e.X - lastMousePos.X;
                pan.Y += e.Y - lastMousePos.Y;
                lastMousePos = e.Location;
                canvas.Invalidate();
            }
        }

        private void Canvas_MouseClick(object sender, MouseEventArgs e)
        {
            var clickedPoint = new Ponto { X = e.X, Y = e.Y };
            float threshold = 5f / zoom; // ajusta para zoom

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
                    canvas.Invalidate();
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
}

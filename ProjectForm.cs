private FlowLayoutPanel flowPreviews;

private void InitializeComponent()
{
    this.flowPreviews = new FlowLayoutPanel();
    this.flowPreviews.Location = new Point(20, 60);
    this.flowPreviews.Size = new Size(760, 400);
    this.flowPreviews.AutoScroll = true;
    this.Controls.Add(this.flowPreviews);

    // Botão para importar DXF
    btnImportarDXF = new Button();
    btnImportarDXF.Text = "Importar DXF";
    btnImportarDXF.Location = new Point(20, 20);
    btnImportarDXF.Click += btnImportarDXF_Click;
    this.Controls.Add(btnImportarDXF);
}

//var leitor = new DxfReader();
        //    var formas = leitor.LerArquivo("meuarquivo.dxf");

//            foreach (var forma in formas)
  //          {
    //            Console.WriteLine($"Forma: {forma.Tipo}");
      //          foreach (var ponto in forma.Pontos)
        //        {
          //          Console.WriteLine($"  Ponto: ({ponto.X}, {ponto.Y})");
            //    }
              //  if (forma.Raio > 0)
                //{
                  //  Console.WriteLine($"  Raio: {forma.Raio}");
               // }
           // }


private void btnImportarDXF_Click(object sender, EventArgs e)
{
    OpenFileDialog openFile = new OpenFileDialog();
    openFile.Filter = "Arquivos DXF (*.dxf)|*.dxf";
    openFile.Title = "Selecione arquivos DXF";
    openFile.Multiselect = true;

    if (openFile.ShowDialog() == DialogResult.OK)
    {
        flowPreviews.Controls.Clear(); // limpa previews anteriores

        foreach (var arquivo in openFile.FileNames)
        {
            var leitor = new DxfReader();
            var formas = leitor.LerArquivo(arquivo);

            // Criar miniatura
            PictureBox preview = new PictureBox();
            preview.Size = new Size(200, 150);
            preview.BorderStyle = BorderStyle.FixedSingle;
            preview.Paint += (s, pe) =>
            {
                DrawPreview(pe.Graphics, formas, preview.Size);
            };

            flowPreviews.Controls.Add(preview);
        }
    }
}

private void DrawPreview(Graphics g, List<Forma> formas, Size size)
{
    if (formas == null || formas.Count == 0) return;

    g.Clear(Color.White);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

    // Calcula bounding box das formas
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
            g.DrawLine(Pens.Black,
                       p1.X * scale + offsetX, p1.Y * scale + offsetY,
                       p2.X * scale + offsetX, p2.Y * scale + offsetY);
        }
        else if (f.Tipo == "Circulo" && f.Raio > 0)
        {
            var centro = f.Pontos[0];
            float raio = f.Raio * scale;
            g.DrawEllipse(Pens.Red,
                          centro.X * scale + offsetX - raio,
                          centro.Y * scale + offsetY - raio,
                          raio * 2, raio * 2);
        }
    }
}


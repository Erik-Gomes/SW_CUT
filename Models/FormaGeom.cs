using System.Collections.Generic;

namespace SW_CUT
{
    // Modelo de geometria usado para desenho/preview e importação DXF
    public class FormaGeom
    {
        public string Tipo { get; set; } // "Linha", "Circulo", "Polilinha", etc.
        public List<Ponto> Pontos { get; set; } = new List<Ponto>();
        public float Raio { get; set; }
        public LinhaTipo LinhaTipo { get; set; }
    }

    public class Ponto
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public enum LinhaTipo
    {
        Contorno,
        Dobra,
        Solta
    }
}

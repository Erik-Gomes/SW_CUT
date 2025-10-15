using System;
using System.Collections.Generic;
using netDxf;
using netDxf.Entities;

namespace SW_CUT
{
    public class Shape
    {
        public string Tipo { get; set; }
        public List<(double X, double Y)> Pontos { get; set; } = new();
        public double Raio { get; set; } = 0; // usado para círculos/arcos
    }

    public class DxfReader
    {
        public List<Shape> LerArquivo(string caminhoArquivo)
        {
            var shapes = new List<Shape>();

            try
            {
                DxfDocument dxf = DxfDocument.Load(caminhoArquivo);

                foreach (EntityObject entidade in dxf.Entities)
                {
                    switch (entidade.Type)
                    {
                        case EntityType.Line:
                            var linha = (Line)entidade;
                            shapes.Add(new Shape
                            {
                                Tipo = "Linha",
                                Pontos = new List<(double X, double Y)>
                                {
                                    (linha.StartPoint.X, linha.StartPoint.Y),
                                    (linha.EndPoint.X, linha.EndPoint.Y)
                                }
                            });
                            break;

                        case EntityType.LwPolyline:
                            var poli = (LwPolyline)entidade;
                            var pontos = new List<(double X, double Y)>();
                            foreach (var v in poli.Vertexes)
                            {
                                pontos.Add((v.Position.X, v.Position.Y));
                            }
                            shapes.Add(new Shape
                            {
                                Tipo = "Polilinha",
                                Pontos = pontos
                            });
                            break;

                        case EntityType.Circle:
                            var circ = (Circle)entidade;
                            shapes.Add(new Shape
                            {
                                Tipo = "Círculo",
                                Pontos = new List<(double X, double Y)>
                                {
                                    (circ.Center.X, circ.Center.Y)
                                },
                                Raio = circ.Radius
                            });
                            break;

                        case EntityType.Arc:
                            var arco = (Arc)entidade;
                            shapes.Add(new Shape
                            {
                                Tipo = "Arco",
                                Pontos = new List<(double X, double Y)>
                                {
                                    (arco.Center.X, arco.Center.Y)
                                },
                                Raio = arco.Radius
                            });
                            break;

                        default:
                            Console.WriteLine($"[Aviso] Entidade {entidade.Type} não tratada.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar DXF: {ex.Message}");
            }

            return shapes;
        }
    }
}

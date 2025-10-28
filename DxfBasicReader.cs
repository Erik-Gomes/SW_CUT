using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SW_CUT
{
    public class DxfBasicReader
    {
        private bool IsInSection(string name, string[] lines, ref int i)
        {
            while (i < lines.Length - 1)
            {
                if (lines[i].Trim() == "0" && lines[i + 1].Trim() == "SECTION")
                {
                    i += 2;
                    if (i < lines.Length - 1 && lines[i].Trim() == "2" && lines[i + 1].Trim() == name)
                    {
                        i += 2;
                        return true;
                    }
                }
                i++;
            }
            return false;
        }

        public List<FormaGeom> LerArquivo(string caminhoArquivo)
        {
            var formas = new List<FormaGeom>();
            if (!File.Exists(caminhoArquivo)) return formas;

            string[] lines = File.ReadAllLines(caminhoArquivo);
            int i = 0;

            // Procura a seção ENTITIES
            if (!IsInSection("ENTITIES", lines, ref i))
            {
                Console.WriteLine("Seção ENTITIES não encontrada no arquivo DXF");
                return formas;
            }

            // Processa entidades até encontrar ENDSEC
            while (i < lines.Length - 1)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";

                if (code == "0")
                {
                    if (value == "ENDSEC") break;

                    FormaGeom forma = null;
                    switch (value)
                    {
                        case "LINE":
                            forma = ParseLine(lines, ref i);
                            break;
                        case "CIRCLE":
                            forma = ParseCircle(lines, ref i);
                            break;
                        case "ARC":
                            forma = ParseArc(lines, ref i);
                            break;
                        case "LWPOLYLINE":
                        case "POLYLINE":
                            forma = ParsePolyline(lines, ref i);
                            break;
                        default:
                            i += 2;
                            continue;
                    }
                    
                    if (forma != null)
                    {
                        formas.Add(forma);
                    }
                }
                else
                {
                    i++;
                }
            }
            // Remove nulls (caso algum parser não reconheça)
            formas.RemoveAll(f => f == null);
            return formas;
        }

        private FormaGeom ParseLine(string[] lines, ref int i)
        {
            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            int start = i;
            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                if (code == "0" && i != start) break;
                switch (code)
                {
                    case "10": x1 = ToFloat(value); break;
                    case "20": y1 = ToFloat(value); break;
                    case "11": x2 = ToFloat(value); break;
                    case "21": y2 = ToFloat(value); break;
                }
                i++;
            }
            i++;
            return new FormaGeom
            {
                Tipo = "Linha",
                Pontos = new List<Ponto> {
                    new Ponto { X = x1, Y = y1 },
                    new Ponto { X = x2, Y = y2 }
                },
                LinhaTipo = LinhaTipo.Contorno
            };
        }

        private FormaGeom ParseCircle(string[] lines, ref int i)
        {
            float cx = 0, cy = 0, raio = 0;
            int start = i;
            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                if (code == "0" && i != start) break;
                switch (code)
                {
                    case "10": cx = ToFloat(value); break;
                    case "20": cy = ToFloat(value); break;
                    case "40": raio = ToFloat(value); break;
                }
                i++;
            }
            i++;
            return new FormaGeom
            {
                Tipo = "Círculo",
                Pontos = new List<Ponto> {
                    new Ponto { X = cx, Y = cy }
                },
                Raio = raio,
                LinhaTipo = LinhaTipo.Contorno
            };
        }

        private FormaGeom ParseArc(string[] lines, ref int i)
        {
            float cx = 0, cy = 0, raio = 0;
            float anguloInicial = 0, anguloFinal = 360;
            int start = i;
            
            // Avança para os dados do arco
            i += 2;

            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                if (code == "0" && i != start) break;
                switch (code)
                {
                    case "10": cx = ToFloat(value); break;
                    case "20": cy = ToFloat(value); break;
                    case "40": raio = ToFloat(value); break;
                    case "50": anguloInicial = ToFloat(value); break;
                    case "51": anguloFinal = ToFloat(value); break;
                }
                i++;
            }

            // Gera pontos do arco para visualização
            var pontos = new List<Ponto>();
            pontos.Add(new Ponto { X = cx, Y = cy }); // Centro

            // Converte ângulos para radianos
            float startRad = anguloInicial * (float)Math.PI / 180;
            float endRad = anguloFinal * (float)Math.PI / 180;

            // Adiciona ponto inicial e final do arco
            pontos.Add(new Ponto { 
                X = cx + raio * (float)Math.Cos(startRad),
                Y = cy + raio * (float)Math.Sin(startRad)
            });
            pontos.Add(new Ponto {
                X = cx + raio * (float)Math.Cos(endRad),
                Y = cy + raio * (float)Math.Sin(endRad)
            });

            return new FormaGeom
            {
                Tipo = "Arco",
                Pontos = pontos,
                Raio = raio,
                LinhaTipo = LinhaTipo.Contorno,
                Angulos = new[] { anguloInicial, anguloFinal }
            };
        }

        private FormaGeom ParsePolyline(string[] lines, ref int i)
        {
            var pontos = new List<Ponto>();
            float x = 0, y = 0;
            bool is_lwpolyline = lines[i + 1].Trim() == "LWPOLYLINE";
            int start = i;

            // Avança para os dados da polyline
            i += 2;

            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";

                // Se encontrar uma nova entidade, para
                if (code == "0" && i != start) break;

                if (is_lwpolyline)
                {
                    // LWPOLYLINE usa códigos 10,20 para cada vértice
                    switch (code)
                    {
                        case "10":
                            x = ToFloat(value);
                            break;
                        case "20":
                            y = ToFloat(value);
                            pontos.Add(new Ponto { X = x, Y = y });
                            break;
                    }
                }
                else
                {
                    // POLYLINE tradicional usa VERTEX
                    if (code == "0" && value == "VERTEX")
                    {
                        i += 2;
                        while (i < lines.Length)
                        {
                            code = lines[i].Trim();
                            value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                            
                            if (code == "0") break;

                            switch (code)
                            {
                                case "10":
                                    x = ToFloat(value);
                                    break;
                                case "20":
                                    y = ToFloat(value);
                                    pontos.Add(new Ponto { X = x, Y = y });
                                    break;
                            }
                            i += 2;
                        }
                        if (value == "SEQEND") break;
                        continue;
                    }
                }
                i++;
            }

            return pontos.Count > 1 ? new FormaGeom
            {
                Tipo = "Polilinha",
                Pontos = pontos,
                LinhaTipo = LinhaTipo.Contorno
            } : null;
        }

        private float ToFloat(string s)
        {
            float v = 0;
            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            return v;
        }
    }
}

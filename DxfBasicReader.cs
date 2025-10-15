using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SW_CUT
{
    public class DxfBasicReader
    {
        public List<FormaGeom> LerArquivo(string caminhoArquivo)
        {
            var formas = new List<FormaGeom>();
            if (!File.Exists(caminhoArquivo)) return formas;

            string[] lines = File.ReadAllLines(caminhoArquivo);
            int i = 0;
            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";

                if (code == "0")
                {
                    switch (value)
                    {
                        case "LINE":
                            formas.Add(ParseLine(lines, ref i));
                            break;
                        case "CIRCLE":
                            formas.Add(ParseCircle(lines, ref i));
                            break;
                        case "ARC":
                            formas.Add(ParseArc(lines, ref i));
                            break;
                        case "LWPOLYLINE":
                        case "POLYLINE":
                            formas.Add(ParsePolyline(lines, ref i));
                            break;
                        default:
                            i += 2;
                            break;
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
                Tipo = "Arco",
                Pontos = new List<Ponto> {
                    new Ponto { X = cx, Y = cy }
                },
                Raio = raio,
                LinhaTipo = LinhaTipo.Contorno
            };
        }

        private FormaGeom ParsePolyline(string[] lines, ref int i)
        {
            var pontos = new List<Ponto>();
            int start = i;
            while (i < lines.Length)
            {
                string code = lines[i].Trim();
                string value = (i + 1 < lines.Length) ? lines[i + 1].Trim() : "";
                if (code == "0" && i != start) break;
                if (code == "10")
                {
                    float x = ToFloat(value);
                    float y = 0;
                    // Find next code 20 for Y
                    int j = i + 2;
                    while (j < lines.Length)
                    {
                        if (lines[j - 1].Trim() == "20")
                        {
                            y = ToFloat(lines[j].Trim());
                            break;
                        }
                        j += 2;
                    }
                    pontos.Add(new Ponto { X = x, Y = y });
                }
                i++;
            }
            i++;
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

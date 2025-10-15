using System;
using System.Collections.Generic;
namespace SW_CUT
{
    public class DxfReader
    {
        // Fallback implementation: retorna lista vazia. Mantive a classe para que o restante do projeto compile.
        // Se desejar habilitar importação completa via netDxf, podemos reintroduzir o código usando a API do pacote
        // e garantir compatibilidade do pacote com a target framework.
        public List<FormaGeom> LerArquivo(string caminhoArquivo)
        {
            Console.WriteLine($"Aviso: leitor DXF fallback usado para '{caminhoArquivo}'. netDxf não está integrado nesta build.");
            return new List<FormaGeom>();
        }
    }
}

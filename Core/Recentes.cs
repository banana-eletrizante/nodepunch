using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NodePunch.Core
{
    internal sealed class ProjetoRecente
    {
        public string Nome { get; set; }
        public string Caminho { get; set; }
        public DateTime AbertoEm { get; set; }
    }

    internal static class Recentes
    {
        private static string Arquivo =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NodePunch", "recentes.json");

        public static List<ProjetoRecente> Listar()
        {
            try
            {
                if (!File.Exists(Arquivo)) return new List<ProjetoRecente>();
                return (JsonSerializer.Deserialize<List<ProjetoRecente>>(File.ReadAllText(Arquivo))
                       ?? new List<ProjetoRecente>())
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Caminho) && Directory.Exists(x.Caminho))
                    .Take(8).ToList();
            }
            catch
            {
                return new List<ProjetoRecente>();
            }
        }

        public static void Registrar(string nome, string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho)) return;
            var items = Listar()
                .Where(x => !string.Equals(x.Caminho, caminho, StringComparison.OrdinalIgnoreCase))
                .ToList();
            items.Insert(0, new ProjetoRecente
            {
                Nome = nome,
                Caminho = caminho,
                AbertoEm = DateTime.Now
            });
            Directory.CreateDirectory(Path.GetDirectoryName(Arquivo));
            File.WriteAllText(Arquivo, JsonSerializer.Serialize(items.Take(8).ToList()));
        }
    }
}

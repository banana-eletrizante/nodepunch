using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class Shell
    {
        public static void AbrirNpmInstall(string caminhoProjeto)
        {
            if (string.IsNullOrWhiteSpace(caminhoProjeto) || !Directory.Exists(caminhoProjeto))
                return;

            try
            {
                string npm = null;
                foreach (string pasta in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
                    if (File.Exists(Path.Combine(pasta.Trim('"'), "npm.cmd"))) { npm = Path.Combine(pasta.Trim('"'), "npm.cmd"); break; }
                if (npm == null)
                {
                    MessageBox.Show("Node.js/npm não foi encontrado no PATH. O backend foi gerado. Instale o Node.js para executar npm install na pasta do projeto.", "NodePunch");
                    return;
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/d /k \"\"" + npm + "\" install\"",
                    WorkingDirectory = caminhoProjeto,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível abrir o npm install: " + ex.Message, "NodePunch");
            }
        }

        public static void AbrirExplorador(string caminhoProjeto)
        {
            if (string.IsNullOrWhiteSpace(caminhoProjeto) || !Directory.Exists(caminhoProjeto))
                return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "\"" + caminhoProjeto + "\"",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}

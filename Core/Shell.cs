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
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/k npm install",
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

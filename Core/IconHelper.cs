using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class IconHelper
    {
        private static Icon _icone;

        public static void AplicarIcone(Form form)
        {
            try
            {
                if (_icone == null)
                {
                    string caminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "nodepunch.ico");
                    if (File.Exists(caminho))
                        _icone = new Icon(caminho);
                }
                if (_icone != null)
                    form.Icon = _icone;
            }
            catch
            {
                // Se não achar o ícone, segue sem ele — não é crítico
            }
        }
    }
}

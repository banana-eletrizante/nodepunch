using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class IconHelper
    {
        private static Icon _icone;

        internal static Icon ObterIcone()
        {
            if (_icone != null) return _icone;
            using var recurso = typeof(IconHelper).Assembly.GetManifestResourceStream("NodePunch.Resources.nodepunch.ico");
            if (recurso == null) return null;
            using var original = new Icon(recurso, 128, 128);
            _icone = (Icon)original.Clone();
            return _icone;
        }

        public static void AplicarIcone(Form form)
        {
            try
            {
                ObterIcone();
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

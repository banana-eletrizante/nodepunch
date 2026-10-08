using System;
using System.Drawing;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal sealed class PainelMarca : Panel
    {
        internal bool Destaque { get; set; }
        internal bool MostrarSelo { get; set; }
        internal PainelMarca()
        {
            DoubleBuffered = true;
            BackColor = Tema.Fundo;
            ResizeRedraw = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            float escala = DeviceDpi / 96f;
            int P(int valor) => (int)Math.Round(valor * escala);
            var icone = IconHelper.ObterIcone();
            if (icone != null && (MostrarSelo || Destaque))
                e.Graphics.DrawIcon(icone, Destaque ? new Rectangle(P(32), P(29), P(56), P(56)) : new Rectangle(P(24), P(19), P(42), P(42)));
            using var linha = new Pen(Tema.Borda);
            e.Graphics.DrawLine(linha, 0, Height - 1, Width, Height - 1);
        }
    }
}

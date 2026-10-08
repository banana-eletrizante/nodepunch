using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NodePunch.Core
{
    // Vector artwork stays crisp at every DPI and adds no image dependency to the exe.
    internal sealed class PainelMarca : Panel
    {
        internal bool MostrarRede { get; set; }
        internal bool MostrarSelo { get; set; }
        internal PainelMarca()
        {
            DoubleBuffered = true;
            BackColor = Tema.Superficie;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            float scale = DeviceDpi / 96f;
            int P(int value) => (int)Math.Round(value * scale);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var linha = new Pen(Color.FromArgb(32, 40, 50));
            for (int x = P(16); x < Width; x += P(24))
                for (int y = P(16); y < Height; y += P(24)) g.DrawEllipse(linha, x, y, 1, 1);
            using var amarelo = new SolidBrush(Tema.Amarelo);
            g.FillRectangle(amarelo, 0, Height - P(3), Width, P(3));
            if (MostrarSelo)
            {
                g.FillRectangle(amarelo, P(24), P(20), P(44), P(42));
                using var fonteSelo = new Font("Consolas", 14f, FontStyle.Bold);
                TextRenderer.DrawText(g, "NP", fonteSelo, new Rectangle(P(24), P(20), P(44), P(42)), Tema.Fundo, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            if (!MostrarRede || Width < P(740)) return;
            var centro = new Point(Width - P(156), P(114));
            Point[] nos = { new(centro.X - P(88), centro.Y - P(55)), new(centro.X + P(78), centro.Y - P(60)), new(centro.X + P(80), centro.Y + P(62)), new(centro.X - P(75), centro.Y + P(65)) };
            using var conexao = new Pen(Color.FromArgb(76, 93, 95), P(2));
            foreach (var no in nos)
            {
                g.DrawLine(conexao, centro, no);
                g.FillEllipse(amarelo, no.X - P(5), no.Y - P(5), P(10), P(10));
            }
            using var aura = new SolidBrush(Color.FromArgb(30, Tema.Amarelo));
            g.FillEllipse(aura, centro.X - P(55), centro.Y - P(55), P(110), P(110));
            PointF[] hexagono = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double angulo = (i * 60 - 30) * Math.PI / 180;
                hexagono[i] = new PointF(centro.X + (float)Math.Cos(angulo) * P(43), centro.Y + (float)Math.Sin(angulo) * P(43));
            }
            g.FillPolygon(amarelo, hexagono);
            using var fonte = new Font("Consolas", 21f, FontStyle.Bold);
            TextRenderer.DrawText(g, "NP", fonte, new Rectangle(centro.X - P(40), centro.Y - P(24), P(80), P(48)), Tema.Fundo, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal sealed class CartaoAcao : Button
    {
        private readonly string titulo;
        private readonly string descricao;
        private bool hover;
        private readonly string simbolo;
        private readonly Color destaque;

        internal CartaoAcao(string titulo, string descricao, Action acao, string simbolo = "NP")
        {
            this.titulo = titulo;
            this.descricao = descricao;
            this.simbolo = simbolo;
            this.destaque = Tema.Amarelo;
            Text = titulo + "\n" + descricao;
            AccessibleName = titulo;
            AccessibleDescription = descricao;
            Size = new Size(230, 84);
            Margin = new Padding(0, 0, 10, 10);
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            UseVisualStyleBackColor = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Click += (s, e) => acao();
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            float escala = DeviceDpi / 96f;
            int P(int value) => (int)Math.Round(value * escala);
            e.Graphics.Clear(Parent?.BackColor ?? Tema.Fundo);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var contorno = new GraphicsPath();
            int d = P(16);
            contorno.AddArc(0, 0, d, d, 180, 90);
            contorno.AddArc(Width - d - 1, 0, d, d, 270, 90);
            contorno.AddArc(Width - d - 1, Height - d - 1, d, d, 0, 90);
            contorno.AddArc(0, Height - d - 1, d, d, 90, 90);
            contorno.CloseFigure();
            using var fundo = new SolidBrush(hover ? Tema.Campo : Tema.Superficie);
            e.Graphics.FillPath(fundo, contorno);
            using var borda = new Pen(Focused || hover ? destaque : Tema.Borda);
            e.Graphics.DrawPath(borda, contorno);
            DesenharIcone(e.Graphics, P(17), P(17), escala);
            using var fonteTitulo = new Font("Segoe UI Semibold", 10.5f);
            using var fonteDescricao = new Font("Segoe UI", 8.5f);
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, titulo, fonteTitulo, new Rectangle(P(58), P(17), Width - P(70), P(26)), Tema.Texto, flags);
            TextRenderer.DrawText(e.Graphics, descricao, fonteDescricao, new Rectangle(P(14), P(54), Width - P(26), P(21)), Tema.Secundario, flags);
        }
        private void DesenharIcone(Graphics g, int x, int y, float escala)
        {
            var estado = g.Save();
            try
            {
                g.TranslateTransform(x, y);
                g.ScaleTransform(escala, escala);
                using var caneta = new Pen(hover || Focused ? destaque : Tema.Secundario, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                if (simbolo == "NP")
                {
                    var icone = IconHelper.ObterIcone();
                    if (icone != null) g.DrawIcon(icone, new Rectangle(0, 0, 28, 28));
                }
                else if (simbolo == "{ }")
                {
                    g.DrawRectangle(caneta, 1, 2, 9, 9); g.DrawRectangle(caneta, 17, 17, 9, 9);
                    g.DrawLines(caneta, new Point[] { new(10, 6), new(21, 6), new(21, 17) });
                    g.DrawLines(caneta, new Point[] { new(5, 11), new(5, 22), new(17, 22) });
                }
                else if (simbolo == "API")
                {
                    g.DrawLines(caneta, new Point[] { new(1, 8), new(25, 8), new(20, 3) });
                    g.DrawLine(caneta, 25, 8, 20, 13);
                    g.DrawLines(caneta, new Point[] { new(25, 21), new(1, 21), new(6, 16) });
                    g.DrawLine(caneta, 1, 21, 6, 26);
                }
                else if (simbolo == "JWT")
                {
                    g.DrawPolygon(caneta, new Point[] { new(14, 1), new(25, 5), new(24, 17), new(20, 23), new(14, 27), new(8, 23), new(4, 17), new(3, 5) });
                    g.DrawLines(caneta, new Point[] { new(8, 13), new(12, 17), new(20, 9) });
                }
                else if (simbolo == "ENV")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int cy = 4 + i * 10, cx = i == 1 ? 18 : 8;
                        g.DrawLine(caneta, 1, cy, cx - 3, cy); g.DrawLine(caneta, cx + 3, cy, 27, cy);
                        g.DrawEllipse(caneta, cx - 3, cy - 3, 6, 6);
                    }
                }
                else if (simbolo == ">_")
                {
                    g.DrawRectangle(caneta, 1, 3, 26, 22);
                    g.DrawLines(caneta, new Point[] { new(6, 9), new(11, 14), new(6, 19) });
                    g.DrawLine(caneta, 15, 19, 22, 19);
                }
                else
                {
                    g.DrawPolygon(caneta, new Point[] { new(14, 1), new(26, 7), new(26, 21), new(14, 27), new(2, 21), new(2, 7) });
                    g.DrawLines(caneta, new Point[] { new(2, 7), new(14, 13), new(26, 7) });
                    g.DrawLine(caneta, 14, 13, 14, 27); g.DrawLine(caneta, 8, 4, 20, 10);
                }
            }
            finally { g.Restore(estado); }
        }
    }
}

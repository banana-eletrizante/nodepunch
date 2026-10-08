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

        internal CartaoAcao(string titulo, string descricao, Action acao, string simbolo = "NP", Color? destaque = null)
        {
            this.titulo = titulo;
            this.descricao = descricao;
            this.simbolo = simbolo;
            this.destaque = destaque ?? Tema.Amarelo;
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
            using var selo = new SolidBrush(Color.FromArgb(25, destaque));
            e.Graphics.FillRectangle(selo, P(14), P(15), P(34), P(30));
            using var fonteSimbolo = new Font("Consolas", 9f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, simbolo, fonteSimbolo, new Rectangle(P(14), P(15), P(34), P(30)), destaque, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            using var fonteTitulo = new Font("Segoe UI Semibold", 10.5f);
            using var fonteDescricao = new Font("Segoe UI", 8.5f);
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, titulo, fonteTitulo, new Rectangle(P(58), P(17), Width - P(70), P(26)), Tema.Texto, flags);
            TextRenderer.DrawText(e.Graphics, descricao, fonteDescricao, new Rectangle(P(14), P(54), Width - P(26), P(21)), Tema.Secundario, flags);
        }
    }
}

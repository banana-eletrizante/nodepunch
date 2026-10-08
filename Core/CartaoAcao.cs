using System;
using System.Drawing;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal sealed class CartaoAcao : Button
    {
        private readonly string titulo;
        private readonly string descricao;
        private bool hover;

        internal CartaoAcao(string titulo, string descricao, Action acao)
        {
            this.titulo = titulo;
            this.descricao = descricao;
            Text = titulo + "\n" + descricao;
            AccessibleName = titulo;
            AccessibleDescription = descricao;
            Size = new Size(216, 66);
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
            e.Graphics.Clear(hover ? Tema.Campo : Tema.Superficie);
            using var borda = new Pen(Focused ? Tema.Amarelo : Tema.Borda);
            e.Graphics.DrawRectangle(borda, 0, 0, Width - 1, Height - 1);
            using var destaque = new SolidBrush(Tema.Amarelo);
            e.Graphics.FillRectangle(destaque, 0, P(15), P(3), Height - P(30));
            using var fonteTitulo = new Font("Segoe UI Semibold", 10.5f);
            using var fonteDescricao = new Font("Segoe UI", 8.5f);
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, titulo, fonteTitulo, new Rectangle(P(15), P(12), Width - P(25), P(24)), Tema.Texto, flags);
            TextRenderer.DrawText(e.Graphics, descricao, fonteDescricao, new Rectangle(P(15), P(37), Width - P(25), P(21)), Tema.Secundario, flags);
        }
    }
}

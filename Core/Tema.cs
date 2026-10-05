using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class Tema
    {
        public static readonly Color Fundo = Color.FromArgb(15, 15, 17);
        public static readonly Color Superficie = Color.FromArgb(24, 24, 27);
        public static readonly Color SuperficieElevada = Color.FromArgb(32, 32, 36);
        public static readonly Color Amarelo = Color.FromArgb(247, 223, 30);
        public static readonly Color AmareloHover = Color.FromArgb(255, 230, 92);
        public static readonly Color Texto = Color.FromArgb(245, 245, 245);
        public static readonly Color TextoSecundario = Color.FromArgb(166, 166, 176);
        public static readonly Color Borda = Color.FromArgb(58, 58, 65);
        public static readonly Color Sucesso = Color.FromArgb(74, 222, 128);
        public static readonly Color Perigo = Color.FromArgb(248, 113, 113);

        public static void Aplicar(Form formulario)
        {
            formulario.BackColor = Fundo;
            formulario.ForeColor = Texto;
            formulario.Font = new Font("Segoe UI", 9f);
            formulario.AutoScaleMode = AutoScaleMode.Dpi;
            formulario.KeyPreview = true;
            Aplicar(formulario.Controls);
        }

        public static void EstilizarBotaoPrimario(Button botao)
        {
            botao.BackColor = Amarelo;
            botao.ForeColor = Color.FromArgb(20, 20, 20);
            botao.FlatStyle = FlatStyle.Flat;
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = AmareloHover;
            botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 195, 15);
            botao.Font = new Font("Segoe UI Semibold", botao.Font.Size, FontStyle.Bold);
            PrepararBotao(botao);
        }

        public static void EstilizarBotaoSecundario(Button botao)
        {
            botao.BackColor = SuperficieElevada;
            botao.ForeColor = Texto;
            botao.FlatStyle = FlatStyle.Flat;
            botao.FlatAppearance.BorderSize = 1;
            botao.FlatAppearance.BorderColor = Borda;
            botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 43, 48);
            botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(52, 52, 58);
            PrepararBotao(botao);
        }

        public static void Arredondar(Control controle, int raio = 10)
        {
            if (controle.Width <= 0 || controle.Height <= 0) return;

            int diametro = Math.Min(raio * 2, Math.Min(controle.Width, controle.Height));
            Rectangle arco = new Rectangle(0, 0, diametro, diametro);
            using GraphicsPath caminho = new GraphicsPath();
            caminho.AddArc(arco, 180, 90);
            arco.X = controle.Width - diametro - 1;
            caminho.AddArc(arco, 270, 90);
            arco.Y = controle.Height - diametro - 1;
            caminho.AddArc(arco, 0, 90);
            arco.X = 0;
            caminho.AddArc(arco, 90, 90);
            caminho.CloseFigure();

            Region regiaoAnterior = controle.Region;
            controle.Region = new Region(caminho);
            regiaoAnterior?.Dispose();
        }

        private static void Aplicar(Control.ControlCollection controles)
        {
            foreach (Control controle in controles)
            {
                switch (controle)
                {
                    case Button botao:
                        botao.TabStop = true;
                        botao.Cursor = Cursors.Hand;
                        botao.AccessibleName ??= botao.Text.Replace(Environment.NewLine, " ");
                        PrepararBotao(botao);
                        break;
                    case TextBox caixa:
                        caixa.BackColor = SuperficieElevada;
                        caixa.ForeColor = Texto;
                        caixa.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ComboBox combo:
                        combo.BackColor = SuperficieElevada;
                        combo.ForeColor = Texto;
                        combo.FlatStyle = FlatStyle.Flat;
                        break;
                    case ListBox lista:
                        lista.BackColor = SuperficieElevada;
                        lista.ForeColor = Texto;
                        lista.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case TreeView arvore:
                        arvore.BackColor = Superficie;
                        arvore.ForeColor = Texto;
                        arvore.LineColor = Borda;
                        break;
                    case CheckBox check:
                        check.ForeColor = Texto;
                        check.FlatStyle = FlatStyle.Flat;
                        break;
                }

                if (controle.HasChildren)
                    Aplicar(controle.Controls);
            }
        }

        private static void PrepararBotao(Button botao)
        {
            botao.Cursor = Cursors.Hand;
            Arredondar(botao, botao.Width <= 42 ? botao.Width / 2 : 8);
            botao.SizeChanged -= Botao_SizeChanged;
            botao.SizeChanged += Botao_SizeChanged;
        }

        private static void Botao_SizeChanged(object sender, EventArgs e)
        {
            if (sender is Button botao)
                Arredondar(botao, botao.Width <= 42 ? botao.Width / 2 : 8);
        }
    }
}

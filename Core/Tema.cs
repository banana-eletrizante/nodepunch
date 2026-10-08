using System;
using System.Drawing;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class Tema
    {
        internal static readonly Color Fundo = Color.FromArgb(15, 18, 24);
        internal static readonly Color Superficie = Color.FromArgb(23, 28, 36);
        internal static readonly Color Campo = Color.FromArgb(31, 38, 48);
        internal static readonly Color Borda = Color.FromArgb(53, 63, 77);
        internal static readonly Color Texto = Color.FromArgb(235, 239, 246);
        internal static readonly Color Secundario = Color.FromArgb(163, 176, 194);
        internal static readonly Color Amarelo = Color.FromArgb(247, 223, 30);

        internal static void Preparar(Form form)
        {
            form.AutoScaleDimensions = new SizeF(96, 96);
            form.AutoScaleMode = AutoScaleMode.Dpi;
        }

        internal static Button Botao(string texto, EventHandler acao, bool principal = false)
        {
            var botao = new Button { Text = texto, Size = new Size(200, 46), FlatStyle = FlatStyle.Flat,
                BackColor = principal ? Amarelo : Campo, ForeColor = principal ? Fundo : Texto,
                Font = new Font("Segoe UI Semibold", 10f), Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false, AccessibleName = texto };
            botao.FlatAppearance.BorderSize = principal ? 0 : 1;
            botao.FlatAppearance.BorderColor = Borda;
            botao.FlatAppearance.MouseOverBackColor = principal ? Color.FromArgb(255, 235, 95) : Color.FromArgb(43, 53, 67);
            botao.Click += acao; return botao;
        }

        internal static void Aplicar(Control raiz)
        {
            foreach (Control control in raiz.Controls)
            {
                if (control is TextBox texto) { texto.BackColor = Campo; texto.ForeColor = Texto; texto.BorderStyle = BorderStyle.FixedSingle; }
                else if (control is ComboBox combo) { combo.BackColor = Campo; combo.ForeColor = Texto; }
                else if (control is ListBox lista) { lista.BackColor = Campo; lista.ForeColor = Texto; }
                else if (control is Button botao)
                {
                    bool principal = botao.BackColor.R > 200 && botao.BackColor.G > 180 && botao.BackColor.B < 100;
                    botao.BackColor = principal ? Amarelo : Campo; botao.ForeColor = principal ? Fundo : Texto;
                    botao.TabStop = true; botao.FlatAppearance.BorderColor = Borda; botao.FlatAppearance.BorderSize = principal ? 0 : 1;
                    botao.UseVisualStyleBackColor = false;
                    botao.FlatAppearance.MouseOverBackColor = principal ? Color.FromArgb(255, 235, 95) : Color.FromArgb(43, 53, 67);
                    botao.AccessibleName = string.IsNullOrEmpty(botao.AccessibleName) ? botao.Text : botao.AccessibleName;
                }
                else if (control is Panel) control.BackColor = Superficie;
                else if (control is Label label && label.ForeColor != Amarelo)
                    label.ForeColor = label.ForeColor.R < 200 && label.ForeColor.G < 200 ? Secundario : Texto;
                else if (control is CheckBox) control.ForeColor = Texto;
                Aplicar(control);
            }
        }

        internal static void Dialogo(Form form, string titulo, string descricao, Button confirmar)
        {
            form.SuspendLayout();
            foreach (Control control in form.Controls) if (control.Dock == DockStyle.None) control.Top += 76;
            form.ClientSize = new Size(form.ClientSize.Width, form.ClientSize.Height + 76);
            form.BackColor = Fundo; Aplicar(form);
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Fundo };
            header.Controls.Add(new Label { Text = titulo, Location = new Point(24, 15), AutoSize = true, Font = new Font("Segoe UI Semibold", 17f), ForeColor = Texto });
            header.Controls.Add(new Label { Text = descricao, Location = new Point(25, 48), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Secundario });
            form.Controls.Add(header); header.BringToFront();
            form.StartPosition = FormStartPosition.CenterParent; form.MinimizeBox = false; form.ShowInTaskbar = false;
            form.AutoScroll = true;
            form.KeyPreview = true;
            form.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { form.Close(); e.Handled = true; } };
            form.AcceptButton = confirmar;
            var controles = new System.Collections.Generic.List<Control>();
            foreach (Control control in form.Controls) if (control.Dock == DockStyle.None) controles.Add(control);
            controles.Sort((a, b) => a.Top != b.Top ? a.Top.CompareTo(b.Top) : a.Left.CompareTo(b.Left));
            for (int i = 0; i < controles.Count; i++) controles[i].TabIndex = i;
            form.ResumeLayout(true);
        }
    }
}

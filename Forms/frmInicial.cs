using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmInicial : Form
    {
        private Button btnCriar, btnAbrir;
        private FlowLayoutPanel pnlRecentes;
        private Label lblQuantidade;

        public frmInicial()
        {
            Tema.Preparar(this);
            IconHelper.AplicarIcone(this);
            Text = "NodePunch";
            ClientSize = new Size(940, 700);
            MinimumSize = new Size(720, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Tema.Fundo;
            Font = new Font("Segoe UI", 10f);
            var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Tema.Fundo };
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 242));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            var hero = new PainelMarca { Dock = DockStyle.Fill, Margin = Padding.Empty, MostrarRede = true, Size = new Size(940, 242) };
            hero.Controls.Add(new Label { Text = "NODEPUNCH  /  BACKEND STUDIO", AutoSize = true, Location = new Point(32, 26), Font = new Font("Consolas", 10f, FontStyle.Bold), ForeColor = Tema.Amarelo, BackColor = Color.Transparent });
            hero.Controls.Add(new Label { Text = "Seu backend começa aqui.", AutoSize = true, Location = new Point(29, 64), Font = new Font("Segoe UI Semibold", 25f), ForeColor = Tema.Texto, BackColor = Color.Transparent });
            hero.Controls.Add(new Label { Text = "Transforme suas ideias em uma API. Um projeto de cada vez.", Location = new Point(32, 119), Size = new Size(548, 26), AutoEllipsis = true, ForeColor = Tema.Secundario, BackColor = Color.Transparent });
            var versao = new Label { Text = "v" + Application.ProductVersion.Split('+')[0], Location = new Point(834, 26), Size = new Size(74, 24), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Tema.Amarelo, BackColor = Tema.Campo, Font = new Font("Consolas", 9f), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            hero.Controls.Add(versao);
            btnCriar = Tema.Botao("+  Novo projeto", btnCriar_Click, true);
            btnCriar.Location = new Point(32, 167); btnCriar.Size = new Size(220, 48); btnCriar.TabIndex = 0;
            btnAbrir = Tema.Botao("Abrir projeto  ↗", btnAbrir_Click);
            btnAbrir.Location = new Point(266, 167); btnAbrir.Size = new Size(200, 48); btnAbrir.TabIndex = 1;
            hero.Controls.AddRange(new Control[] { btnCriar, btnAbrir });
            raiz.Controls.Add(hero, 0, 0);
            var recentes = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(32, 22, 32, 16), Margin = Padding.Empty };
            recentes.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            recentes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var titulo = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, Size = new Size(876, 52) };
            titulo.Controls.Add(new Label { Text = "Continue de onde parou", AutoSize = true, Font = new Font("Segoe UI Semibold", 16f), ForeColor = Tema.Texto });
            lblQuantidade = new Label { Text = "PROJETOS RECENTES", Dock = DockStyle.Right, Width = 180, TextAlign = ContentAlignment.TopRight, Padding = new Padding(0, 9, 0, 0), Font = new Font("Consolas", 8.5f), ForeColor = Tema.Secundario };
            titulo.Controls.Add(lblQuantidade);
            recentes.Controls.Add(titulo, 0, 0);
            pnlRecentes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Margin = Padding.Empty };
            pnlRecentes.SizeChanged += (s, e) => AjustarRecentes();
            recentes.Controls.Add(pnlRecentes, 0, 1);
            raiz.Controls.Add(recentes, 0, 1);
            var rodape = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Tema.Superficie, Size = new Size(940, 40) };
            rodape.Controls.Add(new Label { Text = "Ctrl+N  novo projeto     Ctrl+O  abrir projeto", AutoSize = true, Location = new Point(32, 12), ForeColor = Tema.Secundario, Font = new Font("Consolas", 8.5f) });
            var sobre = Tema.Botao("André Rösler • NodePunch", (s, e) => MessageBox.Show(this, "NODEPUNCH " + Application.ProductVersion.Split('+')[0] + "\n\nDo primeiro nó à sua próxima API.\nExpress • MySQL • PostgreSQL • Firebase\n\nandre-rosler.com", "Sobre o NodePunch", MessageBoxButtons.OK, MessageBoxIcon.Information));
            sobre.Size = new Size(208, 30); sobre.Location = new Point(700, 5); sobre.Anchor = AnchorStyles.Top | AnchorStyles.Right; sobre.FlatAppearance.BorderSize = 0; sobre.BackColor = Tema.Superficie;
            rodape.Controls.Add(sobre); raiz.Controls.Add(rodape, 0, 2);
            Controls.Add(raiz);
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.Control && e.KeyCode == Keys.N) { btnCriar.PerformClick(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.O) { btnAbrir.PerformClick(); e.SuppressKeyPress = true; } };
            CarregarRecentes();
            Shown += (s, e) => AjustarRecentes();
        }

        internal void AtualizarRecentes() => CarregarRecentes();
        private void AjustarRecentes()
        {
            foreach (Control item in pnlRecentes.Controls) item.Width = Math.Max(120, pnlRecentes.ClientSize.Width - 24);
        }
        private void CarregarRecentes()
        {
            while (pnlRecentes.Controls.Count > 0) pnlRecentes.Controls[0].Dispose();
            var items = Recentes.Listar();
            lblQuantidade.Text = items.Count == 0 ? "PROJETOS RECENTES" : items.Count + " PROJETO" + (items.Count == 1 ? "" : "S");
            if (items.Count == 0)
            {
                var vazio = new Panel { Height = 150, BackColor = Tema.Superficie, Padding = new Padding(24) };
                vazio.Controls.Add(new Label { Text = "{ }", AutoSize = true, Location = new Point(24, 22), Font = new Font("Consolas", 21f, FontStyle.Bold), ForeColor = Tema.Amarelo });
                vazio.Controls.Add(new Label { Text = "O próximo projeto pode ser o seu melhor.", AutoSize = true, Location = new Point(24, 72), Font = new Font("Segoe UI Semibold", 12f), ForeColor = Tema.Texto });
                vazio.Controls.Add(new Label { Text = "Crie um backend novo ou abra uma pasta para começar.", AutoSize = true, Location = new Point(24, 104), ForeColor = Tema.Secundario, Font = new Font("Segoe UI", 9f) });
                pnlRecentes.Controls.Add(vazio);
            }
            foreach (var item in items)
            {
                string caminho = item.Caminho;
                var card = new CartaoAcao(item.Nome, caminho, () => AbrirCaminho(caminho, item.Nome), "NP");
                card.AccessibleDescription = caminho;
                pnlRecentes.Controls.Add(card);
            }
            AjustarRecentes();
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            using var form = new frmNovoProjeto { Inicial = this };
            form.ShowDialog(this);
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            using var pasta = new FolderBrowserDialog { Description = "Escolha a pasta do projeto NodePunch", UseDescriptionForTitle = true, ShowNewFolderButton = false };
            if (pasta.ShowDialog(this) != DialogResult.OK) return;
            AbrirCaminho(pasta.SelectedPath, null);
        }

        internal void AbrirCaminho(string caminho, string nome)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !Directory.Exists(caminho))
            {
                MessageBox.Show("Essa pasta não existe mais.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                CarregarRecentes();
                return;
            }
            if (!File.Exists(Path.Combine(caminho, "package.json")))
            {
                MessageBox.Show("Essa pasta não parece um projeto NodePunch (sem package.json).", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            string nomeFinal = string.IsNullOrWhiteSpace(nome) ? new DirectoryInfo(caminho).Name : nome;
            Recentes.Registrar(nomeFinal, caminho);
            if (!Directory.Exists(Path.Combine(caminho, "node_modules")) && MessageBox.Show(
                "As dependências do backend ainda não estão instaladas. Abrir npm install agora?",
                "Dependências do projeto", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Shell.AbrirNpmInstall(caminho);
            new Form1 { NomeProjeto = nomeFinal, CaminhoProjeto = caminho, TelaInicial = this }.Show();
            this.Visible = false;
        }
    }
}

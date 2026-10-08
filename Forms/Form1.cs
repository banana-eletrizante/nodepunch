using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class Form1 : Form
    {
        private TreeView arvore;
        private Label lblProjeto, lblCaminho, lblArquivo;
        private Label lblDetalhes;
        private RichTextBox previa;
        private ToolStripStatusLabel status;
        public string NomeProjeto { get; set; }
        public string CaminhoProjeto { get; set; }
        public bool UsaBanco { get; set; }
        public frmInicial TelaInicial { get; set; }

        public Form1()
        {
            Tema.Preparar(this); IconHelper.AplicarIcone(this);
            Text = "NodePunch • Projeto"; ClientSize = new Size(1180, 760);
            MinimumSize = new Size(940, 650); StartPosition = FormStartPosition.CenterScreen;
            BackColor = Tema.Fundo; ForeColor = Tema.Texto; Font = new Font("Segoe UI", 10f); KeyPreview = true;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Tema.Fundo };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            var header = new PainelMarca { Dock = DockStyle.Fill, Margin = Padding.Empty, MostrarSelo = true, Size = new Size(1180, 100) };
            lblProjeto = new Label { Location = new Point(84, 17), Width = 640, Height = 33, AutoEllipsis = true, Font = new Font("Segoe UI Semibold", 20f), ForeColor = Tema.Texto, BackColor = Color.Transparent };
            lblCaminho = new Label { Location = new Point(85, 56), Width = 660, Height = 26, AutoEllipsis = true, ForeColor = Tema.Secundario, BackColor = Color.Transparent };
            var inicio = Tema.Botao("← Início", (s, e) => Close());
            inicio.Size = new Size(110, 40); inicio.Location = new Point(1038, 27); inicio.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            header.Controls.AddRange(new Control[] { lblProjeto, lblCaminho, inicio }); layout.Controls.Add(header, 0, 0);
            var explorador = new SplitContainer { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Tema.Borda, SplitterWidth = 6, Size = new Size(1180, 600), SplitterDistance = 280, Panel1MinSize = 200, Panel2MinSize = 500 };
            explorador.Panel1.BackColor = Tema.Superficie;
            var tituloArquivos = new Label { Text = "ARQUIVOS DO PROJETO", Dock = DockStyle.Top, Height = 46, Padding = new Padding(18, 17, 0, 0), ForeColor = Tema.Secundario, Font = new Font("Segoe UI Semibold", 9f) };
            arvore = new TreeView { Dock = DockStyle.Fill, BackColor = Tema.Superficie, ForeColor = Tema.Texto, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10f), ItemHeight = 30, Indent = 20, ShowLines = false, FullRowSelect = true, HideSelection = false, ShowNodeToolTips = true };
            arvore.AfterSelect += (s, e) => MostrarArquivo(e.Node.Tag as string); arvore.KeyDown += Arvore_KeyDown;
            explorador.Panel1.Controls.Add(arvore); explorador.Panel1.Controls.Add(tituloArquivos);
            var conteudo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(24), BackColor = Tema.Fundo };
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            conteudo.Controls.Add(new Label { Text = "Ferramentas do projeto", AutoSize = true, Font = new Font("Segoe UI Semibold", 16f), ForeColor = Tema.Texto }, 0, 0);
            var acoes = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, Margin = Padding.Empty };
            AdicionarAcao(acoes, "Model / Controller", "Estruture suas entidades • Ctrl+M", AbrirCriarClasse, "{ }");
            AdicionarAcao(acoes, "Rota Express", "Crie endpoints da API • Ctrl+R", AbrirCriarAPI, "API");
            AdicionarAcao(acoes, "Autenticação JWT", "Cadastro e login • Ctrl+J", AbrirCriarAuth, "JWT");
            AdicionarAcao(acoes, "Configurações", "Ambiente e origens CORS", AbrirConfiguracoes, "ENV");
            AdicionarAcao(acoes, "Visual Studio Code", "Continue no seu editor", AbrirVSCode, ">_");
            AdicionarAcao(acoes, "Dependências", "Instalar pacotes com npm", () => Shell.AbrirNpmInstall(CaminhoProjeto), "npm");
            conteudo.Controls.Add(acoes, 0, 1);
            lblArquivo = new Label { Text = "PRÉVIA DO ARQUIVO", Dock = DockStyle.Fill, ForeColor = Tema.Secundario, AutoEllipsis = true, Padding = new Padding(0, 10, 0, 0), Font = new Font("Segoe UI Semibold", 9f) };
            var barraPrevia = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            lblDetalhes = new Label { Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight, ForeColor = Tema.Secundario, Font = new Font("Consolas", 8.5f), AutoEllipsis = true };
            barraPrevia.Controls.Add(lblArquivo); barraPrevia.Controls.Add(lblDetalhes);
            conteudo.Controls.Add(barraPrevia, 0, 2);
            previa = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Tema.Superficie, ForeColor = Tema.Texto, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 11f), WordWrap = false, DetectUrls = false, Text = "Selecione um arquivo à esquerda para consultar seu conteúdo.", AccessibleName = "Prévia do arquivo, somente leitura" };
            conteudo.Controls.Add(previa, 0, 3); explorador.Panel2.Controls.Add(conteudo); layout.Controls.Add(explorador, 0, 1);
            var rodape = new StatusStrip { Dock = DockStyle.Fill, BackColor = Tema.Superficie, SizingGrip = false };
            status = new ToolStripStatusLabel { Text = "Pronto", ForeColor = Tema.Secundario, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            rodape.Items.Add(status); layout.Controls.Add(rodape, 0, 2); Controls.Add(layout);
            Shown += (s, e) => AtualizarArvore();
            FormClosed += (s, e) => { if (TelaInicial != null && !TelaInicial.IsDisposed) { TelaInicial.Visible = true; TelaInicial.AtualizarRecentes(); } else Application.Exit(); };
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.M) { AbrirCriarClasse(); e.Handled = true; }
                else if (e.Control && e.KeyCode == Keys.R) { AbrirCriarAPI(); e.Handled = true; }
                else if (e.Control && e.KeyCode == Keys.J) { AbrirCriarAuth(); e.Handled = true; }
                else if (e.KeyCode == Keys.F5) { AtualizarArvore(); e.Handled = true; }
            };
        }
        private static void AdicionarAcao(FlowLayoutPanel painel, string titulo, string descricao, Action acao, string simbolo)
        {
            painel.Controls.Add(new CartaoAcao(titulo, descricao, acao, simbolo));
        }
        internal void AtualizarArvore()
        {
            if (string.IsNullOrWhiteSpace(CaminhoProjeto) || !Directory.Exists(CaminhoProjeto)) return;
            lblProjeto.Text = NomeProjeto ?? new DirectoryInfo(CaminhoProjeto).Name;
            lblCaminho.Text = CaminhoProjeto; Text = "NodePunch • " + lblProjeto.Text;
            arvore.BeginUpdate();
            try { Arvore.LoadDirectoryTree(CaminhoProjeto, arvore); Arvore.ExpandirNos(arvore.Nodes); if (arvore.Nodes.Count > 0) arvore.Nodes[0].Expand(); }
            finally { arvore.EndUpdate(); }
            UsaBanco = Funcoes.VerificaUsaBanco(CaminhoProjeto);
            status.Text = "Banco: " + Funcoes.DetectarTipoBanco(CaminhoProjeto) + "   •   Ctrl+M: model   •   Ctrl+R: rota   •   Ctrl+J: JWT   •   F5: atualizar";
            lblArquivo.Text = "PRÉVIA DO ARQUIVO";
            lblDetalhes.Text = "";
            previa.SelectAll(); previa.SelectionColor = Tema.Texto; previa.Select(0, 0);
            previa.Text = "Selecione um arquivo à esquerda para visualizar seu conteúdo.";
        }
        internal void MostrarArquivo(string caminho)
        {
            if (string.IsNullOrEmpty(caminho)) return;
            previa.SelectAll(); previa.SelectionColor = Tema.Texto; previa.Select(0, 0);
            lblDetalhes.Text = "";
            lblArquivo.Text = File.Exists(caminho) ? Path.GetRelativePath(CaminhoProjeto, caminho) : "PRÉVIA DO ARQUIVO";
            try
            {
                if (!File.Exists(caminho)) { previa.Text = "Selecione um arquivo dentro desta pasta."; return; }
                string ext = Path.GetExtension(caminho).ToLowerInvariant();
                if (new FileInfo(caminho).Length > 512 * 1024 || !(ext is ".js" or ".json" or ".md" or ".txt" or ".sql" or ".env" or ".example" or ""))
                { previa.Text = "A prévia está disponível para arquivos de texto de até 512 KB. Abra outros arquivos no seu editor."; return; }
                previa.Text = File.ReadAllText(caminho);
                RealceCodigo.Aplicar(previa, ext);
                int linhas = previa.Text.Length == 0 ? 0 : 1;
                foreach (char c in previa.Text) if (c == '\n') linhas++;
                lblDetalhes.Text = $"{(ext.Length > 0 ? ext[1..].ToUpperInvariant() : "ENV")} • {new FileInfo(caminho).Length / 1024.0:0.#} KB • {linhas} linhas";
            }
            catch (Exception ex) { previa.Text = "Não foi possível ler o arquivo: " + ex.Message; }
        }
        private void Arvore_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || arvore.SelectedNode == null || arvore.SelectedNode.Parent == null) return;
            if (MessageBox.Show(this, $"Excluir \"{arvore.SelectedNode.Text}\"? Esta ação não pode ser desfeita.", "Excluir item", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { Arvore.ExcluirItemSelecionado(arvore); AtualizarArvore(); }
            e.Handled = true;
        }
        private void AbrirCriarClasse() { using var form = new frmCriarClasse { NomeProjeto = NomeProjeto, CaminhoProjeto = CaminhoProjeto, UsaBanco = UsaBanco }; form.ProjetoAtualizado += (s, e) => AtualizarArvore(); form.ShowDialog(this); }
        private void AbrirCriarAPI() { using var form = new frmCriarAPI { NomeProjeto = NomeProjeto, CaminhoProjeto = CaminhoProjeto, UsaBanco = UsaBanco }; form.ProjetoAtualizado += (s, e) => AtualizarArvore(); form.ShowDialog(this); }
        private void AbrirCriarAuth() { using var form = new frmCriarAuth { CaminhoProjeto = CaminhoProjeto }; form.ProjetoAtualizado += (s, e) => AtualizarArvore(); form.ShowDialog(this); }
        private void AbrirConfiguracoes() { using var form = new frmConfiguracoes { CaminhoProjeto = CaminhoProjeto }; form.ShowDialog(this); }
        private void AbrirVSCode()
        {
            string arquivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe");
            if (!File.Exists(arquivo)) arquivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe");
            if (!File.Exists(arquivo)) { MessageBox.Show(this, "Visual Studio Code não foi encontrado.", "Abrir editor"); return; }
            try { var processo = new ProcessStartInfo(arquivo) { UseShellExecute = false }; processo.ArgumentList.Add(CaminhoProjeto); Process.Start(processo); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Abrir editor"); }
        }
    }
}

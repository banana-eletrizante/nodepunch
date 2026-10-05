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
        private MenuStrip menuStrip1;
        private Label lblProjeto;
        private Panel pnlSidebar;
        private Panel pnlAccent;
        private Panel pnlHeader;
        private Panel pnlConteudo;
        private Label lblBanco;
        private Label lblCaminho;
        private ToolTip dicas;

        // Cores — identidade JavaScript
        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorSuperficie = Color.FromArgb(30, 30, 30);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);   // #F7DF1E
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);
        private static readonly Color CorBorda = Color.FromArgb(50, 50, 50);

        public string NomeProjeto { get; set; }
        public string CaminhoProjeto { get; set; }
        public bool UsaBanco { get; set; }

        public Form1()
        {
            InitializeComponent();
            this.Shown += Form1_Shown;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            IconHelper.AplicarIcone(this);
            dicas = new ToolTip
            {
                AutoPopDelay = 6000,
                InitialDelay = 400,
                ReshowDelay = 100,
                ShowAlways = true
            };

            // ===== Menu =====
            menuStrip1 = new MenuStrip
            {
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Renderer = new ToolStripProfessionalRenderer(new MenuCoresJS())
            };

            ToolStripMenuItem miCriarClasse = new ToolStripMenuItem("Model/Controller");
            miCriarClasse.Click += (s, e) => AbrirCriarClasse();
            miCriarClasse.ShortcutKeys = Keys.Control | Keys.M;

            ToolStripMenuItem miCriarAPI = new ToolStripMenuItem("Rota Express (API)");
            miCriarAPI.Click += (s, e) => AbrirCriarAPI();
            miCriarAPI.ShortcutKeys = Keys.Control | Keys.R;

            ToolStripMenuItem miCriarAuth = new ToolStripMenuItem("Autenticação JWT");
            miCriarAuth.Click += (s, e) => AbrirCriarAuth();
            miCriarAuth.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;

            ToolStripMenuItem miConfiguracoes = new ToolStripMenuItem("Configurações");
            miConfiguracoes.Click += (s, e) => AbrirConfiguracoes();
            miConfiguracoes.ShortcutKeys = Keys.Control | Keys.Oemcomma;

            ToolStripMenuItem miAtualizar = new ToolStripMenuItem("Atualizar");
            miAtualizar.Click += (s, e) => AtualizarArvore();
            miAtualizar.ShortcutKeys = Keys.F5;

            ToolStripMenuItem miAbrirVSCode = new ToolStripMenuItem("Abrir no VSCode");
            miAbrirVSCode.Click += (s, e) => AbrirVSCode();
            miAbrirVSCode.Image = ObterIconeVSCode();
            miAbrirVSCode.ImageScaling = ToolStripItemImageScaling.None;

            ToolStripMenuItem miSair = new ToolStripMenuItem("Sair");
            miSair.Click += (s, e) => Application.Exit();

            // Cada item vira uma "aba" própria na barra, lado a lado, em vez de um dropdown único
            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                miCriarClasse,
                miCriarAPI,
                miCriarAuth,
                miConfiguracoes,
                miAtualizar,
                miAbrirVSCode,
                miSair
            });

            foreach (ToolStripItem item in menuStrip1.Items)
            {
                item.ForeColor = Color.White;
                item.Padding = new Padding(10, 4, 10, 4);
            }

            // ===== Header (nome do projeto + barra amarela) =====
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = CorSuperficie,
                Padding = new Padding(12, 0, 12, 0)
            };

            lblProjeto = new Label
            {
                Text = "Projeto:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent
            };

            lblBanco = new Label
            {
                Text = "SEM BANCO",
                ForeColor = CorTextoSec,
                Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, 39),
                BackColor = Color.Transparent
            };

            lblCaminho = new Label
            {
                Text = "",
                ForeColor = CorTextoSec,
                Font = new Font("Segoe UI", 8.5f),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(360, 20),
                Size = new Size(700, 28),
                BackColor = Color.Transparent
            };

            pnlAccent = new Panel
            {
                BackColor = CorAmarelo,
                Height = 3,
                Dock = DockStyle.Bottom
            };

            pnlHeader.Controls.Add(lblProjeto);
            pnlHeader.Controls.Add(lblBanco);
            pnlHeader.Controls.Add(lblCaminho);
            pnlHeader.Controls.Add(pnlAccent);

            // ===== Sidebar com a árvore =====
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 286,
                BackColor = CorSuperficie,
                Padding = new Padding(0)
            };

            Label lblArquivos = new Label
            {
                Text = "ARQUIVOS DO PROJETO",
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(16, 15, 0, 0),
                ForeColor = CorTextoSec,
                Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold)
            };

            Panel pnlAcoesSidebar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = CorSuperficie,
                Padding = new Padding(12, 8, 12, 8)
            };

            Button btnAtualizar = new Button
            {
                Text = "↻  Atualizar",
                Dock = DockStyle.Left,
                Width = 116,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8.5f)
            };
            btnAtualizar.FlatAppearance.BorderColor = CorBorda;
            btnAtualizar.Click += (s, e) => AtualizarArvore();
            dicas.SetToolTip(btnAtualizar, "Atualizar arquivos (F5)");

            Button btnAbrirPasta = new Button
            {
                Text = "Pasta",
                Dock = DockStyle.Right,
                Width = 116,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8.5f)
            };
            btnAbrirPasta.FlatAppearance.BorderColor = CorBorda;
            btnAbrirPasta.Click += (s, e) => AbrirPastaProjeto();
            dicas.SetToolTip(btnAbrirPasta, "Abrir a pasta no Explorador de Arquivos");
            pnlAcoesSidebar.Controls.Add(btnAtualizar);
            pnlAcoesSidebar.Controls.Add(btnAbrirPasta);

            Panel bordaSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 1,
                BackColor = CorBorda
            };

            arvore = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                Indent = 22,
                ItemHeight = 24,
                ShowLines = true,
                LineColor = CorBorda,
                FullRowSelect = true,
                HideSelection = false
            };
            arvore.KeyDown += Arvore_KeyDown;

            arvore.AfterSelect += (s, e) => { };

            pnlSidebar.Controls.Add(arvore);
            pnlSidebar.Controls.Add(pnlAcoesSidebar);
            pnlSidebar.Controls.Add(lblArquivos);
            pnlSidebar.Controls.Add(bordaSidebar);

            // ===== Conteúdo / ações rápidas =====
            pnlConteudo = CriarPainelInicial();

            // ===== Form =====
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(pnlConteudo);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(menuStrip1);

            this.ClientSize = new Size(1100, 650);
            this.BackColor = CorFundo;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "NodePunch";
            this.Font = new Font("Segoe UI", 9f);
            this.FormClosed += (s, e) => Application.Exit();
            Tema.Aplicar(this);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Panel CriarPainelInicial()
        {
            Panel painel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CorFundo,
                Padding = new Padding(48, 42, 48, 32)
            };

            FlowLayoutPanel fluxo = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = CorFundo
            };

            Label etiqueta = new Label
            {
                Text = "WORKSPACE",
                ForeColor = CorAmarelo,
                Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            Label titulo = new Label
            {
                Text = "O que vamos construir?",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 26f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };

            Label subtitulo = new Label
            {
                Text = "Crie a estrutura repetitiva em segundos e concentre-se nas regras da sua API.",
                ForeColor = CorTextoSec,
                Font = new Font("Segoe UI", 11f),
                AutoSize = true,
                MaximumSize = new Size(760, 0),
                Margin = new Padding(0, 0, 0, 28)
            };

            TableLayoutPanel acoes = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Size = new Size(700, 244),
                Margin = new Padding(0, 0, 0, 26),
                BackColor = CorFundo
            };
            acoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            acoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            acoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            acoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            acoes.Controls.Add(CriarCardAcao(
                "MODEL / CONTROLLER\nModele entidades e a camada de controle",
                "Ctrl+M",
                (s, e) => AbrirCriarClasse()), 0, 0);
            acoes.Controls.Add(CriarCardAcao(
                "ROTA EXPRESS\nGere endpoints e validações",
                "Ctrl+R",
                (s, e) => AbrirCriarAPI()), 1, 0);
            acoes.Controls.Add(CriarCardAcao(
                "AUTENTICAÇÃO JWT\nAdicione registro, login e middleware",
                "Ctrl+Shift+A",
                (s, e) => AbrirCriarAuth()), 0, 1);
            acoes.Controls.Add(CriarCardAcao(
                "CONFIGURAÇÕES\nAjuste ambiente e origens CORS",
                "Ctrl+,",
                (s, e) => AbrirConfiguracoes()), 1, 1);

            Panel dica = new Panel
            {
                Size = new Size(700, 78),
                BackColor = CorSuperficie,
                Margin = new Padding(0)
            };
            Tema.Arredondar(dica, 10);
            Label dicaTitulo = new Label
            {
                Text = "DICA",
                ForeColor = CorAmarelo,
                Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            Label dicaTexto = new Label
            {
                Text = "Use F5 para atualizar a árvore. Itens excluídos são enviados para a Lixeira.",
                ForeColor = CorTextoSec,
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true,
                Location = new Point(20, 39)
            };
            dica.Controls.Add(dicaTitulo);
            dica.Controls.Add(dicaTexto);

            fluxo.Controls.Add(etiqueta);
            fluxo.Controls.Add(titulo);
            fluxo.Controls.Add(subtitulo);
            fluxo.Controls.Add(acoes);
            fluxo.Controls.Add(dica);
            painel.Controls.Add(fluxo);
            return painel;
        }

        private Button CriarCardAcao(string texto, string atalho, EventHandler aoClicar)
        {
            Button botao = new Button
            {
                Text = texto + "\n\n" + atalho,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 14),
                Padding = new Padding(18, 12, 12, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Tema.SuperficieElevada,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            botao.FlatAppearance.BorderColor = CorBorda;
            botao.FlatAppearance.BorderSize = 1;
            botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 43, 48);
            botao.Click += aoClicar;
            Tema.Arredondar(botao, 10);
            return botao;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            AtualizarArvore();
        }

        private void AtualizarArvore()
        {
            lblProjeto.Text = "Projeto: " + NomeProjeto;
            lblCaminho.Text = CaminhoProjeto;
            dicas.SetToolTip(lblCaminho, CaminhoProjeto);
            Arvore.LoadDirectoryTree(CaminhoProjeto, arvore);
            UsaBanco = Funcoes.VerificaUsaBanco(CaminhoProjeto);
            TipoBanco tipo = Funcoes.DetectarTipoBanco(CaminhoProjeto);
            lblBanco.Text = tipo == TipoBanco.Nenhum ? "SEM BANCO CONFIGURADO" : "BANCO: " + tipo.ToString().ToUpperInvariant();
            lblBanco.ForeColor = tipo == TipoBanco.Nenhum ? CorTextoSec : Tema.Sucesso;
            arvore.CollapseAll();
            Arvore.ExpandirNos(arvore.Nodes);
            if (arvore.Nodes.Count > 0) arvore.Nodes[0].Expand();
        }

        private void Arvore_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            if (arvore.SelectedNode == null) return;

            if (MessageBox.Show(
                    $"Deseja realmente excluir \"{arvore.SelectedNode.Text}\"?",
                    "Confirmação",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Arvore.ExcluirItemSelecionado(arvore);
            }
        }

        private void AbrirCriarClasse()
        {
            using frmCriarClasse frm = new frmCriarClasse
            {
                NomeProjeto = NomeProjeto,
                CaminhoProjeto = CaminhoProjeto,
                UsaBanco = UsaBanco
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.ShowDialog(this);
        }

        private void AbrirCriarAPI()
        {
            using frmCriarAPI frm = new frmCriarAPI
            {
                NomeProjeto = NomeProjeto,
                CaminhoProjeto = CaminhoProjeto,
                UsaBanco = UsaBanco
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.ShowDialog(this);
        }

        private void AbrirCriarAuth()
        {
            using frmCriarAuth frm = new frmCriarAuth
            {
                CaminhoProjeto = CaminhoProjeto
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.ShowDialog(this);
        }

        private void AbrirConfiguracoes()
        {
            using frmConfiguracoes frm = new frmConfiguracoes
            {
                CaminhoProjeto = CaminhoProjeto
            };
            frm.ShowDialog(this);
        }

        private void AbrirPastaProjeto()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = CaminhoProjeto,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir a pasta: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CaminhoVSCode()
        {
            string caminhoVsc = @"C:\Program Files\Microsoft VS Code\Code.exe";
            if (!File.Exists(caminhoVsc))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                caminhoVsc = Path.Combine(userProfile, @"AppData\Local\Programs\Microsoft VS Code\Code.exe");
            }
            return File.Exists(caminhoVsc) ? caminhoVsc : null;
        }

        private Image ObterIconeVSCode()
        {
            try
            {
                string caminho = CaminhoVSCode();
                if (caminho == null) return null;

                using Icon icone = Icon.ExtractAssociatedIcon(caminho);
                return icone?.ToBitmap();
            }
            catch
            {
                return null;
            }
        }

        private void AbrirVSCode()
        {
            string caminhoVsc = CaminhoVSCode();
            if (caminhoVsc == null)
            {
                MessageBox.Show("Visual Studio Code não encontrado!");
                return;
            }
            try
            {
                Process.Start(caminhoVsc, CaminhoProjeto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir o VSCode: " + ex.Message);
            }
        }
    }

    internal class MenuCoresJS : ProfessionalColorTable
    {
        private static readonly Color Superficie = Color.FromArgb(30, 30, 30);
        private static readonly Color Amarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color Hover = Color.FromArgb(50, 50, 40);
        private static readonly Color Borda = Color.FromArgb(60, 60, 60);

        public override Color MenuBorder => Borda;
        public override Color MenuItemBorder => Amarelo;
        public override Color MenuItemSelected => Hover;
        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd => Hover;
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(60, 60, 45);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(60, 60, 45);
        public override Color MenuStripGradientBegin => Superficie;
        public override Color MenuStripGradientEnd => Superficie;
        public override Color ToolStripDropDownBackground => Superficie;
        public override Color ImageMarginGradientBegin => Superficie;
        public override Color ImageMarginGradientMiddle => Superficie;
        public override Color ImageMarginGradientEnd => Superficie;
        public override Color SeparatorDark => Borda;
        public override Color SeparatorLight => Borda;
        public override Color StatusStripGradientBegin => Superficie;
        public override Color StatusStripGradientEnd => Superficie;
    }
}

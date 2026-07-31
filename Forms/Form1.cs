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

            ToolStripMenuItem miCriarAPI = new ToolStripMenuItem("Rota Express (API)");
            miCriarAPI.Click += (s, e) => AbrirCriarAPI();

            ToolStripMenuItem miCriarAuth = new ToolStripMenuItem("Autenticação JWT");
            miCriarAuth.Click += (s, e) => AbrirCriarAuth();

            ToolStripMenuItem miConfiguracoes = new ToolStripMenuItem("Configurações");
            miConfiguracoes.Click += (s, e) => AbrirConfiguracoes();

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
                Height = 48,
                BackColor = CorSuperficie,
                Padding = new Padding(12, 0, 12, 0)
            };

            lblProjeto = new Label
            {
                Text = "Projeto:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent
            };

            pnlAccent = new Panel
            {
                BackColor = CorAmarelo,
                Height = 3,
                Dock = DockStyle.Bottom
            };

            pnlHeader.Controls.Add(lblProjeto);
            pnlHeader.Controls.Add(pnlAccent);

            // ===== Sidebar com a árvore =====
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = CorSuperficie,
                Padding = new Padding(0)
            };

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
            pnlSidebar.Controls.Add(bordaSidebar);

            // ===== Form =====
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(menuStrip1);

            this.ClientSize = new Size(1100, 650);
            this.BackColor = CorFundo;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.IsMdiContainer = true;
            this.Text = "NodePunch";
            this.Font = new Font("Segoe UI", 9f);
            this.FormClosed += (s, e) => Application.Exit();

            foreach (Control c in this.Controls)
            {
                if (c is MdiClient)
                    c.BackColor = CorFundo;
            }

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            foreach (Control c in this.Controls)
            {
                if (c is MdiClient client)
                {
                    client.BackColor = CorFundo;
                    break;
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            AtualizarArvore();
        }

        private void AtualizarArvore()
        {
            lblProjeto.Text = "Projeto: " + NomeProjeto;
            Arvore.LoadDirectoryTree(CaminhoProjeto, arvore);
            UsaBanco = Funcoes.VerificaUsaBanco(CaminhoProjeto);
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
            frmCriarClasse frm = new frmCriarClasse
            {
                MdiParent = this,
                NomeProjeto = NomeProjeto,
                CaminhoProjeto = CaminhoProjeto,
                UsaBanco = UsaBanco
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.Show();
        }

        private void AbrirCriarAPI()
        {
            frmCriarAPI frm = new frmCriarAPI
            {
                MdiParent = this,
                NomeProjeto = NomeProjeto,
                CaminhoProjeto = CaminhoProjeto,
                UsaBanco = UsaBanco
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.Show();
        }

        private void AbrirCriarAuth()
        {
            frmCriarAuth frm = new frmCriarAuth
            {
                MdiParent = this,
                CaminhoProjeto = CaminhoProjeto
            };
            frm.ProjetoAtualizado += (s, e) => AtualizarArvore();
            frm.Show();
        }

        private void AbrirConfiguracoes()
        {
            frmConfiguracoes frm = new frmConfiguracoes
            {
                MdiParent = this,
                CaminhoProjeto = CaminhoProjeto
            };
            frm.Show();
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

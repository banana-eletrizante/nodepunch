using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmInicial : Form
    {
        private Button btnCriar;
        private Button btnAbrir;
        private Button btnInfo;
        private Button btnFechar;
        private Label lblTitulo;
        private Label lblSub;
        private Label lblVersao;
        private Label lblRecentes;
        private FlowLayoutPanel pnlRecentes;
        private Panel pnlAccent;
        private Panel pnlCard;

        private static readonly Color CorFundo = Color.FromArgb(12, 12, 14);
        private static readonly Color CorCard = Color.FromArgb(22, 22, 24);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(168, 168, 172);
        private static readonly Color CorBorda = Color.FromArgb(48, 48, 52);

        public frmInicial()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            CarregarRecentes();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            IconHelper.AplicarIcone(this);

            pnlAccent = new Panel { BackColor = CorAmarelo, Dock = DockStyle.Top, Height = 3 };

            lblVersao = new Label
            {
                Text = "v1.2",
                Font = new Font("Segoe UI Semibold", 8f),
                ForeColor = Color.Black,
                BackColor = CorAmarelo,
                AutoSize = true,
                Location = new Point(40, 28),
                Padding = new Padding(8, 2, 8, 2)
            };

            btnInfo = CriarBotaoIcone("?", new Point(428, 22));
            btnInfo.Click += (s, e) => MessageBox.Show(
                "NODEPUNCH  1.2\n\nGera um backend Express com:\n• helmet, rate-limit e /health\n• JWT (registrar / login)\n• MySQL, PostgreSQL ou Firebase\n• npm install ao criar o projeto\n\nandre-rosler.com",
                "NodePunch", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnFechar = CriarBotaoIcone("✕", new Point(468, 22));
            btnFechar.Click += (s, e) => Application.Exit();

            lblTitulo = new Label
            {
                Text = "NODEPUNCH",
                Font = new Font("Segoe UI Semibold", 30f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(40, 58),
                BackColor = Color.Transparent
            };
            lblSub = new Label
            {
                Text = "Backend Node.js pronto — sem montar pasta na mão.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = CorTextoSec,
                AutoSize = true,
                Location = new Point(42, 108),
                BackColor = Color.Transparent
            };

            btnCriar = CriarBotaoPrimario("Novo projeto", new Point(40, 156), new Size(210, 50));
            btnCriar.Click += btnCriar_Click;
            btnAbrir = CriarBotaoSecundario("Abrir pasta", new Point(266, 156), new Size(210, 50));
            btnAbrir.Click += btnAbrir_Click;

            pnlCard = new Panel
            {
                Location = new Point(40, 228),
                Size = new Size(436, 250),
                BackColor = CorCard
            };

            lblRecentes = new Label
            {
                Text = "PROJETOS RECENTES",
                Font = new Font("Segoe UI Semibold", 8.5f),
                ForeColor = CorTextoSec,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent
            };
            pnlRecentes = new FlowLayoutPanel
            {
                Location = new Point(12, 40),
                Size = new Size(412, 198),
                BackColor = CorCard,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            pnlCard.Controls.Add(lblRecentes);
            pnlCard.Controls.Add(pnlRecentes);

            this.ClientSize = new Size(516, 510);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "NodePunch";
            this.Font = new Font("Segoe UI", 9f);
            this.Controls.AddRange(new Control[]
            {
                pnlAccent, lblVersao, lblTitulo, lblSub,
                btnCriar, btnAbrir, pnlCard, btnInfo, btnFechar
            });
            this.ResumeLayout(false);
        }

        private void CarregarRecentes()
        {
            pnlRecentes.Controls.Clear();
            var items = Recentes.Listar();
            if (items.Count == 0)
            {
                pnlRecentes.Controls.Add(new Label
                {
                    Text = "Nada aqui ainda. Crie o primeiro backend.",
                    ForeColor = CorTextoSec,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9f),
                    Padding = new Padding(4, 8, 4, 8)
                });
                return;
            }
            foreach (var item in items)
            {
                var caminho = item.Caminho;
                var nome = item.Nome;
                var card = new Panel
                {
                    Width = 388,
                    Height = 52,
                    BackColor = Color.FromArgb(28, 28, 30),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 0, 8)
                };
                var nomeLbl = new Label
                {
                    Text = nome,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 10f),
                    AutoSize = true,
                    Location = new Point(14, 8),
                    BackColor = Color.Transparent
                };
                var pathLbl = new Label
                {
                    Text = Encurtar(caminho),
                    ForeColor = CorTextoSec,
                    Font = new Font("Segoe UI", 8f),
                    AutoSize = true,
                    Location = new Point(14, 28),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(nomeLbl);
                card.Controls.Add(pathLbl);
                EventHandler abrir = (s, e) => AbrirCaminho(caminho, nome);
                card.Click += abrir;
                nomeLbl.Click += abrir;
                pathLbl.Click += abrir;
                card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(40, 38, 20);
                card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(28, 28, 30);
                pnlRecentes.Controls.Add(card);
            }
        }

        private static string Encurtar(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho)) return "";
            return caminho.Length <= 54 ? caminho : "…" + caminho.Substring(caminho.Length - 53);
        }

        private Button CriarBotaoPrimario(string texto, Point loc, Size size)
        {
            var btn = new Button
            {
                Text = texto,
                Location = loc,
                Size = size,
                BackColor = CorAmarelo,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            return btn;
        }

        private Button CriarBotaoSecundario(string texto, Point loc, Size size)
        {
            var btn = new Button
            {
                Text = texto,
                Location = loc,
                Size = size,
                BackColor = CorCard,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = CorBorda;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 36, 40);
            return btn;
        }

        private Button CriarBotaoIcone(string texto, Point loc)
        {
            var btn = new Button
            {
                Text = texto,
                Location = loc,
                Size = new Size(32, 32),
                BackColor = CorFundo,
                ForeColor = CorTextoSec,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = CorCard;
            return btn;
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            new frmNovoProjeto { Inicial = this }.Show();
            this.Visible = false;
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Selecionar pasta"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            AbrirCaminho(Path.GetDirectoryName(ofd.FileName), null);
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
            if (!Directory.Exists(Path.Combine(caminho, "node_modules")))
                Shell.AbrirNpmInstall(caminho);
            new Form1 { NomeProjeto = nomeFinal, CaminhoProjeto = caminho }.Show();
            this.Visible = false;
        }
    }
}

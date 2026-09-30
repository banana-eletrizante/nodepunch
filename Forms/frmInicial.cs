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
        private Label lblRecentes;
        private FlowLayoutPanel pnlRecentes;
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);

        public frmInicial()
        {
            InitializeComponent();
            CarregarRecentes();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            IconHelper.AplicarIcone(this);

            lblTitulo = new Label { Text = "NODEPUNCH", Font = new Font("Segoe UI Semibold", 28f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(40, 48), BackColor = Color.Transparent };
            lblSub = new Label { Text = "O impulso que seu Node.js precisava", Font = new Font("Segoe UI", 10.5f), ForeColor = CorTextoSec, AutoSize = true, Location = new Point(43, 98), BackColor = Color.Transparent };
            pnlAccent = new Panel { BackColor = CorAmarelo, Location = new Point(40, 130), Size = new Size(72, 4) };

            btnCriar = CriarBotaoPrimario("Criar Projeto", new Point(40, 170), new Size(180, 48));
            btnCriar.Click += btnCriar_Click;
            btnAbrir = CriarBotaoSecundario("Abrir Projeto", new Point(236, 170), new Size(180, 48));
            btnAbrir.Click += btnAbrir_Click;

            lblRecentes = new Label { Text = "Recentes", Font = new Font("Segoe UI Semibold", 9.5f), ForeColor = CorTextoSec, AutoSize = true, Location = new Point(40, 236), BackColor = Color.Transparent };
            pnlRecentes = new FlowLayoutPanel { Location = new Point(40, 260), Size = new Size(376, 150), BackColor = CorFundo, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            btnInfo = CriarBotaoIcone("?", new Point(380, 18));
            btnInfo.Click += (s, e) => MessageBox.Show("NODEPUNCH\nVersão 1.1\nGera Express com helmet, rate-limit, health check e .env.example.", "Informações", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnFechar = CriarBotaoIcone("✕", new Point(418, 18));
            btnFechar.Click += (s, e) => Application.Exit();

            this.ClientSize = new Size(460, 430);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "NodePunch";
            this.Controls.AddRange(new Control[] { lblTitulo, lblSub, pnlAccent, btnCriar, btnAbrir, lblRecentes, pnlRecentes, btnInfo, btnFechar });
            this.ResumeLayout(false);
        }

        private void CarregarRecentes()
        {
            pnlRecentes.Controls.Clear();
            var items = Recentes.Listar();
            if (items.Count == 0)
            {
                pnlRecentes.Controls.Add(new Label { Text = "Nenhum projeto recente.", ForeColor = CorTextoSec, AutoSize = true });
                return;
            }
            foreach (var item in items)
            {
                var caminho = item.Caminho;
                var nome = item.Nome;
                var btn = CriarBotaoSecundario(nome, new Point(0, 0), new Size(352, 34));
                btn.Font = new Font("Segoe UI", 8.5f);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Click += (s, e) => AbrirCaminho(caminho, nome);
                pnlRecentes.Controls.Add(btn);
            }
        }

        private Button CriarBotaoPrimario(string texto, Point loc, Size size)
        {
            var btn = new Button { Text = texto, Location = loc, Size = size, BackColor = CorAmarelo, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            return btn;
        }

        private Button CriarBotaoSecundario(string texto, Point loc, Size size)
        {
            var btn = new Button { Text = texto, Location = loc, Size = size, BackColor = CorFundo, ForeColor = CorAmarelo, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = CorAmarelo;
            return btn;
        }

        private Button CriarBotaoIcone(string texto, Point loc)
        {
            var btn = new Button { Text = texto, Location = loc, Size = new Size(32, 32), BackColor = CorFundo, ForeColor = CorTextoSec, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            new frmNovoProjeto { Inicial = this }.Show();
            this.Visible = false;
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog { ValidateNames = false, CheckFileExists = false, CheckPathExists = true, FileName = "Selecionar pasta" };
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
            new Form1 { NomeProjeto = nomeFinal, CaminhoProjeto = caminho }.Show();
            this.Visible = false;
        }
    }
}

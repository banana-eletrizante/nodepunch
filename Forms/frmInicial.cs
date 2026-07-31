using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);
        private static readonly Color CorBorda = Color.FromArgb(58, 58, 58);

        public frmInicial()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            IconHelper.AplicarIcone(this);

            lblTitulo = new Label
            {
                Text = "NODEPUNCH",
                Font = new Font("Segoe UI Semibold", 28f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(40, 48),
                BackColor = Color.Transparent
            };

            lblSub = new Label
            {
                Text = "O impulso que seu Node.js precisava",
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = CorTextoSec,
                AutoSize = true,
                Location = new Point(43, 98),
                BackColor = Color.Transparent
            };

            pnlAccent = new Panel
            {
                BackColor = CorAmarelo,
                Location = new Point(40, 130),
                Size = new Size(72, 4)
            };

            btnCriar = CriarBotaoPrimario("Criar Projeto", new Point(40, 170), new Size(180, 48));
            btnCriar.Click += btnCriar_Click;

            btnAbrir = CriarBotaoSecundario("Abrir Projeto", new Point(236, 170), new Size(180, 48));
            btnAbrir.Click += btnAbrir_Click;

            btnInfo = CriarBotaoIcone("?", new Point(380, 18));
            btnInfo.Click += (s, e) =>
                MessageBox.Show(
                    "NODEPUNCH\nO impulso que seu Node.js precisava!\n\nVersão 1.0\nDesign inspirado no amarelo do JavaScript.",
                    "Informações",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            btnFechar = CriarBotaoIcone("✕", new Point(418, 18));
            btnFechar.Click += (s, e) => Application.Exit();

            this.ClientSize = new Size(460, 260);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "NodePunch";
            this.Font = new Font("Segoe UI", 9f);

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSub);
            this.Controls.Add(pnlAccent);
            this.Controls.Add(btnCriar);
            this.Controls.Add(btnAbrir);
            this.Controls.Add(btnInfo);
            this.Controls.Add(btnFechar);

            this.ResumeLayout(false);
            this.PerformLayout();
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
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 200, 20);
            return btn;
        }

        private Button CriarBotaoSecundario(string texto, Point loc, Size size)
        {
            var btn = new Button
            {
                Text = texto,
                Location = loc,
                Size = size,
                BackColor = CorFundo,
                ForeColor = CorAmarelo,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = CorAmarelo;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 30);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 50, 35);

            btn.MouseEnter += (s, e) =>
            {
                btn.FlatAppearance.BorderColor = CorAmareloHover;
                btn.ForeColor = CorAmareloHover;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.FlatAppearance.BorderColor = CorAmarelo;
                btn.ForeColor = CorAmarelo;
            };
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
                Font = new Font("Segoe UI", 11f),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 40);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 55, 55);

            btn.MouseEnter += (s, e) => btn.ForeColor = CorAmarelo;
            btn.MouseLeave += (s, e) => btn.ForeColor = CorTextoSec;
            return btn;
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            frmNovoProjeto frm = new frmNovoProjeto { Inicial = this };
            frm.Show();
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

            string caminho = Path.GetDirectoryName(ofd.FileName);
            if (!File.Exists(Path.Combine(caminho, "package.json")))
            {
                MessageBox.Show(
                    "Essa pasta não parece ser um projeto nodepunch (não encontrei package.json).",
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            Form1 form = new Form1
            {
                NomeProjeto = new DirectoryInfo(caminho).Name,
                CaminhoProjeto = caminho
            };
            form.Show();
            this.Visible = false;
        }
    }
}

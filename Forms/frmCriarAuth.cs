using System;
using System.Drawing;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmCriarAuth : Form
    {
        private Label lblInfo;
        private Label lblDetectado;
        private Button btnGerar;
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);

        public string CaminhoProjeto { get; set; }
        public event EventHandler ProjetoAtualizado;

        public frmCriarAuth()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            IconHelper.AplicarIcone(this);

            pnlAccent = new Panel { BackColor = CorAmarelo, Dock = DockStyle.Top, Height = 3 };

            Label lblTitulo = new Label
            {
                Text = "Autenticação JWT",
                Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 24),
                BackColor = Color.Transparent
            };

            lblInfo = new Label
            {
                Text = "Gera middleware de proteção de rotas (JWT), controller e rotas de\nregistrar/login (com senha criptografada via bcrypt).\n\nEndpoints criados:\n  POST /api/auth/registrar\n  POST /api/auth/login",
                ForeColor = CorTextoSec,
                AutoSize = true,
                Location = new Point(24, 64),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.Transparent
            };

            lblDetectado = new Label
            {
                Text = "",
                ForeColor = CorAmarelo,
                AutoSize = true,
                Location = new Point(24, 190),
                Font = new Font("Segoe UI Semibold", 9.5f),
                BackColor = Color.Transparent
            };

            btnGerar = new Button
            {
                Text = "Gerar Autenticação",
                Location = new Point(24, 226),
                Size = new Size(200, 44),
                BackColor = CorAmarelo,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnGerar.FlatAppearance.BorderSize = 0;
            btnGerar.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            btnGerar.Click += btnGerar_Click;

            this.ClientSize = new Size(400, 290);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Gerar Autenticação JWT";
            this.Font = new Font("Segoe UI", 9f);

            this.Controls.AddRange(new Control[] { pnlAccent, lblTitulo, lblInfo, lblDetectado, btnGerar });

            this.Shown += (s, e) =>
            {
                TipoBanco tipo = Funcoes.DetectarTipoBanco(CaminhoProjeto);
                if (tipo == TipoBanco.Nenhum)
                {
                    lblDetectado.Text = "⚠ Este projeto não tem banco de dados configurado.\nA autenticação precisa de uma tabela/coleção 'usuarios'.";
                    btnGerar.Enabled = false;
                }
                else
                {
                    lblDetectado.Text = "Banco detectado: " + tipo;
                }
            };

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnGerar_Click(object sender, EventArgs e)
        {
            try
            {
                TipoBanco tipo = Funcoes.DetectarTipoBanco(CaminhoProjeto);
                bool depsOk = Funcoes.GerarAuthJWT(CaminhoProjeto, tipo);
                ProjetoAtualizado?.Invoke(this, EventArgs.Empty);

                string avisoDeps = depsOk ? "" :
                    "\n\n⚠ Não consegui adicionar automaticamente 'jsonwebtoken'/'bcryptjs' no package.json " +
                    "(formato do arquivo pode ter sido alterado manualmente) — adicione à mão se necessário.";

                MessageBox.Show(
                    "Autenticação JWT gerada com sucesso!\n\nRode 'npm install' novamente pra baixar jsonwebtoken e bcryptjs.\n\n" +
                    (tipo != TipoBanco.Firebase ? "Não esqueça de criar a tabela 'usuarios' (colunas: id, email, senha) no seu banco." : "Não esqueça de criar a coleção 'usuarios' no Firestore.") +
                    avisoDeps,
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }
}

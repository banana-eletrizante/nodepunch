using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmConfiguracoes : Form
    {
        private TextBox txtOrigensDev;
        private TextBox txtOrigensProd;
        private TextBox txtEnv;
        private Button btnSalvar;
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorSuperficie = Color.FromArgb(30, 30, 30);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);

        public string CaminhoProjeto { get; set; }

        public frmConfiguracoes()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            IconHelper.AplicarIcone(this);

            pnlAccent = new Panel { BackColor = CorAmarelo, Dock = DockStyle.Top, Height = 3 };

            Label lblTitulo = CriarLabel("Configurações do Projeto", new Point(24, 24));
            lblTitulo.Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold);

            Label lblCorsDev = CriarLabel("Origens CORS (development) — separadas por vírgula:", new Point(24, 68));
            txtOrigensDev = CriarTextBox(new Point(24, 90), new Size(452, 28));

            Label lblCorsProd = CriarLabel("Origens CORS (production) — separadas por vírgula:", new Point(24, 128));
            txtOrigensProd = CriarTextBox(new Point(24, 150), new Size(452, 28));

            Label lblEnv = CriarLabel("Variáveis de ambiente (.env):", new Point(24, 190));
            txtEnv = new TextBox
            {
                Location = new Point(24, 212),
                Size = new Size(452, 180),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9.5f)
            };

            btnSalvar = new Button
            {
                Text = "Salvar Configurações",
                Location = new Point(266, 406),
                Size = new Size(210, 44),
                BackColor = CorAmarelo,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            btnSalvar.Click += btnSalvar_Click;

            this.ClientSize = new Size(500, 470);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Configurações do Projeto";
            this.Font = new Font("Segoe UI", 9f);

            this.Controls.AddRange(new Control[]
            {
                pnlAccent, lblTitulo,
                lblCorsDev, txtOrigensDev,
                lblCorsProd, txtOrigensProd,
                lblEnv, txtEnv,
                btnSalvar
            });

            this.Shown += frmConfiguracoes_Shown;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label CriarLabel(string texto, Point loc)
        {
            return new Label
            {
                Text = texto,
                Location = loc,
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5f),
                BackColor = Color.Transparent
            };
        }

        private TextBox CriarTextBox(Point loc, Size size)
        {
            return new TextBox
            {
                Location = loc,
                Size = size,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
        }

        private void frmConfiguracoes_Shown(object sender, EventArgs e)
        {
            string caminhoCors = Path.Combine(CaminhoProjeto, "src", "config", "cors.js");
            if (File.Exists(caminhoCors))
            {
                var origens = Funcoes.ExtractCorsOrigins(caminhoCors);
                txtOrigensDev.Text = string.Join(", ", origens["development"]);
                txtOrigensProd.Text = string.Join(", ", origens["production"]);
            }

            string caminhoEnv = Path.Combine(CaminhoProjeto, ".env");
            if (File.Exists(caminhoEnv))
                txtEnv.Text = File.ReadAllText(caminhoEnv);
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                string[] origensDev = txtOrigensDev.Text.Split(',').Select(o => o.Trim()).Where(o => o != "").ToArray();
                string[] origensProd = txtOrigensProd.Text.Split(',').Select(o => o.Trim()).Where(o => o != "").ToArray();

                string listaDev = string.Join(", ", origensDev.Select(o => "'" + Funcoes.EscaparJS(o) + "'"));
                string listaProd = string.Join(", ", origensProd.Select(o => "'" + Funcoes.EscaparJS(o) + "'"));

                string tab = "\t";
                string corsConteudo =
"// Configuração de CORS\n" +
"const CORS_ORIGINS = {\n" +
tab + "development: [" + listaDev + "],\n" +
tab + "production: [" + listaProd + "]\n" +
"};\n\n" +
"const ambiente = process.env.NODE_ENV === 'production' ? 'production' : 'development';\n\n" +
"module.exports = {\n" +
tab + "origin: CORS_ORIGINS[ambiente],\n" +
tab + "credentials: true\n" +
"};\n";

                Funcoes.CriarArquivo(Path.Combine(CaminhoProjeto, "src", "config"), "cors", corsConteudo);
                File.WriteAllText(Path.Combine(CaminhoProjeto, ".env"), txtEnv.Text);

                MessageBox.Show("Configurações salvas com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }
}

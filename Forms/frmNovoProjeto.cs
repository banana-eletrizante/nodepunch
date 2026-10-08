using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmNovoProjeto : Form
    {
        private TextBox txtNomeProjeto;
        private TextBox txtCaminho;
        private Button btnAbrirPasta;
        private ComboBox cboBanco;
        private Panel pnlAccent;
        private Panel pnlCamposBanco;
        private Button btnCriarProjeto;
        private CheckBox chkInstalarDependencias;

        // Campos MySQL / PostgreSQL
        private TextBox txtServer, txtPorta, txtSchema, txtUsuario, txtSenha;
        private CheckBox chkSP;

        // Campos Firebase
        private TextBox txtFirebaseProjectId, txtFirebaseCredenciais;
        private Button btnBuscarCredenciais;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorSuperficie = Color.FromArgb(30, 30, 30);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);

        public frmInicial Inicial { get; set; }
        public bool IcCriou { get; set; }

        public frmNovoProjeto()
        {
            Tema.Preparar(this);
            InitializeComponent();
            Tema.Dialogo(this, "Novo projeto", "Configure a pasta e o banco do seu backend.", btnCriarProjeto);
            this.FormClosed += (s, e) => { if (!IcCriou && Inicial != null) Inicial.Visible = true; };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            IconHelper.AplicarIcone(this);

            pnlAccent = new Panel { BackColor = CorAmarelo, Dock = DockStyle.Top, Height = 3 };

            Label lblNome = CriarLabel("Nome do Projeto:", new Point(24, 24));
            txtNomeProjeto = CriarTextBox(new Point(24, 48), new Size(392, 28));

            Label lblCaminho = CriarLabel("Caminho do Projeto:", new Point(24, 90));
            txtCaminho = CriarTextBox(new Point(24, 114), new Size(348, 28));
            txtCaminho.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            btnAbrirPasta = CriarBotaoSecundario("...", new Point(380, 113), new Size(36, 30));
            btnAbrirPasta.Click += btnAbrirPasta_Click;

            Label lblBanco = CriarLabel("Banco de Dados:", new Point(24, 160));
            cboBanco = new ComboBox
            {
                Location = new Point(24, 184),
                Size = new Size(392, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };
            cboBanco.Items.AddRange(new object[] { "Nenhum", "MySQL", "PostgreSQL", "Firebase (Firestore)" });
            cboBanco.SelectedIndex = 1; // MySQL como padrão
            cboBanco.SelectedIndexChanged += cboBanco_SelectedIndexChanged;

            pnlCamposBanco = new Panel
            {
                Location = new Point(24, 222),
                Size = new Size(392, 190),
                BackColor = CorFundo
            };

            CriarCamposMySQLPostgres();
            CriarCamposFirebase();

            chkInstalarDependencias = new CheckBox
            {
                Text = "Instalar dependências do backend (npm install)",
                Location = new Point(24, 426),
                AutoSize = true,
                ForeColor = Color.White
            };
            btnCriarProjeto = CriarBotaoPrimario("Criar Projeto", new Point(256, 464), new Size(160, 44));
            btnCriarProjeto.Click += (sender, args) =>
            {
                try { btnCriarProjeto_Click(sender, args); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Erro ao criar projeto", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            this.ClientSize = new Size(440, 534);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Novo Projeto";
            this.Font = new Font("Segoe UI", 9f);
            this.AutoScroll = true;

            this.Controls.AddRange(new Control[]
            {
                pnlAccent,
                lblNome, txtNomeProjeto,
                lblCaminho, txtCaminho, btnAbrirPasta,
                lblBanco, cboBanco,
                pnlCamposBanco,
                chkInstalarDependencias, btnCriarProjeto
            });

            AtualizarCamposBanco();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CriarCamposMySQLPostgres()
        {
            Label lblServer = CriarLabel("Host / Server:", new Point(0, 0));
            txtServer = CriarTextBox(new Point(0, 24), new Size(180, 28));
            txtServer.Text = "localhost";

            Label lblPorta = CriarLabel("Porta:", new Point(196, 0));
            txtPorta = CriarTextBox(new Point(196, 24), new Size(196, 28));
            txtPorta.Text = "3306";

            Label lblSchema = CriarLabel("Schema / Database:", new Point(0, 60));
            txtSchema = CriarTextBox(new Point(0, 84), new Size(392, 28));

            Label lblUsuario = CriarLabel("Usuário:", new Point(0, 120));
            txtUsuario = CriarTextBox(new Point(0, 144), new Size(180, 28));
            txtUsuario.Text = "root";

            Label lblSenha = CriarLabel("Senha:", new Point(196, 120));
            txtSenha = CriarTextBox(new Point(196, 144), new Size(196, 28));
            txtSenha.PasswordChar = '*';

            chkSP = CriarCheckBox("Utiliza Stored Procedures (só MySQL)", new Point(0, 184));

            pnlCamposBanco.Controls.AddRange(new Control[]
            {
                lblServer, txtServer, lblPorta, txtPorta,
                lblSchema, txtSchema,
                lblUsuario, txtUsuario, lblSenha, txtSenha,
                chkSP
            });
        }

        private void CriarCamposFirebase()
        {
            Label lblProjectId = CriarLabel("Firebase Project ID:", new Point(0, 0));
            txtFirebaseProjectId = CriarTextBox(new Point(0, 24), new Size(392, 28));

            Label lblCredenciais = CriarLabel("Arquivo de credenciais (serviceAccountKey.json):", new Point(0, 60));
            txtFirebaseCredenciais = CriarTextBox(new Point(0, 84), new Size(356, 28));
            btnBuscarCredenciais = CriarBotaoSecundario("...", new Point(0, 84), new Size(36, 30));
            // reposiciona o botão pra ficar colado ao textbox
            txtFirebaseCredenciais.Size = new Size(320, 28);
            btnBuscarCredenciais.Location = new Point(336, 83);
            btnBuscarCredenciais.Click += btnBuscarCredenciais_Click;

            Label lblAjuda = CriarLabel("Baixe o JSON em: Console Firebase → Configurações do Projeto → Contas de Serviço", new Point(0, 120));
            lblAjuda.ForeColor = CorTextoSec;
            lblAjuda.Font = new Font("Segoe UI", 8f);
            lblAjuda.MaximumSize = new Size(392, 0);
            lblAjuda.AutoSize = true;

            pnlCamposBanco.Controls.AddRange(new Control[]
            {
                lblProjectId, txtFirebaseProjectId,
                lblCredenciais, txtFirebaseCredenciais, btnBuscarCredenciais,
                lblAjuda
            });
        }

        private void cboBanco_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarCamposBanco();
        }

        private void AtualizarCamposBanco()
        {
            bool ehSql = cboBanco.SelectedIndex == 1 || cboBanco.SelectedIndex == 2; // MySQL / PostgreSQL
            bool ehFirebase = cboBanco.SelectedIndex == 3;
            bool ehMySQL = cboBanco.SelectedIndex == 1;

            foreach (Control c in pnlCamposBanco.Controls)
                c.Visible = false;

            if (ehSql)
            {
                txtServer.Visible = txtPorta.Visible = txtSchema.Visible = true;
                txtUsuario.Visible = txtSenha.Visible = true;
                chkSP.Visible = ehMySQL;
                txtPorta.Text = ehMySQL ? "3306" : "5432";

                foreach (Control c in pnlCamposBanco.Controls)
                {
                    if (c is Label lbl && (lbl.Text.StartsWith("Host") || lbl.Text.StartsWith("Porta") ||
                        lbl.Text.StartsWith("Schema") || lbl.Text.StartsWith("Usuário") || lbl.Text.StartsWith("Senha")))
                        lbl.Visible = true;
                }
            }
            else if (ehFirebase)
            {
                txtFirebaseProjectId.Visible = true;
                txtFirebaseCredenciais.Visible = true;
                btnBuscarCredenciais.Visible = true;

                foreach (Control c in pnlCamposBanco.Controls)
                {
                    if (c is Label lbl && (lbl.Text.StartsWith("Firebase") || lbl.Text.StartsWith("Arquivo") || lbl.Text.StartsWith("Baixe")))
                        lbl.Visible = true;
                }
            }
            // Se "Nenhum", tudo fica invisível mesmo
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

        private CheckBox CriarCheckBox(string texto, Point loc)
        {
            return new CheckBox
            {
                Text = texto,
                Location = loc,
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent
            };
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
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
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

            btn.MouseEnter += (s, e) => { btn.FlatAppearance.BorderColor = CorAmareloHover; btn.ForeColor = CorAmareloHover; };
            btn.MouseLeave += (s, e) => { btn.FlatAppearance.BorderColor = CorAmarelo; btn.ForeColor = CorAmarelo; };
            return btn;
        }

        private void btnAbrirPasta_Click(object sender, EventArgs e)
        {
            using var pasta = new FolderBrowserDialog
            {
                Description = "Escolha onde criar seu backend",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(txtCaminho.Text) ? txtCaminho.Text : ""
            };
            if (pasta.ShowDialog(this) == DialogResult.OK)
                txtCaminho.Text = pasta.SelectedPath;
        }

        private void btnBuscarCredenciais_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Arquivo JSON (*.json)|*.json",
                Title = "Selecione o serviceAccountKey.json"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                txtFirebaseCredenciais.Text = ofd.FileName;
        }

        private void btnCriarProjeto_Click(object sender, EventArgs e)
        {
            txtNomeProjeto.Text = txtNomeProjeto.Text.Trim();
            if (txtNomeProjeto.Text == "")
            {
                MessageBox.Show("Digite o nome do Projeto", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            txtNomeProjeto.Text = Funcoes.ToCamelCase(txtNomeProjeto.Text);
            if (txtNomeProjeto.Text == "")
            {
                MessageBox.Show("O nome do Projeto só tinha caracteres inválidos (acentos/símbolos). Use letras ou números.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Directory.Exists(txtCaminho.Text))
            {
                MessageBox.Show("Pasta não existe!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            TipoBanco tipo = cboBanco.SelectedIndex switch
            {
                1 => TipoBanco.MySQL,
                2 => TipoBanco.PostgreSQL,
                3 => TipoBanco.Firebase,
                _ => TipoBanco.Nenhum
            };

            if ((tipo == TipoBanco.MySQL || tipo == TipoBanco.PostgreSQL) && txtSchema.Text.Trim() == "")
            {
                MessageBox.Show("Informe o nome do schema/database", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if ((tipo == TipoBanco.MySQL || tipo == TipoBanco.PostgreSQL) &&
                (!int.TryParse(txtPorta.Text.Trim(), out int porta) || porta < 1 || porta > 65535))
            {
                MessageBox.Show("A porta precisa estar entre 1 e 65535.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (tipo == TipoBanco.Firebase && (txtFirebaseProjectId.Text.Trim() == "" || txtFirebaseCredenciais.Text.Trim() == ""))
            {
                MessageBox.Show("Informe o Project ID e o arquivo de credenciais do Firebase", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (tipo == TipoBanco.Firebase && !File.Exists(txtFirebaseCredenciais.Text.Trim()))
            {
                MessageBox.Show("O arquivo de credenciais do Firebase informado não existe.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Funcoes.EhNomeArquivoWindows(txtNomeProjeto.Text))
            {
                MessageBox.Show("Esse nome é reservado pelo Windows. Escolha outro nome para o projeto.", "Atenção");
                return;
            }
            string caminhoProjetoChecagem = Path.Combine(txtCaminho.Text, txtNomeProjeto.Text);
            if (Directory.Exists(caminhoProjetoChecagem) && Directory.GetFileSystemEntries(caminhoProjetoChecagem).Length > 0)
            {
                if (MessageBox.Show(
                        $"Já existe uma pasta \"{txtNomeProjeto.Text}\" com arquivos dentro. Continuar pode sobrescrever arquivos existentes.\n\nDeseja continuar mesmo assim?",
                        "Atenção — pasta já existe",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                string caminhoProjeto = Path.Combine(txtCaminho.Text, txtNomeProjeto.Text);
                Funcoes.CriarPasta(caminhoProjeto);
                Funcoes.CriarPasta(Path.Combine(caminhoProjeto, "src", "models"));
                Funcoes.CriarPasta(Path.Combine(caminhoProjeto, "src", "controllers"));
                Funcoes.CriarPasta(Path.Combine(caminhoProjeto, "src", "routes"));

                if (tipo != TipoBanco.Nenhum)
                {
                    ConexaoBanco dados = new ConexaoBanco { Tipo = tipo };

                    if (tipo == TipoBanco.MySQL || tipo == TipoBanco.PostgreSQL)
                    {
                        dados.Server = txtServer.Text;
                        dados.Porta = txtPorta.Text;
                        dados.User = txtUsuario.Text;
                        dados.Password = txtSenha.Text;
                        dados.Schema = txtSchema.Text;
                        dados.ComSP = tipo == TipoBanco.MySQL && chkSP.Checked;
                    }
                    else if (tipo == TipoBanco.Firebase)
                    {
                        dados.FirebaseProjectId = txtFirebaseProjectId.Text;
                        const string nomeArquivoCredenciais = "firebaseServiceAccountKey.json";
                        dados.FirebaseServiceAccountPath = "./" + nomeArquivoCredenciais;
                        File.Copy(txtFirebaseCredenciais.Text, Path.Combine(caminhoProjeto, nomeArquivoCredenciais), true);
                    }

                    Funcoes.CriarClasseBaseBD(dados, Path.Combine(caminhoProjeto, "src", "base"));
                    Funcoes.CriarEnv(caminhoProjeto, dados);
                }
                else Funcoes.CriarEnv(caminhoProjeto, new ConexaoBanco());

                Funcoes.CriarPackageJson(caminhoProjeto, txtNomeProjeto.Text, tipo);
                Funcoes.CriarServerJs(caminhoProjeto);
                Funcoes.CriarReadme(caminhoProjeto, txtNomeProjeto.Text, tipo);
                Funcoes.CriarGitignore(caminhoProjeto);
                Recentes.Registrar(txtNomeProjeto.Text, caminhoProjeto);
                if (chkInstalarDependencias.Checked) Shell.AbrirNpmInstall(caminhoProjeto);

                Form1 form = new Form1
                {
                    NomeProjeto = txtNomeProjeto.Text,
                    TelaInicial = Inicial,
                    CaminhoProjeto = caminhoProjeto,
                    UsaBanco = tipo != TipoBanco.Nenhum
                };
                form.Show();
                IcCriou = true;
                Inicial?.Hide();
                MessageBox.Show(
                    "Projeto criado com sucesso!\nNão esqueça de rodar 'npm install' na pasta do projeto.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }
}

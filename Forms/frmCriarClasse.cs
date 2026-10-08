using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmCriarClasse : Form
    {
        private TextBox txtNomeClasse;
        private TextBox txtPropriedade;
        private ListBox lstPropriedades;
        private Button btnAdicionarPropriedade;
        private CheckBox chkModelBanco;
        private CheckBox chkControlador;
        private Button btnCriarClasses;
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorSuperficie = Color.FromArgb(30, 30, 30);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);
        private static readonly Color CorBorda = Color.FromArgb(58, 58, 58);

        public string NomeProjeto { get; set; }
        public string CaminhoProjeto { get; set; }
        public bool UsaBanco { get; set; }

        public event EventHandler ProjetoAtualizado;

        public frmCriarClasse()
        {
            Tema.Preparar(this);
            InitializeComponent();
            Tema.Dialogo(this, "Model e Controller", "Defina a entidade e suas propriedades.", btnCriarClasses);
            this.Shown += (s, e) =>
            {
                chkModelBanco.Visible = UsaBanco;
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            IconHelper.AplicarIcone(this);

            pnlAccent = new Panel
            {
                BackColor = CorAmarelo,
                Dock = DockStyle.Top,
                Height = 3
            };

            Label lblNome = CriarLabel("Nome da Classe:", new Point(24, 24));
            txtNomeClasse = CriarTextBox(new Point(24, 48), new Size(372, 28));

            Label lblProp = CriarLabel("Propriedade / Atributo:", new Point(24, 90));
            txtPropriedade = CriarTextBox(new Point(24, 114), new Size(300, 28));
            txtPropriedade.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Return)
                {
                    btnAdicionarPropriedade_Click(s, e);
                    e.SuppressKeyPress = true;
                }
            };

            btnAdicionarPropriedade = CriarBotaoPrimario("+", new Point(334, 113), new Size(62, 30));
            btnAdicionarPropriedade.Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold);
            btnAdicionarPropriedade.Click += btnAdicionarPropriedade_Click;

            Label lblLista = CriarLabel("Propriedades:", new Point(24, 156));
            lblLista.ForeColor = CorTextoSec;

            lstPropriedades = new ListBox
            {
                Location = new Point(24, 178),
                Size = new Size(372, 120),
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                IntegralHeight = false
            };
            lstPropriedades.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && lstPropriedades.SelectedIndex >= 0)
                    lstPropriedades.Items.RemoveAt(lstPropriedades.SelectedIndex);
            };

            chkModelBanco = CriarCheckBox("Model extends Banco (usa persistência)", new Point(24, 312));
            chkModelBanco.Visible = false;

            chkControlador = CriarCheckBox("Criar Controller junto", new Point(24, 340));

            btnCriarClasses = CriarBotaoPrimario("Criar Classe(s)", new Point(236, 380), new Size(160, 44));
            btnCriarClasses.Click += (sender, args) =>
            {
                try { btnCriarClasses_Click(sender, args); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Erro ao criar classe", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            this.ClientSize = new Size(420, 450);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Criar Model / Controller";
            this.Font = new Font("Segoe UI", 9f);

            this.Controls.AddRange(new Control[]
            {
                pnlAccent,
                lblNome, txtNomeClasse,
                lblProp, txtPropriedade, btnAdicionarPropriedade,
                lblLista, lstPropriedades,
                chkModelBanco, chkControlador,
                btnCriarClasses
            });

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

        private void btnAdicionarPropriedade_Click(object sender, EventArgs e)
        {
            txtPropriedade.Text = Funcoes.ToCamelCase(txtPropriedade.Text.Trim());
            if (txtPropriedade.Text == "")
            {
                MessageBox.Show("Digite nome da Propriedade/Atributo", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            foreach (var item in lstPropriedades.Items)
            {
                if (item.ToString().Equals(txtPropriedade.Text, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Essa propriedade já foi adicionada.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    txtPropriedade.Clear();
                    txtPropriedade.Focus();
                    return;
                }
            }
            if (!Funcoes.EhIdentificadorJS(txtPropriedade.Text) || txtPropriedade.Text == "constructor")
            {
                MessageBox.Show("A propriedade deve começar com letra ou sublinhado e não pode usar palavras reservadas ou 'constructor'.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtPropriedade.Clear();
                txtPropriedade.Focus();
                return;
            }
            lstPropriedades.Items.Add(txtPropriedade.Text);
            txtPropriedade.Clear();
            txtPropriedade.Focus();
        }

        private void btnCriarClasses_Click(object sender, EventArgs e)
        {
            txtNomeClasse.Text = txtNomeClasse.Text.Trim();
            if (txtNomeClasse.Text == "")
            {
                MessageBox.Show("Digite nome da Classe", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (lstPropriedades.Items.Count == 0 &&
                MessageBox.Show("A classe não possui nenhum atributo/propriedade, criar mesmo assim?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            string nomeClasse = Funcoes.PrimeiraMaiusculaSemAcento(Funcoes.ToCamelCase(txtNomeClasse.Text));
            if (nomeClasse == "")
            {
                MessageBox.Show("O nome da Classe só tinha caracteres inválidos (acentos/símbolos). Use letras ou números.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!Funcoes.EhIdentificadorJS(nomeClasse) || !Funcoes.EhNomeArquivoWindows(nomeClasse) || (chkModelBanco.Checked && nomeClasse == "Banco"))
            {
                MessageBox.Show("Use um nome que comece com letra ou sublinhado, sem palavras reservadas. Um model que estende Banco precisa ter outro nome.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string caminhoModel = Path.Combine(CaminhoProjeto, "src", "models", nomeClasse + ".js");
            string caminhoController = Path.Combine(CaminhoProjeto, "src", "controllers", nomeClasse + "Controller.js");
            bool modelExiste = File.Exists(caminhoModel);
            bool controllerExiste = chkControlador.Checked && File.Exists(caminhoController);

            if (modelExiste || controllerExiste)
            {
                string oQue = modelExiste && controllerExiste ? "o Model e o Controller" : (modelExiste ? "o Model" : "o Controller");
                if (MessageBox.Show(
                        $"Já existe {oQue} \"{nomeClasse}\". Deseja sobrescrever?",
                        "Atenção — arquivo já existe",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }

            string tab = "\t";

            string campos = "";
            string getSet = "";
            string parametrosCtor = "";
            string atribuicoesCtor = "";

            for (int i = 0; i < lstPropriedades.Items.Count; i++)
            {
                string prop = lstPropriedades.Items[i].ToString();
                campos += tab + "#" + prop + ";\n";

                parametrosCtor += (parametrosCtor == "" ? "" : ", ") + prop + " = null";
                atribuicoesCtor += tab + tab + "this.#" + prop + " = " + prop + ";\n";

                getSet += tab + "get " + prop + "() { return this.#" + prop + "; }\n";
                getSet += tab + "set " + prop + "(valor) { this.#" + prop + " = valor; }\n\n";
            }

            string extendsBanco = chkModelBanco.Checked ? " extends Banco" : "";
            string requireBanco = chkModelBanco.Checked ? "const Banco = require('../base/Banco');\n\n" : "";

            string conteudoModel =
requireBanco +
"class " + nomeClasse + extendsBanco + " {\n" +
campos + "\n" +
tab + "constructor(" + parametrosCtor + ") {\n" +
(chkModelBanco.Checked ? tab + tab + "super();\n" : "") +
atribuicoesCtor +
tab + "}\n\n" +
getSet.TrimEnd('\n') + "\n" +
"}\n\n" +
"module.exports = " + nomeClasse + ";\n";

            Funcoes.CriarArquivo(Path.Combine(CaminhoProjeto, "src", "models"), nomeClasse, conteudoModel);

            if (chkControlador.Checked)
            {
                string requireModel = "const " + nomeClasse + " = require('../models/" + nomeClasse + "');\n\n";
                string conteudoController =
requireModel +
"class " + nomeClasse + "Controller {\n" +
tab + "// Implemente aqui os métodos do controller (ex: listar, criar, atualizar, remover)\n" +
"}\n\n" +
"module.exports = " + nomeClasse + "Controller;\n";
                Funcoes.CriarArquivo(Path.Combine(CaminhoProjeto, "src", "controllers"), nomeClasse + "Controller", conteudoController);
            }

            txtNomeClasse.Clear();
            lstPropriedades.Items.Clear();
            chkModelBanco.Checked = false;
            chkControlador.Checked = false;

            ProjetoAtualizado?.Invoke(this, EventArgs.Empty);
            MessageBox.Show("Classe(s) criada(s) com sucesso!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            Close();
        }
    }
}

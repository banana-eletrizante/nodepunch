using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NodePunch.Core;

namespace NodePunch.Forms
{
    public class frmCriarAPI : Form
    {
        private TextBox txtNomeAPI;
        private CheckBox chkGET, chkPOST, chkPUT, chkDELETE;
        private CheckBox chkValidator;
        private TextBox txtCampos;
        private Button btnCriarAPI;
        private Panel pnlAccent;

        private static readonly Color CorFundo = Color.FromArgb(18, 18, 18);
        private static readonly Color CorSuperficie = Color.FromArgb(30, 30, 30);
        private static readonly Color CorAmarelo = Color.FromArgb(247, 223, 30);
        private static readonly Color CorAmareloHover = Color.FromArgb(255, 229, 102);
        private static readonly Color CorTextoSec = Color.FromArgb(160, 160, 160);

        public string NomeProjeto { get; set; }
        public string CaminhoProjeto { get; set; }
        public bool UsaBanco { get; set; }

        public event EventHandler ProjetoAtualizado;

        public frmCriarAPI()
        {
            InitializeComponent();
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

            Label lblNome = new Label
            {
                Text = "Nome da Rota (ex: usuarios):",
                Location = new Point(24, 24),
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5f),
                BackColor = Color.Transparent
            };

            txtNomeAPI = new TextBox
            {
                Location = new Point(24, 48),
                Size = new Size(292, 28),
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };

            Label lblMetodos = new Label
            {
                Text = "Métodos HTTP:",
                Location = new Point(24, 92),
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5f),
                BackColor = Color.Transparent
            };

            chkGET = CriarCheckBox("GET", new Point(24, 120));
            chkGET.Checked = true;

            chkPOST = CriarCheckBox("POST", new Point(100, 120));
            chkPOST.Checked = true;

            chkPUT = CriarCheckBox("PUT", new Point(180, 120));
            chkPUT.Checked = true;

            chkDELETE = CriarCheckBox("DELETE", new Point(250, 120));
            chkDELETE.Checked = true;

            chkValidator = CriarCheckBox("Usar express-validator (validação robusta)", new Point(24, 150));
            chkValidator.CheckedChanged += (s, e) => txtCampos.Enabled = chkValidator.Checked;

            Label lblCampos = new Label
            {
                Text = "Campos obrigatórios do corpo (separados por vírgula):",
                Location = new Point(24, 178),
                ForeColor = CorTextoSec,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent
            };

            txtCampos = new TextBox
            {
                Location = new Point(24, 198),
                Size = new Size(292, 26),
                BackColor = CorSuperficie,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                Enabled = false,
                PlaceholderText = "ex: nome, email"
            };

            btnCriarAPI = new Button
            {
                Text = "Criar Rota",
                Location = new Point(156, 234),
                Size = new Size(160, 44),
                BackColor = CorAmarelo,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnCriarAPI.FlatAppearance.BorderSize = 0;
            btnCriarAPI.FlatAppearance.MouseOverBackColor = CorAmareloHover;
            btnCriarAPI.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 200, 20);
            btnCriarAPI.Click += btnCriarAPI_Click;

            this.ClientSize = new Size(340, 300);
            this.BackColor = CorFundo;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Criar Rota Express";
            this.Font = new Font("Segoe UI", 9f);

            this.Controls.AddRange(new Control[]
            {
                pnlAccent,
                lblNome, txtNomeAPI,
                lblMetodos,
                chkGET, chkPOST, chkPUT, chkDELETE,
                chkValidator, lblCampos, txtCampos,
                btnCriarAPI
            });

            this.ResumeLayout(false);
            this.PerformLayout();
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

        private void btnCriarAPI_Click(object sender, EventArgs e)
        {
            txtNomeAPI.Text = txtNomeAPI.Text.Trim();
            if (txtNomeAPI.Text == "")
            {
                MessageBox.Show("Digite o nome da rota", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!chkGET.Checked && !chkPOST.Checked && !chkPUT.Checked && !chkDELETE.Checked)
            {
                MessageBox.Show("Escolha pelo menos um método HTTP", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string nome = Funcoes.ToCamelCase(txtNomeAPI.Text);
            if (nome == "")
            {
                MessageBox.Show("O nome da Rota só tinha caracteres inválidos (acentos/símbolos). Use letras ou números.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!Funcoes.EhIdentificadorJS(nome))
            {
                MessageBox.Show("A rota deve começar com letra ou sublinhado e não pode usar palavras reservadas.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            string tab = "\t";
            bool usaValidator = chkValidator.Checked;
            try
            {
                Funcoes.ValidarRegistroRota(CaminhoProjeto, nome);

            string caminhoRota = Path.Combine(CaminhoProjeto, "src", "routes", nome + "Routes.js");
            if (File.Exists(caminhoRota))
            {
                if (MessageBox.Show(
                        $"Já existe uma rota \"{nome}\". Deseja sobrescrever?",
                        "Atenção — arquivo já existe",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }

            string[] campos = txtCampos.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(c => c.Trim()).Where(c => c != "").Distinct().ToArray();

            string conteudo;

            if (usaValidator)
            {
                string cadeiaValidacoes = campos.Length == 0
                    ? tab + "// Nenhum campo obrigatório definido — ajuste manualmente se precisar\n"
                    : string.Join("\n", campos.Select(c => tab + "body('" + Funcoes.EscaparJS(c) + "').notEmpty().withMessage('" + Funcoes.EscaparJS(c) + " é obrigatório'),"));

                string handlersValidator = "";
                if (chkGET.Checked)
                    handlersValidator += "router.get('/', async (req, res) => {\n" + tab + "// TODO: implementar listagem\n" + tab + "res.json({ mensagem: 'GET " + nome + "' });\n});\n\n";
                if (chkPOST.Checked)
                    handlersValidator += "router.post('/', [\n" + cadeiaValidacoes + "\n], async (req, res) => {\n" +
                        tab + "const erros = validationResult(req);\n" +
                        tab + "if (!erros.isEmpty()) return res.status(400).json({ erros: erros.array() });\n\n" +
                        tab + "// TODO: implementar criação\n" +
                        tab + "res.status(201).json({ mensagem: 'Criado com sucesso' });\n});\n\n";
                if (chkPUT.Checked)
                    handlersValidator += "router.put('/:id', [\n" + cadeiaValidacoes + "\n], async (req, res) => {\n" +
                        tab + "const erros = validationResult(req);\n" +
                        tab + "if (!erros.isEmpty()) return res.status(400).json({ erros: erros.array() });\n\n" +
                        tab + "// TODO: implementar atualização\n" +
                        tab + "res.json({ mensagem: 'Atualizado com sucesso' });\n});\n\n";
                if (chkDELETE.Checked)
                    handlersValidator += "router.delete('/:id', async (req, res) => {\n" + tab + "// TODO: implementar remoção\n" + tab + "res.json({ mensagem: 'Removido com sucesso' });\n});\n\n";

                conteudo =
"const express = require('express');\n" +
"const { body, validationResult } = require('express-validator');\n" +
"const router = express.Router();\n\n" +
handlersValidator +
"module.exports = router;\n";

                bool depOk = Funcoes.AdicionarDependencia(CaminhoProjeto, "express-validator", "^7.2.0");
                if (!depOk)
                {
                    MessageBox.Show(
                        "Não consegui adicionar 'express-validator' ao package.json. Corrija o JSON e tente novamente.",
                        "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                string handlers = "";
                if (chkGET.Checked)
                    handlers += "router.get('/', async (req, res) => {\n" + tab + "// TODO: implementar listagem\n" + tab + "res.json({ mensagem: 'GET " + nome + "' });\n});\n\n";
                if (chkPOST.Checked)
                    handlers += "router.post('/', async (req, res) => {\n" + tab + "const corpo = req.body;\n" + tab + "if (!validaCorpoRequisicao(res, corpo)) return;\n" + tab + "// TODO: implementar criação\n" + tab + "res.status(201).json({ mensagem: 'Criado com sucesso' });\n});\n\n";
                if (chkPUT.Checked)
                    handlers += "router.put('/:id', async (req, res) => {\n" + tab + "const corpo = req.body;\n" + tab + "if (!validaCorpoRequisicao(res, corpo)) return;\n" + tab + "// TODO: implementar atualização\n" + tab + "res.json({ mensagem: 'Atualizado com sucesso' });\n});\n\n";
                if (chkDELETE.Checked)
                    handlers += "router.delete('/:id', async (req, res) => {\n" + tab + "// TODO: implementar remoção\n" + tab + "res.json({ mensagem: 'Removido com sucesso' });\n});\n\n";

                conteudo =
"const express = require('express');\n" +
"const router = express.Router();\n\n" +
"// Equivalente ao validaCorpoRequisicao/validaChaves do kickphp\n" +
"function validaCorpoRequisicao(res, corpo, camposObrigatorios = []) {\n" +
tab + "if (!corpo || Object.keys(corpo).length === 0) {\n" +
tab + tab + "res.status(400).json({ mensagem: 'Dados inválidos!' });\n" +
tab + tab + "return false;\n" +
tab + "}\n" +
tab + "for (const campo of camposObrigatorios) {\n" +
tab + tab + "if (!(campo in corpo) || corpo[campo] === '') {\n" +
tab + tab + tab + "res.status(400).json({ mensagem: 'Dados incorretos. Verifique a documentação da API e tente novamente!' });\n" +
tab + tab + tab + "return false;\n" +
tab + tab + "}\n" +
tab + "}\n" +
tab + "return true;\n" +
"}\n\n" +
handlers +
"module.exports = router;\n";
            }

            Funcoes.CriarArquivo(Path.Combine(CaminhoProjeto, "src", "routes"), nome + "Routes", conteudo);
            Funcoes.RegistrarRotaNoServer(CaminhoProjeto, nome);

            txtNomeAPI.Clear();
            txtCampos.Clear();
            chkValidator.Checked = false;
            ProjetoAtualizado?.Invoke(this, EventArgs.Empty);
            MessageBox.Show("Rota criada com sucesso!" + (usaValidator ? "\nRode npm install para instalar express-validator antes de iniciar o backend." : ""), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao criar rota", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

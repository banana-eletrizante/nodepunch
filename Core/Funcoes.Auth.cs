using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace NodePunch.Core
{
    internal static partial class Funcoes
    {
        internal static string[] ArquivosAuth(string caminhoProjeto) => new[]
        {
            Path.Combine(caminhoProjeto, "src", "middleware", "auth.js"),
            Path.Combine(caminhoProjeto, "src", "controllers", "authController.js"),
            Path.Combine(caminhoProjeto, "src", "routes", "authRoutes.js"),
            Path.Combine(caminhoProjeto, "usuarios.sql")
        };

        public static bool GerarAuthJWT(string caminhoProjeto, TipoBanco tipo, bool sobrescrever = false)
        {
            if (tipo == TipoBanco.Nenhum || DetectarTipoBanco(caminhoProjeto) != tipo)
                throw new InvalidOperationException("Configure o banco de dados antes de gerar autenticação.");
            if (!sobrescrever && ArquivosAuth(caminhoProjeto).Any(File.Exists))
                throw new IOException("Já existem arquivos de autenticação. Confirme antes de sobrescrever.");
            ValidarRegistroRota(caminhoProjeto, "auth");
            if (tipo == TipoBanco.MySQL)
            {
                string bancoPath = Path.Combine(caminhoProjeto, "src", "base", "Banco.js");
                string banco = File.ReadAllText(bancoPath);
                if (!banco.Contains("static async consultarSql("))
                {
                    const string marcadorPool = "static #pool = null;";
                    if (!banco.Contains(marcadorPool) || !banco.Contains("static async #conectar("))
                        throw new InvalidOperationException("Banco.js foi personalizado. Adicione um método consultarSql(comando, parametros) para usar a autenticação.");
                    File.WriteAllText(bancoPath, banco.Replace(marcadorPool, marcadorPool + "\n\n" + ConsultaSqlMySQL));
                }
            }
            if (!AdicionarDependencia(caminhoProjeto, "jsonwebtoken", "^9.0.2") ||
                !AdicionarDependencia(caminhoProjeto, "bcryptjs", "^2.4.3"))
                throw new InvalidOperationException("Não foi possível atualizar as dependências. Corrija o package.json antes de gerar a autenticação.");
            string middleware =
"const jwt = require('jsonwebtoken');\nrequire('dotenv').config();\n\n" +
"function verificarToken(req, res, next) {\n" +
tab + "const authHeader = req.headers['authorization'];\n" +
tab + "const match = typeof authHeader === 'string' && /^Bearer\\s+(\\S+)$/i.exec(authHeader);\n" +
tab + "const token = match && match[1];\n" +
tab + "if (!token) return res.status(401).json({ mensagem: 'Token não fornecido.' });\n" +
tab + "jwt.verify(token, process.env.JWT_SECRET, (erro, usuario) => {\n" +
tab + tab + "if (erro) return res.status(403).json({ mensagem: 'Token inválido ou expirado.' });\n" +
tab + tab + "req.usuario = usuario;\n" +
tab + tab + "next();\n" +
tab + "});\n" +
"}\nmodule.exports = verificarToken;\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "middleware"), "auth", middleware);

            string controller = tipo == TipoBanco.Firebase ? ControllerFirebase() : ControllerSql(tipo);
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "controllers"), "authController", controller);

            string routes =
"const express = require('express');\n" +
"const AuthController = require('../controllers/authController');\n" +
"const router = express.Router();\n\n" +
"router.post('/registrar', (req, res) => AuthController.registrar(req, res));\n" +
"router.post('/login', (req, res) => AuthController.login(req, res));\n\n" +
"module.exports = router;\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "routes"), "authRoutes", routes);

            if (tipo != TipoBanco.Firebase)
            {
                string sql = tipo == TipoBanco.PostgreSQL
                    ? "CREATE TABLE IF NOT EXISTS usuarios (\n  id SERIAL PRIMARY KEY,\n  email VARCHAR(190) NOT NULL UNIQUE,\n  senha VARCHAR(255) NOT NULL\n);\n"
                    : "CREATE TABLE IF NOT EXISTS usuarios (\n  id INT AUTO_INCREMENT PRIMARY KEY,\n  email VARCHAR(190) NOT NULL UNIQUE,\n  senha VARCHAR(255) NOT NULL\n);\n";
                CriarArquivo(caminhoProjeto, "usuarios", sql, ".sql");
            }

            RegistrarRotaNoServer(caminhoProjeto, "auth");
            AdicionarVariavelEnv(caminhoProjeto, "JWT_SECRET", Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
            AdicionarVariavelEnv(caminhoProjeto, "JWT_EXPIRES_IN", "1h");
            string exemplo = Path.Combine(caminhoProjeto, ".env.example");
            string modelo = File.Exists(exemplo) ? File.ReadAllText(exemplo) : "";
            if (!Regex.IsMatch(modelo, @"(?m)^JWT_SECRET\s*="))
                File.AppendAllText(exemplo, (modelo.EndsWith("\n") || modelo.Length == 0 ? "" : "\n") + "JWT_SECRET=\nJWT_EXPIRES_IN=1h\n");
            return true;
        }

        internal static void ValidarRegistroRota(string caminhoProjeto, string nome)
        {
            if (!EhIdentificadorJS(nome)) throw new ArgumentException("Nome de rota inválido.");
            string caminhoServer = Path.Combine(caminhoProjeto, "server.js");
            if (!File.Exists(caminhoServer)) throw new FileNotFoundException("server.js não encontrado.");
            string conteudo = File.ReadAllText(caminhoServer);
            if (!RotaRegistrada(conteudo, nome) &&
                !Regex.IsMatch(conteudo, @"(?m)^\s*// Exemplo: app\.use\('/api/exemplo', require\('\./src/routes/exemploRoutes'\)\);\r?$"))
                throw new InvalidOperationException("O marcador de rotas foi removido do server.js. Restaure a linha '// Exemplo: app.use(...)' antes de gerar uma rota.");
        }

        private static bool RotaRegistrada(string conteudo, string nome) => Regex.IsMatch(conteudo,
            @"(?m)^[ \t]*app\.use\('/api/" + Regex.Escape(nome) + @"',\s*require\('\./src/routes/" + Regex.Escape(nome) + @"Routes'\)\);[ \t]*\r?$");

        public static void RegistrarRotaNoServer(string caminhoProjeto, string nome)
        {
            ValidarRegistroRota(caminhoProjeto, nome);
            string caminhoServer = Path.Combine(caminhoProjeto, "server.js");
            if (!File.Exists(caminhoServer)) return;
            string conteudo = File.ReadAllText(caminhoServer);
            string linhaRota = "app.use('/api/" + nome + "', require('./src/routes/" + nome + "Routes'));\n";
            if (RotaRegistrada(conteudo, nome)) return;
            string novaLinha = conteudo.Contains("\r\n") ? "\r\n" : "\n";
            var marcador = new Regex(@"(?m)^\s*// Exemplo: app\.use\('/api/exemplo', require\('\./src/routes/exemploRoutes'\)\);\r?$");
            File.WriteAllText(caminhoServer, marcador.Replace(conteudo, m => m.Value.TrimEnd('\r') + novaLinha + linhaRota.TrimEnd('\n'), 1));
        }

        private static string ControllerSql(TipoBanco tipo)
        {
            bool pg = tipo == TipoBanco.PostgreSQL;
            string insert = pg
                ? "INSERT INTO usuarios (email, senha) VALUES ($1, $2) RETURNING id, email"
                : "INSERT INTO usuarios (email, senha) VALUES (?, ?)";
            string select = pg
                ? "SELECT id, email, senha FROM usuarios WHERE email = $1 LIMIT 1"
                : "SELECT id, email, senha FROM usuarios WHERE email = ? LIMIT 1";
            string afterInsert = pg
                ? "const criado = linhas[0];\n" + tab + tab + "return res.status(201).json({ token: AuthController.token(criado), usuario: { id: criado.id, email: criado.email } });\n"
                : "const id = linhas.insertId || null;\n" + tab + tab + "return res.status(201).json({ token: AuthController.token({ id, email }), usuario: { id, email } });\n";

            string consulta = pg ? "consultar" : "consultarSql";
            return
"const bcrypt = require('bcryptjs');\n" +
"const jwt = require('jsonwebtoken');\n" +
"const Banco = require('../base/Banco');\n" +
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static token(usuario) {\n" +
tab + tab + "return jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: process.env.JWT_EXPIRES_IN || '1h' });\n" +
tab + "}\n\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (typeof email !== 'string' || email.length > 190 || !/^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$/.test(email) || typeof senha !== 'string' || senha.length < 8 || Buffer.byteLength(senha, 'utf8') > 72) return res.status(400).json({ mensagem: 'Informe e-mail válido e senha com no mínimo 8 caracteres e no máximo 72 bytes.' });\n" +
tab + tab + tab + "const hash = await bcrypt.hash(senha, 10);\n" +
tab + tab + tab + "const linhas = await Banco." + consulta + "(`" + insert + "`, [email, hash]);\n" +
tab + tab + afterInsert +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "if (String(erro.message || erro).includes('Duplicate') || String(erro.code) === '23505') {\n" +
tab + tab + tab + tab + "return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "}\n" +
tab + tab + tab + "return res.status(500).json({ mensagem: 'Erro ao registrar.' });\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "static async login(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (typeof email !== 'string' || !email || typeof senha !== 'string' || !senha || Buffer.byteLength(senha, 'utf8') > 72) return res.status(400).json({ mensagem: 'Informe email e senha válidos.' });\n" +
tab + tab + tab + "const linhas = await Banco." + consulta + "(`" + select + "`, [email]);\n" +
tab + tab + tab + "const usuario = Array.isArray(linhas) ? linhas[0] : linhas;\n" +
tab + tab + tab + "if (!usuario) return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "const ok = await bcrypt.compare(senha, usuario.senha);\n" +
tab + tab + tab + "if (!ok) return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "return res.json({ token: AuthController.token(usuario), usuario: { id: usuario.id, email: usuario.email } });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "return res.status(500).json({ mensagem: 'Erro ao autenticar.' });\n" +
tab + tab + "}\n" +
tab + "}\n" +
"}\nmodule.exports = AuthController;\n";
        }

        private static string ControllerFirebase()
        {
            return
"const bcrypt = require('bcryptjs');\n" +
"const jwt = require('jsonwebtoken');\n" +
"const Banco = require('../base/Banco');\n" +
"const { createHash } = require('crypto');\n" +
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static token(usuario) {\n" +
tab + tab + "return jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: process.env.JWT_EXPIRES_IN || '1h' });\n" +
tab + "}\n\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (typeof email !== 'string' || email.length > 190 || !/^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$/.test(email) || typeof senha !== 'string' || senha.length < 8 || Buffer.byteLength(senha, 'utf8') > 72) return res.status(400).json({ mensagem: 'Informe e-mail válido e senha com no mínimo 8 caracteres e no máximo 72 bytes.' });\n" +
tab + tab + tab + "const existentes = await Banco.consultar('usuarios', [['email', '==', email]]);\n" +
tab + tab + tab + "if (existentes.length) return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "const hash = await bcrypt.hash(senha, 10);\n" +
tab + tab + tab + "const ref = Banco.db.collection('usuarios').doc(createHash('sha256').update(email).digest('hex'));\n" +
tab + tab + tab + "await ref.create({ email, senha: hash });\n" +
tab + tab + tab + "const id = ref.id;\n" +
tab + tab + tab + "return res.status(201).json({ token: AuthController.token({ id, email }), usuario: { id, email } });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "if (erro.code === 6 || erro.code === 'already-exists') return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "return res.status(500).json({ mensagem: 'Erro ao registrar.' });\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "static async login(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (typeof email !== 'string' || !email || typeof senha !== 'string' || !senha || Buffer.byteLength(senha, 'utf8') > 72) return res.status(400).json({ mensagem: 'Informe email e senha válidos.' });\n" +
tab + tab + tab + "const encontrados = await Banco.consultar('usuarios', [['email', '==', email]]);\n" +
tab + tab + tab + "const usuario = encontrados[0];\n" +
tab + tab + tab + "if (!usuario) return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "const ok = await bcrypt.compare(senha, usuario.senha);\n" +
tab + tab + tab + "if (!ok) return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "return res.json({ token: AuthController.token(usuario), usuario: { id: usuario.id, email: usuario.email } });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "return res.status(500).json({ mensagem: 'Erro ao autenticar.' });\n" +
tab + tab + "}\n" +
tab + "}\n" +
"}\nmodule.exports = AuthController;\n";
        }
    }
}

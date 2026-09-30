using System;
using System.IO;

namespace NodePunch.Core
{
    internal static partial class Funcoes
    {
        public static bool GerarAuthJWT(string caminhoProjeto, TipoBanco tipo)
        {
            string middleware =
"const jwt = require('jsonwebtoken');\nrequire('dotenv').config();\n\n" +
"function verificarToken(req, res, next) {\n" +
tab + "const authHeader = req.headers['authorization'];\n" +
tab + "const token = authHeader && authHeader.split(' ')[1];\n" +
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
            bool jwt = AdicionarDependencia(caminhoProjeto, "jsonwebtoken", "^9.0.2");
            bool bcrypt = AdicionarDependencia(caminhoProjeto, "bcryptjs", "^2.4.3");
            AdicionarVariavelEnv(caminhoProjeto, "JWT_SECRET", Guid.NewGuid().ToString("N"));
            return jwt && bcrypt;
        }

        public static void RegistrarRotaNoServer(string caminhoProjeto, string nome)
        {
            string caminhoServer = Path.Combine(caminhoProjeto, "server.js");
            if (!File.Exists(caminhoServer)) return;
            string conteudo = File.ReadAllText(caminhoServer);
            string linhaRota = "app.use('/api/" + nome + "', require('./src/routes/" + nome + "Routes'));\n";
            string marcador = "// Exemplo: app.use('/api/exemplo', require('./src/routes/exemploRoutes'));\n";
            if (conteudo.Contains(linhaRota)) return;
            if (conteudo.Contains(marcador))
                File.WriteAllText(caminhoServer, conteudo.Replace(marcador, marcador + linhaRota));
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

            return
"const bcrypt = require('bcryptjs');\n" +
"const jwt = require('jsonwebtoken');\n" +
"const Banco = require('../base/Banco');\n" +
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static token(usuario) {\n" +
tab + tab + "return jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: '7d' });\n" +
tab + "}\n\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (!email || !senha || senha.length < 6) return res.status(400).json({ mensagem: 'Informe email e senha (mín. 6).' });\n" +
tab + tab + tab + "const hash = await bcrypt.hash(senha, 10);\n" +
tab + tab + tab + "const linhas = await Banco.consultar(`" + insert + "`, [email, hash]);\n" +
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
tab + tab + tab + "if (!email || !senha) return res.status(400).json({ mensagem: 'Informe email e senha.' });\n" +
tab + tab + tab + "const linhas = await Banco.consultar(`" + select + "`, [email]);\n" +
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
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static token(usuario) {\n" +
tab + tab + "return jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: '7d' });\n" +
tab + "}\n\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (!email || !senha || senha.length < 6) return res.status(400).json({ mensagem: 'Informe email e senha (mín. 6).' });\n" +
tab + tab + tab + "const existentes = await Banco.consultar('usuarios', [['email', '==', email]]);\n" +
tab + tab + tab + "if (existentes.length) return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "const hash = await bcrypt.hash(senha, 10);\n" +
tab + tab + tab + "const id = await Banco.executar('usuarios', { email, senha: hash });\n" +
tab + tab + tab + "return res.status(201).json({ token: AuthController.token({ id, email }), usuario: { id, email } });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "return res.status(500).json({ mensagem: 'Erro ao registrar.' });\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "static async login(req, res) {\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const { email, senha } = req.body || {};\n" +
tab + tab + tab + "if (!email || !senha) return res.status(400).json({ mensagem: 'Informe email e senha.' });\n" +
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

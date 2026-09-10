using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NodePunch.Core
{
    internal static class Funcoes
    {
        private const string tab = "\t";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static void CriarPasta(string caminho)
        {
            if (!Directory.Exists(caminho))
                Directory.CreateDirectory(caminho);
        }

        public static void CriarArquivo(string caminho, string nome, string conteudo, string extensao = ".js")
        {
            CriarPasta(caminho);
            try
            {
                File.WriteAllText(
                    Path.Combine(caminho, nome + extensao),
                    conteudo,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
            catch (Exception ex)
            {
                throw new IOException(
                    "Não foi possível criar o arquivo: " + Path.Combine(caminho, nome + extensao),
                    ex);
            }
        }

        // Ponto único de entrada: decide qual Banco.js gerar conforme o tipo escolhido
        public static void CriarClasseBaseBD(ConexaoBanco dados, string caminhoBase)
        {
            string conteudo = dados.Tipo switch
            {
                TipoBanco.MySQL => GerarBancoMySQL(dados),
                TipoBanco.PostgreSQL => GerarBancoPostgreSQL(dados),
                TipoBanco.Firebase => GerarBancoFirebase(dados),
                _ => throw new Exception("Tipo de banco não suportado.")
            };

            CriarArquivo(caminhoBase, "Banco", conteudo);
        }

        private static string GerarBancoMySQL(ConexaoBanco dados)
        {
            string consultarExecutar;

            if (dados.ComSP)
            {
                consultarExecutar =
tab + "static async consultar(comando, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "const [linhas] = await Banco.#pool.execute(comando, parametros);\n" +
tab + tab + "return linhas;\n" +
tab + "}\n\n" +
tab + "static async executar(comando, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "await Banco.#pool.execute(comando, parametros);\n" +
tab + "}\n\n" +
tab + "// Chamadas específicas para stored procedures\n" +
tab + "static async consultarProcedure(nomeProcedure, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "if (!/^[A-Za-z0-9_]+$/.test(nomeProcedure)) throw new Error('Nome de procedure inválido.');\n" +
tab + tab + "const placeholders = parametros.map(() => '?').join(', ');\n" +
tab + tab + "const sql = placeholders ? `CALL ${nomeProcedure}(${placeholders})` : `CALL ${nomeProcedure}`;\n" +
tab + tab + "const [linhas] = await Banco.#pool.query(sql, parametros);\n" +
tab + tab + "return linhas[0] || [];\n" +
tab + "}\n\n" +
tab + "static async executarProcedure(nomeProcedure, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "if (!/^[A-Za-z0-9_]+$/.test(nomeProcedure)) throw new Error('Nome de procedure inválido.');\n" +
tab + tab + "const placeholders = parametros.map(() => '?').join(', ');\n" +
tab + tab + "const sql = placeholders ? `CALL ${nomeProcedure}(${placeholders})` : `CALL ${nomeProcedure}`;\n" +
tab + tab + "await Banco.#pool.query(sql, parametros);\n" +
tab + "}\n";
            }
            else
            {
                consultarExecutar =
tab + "static async consultar(comando, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "const [linhas] = await Banco.#pool.execute(comando, parametros);\n" +
tab + tab + "return linhas;\n" +
tab + "}\n\n" +
tab + "static async executar(comando, parametros = []) {\n" +
tab + tab + "await Banco.#conectar();\n" +
tab + tab + "await Banco.#pool.execute(comando, parametros);\n" +
tab + "}\n";
            }

            return
"const mysql = require('mysql2/promise');\n" +
"require('dotenv').config();\n\n" +
"class Banco {\n" +
tab + "static #pool = null;\n\n" +
tab + "static async #conectar() {\n" +
tab + tab + "if (Banco.#pool !== null) return;\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "Banco.#pool = mysql.createPool({\n" +
tab + tab + tab + tab + "host: process.env.DB_HOST,\n" +
tab + tab + tab + tab + "port: process.env.DB_PORT || " + (string.IsNullOrWhiteSpace(dados.Porta) ? "3306" : dados.Porta) + ",\n" +
tab + tab + tab + tab + "user: process.env.DB_USER,\n" +
tab + tab + tab + tab + "password: process.env.DB_PASSWORD,\n" +
tab + tab + tab + tab + "database: process.env.DB_NAME,\n" +
tab + tab + tab + tab + "charset: 'utf8mb4',\n" +
tab + tab + tab + tab + "waitForConnections: true,\n" +
tab + tab + tab + tab + "connectionLimit: 10\n" +
tab + tab + tab + "});\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "throw new Error('Erro ao conectar ao Servidor MySQL.');\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
consultarExecutar +
"}\n\n" +
"module.exports = Banco;\n";
        }

        private static string GerarBancoPostgreSQL(ConexaoBanco dados)
        {
            return
"const { Pool } = require('pg');\n" +
"require('dotenv').config();\n\n" +
"class Banco {\n" +
tab + "static #pool = null;\n\n" +
tab + "static #conectar() {\n" +
tab + tab + "if (Banco.#pool !== null) return;\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "Banco.#pool = new Pool({\n" +
tab + tab + tab + tab + "host: process.env.DB_HOST,\n" +
tab + tab + tab + tab + "port: process.env.DB_PORT || " + (string.IsNullOrWhiteSpace(dados.Porta) ? "5432" : dados.Porta) + ",\n" +
tab + tab + tab + tab + "user: process.env.DB_USER,\n" +
tab + tab + tab + tab + "password: process.env.DB_PASSWORD,\n" +
tab + tab + tab + tab + "database: process.env.DB_NAME,\n" +
tab + tab + tab + tab + "max: 10\n" +
tab + tab + tab + "});\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "throw new Error('Erro ao conectar ao Servidor PostgreSQL.');\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "// comando usa placeholders posicionais do Postgres: $1, $2, ...\n" +
tab + "static async consultar(comando, parametros = []) {\n" +
tab + tab + "Banco.#conectar();\n" +
tab + tab + "const resultado = await Banco.#pool.query(comando, parametros);\n" +
tab + tab + "return resultado.rows;\n" +
tab + "}\n\n" +
tab + "static async executar(comando, parametros = []) {\n" +
tab + tab + "Banco.#conectar();\n" +
tab + tab + "await Banco.#pool.query(comando, parametros);\n" +
tab + "}\n" +
"}\n\n" +
"module.exports = Banco;\n";
        }

        private static string GerarBancoFirebase(ConexaoBanco dados)
        {
            return
"const admin = require('firebase-admin');\n" +
"const path = require('path');\n" +
"require('dotenv').config();\n\n" +
"class Banco {\n" +
tab + "static #app = null;\n" +
tab + "static #db = null;\n\n" +
tab + "static #inicializar() {\n" +
tab + tab + "if (Banco.#app !== null) return;\n\n" +
tab + tab + "const caminhoCredenciais = path.resolve(process.env.FIREBASE_SERVICE_ACCOUNT);\n" +
tab + tab + "const serviceAccount = require(caminhoCredenciais);\n\n" +
tab + tab + "Banco.#app = admin.initializeApp({\n" +
tab + tab + tab + "credential: admin.credential.cert(serviceAccount),\n" +
tab + tab + tab + "projectId: process.env.FIREBASE_PROJECT_ID\n" +
tab + tab + "});\n" +
tab + tab + "Banco.#db = admin.firestore();\n" +
tab + "}\n\n" +
tab + "static get db() {\n" +
tab + tab + "Banco.#inicializar();\n" +
tab + tab + "return Banco.#db;\n" +
tab + "}\n\n" +
tab + "// filtros: [['campo', '==', valor], ...]\n" +
tab + "static async consultar(colecao, filtros = []) {\n" +
tab + tab + "Banco.#inicializar();\n" +
tab + tab + "let ref = Banco.#db.collection(colecao);\n" +
tab + tab + "filtros.forEach(([campo, operador, valor]) => { ref = ref.where(campo, operador, valor); });\n" +
tab + tab + "const snapshot = await ref.get();\n" +
tab + tab + "return snapshot.docs.map(doc => ({ id: doc.id, ...doc.data() }));\n" +
tab + "}\n\n" +
tab + "// Cria (sem id) ou atualiza (com id) um documento\n" +
tab + "static async executar(colecao, dados, id = null) {\n" +
tab + tab + "Banco.#inicializar();\n" +
tab + tab + "const ref = Banco.#db.collection(colecao);\n" +
tab + tab + "if (id) {\n" +
tab + tab + tab + "await ref.doc(id).set(dados, { merge: true });\n" +
tab + tab + tab + "return id;\n" +
tab + tab + "}\n" +
tab + tab + "const docRef = await ref.add(dados);\n" +
tab + tab + "return docRef.id;\n" +
tab + "}\n" +
"}\n\n" +
"module.exports = Banco;\n";
        }

        // Gera o package.json do projeto
        public static void CriarPackageJson(string caminhoProjeto, string nomeProjeto, TipoBanco tipoBanco)
        {
            JsonObject dependencias = new JsonObject
            {
                ["cors"] = "^2.8.6",
                ["dotenv"] = "^17.4.2",
                ["express"] = "^5.2.1",
                ["express-rate-limit"] = "^8.7.0",
                ["helmet"] = "^8.3.0"
            };

            switch (tipoBanco)
            {
                case TipoBanco.MySQL:
                    dependencias["mysql2"] = "^3.24.4";
                    break;
                case TipoBanco.PostgreSQL:
                    dependencias["pg"] = "^8.23.0";
                    break;
                case TipoBanco.Firebase:
                    dependencias["firebase-admin"] = "^14.3.0";
                    break;
            }

            JsonObject package = new JsonObject
            {
                ["name"] = nomeProjeto.ToLowerInvariant(),
                ["version"] = "1.0.0",
                ["private"] = true,
                ["description"] = "Backend Express gerado pelo NodePunch",
                ["main"] = "server.js",
                ["engines"] = new JsonObject { ["node"] = ">=20" },
                ["scripts"] = new JsonObject
                {
                    ["start"] = "node server.js",
                    ["dev"] = "nodemon server.js",
                    ["check"] = "node --check server.js"
                },
                ["dependencies"] = dependencias,
                ["devDependencies"] = new JsonObject { ["nodemon"] = "^3.1.14" }
            };

            string conteudo = package.ToJsonString(JsonOptions) + Environment.NewLine;
            CriarArquivo(caminhoProjeto, "package", conteudo, ".json");
        }

        // Gera o .env conforme o tipo de banco
        public static void CriarEnv(string caminhoProjeto, ConexaoBanco dados)
        {
            string conteudo = "NODE_ENV=development\n" +
                              "PORT=3000\n" +
                              "API_RATE_LIMIT_MAX=100\n";

            switch (dados.Tipo)
            {
                case TipoBanco.MySQL:
                    conteudo += "DB_HOST=" + dados.Server + "\n" +
                                "DB_PORT=" + (string.IsNullOrWhiteSpace(dados.Porta) ? "3306" : dados.Porta) + "\n" +
                                "DB_USER=" + dados.User + "\n" +
                                "DB_PASSWORD=" + dados.Password + "\n" +
                                "DB_NAME=" + dados.Schema + "\n";
                    break;
                case TipoBanco.PostgreSQL:
                    conteudo += "DB_HOST=" + dados.Server + "\n" +
                                "DB_PORT=" + (string.IsNullOrWhiteSpace(dados.Porta) ? "5432" : dados.Porta) + "\n" +
                                "DB_USER=" + dados.User + "\n" +
                                "DB_PASSWORD=" + dados.Password + "\n" +
                                "DB_NAME=" + dados.Schema + "\n";
                    break;
                case TipoBanco.Firebase:
                    conteudo += "FIREBASE_PROJECT_ID=" + dados.FirebaseProjectId + "\n" +
                                "FIREBASE_SERVICE_ACCOUNT=" + dados.FirebaseServiceAccountPath + "\n";
                    break;
            }

            CriarArquivo(caminhoProjeto, ".env", conteudo, "");
        }

        public static void CriarEnvExemplo(string caminhoProjeto, TipoBanco tipo)
        {
            string conteudo = "NODE_ENV=development\n" +
                              "PORT=3000\n" +
                              "API_RATE_LIMIT_MAX=100\n";

            if (tipo == TipoBanco.MySQL || tipo == TipoBanco.PostgreSQL)
            {
                conteudo += "DB_HOST=localhost\n" +
                            "DB_PORT=" + (tipo == TipoBanco.MySQL ? "3306" : "5432") + "\n" +
                            "DB_USER=\n" +
                            "DB_PASSWORD=\n" +
                            "DB_NAME=\n";
            }
            else if (tipo == TipoBanco.Firebase)
            {
                conteudo += "FIREBASE_PROJECT_ID=\n" +
                            "FIREBASE_SERVICE_ACCOUNT=./firebaseServiceAccountKey.json\n";
            }

            conteudo += "JWT_SECRET=\n";
            CriarArquivo(caminhoProjeto, ".env.example", conteudo, "");
        }

        // Gera o server.js (ponto de entrada Express)
        public static void CriarServerJs(string caminhoProjeto)
        {
            string conteudo =
"require('dotenv').config();\n" +
"const express = require('express');\n" +
"const cors = require('cors');\n" +
"const helmet = require('helmet');\n" +
"const { rateLimit } = require('express-rate-limit');\n" +
"const corsOptions = require('./src/config/cors');\n\n" +
"const app = express();\n\n" +
"app.disable('x-powered-by');\n" +
"app.use(helmet());\n" +
"app.use(express.json({ limit: '1mb' }));\n" +
"app.use(cors(corsOptions));\n\n" +
"app.use('/api', rateLimit({\n" +
tab + "windowMs: 15 * 60 * 1000,\n" +
tab + "limit: Number(process.env.API_RATE_LIMIT_MAX) || 100,\n" +
tab + "standardHeaders: 'draft-8',\n" +
tab + "legacyHeaders: false\n" +
"}));\n\n" +
"app.get('/health', (req, res) => {\n" +
tab + "res.json({ status: 'ok', timestamp: new Date().toISOString() });\n" +
"});\n\n" +
"// Rotas serão registradas aqui pelo nodepunch conforme você criar novas APIs\n" +
"// Exemplo: app.use('/api/exemplo', require('./src/routes/exemploRoutes'));\n\n" +
"app.use((req, res) => {\n" +
tab + "res.status(404).json({ mensagem: 'Rota não encontrada.' });\n" +
"});\n\n" +
"app.use((erro, req, res, next) => {\n" +
tab + "console.error(erro);\n" +
tab + "res.status(500).json({ mensagem: 'Erro interno do servidor.' });\n" +
"});\n\n" +
"const PORT = process.env.PORT || 3000;\n" +
"app.listen(PORT, () => {\n" +
tab + "console.log(`Servidor rodando em http://localhost:${PORT}`);\n" +
"});\n";
            CriarArquivo(caminhoProjeto, "server", conteudo);

            string corsConteudo =
"// Configuração de CORS\n" +
"const CORS_ORIGINS = {\n" +
tab + "development: ['http://localhost:5173'],\n" +
tab + "production: []\n" +
"};\n\n" +
"const ambiente = process.env.NODE_ENV === 'production' ? 'production' : 'development';\n\n" +
"module.exports = {\n" +
tab + "origin: CORS_ORIGINS[ambiente],\n" +
tab + "credentials: true\n" +
"};\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "config"), "cors", corsConteudo);
        }

        // Gera README.md do projeto
        public static void CriarReadme(string caminhoProjeto, string nomeProjeto, TipoBanco tipo)
        {
            string dbInfo = tipo switch
            {
                TipoBanco.MySQL => "- Banco de dados: **MySQL** (configurado em `.env`)\n",
                TipoBanco.PostgreSQL => "- Banco de dados: **PostgreSQL** (configurado em `.env`)\n",
                TipoBanco.Firebase => "- Banco de dados: **Firebase Firestore** (configurado em `.env` + `serviceAccountKey.json`)\n",
                _ => ""
            };

            string conteudo =
"# " + nomeProjeto + "\n\n" +
"Backend Node.js com Express 5, gerado pelo [NodePunch](https://github.com/banana-eletrizante/nodepunch).\n\n" +
"## Requisitos\n\n" +
"- Node.js 20 ou superior\n" +
"- npm\n\n" +
"## Primeiros passos\n\n" +
"```bash\n" +
"npm install\n" +
"npm run dev   # com hot-reload (nodemon)\n" +
"# ou\n" +
"npm run start\n" +
"```\n\n" +
"Verifique o serviço em `GET http://localhost:3000/health`.\n\n" +
"Revise o `.env` gerado antes de iniciar. O `.env.example` pode ser versionado como referência, mas não contém segredos.\n\n" +
"## Estrutura\n\n" +
"```\n" +
"src/\n" +
tab + "base/         # Classe Banco (persistência)\n" +
tab + "models/       # Classes de modelo (ES6)\n" +
tab + "controllers/  # Controllers\n" +
tab + "routes/       # Rotas Express\n" +
tab + "config/       # Configurações (CORS, etc.)\n" +
"server.js       # Ponto de entrada\n" +
"```\n\n" +
"## Configuração\n\n" +
dbInfo +
"- Variáveis de ambiente em `.env` (não versionado)\n" +
"- Modelo seguro das variáveis em `.env.example`\n" +
"- Origens permitidas em `src/config/cors.js`\n" +
"- Limite de requisições em `API_RATE_LIMIT_MAX`\n\n" +
"## Segurança incluída\n\n" +
"O servidor utiliza Helmet, limite de corpo JSON, rate limiting, CORS configurável e respostas genéricas para erros internos.\n\n" +
"## Autenticação\n\n" +
"Se você gerou autenticação JWT pelo nodepunch, veja as rotas em `src/routes/authRoutes.js` " +
"(`POST /api/auth/registrar` e `POST /api/auth/login`). Para bancos SQL, crie uma tabela `usuarios` " +
"com `id`, `email` único e `senha`. Nunca versione `.env` nem credenciais do Firebase.\n";

            CriarArquivo(caminhoProjeto, "README", conteudo, ".md");
        }

        // Gera .gitignore do projeto
        public static void CriarGitignore(string caminhoProjeto)
        {
            string conteudo =
"node_modules/\n" +
".env\n" +
"firebaseServiceAccountKey.json\n" +
"*.log\n" +
"npm-debug.log*\n" +
".DS_Store\n" +
"dist/\n" +
"build/\n";
            CriarArquivo(caminhoProjeto, ".gitignore", conteudo, "");
        }

        // Detecta o tipo de banco de um projeto existente, lendo o Banco.js gerado
        internal static TipoBanco DetectarTipoBanco(string caminhoProjeto)
        {
            string caminhoBanco = Path.Combine(caminhoProjeto, "src", "base", "Banco.js");
            if (!File.Exists(caminhoBanco)) return TipoBanco.Nenhum;

            string conteudo = File.ReadAllText(caminhoBanco);
            if (conteudo.Contains("firebase-admin")) return TipoBanco.Firebase;
            if (conteudo.Contains("require('pg')")) return TipoBanco.PostgreSQL;
            if (conteudo.Contains("mysql2")) return TipoBanco.MySQL;
            return TipoBanco.Nenhum;
        }

        // Adiciona uma dependência ao package.json existente, se ainda não estiver lá (edição simples baseada em texto)
        // Retorna true se adicionou ou já existia; false se não conseguiu localizar onde inserir
        public static bool AdicionarDependencia(string caminhoProjeto, string nomePacote, string versao, bool devDependency = false)
        {
            string caminhoPackage = Path.Combine(caminhoProjeto, "package.json");
            if (!File.Exists(caminhoPackage)) return false;

            try
            {
                JsonObject package = JsonNode.Parse(File.ReadAllText(caminhoPackage))?.AsObject();
                if (package == null) return false;

                string nomeSecao = devDependency ? "devDependencies" : "dependencies";
                JsonObject secao = package[nomeSecao] as JsonObject ?? new JsonObject();
                package[nomeSecao] = secao;
                secao[nomePacote] = versao;

                File.WriteAllText(
                    caminhoPackage,
                    package.ToJsonString(JsonOptions) + Environment.NewLine,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        // Adiciona uma variável ao .env existente, se ainda não estiver lá
        public static void AdicionarVariavelEnv(string caminhoProjeto, string chave, string valor)
        {
            string caminhoEnv = Path.Combine(caminhoProjeto, ".env");
            string conteudo = File.Exists(caminhoEnv) ? File.ReadAllText(caminhoEnv) : "";
            bool existe = conteudo.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Any(linha => linha.TrimStart().StartsWith(chave + "=", StringComparison.Ordinal));
            if (existe) return;

            conteudo += (conteudo.EndsWith("\n") || conteudo == "" ? "" : "\n") + chave + "=" + valor + "\n";
            File.WriteAllText(caminhoEnv, conteudo);
        }

        // Gera autenticação JWT (registrar/login) conforme o tipo de banco do projeto
        // Retorna true se tudo (incluindo dependências no package.json) foi aplicado com sucesso
        public static bool GerarAuthJWT(string caminhoProjeto, TipoBanco tipo)
        {
            if (tipo == TipoBanco.Nenhum)
                throw new InvalidOperationException("Configure um banco de dados antes de gerar autenticação.");

            string middleware =
"const jwt = require('jsonwebtoken');\n" +
"require('dotenv').config();\n\n" +
"function verificarToken(req, res, next) {\n" +
tab + "const authHeader = req.headers['authorization'];\n" +
tab + "const token = authHeader && authHeader.split(' ')[1];\n\n" +
tab + "if (!token) {\n" +
tab + tab + "return res.status(401).json({ mensagem: 'Token não fornecido.' });\n" +
tab + "}\n\n" +
tab + "jwt.verify(token, process.env.JWT_SECRET, (erro, usuario) => {\n" +
tab + tab + "if (erro) {\n" +
tab + tab + tab + "return res.status(403).json({ mensagem: 'Token inválido ou expirado.' });\n" +
tab + tab + "}\n" +
tab + tab + "req.usuario = usuario;\n" +
tab + tab + "next();\n" +
tab + "});\n" +
"}\n\n" +
"module.exports = verificarToken;\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "middleware"), "auth", middleware);

            string authController;
            if (tipo == TipoBanco.Firebase)
            {
                authController =
"const bcrypt = require('bcryptjs');\n" +
"const jwt = require('jsonwebtoken');\n" +
"const Banco = require('../base/Banco');\n" +
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "const { email, senha } = req.body;\n" +
tab + tab + "const emailNormalizado = String(email || '').trim().toLowerCase();\n" +
tab + tab + "if (!emailNormalizado || typeof senha !== 'string' || senha.length < 8) {\n" +
tab + tab + tab + "return res.status(400).json({ mensagem: 'Informe um e-mail e uma senha com pelo menos 8 caracteres.' });\n" +
tab + tab + "}\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const existentes = await Banco.consultar('usuarios', [['email', '==', emailNormalizado]]);\n" +
tab + tab + tab + "if (existentes.length > 0) return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "const senhaCriptografada = await bcrypt.hash(senha, 12);\n" +
tab + tab + tab + "await Banco.executar('usuarios', { email: emailNormalizado, senha: senhaCriptografada });\n" +
tab + tab + tab + "res.status(201).json({ mensagem: 'Usuário registrado com sucesso.' });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "res.status(500).json({ mensagem: 'Erro ao registrar usuário.' });\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "static async login(req, res) {\n" +
tab + tab + "const { email, senha } = req.body;\n" +
tab + tab + "const emailNormalizado = String(email || '').trim().toLowerCase();\n" +
tab + tab + "if (!emailNormalizado || typeof senha !== 'string') return res.status(400).json({ mensagem: 'E-mail e senha são obrigatórios.' });\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const usuarios = await Banco.consultar('usuarios', [['email', '==', emailNormalizado]]);\n" +
tab + tab + tab + "const usuario = usuarios[0];\n" +
tab + tab + tab + "if (!usuario || !(await bcrypt.compare(senha, usuario.senha))) {\n" +
tab + tab + tab + tab + "return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "}\n" +
tab + tab + tab + "const token = jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: '8h' });\n" +
tab + tab + tab + "res.json({ token });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "res.status(500).json({ mensagem: 'Erro ao autenticar usuário.' });\n" +
tab + tab + "}\n" +
tab + "}\n" +
"}\n\n" +
"module.exports = AuthController;\n";
            }
            else
            {
                string placeholder1 = tipo == TipoBanco.PostgreSQL ? "$1" : "?";
                string placeholder2 = tipo == TipoBanco.PostgreSQL ? "$2" : "?";
                string sqlInsert = "INSERT INTO usuarios (email, senha) VALUES (" + placeholder1 + ", " + placeholder2 + ")";
                string sqlSelect = "SELECT * FROM usuarios WHERE email = " + placeholder1;

                authController =
"const bcrypt = require('bcryptjs');\n" +
"const jwt = require('jsonwebtoken');\n" +
"const Banco = require('../base/Banco');\n" +
"require('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static async registrar(req, res) {\n" +
tab + tab + "const { email, senha } = req.body;\n" +
tab + tab + "const emailNormalizado = String(email || '').trim().toLowerCase();\n" +
tab + tab + "if (!emailNormalizado || typeof senha !== 'string' || senha.length < 8) {\n" +
tab + tab + tab + "return res.status(400).json({ mensagem: 'Informe um e-mail e uma senha com pelo menos 8 caracteres.' });\n" +
tab + tab + "}\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const senhaCriptografada = await bcrypt.hash(senha, 12);\n" +
tab + tab + tab + "await Banco.executar('" + sqlInsert + "', [emailNormalizado, senhaCriptografada]);\n" +
tab + tab + tab + "res.status(201).json({ mensagem: 'Usuário registrado com sucesso.' });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "if (erro.code === 'ER_DUP_ENTRY' || erro.code === '23505') return res.status(409).json({ mensagem: 'E-mail já cadastrado.' });\n" +
tab + tab + tab + "res.status(500).json({ mensagem: 'Erro ao registrar usuário.' });\n" +
tab + tab + "}\n" +
tab + "}\n\n" +
tab + "static async login(req, res) {\n" +
tab + tab + "const { email, senha } = req.body;\n" +
tab + tab + "const emailNormalizado = String(email || '').trim().toLowerCase();\n" +
tab + tab + "if (!emailNormalizado || typeof senha !== 'string') return res.status(400).json({ mensagem: 'E-mail e senha são obrigatórios.' });\n\n" +
tab + tab + "try {\n" +
tab + tab + tab + "const usuarios = await Banco.consultar('" + sqlSelect + "', [emailNormalizado]);\n" +
tab + tab + tab + "const usuario = usuarios[0];\n" +
tab + tab + tab + "if (!usuario || !(await bcrypt.compare(senha, usuario.senha))) {\n" +
tab + tab + tab + tab + "return res.status(401).json({ mensagem: 'Credenciais inválidas.' });\n" +
tab + tab + tab + "}\n" +
tab + tab + tab + "const token = jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: '8h' });\n" +
tab + tab + tab + "res.json({ token });\n" +
tab + tab + "} catch (erro) {\n" +
tab + tab + tab + "res.status(500).json({ mensagem: 'Erro ao autenticar usuário.' });\n" +
tab + tab + "}\n" +
tab + "}\n" +
"}\n\n" +
"module.exports = AuthController;\n";
            }
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "controllers"), "authController", authController);

            string authRoutes =
"const express = require('express');\n" +
"const router = express.Router();\n" +
"const AuthController = require('../controllers/authController');\n\n" +
"router.post('/registrar', AuthController.registrar);\n" +
"router.post('/login', AuthController.login);\n\n" +
"module.exports = router;\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "routes"), "authRoutes", authRoutes);

            bool depsOk = AdicionarDependencia(caminhoProjeto, "jsonwebtoken", "^9.0.3");
            depsOk &= AdicionarDependencia(caminhoProjeto, "bcryptjs", "^3.0.3");
            AdicionarVariavelEnv(caminhoProjeto, "JWT_SECRET", Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

            string caminhoServer = Path.Combine(caminhoProjeto, "server.js");
            if (File.Exists(caminhoServer))
            {
                string conteudoServer = File.ReadAllText(caminhoServer);
                string linhaRota = "app.use('/api/auth', require('./src/routes/authRoutes'));\n";
                string marcador = "// Exemplo: app.use('/api/exemplo', require('./src/routes/exemploRoutes'));\n";
                if (conteudoServer.Contains(marcador) && !conteudoServer.Contains(linhaRota))
                {
                    conteudoServer = conteudoServer.Replace(marcador, marcador + linhaRota);
                    File.WriteAllText(caminhoServer, conteudoServer);
                }
            }

            return depsOk;
        }

        // Escapa aspas simples e barras invertidas para uso seguro dentro de strings JS de aspas simples
        public static string EscaparJS(string valor)
        {
            if (valor == null) return "";
            return valor.Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private static readonly HashSet<string> PalavrasReservadasJS = new HashSet<string>
        {
            "break","case","catch","class","const","continue","debugger","default","delete","do",
            "else","export","extends","finally","for","function","if","import","in","instanceof",
            "new","return","super","switch","this","throw","try","typeof","var","void","while",
            "with","yield","let","static","await","async","enum","implements","interface","package",
            "private","protected","public","null","true","false","undefined"
        };

        public static bool EhPalavraReservadaJS(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && PalavrasReservadasJS.Contains(valor.Trim().ToLower());
        }

        public static bool EhIdentificadorJSValido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) &&
                   Regex.IsMatch(valor, "^[A-Za-z_$][A-Za-z0-9_$]*$");
        }

        public static string PrimeiraMaiusculaSemAcento(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            string t = RemoverAcentos(texto.Trim());
            return char.ToUpper(t[0]) + t.Substring(1);
        }

        public static string MinusculaSemAcento(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            return RemoverAcentos(texto.Trim()).ToLower();
        }

        private static string RemoverAcentos(string texto)
        {
            return new string(texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray()).Normalize(NormalizationForm.FormC);
        }

        internal static bool VerificaTipoProjeto(string caminhoProjeto)
        {
            return Directory.Exists(Path.Combine(caminhoProjeto, "src"));
        }

        internal static bool VerificaUsaBanco(string caminhoProjeto)
        {
            return File.Exists(Path.Combine(caminhoProjeto, "src", "base", "Banco.js"));
        }

        public static Dictionary<string, List<string>> ExtractCorsOrigins(string corsFilePath)
        {
            var dic = new Dictionary<string, List<string>>
            {
                { "development", new List<string>() },
                { "production", new List<string>() }
            };
            string input = File.ReadAllText(corsFilePath);
            Regex regex = new Regex(@"(development|production):\s*\[(.*?)\]", RegexOptions.Singleline);
            foreach (Match m in regex.Matches(input))
            {
                string chave = m.Groups[1].Value;
                Regex valores = new Regex("'([^']+)'");
                foreach (Match v in valores.Matches(m.Groups[2].Value))
                    dic[chave].Add(v.Groups[1].Value);
            }
            return dic;
        }

        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string semAcento = RemoverAcentos(input);
            string limpo = Regex.Replace(semAcento, "[^a-zA-Z0-9_\\s]", "").Trim();

            if (Regex.IsMatch(limpo, "^[a-z0-9]+(_[a-z0-9]+)*$"))
                return limpo;

            limpo = limpo.Replace("_", " ");
            string[] partes = limpo.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder(partes[0].ToLower());
            for (int i = 1; i < partes.Length; i++)
            {
                string p = partes[i].ToLower();
                sb.Append(char.ToUpper(p[0]) + p.Substring(1));
            }
            return sb.ToString();
        }
    }
}

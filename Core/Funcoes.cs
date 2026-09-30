using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NodePunch.Core
{
    internal static partial class Funcoes
    {
        private const string tab = "\t";

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
                using StreamWriter sw = new StreamWriter(Path.Combine(caminho, nome + extensao));
                sw.Write(conteudo);
            }
            catch
            {
                throw new Exception("Não foi possível criar o arquivo: " + Path.Combine(caminho, nome + extensao));
            }
        }

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

        public static void CriarPackageJson(string caminhoProjeto, string nomeProjeto, TipoBanco tipoBanco)
        {
            string deps = "\"express\": \"^4.21.2\",\n" + tab + tab + "\"cors\": \"^2.8.5\",\n" + tab + tab + "\"dotenv\": \"^16.4.7\",\n" + tab + tab + "\"helmet\": \"^8.0.0\",\n" + tab + tab + "\"express-rate-limit\": \"^7.5.0\"";
            switch (tipoBanco)
            {
                case TipoBanco.MySQL:
                    deps += ",\n" + tab + tab + "\"mysql2\": \"^3.12.0\"";
                    break;
                case TipoBanco.PostgreSQL:
                    deps += ",\n" + tab + tab + "\"pg\": \"^8.13.1\"";
                    break;
                case TipoBanco.Firebase:
                    deps += ",\n" + tab + tab + "\"firebase-admin\": \"^13.0.2\"";
                    break;
            }
            string conteudo =
"{\n" +
tab + "\"name\": \"" + nomeProjeto.ToLower() + "\",\n" +
tab + "\"version\": \"1.0.0\",\n" +
tab + "\"private\": true,\n" +
tab + "\"main\": \"server.js\",\n" +
tab + "\"scripts\": {\n" +
tab + tab + "\"start\": \"node server.js\",\n" +
tab + tab + "\"dev\": \"nodemon server.js\"\n" +
tab + "},\n" +
tab + "\"dependencies\": {\n" +
tab + tab + deps + "\n" +
tab + "},\n" +
tab + "\"devDependencies\": {\n" +
tab + tab + "\"nodemon\": \"^3.1.9\"\n" +
tab + "}\n" +
"}\n";
            CriarArquivo(caminhoProjeto, "package", conteudo, ".json");
        }

        public static void CriarEnv(string caminhoProjeto, ConexaoBanco dados)
        {
            string conteudo = "NODE_ENV=development\nPORT=3000\n";
            string exemplo = "NODE_ENV=development\nPORT=3000\n";
            switch (dados.Tipo)
            {
                case TipoBanco.MySQL:
                case TipoBanco.PostgreSQL:
                    string porta = string.IsNullOrWhiteSpace(dados.Porta)
                        ? (dados.Tipo == TipoBanco.PostgreSQL ? "5432" : "3306")
                        : dados.Porta;
                    conteudo += "DB_HOST=" + dados.Server + "\nDB_PORT=" + porta + "\nDB_USER=" + dados.User + "\nDB_PASSWORD=" + dados.Password + "\nDB_NAME=" + dados.Schema + "\n";
                    exemplo += "DB_HOST=localhost\nDB_PORT=" + porta + "\nDB_USER=" + dados.User + "\nDB_PASSWORD=\nDB_NAME=" + dados.Schema + "\n";
                    break;
                case TipoBanco.Firebase:
                    conteudo += "FIREBASE_PROJECT_ID=" + dados.FirebaseProjectId + "\nFIREBASE_SERVICE_ACCOUNT=" + dados.FirebaseServiceAccountPath + "\n";
                    exemplo += "FIREBASE_PROJECT_ID=" + dados.FirebaseProjectId + "\nFIREBASE_SERVICE_ACCOUNT=./firebaseServiceAccountKey.json\n";
                    break;
            }
            CriarArquivo(caminhoProjeto, ".env", conteudo, "");
            CriarArquivo(caminhoProjeto, ".env.example", exemplo, "");
        }

        public static void CriarServerJs(string caminhoProjeto)
        {
            string conteudo =
"require('dotenv').config();\n" +
"const express = require('express');\n" +
"const cors = require('cors');\n" +
"const helmet = require('helmet');\n" +
"const rateLimit = require('express-rate-limit');\n" +
"const corsOptions = require('./src/config/cors');\n\n" +
"const app = express();\n\n" +
"app.use(helmet());\n" +
"app.use(express.json({ limit: '1mb' }));\n" +
"app.use(cors(corsOptions));\n" +
"app.use(rateLimit({ windowMs: 15 * 60 * 1000, max: 200, standardHeaders: true }));\n\n" +
"app.get('/health', (_req, res) => {\n" +
tab + "res.json({ ok: true, uptime: process.uptime() });\n" +
"});\n\n" +
"// Rotas serão registradas aqui pelo NodePunch conforme você criar novas APIs\n" +
"// Exemplo: app.use('/api/exemplo', require('./src/routes/exemploRoutes'));\n\n" +
"app.use((req, res) => {\n" +
tab + "res.status(404).json({ mensagem: 'Rota não encontrada.' });\n" +
"});\n\n" +
"app.use((err, req, res, _next) => {\n" +
tab + "console.error(err);\n" +
tab + "res.status(500).json({ mensagem: 'Erro interno.' });\n" +
"});\n\n" +
"const PORT = process.env.PORT || 3000;\n" +
"app.listen(PORT, () => {\n" +
tab + "console.log(`Servidor rodando em http://localhost:${PORT}`);\n" +
"});\n";
            CriarArquivo(caminhoProjeto, "server", conteudo);
            string corsConteudo =
"const CORS_ORIGINS = {\n" +
tab + "development: ['http://localhost:5173', 'http://localhost:3000'],\n" +
tab + "production: []\n" +
"};\n\n" +
"const ambiente = process.env.NODE_ENV === 'production' ? 'production' : 'development';\n\n" +
"module.exports = {\n" +
tab + "origin: CORS_ORIGINS[ambiente],\n" +
tab + "credentials: true\n" +
"};\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "config"), "cors", corsConteudo);
        }

        public static void CriarReadme(string caminhoProjeto, string nomeProjeto, TipoBanco tipo)
        {
            string dbInfo = tipo switch
            {
                TipoBanco.MySQL => "- Banco de dados: **MySQL** (`.env`)\n",
                TipoBanco.PostgreSQL => "- Banco de dados: **PostgreSQL** (`.env`)\n",
                TipoBanco.Firebase => "- Banco de dados: **Firebase Firestore**\n",
                _ => ""
            };
            string conteudo =
"# " + nomeProjeto + "\n\n" +
"Backend Node.js + Express gerado com [NodePunch](https://github.com/banana-eletrizante/nodepunch).\n\n" +
"## Como rodar\n\n```bash\nnpm install\nnpm run dev\n```\n\n" +
"Health check: `GET http://localhost:3000/health`\n\n" +
dbInfo +
"- Copie `.env.example` para `.env` se precisar versionar um modelo sem senha.\n";
            CriarArquivo(caminhoProjeto, "README", conteudo, ".md");
            Recentes.Registrar(nomeProjeto, caminhoProjeto);
            Shell.AbrirNpmInstall(caminhoProjeto);
        }

        public static void CriarGitignore(string caminhoProjeto)
        {
            CriarArquivo(caminhoProjeto, ".gitignore", "node_modules/\n.env\n.env.local\nfirebaseServiceAccountKey.json\n*.log\n.DS_Store\ndist/\nbuild/\n", "");
        }

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

        public static bool AdicionarDependencia(string caminhoProjeto, string nomePacote, string versao, bool devDependency = false)
        {
            string caminhoPackage = Path.Combine(caminhoProjeto, "package.json");
            if (!File.Exists(caminhoPackage)) return false;
            string conteudo = File.ReadAllText(caminhoPackage);
            string chave = "\"" + nomePacote + "\"";
            if (conteudo.Contains(chave)) return true;
            string secao = devDependency ? "\"devDependencies\": {" : "\"dependencies\": {";
            if (!conteudo.Contains(secao)) return false;
            File.WriteAllText(caminhoPackage, conteudo.Replace(secao, secao + "\n" + tab + tab + chave + ": \"" + versao + "\","));
            return true;
        }

        public static void AdicionarVariavelEnv(string caminhoProjeto, string chave, string valor)
        {
            string caminhoEnv = Path.Combine(caminhoProjeto, ".env");
            string conteudo = File.Exists(caminhoEnv) ? File.ReadAllText(caminhoEnv) : "";
            if (conteudo.Contains(chave + "=")) return;
            conteudo += (conteudo.EndsWith("\n") || conteudo == "" ? "" : "\n") + chave + "=" + valor + "\n";
            File.WriteAllText(caminhoEnv, conteudo);
        }

        public static string EscaparJS(string valor)
        {
            if (valor == null) return "";
            return valor.Replace("\\", "\\\\").Replace("'", "\\'");
        }

        public static bool EhPalavraReservadaJS(string valor)
        {
            string[] palavras = { "break","case","catch","class","const","continue","default","delete","do","else","export","extends","finally","for","function","if","import","in","instanceof","new","return","super","switch","this","throw","try","typeof","var","void","while","yield","let","static","await","async","enum","null","true","false","undefined" };
            return !string.IsNullOrWhiteSpace(valor) && Array.IndexOf(palavras, valor.Trim().ToLower()) >= 0;
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

        internal static bool VerificaTipoProjeto(string caminhoProjeto) =>
            Directory.Exists(Path.Combine(caminhoProjeto, "src"));

        internal static bool VerificaUsaBanco(string caminhoProjeto) =>
            File.Exists(Path.Combine(caminhoProjeto, "src", "base", "Banco.js"));

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
                foreach (Match v in new Regex("'([^']+)'").Matches(m.Groups[2].Value))
                    dic[chave].Add(v.Groups[1].Value);
            }
            return dic;
        }

        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            string semAcento = RemoverAcentos(input);
            string limpo = Regex.Replace(semAcento, "[^a-zA-Z0-9_\\s]", "").Trim();
            if (Regex.IsMatch(limpo, "^[a-z0-9]+(_[a-z0-9]+)*$")) return limpo;
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

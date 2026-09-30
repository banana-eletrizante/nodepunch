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

            string controller =
"const bcrypt = require('bcryptjs');\nconst jwt = require('jsonwebtoken');\nrequire('dotenv').config();\n\n" +
"class AuthController {\n" +
tab + "static token(usuario) {\n" +
tab + tab + "return jwt.sign({ id: usuario.id, email: usuario.email }, process.env.JWT_SECRET, { expiresIn: '7d' });\n" +
tab + "}\n" +
"}\nmodule.exports = AuthController;\n";
            CriarArquivo(Path.Combine(caminhoProjeto, "src", "controllers"), "authController", controller);

            AdicionarDependencia(caminhoProjeto, "jsonwebtoken", "^9.0.2");
            AdicionarDependencia(caminhoProjeto, "bcryptjs", "^2.4.3");
            AdicionarVariavelEnv(caminhoProjeto, "JWT_SECRET", System.Guid.NewGuid().ToString("N"));
            return true;
        }
    }
}

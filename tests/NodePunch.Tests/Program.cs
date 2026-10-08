using System.Text.Json.Nodes;
using NodePunch.Core;
using NodePunch.Forms;

internal static class Program
{
    private static int checks;
    private static void Check(bool ok, string label)
    {
        if (!ok) throw new Exception(label);
        checks++;
    }

    private static void Throws<T>(Action action, string label) where T : Exception
    {
        try { action(); }
        catch (T) { checks++; return; }
        throw new Exception(label);
    }

    [STAThread]
    private static void Main(string[] args)
    {
        string root = Path.Combine(Path.GetFullPath(args.Length > 0 ? args[0] : "generated-tests"), "run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Check(!Funcoes.EhIdentificadorJS("123usuario"), "identifier cannot begin with digit");
        Check(Funcoes.EhIdentificadorJS("usuario_2"), "valid identifier");
        Check(!Funcoes.EhIdentificadorJS("class"), "reserved word");
        Check(!Funcoes.EhNomeArquivoWindows("CON"), "Windows reserved filename");
        Check(!Funcoes.EscaparJS("nome'\nalert(1)").Contains('\n'), "escape line breaks in JavaScript");
        Throws<ArgumentException>(() => Funcoes.ValorEnv("senha\nINJECTED=true"), "reject env injection");

        string packageDir = Path.Combine(root, "package-edits");
        Directory.CreateDirectory(packageDir);
        string packagePath = Path.Combine(packageDir, "package.json");
        File.WriteAllText(packagePath, "{\"name\":\"test\",\"scripts\":{\"start\":\"node server.js\"},\"devDependencies\":{\"jsonwebtoken\":\"^9.0.2\"},\"custom\":42}");
        Check(Funcoes.AdicionarDependencia(packageDir, "jsonwebtoken", "^9.0.2"), "compact package and missing section");
        var package = JsonNode.Parse(File.ReadAllText(packagePath))!;
        Check(package["dependencies"]?["jsonwebtoken"] != null && package["devDependencies"]?["jsonwebtoken"] == null, "move runtime dependency out of devDependencies");
        Check((int)package["custom"]! == 42, "preserve custom package fields");
        File.WriteAllText(packagePath, "{bad JSON}");
        Check(!Funcoes.AdicionarDependencia(packageDir, "bcryptjs", "^2.4.3") && File.ReadAllText(packagePath) == "{bad JSON}", "invalid package stays intact");
        File.WriteAllText(packagePath, "{\"dependencies\":{\"jsonwebtoken\":null}}");
        Check(!Funcoes.AdicionarDependencia(packageDir, "jsonwebtoken", "^9.0.2"), "null dependency version is rejected");

        foreach (var (name, tipo, procedures) in new[]
        {
            ("none", TipoBanco.Nenhum, false), ("mysql", TipoBanco.MySQL, false),
            ("mysql-procedures", TipoBanco.MySQL, true), ("postgres", TipoBanco.PostgreSQL, false),
            ("firebase", TipoBanco.Firebase, false)
        })
        {
            string dir = Path.Combine(root, name);
            Directory.CreateDirectory(dir);
            var dados = new ConexaoBanco { Tipo = tipo, Server = "localhost", User = "user", Password = " senha#com espaço ", Schema = "app", ComSP = procedures, FirebaseProjectId = "project", FirebaseServiceAccountPath = "./firebaseServiceAccountKey.json" };
            Funcoes.CriarPackageJson(dir, "TesteApp", tipo);
            Funcoes.CriarEnv(dir, dados);
            Funcoes.CriarServerJs(dir);
            Funcoes.CriarReadme(dir, "TesteApp", tipo);
            Funcoes.CriarGitignore(dir);
            Check(File.Exists(Path.Combine(dir, ".env")) && File.Exists(Path.Combine(dir, ".env.example")), "env files for " + name);
            Check((string?)JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "package.json")))?["name"] == "testeapp", "npm name for " + name);
            if (tipo != TipoBanco.Nenhum)
            {
                Funcoes.CriarClasseBaseBD(dados, Path.Combine(dir, "src", "base"));
                Check(Funcoes.GerarAuthJWT(dir, tipo), "auth generated for " + name);
                string controllerPath = Path.Combine(dir, "src", "controllers", "authController.js");
                string original = File.ReadAllText(controllerPath);
                Throws<IOException>(() => Funcoes.GerarAuthJWT(dir, tipo), "auth overwrite requires confirmation");
                Check(File.ReadAllText(controllerPath) == original, "auth file preserved after refusal");
                string secret = File.ReadAllText(Path.Combine(dir, ".env"));
                Check(Funcoes.GerarAuthJWT(dir, tipo, true), "explicit auth regeneration");
                Check(File.ReadAllText(Path.Combine(dir, ".env")) == secret, "JWT secret stays unchanged");
                Check(!File.ReadAllText(Path.Combine(dir, ".env.example")).Contains(secret.Split("JWT_SECRET=")[1].Split('\n')[0]), "example contains no real JWT key");
                if (tipo == TipoBanco.MySQL) Check(original.Contains("Banco.consultarSql("), "auth bypasses procedures");
            }
            else Throws<InvalidOperationException>(() => Funcoes.GerarAuthJWT(dir, tipo), "auth requires a database");
            string server = Path.Combine(dir, "server.js");
            File.WriteAllText(server, File.ReadAllText(server).Replace("\r\n", "\n").Replace("\n", "\r\n"));
            Funcoes.RegistrarRotaNoServer(dir, "produtos");
            Funcoes.RegistrarRotaNoServer(dir, "produtos");
            string result = File.ReadAllText(server);
            Check(result.Split("require('./src/routes/produtosRoutes')").Length == 2, "CRLF and idempotent route registration");
            Funcoes.CriarArquivo(Path.Combine(dir, "src", "routes"), "produtosRoutes", "module.exports = require('express').Router();\n");
        }

        string env = Path.Combine(packageDir, ".env");
        File.WriteAllText(env, "# JWT_SECRET=example\nJWT_SECRET=\n");
        Funcoes.AdicionarVariavelEnv(packageDir, "JWT_SECRET", "real-secret");
        Check(File.ReadAllText(env).Contains("JWT_SECRET='real-secret'") && File.ReadAllText(env).Contains("# JWT_SECRET=example"), "comments don't suppress empty secret replacement");
        string broken = Path.Combine(root, "no-marker");
        Directory.CreateDirectory(broken);
        File.WriteAllText(Path.Combine(broken, "server.js"), "custom server");
        Throws<InvalidOperationException>(() => Funcoes.RegistrarRotaNoServer(broken, "produtos"), "missing route marker reported");
        Check(File.ReadAllText(Path.Combine(broken, "server.js")) == "custom server", "custom server preserved");
        string exemploDir = Path.Combine(root, "none");
        Funcoes.RegistrarRotaNoServer(exemploDir, "exemplo");
        Check(System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(Path.Combine(exemploDir, "server.js")), @"(?m)^app\.use\('/api/exemplo'"), "commented example is not an installed route");
        Funcoes.CriarArquivo(Path.Combine(exemploDir, "src", "routes"), "exemploRoutes", "module.exports = require('express').Router();\n");

        string legacy = Path.Combine(root, "legacy-mysql-procedures");
        Funcoes.CriarPasta(legacy);
        Funcoes.CriarPackageJson(legacy, "legacy", TipoBanco.MySQL);
        Funcoes.CriarEnv(legacy, new ConexaoBanco { Tipo = TipoBanco.MySQL });
        Funcoes.CriarServerJs(legacy);
        Funcoes.CriarClasseBaseBD(new ConexaoBanco { Tipo = TipoBanco.MySQL, ComSP = true }, Path.Combine(legacy, "src", "base"));
        string legacyBanco = Path.Combine(legacy, "src", "base", "Banco.js");
        string bancoTexto = File.ReadAllText(legacyBanco);
        int inicio = bancoTexto.IndexOf("\tstatic async consultarSql(", StringComparison.Ordinal);
        int fim = bancoTexto.IndexOf("\tstatic async #conectar(", inicio, StringComparison.Ordinal);
        File.WriteAllText(legacyBanco, bancoTexto.Remove(inicio, fim - inicio));
        Check(Funcoes.GerarAuthJWT(legacy, TipoBanco.MySQL), "auth works with old generated MySQL projects");
        Check(File.ReadAllText(legacyBanco).Contains("static async consultarSql("), "legacy MySQL adapter upgraded");

        // Construct all Windows Forms without showing windows or invoking npm.
        foreach (var form in new System.Windows.Forms.Form[] { new frmInicial(), new frmNovoProjeto(), new Form1(), new frmCriarAPI(), new frmCriarAuth(), new frmCriarClasse(), new frmConfiguracoes() })
        {
            Check(form.Controls.Count > 0, "form initializes: " + form.GetType().Name);
            form.Dispose();
        }
        Console.WriteLine($"Passed {checks} checks. Generated backends: {root}");
    }
}

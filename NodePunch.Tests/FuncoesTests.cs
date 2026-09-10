using System.Diagnostics;
using System.Text.Json.Nodes;
using NodePunch.Core;

namespace NodePunch.Tests;

[TestClass]
public sealed class FuncoesTests
{
    private string _diretorio = null!;

    [TestInitialize]
    public void Preparar()
    {
        _diretorio = Path.Combine(Path.GetTempPath(), "nodepunch-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_diretorio);
    }

    [TestCleanup]
    public void Limpar()
    {
        if (Directory.Exists(_diretorio))
            Directory.Delete(_diretorio, recursive: true);
    }

    [TestMethod]
    public void ToCamelCase_NormalizaAcentosEspacosESimbolos()
    {
        Assert.AreEqual("minhaRotaApi", Funcoes.ToCamelCase("Minha rota API!"));
        Assert.AreEqual("usuario", Funcoes.ToCamelCase("usuário"));
        Assert.AreEqual(string.Empty, Funcoes.ToCamelCase("!@#"));
    }

    [TestMethod]
    public void IdentificadorJS_RejeitaNumeroInicialEPalavraReservada()
    {
        Assert.IsFalse(Funcoes.EhIdentificadorJSValido("1usuario"));
        Assert.IsTrue(Funcoes.EhIdentificadorJSValido("_usuario1"));
        Assert.IsTrue(Funcoes.EhPalavraReservadaJS("class"));
    }

    [TestMethod]
    public void Exclusao_AceitaSomenteItensDentroDaRaiz()
    {
        string raiz = Path.Combine(_diretorio, "projeto");
        string filho = Path.Combine(raiz, "src", "arquivo.js");
        string irmao = Path.Combine(_diretorio, "projeto-segredos", "arquivo.txt");

        Assert.IsTrue(Arvore.EhCaminhoSeguroParaExcluir(raiz, filho));
        Assert.IsFalse(Arvore.EhCaminhoSeguroParaExcluir(raiz, raiz));
        Assert.IsFalse(Arvore.EhCaminhoSeguroParaExcluir(raiz, irmao));
        Assert.IsFalse(Arvore.EhCaminhoSeguroParaExcluir(raiz, null));
    }

    [TestMethod]
    [DataRow("Nenhum", null)]
    [DataRow("MySQL", "mysql2")]
    [DataRow("PostgreSQL", "pg")]
    [DataRow("Firebase", "firebase-admin")]
    public void PackageJson_EValidoEIncluiDependenciaDoBanco(string nomeTipo, string? dependenciaBanco)
    {
        TipoBanco tipo = Enum.Parse<TipoBanco>(nomeTipo);

        Funcoes.CriarPackageJson(_diretorio, "MinhaApi", tipo);

        JsonObject package = JsonNode.Parse(File.ReadAllText(Path.Combine(_diretorio, "package.json")))!.AsObject();
        JsonObject dependencies = package["dependencies"]!.AsObject();

        Assert.AreEqual("minhaapi", package["name"]!.GetValue<string>());
        Assert.AreEqual(">=20", package["engines"]!["node"]!.GetValue<string>());
        Assert.IsTrue(dependencies.ContainsKey("express"));
        Assert.IsTrue(dependencies.ContainsKey("helmet"));
        Assert.IsTrue(dependencies.ContainsKey("express-rate-limit"));
        if (dependenciaBanco != null)
            Assert.IsTrue(dependencies.ContainsKey(dependenciaBanco));
    }

    [TestMethod]
    public void AdicionarDependencia_PreservaJsonEAtualizaVersao()
    {
        Funcoes.CriarPackageJson(_diretorio, "api", TipoBanco.Nenhum);

        Assert.IsTrue(Funcoes.AdicionarDependencia(_diretorio, "express-validator", "^7.3.2"));
        Assert.IsTrue(Funcoes.AdicionarDependencia(_diretorio, "express-validator", "^7.3.2"));

        JsonObject package = JsonNode.Parse(File.ReadAllText(Path.Combine(_diretorio, "package.json")))!.AsObject();
        Assert.AreEqual("^7.3.2", package["dependencies"]!["express-validator"]!.GetValue<string>());
    }

    [TestMethod]
    public void AdicionarVariavelEnv_ComparaAChaveCompleta()
    {
        File.WriteAllText(Path.Combine(_diretorio, ".env"), "OUTRO_JWT_SECRET=existente\n");

        Funcoes.AdicionarVariavelEnv(_diretorio, "JWT_SECRET", "novo");

        string env = File.ReadAllText(Path.Combine(_diretorio, ".env"));
        StringAssert.Contains(env, "OUTRO_JWT_SECRET=existente");
        StringAssert.Contains(env, "JWT_SECRET=novo");
    }

    [TestMethod]
    public void ServerJs_IncluiProtecoesEEndpointDeSaude()
    {
        Funcoes.CriarServerJs(_diretorio);

        string server = File.ReadAllText(Path.Combine(_diretorio, "server.js"));
        StringAssert.Contains(server, "helmet()");
        StringAssert.Contains(server, "rateLimit(");
        StringAssert.Contains(server, "app.get('/health'");
        StringAssert.Contains(server, "app.disable('x-powered-by')");
    }

    [TestMethod]
    public void MySqlComStoredProcedures_MantemConsultasSqlParaAutenticacao()
    {
        ConexaoBanco dados = CriarConexao(TipoBanco.MySQL);
        dados.ComSP = true;

        Funcoes.CriarClasseBaseBD(dados, Path.Combine(_diretorio, "src", "base"));

        string banco = File.ReadAllText(Path.Combine(_diretorio, "src", "base", "Banco.js"));
        StringAssert.Contains(banco, "static async consultar(comando");
        StringAssert.Contains(banco, "static async executar(comando");
        StringAssert.Contains(banco, "static async consultarProcedure(nomeProcedure");
        StringAssert.Contains(banco, "static async executarProcedure(nomeProcedure");
        StringAssert.Contains(banco, "Nome de procedure inválido");
        Assert.AreEqual(0, VerificarSintaxeNode(Path.Combine(_diretorio, "src", "base", "Banco.js")));
    }

    [TestMethod]
    public void GerarAuthJWT_SemBancoFalhaComMensagemClara()
    {
        InvalidOperationException erro = Assert.ThrowsExactly<InvalidOperationException>(
            () => Funcoes.GerarAuthJWT(_diretorio, TipoBanco.Nenhum));

        StringAssert.Contains(erro.Message, "banco de dados");
    }

    [TestMethod]
    [DataRow("MySQL")]
    [DataRow("PostgreSQL")]
    [DataRow("Firebase")]
    public void TemplatesComBancoEAuth_GeramJavaScriptValido(string nomeTipo)
    {
        TipoBanco tipo = Enum.Parse<TipoBanco>(nomeTipo);
        ConexaoBanco dados = CriarConexao(tipo);

        Funcoes.CriarPackageJson(_diretorio, "api", tipo);
        Funcoes.CriarEnv(_diretorio, dados);
        Funcoes.CriarEnvExemplo(_diretorio, tipo);
        Funcoes.CriarServerJs(_diretorio);
        Funcoes.CriarClasseBaseBD(dados, Path.Combine(_diretorio, "src", "base"));

        Assert.IsTrue(Funcoes.GerarAuthJWT(_diretorio, tipo));

        foreach (string arquivo in Directory.GetFiles(_diretorio, "*.js", SearchOption.AllDirectories))
            Assert.AreEqual(0, VerificarSintaxeNode(arquivo), $"JavaScript inválido: {arquivo}");
    }

    private static ConexaoBanco CriarConexao(TipoBanco tipo)
    {
        return new ConexaoBanco
        {
            Tipo = tipo,
            Server = "localhost",
            Porta = tipo == TipoBanco.MySQL ? "3306" : "5432",
            User = "app",
            Password = "segredo",
            Schema = "app",
            FirebaseProjectId = "demo",
            FirebaseServiceAccountPath = "./firebaseServiceAccountKey.json"
        };
    }

    private static int VerificarSintaxeNode(string arquivo)
    {
        using Process processo = Process.Start(new ProcessStartInfo
        {
            FileName = "node",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            ArgumentList = { "--check", arquivo }
        }) ?? throw new InvalidOperationException("Não foi possível iniciar o Node.js.");

        processo.WaitForExit(10_000);
        return processo.ExitCode;
    }
}

# NodePunch

Gerador desktop para criar a base de APIs Node.js com rapidez e consistência. O NodePunch oferece uma interface Windows simples para montar projetos Express, configurar persistência, criar models, controllers, rotas e autenticação sem repetir scaffolding manual.

> Menos tempo preparando pastas e arquivos; mais tempo implementando as regras do produto.

## O que ele gera

- Projeto Express 5 organizado em `models`, `controllers`, `routes`, `config` e `base`.
- Integração opcional com MySQL, PostgreSQL ou Firebase Firestore.
- Pool de conexões e consultas parametrizadas para bancos SQL.
- Rotas GET, POST, PUT e DELETE, com registro automático no servidor.
- Validação opcional de payload com `express-validator`.
- Autenticação JWT com registro, login, hash de senha via bcrypt e middleware de proteção.
- CORS separado por ambiente, endpoint de saúde e tratamento padronizado de erros.
- Proteções iniciais com Helmet, limite de JSON e rate limiting.
- `.env`, `.env.example`, `.gitignore`, `package.json` e README do projeto gerado.

## Bancos compatíveis

| Opção | Pacote utilizado | Observações |
| --- | --- | --- |
| Sem banco | — | Estrutura Express pronta para receber outra persistência. |
| MySQL | `mysql2` | Consultas SQL ou stored procedures. |
| PostgreSQL | `pg` | Pool e placeholders posicionais (`$1`, `$2`...). |
| Firebase | `firebase-admin` | Firestore com credencial de service account ignorada pelo Git. |

## Requisitos

- Windows 10 ou 11.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) para desenvolver ou executar pelo código-fonte.
- [Node.js 20 ou superior](https://nodejs.org/) para executar os backends gerados.
- Visual Studio com suporte a desenvolvimento desktop .NET, ou outro editor compatível com C#.

## Executar o NodePunch

Clone o repositório e execute:

```powershell
dotnet restore .\NodePunch.csproj
dotnet run --project .\NodePunch.csproj
```

Também é possível abrir `nodepunch.slnx` diretamente no Visual Studio.

## Fluxo de uso

1. Escolha **Criar novo projeto** e indique nome e pasta.
2. Selecione o banco e informe a conexão, ou continue sem persistência.
3. No painel do projeto, crie classes e controllers.
4. Gere rotas e escolha os métodos HTTP desejados.
5. Se houver banco configurado, gere a autenticação JWT.
6. Abra o backend no editor, execute `npm install` e depois `npm run dev`.

O backend expõe `GET /health` para verificação rápida do serviço. As novas rotas são registradas sob `/api/<nome>`.

## Estrutura do repositório

```text
Core/
  Arvore.cs          navegação e exclusão segura de arquivos
  ConexaoBanco.cs    modelo das configurações de persistência
  Funcoes.cs         templates e utilitários de geração
Forms/
  frmInicial.cs      abertura ou criação de projetos
  frmNovoProjeto.cs  assistente de configuração
  Form1.cs           área principal do projeto
  frmCriar*.cs       geradores de classes, rotas e autenticação
Resources/
  nodepunch.ico
Program.cs           ponto de entrada do aplicativo
```

## Segurança e limites

O NodePunch entrega uma base segura para desenvolvimento, mas não substitui a revisão específica de cada aplicação. Antes de publicar um backend:

- restrinja as origens de produção em `src/config/cors.js`;
- mantenha `.env` e credenciais do Firebase fora do repositório;
- use uma coluna `email` com índice único na tabela `usuarios`;
- ajuste o rate limit à carga esperada;
- implemente autorização por função/permissão quando necessário;
- adicione testes para as regras de negócio criadas após o scaffolding.

## Desenvolvimento

```powershell
dotnet build .\nodepunch.slnx
dotnet run --project .\NodePunch.csproj
```

O aplicativo principal usa .NET 8 e Windows Forms sem pacotes NuGet externos; o projeto de testes usa MSTest.

### Testes

```powershell
dotnet test .\nodepunch.slnx --configuration Release
```

A suíte automatizada valida os utilitários, o `package.json`, as variáveis de ambiente, os recursos de segurança e a sintaxe JavaScript dos templates MySQL, PostgreSQL e Firebase.

## Licença

Distribuído sob a licença MIT. Consulte [LICENSE](LICENSE).

## Autor

Projeto de André Rosler. Mais informações em [andre-rosler.com/projetos](https://andre-rosler.com/projetos).

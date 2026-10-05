# nodepunch: gerador desktop de backends Node.js

> AGENTS.md criado em 05/10/2026 (cópia do Raspberry Pi). Contexto geral em `../AGENTS.md`.

- **O que é:** app Windows (WinForms) que gera a base de APIs Node.js/Express 5: models, controllers, rotas, persistência opcional (MySQL, PostgreSQL ou Firestore) e autenticação JWT. Versão 1.3.0 no `NodePunch.csproj`; licença MIT.
- **Stack:** C#, .NET 8 (`net8.0-windows`, `UseWindowsForms`), sem pacotes NuGet no app; testes com MSTest em `NodePunch.Tests/`. Os backends gerados exigem Node.js 20+.
- **Estrutura:** `Program.cs` (entrada; abre `frmInicial`), `Core/` (Arvore, ConexaoBanco, Funcoes com os templates, IconHelper, Tema), `Forms/` (frmInicial, frmNovoProjeto, Form1, frmCriarAPI, frmCriarAuth, frmCriarClasse, frmConfiguracoes), `Resources/nodepunch.ico`, `NodePunch.Tests/FuncoesTests.cs`, `nodepunch.slnx`, `.github/workflows/build.yml`.
- **Comandos** (README/CI; só rodam no Windows, porque o alvo é WinForms):
  - `dotnet restore .\nodepunch.slnx`
  - `dotnet build .\nodepunch.slnx --configuration Release`
  - `dotnet run --project .\NodePunch.csproj`
  - `dotnet test .\nodepunch.slnx --configuration Release`
- **CI:** GitHub Actions em `windows-latest` (.NET 8 e Node 20): restore, build e test em push na `master` e em PRs. Não há deploy; a distribuição é pelo código-fonte (o README não cita releases além da tag `v1.2`).
- **Git (05/10/2026, com `core.autocrlf=true`):** branch `master`, **1 commit à frente** de `origin/master` (`14ffce8` "feat: modernize project generation and add tests", ainda não enviado). Há 8 arquivos modificados e não commitados (`Forms/*.cs` e `Program.cs`; a maior parte em `Forms/Form1.cs`) e `Core/Tema.cs` não rastreado, além deste `AGENTS.md`. Tag `v1.2`.
- **Convenções:** formulários com prefixo `frm`, nomes de domínio em português (`Arvore`, `Funcoes`, `ConexaoBanco`), commits `feat:`/`docs:` em inglês. `bin/`, `obj/` e `.vs/` são ignorados pelo `.gitignore` do Visual Studio.
- **Localização:** `/mnt/hdd/Projetos/Carreira Profissional/nodepunch` · `Z:\Projetos\Carreira Profissional\nodepunch` · via Tailscale `ssh andrew@100.97.106.15` · MCP: `Carreira Profissional/nodepunch/...`.
- **Dependências:** `bin/`, `obj/` e `.vs/` vieram do Windows. Rode `dotnet restore` na máquina de trabalho. No Linux, use `git -c core.autocrlf=true status`.

- Remoto SSH nesta cópia do Pi: `git@github-nodepunch:banana-eletrizante/nodepunch.git`.

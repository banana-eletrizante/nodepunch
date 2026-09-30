# NodePunch

Ferramenta desktop em C# para agilizar a criação de backends Node.js (Express).

Gera a estrutura do projeto, rotas, models, JWT e arquivos base a partir de uma UI Windows.

Site: [andre-rosler.com](https://andre-rosler.com/projetos)

## Novidades da 1.1

- Projetos recentes na tela inicial
- Backend gerado com Helmet, rate-limit e `GET /health`
- `.env.example` separado do `.env`
- Dependências atualizadas
- README do projeto gerado mais completo

## Requisitos

- .NET 8 SDK (Windows)
- Node.js no PATH, se for executar o backend gerado

## Abrir

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

Ou abra `NodePunch.sln` / `NodePunch.csproj` no Visual Studio / Rider.

## Estrutura

```
Program.cs          entrada
Forms/              interface Windows Forms
Core/               geração de templates e lógica
Resources/          assets
```

## Licença

MIT.

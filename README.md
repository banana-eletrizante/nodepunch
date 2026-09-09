# NodePunch

Ferramenta desktop em C# para agilizar a criação de backends Node.js (Express / TypeScript).

Gera a estrutura do projeto, rotas e arquivos base a partir de uma UI Windows — menos scaffolding manual, mais tempo no domínio.

Site: [andre-rosler.com](https://andre-rosler.com/projetos)

## Requisitos

- .NET SDK (Windows)
- Node.js no PATH, se for executar o backend gerado

## Abrir

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

Ou abra `nodepunch.slnx` / `NodePunch.csproj` no Visual Studio / Rider.

## Estrutura

```
Program.cs          entrada
Forms/              interface Windows Forms
Core/               geração de templates e lógica
Resources/          assets
```

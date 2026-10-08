# NodePunch

App Windows em C# que monta um backend Node.js (Express) a partir de uma tela.
Você escolhe nome, pasta e banco. O app cria a árvore, o `server.js` e o `.env`.
A instalação das dependências do backend é opcional na tela de criação.

Site: [andre-rosler.com/projetos](https://andre-rosler.com/projetos)

## O que ele gera

- `server.js` com Helmet, CORS, rate-limit e `GET /health`
- `.env` + `.env.example`
- Camada de banco: MySQL, PostgreSQL ou Firebase
- Rotas Express (GET/POST/PUT/DELETE), com ou sem `express-validator`
- JWT: `POST /api/auth/registrar` e `POST /api/auth/login` + `usuarios.sql`
- README e `.gitignore` do projeto novo

## Como usar

1. Abra o NodePunch e clique em **Novo projeto**.
2. Informe nome, pasta e banco.
3. Marque a opção de instalar dependências ou rode `npm install` na pasta gerada.
4. Nos cartões do projeto: Model/Controller, Rota Express ou Autenticação JWT.
5. `npm run dev` na pasta gerada. Health: `http://localhost:3000/health`.

Selecione um arquivo no explorador para consultar a prévia. Arraste a divisória para
ajustar a largura da árvore. O botão **Início** retorna à lista de projetos recentes.

| Atalho | Ação |
| --- | --- |
| Ctrl+N / Ctrl+O | Novo projeto / abrir pasta na tela inicial |
| Ctrl+M / Ctrl+R / Ctrl+J | Model / rota / autenticação no projeto |
| F5 | Atualizar arquivos do projeto |
| Escape | Fechar um formulário |

## Abrir o app

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

Para executar o código-fonte, use o SDK .NET 8 ou superior no Windows.
O executável portátil inclui o runtime: basta baixar o `NodePunch.exe` na release e abrir.
Node.js/npm é necessário para executar os backends gerados, não para abrir o NodePunch.

## Publicar o executável portátil

```powershell
dotnet publish NodePunch.csproj -p:PublishProfile=Portable -o artifacts/portable
```

O perfil gera um único `.exe` para Windows x64, com compressão e o ícone embutido.
Não usa trimming, que não é compatível com todos os recursos do Windows Forms.

## Verificação

```powershell
dotnet run --project tests/NodePunch.Tests/NodePunch.Tests.csproj -c Release -- tests/generated
npm ci --prefix tests
node tests/generated-backends.cjs tests/generated
```

Os testes verificam os arquivos gerados, edição de dependências, preservação da chave JWT,
cadastro/login com bancos simulados, inicialização das telas e HTTP real do Express (`/health`, 404 e JSON inválido).
Não conectam a bancos de produção.

MIT © André Rösler

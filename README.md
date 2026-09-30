# NodePunch

App Windows em C# que monta um backend Node.js (Express) a partir de uma tela.
Você escolhe nome, pasta e banco. O app cria a árvore, o `server.js`, o `.env` e já abre o `npm install`.

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
3. Espere o terminal de `npm install`.
4. No menu do projeto: Model/Controller, Rota Express ou Autenticação JWT.
5. `npm run dev` na pasta gerada. Health: `http://localhost:3000/health`.

## Abrir o app

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

Requer .NET 8 no Windows.

MIT © André Rösler

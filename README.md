# NodePunch

Ferramenta desktop em C# para criar backends Node.js (Express) sem montar a pasta na mão.

Site: [andre-rosler.com](https://andre-rosler.com/projetos)

## 1.2.0

- Cria o projeto e já abre `npm install`
- Menu com `npm install` e Abrir pasta
- JWT com `POST /api/auth/registrar` e `POST /api/auth/login`
- Rotas novas entram sozinhas no `server.js`
- Helmet, rate-limit, `/health`, `.env.example`
- Projetos recentes na tela inicial

## Abrir

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

MIT © André Rösler

# NodePunch

Ferramenta desktop em C# para agilizar a criação de backends Node.js (Express).

Site: [andre-rosler.com](https://andre-rosler.com/projetos)

## 1.1.1

- JWT de verdade de novo: `POST /api/auth/registrar` e `POST /api/auth/login`, middleware e `usuarios.sql`
- `server.js` volta a registrar rotas novas automaticamente
- Helmet, rate-limit, health check e `.env.example`
- Tela inicial com projetos recentes

## Abrir

```bash
dotnet restore
dotnet run --project NodePunch.csproj
```

MIT.

# Histórico

## 1.3.0

- `.env` e `.env.example` também são gerados em projetos sem banco.
- Dependências são editadas como JSON, preservando scripts e campos personalizados.
  Pacotes usados em execução deixam de ficar somente em `devDependencies`.
- Autenticação MySQL funciona com projetos configurados para stored procedures.
  Adaptadores antigos gerados pelo NodePunch recebem o método de SQL direto quando necessário.
- Gerar autenticação novamente pede confirmação, preserva a chave JWT existente e
  informa quando o servidor perdeu o marcador necessário para registrar rotas.
- Cadastro valida tipos, e-mail e limites de senha do bcrypt; tokens novos têm validade
  configurável por `JWT_EXPIRES_IN` (padrão de uma hora), e a chave é gerada com 32 bytes aleatórios.
- Cadastro Firestore usa criação atômica com ID estável por e-mail, evitando duplicatas
  quando duas requisições de cadastro chegam ao mesmo tempo.
- Nomes inválidos de classes/propriedades e nomes reservados do Windows são rejeitados.
  Campos de validação são escapados antes de serem colocados em JavaScript.
- Portas de banco precisam estar entre 1 e 65535. Senhas com `#` e espaços são
  preservadas no `.env`; quebras de linha nos valores são rejeitadas.
- Instalar dependências com npm é opcional; o app avisa quando npm não está disponível.
  Gerar README não inicia processos nem altera a lista de projetos recentes.
- O explorador não apaga a raiz do projeto nem percorre links de diretório.
  Abrir VS Code funciona em caminhos com espaços; a tela de JWT tem largura suficiente para as instruções.
- Erros de JSON inválido e corpos muito grandes retornam 400/413 no backend gerado.
- Executável portátil comprimido, runtime incluído e ícone incorporado, sem instalador.
- Testes automatizados do aplicativo, geradores, autenticação e endpoints Express, executados no CI.

As mudanças nos templates se aplicam aos backends criados ou regenerados com esta versão.
Arquivos existentes não são migrados automaticamente.

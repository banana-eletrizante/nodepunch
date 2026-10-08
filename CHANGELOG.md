# Histórico

## 1.5.0

- Nova tela inicial com composição exclusiva NodePunch, assinatura do autor e rede de nós desenhada em vetor.
- Cartões arredondados com símbolos e cores próprias para model, API, JWT, ambiente, editor e npm.
- Cabeçalhos com selo NP e a mesma identidade nas telas de projeto e nos formulários.
- Projetos recentes com cartões, nomes e caminhos separados; estado vazio com orientação para começar.
- Prévia com realce de palavras-chave, comentários, strings e números em arquivos de código.
  Arquivos maiores continuam disponíveis em texto simples para preservar a responsividade.
- Barra de prévia informa tipo, tamanho e número de linhas; arquivos recebem cores por extensão no explorador.
- Seleção de pasta usa o diálogo de pastas do Windows, tanto para abrir quanto para criar projetos.
- Mantém o executável portátil comprimido e a navegação por teclado.

## 1.4.0

- Painel de projeto com seis cartões de ação, títulos e descrições, no lugar da área MDI vazia.
- Explorador com largura ajustável e prévia de arquivos de texto, incluindo `.env.example`.
  A prévia é somente leitura e limita arquivos a 512 KB.
- Paleta escura consistente, amarelo de destaque, contraste, espaçamento e cabeçalhos padronizados.
- Tela inicial redimensionável e projetos recentes acessíveis pelo teclado.
- Fechar o projeto retorna à tela inicial e atualiza os recentes.
- Ferramentas abrem como diálogos centralizados, com Enter para confirmar e Escape para fechar.
  Enter no editor de `.env` continua inserindo uma nova linha.
- Atalhos Ctrl+N/Ctrl+O na tela inicial, Ctrl+M/Ctrl+R/Ctrl+J e F5 no projeto.
- Escala de DPI por monitor e verificações de layout em 100% e 150%.

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

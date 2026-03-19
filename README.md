# Desafio QA - Minhas Finanças

[![QA Pipeline](https://github.com/CayoViegas/desafio-qa/actions/workflows/ci.yml/badge.svg)](https://github.com/CayoViegas/desafio-qa/actions/workflows/ci.yml)

Bem-vindo ao repositório de automação de testes do sistema **Minhas Finanças**.

## Estruturação da Pirâmide de Testes e Justificativas

A estratégia foi desenhada com base na **Pirâmide de Testes**, dividindo a responsabilidade para obter *feedback* rápido, isolamento de falhas e garantia de que a experiência do usuário final é impecável.

### 1. Testes de Unidade (Base da Pirâmide)
- **O que testam:** Entidades do Domínio (`Pessoa`, `Categoria`, `Transacao`) e validações de DTOs isoladamente.
- **Justificativa da Escolha:** São os testes mais rápidos e baratos. Foram escolhidos para validar a lógica pura da aplicação (ex: cálculo exato de idade, validação de datas no passado/futuro e regras intrínsecas de negócio) sem a lentidão de subir um banco de dados. Utilizamos *Reflection* para testar métodos e propriedades `internal` sem quebrar o encapsulamento da Clean Architecture.

### 2. Testes de Integração (Meio da Pirâmide)
- **O que testam:** As rotas da API (`Controllers`), a camada de Serviços e as consultas no Banco de Dados via Entity Framework.
- **Justificativa da Escolha:** O sistema realiza cálculos vitais de saldo (Totais por Pessoa) e Exclusão em Cascata. Testes unitários não conseguem validar se as *queries* SQL geradas pelo EF Core estão corretas. Foi configurado um ambiente com SQLite em memória (`CustomWebApplicationFactory`) para simular o banco real, provando que a API soma e filtra as transações adequadamente e deleta os dados em cascata.

### 3. Testes End-to-End - E2E (Topo da Pirâmide)
- **O que testam:** A jornada visual do usuário no frontend React utilizando **Playwright**.
- **Justificativa da Escolha:** Validam a integração final entre Frontend, Backend e Banco de Dados. Foram criados fluxos dinâmicos gerando dados com `Date.now()` para evitar *State Leakage* (vazamento de estado) entre execuções. O E2E foca em garantir que os bloqueios de regras de negócio reflitam corretamente na interface visual (exibição de Toasts) e que a navegação da *Single Page Application* (SPA) funcione adequadamente.

---

## Bugs Identificados e Documentados

Durante o desenvolvimento da automação, foram documentados comportamentos anômalos na pasta [`docs/bugs`](./docs/bugs). Abaixo estão os bugs mapeados contra as regras de negócio do sistema:

1. **[BUG-001] Erro 500 ao tentar cadastrar receita para menor de idade**
   - **Regra que falhou:** *Menores de 18 anos só podem registrar despesas.*
   - **Descrição:** O `ExceptionMiddleware` não trata exceções de domínio adequadamente. Ao invés de retornar um `400 Bad Request` com a mensagem amigável da regra, a API retorna um `500 Internal Server Error`, impedindo o frontend de exibir a causa exata ao usuário.

2. **[BUG-002] Erro 500 por incompatibilidade de Categoria**
   - **Regra que falhou:** *Uma Transação só pode ser associada a uma Categoria que permita seu tipo (Receita/Despesa).*
   - **Descrição:** Similar ao BUG-001, ao forçar via API a criação de uma transação de Receita utilizando uma Categoria estritamente de Despesa, o sistema gera uma exceção de validação não tratada no nível HTTP (500).

3. **[BUG-003] Falha de invalidação de Cache na Exclusão em Cascata**
   - **Regra que falhou:** *Ao excluir uma Pessoa, todas as suas transações devem ser apagadas (Cascata) e a UI deve refletir o estado atual.*
   - **Descrição:** O backend exclui as transações corretamente. Porém, na interface React, o *hook* `useDeletePessoa` não invalida o cache do React Query para a rota de transações. O usuário é forçado a atualizar a página manualmente (F5) para ver as transações desaparecerem da listagem.

---

## Integração Contínua (CI/CD) e Limitações

Este repositório conta com uma pipeline configurada no **GitHub Actions** (`.github/workflows/ci.yml`).

**Nota sobre o CI/CD e as Limitações do Desafio:** Em respeito à regra restritiva de *não realizar o upload do código-fonte da aplicação*, a pipeline foi configurada como uma **Prova de Conceito (PoC) de Qualidade**. Como os testes de backend necessitam das referências do projeto original para compilar, e os testes E2E necessitam da aplicação rodando, a pipeline foca em realizar o *checkout* do código de automação e executar a validação estática do TypeScript do Playwright através de um *Dry Run* (`npx playwright test --list`). Isso garante que os scripts criados compilam e estão livres de erros de sintaxe, demonstrando conhecimento em CI/CD e infraestrutura, mas respeitando integralmente a regra do repositório limpo.

---

## Como executar os testes localmente

Para a execução real dos testes E2E, a aplicação original precisa estar hospedada na máquina local.

1. **Suba a API:**
```bash
cd api/MinhasFinancas.API
dotnet run --urls "http://localhost:5000"
```
2. **Suba o Frontend:** (Em outro terminal)
```bash
cd web
bun install
bunx vite --port 5173
```
3. **Execute o Backend Tests:** (Em um terceiro terminal, na raiz do repositório)
```bash
dotnet test BackendTests/BackendTests.csproj
```
4. **Execute o Playwright (E2E):**
```bash
cd e2e-tests
npm install
npx playwright test --ui
```

---
# 🚀 Plano de Execução de Testes — PotentialWind 🌬️⚡

## 1. Introdução e Objetivo

Este **Plano de Execução de Testes** estabelece a rotina operacional, a infraestrutura, o cronograma e os procedimentos formais de execução para as suítes de **Testes Unitários (xUnit / .NET)** e **Testes Automatizados BDD (Cucumber.js)** do sistema **PotentialWind**.

O objetivo é garantir a qualidade contínua, a acurácia dos cálculos físicos e financeiros de engenharia eólica e a estabilidade da aplicação em todas as etapas do ciclo de desenvolvimento de software.

---

## 2. Estratégia e Níveis de Execução

```text
 ┌─────────────────────────────────────────────────────────┐
 │               Fase 1: Desenvolvimento Local             │
 │  • Execução de Testes Unitários Rápido (xUnit)          │
 └──────────────────────────┬──────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────┐
 │               Fase 2: Validação de Commit               │
 │  • Execução de Testes BDD Automatizados (Cucumber)      │
 └──────────────────────────┬──────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────┐
 │               Fase 3: Pipeline de CI/CD                 │
 │  • Compilação de Backend e Frontend                     │
 │  • Execução Regressiva Completa & Geração de Relatórios │
 └─────────────────────────────────────────────────────────┘
```

---

## 3. Matriz de Ambientes e Ferramentas

| Nível de Teste | Framework / Ferramenta | Localização do Código | Ambiente de Execução | Tempo Médio Esperado |
| :--- | :--- | :--- | :--- | :--- |
| **Testes Unitários** | xUnit / .NET 10 | `backend/PotentialWind.Tests/` | Local / Pipeline CI/CD | < 3 segundos |
| **Testes BDD (Aceitação)** | Cucumber.js & Axios | `tests/bdd/` | API Rodando Local/Staging | < 5 segundos |
| **Compilação Frontend** | Angular CLI / TypeScript | `frontend/` | Node.js 20+ | < 15 segundos |

---

## 4. Guia Passo a Passo de Execução

### 4.1 Passo 1: Execução da Suíte de Testes Unitários (xUnit)

Os testes unitários validam isoladamente os algoritmos matemáticos (Função Gamma, Weibull, Hellmann Power Law, Jensen Wake Model, LCOE, VPL e TIR) e os Controllers da Web API.

```bash
# Executar todos os testes unitários no terminal
dotnet test

# Executar com detalhamento de testes aprovados/reprovados
dotnet test --logger "console;verbosity=detailed"

# Gerar relatório de cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### 4.2 Passo 2: Execução da Suíte de Testes BDD Automatizados (Cucumber)

Os testes BDD executam cenários de negócios descritos em Gherkin validados diretamente contra a API REST em execução.

**Pré-requisito:** Iniciar o backend da API no terminal 1:
```bash
# Iniciar a API Web do PotentialWind
dotnet run --project backend/PotentialWind.Api
```

No terminal 2 (execução dos cenários Cucumber):
```bash
# Navegar até o diretório de testes BDD
cd tests/bdd

# Instalar dependências (caso seja a primeira execução)
npm install

# Executar todos os 5 cenários BDD em Português
npm test

# Executar apenas cenários marcados com a tag @smoke
npx cucumber-js --tags "@smoke"

# Executar apenas cenários do módulo de Weibull
npx cucumber-js --tags "@weibull"
```

### 4.3 Passo 3: Verificação do Frontend Angular

```bash
# Navegar até o diretório do frontend
cd frontend

# Verificar se há erros de tipagem TypeScript
npx tsc --noEmit

# Compilar a aplicação em modo de produção
npm run build
```

---

## 5. Critérios de Aceite e Regras de Bloqueio

1. **Critério de Aprovação de Build:**
   - **100% dos testes unitários (16/16)** e **100% dos cenários BDD (5/5)** devem passar sem nenhuma falha.
2. **Regra de Bloqueio de Merge/Deploy:**
   - Nenhum Pull Request ou commit pode ser integrado na branch principal (`main`) caso haja qualquer falha em testes unitários ou BDD.
3. **Métrica de Cobertura de Código Mínima:**
   - Mínimo de **90% de cobertura de código** nos serviços de cálculo matemático e financeiro (`PotentialWind.Api.Services`).

---

## 6. Papéis e Responsabilidades

- **Desenvolvedor:** Responsável por executar `dotnet test` e escrever novos testes unitários para cada nova funcionalidade desenvolvida.
- **Engenheiro de QA / Automação:** Responsável por manter as especificações Gherkin (`.feature`) e implementar os *Step Definitions* no Cucumber.
- **DevOps / CI-CD:** Responsável por garantir a execução automática dos testes no pipeline a cada push ou pull request.

---
*Plano de Execução de Testes criado para a plataforma **PotentialWind**.*

# 🥒 Plano de Testes Automatizados com Cucumber e BDD — PotentialWind 🌬️⚡

## 1. Visão Geral

Este documento apresenta o plano de implementação de **Testes Automatizados de Aceitação BDD (Behavior-Driven Development)** utilizando **Cucumber** no ecossistema da aplicação **PotentialWind**.

O uso do Cucumber visa aproximar as especificações técnicas e de engenharia eólica (cálculos de Weibull, perdas por esteira, LCOE, VPL, TIR e simulações) do entendimento funcional e de negócios, permitindo que engenheiros eólicos, desenvolvedores e QA validem as funcionalidades da plataforma através de cenários descritos em linguagem natural com sintaxe **Gherkin**.

---

## 2. Arquitetura da Solução BDD

```text
PotentialWind/
├── tests/
│   └── bdd/
│       ├── features/                         # Arquivos de especificação Gherkin (.feature)
│       │   ├── calculo_weibull.feature
│       │   ├── modelo_esteira_jensen.feature
│       │   ├── simulacao_parque_eolico.feature
│       │   └── viabilidade_financeira_lcoe.feature
│       ├── step_definitions/                 # Passos automatizados (Step Definitions)
│       │   ├── calculo_weibull_steps.js
│       │   ├── modelo_esteira_steps.js
│       │   └── simulacao_parque_steps.js
│       ├── support/                          # Hooks de setup, cliente HTTP e suporte
│       │   └── world.js
│       ├── cucumber.json                     # Configuração de relatórios e tags do Cucumber
│       └── package.json                      # Dependências (@cucumber/cucumber, axios)
```

### Tecnologias Utilizadas:
- **Cucumber.js (`@cucumber/cucumber`):** Executor oficial do Cucumber para testes BDD em Node.js.
- **Axios / Node-Fetch:** Cliente HTTP para automação de testes de API REST BDD.
- **Linguagem Gherkin (Português - `# language: pt`):** Palavras-chave: `Funcionalidade`, `Cenário`, `Esquema do Cenário`, `Dado`, `Quando`, `Então`, `E`.

---

## 3. Mapeamento das Funcionalidades e Cenários BDD

### 3.1 Módulo: Análise Rápida de Potencial Eólico (Weibull & Cisalhamento)
- **Cenário 1:** Extrapolação vertical de velocidade de vento pela Lei de Potência de Hellmann.
- **Cenário 2:** Estimativa dos parâmetros de forma $k$ e escala $c$ da distribuição de Weibull.
- **Cenário 3:** Classificação do recurso eólico conforme padrão IEC 61400 / NREL.

### 3.2 Módulo: Modelo Linear de Esteira de Jensen (Park Model)
- **Cenário 1:** Cálculo de perda de velocidade do vento e porcentagem de perda por esteira em função do espaçamento $D$.
- **Cenário 2:** Variação de perdas entre parques onshore e offshore.

### 3.3 Módulo: Simulação de Parque Eólico & Viabilidade Econômica
- **Cenário 1:** Simulação completa de parque eólico calculando Gross AEP, Net AEP e Fator de Capacidade.
- **Cenário 2:** Cálculo dos indicadores econômicos LCOE, VPL (NPV) e TIR (IRR) para parque viável.
- **Cenário 3:** Estimativa dos indicadores ESG (Ton de $CO_2$ evitadas e residências alimentadas).

---

## 4. Marcação por Tags (Tags Strategy)

| Tag | Descrição |
| :--- | :--- |
| `@weibull` | Testes focados nos cálculos estatísticos da distribuição de Weibull. |
| `@jensen` | Testes do modelo de esteira de Jensen. |
| `@financeiro` | Testes de viabilidade econômica, LCOE e VPL. |
| `@simulacao` | Testes da simulação completa do parque eólico. |
| `@smoke` | Cenários críticos para validação rápida no pipeline CI/CD. |

---

## 5. Como Executar os Testes Cucumber

```bash
# Navegar até a pasta dos testes BDD
cd tests/bdd

# Instalar dependências do Cucumber
npm install

# Executar todos os testes BDD com Cucumber
npm test

# Executar cenários específicos via Tags (exemplo: @weibull)
npx cucumber-js --tags "@weibull"
```

---
*Plano de automação BDD com Cucumber elaborado para o projeto **PotentialWind**.*

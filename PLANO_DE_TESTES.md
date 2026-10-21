# 🧪 Plano de Testes Unitários — PotentialWind 🌬️⚡

## 1. Visão Geral e Objetivos

O **PotentialWind** é uma solução completa de engenharia eólica para avaliação de viabilidade técnica, energética e financeira de parques eólicos onshore e offshore.

Este **Plano de Testes Unitários** estabelece a estratégia, a arquitetura de testes, o mapeamento detalhado dos cenários de teste, os critérios de aceite e as métricas de cobertura para garantir a confiabilidade matemática, física, financeira e operacional do sistema.

### Objetivos do Plano de Testes:
- **Precisão Científica e Matemática:** Garantir a exatidão das equações de Weibull, Função Gamma de Euler, Lei de Potência de Hellmann e Modelo de Esteira de Jensen.
- **Confiabilidade Financeira:** Validar as métricas de CAPEX, OPEX, LCOE, NPV (VPL), IRR (TIR), Payback Simples e estimativas ESG.
- **Robustez dos Serviços e Controllers:** Assegurar que os orquestradores de cálculo e endpoints REST tratem entradas válidas, nulas e limítrofes sem exceções não tratadas.
- **Integridade do Frontend Angular:** Assegurar que os componentes, formulários e serviços de API reajam adequadamente às interações do usuário e respostas da API.

---

## 2. Escopo e Pirâmide de Testes

### 2.1 Módulos no Escopo de Testes

```
                                  [ UI / E2E ]
                                 /            \
                                /  Frontend    \
                               /   Component    \
                              /     Tests        \
                             /--------------------\
                            /  Backend Controller  \
                           /    & Service Tests     \
                          /--------------------------\
                         /  Backend Unit Tests Math   \
                        /   & Financial Calculations   \
                       ----------------------------------
```

1. **Calculadores de Engenharia Eólica (`PotentialWind.Api.Services`):**
   - `WeibullCalculator`: Função Gamma, estimativa de parâmetros $k$ e $c$, densidade de potência, cisalhamento vertical e classificação IEC.
   - `WakeLossCalculator`: Perdas por efeito esteira (Modelo de Jensen) e estimativa de footprint de área territorial.
   - `FinancialCalculator`: CAPEX, OPEX, Receita Anual, LCOE, VPL/NPV, TIR/IRR, Payback e indicadores ESG ($CO_2$ evitado, residências alimentadas).
   - `WindCalculationService`: Análise rápida de Weibull, simulação completa de parque eólico, discretização em bins e perfil mensal de geração.

2. **Controladores Web API (`PotentialWind.Api.Controllers`):**
   - `WindCalculationsController`: Validação dos endpoints POST para cálculo rápido Weibull e simulação de parque.
   - `WindTurbinesController`: Operações CRUD e consulta de curvas de potência de aerogeradores.
   - `WindFarmsController`: Gestão e simulações de parques eólicos.
   - `WindMeasurementsController`: Processamento de medições anemométricas e rosa dos ventos.
   - `DatabaseController`: Status da base de dados e conectividade SQLite/MySQL.

3. **Frontend Angular (`frontend/src/app`):**
   - `WindApiService`: Integração via HTTP client para Weibull, parques, turbinas e medições.
   - Components Standalone: `WindCalculatorComponent`, `FarmDesignerComponent`, `EconomicFeasibilityComponent`, `TurbineCatalogComponent`, `MeasurementAnalyzerComponent`.

---

## 3. Matriz e Casos de Teste Detalhados

### 3.1 Módulo: Calculador de Weibull e Perfil Vertical (`WeibullCalculator`)

| ID Caso | Componente / Função | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-WBL-01** | `Gamma(x)` | $x \in \{1.0, 2.0, 3.0, 4.0, 0.5\}$ | Retornar valores conhecidos da Função Gamma de Euler: $\Gamma(1)=1, \Gamma(2)=1, \Gamma(3)=2, \Gamma(4)=6, \Gamma(0.5)=\sqrt{\pi} \approx 1.77245$. | Alta (Crítico) |
| **UT-WBL-02** | `EstimateFromMeanAndStdDev` | Velocidade Média = 8.5 m/s, Desvio Padrão = 3.8 m/s | Retornar fator de forma $k \in [1.5, 3.0]$ e fator de escala $c > \text{média}$. | Alta |
| **UT-WBL-03** | `CalculatePowerDensity` | $k = 2.0$, $c_1 = 7.0$ m/s, $c_2 = 10.0$ m/s | Densidade de potência com $c_2$ deve ser no mínimo $2\times$ maior que com $c_1$ (proporcional a $c^3$). | Alta |
| **UT-WBL-04** | `ExtrapolateHeightPowerLaw` | $v_{50m} = 8.0$ m/s, $h_{hub} = 120$ m, $\alpha = 0.14$ | $v_{120m} \approx 9.04$ m/s ($v_{hub} > v_{ref}$). | Alta |
| **UT-WBL-05** | `ClassifyWindResource` | Múltiplas combinações de Densidade de Potência e Velocidade | Classificar corretamente conforme IEC 61400 / NREL (Classes 1 a 7). | Média |
| **UT-WBL-06** | `CumulativeProbability` | $v = 0$, $v = c$, $v = \infty$ | $P(v \le 0) = 0$, $P(v \le c) \approx 0.6321$ ($1 - e^{-1}$), $P(v \to \infty) = 1.0$. | Alta |
| **UT-WBL-07** | `ProbabilityDensity` | $v < 0$ | Retornar 0.0 sem estourar exceção. | Média |

### 3.2 Módulo: Calculador de Esteira e Área (`WakeLossCalculator`)

| ID Caso | Componente / Função | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-WKE-01** | `EstimateWakeLossPercentage` | Espaçamento em diâmetros $S_D = 4.0$ vs $S_D = 8.0$, 20 turbinas | Espaçamento menor ($4D$) deve resultar em perdas por esteira estritamente maiores que $8D$. | Alta |
| **UT-WKE-02** | `EstimateWakeLossPercentage` | N° Turbinas = 1 vs N° Turbinas = 50 | Para 1 turbina isolated, perda de esteira deve ser 0%. | Alta |
| **UT-WKE-03** | `EstimateRequiredLandArea` | 10 turbinas, Rotor = 150m, Espaçamento = 5D | Calcular $km^2$ e Hectares positivos com proporção exata ($1 km^2 = 100 ha$). | Média |

### 3.3 Módulo: Calculador de Viabilidade Financeira (`FinancialCalculator`)

| ID Caso | Componente / Função | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-FIN-01** | `Calculate` (Parque Viável) | 100MW, AEP Líquido = 420.000 MWh, CAPEX/MW = R$ 5M, Tarifa = R$ 220/MWh, Taxa de Desconto = 8.5% | NPV > 0, IRR > Taxa de Desconto (8.5%), Payback entre 4 e 10 anos, LCOE coerente (R$ 80 - 180 / MWh). | Alta (Crítico) |
| **UT-FIN-02** | `Calculate` (Métricas ESG) | AEP Líquido = 420.000 MWh | Mão de obra / Famílias atendidas > 100.000, Tons de $CO_2$ evitadas $> 0$. | Média |
| **UT-FIN-03** | `Calculate` (Fluxo de Caixa) | Vida útil = 25 anos | Retornar exatamente 25 anos de projeção financeira no fluxo de caixa. | Média |
| **UT-FIN-04** | `Calculate` (Parque Inviável) | Preço da energia R$ 10/MWh, CAPEX R$ 20M/MW | NPV < 0 e Payback excedendo o tempo de vida do projeto. | Alta |

### 3.4 Módulo: Serviço Orquestrador de Cálculos (`WindCalculationService`)

| ID Caso | Componente / Função | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-SVC-01** | `AnalyzeQuickPotentialAsync` | Média = 8.0 m/s, $h_{ref}=50$m, $h_{target}=120$m | Retornar DTO de análise rápida com velocidade extrapolada, Weibull $k$ e $c$, 50 bins de velocidade e classificação. | Alta |
| **UT-SVC-02** | `AnalyzeQuickPotentialAsync` | Parâmetros com WeibullK informado explicitamente ($k=2.4$) | Respeitar o $k$ customizado e ajustar $c$ coerentemente. | Alta |
| **UT-SVC-03** | `ComputeWindDistributionBins` | Weibull $k=2.0$, $c=9.0$, Curva de Potência da Vestas V162 | Bins de 0.5 a 25.0 m/s totalizando ~8760 horas no ano e somatório de energia condizente. | Alta |
| **UT-SVC-04** | `ComputeMonthlyProduction` | AEP Bruto = 100.000 MWh, Fator de Perdas = 15%, Tarifa = R$ 200/MWh | Retornar 12 meses, pico no 2° semestre (padrão Brasil), somatório de energia bruta igual a 100.000 MWh. | Média |
| **UT-SVC-05** | `RunWindFarmSimulationAsync` | ID de Parque Existente no DB InMemory | Executar a simulação completa, aplicar modelo de esteira, calcular financeiro e persistir `WindSimulationResult` no DB. | Alta |

### 3.5 Módulo: Controllers REST (`WindCalculationsController` & `WindTurbinesController`)

| ID Caso | Componente / Função | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-CTR-01** | `WindCalculationsController.AnalyzeQuick` | Objeto `QuickWeibullAnalysisRequest` válido | Retornar `200 OK` com payload preenchido. | Alta |
| **UT-CTR-02** | `WindCalculationsController.AnalyzeQuick` | Requisição nula / inválida | Retornar `400 Bad Request`. | Alta |
| **UT-CTR-03** | `WindTurbinesController.GetAll` | Consulta ao catálogo de aerogeradores | Retornar `200 OK` com lista contendo os modelos comerciais do seed (Vestas, SG, GE, Nordex, Goldwind). | Alta |
| **UT-CTR-04** | `WindTurbinesController.GetById` | ID inexistente | Retornar `404 Not Found`. | Média |

### 3.6 Módulo: Frontend Angular (Services & Components)

| ID Caso | Componente / Serviço | Cenário / Entrada | Resultado Esperado | Prioridade |
| :--- | :--- | :--- | :--- | :--- |
| **UT-FE-01** | `WindApiService` | Chamada ao método `getTurbines()` | Realizar requisição HTTP GET para `/api/windturbines` e retornar Observable de `WindTurbine[]`. | Alta |
| **UT-FE-02** | `WindApiService` | Chamada ao método `analyzeQuickPotential(req)` | Realizar requisição HTTP POST com payload DTO e retornar a análise tratada. | Alta |
| **UT-FE-03** | `WindCalculatorComponent` | Submissão do formulário de Weibull | Chamar o serviço `WindApiService` e renderizar o gráfico de distribuição de Weibull e Bins. | Alta |
| **UT-FE-04** | `FarmDesignerComponent` | Alteração do número de turbinas ou espaçamento $D$ | Recalcular automaticamente as perdas por esteira e atualizar a estimativa territorial. | Média |

---

## 4. Métricas de Cobertura e Estratégia de Execução

### 4.1 Metas de Cobertura de Código (Code Coverage Target)
- **Serviços de Engenharia & Cálculos Matémáticos:** $\ge 95\%$ de cobertura de linhas e branches.
- **Serviços de Aplicação (`WindCalculationService`):** $\ge 90\%$ de cobertura.
- **Controllers Web API:** $\ge 85\%$ de cobertura.
- **Frontend Core Services & Utilities:** $\ge 85\%$ de cobertura.

### 4.2 Ferramentas e Frameworks Utilizados
- **Backend (.NET 10 C#):**
  - **xUnit:** Framework para asserções e estrutura de execução de testes unitários.
  - **Microsoft.EntityFrameworkCore.InMemory:** Banco de dados em memória para testes de repositório e contexto EF Core.
  - **Moq:** Framework de mocks para isolamento de dependências.
  - **Coverlet:** Coletor de cobertura de código (`coverlet.collector`).
- **Frontend (Angular 22 / TypeScript):**
  - **Jasmine / Karma / Vitest:** Frameworks de testes e runner de especificação para componentes e serviços Angular.

---

## 5. Pipeline de Execução de Testes

Para executar toda a suíte de testes unitários do backend via linha de comando no ambiente do projeto:

```bash
# Execução padrão de todos os testes unitários
dotnet test

# Execução com relatório detalhado de cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

---
*Documento elaborado para o projeto **PotentialWind**.*

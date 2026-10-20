# PotentialWind 🌬️⚡

> **Sistema de Cálculo e Análise de Potencial Energético para Construção de Parques Eólicos**

O **PotentialWind** é uma aplicação completa de engenharia eólica para avaliação de viabilidade técnica, energética e financeira de parques eólicos onshore e offshore. O projeto combina cálculos matemáticos e anemométricos rigorosos no backend em **C# (.NET 9)** com uma interface moderna e interativa em **Angular 22**.

---

## 🚀 Funcionalidades Principais

1. **Calculadora Estatística de Weibull e Perfil Vertical (Wind Shear):**
   - Ajuste da Distribuição de Weibull de 2 parâmetros ($k$ = fator de forma, $c$ = fator de escala em m/s).
   - Extrapolação vertical de velocidade do vento pela **Lei de Potência de Hellmann** ($v(h) = v_{ref} \cdot (h / h_{ref})^\alpha$) e Lei Logarítmica.
   - Cálculo da **Densidade de Potência Eólica** ($W/m^2$): $P_d = \frac{1}{2}\rho c^3 \Gamma(1 + 3/k)$.
   - Classificação do recurso eólico de acordo com os padrões **IEC 61400** e **NREL** (Classes 1 a 7).
   - Discretização em classes de velocidade (*wind bins*) e integração numérica para estimativa de geração.

2. **Projetista e Layout de Parques Eólicos (Farm Designer):**
   - Dimensionamento de turbinas eólicas e arranjo espacial (múltiplos de diâmetro de rotor $D$).
   - Estimativa de perdas por **Efeito Esteira (Wake Effect Losses)** com aproximação linear do modelo de **Jensen (Park Model)**.
   - Balanço global de perdas: Esteira, Perdas Elétricas (cabos e transformadores), Indisponibilidade Técnica e Degradação Ambiental.
   - Cálculo da Produção Anual de Energia Bruta e Líquida (**Gross & Net AEP** em MWh/ano) e **Fator de Capacidade (%)**.
   - Estimativa da área territorial ocupada ($km^2$ e hectares).

3. **Catálogo de Aerogeradores Comerciais:**
   - Modelos de ponta pré-cadastrados (Vestas V162-6.2MW, Siemens Gamesa SG 5.0-145, GE Vernova Cypress 5.3-158, Nordex N163/5.X, Goldwind GW155-4.5MW).
   - Curvas de potência $P(v)$ interativas com velocidades de partida (*cut-in*), potência nominal (*rated*) e corte de segurança (*cut-out*).
   - Cadastro dinâmico de novos modelos personalizados.

4. **Tratamento de Dados Anemométricos & Rosa dos Ventos:**
   - Ingestão e análise de séries temporais de dados de velocidade, direção, pressão e temperatura.
   - **Rosa dos Ventos Interativa (Wind Rose)** com 16 setores direcionais em coordenadas polares (SVG).
   - Ajuste empírico de Weibull a partir da média e desvio padrão amostrais (Método de Justus).

5. **Análise de Viabilidade Econômica & LCOE:**
   - Custo Nivelado da Energia (**LCOE** em R$/MWh).
   - Valor Presente Líquido (**VPL / NPV**) e Taxa Interna de Retorno (**TIR / IRR**).
   - *Payback* Simples e Fluxo de Caixa Anual.
   - Geração sazonal mensal (perfil anual de 12 meses com curva típica brasileira de ventos alísios).
   - Indicadores ESG: Toneladas de $CO_2$ evitadas ao ano e famílias atendidas.

6. **Persistência Híbrida: SQLite com Migração Transparente para MySQL:**
   - Inicialmente opera com **SQLite** local (`potentialwind.db`), sem necessidade de instalar SGBDs externos para testes ou desenvolvimento.
   - Estruturado com Entity Framework Core 9 e `Pomelo.EntityFrameworkCore.MySql` para migração direta e transparente para **MySQL / MariaDB** alterando apenas uma chave de configuração.

---

## 🛠️ Tecnologias Utilizadas

- **Backend:**
  - C# .NET 9 Web API
  - Entity Framework Core 9 (Code-First)
  - Microsoft.EntityFrameworkCore.Sqlite
  - Pomelo.EntityFrameworkCore.MySql
  - Swashbuckle / OpenAPI (Swagger)
  - Testes Unitários com xUnit
- **Frontend:**
  - Angular 22 (Standalone Components)
  - TypeScript 6 & RxJS
  - SCSS modular responsivo com tema de energias renováveis
  - Gráficos vetoriais interativos em SVG puro (Weibull, Curvas de Potência, Rosa dos Ventos Polar e Barras Mensais)
- **Banco de Dados:**
  - SQLite (padrão)
  - MySQL / MariaDB (suportado nativamente)

---

## 📁 Estrutura do Repositório

```text
PotentialWind/
├── backend/
│   ├── PotentialWind.Api/
│   │   ├── Controllers/
│   │   │   ├── WindCalculationsController.cs  # Weibull, AEP, perdas de esteira e cisalhamento
│   │   │   ├── WindFarmsController.cs         # CRUD e simulações de parques eólicos
│   │   │   ├── WindTurbinesController.cs      # Catálogo de aerogeradores e curvas P(v)
│   │   │   ├── WindMeasurementsController.cs  # Dados anemométricos e rosa dos ventos
│   │   │   └── DatabaseController.cs          # Status do banco e guia de migração
│   │   ├── Models/
│   │   │   ├── WindFarm.cs
│   │   │   ├── WindTurbine.cs
│   │   │   ├── WindMeasurement.cs
│   │   │   ├── WindSimulationResult.cs
│   │   │   └── DTOs/WindCalculationDtos.cs
│   │   ├── Services/
│   │   │   ├── WeibullCalculator.cs           # Funções matemáticas (Gamma, Weibull, Shear)
│   │   │   ├── WakeLossCalculator.cs          # Modelo linear de Jensen (esteira)
│   │   │   ├── FinancialCalculator.cs         # LCOE, NPV, IRR, Payback e ESG
│   │   │   └── WindCalculationService.cs      # Orquestrador de simulações
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── DatabaseProviderExtensions.cs  # Alternância transparente SQLite/MySQL
│   │   │   └── SeedData.cs                    # Carga inicial com parques, turbinas e medições
│   │   ├── appsettings.json
│   │   └── Program.cs
│   └── PotentialWind.Tests/                   # Testes unitários de engenharia eólica
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── navbar/                    # Barra de navegação e indicador de banco
│   │   │   │   ├── dashboard/                 # Visão geral de projetos e KPIs
│   │   │   │   ├── wind-calculator/           # Calculadora interativa Weibull e Bins
│   │   │   │   ├── farm-designer/             # Layout de parque, perdas e grid visual
│   │   │   │   ├── turbine-catalog/           # Modelos de turbinas e curva de potência
│   │   │   │   ├── measurement-analyzer/      # Rosa dos ventos e séries temporais
│   │   │   │   ├── economic-feasibility/      # Viabilidade financeira e LCOE
│   │   │   │   └── database-migration/        # Painel de persistência e migração
│   │   │   └── core/
│   │   │       ├── models/wind.models.ts
│   │   │       └── services/wind-api.service.ts
│   │   └── styles.scss
├── PotentialWind.sln
├── .gitignore
└── README.md
```

---

## 🏃 Como Executar o Projeto

### Pré-requisitos
- [.NET 9 ou .NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (versão 20+) e npm](https://nodejs.org/)

### 1. Executando o Backend (.NET)

Abra um terminal no diretório raiz do projeto:

```bash
# Restaurar dependências e compilar a solução
dotnet build

# Executar os testes unitários
dotnet test

# Iniciar a API Web
dotnet run --project backend/PotentialWind.Api
```

- A API estará disponível em: `http://localhost:5000` (e `http://localhost:5273`)
- A documentação interativa **Swagger** estará disponível na raiz: `http://localhost:5000`

> **Nota:** Ao iniciar pela primeira vez, o `SeedData` criará automaticamente o banco de dados `potentialwind.db` (SQLite) com modelos de aerogeradores comerciais, dois parques eólicos de referência e séries temporais de medições.

### 2. Executando o Frontend (Angular)

Em outro terminal, navegue até a pasta `frontend`:

```bash
cd frontend

# Iniciar o servidor de desenvolvimento Angular
npm start
```

- Acesse a aplicação no seu navegador: `http://localhost:4200`

---

## 🗄️ Estrutura de Banco de Dados: SQLite & Migração para MySQL

A aplicação utiliza o padrão Provider Factory no EF Core (`DatabaseProviderExtensions.cs`), permitindo alternar de banco sem alterar uma única linha de código C#.

### Configuração Atual (Padrão: SQLite)
No arquivo `backend/PotentialWind.Api/appsettings.json`:
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=potentialwind.db"
  }
}
```

### Como Migrar para o MySQL:
1. Tenha uma instância do MySQL ou MariaDB em execução (localmente ou em nuvem).
2. Crie a base de dados:
   ```sql
   CREATE DATABASE potentialwind CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```
3. No arquivo `backend/PotentialWind.Api/appsettings.json`, altere o provedor e a string de conexão:
   ```json
   {
     "DatabaseProvider": "MySql",
     "ConnectionStrings": {
       "SqliteConnection": "Data Source=potentialwind.db",
       "MySqlConnection": "Server=localhost;Port=3306;Database=potentialwind;User=root;Password=sua_senha;CharSet=utf8mb4;"
     }
   }
   ```
4. Reinicie o backend (`dotnet run --project backend/PotentialWind.Api`).
5. O sistema detectará o MySQL automaticamente e executará o provisionamento e *seed* das tabelas na nova base.

---

## 📤 Publicando no seu GitHub (`PotentialWind`)

Como o repositório Git local já está devidamente inicializado e comitados todos os arquivos, siga os passos abaixo para publicar no seu GitHub:

1. Acesse o seu GitHub e crie um novo repositório com o nome **`PotentialWind`** (sem inicializar com README ou .gitignore, pois já criamos localmente).
2. No seu terminal, vincule o repositório remoto e faça o push:

```bash
# Adicionar o repositório remoto do seu GitHub
git remote add origin https://github.com/SEU_USUARIO/PotentialWind.git

# Enviar o código para a branch main
git push -u origin main
```

---

## 🧪 Fórmulas e Modelos Científicos Implementados

- **Função Gamma de Euler:**
  Aproximação analítica de Lanczos para cálculo exato de $\Gamma(1 + 1/k)$ e $\Gamma(1 + 3/k)$.
- **Distribuição de Weibull:**
  $$f(v) = \frac{k}{c} \left(\frac{v}{c}\right)^{k-1} \exp\left(-\left(\frac{v}{c}\right)^k\right)$$
- **Lei de Potência de Cisalhamento de Hellmann:**
  $$v(h) = v_{ref} \left(\frac{h}{h_{ref}}\right)^\alpha$$
- **Modelo Linear de Esteira de Jensen (Park Model):**
  $$\frac{\Delta v}{v_0} = \frac{1 - \sqrt{1 - C_t}}{(1 + 2 k_w \cdot S_D)^2}$$
- **Custo Nivelado da Energia (LCOE):**
  $$\text{LCOE} = \frac{\text{CAPEX} + \sum_{t=1}^N \frac{\text{OPEX}_t}{(1+r)^t}}{\sum_{t=1}^N \frac{\text{AEP Líquido}_t}{(1+r)^t}}$$

---

Desenvolvido para engenharia e planejamento de parques eólicos modernos.

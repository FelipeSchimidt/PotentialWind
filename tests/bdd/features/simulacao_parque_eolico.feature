# language: pt
@simulacao @financeiro
Funcionalidade: Simulação Completa e Viabilidade Financeira de Parque Eólico
  Como um analista financeiro e de engenharia
  Eu quero simular a produção de energia e os indicadores financeiros de um parque
  Para determinar o LCOE, VPL, TIR e impacto ambiental ESG do empreendimento

  @smoke
  Cenário: Simulação de parque eólico viável com indicadores positivos
    Dado que existe um parque eólico cadastrado no sistema
    Quando eu executo a simulação completa do parque com 20 turbinas
    Então a produção de energia líquida (Net AEP) deve ser menor que a produção bruta (Gross AEP)
    E o Fator de Capacidade do parque deve ser superior a 30%
    E o Valor Presente Líquido (VPL) do projeto deve ser maior que zero
    E o Custo Nivelado de Energia (LCOE) deve estar compreendido entre R$ 80 e R$ 250 por MWh
    E o número estimado de famílias atendidas deve ser maior que 50.000

# language: pt
@weibull
Funcionalidade: Calculadora e Análise Estatística de Weibull
  Como um engenheiro eólico
  Eu quero calcular a extrapolação da velocidade do vento e ajustar a distribuição de Weibull
  Para avaliar o potencial energético de um determinado sítio eólico

  @smoke
  Cenário: Extrapolação vertical de velocidade do vento pela Lei de Potência de Hellmann
    Dado que a velocidade do vento medida na altura de referência de 50 metros é 8.5 m/s
    Quando eu solicito a análise de potencial para a altura do cubo do rotor de 120 metros
    Então a velocidade do vento extrapolada deve ser superior a 9.5 m/s
    E o fator de escala Weibull C calculado deve ser superior a 10.0 m/s
    E a densidade de potência eólica deve ser classificada como "Classe 7 - Extraordinário"

  Cenário: Discretização da distribuição em bins de velocidade
    Dado que a velocidade média de vento é 8.0 m/s na altura de 50m
    Quando eu solicito a análise rápida de Weibull
    Então o sistema deve retornar exatamente 50 bins de velocidade entre 0.5 e 25.0 m/s
    E a soma do número de horas por ano de todos os bins deve ser aproximadamente 8760 horas

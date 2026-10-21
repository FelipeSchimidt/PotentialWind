# language: pt
@jensen
Funcionalidade: Estimativa de Perdas por Efeito Esteira (Jensen Park Model)
  Como um projetista de parques eólicos
  Eu quero estimar as perdas por interferência de esteira entre aerogeradores
  Para otimizar o layout do parque e diminuir a degradação de geração

  @smoke
  Cenário: Perdas por esteira diminuem com o aumento do espaçamento entre turbinas
    Dado um arranjo com 20 turbinas eólicas
    Quando eu calculo a perda por esteira para um espaçamento apertado de 4.0 diâmetros
    E eu calculo a perda por esteira para um espaçamento amplo de 8.0 diâmetros
    Então a porcentagem de perda para 4.0 diâmetros deve ser estritamente maior que para 8.0 diâmetros

  Cenário: Comparação de perdas onshore e offshore
    Dado um parque eólico com 15 turbinas e espaçamento de 6.0 diâmetros
    Quando eu solicito a estimativa de perda por esteira onshore
    E eu solicito a estimativa de perda por esteira offshore
    Então a perda por esteira offshore deve ser menor devido ao menor coeficiente de expansão de esteira no mar

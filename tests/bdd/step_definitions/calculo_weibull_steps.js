const { Given, When, Then } = require('@cucumber/cucumber');
const assert = require('assert');

Given('que a velocidade do vento medida na altura de referência de {int} metros é {float} m\\/s', function (refHeight, speed) {
  this.contextData.refHeight = refHeight;
  this.contextData.refSpeed = speed;
});

When('eu solicito a análise de potencial para a altura do cubo do rotor de {int} metros', async function (targetHeight) {
  this.response = await this.client.post('/api/windcalculations/quick-analysis', {
    averageWindSpeed: this.contextData.refSpeed,
    referenceHeightM: this.contextData.refHeight,
    targetHeightM: targetHeight
  });
});

Then('a velocidade do vento extrapolada deve ser superior a {float} m\\/s', function (minExtrapolatedSpeed) {
  assert.strictEqual(this.response.status, 200);
  assert.ok(
    this.response.data.extrapolatedSpeedMs > minExtrapolatedSpeed,
    `Esperado > ${minExtrapolatedSpeed}, obtido ${this.response.data.extrapolatedSpeedMs}`
  );
});

Then('o fator de escala Weibull C calculado deve ser superior a {float} m\\/s', function (minC) {
  assert.ok(
    this.response.data.weibullC > minC,
    `Esperado Weibull C > ${minC}, obtido ${this.response.data.weibullC}`
  );
});

Then('a densidade de potência eólica deve ser classificada como {string}', function (expectedClass) {
  assert.strictEqual(this.response.data.windClass, expectedClass);
});

Given('que a velocidade média de vento é {float} m\\/s na altura de {int}m', function (speed, refHeight) {
  this.contextData.refSpeed = speed;
  this.contextData.refHeight = refHeight;
});

When('eu solicito a análise rápida de Weibull', async function () {
  this.response = await this.client.post('/api/windcalculations/quick-analysis', {
    averageWindSpeed: this.contextData.refSpeed,
    referenceHeightM: this.contextData.refHeight,
    targetHeightM: 120
  });
});

Then('o sistema deve retornar exatamente {int} bins de velocidade entre {float} e {float} m\\/s', function (count, minV, maxV) {
  assert.strictEqual(this.response.status, 200);
  assert.strictEqual(this.response.data.windBins.length, count);
  assert.strictEqual(this.response.data.windBins[0].windSpeed, minV);
  assert.strictEqual(this.response.data.windBins[count - 1].windSpeed, maxV);
});

Then('a soma do número de horas por ano de todos os bins deve ser aproximadamente {int} horas', function (expectedHours) {
  const totalHours = this.response.data.windBins.reduce((acc, bin) => acc + bin.hoursPerYear, 0);
  assert.ok(
    Math.abs(totalHours - expectedHours) < 50,
    `Esperado aproximadamente ${expectedHours} horas, obtido ${totalHours}`
  );
});

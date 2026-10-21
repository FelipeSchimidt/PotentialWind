const { Given, When, Then } = require('@cucumber/cucumber');
const assert = require('assert');

Given('um arranjo com {int} turbinas eólicas', function (count) {
  this.contextData.turbineCount = count;
});

When('eu calculo a perda por esteira para um espaçamento apertado de {float} diâmetros', async function (spacingD) {
  const res = await this.client.get(`/api/windcalculations/wake-loss?spacingD=${spacingD}&turbineCount=${this.contextData.turbineCount}`);
  this.contextData.tightWakeLoss = res.data.wakeLossPercent;
});

When('eu calculo a perda por esteira para um espaçamento amplo de {float} diâmetros', async function (spacingD) {
  const res = await this.client.get(`/api/windcalculations/wake-loss?spacingD=${spacingD}&turbineCount=${this.contextData.turbineCount}`);
  this.contextData.wideWakeLoss = res.data.wakeLossPercent;
});

Then('a porcentagem de perda para {float} diâmetros deve ser estritamente maior que para {float} diâmetros', function (tightD, wideD) {
  assert.ok(
    this.contextData.tightWakeLoss > this.contextData.wideWakeLoss,
    `Perda para ${tightD}D (${this.contextData.tightWakeLoss}%) deveria ser maior que para ${wideD}D (${this.contextData.wideWakeLoss}%)`
  );
});

Given('um parque eólico com {int} turbinas e espaçamento de {float} diâmetros', function (count, spacingD) {
  this.contextData.turbineCount = count;
  this.contextData.spacingD = spacingD;
});

When('eu solicito a estimativa de perda por esteira onshore', async function () {
  const res = await this.client.get(`/api/windcalculations/wake-loss?spacingD=${this.contextData.spacingD}&turbineCount=${this.contextData.turbineCount}&isOffshore=false`);
  this.contextData.onshoreLoss = res.data.wakeLossPercent;
});

When('eu solicito a estimativa de perda por esteira offshore', async function () {
  const res = await this.client.get(`/api/windcalculations/wake-loss?spacingD=${this.contextData.spacingD}&turbineCount=${this.contextData.turbineCount}&isOffshore=true`);
  this.contextData.offshoreLoss = res.data.wakeLossPercent;
});

Then('a perda por esteira offshore deve ser menor devido ao menor coeficiente de expansão de esteira no mar', function () {
  assert.ok(
    this.contextData.offshoreLoss < this.contextData.onshoreLoss,
    `Perda offshore (${this.contextData.offshoreLoss}%) deveria ser menor que onshore (${this.contextData.onshoreLoss}%)`
  );
});

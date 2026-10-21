const { Given, When, Then } = require('@cucumber/cucumber');
const assert = require('assert');

Given('que existe um parque eólico cadastrado no sistema', function () {
  this.contextData.windFarmId = 1;
});

When('eu executo a simulação completa do parque com {int} turbinas', async function (turbineCount) {
  this.response = await this.client.post('/api/windcalculations/simulate-farm', {
    windFarmId: this.contextData.windFarmId,
    turbineCount: turbineCount
  });
});

Then('a produção de energia líquida \\(Net AEP) deve ser menor que a produção bruta \\(Gross AEP)', function () {
  assert.strictEqual(this.response.status, 200);
  const data = this.response.data;
  assert.ok(
    data.netAepMWh < data.grossAepMWh,
    `Net AEP (${data.netAepMWh}) deveria ser menor que Gross AEP (${data.grossAepMWh})`
  );
});

Then('o Fator de Capacidade do parque deve ser superior a {int}%', function (minCapFactor) {
  assert.ok(
    this.response.data.capacityFactorPercent > minCapFactor,
    `Fator de capacidade (${this.response.data.capacityFactorPercent}%) deveria ser > ${minCapFactor}%`
  );
});

Then('o Valor Presente Líquido \\(VPL) do projeto deve ser maior que zero', function () {
  assert.ok(
    this.response.data.npv > 0,
    `VPL/NPV (${this.response.data.npv}) deveria ser positivo`
  );
});

Then('o Custo Nivelado de Energia \\(LCOE) deve estar compreendido entre R$ {int} e R$ {int} por MWh', function (minLcoe, maxLcoe) {
  const lcoe = this.response.data.lcoePerMWh;
  assert.ok(
    lcoe >= minLcoe && lcoe <= maxLcoe,
    `LCOE (R$ ${lcoe}) deveria estar entre ${minLcoe} e ${maxLcoe}`
  );
});

Then('o número estimado de famílias atendidas deve ser maior que {float}', function (minFamilies) {
  assert.ok(
    this.response.data.homesPoweredEstimate > minFamilies,
    `Famílias atendidas (${this.response.data.homesPoweredEstimate}) deveria ser > ${minFamilies}`
  );
});

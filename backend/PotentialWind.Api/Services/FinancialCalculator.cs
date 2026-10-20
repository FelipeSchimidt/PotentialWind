namespace PotentialWind.Api.Services;

public static class FinancialCalculator
{
    public record FinancialResult(
        decimal TotalCapex,
        decimal AnnualOpex,
        decimal AnnualRevenue,
        decimal NetAnnualCashFlow,
        decimal LcoePerMWh,
        double SimplePaybackYears,
        decimal Npv,
        double IrrPercent,
        double Co2AvoidedTonsPerYear,
        int HomesPoweredEstimate
    );

    public static FinancialResult Calculate(
        double totalCapacityMW,
        double netAepMWh,
        decimal capexPerMW,
        double opexPercentOfCapex,
        decimal energyPriceMWh,
        double discountRatePercent,
        int lifespanYears = 25)
    {
        if (totalCapacityMW <= 0 || netAepMWh <= 0)
        {
            return new FinancialResult(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        decimal totalCapex = (decimal)totalCapacityMW * capexPerMW;
        decimal annualOpex = totalCapex * ((decimal)opexPercentOfCapex / 100m);
        decimal annualRevenue = (decimal)netAepMWh * energyPriceMWh;
        decimal netAnnualCashFlow = annualRevenue - annualOpex;

        // Payback simples
        double simplePayback = netAnnualCashFlow > 0 ? (double)(totalCapex / netAnnualCashFlow) : 999.0;
        if (simplePayback > lifespanYears) simplePayback = lifespanYears + 1.0;

        // Fator de desconto anual r
        double r = discountRatePercent / 100.0;

        // NPV (VPL) e LCOE
        // LCOE = (CAPEX + Sum(OPEX / (1+r)^t)) / Sum(NetAEP / (1+r)^t)
        double discountedOpexSum = 0.0;
        double discountedEnergySum = 0.0;
        double discountedCashFlowSum = 0.0;

        for (int t = 1; t <= lifespanYears; t++)
        {
            double discountFactor = Math.Pow(1.0 + r, t);
            discountedOpexSum += (double)annualOpex / discountFactor;
            discountedEnergySum += netAepMWh / discountFactor;
            discountedCashFlowSum += (double)netAnnualCashFlow / discountFactor;
        }

        decimal npv = (decimal)discountedCashFlowSum - totalCapex;

        decimal lcoe = 0m;
        if (discountedEnergySum > 0)
        {
            double totalDiscountedCosts = (double)totalCapex + discountedOpexSum;
            lcoe = (decimal)(totalDiscountedCosts / discountedEnergySum);
        }

        // TIR (IRR)
        double irr = CalculateIrr((double)totalCapex, (double)netAnnualCashFlow, lifespanYears);

        // Métricas ambientais e sociais
        // Fator de emissão médio do SIN brasileiro: aprox. 0.384 toneladas de CO2 / MWh de energia limpa gerada
        double co2AvoidedTons = netAepMWh * 0.384;

        // Consumo médio residencial anual brasileiro: aprox. 2.4 MWh/ano (200 kWh/mês)
        int homesPowered = (int)Math.Round(netAepMWh / 2.4);

        return new FinancialResult(
            TotalCapex: Math.Round(totalCapex, 2),
            AnnualOpex: Math.Round(annualOpex, 2),
            AnnualRevenue: Math.Round(annualRevenue, 2),
            NetAnnualCashFlow: Math.Round(netAnnualCashFlow, 2),
            LcoePerMWh: Math.Round(lcoe, 2),
            SimplePaybackYears: Math.Round(simplePayback, 1),
            Npv: Math.Round(npv, 2),
            IrrPercent: Math.Round(irr, 2),
            Co2AvoidedTonsPerYear: Math.Round(co2AvoidedTons, 1),
            HomesPoweredEstimate: homesPowered
        );
    }

    /// <summary>
    /// Calcula a Taxa Interna de Retorno (TIR / IRR) pelo método da bissecção
    /// </summary>
    private static double CalculateIrr(double initialInvestment, double netAnnualFlow, int years)
    {
        if (initialInvestment <= 0 || netAnnualFlow <= 0) return 0.0;
        if (netAnnualFlow * years <= initialInvestment) return -10.0; // Projeto não amortiza nem sem taxa

        double low = -0.50; // -50%
        double high = 1.00; // 100%
        double tolerance = 1e-5;

        for (int iter = 0; iter < 100; iter++)
        {
            double mid = (low + high) / 2.0;
            double npvMid = -initialInvestment;

            for (int t = 1; t <= years; t++)
            {
                npvMid += netAnnualFlow / Math.Pow(1.0 + mid, t);
            }

            if (Math.Abs(npvMid) < tolerance)
            {
                return mid * 100.0;
            }

            if (npvMid > 0)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        return ((low + high) / 2.0) * 100.0;
    }
}

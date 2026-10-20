export interface PowerCurvePoint {
  windSpeed: number;
  powerKW: number;
}

export interface WindTurbine {
  id: number;
  manufacturer: string;
  modelName: string;
  ratedPowerKW: number;
  rotorDiameterM: number;
  hubHeightM: number;
  cutInSpeed: number;
  ratedSpeed: number;
  cutOutSpeed: number;
  powerCurveJson: string;
  createdAt?: string;
  sweptAreaM2?: number;
}

export interface WindMeasurement {
  id: number;
  windFarmId: number;
  timestamp: string;
  windSpeed: number;
  windDirectionDeg: number;
  measurementHeightM: number;
  temperatureC?: number;
  pressureHPa?: number;
}

export interface WindBin {
  windSpeed: number;
  probabilityDensity: number;
  cumulativeProbability: number;
  hoursPerYear: number;
  turbinePowerKW: number;
  energyMWh: number;
}

export interface MonthlyProduction {
  month: number;
  monthName: string;
  averageSpeed: number;
  grossMWh: number;
  netMWh: number;
  revenue: number;
}

export interface WindSimulationResult {
  id: number;
  windFarmId: number;
  calculatedAt: string;
  averageWindSpeedAtHub: number;
  weibullK: number;
  weibullC: number;
  powerDensityWM2: number;
  windClass: string;
  totalCapacityMW: number;
  grossAepMWh: number;
  netAepMWh: number;
  capacityFactorPercent: number;
  fullLoadHours: number;
  annualRevenue: number;
  totalCapex: number;
  annualOpex: number;
  lcoePerMWh: number;
  simplePaybackYears: number;
  npv: number;
  irrPercent: number;
  co2AvoidedTonsPerYear: number;
  homesPoweredEstimate: number;
  windBinsJson: string;
  monthlyProductionJson: string;
}

export interface WindFarm {
  id: number;
  name: string;
  description: string;
  location: string;
  latitude: number;
  longitude: number;
  altitudeM: number;
  roughnessExponent: number;
  roughnessLengthZ0: number;
  airDensityKgM3: number;
  selectedTurbineId?: number;
  selectedTurbine?: WindTurbine;
  turbineCount: number;
  turbineSpacingD: number;
  wakeLossPercentage: number;
  electricalLossPercentage: number;
  availabilityLossPercentage: number;
  otherLossesPercentage: number;
  energyPriceMWh: number;
  capexPerMW: number;
  opexAnnualPercentCapex: number;
  discountRatePercent: number;
  projectLifespanYears: number;
  createdAt?: string;
  updatedAt?: string;
  totalCapacityMW?: number;
  measurementsCount?: number;
  latestSimulation?: WindSimulationResult;
}

export interface QuickAnalysisRequest {
  averageWindSpeed: number;
  standardDeviation?: number;
  weibullK?: number;
  weibullC?: number;
  referenceHeightM: number;
  targetHeightM: number;
  roughnessAlpha: number;
  airDensityKgM3: number;
  turbineId?: number;
}

export interface QuickAnalysisResponse {
  referenceHeightM: number;
  targetHeightM: number;
  referenceSpeedMs: number;
  extrapolatedSpeedMs: number;
  weibullK: number;
  weibullC: number;
  powerDensityWM2: number;
  windClass: string;
  recommendation: string;
  windBins: WindBin[];
  singleTurbineGrossAepMWh?: number;
  singleTurbineCapacityFactorPercent?: number;
}

export interface DatabaseStatus {
  currentProvider: string;
  connectionStringSample: string;
  canConnect: boolean;
  windFarmsCount: number;
  windTurbinesCount: number;
  measurementsCount: number;
  migrationNotes: string;
}

export interface MeasurementSummary {
  totalRecords: number;
  meanSpeed: number;
  minSpeed: number;
  maxSpeed: number;
  stdDev: number;
  weibullK: number;
  weibullC: number;
  powerDensity: number;
  windClass: string;
  recommendation: string;
  windRose: Array<{
    sector: string;
    angle: number;
    frequencyPercent: number;
    averageSpeed: number;
    count: number;
  }>;
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  WindFarm,
  WindTurbine,
  WindMeasurement,
  WindSimulationResult,
  QuickAnalysisRequest,
  QuickAnalysisResponse,
  DatabaseStatus,
  MeasurementSummary
} from '../models/wind.models';

@Injectable({
  providedIn: 'root'
})
export class WindApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api';

  // --- Parques Eólicos ---
  getWindFarms(): Observable<WindFarm[]> {
    return this.http.get<WindFarm[]>(`${this.baseUrl}/windfarms`);
  }

  getWindFarm(id: number): Observable<WindFarm> {
    return this.http.get<WindFarm>(`${this.baseUrl}/windfarms/${id}`);
  }

  createWindFarm(farm: Partial<WindFarm>): Observable<WindFarm> {
    return this.http.post<WindFarm>(`${this.baseUrl}/windfarms`, farm);
  }

  updateWindFarm(id: number, farm: Partial<WindFarm>): Observable<WindFarm> {
    return this.http.put<WindFarm>(`${this.baseUrl}/windfarms/${id}`, farm);
  }

  deleteWindFarm(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/windfarms/${id}`);
  }

  getFarmSimulations(farmId: number): Observable<WindSimulationResult[]> {
    return this.http.get<WindSimulationResult[]>(`${this.baseUrl}/windfarms/${farmId}/simulations`);
  }

  // --- Aerogeradores ---
  getTurbines(): Observable<WindTurbine[]> {
    return this.http.get<WindTurbine[]>(`${this.baseUrl}/windturbines`);
  }

  getTurbine(id: number): Observable<WindTurbine> {
    return this.http.get<WindTurbine>(`${this.baseUrl}/windturbines/${id}`);
  }

  createTurbine(turbine: Partial<WindTurbine>): Observable<WindTurbine> {
    return this.http.post<WindTurbine>(`${this.baseUrl}/windturbines`, turbine);
  }

  updateTurbine(id: number, turbine: Partial<WindTurbine>): Observable<WindTurbine> {
    return this.http.put<WindTurbine>(`${this.baseUrl}/windturbines/${id}`, turbine);
  }

  deleteTurbine(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/windturbines/${id}`);
  }

  // --- Medições e Rosa dos Ventos ---
  getMeasurements(windFarmId: number, limit: number = 100): Observable<WindMeasurement[]> {
    return this.http.get<WindMeasurement[]>(`${this.baseUrl}/windmeasurements?windFarmId=${windFarmId}&limit=${limit}`);
  }

  getMeasurementsSummary(windFarmId: number): Observable<MeasurementSummary> {
    return this.http.get<MeasurementSummary>(`${this.baseUrl}/windmeasurements/summary?windFarmId=${windFarmId}`);
  }

  importBatchMeasurements(windFarmId: number, points: any[], heightM: number = 50.0): Observable<any> {
    return this.http.post(`${this.baseUrl}/windmeasurements/batch`, {
      windFarmId,
      measurementHeightM: heightM,
      points
    });
  }

  // --- Cálculos e Simulações ---
  quickAnalysis(request: QuickAnalysisRequest): Observable<QuickAnalysisResponse> {
    return this.http.post<QuickAnalysisResponse>(`${this.baseUrl}/windcalculations/quick-analysis`, request);
  }

  simulateFarm(farmId: number, turbineId?: number, count?: number, overrideSpeed?: number): Observable<WindSimulationResult> {
    return this.http.post<WindSimulationResult>(`${this.baseUrl}/windcalculations/simulate-farm`, {
      windFarmId: farmId,
      turbineId,
      turbineCount: count,
      overrideAverageWindSpeed: overrideSpeed
    });
  }

  estimateWakeLoss(spacingD: number, turbineCount: number, isOffshore: boolean = false): Observable<{ wakeLossPercent: number }> {
    return this.http.get<{ wakeLossPercent: number }>(
      `${this.baseUrl}/windcalculations/wake-loss?spacingD=${spacingD}&turbineCount=${turbineCount}&isOffshore=${isOffshore}`
    );
  }

  extrapolateShear(speedRef: number, heightRef: number, heightTarget: number, roughnessAlpha: number = 0.14): Observable<any> {
    return this.http.get(
      `${this.baseUrl}/windcalculations/shear-extrapolation?speedRef=${speedRef}&heightRef=${heightRef}&heightTarget=${heightTarget}&roughnessAlpha=${roughnessAlpha}`
    );
  }

  // --- Banco de Dados e Migração ---
  getDatabaseStatus(): Observable<DatabaseStatus> {
    return this.http.get<DatabaseStatus>(`${this.baseUrl}/database/status`);
  }

  getMigrationGuide(): Observable<{ title: string; steps: string[] }> {
    return this.http.get<{ title: string; steps: string[] }>(`${this.baseUrl}/database/migration-guide`);
  }
}

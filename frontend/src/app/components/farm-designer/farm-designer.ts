import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindFarm, WindTurbine, WindSimulationResult } from '../../core/models/wind.models';

@Component({
  selector: 'app-farm-designer',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './farm-designer.html',
  styleUrls: ['./farm-designer.scss']
})
export class FarmDesignerComponent implements OnInit {
  private readonly api = inject(WindApiService);
  private readonly route = inject(ActivatedRoute);

  farms: WindFarm[] = [];
  turbines: WindTurbine[] = [];
  selectedFarmId: number | null = null;
  currentFarm: WindFarm | null = null;

  selectedTurbine: WindTurbine | null = null;
  turbineCount = 20;
  turbineSpacingD = 6.0;

  wakeLossPercent = 6.8;
  requiredAreaKm2 = 0;
  requiredAreaHectares = 0;

  isSimulating = false;
  simulationResult: WindSimulationResult | null = null;
  saveMessage: string | null = null;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.api.getTurbines().subscribe((turbines) => {
      this.turbines = turbines;
      this.api.getWindFarms().subscribe((farms) => {
        this.farms = farms;

        // Verifica query param farmId
        const farmIdParam = this.route.snapshot.queryParamMap.get('farmId');
        if (farmIdParam) {
          const parsedId = parseInt(farmIdParam, 10);
          const found = farms.find(f => f.id === parsedId);
          if (found) {
            this.selectFarm(found);
            return;
          }
        }

        if (farms.length > 0) {
          this.selectFarm(farms[0]);
        }
      });
    });
  }

  selectFarm(farm: WindFarm): void {
    this.currentFarm = farm;
    this.selectedFarmId = farm.id;
    this.turbineCount = farm.turbineCount;
    this.turbineSpacingD = farm.turbineSpacingD;
    this.selectedTurbine = this.turbines.find(t => t.id === farm.selectedTurbineId) || this.turbines[0] || null;
    this.simulationResult = farm.latestSimulation || null;
    this.recalculateWakeAndArea();
  }

  onFarmChange(): void {
    const found = this.farms.find(f => f.id === Number(this.selectedFarmId));
    if (found) {
      this.selectFarm(found);
    }
  }

  onTurbineChange(): void {
    if (this.currentFarm && this.selectedTurbine) {
      this.currentFarm.selectedTurbineId = this.selectedTurbine.id;
    }
    this.recalculateWakeAndArea();
  }

  recalculateWakeAndArea(): void {
    if (!this.selectedTurbine) return;

    // Calcula esteira via API
    this.api.estimateWakeLoss(this.turbineSpacingD, this.turbineCount).subscribe((res) => {
      this.wakeLossPercent = res.wakeLossPercent;
      if (this.currentFarm) {
        this.currentFarm.wakeLossPercentage = this.wakeLossPercent;
      }
    });

    // Calcula área requerida (5D x 3D por turbina)
    const downwindM = this.turbineSpacingD * this.selectedTurbine.rotorDiameterM;
    const crosswindM = Math.max(3.0, this.turbineSpacingD * 0.6) * this.selectedTurbine.rotorDiameterM;
    const totalAreaM2 = downwindM * crosswindM * this.turbineCount;

    this.requiredAreaKm2 = Math.round((totalAreaM2 / 1_000_000.0) * 100) / 100;
    this.requiredAreaHectares = Math.round((totalAreaM2 / 10_000.0) * 10) / 10;
  }

  get totalCombinedLossPercent(): number {
    if (!this.currentFarm) return 0;
    const f1 = 1 - (this.wakeLossPercent / 100.0);
    const f2 = 1 - (this.currentFarm.electricalLossPercentage / 100.0);
    const f3 = 1 - (this.currentFarm.availabilityLossPercentage / 100.0);
    const f4 = 1 - (this.currentFarm.otherLossesPercentage / 100.0);
    const totalNet = f1 * f2 * f3 * f4;
    return Math.round((1 - totalNet) * 1000) / 10;
  }

  get totalFarmCapacityMW(): number {
    if (!this.selectedTurbine) return 0;
    return Math.round(((this.selectedTurbine.ratedPowerKW / 1000.0) * this.turbineCount) * 10) / 10;
  }

  get gridRows(): number {
    return Math.ceil(Math.sqrt(this.turbineCount));
  }

  get gridCols(): number {
    return Math.ceil(this.turbineCount / this.gridRows);
  }

  runSimulation(): void {
    if (!this.currentFarm) return;

    this.isSimulating = true;
    this.saveMessage = null;

    // Atualiza dados no backend primeiro
    if (this.selectedTurbine) {
      this.currentFarm.selectedTurbineId = this.selectedTurbine.id;
    }
    this.currentFarm.turbineCount = this.turbineCount;
    this.currentFarm.turbineSpacingD = this.turbineSpacingD;
    this.currentFarm.wakeLossPercentage = this.wakeLossPercent;

    this.api.updateWindFarm(this.currentFarm.id, this.currentFarm).subscribe({
      next: (updatedFarm) => {
        this.currentFarm = updatedFarm;

        // Dispara simulação
        this.api.simulateFarm(this.currentFarm.id, this.selectedTurbine?.id, this.turbineCount).subscribe({
          next: (res) => {
            this.simulationResult = res;
            this.currentFarm!.latestSimulation = res;
            this.isSimulating = false;
            this.saveMessage = 'Simulação executada e resultados salvos com sucesso no banco de dados!';
            setTimeout(() => this.saveMessage = null, 5000);
          },
          error: (err) => {
            alert('Erro ao executar simulação: ' + (err.error?.message || err.message));
            this.isSimulating = false;
          }
        });
      },
      error: (err) => {
        alert('Erro ao atualizar parque: ' + err.message);
        this.isSimulating = false;
      }
    });
  }
}

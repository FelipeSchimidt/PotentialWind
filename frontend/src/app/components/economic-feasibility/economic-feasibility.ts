import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindFarm, WindSimulationResult, MonthlyProduction } from '../../core/models/wind.models';

@Component({
  selector: 'app-economic-feasibility',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './economic-feasibility.html',
  styleUrls: ['./economic-feasibility.scss']
})
export class EconomicFeasibilityComponent implements OnInit {
  private readonly api = inject(WindApiService);

  farms: WindFarm[] = [];
  selectedFarmId: number | null = null;
  currentFarm: WindFarm | null = null;
  simulation: WindSimulationResult | null = null;
  monthlyData: MonthlyProduction[] = [];

  ngOnInit(): void {
    this.api.getWindFarms().subscribe((farms) => {
      this.farms = farms;
      if (farms.length > 0) {
        this.selectFarm(farms[0]);
      }
    });
  }

  selectFarm(farm: WindFarm): void {
    this.currentFarm = farm;
    this.selectedFarmId = farm.id;
    this.simulation = farm.latestSimulation || null;
    this.parseMonthlyData();
  }

  onFarmChange(): void {
    const found = this.farms.find(f => f.id === Number(this.selectedFarmId));
    if (found) {
      this.selectFarm(found);
    }
  }

  parseMonthlyData(): void {
    if (this.simulation?.monthlyProductionJson) {
      try {
        this.monthlyData = JSON.parse(this.simulation.monthlyProductionJson);
      } catch {
        this.monthlyData = [];
      }
    } else {
      this.monthlyData = [];
    }
  }

  getMaxMonthlyEnergy(): number {
    if (this.monthlyData.length === 0) return 1000;
    return Math.max(...this.monthlyData.map(m => m.netMWh), 1000);
  }

  updateAndRecalculate(): void {
    if (!this.currentFarm) return;

    this.api.updateWindFarm(this.currentFarm.id, this.currentFarm).subscribe({
      next: (updated) => {
        this.currentFarm = updated;
        this.api.simulateFarm(this.currentFarm.id).subscribe({
          next: (res) => {
            this.simulation = res;
            this.currentFarm!.latestSimulation = res;
            this.parseMonthlyData();
          }
        });
      }
    });
  }
}

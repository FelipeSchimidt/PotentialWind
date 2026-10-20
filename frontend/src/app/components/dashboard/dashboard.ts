import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindFarm, WindTurbine } from '../../core/models/wind.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(WindApiService);

  windFarms: WindFarm[] = [];
  turbines: WindTurbine[] = [];
  loading = true;
  simulatingId: number | null = null;
  showCreateModal = false;

  newFarm: Partial<WindFarm> = {
    name: '',
    location: '',
    description: '',
    latitude: -12.5,
    longitude: -41.5,
    altitudeM: 500,
    roughnessExponent: 0.14,
    airDensityKgM3: 1.225,
    turbineCount: 15,
    turbineSpacingD: 5.5,
    wakeLossPercentage: 6.5,
    electricalLossPercentage: 2.5,
    availabilityLossPercentage: 2.5,
    otherLossesPercentage: 1.5,
    energyPriceMWh: 215,
    capexPerMW: 5300000,
    opexAnnualPercentCapex: 2.3,
    discountRatePercent: 8.5,
    projectLifespanYears: 25
  };

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.api.getWindFarms().subscribe({
      next: (farms) => {
        this.windFarms = farms;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar parques:', err);
        this.loading = false;
      }
    });

    this.api.getTurbines().subscribe({
      next: (turbines) => {
        this.turbines = turbines;
        if (turbines.length > 0 && !this.newFarm.selectedTurbineId) {
          this.newFarm.selectedTurbineId = turbines[0].id;
        }
      }
    });
  }

  get totalCapacityMW(): number {
    return this.windFarms.reduce((sum, f) => sum + (f.totalCapacityMW || 0), 0);
  }

  get totalAepGWh(): number {
    return this.windFarms.reduce((sum, f) => {
      const aep = f.latestSimulation?.netAepMWh || 0;
      return sum + (aep / 1000.0);
    }, 0);
  }

  get averageCapacityFactor(): number {
    const simulated = this.windFarms.filter(f => f.latestSimulation?.capacityFactorPercent);
    if (simulated.length === 0) return 0;
    const sum = simulated.reduce((acc, f) => acc + (f.latestSimulation?.capacityFactorPercent || 0), 0);
    return Math.round((sum / simulated.length) * 10) / 10;
  }

  runSimulation(farm: WindFarm): void {
    this.simulatingId = farm.id;
    this.api.simulateFarm(farm.id).subscribe({
      next: (result) => {
        farm.latestSimulation = result;
        this.simulatingId = null;
      },
      error: (err) => {
        console.error('Erro ao simular:', err);
        alert('Erro ao rodar simulação: ' + (err.error?.message || err.message));
        this.simulatingId = null;
      }
    });
  }

  createFarm(): void {
    if (!this.newFarm.name || !this.newFarm.selectedTurbineId) {
      alert('Por favor, preencha o nome do parque e selecione um aerogerador.');
      return;
    }

    this.api.createWindFarm(this.newFarm).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.loadData();
      },
      error: (err) => {
        alert('Erro ao criar parque eólico: ' + (err.error?.message || err.message));
      }
    });
  }

  deleteFarm(id: number, event: Event): void {
    event.stopPropagation();
    if (confirm('Tem certeza que deseja excluir este parque eólico?')) {
      this.api.deleteWindFarm(id).subscribe({
        next: () => this.loadData(),
        error: (err) => alert('Erro ao excluir: ' + err.message)
      });
    }
  }
}

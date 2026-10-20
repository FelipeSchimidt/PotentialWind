import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindFarm, WindMeasurement, MeasurementSummary } from '../../core/models/wind.models';

@Component({
  selector: 'app-measurement-analyzer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './measurement-analyzer.html',
  styleUrls: ['./measurement-analyzer.scss']
})
export class MeasurementAnalyzerComponent implements OnInit {
  private readonly api = inject(WindApiService);

  farms: WindFarm[] = [];
  selectedFarmId: number | null = null;
  summary: MeasurementSummary | null = null;
  measurements: WindMeasurement[] = [];
  loading = false;
  importing = false;

  ngOnInit(): void {
    this.api.getWindFarms().subscribe((farms) => {
      this.farms = farms;
      if (farms.length > 0) {
        this.selectedFarmId = farms[0].id;
        this.loadFarmMeasurements();
      }
    });
  }

  loadFarmMeasurements(): void {
    if (!this.selectedFarmId) return;

    this.loading = true;
    this.api.getMeasurementsSummary(this.selectedFarmId).subscribe({
      next: (sum) => {
        this.summary = sum;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar resumo anemométrico:', err);
        this.loading = false;
      }
    });

    this.api.getMeasurements(this.selectedFarmId, 50).subscribe({
      next: (list) => this.measurements = list
    });
  }

  onFarmChange(): void {
    this.loadFarmMeasurements();
  }

  getWindRosePolygon(rose: Array<{ angle: number; frequencyPercent: number }>): string {
    if (!rose || rose.length === 0) return '';

    const cx = 200;
    const cy = 200;
    const maxFreq = Math.max(...rose.map(r => r.frequencyPercent), 20);
    const radiusScale = 150 / maxFreq;

    const points = rose.map(r => {
      // Ângulo em radianos (0° é Norte, no topo -> subtrai 90°)
      const rad = (r.angle - 90) * (Math.PI / 180);
      const dist = Math.max(10, r.frequencyPercent * radiusScale);
      const x = cx + dist * Math.cos(rad);
      const y = cy + dist * Math.sin(rad);
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    });

    return points.join(' ');
  }

  generateSampleData(): void {
    if (!this.selectedFarmId) return;

    this.importing = true;
    const points: any[] = [];
    const now = new Date();

    for (let i = 0; i < 200; i++) {
      const time = new Date(now.getTime() - i * 3600 * 1000);
      // Simula ventos predominantes de Sudeste (100° a 140°)
      const speed = Math.max(1.0, 8.2 + (Math.random() * 4 - 2));
      const dir = (120 + (Math.random() * 60 - 30) + 360) % 360;

      points.push({
        timestamp: time.toISOString(),
        windSpeed: Math.round(speed * 10) / 10,
        windDirectionDeg: Math.round(dir),
        temperatureC: 24.5,
        pressureHPa: 1012.0
      });
    }

    this.api.importBatchMeasurements(this.selectedFarmId, points, 50.0).subscribe({
      next: () => {
        this.importing = false;
        this.loadFarmMeasurements();
      },
      error: (err) => {
        alert('Erro ao importar medições: ' + err.message);
        this.importing = false;
      }
    });
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindTurbine, QuickAnalysisRequest, QuickAnalysisResponse, WindBin } from '../../core/models/wind.models';

@Component({
  selector: 'app-wind-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './wind-calculator.html',
  styleUrls: ['./wind-calculator.scss']
})
export class WindCalculatorComponent implements OnInit {
  private readonly api = inject(WindApiService);

  turbines: WindTurbine[] = [];
  loading = false;

  request: QuickAnalysisRequest = {
    averageWindSpeed: 8.5,
    referenceHeightM: 50.0,
    targetHeightM: 120.0,
    roughnessAlpha: 0.14,
    airDensityKgM3: 1.225,
    turbineId: undefined
  };

  result: QuickAnalysisResponse | null = null;
  activeTab: 'curve' | 'bins' = 'curve';

  terrainPresets = [
    { name: 'Superfície de Água / Mar (Offshore)', alpha: 0.10 },
    { name: 'Terreno Plano com Gramíneas (Padrão)', alpha: 0.14 },
    { name: 'Terreno com Arbustos / Cercas', alpha: 0.18 },
    { name: 'Bosques / Terreno Irregular com Obstáculos', alpha: 0.22 }
  ];

  ngOnInit(): void {
    this.api.getTurbines().subscribe({
      next: (turbines) => {
        this.turbines = turbines;
        if (turbines.length > 0) {
          this.request.turbineId = turbines[0].id;
        }
        this.calculate();
      }
    });
  }

  applyPreset(alpha: number): void {
    this.request.roughnessAlpha = alpha;
    this.calculate();
  }

  calculate(): void {
    if (this.request.averageWindSpeed <= 0) return;

    this.loading = true;
    this.api.quickAnalysis(this.request).subscribe({
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro no cálculo:', err);
        this.loading = false;
      }
    });
  }

  getSvgPath(bins: WindBin[]): string {
    if (!bins || bins.length === 0) return '';

    const maxDensity = Math.max(...bins.map(b => b.probabilityDensity), 0.15);
    const width = 600;
    const height = 240;

    let path = `M 0 ${height}`;
    bins.forEach((b) => {
      const x = (b.windSpeed / 25.0) * width;
      const y = height - (b.probabilityDensity / maxDensity) * (height - 20);
      path += ` L ${x.toFixed(1)} ${y.toFixed(1)}`;
    });

    path += ` L ${width} ${height} Z`;
    return path;
  }

  getSvgPowerPath(bins: WindBin[]): string {
    if (!bins || bins.length === 0) return '';

    const maxPower = Math.max(...bins.map(b => b.turbinePowerKW), 1000);
    const width = 600;
    const height = 240;

    let path = '';
    bins.forEach((b, idx) => {
      const x = (b.windSpeed / 25.0) * width;
      const y = height - (b.turbinePowerKW / maxPower) * (height - 20);
      if (idx === 0) {
        path += `M ${x.toFixed(1)} ${y.toFixed(1)}`;
      } else {
        path += ` L ${x.toFixed(1)} ${y.toFixed(1)}`;
      }
    });

    return path;
  }
}

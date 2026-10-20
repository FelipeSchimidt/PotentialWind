import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WindApiService } from '../../core/services/wind-api.service';
import { WindTurbine, PowerCurvePoint } from '../../core/models/wind.models';

@Component({
  selector: 'app-turbine-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './turbine-catalog.html',
  styleUrls: ['./turbine-catalog.scss']
})
export class TurbineCatalogComponent implements OnInit {
  private readonly api = inject(WindApiService);

  turbines: WindTurbine[] = [];
  selectedTurbine: WindTurbine | null = null;
  parsedCurve: PowerCurvePoint[] = [];
  showModal = false;

  newTurbine: Partial<WindTurbine> = {
    manufacturer: '',
    modelName: '',
    ratedPowerKW: 5000,
    rotorDiameterM: 150,
    hubHeightM: 120,
    cutInSpeed: 3.0,
    ratedSpeed: 11.0,
    cutOutSpeed: 25.0
  };

  ngOnInit(): void {
    this.loadTurbines();
  }

  loadTurbines(): void {
    this.api.getTurbines().subscribe({
      next: (turbines) => {
        this.turbines = turbines;
        if (turbines.length > 0 && !this.selectedTurbine) {
          this.selectTurbine(turbines[0]);
        }
      }
    });
  }

  selectTurbine(turbine: WindTurbine): void {
    this.selectedTurbine = turbine;
    try {
      this.parsedCurve = JSON.parse(turbine.powerCurveJson || '[]');
    } catch {
      this.parsedCurve = [];
    }
  }

  getSvgPowerCurve(): string {
    if (!this.parsedCurve || this.parsedCurve.length === 0) return '';

    const maxPower = this.selectedTurbine?.ratedPowerKW || 5000;
    const width = 600;
    const height = 240;

    let path = `M 0 ${height}`;
    this.parsedCurve.forEach((p) => {
      const x = (p.windSpeed / 25.0) * width;
      const y = height - (p.powerKW / maxPower) * (height - 20);
      path += ` L ${x.toFixed(1)} ${y.toFixed(1)}`;
    });

    path += ` L ${width} ${height} Z`;
    return path;
  }

  createTurbine(): void {
    if (!this.newTurbine.manufacturer || !this.newTurbine.modelName) {
      alert('Preencha o fabricante e modelo.');
      return;
    }

    // Gera curva de potência padrão cúbica
    const points: PowerCurvePoint[] = [];
    const cutIn = this.newTurbine.cutInSpeed || 3.0;
    const rated = this.newTurbine.ratedSpeed || 11.0;
    const cutOut = this.newTurbine.cutOutSpeed || 25.0;
    const pNom = this.newTurbine.ratedPowerKW || 5000;

    for (let v = 0; v <= 30; v += 0.5) {
      let p = 0;
      if (v >= cutIn && v <= cutOut) {
        if (v >= rated) {
          p = pNom;
        } else {
          const frac = (v - cutIn) / (rated - cutIn);
          p = pNom * Math.pow(frac, 2.5);
          if (p > pNom) p = pNom;
        }
      }
      points.push({ windSpeed: Math.round(v * 10) / 10, powerKW: Math.round(p) });
    }

    this.newTurbine.powerCurveJson = JSON.stringify(points);

    this.api.createTurbine(this.newTurbine).subscribe({
      next: (created) => {
        this.showModal = false;
        this.loadTurbines();
        this.selectTurbine(created);
      },
      error: (err) => alert('Erro ao cadastrar aerogerador: ' + err.message)
    });
  }

  deleteTurbine(id: number, event: Event): void {
    event.stopPropagation();
    if (confirm('Deseja excluir este modelo de aerogerador?')) {
      this.api.deleteTurbine(id).subscribe({
        next: () => {
          this.selectedTurbine = null;
          this.loadTurbines();
        },
        error: (err) => alert('Erro ao excluir: ' + err.message)
      });
    }
  }
}

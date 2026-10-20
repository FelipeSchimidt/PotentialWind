import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';
import { WindCalculatorComponent } from './components/wind-calculator/wind-calculator';
import { FarmDesignerComponent } from './components/farm-designer/farm-designer';
import { TurbineCatalogComponent } from './components/turbine-catalog/turbine-catalog';
import { MeasurementAnalyzerComponent } from './components/measurement-analyzer/measurement-analyzer';
import { EconomicFeasibilityComponent } from './components/economic-feasibility/economic-feasibility';
import { DatabaseMigrationComponent } from './components/database-migration/database-migration';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'calculator', component: WindCalculatorComponent },
  { path: 'designer', component: FarmDesignerComponent },
  { path: 'turbines', component: TurbineCatalogComponent },
  { path: 'measurements', component: MeasurementAnalyzerComponent },
  { path: 'economics', component: EconomicFeasibilityComponent },
  { path: 'database', component: DatabaseMigrationComponent },
  { path: '**', redirectTo: 'dashboard' }
];

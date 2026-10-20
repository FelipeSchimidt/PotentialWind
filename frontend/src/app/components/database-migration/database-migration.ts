import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WindApiService } from '../../core/services/wind-api.service';
import { DatabaseStatus } from '../../core/models/wind.models';

@Component({
  selector: 'app-database-migration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './database-migration.html',
  styleUrls: ['./database-migration.scss']
})
export class DatabaseMigrationComponent implements OnInit {
  private readonly api = inject(WindApiService);

  status: DatabaseStatus | null = null;
  guideSteps: string[] = [];
  loading = false;

  ngOnInit(): void {
    this.refreshStatus();
    this.loadGuide();
  }

  refreshStatus(): void {
    this.loading = true;
    this.api.getDatabaseStatus().subscribe({
      next: (res) => {
        this.status = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  loadGuide(): void {
    this.api.getMigrationGuide().subscribe({
      next: (res) => this.guideSteps = res.steps,
      error: () => this.guideSteps = []
    });
  }
}

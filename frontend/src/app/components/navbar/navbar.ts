import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { WindApiService } from '../../core/services/wind-api.service';
import { DatabaseStatus } from '../../core/models/wind.models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss']
})
export class NavbarComponent implements OnInit {
  private readonly api = inject(WindApiService);
  dbStatus: DatabaseStatus | null = null;

  ngOnInit(): void {
    this.loadDbStatus();
  }

  loadDbStatus(): void {
    this.api.getDatabaseStatus().subscribe({
      next: (status) => this.dbStatus = status,
      error: () => this.dbStatus = null
    });
  }
}

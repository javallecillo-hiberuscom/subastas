import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of toastService.getToasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.tipo">
          <div class="toast-icon">
            @switch (toast.tipo) {
              @case ('success') { <span>✓</span> }
              @case ('error') { <span>✕</span> }
              @case ('info') { <span>ℹ</span> }
              @case ('warning') { <span>⚠</span> }
            }
          </div>
          <div class="toast-message">{{ toast.mensaje }}</div>
          <button class="toast-close" (click)="toastService.cerrar(toast.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 80px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 400px;
    }

    .toast {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px 20px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      background: white;
      animation: slideIn 0.3s ease-out;
      min-width: 300px;
    }

    @keyframes slideIn {
      from {
        transform: translateX(400px);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .toast-icon {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 16px;
      flex-shrink: 0;
    }

    .toast-success .toast-icon {
      background: #10b981;
      color: white;
    }

    .toast-error .toast-icon {
      background: #ef4444;
      color: white;
    }

    .toast-info .toast-icon {
      background: #3b82f6;
      color: white;
    }

    .toast-warning .toast-icon {
      background: #f59e0b;
      color: white;
    }

    .toast-message {
      flex: 1;
      color: #1f2937;
      font-size: 14px;
      line-height: 1.5;
    }

    .toast-close {
      background: none;
      border: none;
      color: #6b7280;
      font-size: 24px;
      line-height: 1;
      cursor: pointer;
      padding: 0;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      transition: color 0.2s;
    }

    .toast-close:hover {
      color: #1f2937;
    }

    .toast-success {
      border-left: 4px solid #10b981;
    }

    .toast-error {
      border-left: 4px solid #ef4444;
    }

    .toast-info {
      border-left: 4px solid #3b82f6;
    }

    .toast-warning {
      border-left: 4px solid #f59e0b;
    }

    @media (max-width: 768px) {
      .toast-container {
        right: 10px;
        left: 10px;
        max-width: none;
      }

      .toast {
        min-width: auto;
      }
    }
  `]
})
export class ToastComponent {
  toastService = inject(ToastService);
}

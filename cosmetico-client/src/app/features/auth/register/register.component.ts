import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  /// Validare cross-field: parolele trebuie sa coincida (Laborator: validari pe formulare).
  private static passwordsMatch(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
  }

  protected readonly form = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          // aceleasi reguli ca Identity pe server: minim o cifra si o majuscula
          Validators.pattern(/^(?=.*[A-Z])(?=.*\d).+$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [RegisterComponent.passwordsMatch] },
  );

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { fullName, email, password } = this.form.getRawValue();
    this.auth.register({ fullName, email, password }).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.errorMessage.set(extractApiError(err));
        this.submitting.set(false);
      },
    });
  }
}

import { Component, inject, signal } from '@angular/core';
import { FormControl, Validators, ReactiveFormsModule } from '@angular/forms';

import { DataService, EmailResponse } from '../../services/data/data.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-form',
  standalone: true,
  imports: [ReactiveFormsModule], // Required for [formControl]
  templateUrl: './form.component.html',
  styleUrl: './form.component.scss'
})
export class FormComponent {
  private dataService = inject(DataService);

  emailControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.email]
  });

  isSubmitting = signal(false);
  responseRecivedDate = signal('');
  errorMessage = signal('');

  async onSubmit() {
    if (this.emailControl.invalid) return;

    this.isSubmitting.set(true);
    this.responseRecivedDate.set('');
    this.errorMessage.set('');

    try {
      let response: EmailResponse = await this.dataService.subscribeEmail(this.emailControl.value);

      this.responseRecivedDate.set(response.recivedDate);
    } catch (res: unknown) {
      if ((res as HttpErrorResponse).error.email) {
        this.responseRecivedDate.set((res as HttpErrorResponse).error.recivedDate);
      } else {
        this.responseRecivedDate.set((res as HttpErrorResponse).message);
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

}
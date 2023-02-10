import { Component, Input } from '@angular/core';

@Component({
  selector: 'caas-errors',
  templateUrl: './errors.component.html',
  styles: [
  ]
})
export class ErrorsComponent {
  @Input() errors: string[] = [];
}

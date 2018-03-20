import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Validators, FormControl } from '@angular/forms';

@Component({
	selector: 'people-form-field-input',
	templateUrl: './form-field-input.component.html',
	styleUrls: ['./form-field-input.component.scss']
})
export class FormFieldInputComponent implements OnInit {
	constructor() {}

	@Input() value: string;
	@Input() inputMinLength? = 3;
	@Output() onValueChange = new EventEmitter<string>();

	formControl: FormControl;

	ngOnInit() {
		this.formControl = new FormControl(this.value, [
			Validators.required,
			Validators.minLength(this.inputMinLength)
		]);
	}

	updateValue(value) {
		this.onValueChange.emit(value);
	}
}

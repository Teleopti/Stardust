import { AsyncValidatorFn, FormControl, ValidatorFn } from '@angular/forms';
import { AbstractControlOptions } from '@angular/forms/src/model';

export class FormControlWithInitial extends FormControl {
    constructor(
        formState?: any,
        validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null,
        asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null
    ) {
        super(formState, validatorOrOpts, asyncValidator);
        this.initialValue = formState;
    }
    public initialValue: string;

    get sameAsInitialValue() {
        return this.initialValue === this.value;
    }
}

import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavigationService, WorkspaceService, AppLogonService } from '../../services';
import { MatTableDataSource, MatList, MatListItem, MatGridList} from '@angular/material';
import { Person } from '../../types';
import { Subject, Observable } from 'rxjs';
import { map, flatMap, filter, distinctUntilChanged, first } from 'rxjs/operators';
import { FormBuilder, FormControl, FormGroup, FormArray, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';

export function duplicateNameValidator(appLogonService: AppLogonService): AsyncValidatorFn {
	const stateToErrorMessage = state => ({ duplicateNameValidator: state });

	return (control: AbstractControl): Observable<ValidationErrors> => {
		return Observable.timer(300).pipe(
			first(),
			filter(() => control.value != null && control.value.length > 0),
			flatMap(() => appLogonService.logonNameExists(control.value)),
			filter(exists => exists === true),
			map(exists => stateToErrorMessage(exists))
		);
	};
}


@Component({
	selector: 'people-app-logon-page',
	templateUrl: './app-logon-page.component.html',
	styleUrls: ['./app-logon-page.component.scss']
})

export class AppLogonPageComponent implements OnInit, OnDestroy {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public appLogonService: AppLogonService,
		private formBuilder: FormBuilder
	) {
		this.createForm();
	}

	private componentDestroyed: Subject<any> = new Subject();

	logonForm: FormGroup;
	duplicationError: boolean = false;

	ngOnInit() {
	}

	ngOnDestroy() {
		this.componentDestroyed.next();
		this.componentDestroyed.complete();
	}

	createForm() {
		let name = new FormControl("a name?");
		name.disable();
		this.logonForm = this.formBuilder.group({
		  name,
		  logons: this.formBuilder.array([])
		});
		this.workspaceService.people$.takeUntil(this.componentDestroyed).subscribe({
			next: (people: Person[]) => {
				this.rebuildForm(people.map(p => ({
					...p,
					FirstName: 'hej',
					LastName: 'blau',
					ApplicationLogon: p.ApplicationLogon
				})));
				if (people.length === 0) this.nav.navToSearch();
			}
		});
		this.logons.valueChanges.subscribe({
		  next: value => this.onFormValueChanges(value)
		});
	  }

	  rebuildForm(values?: Person[]) {
		while (this.logons.length > 0) this.logons.removeAt(0);
		values
		  .map(value => {
			  var formGroup = this.formBuilder.group(value)
			  var appLogon = formGroup.get("ApplicationLogon") as FormControl
			  appLogon.setAsyncValidators([duplicateNameValidator(this.appLogonService)])
			  return formGroup;
			})
		  .forEach(control => this.logons.push(control));
	  }
	
	  onFormValueChanges(values: Person[]) {
		let errors = values
		  .filter(({ ApplicationLogon }) => ApplicationLogon.length > 0)
		  .reduce((errors, person, index, people) => {
			const peopleWithSameLogon = people.filter(
			  p => p.Id !== person.Id && p.ApplicationLogon === person.ApplicationLogon
			).length;
			if (peopleWithSameLogon === 0) return errors;
			return errors.concat({ [person.ApplicationLogon]: person.Id });
		  }, []);
		this.duplicationError = errors.length > 0;
	  }

	  get logons(): FormArray {
		return this.logonForm.get("logons") as FormArray;
	  }

	isValid(): boolean {
		return true;
	}

	getName(person: Person): string {
		return `${person.FirstName} ${person.LastName}`;
	}

	save(): void {}
}

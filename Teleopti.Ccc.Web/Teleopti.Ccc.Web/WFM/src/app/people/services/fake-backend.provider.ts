import { Injectable } from '@angular/core';
import {
	HttpRequest,
	HttpResponse,
	HttpHandler,
	HttpEvent,
	HttpInterceptor,
	HTTP_INTERCEPTORS,
	HttpErrorResponse
} from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/of';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/delay';
import 'rxjs/add/operator/mergeMap';
import { Role, Person } from '../types';

export const ROLES: Array<Role> = [
	{
		Id: 'id1',
		Name: 'Role 1'
	},
	{
		Id: 'id2',
		Name: 'Role 2'
	},
	{
		Id: 'id3',
		Name: 'Role 3'
	},
	{
		Id: 'id4',
		Name: 'Role 4'
	}
];

export const PEOPLE: Array<Person> = [
	{
		Id: 'id1',
		FirstName: 'First1',
		LastName: 'Lastname1',
		Roles: [
			{
				Id: 'id1',
				Name: 'Role 1'
			},
			{
				Id: 'id2',
				Name: 'Role 2'
			}
		]
	},
	{
		Id: 'id2',
		FirstName: 'First2',
		LastName: 'Lastname2',
		Roles: [
			{
				Id: 'id1',
				Name: 'Role 1'
			}
		]
	},
	{
		Id: 'id3',
		FirstName: 'First3',
		LastName: 'Lastname3',
		Roles: [
			{
				Id: 'id1',
				Name: 'Role 1'
			}
		]
	}
];

export const PEOPLE_SEARCH = {
	People: [
		{
			FirstName: 'Ashley',
			LastName: 'Andeen',
			EmploymentNumber: '137545',
			PersonId: '11610fe4-0130-4568-97de-9b5e015b2564',
			Email: 'Ashley.Andeen@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 8' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '209209' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Preferences'
		},
		{
			FirstName: 'Prashant',
			LastName: 'Arora',
			EmploymentNumber: '137547',
			PersonId: '9d42c9bf-f766-473f-970c-9b5e015b2564',
			Email: 'Prashant.Arora@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 5' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '209209' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Preferences'
		},
		{
			FirstName: 'Pierre',
			LastName: 'Baldi',
			EmploymentNumber: '137567',
			PersonId: 'b0e35119-4661-4a1b-8772-9b5e015b2564',
			Email: 'Pierre.Baldi@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 3' },
				{ Key: 'Cell Phone', Value: '46709218052' },
				{ Key: 'Manager Emp Nr', Value: '209209' },
				{ Key: 'Manager/Trainer', Value: 'Trainer' }
			],
			Team: 'London/Team Preferences'
		},
		{
			FirstName: 'Juancho',
			LastName: 'Banaag',
			EmploymentNumber: '137569',
			PersonId: 'fdb75a4e-5765-4857-b213-9b5e015b2564',
			Email: 'Juancho.Banaag@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 7' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '209209' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Preferences'
		},
		{
			FirstName: 'Mark',
			LastName: 'Bergman',
			EmploymentNumber: '137634',
			PersonId: 'c6d9c037-46e8-4947-93b4-9b5e015b2564',
			Email: 'Mark.Bergman@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 2' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Flexible'
		},
		{
			FirstName: 'Lubomir',
			LastName: 'Bic',
			EmploymentNumber: '137712',
			PersonId: '19d4a10a-6382-4c5a-960b-9b5e015b2564',
			Email: 'Lubomir.Bic@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 5' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Flexible'
		},
		{
			FirstName: 'Daniel',
			LastName: 'Billsus',
			EmploymentNumber: '137727',
			PersonId: '4fd900ad-2b33-469c-87ac-9b5e015b2564',
			Email: 'Daniel.Billsus@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 9' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'Shenzhen/Shenzhen 1'
		},
		{
			FirstName: 'Alfred',
			LastName: 'Bork',
			EmploymentNumber: '137784',
			PersonId: 'e34730fd-9b4d-4572-961a-9b5e015b2564',
			Email: 'Alfred.Bork@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 2' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Flexible'
		},
		{
			FirstName: 'Erin',
			LastName: 'Bradner',
			EmploymentNumber: '137865',
			PersonId: 'd4dcbb15-a69c-41ae-941a-9b5e015b2564',
			Email: 'Erin.Bradner@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 2' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: '' }
			],
			Team: 'London/Team Flexible'
		},
		{
			FirstName: 'John',
			LastName: 'Smith',
			EmploymentNumber: '209209',
			PersonId: '47a3d4aa-3cd8-4235-a7eb-9b5e015b2560',
			Email: 'john.smith@insurance.com',
			LeavingDate: '',
			OptionalColumnValues: [
				{ Key: 'Agent Rank', Value: 'Tier 5' },
				{ Key: 'Cell Phone', Value: '' },
				{ Key: 'Manager Emp Nr', Value: '' },
				{ Key: 'Manager/Trainer', Value: 'Manager' }
			],
			Team: 'London/Team Preferences'
		}
	],
	CurrentPage: 1,
	TotalPages: 21,
	OptionalColumns: ['Agent Rank', 'Cell Phone', 'Manager Emp Nr', 'Manager/Trainer']
};

@Injectable()
export class FakeBackendInterceptor implements HttpInterceptor {
	constructor() {}

	intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		// wrap in delayed observable to simulate server api call
		return Observable.of(null).mergeMap(() => {
			if (request.url.endsWith('/api/PeopleData/fetchRoles') && request.method === 'GET') {
				return Observable.of(new HttpResponse({ status: 200, body: ROLES }));
			}

			if (request.url.includes('/api/Search/People/Keyword') && request.method === 'GET') {
				return Observable.of(new HttpResponse({ status: 200, body: PEOPLE_SEARCH }));
			}

			if (request.url.endsWith('/api/PeopleData/fetchPersons') && request.method === 'POST') {
				return Observable.of(new HttpResponse({ status: 200, body: PEOPLE }));
			}

			if (request.url.endsWith('/api/Search/FindPeople') && request.method === 'POST') {
				return Observable.of(new HttpResponse({ status: 200, body: [] }));
			}

			// pass through any requests not handled above
			return next.handle(request);
		});
	}
}

export let fakeBackendProvider = {
	// use fake backend in place of Http service for backend-less development
	provide: HTTP_INTERCEPTORS,
	useClass: FakeBackendInterceptor,
	multi: true
};

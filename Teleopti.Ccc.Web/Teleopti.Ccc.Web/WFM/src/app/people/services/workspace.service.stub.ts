import { WorkspaceService } from './workspace.service';
import { Person } from '../types';
import { BehaviorSubject } from 'rxjs';
import { PEOPLE } from './fake-backend/people';

export class WorkspaceServiceStub extends WorkspaceService {
	people$ = new BehaviorSubject<Array<Person>>(PEOPLE);
}

import { Component } from '@angular/core';
import { Person, Role } from '../../types';

import { RolePage } from '../shared/role-page';

@Component({
	selector: 'people-grant',
	templateUrl: './grant-page.component.html',
	styleUrls: ['./grant-page.component.scss']
})
export class GrantPageComponent extends RolePage {}

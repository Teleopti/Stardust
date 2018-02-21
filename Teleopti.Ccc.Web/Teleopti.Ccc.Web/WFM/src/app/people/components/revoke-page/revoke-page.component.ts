import { Component, Inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Person, Role } from '../../types';
import { RolePage } from '../shared/role-page';

@Component({
	selector: 'app-revoke',
	templateUrl: './revoke-page.component.html',
	styleUrls: ['./revoke-page.component.scss']
})
export class RevokePageComponent extends RolePage {}

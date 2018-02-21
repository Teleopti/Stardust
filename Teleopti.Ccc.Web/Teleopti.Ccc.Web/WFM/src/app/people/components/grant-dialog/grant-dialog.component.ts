import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatCheckboxModule } from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Person, Role } from '../../types';
import { RoleDialog } from '../../components/shared/role-dialog';

@Component({
	templateUrl: './grant-dialog.component.html',
	styleUrls: ['./grant-dialog.scss']
})
export class GrantDialog extends RoleDialog {}

export interface GrantResponse {
	roles: Array<string>;
}

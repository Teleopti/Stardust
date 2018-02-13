import { Component, OnInit, Inject } from '@angular/core';
import {MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatCheckboxModule} from '@angular/material';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import { People } from './people';
import { ROLES } from './roles.mock';
import { Roles } from './roles';

@Component({
    selector: 'dialog-content-example-dialog',
    templateUrl: './peoplemodal.component.html'
  })
  export class DialogContentExampleDialog {
    itemArr: Array<People>;
    currentRoles: Array<Roles>;
  }
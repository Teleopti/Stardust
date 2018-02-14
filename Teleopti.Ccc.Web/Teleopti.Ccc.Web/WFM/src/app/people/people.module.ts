import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { SharedModule } from '../shared/shared.module';

import { PeopleComponent } from './people.component';
import {
  MatInputModule,
  MatDialogModule,
  MatProgressSpinnerModule,
  MatButtonModule,
  MatDialog,
  MatDialogRef,
  MatCheckboxModule,
} from '@angular/material';
import { DialogContentExampleDialog } from './peoplemodal.component';

@NgModule({
  declarations: [
    PeopleComponent,
    DialogContentExampleDialog
  ],
  imports: [
    SharedModule
  ],
  providers: [],
  exports: [
      
  ],
  entryComponents: [
    PeopleComponent,
    DialogContentExampleDialog
  ]
})
export class PeopleModule {
  constructor() { }
  ngDoBootstrap() {
  }
 }

 angular.module('wfm').directive('ng2People', downgradeComponent({component: PeopleComponent}) as angular.IDirectiveFactory);
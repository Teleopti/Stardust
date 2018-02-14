import { Component, OnInit, Inject } from '@angular/core';

import { PEOPLE } from './people.mock';

import { People } from './people';
import { ROLES } from './roles.mock';
import { Roles } from './roles';
import { DialogContentExampleDialog } from './peoplemodal.component';

import {
	MatInputModule,
	MatDialogModule,
	MatProgressSpinnerModule,
	MatButtonModule,
	MatDialog,
    MatDialogRef,
    MatCheckboxModule
} from '@angular/material'; 

@Component({
  selector: 'app-people',
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.css']
})
export class PeopleComponent implements OnInit {
/* 
  constructor(public dialog: MatDialog) { console.log(dialog); }

  people = PEOPLE;
  itemArr: Array<People>;
  roles = ROLES;
  currentRoles: Array<Roles>;

  openDialog() {
    const dialogRef = this.dialog.open(DialogContentExampleDialog, {
      height: '350px'
    });

    dialogRef.componentInstance.itemArr = this.itemArr;
    dialogRef.componentInstance.currentRoles = this.currentRoles;

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }

  assertMulti(person){
    let index = this.itemArr.indexOf(person, 0);

    if(index > -1){
      this.itemArr.splice(index, 1);
    }else{
      this.itemArr.push(person);
    }

    this.itemArr.forEach(function(value){
      console.log(value);
    });
  }
*/
  ngOnInit() {

    
  } 
}
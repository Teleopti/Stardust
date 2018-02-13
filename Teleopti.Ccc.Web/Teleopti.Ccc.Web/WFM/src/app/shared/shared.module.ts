import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

@NgModule({
    imports: [CommonModule, FormsModule, BrowserAnimationsModule],
    exports: [CommonModule, FormsModule, BrowserAnimationsModule],
    providers: [],
    entryComponents: []
})
export class SharedModule {}

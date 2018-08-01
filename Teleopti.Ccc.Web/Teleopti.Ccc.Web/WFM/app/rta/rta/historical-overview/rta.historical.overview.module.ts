import {NgModule} from '@angular/core';
import {RtaHistoricalOverviewComponent} from "./rta.historical.overview.component";
import {BrowserModule} from "@angular/platform-browser";
import {FormsModule} from "@angular/forms";
import {SearchPipe} from "./search.pipe";

@NgModule({
	declarations: [RtaHistoricalOverviewComponent,SearchPipe],
	imports: [BrowserModule, FormsModule],
	exports:[SearchPipe],
	entryComponents: [RtaHistoricalOverviewComponent]
})

export class RtaHistoricalOverviewModule {
}

import {NgModule} from '@angular/core';
import {RtaHistoricalOverviewComponent} from "./rta.historical.overview.component";
import {BrowserModule} from "@angular/platform-browser";
import {FormsModule} from "@angular/forms";
import {SearchPipe} from "./search.pipe";
import {rtaServiceProvider} from "./rtaService.provider";

@NgModule({
	declarations: [RtaHistoricalOverviewComponent,SearchPipe],
	imports: [BrowserModule, FormsModule],
	exports:[SearchPipe],
	providers:[rtaServiceProvider],
	entryComponents: [RtaHistoricalOverviewComponent]
})

export class RtaHistoricalOverviewModule {
}

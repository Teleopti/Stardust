import {NgModule} from '@angular/core';
import {RtaHistoricalOverviewComponent} from "./rta.historical.overview.component";
import {BrowserModule} from "@angular/platform-browser";
import {FormsModule} from "@angular/forms";
import {SearchPipe} from "./search.pipe";
import {rtaDataServiceProvider} from "./rta.data.service.provider";
import {rtaStateServiceProvider} from "./rta.state.service.provider";

@NgModule({
	declarations: [RtaHistoricalOverviewComponent,SearchPipe],
	imports: [BrowserModule, FormsModule],
	exports:[SearchPipe],
	providers:[rtaDataServiceProvider, rtaStateServiceProvider],
	entryComponents: [RtaHistoricalOverviewComponent]
})

export class RtaHistoricalOverviewModule {
}

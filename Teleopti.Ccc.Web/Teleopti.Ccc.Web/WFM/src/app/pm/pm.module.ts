import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import {
    WorkspaceComponent
} from './components';
// import { NavigationService, RolesService, SearchOverridesService, SearchService, WorkspaceService } from './services';

@NgModule({
    declarations: [
        WorkspaceComponent
    ],
    imports: [],
    providers: [],
    exports: [],
    entryComponents: [WorkspaceComponent]
})
export class PmModule {
    ngDoBootstrap() {}
}

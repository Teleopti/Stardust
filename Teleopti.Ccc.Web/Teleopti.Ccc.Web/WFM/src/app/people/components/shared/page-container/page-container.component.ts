import { Component, OnInit, Input } from '@angular/core';

@Component({
	selector: 'people-page-container',
	templateUrl: './page-container.component.html',
	styleUrls: ['./page-container.component.scss']
})
export class PageContainerComponent {
	@Input() showWorkspace?: boolean = true;
}

import { Component, OnInit } from '@angular/core';

@Component({
	selector: 'people-playground',
	templateUrl: './playground.component.html',
	styleUrls: ['./playground.component.scss']
})
export class PlaygroundComponent implements OnInit {
	constructor() {}

	ngOnInit() {}

	dataSource = [
		{
			text: 'text1',
			number: 2135847
		},
		{
			text: 'text2',
			number: 2.3546
		}
	];
	displayedColumns = ['text', 'number'];
}

import { Component, Inject, OnInit } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { Area, AreaService } from '../../shared/area.service';

type AreaConfig = {
	InternalName?: string;
	url?: string;
	inNewTab?: boolean;
	icon?: string;
};

type AreaWithConfig = Area & AreaConfig;

type AreaGroup = Array<AreaWithConfig>;

const GROUPS: AreaGroup[] = [
	[
		{ InternalName: 'people', icon: 'mdi mdi-account-group' },
		{ InternalName: 'permissions', icon: 'mdi mdi-account-key' },
		{ InternalName: 'forecast', icon: 'mdi mdi-chart-line' },
		{ InternalName: 'resourceplanner', icon: 'mdi mdi-calendar' },
		{ InternalName: 'outbound', icon: 'mdi mdi-phone-forward' }
	],
	[
		{ InternalName: 'teams', icon: 'mdi mdi-account-multiple' },
		{ InternalName: 'requests', icon: 'mdi mdi-numeric-9-plus-box-multiple-outline' },
		{ InternalName: 'staffingModule', icon: 'mdi mdi-account-switch' },
		{ InternalName: 'intraday', icon: 'mdi mdi-compass-outline' },
		{ InternalName: 'rta', icon: 'mdi mdi-alarm' }
	],
	[{ InternalName: 'seatPlan', icon: 'mdi mdi-tab-unselected' }, { InternalName: 'seatMap', icon: 'mdi mdi-tab' }],
	[{ InternalName: 'reports', icon: 'mdi mdi-chart-bar' }, { InternalName: 'gamification', icon: 'mdi mdi-trophy' }],
	[
		{ InternalName: 'myTime', inNewTab: true, url: '../MyTime', icon: 'mdi mdi-calendar-clock' },
		{
			icon: 'mdi mdi-elevation-rise',
			InternalName: 'pm'
		}
	]
];

@Component({
	selector: 'side-menu',
	templateUrl: './side-menu.component.html',
	styleUrls: ['./side-menu.component.scss']
})
export class SideMenuComponent implements OnInit {
	showMenu: boolean = this.isMobileView();
	areas: AreaWithConfig[] = [];
	groups: AreaGroup[];

	constructor(@Inject('$state') private $state: IStateService, private areaService: AreaService) {
		this.areaService.getAreas().subscribe(areas => {
			this.areas = areas;
			this.groups = GROUPS.map(group => this.filterPermittedAreas(group)).map(group => this.addAreaInfo(group));
		});
	}

	ngOnInit() {
		window.matchMedia('(max-width: 768px)').addListener(mq => {
			if (mq.matches) this.showMenu = false;
		});
	}

	addAreaInfo(group: AreaGroup) {
		const mergeAreaConfigs = groupArea => {
			const area = this.areas.find(area => area.InternalName === groupArea.InternalName);
			return { ...area, ...groupArea };
		};
		return group.map(mergeAreaConfigs);
	}

	filterPermittedAreas(group: AreaGroup) {
		const permissionToViewAreFilter = area =>
			this.areas.findIndex(({ InternalName }) => InternalName === area.InternalName) > -1;
		return group.filter(permissionToViewAreFilter);
	}

	toggleMenu() {
		this.showMenu = !this.showMenu;
	}

	go(event: Event, area: AreaWithConfig) {
		if (!area.inNewTab) {
			event.preventDefault();
			this.$state.go(area.InternalName);
		}
	}

	isMobileView() {
		return window.innerWidth > 768 ? true : false;
	}

	isActive(area: Area) {
		return this.$state.current.name.indexOf(area.InternalName) == 0;
	}
}

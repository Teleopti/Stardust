import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { untilDestroyed } from 'ngx-take-until-destroy';
import { first } from 'rxjs/operators';
import { MediaQueryService } from 'src/app/browser/services/media-query.service';
import { Area, AreaService } from '../../shared/area.service';
import { ToggleMenuService } from '../../shared/toggle-menu.service';

interface AreaConfig {
	InternalName?: string;
	CustomStateName?: string;
	url?: string;
	inNewTab?: boolean;
	icon?: string;
}

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
		{ InternalName: 'intraday', icon: 'mdi mdi-compass-outline', CustomStateName: 'intraday.index' },
		{ InternalName: 'rta', icon: 'mdi mdi-alarm' }
	],
	[{ InternalName: 'seatPlan', icon: 'mdi mdi-tab-unselected' }, { InternalName: 'seatMap', icon: 'mdi mdi-tab' }],
	[
		{ InternalName: 'reports', icon: 'mdi mdi-chart-bar' },
		{
			icon: 'mdi mdi-elevation-rise',
			InternalName: 'insights'
		},
		{ InternalName: 'gamification', icon: 'mdi mdi-trophy' }
	],
	[{ InternalName: 'myTime', inNewTab: true, url: '../MyTime', icon: 'mdi mdi-calendar-clock' }]
];

@Component({
	selector: 'side-menu',
	templateUrl: './side-menu.component.html',
	styleUrls: ['./side-menu.component.scss']
})
export class SideMenuComponent implements OnInit, OnDestroy {
	showMenu: boolean;
	areas: AreaWithConfig[] = [];
	groups: AreaGroup[];

	constructor(
		@Inject('$state') private $state: IStateService,
		private areaService: AreaService,
		public toggleMenuService: ToggleMenuService,
		private mediaQueryService: MediaQueryService
	) {
		this.areaService.getAreas().subscribe(areas => {
			this.areas = areas;
			const filterEmptyGroups = (group: AreaGroup) => group.length > 0;
			this.groups = GROUPS.map(group => this.filterPermittedAreas(group))
				.filter(group => filterEmptyGroups(group))
				.map(group => this.addAreaInfo(group));
		});
	}

	ngOnInit() {
		this.toggleMenuService.showMenu$.pipe(untilDestroyed(this)).subscribe(isVisible => {
			this.showMenu = isVisible;
		});
		this.mediaQueryService.isMobileSize$.pipe(untilDestroyed(this)).subscribe(isMobileView => {
			if (isMobileView) this.toggleMenuService.setMenuVisible(false);
		});
	}

	ngOnDestroy(): void {}

	addAreaInfo(group: AreaGroup) {
		const mergeAreaConfigs = groupArea => {
			const area = this.areas.find(a => a.InternalName === groupArea.InternalName);
			return { ...area, ...groupArea };
		};
		return group.map(mergeAreaConfigs);
	}

	filterPermittedAreas(group: AreaGroup) {
		const permissionToViewAreFilter = area =>
			this.areas.findIndex(({ InternalName }) => InternalName === area.InternalName) > -1;
		return group.filter(permissionToViewAreFilter);
	}

	go(event: Event, area: AreaWithConfig) {
		if (area.inNewTab) return;
		event.preventDefault();
		const stateName = area.CustomStateName ? area.CustomStateName : area.InternalName;
		this.mediaQueryService.isMobileSize$.pipe(first()).subscribe(isMobileView => {
			if (isMobileView) this.toggleMenuService.setMenuVisible(false);
			this.$state.go(stateName, {}, { reload: true });
		});
	}

	isActive(area: Area) {
		return this.$state.current.name.indexOf(area.InternalName) === 0;
	}
}

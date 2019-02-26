import { Component, Inject } from '@angular/core';
import { interval } from 'rxjs';
import { map } from 'rxjs/operators';
import { NavigationService, TogglesService } from 'src/app/core/services';
import { AreaService } from '../../shared/area.service';
import { ToggleMenuService } from '../../shared/toggle-menu.service';
import { SystemSettingsService } from '../../../system-settings/shared/system-settings.service';

@Component({
	selector: 'top-navigation',
	templateUrl: './top-navigation.component.html',
	styleUrls: ['./top-navigation.component.scss']
})
export class TopNavigationComponent {
	helpUrl$ = interval(400).pipe(
		map(() => {
			return 'https://wiki.teleopti.com/TeleoptiWFM/' + this.$state.current.name;
		})
	);
	mainUrl = this.$state.href('main');
	systemSettingArea = null;
	isSystemSettingsVisible = false;
	constructor(
		@Inject('$state') private $state,
		public toggleService: TogglesService,
		public navigationService: NavigationService,
		public toggleMenuService: ToggleMenuService,
		public areaService: AreaService,
		public systemSettingsService: SystemSettingsService
	) {
		this.systemSettingsService.checkPermission().subscribe(hasPermission => {
			toggleService.toggles$.subscribe({
				next: toggles => {
					this.isSystemSettingsVisible =
						toggles.WFM_Setting_BankHolidayCalendar_Create_79297 && hasPermission;
				}
			});
		});

		areaService.getAreas().subscribe(areas => {
			this.systemSettingArea = areas.filter(a => a.InternalName === 'systemSettings')[0];
		});
	}
	goToSystemSettings() {
		this.$state.go(this.systemSettingArea.InternalName, {}, { reload: true });
	}
}

function MenuController($state) {
	var ctrl = this;
	ctrl.$onInit = function () {
		ctrl.menuItem = setMenuItem();
	}
	ctrl.closeMenuOnPhones = function () {
		ctrl.open = false;
	}

	var menuItems = [
		{
			i: 'mdi mdi-chart-line',
			name: 'forecasting'
		},
		{
			i: 'mdi mdi-calendar',
			name: 'resourceplanner'
		},
		{
			i: 'mdi mdi-account-key',
			name: 'permissions'
		},
		{
			i: 'mdi mdi-phone-forward',
			name: 'outbound'
		},
		{
			i: 'mdi mdi-account-multiple',
			name: 'people'
		},
		{
			i: 'mdi mdi-numeric-9-plus-box-multiple-outline ',
			name: 'requests'
		},
		{
			i: 'mdi mdi-tab-unselected',
			name: 'seatPlan'
		},
		{
			i: 'mdi mdi-tab',
			name: 'seatMap'
		},
		{
			i: 'mdi mdi-alarm',
			name: 'rta'
		},
		{
			i: 'mdi mdi-compass-outline',
			name: 'intraday'
		},
		{
			i: 'mdi mdi-account-multiple',
			name: 'teams'
		},
		{
			i: 'mdi mdi-chart-bar',
			name: 'reports'
		},
		{
			i: 'mdi mdi-rotate-3d',
			name: 'staffing'
		},
		{
			i: 'mdi mdi-calendar-clock',
			name: 'myTime',
			url: '../MyTime',
			isStateless: true,
			isOpenInNewTab: true
		}
	];
	function setMenuItem() {
		var match = menuItems.find(function (icon) {
			return icon.name === ctrl.data.InternalName;
		});
		match.displayName = ctrl.data.Name;
		match.class = function () {
			if ($state.current.name.indexOf(match.name) == 0)
				return "main-menu-link-active nav-item-active";
		};
		return match;
	}


}

angular.module('wfm.areas').component('mainmenu', {
	templateUrl: 'app/global/areas/menu-component.html',
	controller: MenuController,
	bindings: {
		data: '=',
		open: '='
	}
});

'use strict';

(function () {

	angular.module('wfm.teamSchedule').directive('addAbsence', ['$locale', absencePanel]);

	function absencePanel($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/addabsencepanel.html',
			scope: {
				selectedDate: '&',
				permissions: '='
			},
			controller: ['$timeout', addAbsenceCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {
				//scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				//scope.$on('$localeChangeSuccess', function () {
				//	scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				//});
				//scope.vm.isMiniMode = 'mini' in attr;
			}
		};
	};


	function addAbsenceCtrl($timeout) {
		var vm = this;
		vm.selectedAbsenceStartDate = vm.selectedDate();
		vm.selectedAbsenceEndDate = vm.selectedDate();
		//vm.absencePermissions = vm.permissions;
		//	{
		//	IsAddIntradayAbsenceAvailable: vm.permissions.IsAddIntradayAbsenceAvailable,
		//	IsAddFullDayAbsenceAvailable: vm.permissions.IsAddFullDayAbsenceAvailable
		//};
	};

})();
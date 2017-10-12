(function (angular) {
	'use strict';
	angular.module('wfm.teamSchedule').component('teamsExportSchedule', {
		controller: TeamsExportScheduleCtrl,
		templateUrl: 'app/teamSchedule/html/exportSchedule.html',
		controllerAs: 'vm'
	});

	TeamsExportScheduleCtrl.$inject = ['$q', 'groupPageService'];
	function TeamsExportScheduleCtrl($q, groupPageService) {
		var vm = this;

		vm.availableGroups = {BusinessHierarchy:[],GroupPages:[]};
		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};

		vm.getGroupPagesAsync = function (date) {
			var dateStr = moment(date).format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(dateStr, dateStr).then(function (data) {
				vm.availableGroups = data;
			});
		};
		
		vm.$onInit = function () {
			vm.getGroupPagesAsync(new Date());
		}
	}
})(angular);
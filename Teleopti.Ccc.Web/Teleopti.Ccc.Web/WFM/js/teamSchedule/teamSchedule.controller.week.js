(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyCtrl', TeamScheduleWeeklyCtrl);

	TeamScheduleWeeklyCtrl.$inject = ['$stateParams'];
	function TeamScheduleWeeklyCtrl(params) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined(params.keyword) && params.keyword !== '' ? params.keyword : '',
			searchKeywordChanged: false
		}

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadSchedules();
		}

		vm.scheduleDate = angular.isDefined(params.selectedDate) ? params.selectedDate : new Date();

		vm.loadSchedules = function () {
			vm.isLoading = true;
//TODO : load week schedule 
		};
	}
})()
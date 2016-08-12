(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyCtrl', TeamScheduleWeeklyCtrl);

	TeamScheduleWeeklyCtrl.$inject = ['$stateParams', 'PersonScheduleWeekViewCreator'];
	function TeamScheduleWeeklyCtrl(params, WeekViewCreator) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined(params.keyword) && params.keyword !== '' ? params.keyword : '',
			searchKeywordChanged: false
		};

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadSchedules();
		};
		vm.isLoading = false;
		
		vm.scheduleDate = params.selectedDate ? moment(params.selectedDate).startOf('week').toDate() : moment().startOf('week').toDate();

		vm.gotoPreviousWeek = function () { };
		vm.gotoNextWeek = function () { };

		vm.onScheduleDateChanged = function () { 
			if (vm.scheduleDate != moment(vm.scheduleDate).startOf('week').toDate()) {
				vm.scheduleDate = moment(vm.scheduleDate).startOf('week').toDate();
			}
		};

		vm.loadSchedules = function () {
			console.log("fake data");
			vm.isLoading = true;
			//TODO : load week schedule 
			vm.weekDayNames = ["Monday", "Tuesday", "Wendesday", "Thursday", "Friday", "Saturday", "Sunday"];
			var fakeData = [
				{
					PersonId: "ashley",
					Name: "ashley",
					Days: [
						{
							Date: "2016-08-08",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: 'grey'
						}, {
							Date: "2016-08-09",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#ffffff'
						}, {
							Date: "2016-08-10",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: 'grey'
						}, {
							Date: "2016-08-11",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-12",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-13",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}, {
							Date: "2016-08-14",
							SummeryTitle: "Early",
							SummeryTimeSpan: "08:00-17:00",
							Color: '#FFFFFF'
						}
					]
				}
			];
			vm.groupWeeks = WeekViewCreator.Create(fakeData);

		};
		vm.loadSchedules();

	}
})()
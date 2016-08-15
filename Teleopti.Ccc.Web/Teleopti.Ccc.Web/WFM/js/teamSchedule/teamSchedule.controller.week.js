(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyCtrl', TeamScheduleWeeklyCtrl);

	TeamScheduleWeeklyCtrl.$inject = ['$stateParams', '$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService'];
	function TeamScheduleWeeklyCtrl(params, $locale,$filter, WeekViewCreator, Util) {
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

		vm.weekDayNames = Util.getWeekdayNames();
		vm.weekDays = [];


		vm.loadSchedules = function () {
			console.log("fake data");
			vm.isLoading = true;
		    //TODO : load week schedule 
			vm.weekDays = getWeekDays();
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

		function getWeekDays() {
			if (vm.scheduleDate) {
				var dates = [];
				for (var i = 0; i < 7; i++) {
					dates.push({
						name: vm.weekDayNames[i],
						date: $filter('date')(moment(vm.scheduleDate).add(i, 'days').toDate(), 'shortDate')
					});
				}
				return dates;
			}
			return [];
		}

		vm.loadSchedules();

	}
})()
(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyCtrl', TeamScheduleWeeklyCtrl);

	TeamScheduleWeeklyCtrl.$inject = ['$stateParams', '$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc'];
	function TeamScheduleWeeklyCtrl(params, $locale, $filter, WeekViewCreator, Util, weekViewScheduleSvc) {
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
			if (!moment(vm.scheduleDate).startOf('week').isSame(vm.scheduleDate, 'day')) {
				vm.scheduleDate = moment(vm.scheduleDate).startOf('week').toDate();
			} else {
				vm.loadSchedules();
			}
		};

		vm.weekDayNames = Util.getWeekdayNames();
		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.loadSchedules = function () {		
			vm.isLoading = true;
			//TODO: replace fake with real		
			var inputForm = getParamsForLoadingSchedules();
			weekViewScheduleSvc.getFakeSchedules(inputForm).then(function(schedulesData) {
				vm.groupWeeks = WeekViewCreator.Create(schedulesData);
				vm.isLoading = false;
			}).catch(function() {
				vm.isLoading = false;
			});
		};

		init();

		function init() {
			vm.weekDays = Util.getWeekdays();
			vm.paginationOptions.totalPages = 1;
			vm.loadSchedules();
		}

		function getParamsForLoadingSchedules(options) {
			if (options == undefined) options = {};
			var params = {
				keyword: options.keyword != undefined ? options.keyword : vm.searchOptions.keyword,
				date: options.date != undefined ? options.date : moment(vm.scheduleDate).format("YYYY-MM-DD"),
				pageSize: options.pageSize != undefined ? options.pageSize : vm.paginationOptions.pageSize,
				currentPageIndex: options.currentPageIndex != undefined ? options.currentPageIndex : vm.paginationOptions.pageNumber,
			};
			return params;
		}

	}
})()
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

		vm.isLoading = false;

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadSchedules();
		};
		
		vm.scheduleDate = params.selectedDate ? moment(params.selectedDate).startOf('week').toDate() : moment().startOf('week').toDate();
	
		vm.onScheduleDateChanged = function () { 
			if (!moment(vm.scheduleDate).startOf('week').isSame(vm.scheduleDate, 'day')) {
				vm.scheduleDate = moment(vm.scheduleDate).startOf('week').toDate();
			}
			vm.weekDays = Util.getWeekdays(vm.scheduleDate);
			vm.loadSchedules();			
		};
	
		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.loadSchedules = function () {		
			vm.isLoading = true;			
			var inputForm = getParamsForLoadingSchedules();
			weekViewScheduleSvc.getSchedules(inputForm).then(function (data) {				
				vm.groupWeeks = WeekViewCreator.Create(data.PersonWeekSchedules);
				vm.paginationOptions.totalPages = vm.paginationOptions.pageSize > 0? Math.ceil(data.Total / (vm.paginationOptions.pageSize + 0.01) ) : 0;
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
				Keyword: options.keyword != undefined ? options.keyword : vm.searchOptions.keyword,
				Date: options.date != undefined ? options.date : moment(vm.scheduleDate).format("YYYY-MM-DD"),
				PageSize: options.pageSize != undefined ? options.pageSize : vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex != undefined ? options.currentPageIndex : vm.paginationOptions.pageNumber,
			};
			return params;
		}

	}
})()
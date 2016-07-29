(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService', 'Toggle', 'LeaderBoardViewModelFactory'];

	function LeaderBoardCtrl(LeaderBoardSvc, ToggleSvc, VMFactory) {
		var vm = this;
		vm.isLoading = true;
		vm.selectedDate = new Date();
		vm.dateRangePickerTemplateType = "inline";
		vm.toggleDateRangePicker = false;
		vm.showInputTimeErrorMessage = false;
		vm.isLeaderBoardEnabled = ToggleSvc.WfmReportPortal_LeaderBoard_39440;

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: ToggleSvc.WfmPeople_AdvancedSearch_32973,
			searchKeywordChanged: false
		};

		vm.selectedPeriod = {
			startDate: new Date(moment(vm.selectedDate).subtract(30,'days')),
			endDate: new Date(moment(vm.selectedDate).subtract(1, 'days'))
		};

		vm.shortDateFormatStart = moment(vm.selectedPeriod.startDate).format('YYYY-MM-DD');
		vm.shortDateFormatEnd = moment(vm.selectedPeriod.endDate).format('YYYY-MM-DD');

		vm.toggleDateRangePickerFn = function($event, status){
			$event.stopPropagation();
			vm.toggleDateRangePicker = !vm.toggleDateRangePicker;		
			if(vm.toggleDateRangePicker && !vm.selectedPeriod){
				vm.selectedPeriod = {
					startDate: new Date(vm.shortDateFormatStart), 
					endDate: new Date(vm.shortDateFormatEnd) 
				};
			}

			checkInputTimeErrorMessageStatus();
		};

		vm.toggleOffDateRangePickerFn = function(){
			vm.toggleDateRangePicker = false;
			checkInputTimeErrorMessageStatus();
		};

		vm.afterShortDateStringChange = function() {
			var start = new Date(vm.shortDateFormatStart);
			var end = new Date(vm.shortDateFormatEnd);

			if((!isNaN(start.getTime()) && start.getTime() > 0) && (!isNaN(end.getTime()) && end.getTime() > 0)){
				vm.selectedPeriod = {
					startDate: start,
					endDate: end
				};
				vm.afterSelectedDateChange();
			}

			checkInputTimeErrorMessageStatus();
		};

		function checkInputTimeErrorMessageStatus(){
			var start = new Date(vm.shortDateFormatStart);
			var end = new Date(vm.shortDateFormatEnd);

			if(vm.toggleDateRangePicker){
				vm.showInputTimeInvalidErrorMessage = false;
				vm.showInputTimeFormatErrorMessage = false;
			}else{
				vm.showInputTimeInvalidErrorMessage = start > end
				vm.showInputTimeFormatErrorMessage = (isNaN(start.getTime()) || start.getTime() <= 0) || (isNaN(end.getTime()) || end.getTime() <= 0);
			}
		};

		vm.afterSelectedDateChange = function() {
			if (vm.selectedPeriod && vm.selectedPeriod.startDate <= vm.selectedPeriod.endDate) {
				vm.isLoading = true;
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function(data) {
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
				vm.shortDateFormatStart = moment(vm.selectedPeriod.startDate).format('YYYY-MM-DD');
				vm.shortDateFormatEnd = moment(vm.selectedPeriod.endDate).format('YYYY-MM-DD');
			}
		};

		vm.onKeyWordInSearchInputChanged = function () {
			if (vm.selectedPeriod && vm.selectedPeriod.startDate <= vm.selectedPeriod.endDate) {
				vm.isLoading = true;
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function(data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}else{
				vm.selectedPeriod = {
					startDate: new Date(vm.shortDateFormatStart),
					endDate: new Date(vm.shortDateFormatEnd)
				};
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function(data) {
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}
		};

		vm.init = function () {
			LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function (data) {
				vm.searchOptions.keyword = data.Keyword;
				vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
				vm.isLoading = false;
			});
		};

		vm.isLeaderBoardEnabled && vm.init();
	}
})();
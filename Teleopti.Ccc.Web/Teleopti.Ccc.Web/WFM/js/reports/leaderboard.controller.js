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


		vm.afterShortDateStringChange = function(){
			vm.isInputDateValid = true;
			var currentDayStart = new Date(vm.shortDateFormatStart);
			var currentDayEnd = new Date(vm.shortDateFormatEnd);
			var newSelectedPeriod = {startDate: null, endDate: null};

			if (!isNaN(currentDayStart.getTime()) && currentDayStart.getTime() > 0) {
				newSelectedPeriod.startDate = new Date(vm.shortDateFormatStart);
			} else {
				vm.isInputDateValid = false;
			}

			if (!isNaN(currentDayEnd.getTime()) && currentDayEnd.getTime() > 0) {
				newSelectedPeriod.endDate = new Date(vm.shortDateFormatEnd);
			} else {
				vm.isInputDateValid = false;
			}

			if(vm.isInputDateValid){
				vm.selectedPeriod = newSelectedPeriod;
				vm.afterSelectedDateChange();
			}
		}

		vm.afterSelectedDateChange = function (start, end) {
			if(vm.selectedPeriod && vm.selectedPeriod.startDate < vm.selectedPeriod.endDate){
				vm.isLoading = true;
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function(data){
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
				vm.shortDateFormatStart = moment(vm.selectedPeriod.startDate).format('YYYY-MM-DD');
				vm.shortDateFormatEnd = moment(vm.selectedPeriod.endDate).format('YYYY-MM-DD');
			}
		};

		vm.onKeyWordInSearchInputChanged = function () {
			vm.isLoading = true;
			LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function (data) {
				vm.searchOptions.keyword = data.Keyword;
				vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
				vm.isLoading = false;
			});
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
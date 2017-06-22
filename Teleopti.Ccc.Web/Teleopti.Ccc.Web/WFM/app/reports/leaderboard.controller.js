(function() {
	'use strict';

	angular.module('wfm.reports')
		.controller('LeaderBoardController', ['$scope', 'LeaderBoardService', 'Toggle', 'LeaderBoardViewModelFactory', LeaderBoardCtrl]);

	function LeaderBoardCtrl($scope, LeaderBoardSvc, ToggleSvc, LeaderBoardViewModelFactory) {
		var vm = this;
		vm.isLoading = true;
		vm.selectedDate = new Date();
		vm.showInputTimeErrorMessage = false;
		vm.isLeaderBoardEnabled = ToggleSvc.WfmReportPortal_LeaderBoard_39440;

		vm.searchOptions = {
			keyword: '',
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};

		vm.selectedPeriod = {
			startDate: new Date(moment(vm.selectedDate).subtract(30, 'days')),
			endDate: new Date(moment(vm.selectedDate).subtract(1, 'days'))
		};
		vm.dateRangePickerTemplateType = 'popup';

		vm.afterSelectedDateChange = function() {
			vm.loadLeaderBoardData();
		};

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadLeaderBoardData();
		};

		vm.loadLeaderBoardData = function() {
			if (vm.selectedPeriod && vm.selectedPeriod.startDate <= vm.selectedPeriod.endDate) {
				var period = {
					startDate: moment(vm.selectedPeriod.startDate).format('YYYY-MM-DD'),
					endDate: moment(vm.selectedPeriod.endDate).format('YYYY-MM-DD')
				};

				vm.isLoading = true;
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, period).then(function(data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = LeaderBoardViewModelFactory.Create(data.AgentBadges);
					vm.searchOptions.focusingSearch = false;
					vm.isLoading = false;
				});
			}

		};

		vm.init = function() {
			$scope.$watch(function() {
				return vm.selectedPeriod;
			}, function(newValue, oldValue) {
				if (newValue)
					vm.afterSelectedDateChange();
			});
		};

		vm.isLeaderBoardEnabled && vm.init();
	}
})();
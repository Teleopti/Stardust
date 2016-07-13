﻿(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService', 'Toggle', 'LeaderBoardViewModelFactory'];

	function LeaderBoardCtrl(LeaderBoardSvc, ToggleSvc, VMFactory) {
		var vm = this;
		vm.isLoading = true;
		vm.selectedDate = new Date();
		vm.dateRangePickerTemplateType = "popup";
		vm.isLeaderBoardEnabled = ToggleSvc.WfmReportPortal_LeaderBoard_39440;
		vm.showDatePicker = ToggleSvc.WfmReportPortal_LeaderBoardByPeriod_39620;

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: ToggleSvc.WfmPeople_AdvancedSearch_32973,
			searchKeywordChanged: false
		};

		vm.selectedPeriod = {
			startDate: new Date(moment(vm.selectedDate).subtract(30,'days')),
			endDate: new Date(moment(vm.selectedDate).subtract(1, 'days'))
		};

		vm.afterSelectedDateChange = function () {
			if(vm.showDatePicker && vm.selectedPeriod){
				vm.isLoading = true;
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function(data){
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}
		};

		vm.onKeyWordInSearchInputChanged = function () {
			vm.isLoading = true;
			if(vm.showDatePicker){
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function (data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}else{
				LeaderBoardSvc.getLeaderBoardDefaultData(vm.searchOptions.keyword).then(function (data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}
		};

		vm.init = function () {
			vm.isLoading = true;
			if(vm.showDatePicker){
				LeaderBoardSvc.getLeaderBoardDataByPeriod(vm.searchOptions.keyword, vm.selectedPeriod).then(function (data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}else{
				LeaderBoardSvc.getLeaderBoardDefaultData(vm.searchOptions.keyword).then(function (data) {
					vm.searchOptions.keyword = data.Keyword;
					vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
					vm.isLoading = false;
				});
			}
		};

		vm.isLeaderBoardEnabled && vm.init();
	}
})();
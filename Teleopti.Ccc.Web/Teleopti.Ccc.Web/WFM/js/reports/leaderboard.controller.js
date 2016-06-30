(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService', 'Toggle', 'LeaderBoardViewModelFactory'];

	function LeaderBoardCtrl(LeaderBoardSvc, ToggleSvc, VMFactory) {
		var vm = this;

		vm.onKeyWordInSearchInputChanged = function () {
			LeaderBoardSvc.getLeaderBoardData(vm.searchOptions.keyword).then(function (data) {
				vm.searchOptions.keyword = data.Keyword;
				vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
			});
		};

		vm.init = function () {

			vm.searchOptions = {
				keyword: '',
				isAdvancedSearchEnabled: ToggleSvc.WfmPeople_AdvancedSearch_32973,
				searchKeywordChanged: false
			};

			LeaderBoardSvc.getLeaderBoardData(vm.searchOptions.keyword).then(function (data) {
				vm.searchOptions.keyword = data.Keyword;
				vm.leaderBoardTableList = VMFactory.Create(data.AgentBadges);
			});
		};

		vm.init();
	}
})();
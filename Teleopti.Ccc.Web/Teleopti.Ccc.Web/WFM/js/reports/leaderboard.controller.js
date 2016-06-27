(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService', 'Toggle'];

	function LeaderBoardCtrl(LeaderBoardSvc, ToggleSvc) {
		var vm = this;

		vm.onKeyWordInSearchInputChanged = function () {
			LeaderBoardSvc.getLeaderBoardData(vm.searchOptions.keyword).then(function (data) {
				vm.searchOptions.keyword = data.Keyword;
				vm.leaderBoardTableList = data.AgentBadges;
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
				vm.leaderBoardTableList = data.AgentBadges;
			});
		};

		vm.init();
	}
})();
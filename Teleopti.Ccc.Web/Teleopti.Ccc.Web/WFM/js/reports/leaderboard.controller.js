(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService'];


	function LeaderBoardCtrl(LeaderBoardSvc) {

		var vm = this;

		vm.searchOptions = {};

	}
})();
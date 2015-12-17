'use stirct';

(function () {

	angular.module('wfm.teamSchedule').directive('scheduleListContainer', scheduleListContainerDirective);


	function scheduleListContainerDirective() {
		return {
			scope: {
				scheduleInfo: '=scheduleListContainer'
			},
			restrict: 'A',
			controllerAs: 'vm',
			link: linkFunction,
			bindToController: true,
			controller: scheduleListContainerCtrl
		};

	};



	function scheduleListContainerCtrl() {

		var vm = this;

		vm.init = function () {

			convertScheduleInfoToScheduleViewModel(vm);
		}
		

	};

	function linkFunction(scope, element, attr) {

		scope.vm.init();
	}

	function convertScheduleInfoToScheduleViewModel(vm) {

		vm.scheduleList = vm.scheduleInfo;
	}


})();
'use strict';

(function () {

	function seatmapRightPanel() {
		return {
			controller: ['$mdSidenav', function ($mdSidenav) {
				var vm = this;

				vm.openRightPanel = function () {
					if (!$mdSidenav('right-panel').isOpen()) {
						$mdSidenav('right-panel').open();
					}
				};

				vm.closeRightPanel = function () {
					if ($mdSidenav('right-panel').isOpen()) {
						$mdSidenav('right-panel').close();
					}
				};

				vm.toggleRightPanel = function () {
					$mdSidenav('right-panel').toggle();
				};

			}],
			controllerAs: 'vm',
			scope: {
			},
			transclude: true,
			templateUrl: 'js/seatManagement/html/rightpanel.html'
		}
	};

	angular.module('wfm.seatPlan').directive('seatmapRightPanel', seatmapRightPanel);
})();
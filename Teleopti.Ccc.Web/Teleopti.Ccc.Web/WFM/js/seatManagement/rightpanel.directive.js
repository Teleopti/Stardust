'use strict';

(function () {

	angular.module('wfm.seatMap').controller('seatmapRightPanelCtrl', seatmapRightPanelCtrl);
	seatmapRightPanelCtrl.$inject = ['$mdSidenav', '$scope'];

	function seatmapRightPanelCtrl($mdSidenav, $scope) {
		var vm = this;

		vm.openPanel = function () {
			if (!$mdSidenav('right-panel').isOpen()) {
				$mdSidenav('right-panel').open();
			}
			vm.showRightPanel = true;
			vm.showResizer = true;
		};

		vm.closePanel = function () {
			if ($mdSidenav('right-panel').isOpen()) {
				$mdSidenav('right-panel').close();
			}
			vm.showRightPanel = false;
			vm.showResizer = false;
		};

		vm.togglePanel = function () {
			if (vm.showRightPanel) {
				vm.openPanel();
			} else {
				vm.closePanel();
			}
		};

		$scope.$watch(function () { return vm.showRightPanel }, function () {
			vm.togglePanel();
		}, true);
	};

	angular.module('wfm.seatMap').directive('seatmapRightPanel', seatmapRightPanel);

	function seatmapRightPanel() {
		return {
			controller: 'seatmapRightPanelCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				showRightPanel: '=',
				showToggleButton: '=',
				panelTitle:'='
			},
			transclude: true,
			templateUrl: 'js/seatManagement/html/rightpanel.html'
		}
	};
})();
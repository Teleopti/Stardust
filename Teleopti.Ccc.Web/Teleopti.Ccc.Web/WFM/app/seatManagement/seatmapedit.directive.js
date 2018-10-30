(function() {
	angular.module('wfm.seatMap').directive('seatmapEditor', seatmapEditorDirective);

	function seatmapEditorDirective() {
		return {
			controller: 'SeatMapEditController',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapEditor', '^seatmapCanvas'],
			scope: {},
			templateUrl: 'app/seatManagement/html/seatmapeditor.html',
			restrict: 'E',
			link: function(scope, element, attributes, controllers) {
				var vm = controllers[0];
				vm.parentVm = controllers[1];
				vm.init();
			}
		};
	}
})();

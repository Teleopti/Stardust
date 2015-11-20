(function () {

	angular.module('wfm.seatMap')
		.directive('seatmapEditor', seatmapEditorDirective);

	function seatmapEditorDirective() {
		return {
			controller: 'SeatMapEditCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapEditor', '^seatmapCanvas'],
			scope: {

			},
			templateUrl: "js/seatManagement/html/seatmapeditor.html",
			restrict: "E",
			link: linkFunction
		};
	};

	function linkFunction(scope, element, attributes, controllers) {
		var vm = controllers[0];
		vm.parentVm = controllers[1];
		vm.init();
	};

}());
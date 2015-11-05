(function () {

	var directive = function () {

		return {
			scope: {},
			controller: 'SeatMapEditCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapEditor','^seatmapCanvas'],
			templateUrl: "js/seatManagement/html/seatmapeditor.html",
			link: linkFunction
		};
	};

	angular.module('wfm.seatMap')
		.directive('seatmapEditor', directive);

	function linkFunction(scope, element, attributes, controllers) {
		var vm = controllers[0];
		var parentVm = controllers[1];
		vm.parentVm = parentVm;
	};

}());
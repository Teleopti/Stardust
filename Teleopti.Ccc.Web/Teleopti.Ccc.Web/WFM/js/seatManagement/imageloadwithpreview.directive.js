(function () {

	var directive = function () {

		return {
			templateUrl: "js/seatManagement/html/imageLoadWithPreview.html",
			link: linkFunction
		};
	};

	angular.module('wfm.seatMap')
		.directive('imageLoadWithPreview', directive);

	function linkFunction(scope, element, attributes, vm) {

	};

}());

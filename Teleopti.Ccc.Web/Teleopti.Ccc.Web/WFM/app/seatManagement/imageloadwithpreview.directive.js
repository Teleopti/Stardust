(function () {

	var directive = function () {

		return {
			templateUrl: "app/seatManagement/html/imageLoadWithPreview.html",
			link: linkFunction
		};
	};

	angular.module('wfm.seatMap')
		.directive('imageLoadWithPreview', directive);

	function linkFunction(scope, element, attributes, vm) {

	};

}());

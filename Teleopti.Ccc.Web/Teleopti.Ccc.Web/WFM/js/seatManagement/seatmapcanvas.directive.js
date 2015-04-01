'use strict';


angular.module('wfm.seatMap')
	.directive('keypressEvents', ['$document', '$rootScope',
	  function ($document, $rootScope) {
	  	return {
	  		restrict: 'A',
	  		link: function () {
	  			$document.bind('keypress', function (e) {
	  				console.log('Got keypress:', e.which);
	  				$rootScope.$broadcast('keypress', e);
	  				$rootScope.$broadcast('keypress:' + e.which, e);
	  			});
	  		}
	  	};
	  }
	]);



(function () {

	var directive = function () {

		return {
			controller: seatMapCanvasDirectiveController,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/seatManagement/html/seatmapcanvas.html",
			link: linkFunction

		};
	};

	angular.module('wfm.seatMap')
		.directive('seatmapCanvas', directive);



	function linkFunction(scope, element, attributes, vm) {

		//scope.vm.xxx

		//vm.xxx

	};

	function seatMapCanvasDirectiveController($document, $window, canvasUtils, canvasEditor, seatMapService) {

		var vm = this;
		var canvas = new fabric.CanvasWithViewport('c');

		vm.allowEdit = false;
		vm.seatMapId = null;
		vm.parentId = null;

		init();

		vm.toggleMoveMode = function () {
			canvasUtils.toggleMoveMode(canvas);
		};

		vm.toggleEditMode = function () {
			vm.allowEdit = !vm.allowEdit;
			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
		};

		function init() {

			canvasUtils.setSelectionMode(canvas, vm.allowEdit);
			canvasUtils.setupCanvas(canvas);
			setupHandleLocationClick();

			$(document).ready(function () {
				$(window).resize(resize);
			});

			resize();
			createListenersKeyboard();
			loadSeatMapFromId(null);

		};

		function resize() {
			canvasUtils.resize(canvas);
		};

		function createListenersKeyboard() {
			document.onkeydown = onKeyDownHandler;
		};

		function onKeyDownHandler(event) {
			//Robtodo: edit/non edit mode
			//if (self.allowEdit()) {
			canvasEditor.OnKeyDownHandler(canvas, event);
			//}

		};

		function loadSeatMapFromId(id) {
			seatMapService.seatMap.query(id).$promise.then(function (data) {
				loadSeatMap(data);
			});
		}

		function loadSeatMap(data) {
			if (data != null) {

				vm.parentId = data.ParentId;
				vm.Id = data.Id;

				canvasUtils.loadSeatMap(canvas, data, vm.allowEdit, onLoadSeatMapCompleted);

			}
			else {
				//self.Loading(false);
				//self.ResetZoom();
			}
		};

		function onLoadSeatMapCompleted(data) {

			//Robtodo: data has seatPriority, Id etc..

			//canvasEditor.LoadExistingSeatMapData(data);
			//self.breadcrumb(data.BreadcrumbInfo);

			//self.Loading(false);
			//self.ResetZoom();
			//self.CacheObjectsAsImages();

		}

		function setupHandleLocationClick() {
			canvas.on('mouse:down', function (e) {
				if (!vm.allowEdit) {
					var location = canvasUtils.getFirstObjectOfTypeFromCanvasObject(e.target, "location");
					if (location != null) {
						loadSeatMapOnLocationClick(location);
					}
				};
			});
		};

		function loadSeatMapOnLocationClick(location) {
			console.log(location.id);
			loadSeatMapFromId(location.id);
		};



	};

	seatMapCanvasDirectiveController.$inject = [
		'$document', '$window', 'seatMapCanvasUtilsService', 'seatMapCanvasEditService', 'seatMapService'
	];

}());


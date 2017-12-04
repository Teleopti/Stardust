'use strict';
(function () {
	angular.module('wfm.requests')
        .directive('synchronizeHeaderHeight', synchronizeHeaderHeightDirective);

	function synchronizeHeaderHeightDirective() {
		return {
			restrict: 'A',
			link: link
		};

		function link(scope, elem) {
			var binded = false;
			elem.on('click', function (e) {
				if (!binded) bindHeightWatcher();
			});
			function bindHeightWatcher() {
				var shiftTradeHeader = document.querySelectorAll('requests-overview[shift-trade-view] .ui-grid-header-canvas');
				var absenceAndTextHeader = document.querySelector('requests-overview:not([shift-trade-view]) .ui-grid-pinned-container-left .ui-grid-header-canvas');

				scope.$watch(function () {
					var shiftTradeCanvas = angular.element(document.querySelectorAll('requests-overview[shift-trade-view] .ui-grid-header-canvas'));
					var absenceAndTextCanvas = angular.element(document.querySelectorAll('requests-overview:not([shift-trade-view]) .ui-grid-header-canvas'));

					$(shiftTradeHeader[0]).height(shiftTradeCanvas[2].clientHeight); // synchronize with the third one since it's the only one moving, for now..
					$(shiftTradeHeader[1]).height(shiftTradeCanvas[2].clientHeight);
					$(absenceAndTextHeader).height(absenceAndTextCanvas[1].clientHeight);
				});
				binded = true;
			}
		}
	}
}());

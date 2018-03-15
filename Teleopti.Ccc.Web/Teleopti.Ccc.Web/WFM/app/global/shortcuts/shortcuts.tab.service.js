(function (angular) {
	'use strict';
	angular
		.module('shortcutsService')
		.service('TabShortCut', TabShortCut);

	TabShortCut.$inject = ['$document', 'keyCodes'];

	function TabShortCut($document, keyCodes) {
		var focusStyle = '1px solid #09F';
		var tabbedTargets = [];

		this.unifyFocusStyle = function () {
			$document.on('keyup', function (event) {
				if (event.keyCode === keyCodes.TAB) {
					traverseWithTab(event);
				}
			});

			$document.on('foucsout', function () {
				clearTabbedTarget();
			});

			function traverseWithTab(event) {
				tabbedTargets.push(event);
				tabbedTargets[0].target.style.outline = focusStyle;

				if (tabbedTargets.length > 1) {
					tabbedTargets[0].target.style.outline = "";
					tabbedTargets[1].target.style.outline = focusStyle;
					tabbedTargets.splice(0, 1);
				}
			};

			function clearTabbedTarget() {
				if (!tabbedTargets.length)
					return;
				tabbedTargets.forEach(function (e) {
					e.target.style.outline = "";
				})
				tabbedTargets = [];
			};
		}

	}

})(angular);
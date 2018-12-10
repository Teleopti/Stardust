(function() {
	angular.module('wfm.helpingDirectives', []).directive('outsideClick', ['$parse', outsideClickDirective]);

	function outsideClickDirective($parse) {
		return {
			restrict: 'A',
			link: function(scope, element, attrs) {
				var outsideClickBackdrop = document.createElement('div');
				outsideClickBackdrop.classList = 'outside-click-backdrop';
				outsideClickBackdrop.style.position = 'fixed';
				outsideClickBackdrop.style.top = 0;
				outsideClickBackdrop.style.right = 0;
				outsideClickBackdrop.style.bottom = 0;
				outsideClickBackdrop.style.left = 0;
				outsideClickBackdrop.style.zIndex = '50';
				outsideClickBackdrop.style.background = 'rgba(0,0,0,.16)';
				outsideClickBackdrop.addEventListener('click', clickEventHandler);

				document.body.appendChild(outsideClickBackdrop);

				element[0].style.zIndex = '100';

				scope.$on('$destroy', function() {
					document.body.removeChild(outsideClickBackdrop);
				});

				function clickEventHandler(event) {
					if (element[0].contains(event.target)) {
						return;
					}
					scope.$apply(function() {
						$parse(attrs.outsideClick)(scope, { $event: event });
					});
				}
			}
		};
	}
})();

(function() {
	'use strict';
	/**
	 * Add a 'scrollfix' class to the element when the page scrolls past it's position.Go crazy with this class.
	 * pass a number to override the detected offset.
	 * for exemple scrollfix="+10"
	 */
	angular.module('wfm.outbound').directive('scrollfix', [scrollfixCtrl]);

	function scrollfixCtrl() {
		return {
			link: postLink
		};

		function postLink(scope, elm, attrs, scrollfixTarget) {
			var top = elm[0].offsetTop,
				$target = angular.element(document.querySelector('#materialcontainer'));
			if (!attrs.scrollfix) {
				attrs.scrollfix = top;
			} else if (typeof (attrs.scrollfix) === 'string') {
				if (attrs.scrollfix.charAt(0) === '-') {
					attrs.scrollfix = top - parseFloat(attrs.scrollfix.substr(1));
				} else if (attrs.scrollfix.charAt(0) === '+') {
					attrs.scrollfix = top + parseFloat(attrs.scrollfix.substr(1));
				}
			}

			function onScroll() {
				var offset = $target[0].scrollTop;
				if (!elm.hasClass('scrollfix') && offset > attrs.scrollfix) {
					elm.addClass('scrollfix');
				} else if (elm.hasClass('scrollfix') && offset <= attrs.scrollfix) {
					elm.removeClass('scrollfix');
				}
			}

			$target.on('scroll', onScroll);

			scope.$on('$destroy', function() {
				$target.off('scroll', onScroll);
			});
		}
	}
})();


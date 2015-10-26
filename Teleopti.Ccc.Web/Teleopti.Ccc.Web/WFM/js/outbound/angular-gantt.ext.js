(function() {

	'use strict';

	angular.module('gantt').config(['$provide', function ($provide) {

		$provide.decorator('GanttScroll', [
			'$delegate', function($delegate) {
				$delegate.prototype.getBordersWidth = function () {
					var result = this.$element === undefined ? undefined : (this.$element[0].offsetWidth - $(this.$element[0]).width());
					return result;
				};
				return $delegate;
			}
		]);
	}]);

	// gantt-column-header-content
	angular.module('gantt').directive('ganttColumnHeaderContent', ganttColumnHeaderContent);

	function ganttColumnHeaderContent() {
		return {
			scope: {
				labelContent: '@',
				labelUnit: '@'
			},
			link: postlink
		}

		function postlink(scope, elem, attrs) {		
			if (isContentLikeHtml(scope.labelContent)) {
				var contentElem = angular.element(scope.labelContent);
				elem.append(contentElem);
			} else {
				elem.text(scope.labelContent);
			}
		}
	}
	function isContentLikeHtml(content) {
		return /^<.*>$/.test(content);
	}

})();
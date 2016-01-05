(function() {

	'use strict';

	angular.module('gantt').config(['$provide', function ($provide) {

		$provide.decorator('GanttScroll', [
			'$delegate', function ($delegate) {
				$delegate.prototype.getBordersWidth = function() {
					if (this.$element === undefined) {
						return undefined;
					}

					if (this.$element[0].clientWidth) {
						return this.$element[0].offsetWidth - this.$element[0].clientWidth;
					} else {
						//fix for IE11
						var borderLeft = window.getComputedStyle(this.$element[0]).getPropertyValue('border-left-width') ? window.getComputedStyle(this.$element[0]).getPropertyValue('border-left-width').match(/\d+/)[0] : 0;
						var borderRight = window.getComputedStyle(this.$element[0]).getPropertyValue('border-right-width') ? window.getComputedStyle(this.$element[0]).getPropertyValue('border-right-width').match(/\d+/)[0] : 0;

						return parseInt(borderLeft) + parseInt(borderRight);
					}
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
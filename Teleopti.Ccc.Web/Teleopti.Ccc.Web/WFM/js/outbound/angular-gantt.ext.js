(function() {

	'use strict';

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
(function() {
    'use strict';

    angular.module('wfm.outbound').directive('expandable', [
		'$animate',
		function ($animate) {
		    return {
		        restrict: 'A',
		        link: postLink,
		        scope: {}
		    };

		    function postLink(scope, elem, attrs) {
		        scope.$on('expandable.expand', function () {
		            $animate.addClass(elem, 'expand').then(function () { elem.removeClass('expand'); });
		        });

		    }
		}
    ]);
})();
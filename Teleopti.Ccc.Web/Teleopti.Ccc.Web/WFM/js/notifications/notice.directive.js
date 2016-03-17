'use strict';
(function() {
	var wfmNotice = angular.module('wfm.notice');

	wfmNotice.directive('wfmNotice', function($timeout, $rootScope) {
		return {
			restrict: 'EA',
			scope: {
				notices: '='
			},
			link: function(scope, el, ctrl) {
				scope.notices = [];
				function deleteNotice(notice) {
					var index = scope.notices.indexOf(notice);
					if (index > -1) {
						scope.notices.splice(index, 1);
					}
				}

				function destroyOnStateChange(notice) {
					$rootScope.$on('$stateChangeSuccess', function() {
						if (notice.destroyOnStateChange) {
							deleteNotice(notice);
						} else {
							return;
						}
					});
				};

				scope.$watch('notices', function(newNotices) {
					newNotices.forEach(function(notice) {
						destroyOnStateChange(notice);
						if (notice.timeToLive !== null) {
							$timeout(function() {
								deleteNotice(notice);
							}, notice.timeToLive);
						} else {
							return;
						}
					});
				});
			},
			template: '<div ng-repeat="notice in notices"><div class="wfm-notice">{{notice}}</div></div>',
			controller: function($scope) {
			}
		}
	});

})();

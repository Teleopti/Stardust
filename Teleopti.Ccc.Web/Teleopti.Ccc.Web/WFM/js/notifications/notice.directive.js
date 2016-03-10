'use strict';
(function() {
	var wfmNotice = angular.module('wfm.notice', ['angular-growl', 'ngAnimate']);
	wfmNotice.controller('wfmNoticeCtrl', wfmNoticeCtrl);
	wfmNoticeCtrl.$inject = ['$scope', 'growl'];

	function wfmNoticeCtrl($scope, growl) {

		$(function() {
			var mainPageContainerElement = $('#materialcontainer');
			var notificationBarElement = $('#notice');

			mainPageContainerElement.scroll(function() {
				var uiHeight = $('[ui-view]').height();
				var containerHeight = mainPageContainerElement.height();
				var scrollTop = mainPageContainerElement.scrollTop();

				if (uiHeight > containerHeight) {
					notificationBarElement.css({
						bottom: -scrollTop
					});
				} else {
					notificationBarElement.css({
						bottom: 0
					});
				}
			});

			$(window).resize(function() {
				if ($('.growl-container>.growl-item').length > 0) {
					mainPageContainerElement.scrollTop(0);
				}
			});
		});
	}

	wfmNotice.directive('wfmNotice', wfmNoticeDirective);

	function wfmNoticeDirective() {
		return {
			controller: 'wfmNoticeCtrl',
			template: "<div growl></div>",
			scope: true
		}
	}

})();

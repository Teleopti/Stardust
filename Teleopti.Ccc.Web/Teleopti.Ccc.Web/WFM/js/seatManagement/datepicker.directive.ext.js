'use strict';

(function () {

	angular.module('wfm.seatPlan').config(function ($provide) {
		$provide.decorator('datepickerDirective', function ($delegate) {
			var directive = $delegate[0];
			var link = directive.link;

			directive.$$isolateBindings['onChangeOfMonth'] = {
				attrName: 'onChangeOfMonth',
				mode: '&',
				optional: true
			};

			directive.compile = function () {
				return function (scope, element, attrs, ctrl) {
					// set start of week based on locale.
					ctrl[0].startingDay = moment.localeData()._week.dow;


					link.apply(this, arguments);
					scope.$watch(function () {
						return ctrl[0].activeDate.getTime();
					}, function () {
						scope.onChangeOfMonth({ date: ctrl[0].activeDate });
					});
				}
			};

			return $delegate;
		});
	});
}());

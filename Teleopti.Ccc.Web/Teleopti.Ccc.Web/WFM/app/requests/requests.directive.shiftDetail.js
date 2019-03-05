'use strict';

(function() {
	angular
		.module('wfm.requests')
		.directive('requestsShiftDetail', [
			'shiftTradeScheduleService',
			'serviceDateFormatHelper',
			'GroupScheduleFactory',
			'requestScheduleService',
			requestsShiftDetailDirective
		]);

	function requestsShiftDetailDirective(
		shiftTradeScheduleService,
		serviceDateFormatHelper,
		groupScheduleFactory,
		requestScheduleService
	) {
		return {
			restrict: 'A',
			scope: {
				personIds: '=',
				date: '=',
				targetTimezone: '=',
				showShiftDetail: '&'
			},
			link: function(scope, elem, attrs, ctrls) {
				var personIds = scope.personIds;
				var scheduleDate = serviceDateFormatHelper.getDateOnly(scope.date);
				var targetTimezone = scope.targetTimezone;
				var showShiftDetail = scope.showShiftDetail;

				elem.bind('click', function(event) {
					event.stopPropagation();

					var position = requestScheduleService.buildScheduleContainerStyle(personIds.length, event);
					updateShiftStatusForSelectedPerson(
						personIds,
						scheduleDate,
						targetTimezone,
						position,
						showShiftDetail
					);
				});

				function updateShiftStatusForSelectedPerson(
					personIds,
					scheduleDate,
					_targetTimeZone,
					position,
					showShiftDetail
				) {
					if (personIds.length === 0) {
						return;
					}
					shiftTradeScheduleService
						.getSchedules(scheduleDate, personIds[0], personIds[1])
						.then(function(result) {
							showShiftDetail &&
								showShiftDetail({
									params: {
										width: position.width,
										height: position.height,
										left: position.left,
										top: position.top,
										schedules: result,
										targetTimezone: _targetTimeZone
									}
								});
						});
				}
			}
		};
	}
})();

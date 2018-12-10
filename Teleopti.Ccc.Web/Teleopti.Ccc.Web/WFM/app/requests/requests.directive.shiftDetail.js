'use strict';

(function() {
	var requestsShiftDetailDirective = function (
		shiftTradeScheduleService,
		serviceDateFormatHelper
	) {
		return {
			restrict: 'A',
			scope: {
				personIds: '=',
				date: '=',
				targetTimezone: '=',
				showShiftDetail: '&'
			},
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {
			var personIds = scope.personIds;
			var scheduleDate = serviceDateFormatHelper.getDateOnly(scope.date);
			var targetTimezone = scope.targetTimezone;
			var showShiftDetail = scope.showShiftDetail;

			elem.bind('click', function(oEvent) {
				var position = getShiftDetailDisplayPosition(oEvent);
				updateShiftStatusForSelectedPerson(personIds[0], personIds[1], scheduleDate, targetTimezone, position, showShiftDetail);
				oEvent.stopPropagation();
			});
		}

		function getShiftDetailDisplayPosition(oEvent) {
			var shiftDetailLeft =
				document.body.clientWidth - oEvent.pageX < 710 ? document.body.clientWidth - 710 : oEvent.pageX - 5;
			var shiftDetailTop =
				document.body.clientHeight - oEvent.pageY < 145 ? document.body.clientHeight - 145 : oEvent.pageY - 5;

			return {
				top: shiftDetailTop,
				left: shiftDetailLeft
			};
		}

		function updateShiftStatusForSelectedPerson(
			personFromId,
			personToId,
			scheduleDate,
			_targetTimeZone,
			position,
			showShiftDetail
		) {
			if (!personFromId || !personToId) {
				return;
			}

			shiftTradeScheduleService.getSchedules(scheduleDate, personFromId, personToId).then(function(result) {
				showShiftDetail && showShiftDetail({ params: { left: position.left, top: position.top, schedules: result, targetTimezone: _targetTimeZone } });
			});
		}
	};

	angular
		.module('wfm.requests')
		.directive('requestsShiftDetail', [
			'shiftTradeScheduleService',
			'serviceDateFormatHelper',
			requestsShiftDetailDirective
		]);
})();

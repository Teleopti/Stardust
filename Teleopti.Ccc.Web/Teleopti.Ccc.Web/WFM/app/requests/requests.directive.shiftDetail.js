'use strict';

(function() {
	var requestsShiftDetailDirective = function(
		teamScheduleSvc,
		groupScheduleFactory,
		currentUserInfo,
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
				updateShiftStatusForSelectedPerson(personIds, scheduleDate, targetTimezone, position, showShiftDetail);
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
			personIds,
			scheduleDate,
			targetTimezone,
			position,
			showShiftDetail
		) {
			if (personIds.length === 0) {
				return;
			}

			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			teamScheduleSvc.getSchedules(scheduleDate, personIds).then(function(result) {
				var schedulesToDisplay = result.Schedules.filter(function(schedule) {
					return schedule.Date === scheduleDate;
				});
				var schedules = groupScheduleFactory.Create(schedulesToDisplay, scheduleDate, targetTimezone, 48);
				showShiftDetail &&
					showShiftDetail({ params: { left: position.left, top: position.top, schedules: schedules } });
			});
		}
	};

	angular
		.module('wfm.requests')
		.directive('requestsShiftDetail', [
			'TeamSchedule',
			'GroupScheduleFactory',
			'CurrentUserInfo',
			'serviceDateFormatHelper',
			requestsShiftDetailDirective
		]);
})();

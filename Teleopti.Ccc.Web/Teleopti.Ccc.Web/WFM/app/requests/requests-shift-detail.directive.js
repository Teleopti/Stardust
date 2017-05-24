"use strict";

(function() {

	var requestsShiftDetailDirective = function(teamScheduleSvc, groupScheduleFactory, currentUserInfo) {
		return {
			restrict: "A",
			require: ["^requestsTableContainer"],
			scope: {
				personIds: "=",
				date: "=",
				targetTimezone: "="
			},
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {
			var personIds = scope.personIds;
			var scheduleDate = scope.date;
			var targetTimezone = scope.targetTimezone;

			var requestsTableContainerCtrl = ctrls[0];
			elem.bind("click", function(oEvent) {
				var position = getShiftDetailDisplayPosition(oEvent);
				updateShiftStatusForSelectedPerson(personIds, moment(scheduleDate), targetTimezone,
					requestsTableContainerCtrl, position);
				oEvent.stopPropagation();
			});
		}

		function getShiftDetailDisplayPosition(oEvent) {
			var shiftDetailLeft = (document.body.clientWidth - oEvent.pageX) < 710
				? document.body.clientWidth - 710
				: oEvent.pageX - 5;
			var shiftDetailTop = (document.body.clientHeight - oEvent.pageY) < 145
				? document.body.clientHeight - 145
				: oEvent.pageY - 5;

			return {
				top: shiftDetailTop,
				left: shiftDetailLeft
			};
		}

		function updateShiftStatusForSelectedPerson(personIds, scheduleDate, targetTimezone, requestsTableContainerCtrl, position) {
			if (personIds.length === 0) {
				return;
			}

			var currentDate = scheduleDate.format("YYYY-MM-DD");
			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;

			teamScheduleSvc.getSchedules(currentDate, personIds)
				.then(function(result) {
					var schedulesToDisplay = result.Schedules.filter(function(schedule) {
						return schedule.Date === currentDate;
					});

					var scheduleStartDate = scheduleDate;
					if (schedulesToDisplay.length > 0 && targetTimezone != null && targetTimezone !== currentUserTimezone) {
						var currentUserUtcOffset = moment().tz(currentUserTimezone).utcOffset();
						var targetUserUtcOffset = moment().tz(targetTimezone).utcOffset();
						var timeDifferent = targetUserUtcOffset - currentUserUtcOffset;

						scheduleStartDate = moment(new Date(8640000000000000)); // Maximum date in js
						schedulesToDisplay.forEach(function(schedule) {
							// Time returned from server side is in current user's timezone;
							// Need convert into target timezone
							schedule.Projection.forEach(function(projection) {
								var startMoment = moment(projection.Start).add(timeDifferent, "minutes");
								projection.Start = startMoment.format("YYYY-MM-DD HH:mm");
								if (startMoment.isBefore(scheduleStartDate, "day")) {
									scheduleStartDate = startMoment;
								}

								var endMoment = moment(projection.End).add(timeDifferent, "minutes");
								projection.End = endMoment.format("YYYY-MM-DD HH:mm");
							});
						});
					}

					var schedules = groupScheduleFactory.Create(schedulesToDisplay, scheduleStartDate, 48);
					requestsTableContainerCtrl.showShiftDetail(position.top, position.left, schedules);
				});
		}
	};

	angular.module("wfm.requests").directive("requestsShiftDetail",
		["TeamSchedule", "GroupScheduleFactory", "CurrentUserInfo", requestsShiftDetailDirective]);
})();
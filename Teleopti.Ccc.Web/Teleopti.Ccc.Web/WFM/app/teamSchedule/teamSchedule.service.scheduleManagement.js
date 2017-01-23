(function() {
	'use strict';

	angular.module('wfm.teamSchedule').service('ScheduleManagement', ['Toggle', '$filter', 'TeamSchedule', 'GroupScheduleFactory', 'CurrentUserInfo', ScheduleManagementService]);

	function ScheduleManagementService(toggleSvc, $filter, teamScheduleSvc, groupScheduleFactory, CurrentUserInfo) {
		var svc = this;

		svc.rawSchedules = [];
		svc.groupScheduleVm = {};

		svc.findPersonScheduleVmForPersonId = findPersonScheduleVmForPersonId;
		svc.recreateScheduleVm = recreateScheduleVm;
		svc.resetSchedules = resetSchedules;
		svc.mergeSchedules = mergeSchedules;
		svc.updateScheduleForPeoples = updateScheduleForPeoples;
		svc.resetSchedulesForPeople = resetSchedulesForPeople;
		svc.getEarliestStartOfSelectedSchedules = getEarliestStartOfSelectedSchedules;
		svc.getLatestStartOfSelectedSchedules = getLatestStartOfSelectedSchedules;
		svc.getLatestPreviousDayOvernightShiftEnd = getLatestPreviousDayOvernightShiftEnd;
		svc.getLatestStartTimeOfSelectedSchedulesProjections = getLatestStartTimeOfSelectedSchedulesProjections;

		function findPersonScheduleVmForPersonId(personId) {
			var result = svc.groupScheduleVm.Schedules.filter(function(vm) {
				return vm.PersonId === personId;
			});
			if (result.length === 0) return null;
			return result[0];
		}

		function convertScheduleToTimezone(schedule, timezone) {

			if ((!timezone) || (timezone === CurrentUserInfo.CurrentUserInfo().DefaultTimeZone)) return schedule;

			var copiedSchedule = angular.copy(schedule);

			angular.forEach(copiedSchedule.Projection, function(p) {
				p.Start = $filter('timezone')(p.Start, timezone);
				p.End = $filter('timezone')(p.End, timezone);
			});
			if (copiedSchedule.DayOff) {
				copiedSchedule.DayOff.Start = $filter('timezone')(copiedSchedule.DayOff.Start, timezone);
				copiedSchedule.DayOff.End = $filter('timezone')(copiedSchedule.DayOff.End, timezone);
			}

			return copiedSchedule;
		}

		function recreateScheduleVm(scheduleDateMoment, timezone) {
			var timezoneAdjustedSchedules = svc.rawSchedules.map(function(schedule) {
				return convertScheduleToTimezone(schedule, timezone);
			});

			var useNextDaySchedules = toggleSvc.WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305;

			svc.groupScheduleVm = groupScheduleFactory.Create(timezoneAdjustedSchedules, scheduleDateMoment, useNextDaySchedules);
		}

		function resetSchedules(schedules, scheduleDateMoment, timezone) {
			svc.rawSchedules = schedules;
			recreateScheduleVm(scheduleDateMoment, timezone);
		}

		function mergeSchedules(schedules, scheduleDateMoment, timezone) {
			recreateScheduleVm(scheduleDateMoment, timezone);
		}

		function updateScheduleForPeoples(personIdList, scheduleDateMoment, timezone, afterLoading) {
			var scheduleDateStr = scheduleDateMoment.format('YYYY-MM-DD');
			teamScheduleSvc.getSchedules(scheduleDateStr, personIdList).then(function(result) {

				angular.forEach(result.Schedules, function(schedule) {
					for (var i = 0; i < svc.rawSchedules.length; i++) {
						if (schedule.PersonId === svc.rawSchedules[i].PersonId && svc.rawSchedules[i].Date === schedule.Date) {
							svc.rawSchedules[i] = schedule;
							break;
						}
					}
				});
				svc.mergeSchedules(svc.rawSchedules, scheduleDateMoment, timezone);
				afterLoading();
			});
		}

		function resetSchedulesForPeople(personIds) {
			angular.forEach(personIds, function(person) {
				var length = svc.groupScheduleVm.Schedules.length;
				for (var i = 0; i < length; i++) {
					var schedule = svc.groupScheduleVm.Schedules[i];
					var shiftsForSelectedDate;
					if (person === schedule.PersonId) {
						schedule.IsSelected = false;
						shiftsForSelectedDate = schedule.Shifts.filter(function (shift) {
							return shift.Date === schedule.Date;
						});
						if (shiftsForSelectedDate.length > 0) {
							angular.forEach(shiftsForSelectedDate[0].Projections, function(projection) {
								projection.Selected = false;
							});
						}
						break;
					}
				}
			});
		}

		function getEarliestStartOfSelectedSchedules(scheduleDateMoment, selectedPersonIds) {
			selectedPersonIds.forEach(function(x) {
				if (!angular.isString(x))
					throw 'Invalid parameter.';
			});

			var startUpdated = false;
			var earlistStart = moment('2099-12-31');

			svc.groupScheduleVm.Schedules.forEach(function(schedule) {
				var scheduleStart = moment(schedule.ScheduleStartTime());

				if (selectedPersonIds.indexOf(schedule.PersonId) > -1 && scheduleStart < earlistStart) {
					startUpdated = true;
					earlistStart = scheduleStart;
				}
			});

			if (!startUpdated) {
				// Set to 08:00 for empty schedule or day off
				earlistStart = scheduleDateMoment.startOf('day').add(8, 'hour');
			}

			return earlistStart.toDate();
		}

		function getLatestStartOfSelectedSchedules(scheduleDateMoment, selectedPersonIds) {
			selectedPersonIds.forEach(function(x) {
				if (!angular.isString(x))
					throw 'Invalid parameter.';
			});

			var startUpdated = false;
			var latestStart = scheduleDateMoment.startOf('day');

			svc.groupScheduleVm.Schedules.forEach(function(schedule) {
				var scheduleStart = moment(schedule.ScheduleStartTime());

				if (selectedPersonIds.indexOf(schedule.PersonId) > -1 && scheduleStart > latestStart) {
					startUpdated = true;
					latestStart = scheduleStart;
				}
			});

			return startUpdated ? latestStart.toDate() : null;
		}

		function getLatestPreviousDayOvernightShiftEnd(scheduleDateMoment, selectedPersonIds) {
			selectedPersonIds.forEach(function(x) {
				if (!angular.isString(x))
					throw 'Invalid parameter.';
			});

			var previousDayShifts = [];

			svc.groupScheduleVm.Schedules.forEach(function(schedule) {
				if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
					previousDayShifts = previousDayShifts.concat(schedule.Shifts.filter(function(shift) {
						return shift.Projections.length > 0 &&
							shift.Date !== scheduleDateMoment.format('YYYY-MM-DD');
					}));
				}
			});

			if (previousDayShifts.length === 0) return null;

			var latestEndTimeMoment = null;

			previousDayShifts.forEach(function(shift) {
				shift.Projections.forEach(function(projection) {
					var projectionEndMoment = moment(projection.Start).add(projection.Minutes, 'minute');
					if (latestEndTimeMoment === null || latestEndTimeMoment < projectionEndMoment)
						latestEndTimeMoment = projectionEndMoment;
				});
			});

			return latestEndTimeMoment ? latestEndTimeMoment.toDate() : null;
		}

		function getLatestStartTimeOfSelectedSchedulesProjections(scheduleDateMoment, selectedPersonIds) {

			selectedPersonIds.forEach(function(x) {
				if (!angular.isString(x))
					throw 'Invalid parameter.';
			});

			var latestStart = null;
			var projectionShiftLayerIds = [];
			var currentDayShifts = [];

			svc.groupScheduleVm.Schedules.forEach(function(schedule) {
				if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
					currentDayShifts = currentDayShifts.concat(schedule.Shifts.filter(function(shift) {
						return shift.Date === scheduleDateMoment.format('YYYY-MM-DD');
					}));
				}
			});
			currentDayShifts.forEach(function(shift) {
				if (shift.Projections) {
					shift.Projections.forEach(function(projection) {
						var scheduleStart = moment(projection.Start).toDate();
						if (projection.Selected && (latestStart === null || scheduleStart >= latestStart)) {
							var exist = projection.ShiftLayerIds && projection.ShiftLayerIds.some(function(layerId) {
								return projectionShiftLayerIds.indexOf(layerId) > -1;
							});
							if (exist) return;

							latestStart = scheduleStart;
							projectionShiftLayerIds = projectionShiftLayerIds.concat(projection.ShiftLayerIds);
						}
					});
				}
			});
			return latestStart;
		}
	}
})();
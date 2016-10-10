﻿(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ScheduleManagement", [
		"$resource", "Toggle", "$q", "$filter", 'TeamSchedule', 'GroupScheduleFactory', 'CurrentUserInfo',
		function ($resource, toggleSvc, $q, $filter, teamScheduleSvc, groupScheduleFactory, CurrentUserInfo) {
			var svc = this;

			svc.rawSchedules = [];
			svc.groupScheduleVm = {};

			function convertScheduleToTimezone(schedule, timezone) {

				if ((!timezone) || (timezone === CurrentUserInfo.CurrentUserInfo().DefaultTimeZone)) return schedule;

				var copiedSchedule = angular.copy(schedule);

				
				angular.forEach(copiedSchedule.Projection, function(p) {
					p.Start = $filter('timezone')(p.Start, timezone);
				});
								
				return copiedSchedule;
			}

			var recreateScheduleVm = function (scheduleDateMoment, timezone) {				
				var timezoneAdjustedSchedules = svc.rawSchedules.map(function(schedule) {
					return convertScheduleToTimezone(schedule, timezone);
				});
				svc.groupScheduleVm = groupScheduleFactory.Create(timezoneAdjustedSchedules, scheduleDateMoment);
			};

			svc.recreateScheduleVm = recreateScheduleVm;

			svc.resetSchedules = function (schedules, scheduleDateMoment, timezone) {
				svc.rawSchedules = schedules;
				recreateScheduleVm(scheduleDateMoment, timezone);
			};

			svc.mergeSchedules = function (schedules, scheduleDateMoment, timezone) {
				recreateScheduleVm(scheduleDateMoment, timezone);
			};

			svc.updateScheduleForPeoples = function(personIdList, scheduleDateMoment, timezone, afterLoading) {
				var scheduleDateStr = scheduleDateMoment.format('YYYY-MM-DD');
				teamScheduleSvc.getSchedules(scheduleDateStr, personIdList).then(function(result) {

					angular.forEach(result.Schedules, function(schedule) {
						var personId = schedule.PersonId;

						if (schedule.Date === scheduleDateStr) {
							for (var i = 0; i < svc.rawSchedules.length; i++) {
								if (personId === svc.rawSchedules[i].PersonId && svc.rawSchedules[i].Date === scheduleDateStr) {
									svc.rawSchedules[i] = schedule;
									break;
								}
							}

						}

					});
					svc.mergeSchedules(svc.rawSchedules, scheduleDateMoment, timezone);
					afterLoading();
				});
			};

			svc.resetSchedulesForPeople = function(personIds) {
				angular.forEach(personIds, function(person) {
					var length = svc.groupScheduleVm.Schedules.length;
					for (var i = 0; i < length; i++) {
						var schedule = svc.groupScheduleVm.Schedules[i];
						if (person === schedule.PersonId) {
							schedule.IsSelected = false;
							var shiftsForSelectedDate = schedule.Shifts.filter(function(shift) {
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

			};

			svc.getEarliestStartOfSelectedSchedule = function (scheduleDateMoment, selectedPersonIds) {
				selectedPersonIds.forEach(function (x) {
					if (typeof x !== 'string')
						throw "Invalid parameter.";
				});

				var startUpdated = false;
				var earlistStart = moment("2099-12-31");

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
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

			svc.getLatestStartOfSelectedSchedule = function (scheduleDateMoment, selectedPersonIds) {
				selectedPersonIds.forEach(function (x) {
					if (typeof x !== 'string')
						throw "Invalid parameter.";
				});

				var startUpdated = false;
				var latestStart = scheduleDateMoment.startOf('day');

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
					var scheduleStart = moment(schedule.ScheduleStartTime());
								
					if (selectedPersonIds.indexOf(schedule.PersonId) > -1 && scheduleStart > latestStart) {						
						startUpdated = true;
						latestStart = scheduleStart;
					}
				});

				return startUpdated ? latestStart.toDate() : null;
			};

			svc.getLatestPreviousDayOvernightShiftEnd = function (scheduleDateMoment, selectedPersonIds) {
				selectedPersonIds.forEach(function (x) {
					if (typeof x !== 'string')
						throw "Invalid parameter.";
				});

				var previousDayShifts = [];

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
					if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
						previousDayShifts = previousDayShifts.concat(schedule.Shifts.filter(function(shift) {
							return shift.Projections.length > 0
								&& shift.Date !== scheduleDateMoment.format('YYYY-MM-DD');
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

			svc.getLatestStartTimeOfSelectedScheduleProjection = function (scheduleDateMoment, selectedPersonIds) {

				selectedPersonIds.forEach(function(x) {
					if (typeof x !== 'string')
						throw "Invalid parameter.";
				});

				var latestStart = null;
				var projectionShiftLayerIds = [];
				var currentDayShifts = [];

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
					if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
						currentDayShifts = currentDayShifts.concat(schedule.Shifts.filter(function (shift) {
							return shift.Date === scheduleDateMoment.format('YYYY-MM-DD');
						}));
					}
				});
				currentDayShifts.forEach(function (shift) {
					if (shift.Projections) {
						shift.Projections.forEach(function (projection) {
							var scheduleStart = moment(projection.Start).toDate();
							if (projection.Selected && (latestStart === null || scheduleStart >= latestStart )) {
							    var exist =  projection.ShiftLayerIds && projection.ShiftLayerIds.some(function (layerId) {
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
			};
		}
	]);
})();

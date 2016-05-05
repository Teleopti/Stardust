"use strict";

angular.module("wfm.teamSchedule").service("ScheduleManagement", [
	"$resource", "Toggle", "$q", 'TeamSchedule', 'GroupScheduleFactory',
	function ($resource, toggleSvc, $q, teamScheduleSvc, groupScheduleFactory) {
		var svc = this;

		svc.rawSchedules = [];
		svc.groupScheduleVm = {};

		var recreateScheduleVm = function(scheduleDateMoment) {
			svc.groupScheduleVm = groupScheduleFactory.Create(svc.rawSchedules, scheduleDateMoment);
		}

		svc.resetSchedules = function (schedules, scheduleDateMoment) {
			svc.rawSchedules = schedules;
			recreateScheduleVm(scheduleDateMoment);
		};

		svc.mergeSchedules = function (schedules, scheduleDateMoment) {
			svc.rawSchedules = svc.rawSchedules.concat(schedules);
			recreateScheduleVm(scheduleDateMoment);
		}

		svc.updateScheduleForPeoples = function (personIdList, scheduleDateMoment, afterLoading) {
			var currentPeoples = [];
			angular.forEach(svc.rawSchedules, function (schedule) {
				if (personIdList.indexOf(schedule.PersonId) > -1) {
					currentPeoples.push(schedule.PersonId);
					schedule.ContractTimeMinutes = 0;
					schedule.Projection = null;
					schedule.WorkTimeMinutes = 0;
				}
			});

			teamScheduleSvc.getSchedules(scheduleDateMoment.format('YYYY-MM-DD'), currentPeoples).then(function (result) {
				svc.mergeSchedules(result.Schedules, scheduleDateMoment);
				afterLoading();
			});
		}

		svc.resetSchedulesForPeople = function (personIds) {
			angular.forEach(personIds, function (person) {
				for (var i = 0; i < svc.groupScheduleVm.Schedules.length; i++) {
					if (person == svc.groupScheduleVm.Schedules[i].PersonId) {
						svc.groupScheduleVm.Schedules[i].IsSelected = false;
						var shiftsForSelectedDate = svc.groupScheduleVm.Schedules[i].Shifts.filter(function (shift) {
							return shift.Date.isSame(svc.groupScheduleVm.Schedules[i].Date, 'day');
						});
						if (shiftsForSelectedDate.length > 0) {
							angular.forEach(shiftsForSelectedDate[0].Projections, function (projection) {
								projection.Selected = false;
							})
						}
						break;
					}
				}
			});

		}

		svc.getEarliestStartOfSelectedSchedule = function(scheduleDateMoment, selectedPersonIds) {
			var startUpdated = false;
			var earlistStart = new Date("2099-12-31");
			for (var i = 0; i < svc.groupScheduleVm.Schedules.length; i++) {
				var schedule = svc.groupScheduleVm.Schedules[i];
				var scheduleStart = new Date(schedule.ScheduleStartTime());
				if (selectedPersonIds.indexOf(schedule.PersonId) > -1 && scheduleStart < earlistStart) {
					startUpdated = true;
					earlistStart = scheduleStart;
				}
			}

			if (!startUpdated) {
				// Set to 08:00 for empty schedule or day off
				earlistStart = scheduleDateMoment.startOf('day').add(8, 'hour').toDate();
			}

			return earlistStart;
		}

		svc.getLatestStartOfSelectedSchedule = function (scheduleDateMoment, selectedPersonIds) {
			var startUpdated = false;
			var latestStart = scheduleDateMoment.toDate();
			svc.groupScheduleVm.Schedules.forEach(function(schedule) {
				var scheduleStart = schedule.ScheduleStartTime();
				if (selectedPersonIds.indexOf(schedule.PersonId) > -1 && scheduleStart > latestStart) {
					startUpdated = true;
					latestStart = scheduleStart;
				}
			});

			if (!startUpdated) {
				// Set to 08:00 for empty schedule or day off
				latestStart = scheduleDateMoment.startOf('day').add(8, 'hour').toDate();
			}

			return latestStart;
		};

		svc.getLatestStartTimeOfSelectedScheduleProjection = function (scheduleDateMoment, selectedPersonIds) {
			var latestStart = 0;
			var currentDayShifts = [];
			svc.groupScheduleVm.Schedules.forEach(function (schedule) {
				if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
					currentDayShifts = currentDayShifts.concat(schedule.Shifts.filter(function (shift) {
						return shift.Date.toDate().toDateString() == scheduleDateMoment.toDate().toDateString();
					}));
				}
			});
			currentDayShifts.forEach(function (shift) {
				if (shift.Projections) {
					var shiftLayerIds = [];
					shift.Projections.forEach(function (projection) {
						if (projection.Selected) {
							var scheduleStart = moment(projection.Start).toDate();
							if (scheduleStart > latestStart && !angular.equals(shiftLayerIds, projection.ShiftLayerIds)) {
								latestStart = scheduleStart;
								shiftLayerIds = projection.ShiftLayerIds;
							}
						}
					});
				}
			});
			return latestStart;
		};
	}
]);

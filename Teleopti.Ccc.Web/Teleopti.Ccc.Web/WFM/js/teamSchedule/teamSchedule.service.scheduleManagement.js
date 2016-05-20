(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service("ScheduleManagement", [
		"$resource", "Toggle", "$q", 'TeamSchedule', 'GroupScheduleFactory',
		function ($resource, toggleSvc, $q, teamScheduleSvc, groupScheduleFactory) {
			var svc = this;

			svc.rawSchedules = [];
			svc.groupScheduleVm = {};

			var recreateScheduleVm = function (scheduleDateMoment) {
				svc.groupScheduleVm = groupScheduleFactory.Create(svc.rawSchedules, scheduleDateMoment);
			}

			svc.resetSchedules = function (schedules, scheduleDateMoment) {
				svc.rawSchedules = schedules;
				recreateScheduleVm(scheduleDateMoment);
			};

			svc.mergeSchedules = function (schedules, scheduleDateMoment) {
				recreateScheduleVm(scheduleDateMoment);
			}

			svc.updateScheduleForPeoples = function (personIdList, scheduleDateMoment, afterLoading) {
				teamScheduleSvc.getSchedules(scheduleDateMoment.format('YYYY-MM-DD'), personIdList).then(function (result) {

					angular.forEach(result.Schedules, function (schedule) {
						var personId = schedule.PersonId;
						for (var i = 0; i < svc.rawSchedules.length; i++) {
							if (personId === svc.rawSchedules[i].PersonId) {
								svc.rawSchedules[i] = schedule;
								break;
							}
						}
					});

					svc.mergeSchedules(svc.rawSchedules, scheduleDateMoment);
					afterLoading();
				});
			}

			svc.resetSchedulesForPeople = function (personIds) {
				angular.forEach(personIds, function (person) {
					for (var i = 0; i < svc.groupScheduleVm.Schedules.length; i++) {
						var schedule = svc.groupScheduleVm.Schedules[i];
						if (person == schedule.PersonId) {
							schedule.IsSelected = false;
							var shiftsForSelectedDate = schedule.Shifts.filter(function (shift) {
								return shift.Date.isSame(schedule.Date, 'day');
							});
							if (shiftsForSelectedDate.length > 0) {
								angular.forEach(shiftsForSelectedDate[0].Projections, function (projection) {
									projection.Selected = false;
								});
							}
							break;
						}
					}
				});

			}

			svc.getEarliestStartOfSelectedSchedule = function (scheduleDateMoment, selectedPersonIds) {
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

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
					var scheduleStart = moment(schedule.ScheduleStartTime());

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

			svc.getLatestPreviousDayOvernightShiftEnd = function (scheduleDateMoment, selectedPersonIds) {
				var previousDayShifts = [];

				svc.groupScheduleVm.Schedules.forEach(function (schedule) {
					if (selectedPersonIds.indexOf(schedule.PersonId) > -1) {
						previousDayShifts = previousDayShifts.concat(schedule.Shifts.filter(function(shift) {
							return shift.Projections.length > 0
								&& new Date(shift.Date.toDate().toDateString()) < new Date(scheduleDateMoment.toDate().toDateString());
						}));
					}
				});

				if (previousDayShifts.length == 0) return null;

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
				var latestStart = null;
				var projectionShiftLayerIds = [];
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

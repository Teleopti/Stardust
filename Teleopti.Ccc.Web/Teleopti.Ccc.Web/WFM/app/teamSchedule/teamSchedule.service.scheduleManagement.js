(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('ScheduleManagement', ['Toggle', '$filter', 'TeamSchedule', 'GroupScheduleFactory', 'CurrentUserInfo', ScheduleManagement]);

	function ScheduleManagement(toggleSvc, $filter, teamScheduleSvc, groupScheduleFactory, CurrentUserInfo) {

		function ScheduleManagementService() {
			var svc = this;

			svc.rawSchedules = [];
			svc.groupScheduleVm = {};

			svc.schedules = getSchedules;
			svc.findPersonScheduleVmForPersonId = findPersonScheduleVmForPersonId;
			svc.recreateScheduleVm = recreateScheduleVm;
			svc.resetSchedules = resetSchedules;
			svc.mergeSchedules = mergeSchedules;
			svc.updateScheduleForPeoples = updateScheduleForPeoples;
			svc.resetSchedulesForPeople = resetSchedulesForPeople;

			function getSchedules() {
				return svc.groupScheduleVm.Schedules;
			}

			function findPersonScheduleVmForPersonId(personId) {
				var result = svc.groupScheduleVm.Schedules.filter(function (vm) {
					return vm.PersonId === personId;
				});
				if (result.length === 0) return null;
				return result[0];
			}

			function convertScheduleToTimezone(schedule, timezone) {

				if ((!timezone) || (timezone === CurrentUserInfo.CurrentUserInfo().DefaultTimeZone)) return schedule;

				var copiedSchedule = angular.copy(schedule);

				angular.forEach(copiedSchedule.Projection, function (p) {
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
				var timezoneAdjustedSchedules = svc.rawSchedules.map(function (schedule) {
					return convertScheduleToTimezone(schedule, timezone);
				});

				var useNextDaySchedules = true;
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
				teamScheduleSvc.getSchedules(scheduleDateStr, personIdList).then(function (result) {

					angular.forEach(result.Schedules, function (schedule) {
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
				angular.forEach(personIds, function (person) {
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
								angular.forEach(shiftsForSelectedDate[0].Projections, function (projection) {
									projection.Selected = false;
								});
							}
							break;
						}
					}
				});
			}

		}

		this.newService = function () {
			return new ScheduleManagementService();
		};

		ScheduleManagementService.call(this);
	}

})();
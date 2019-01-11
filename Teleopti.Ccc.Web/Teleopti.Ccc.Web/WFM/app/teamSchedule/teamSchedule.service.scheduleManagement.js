(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('ScheduleManagement',
			['TeamSchedule',
				'GroupScheduleFactory',
				'CurrentUserInfo',
				ScheduleManagement]);

	function ScheduleManagement(teamScheduleSvc, groupScheduleFactory, CurrentUserInfo) {

		function ScheduleManagementService() {
			var svc = this;

			svc.rawSchedules = [];
			svc.groupScheduleVm = {};

			svc.schedules = getSchedules;
			svc.findPersonScheduleVmForPersonId = findPersonScheduleVmForPersonId;
			svc.recreateScheduleVm = recreateScheduleVm;
			svc.resetSchedules = resetSchedules;
			svc.updateScheduleForPeoples = updateScheduleForPeoples;
			svc.resetSchedulesForPeople = resetSchedulesForPeople;
			svc.updateSchedulesByRawData = updateSchedulesByRawData;
			svc.getRawScheduleByPersonId = getRawScheduleByPersonId;

			function getSchedules() {
				return svc.groupScheduleVm.Schedules;
			}

			function findPersonScheduleVmForPersonId(personId) {
				if (svc.groupScheduleVm.Schedules)
					return svc.groupScheduleVm.Schedules.filter(function (vm) {
						return vm.PersonId === personId;
					})[0];
			}

			function recreateScheduleVm(queryDate, timezone, personIdInEditing) {
				timezone = timezone || CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
				var createdGroupScheduleVm = groupScheduleFactory.Create(svc.rawSchedules, queryDate, timezone);
				if (!personIdInEditing) {
					svc.groupScheduleVm = createdGroupScheduleVm;
					return;
				}

				var isTimelineLengthChanged =
					createdGroupScheduleVm.TimeLine && svc.groupScheduleVm.TimeLine
					&& (svc.groupScheduleVm.TimeLine.LengthPercentPerMinute !== createdGroupScheduleVm.TimeLine.LengthPercentPerMinute);

				createdGroupScheduleVm.Schedules.forEach(function (schedule, i) {
					if (personIdInEditing === schedule.PersonId) {
						if (isTimelineLengthChanged) {
							Object.assign(svc.groupScheduleVm.Schedules[i], schedule);
						}
					} else {
						svc.groupScheduleVm.Schedules[i] = schedule;
					}
				});

				svc.groupScheduleVm.TimeLine = createdGroupScheduleVm.TimeLine;

			}



			function resetSchedules(schedules, queryDate, timezone) {
				svc.rawSchedules = schedules;
				recreateScheduleVm(queryDate, timezone);
			}

			function getRawScheduleByPersonId(queryDate, personId) {
				for (var i = 0; i < svc.rawSchedules.length; i++) {
					if (personId === svc.rawSchedules[i].PersonId
						&& svc.rawSchedules[i].Date === queryDate) {
						return svc.rawSchedules[i];
					}
				};
			}

			function updateScheduleForPeoples(personIdList, queryDate, timezone, afterLoading, personIdInEditing) {
				teamScheduleSvc.getSchedules(queryDate, personIdList).then(function (result) {
					updateSchedulesByRawData(queryDate, timezone, result.Schedules, personIdInEditing);
					afterLoading && afterLoading();
				});
			}

			function updateSchedulesByRawData(queryDate, timezone, rawSchedules, personIdInEditing) {
				angular.forEach(rawSchedules, function (schedule) {
					for (var i = 0; i < svc.rawSchedules.length; i++) {
						if (schedule.PersonId === svc.rawSchedules[i].PersonId
							&& svc.rawSchedules[i].Date === schedule.Date) {
							svc.rawSchedules[i] = schedule;
							break;
						}
					}
				});
				recreateScheduleVm(queryDate, timezone, personIdInEditing);
			}

			function resetSchedulesForPeople(personIds) {
				angular.forEach(personIds, function (personId) {
					var length = svc.groupScheduleVm.Schedules.length;
					for (var i = 0; i < length; i++) {
						var schedule = svc.groupScheduleVm.Schedules[i];
						if (personId === schedule.PersonId) {
							schedule.IsSelected = false;
							schedule.Shifts && schedule.Shifts
								.filter(function (shift) {
									return shift.Date === schedule.Date;
								})
								.forEach(shiftsForSelectedDate[0].Projections, function (projection) {
									projection.Selected = false;
								});
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
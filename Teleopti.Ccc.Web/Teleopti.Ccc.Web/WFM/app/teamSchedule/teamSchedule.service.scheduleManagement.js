(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('ScheduleManagement',
			['TeamSchedule',
				'GroupScheduleFactory',
				'CurrentUserInfo',
				ScheduleManagement]);

	function ScheduleManagement( teamScheduleSvc, groupScheduleFactory, CurrentUserInfo) {

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

			function getSchedules() {
				return svc.groupScheduleVm.Schedules;
			}

			function findPersonScheduleVmForPersonId(personId) {
				if (svc.groupScheduleVm.Schedules)
					return svc.groupScheduleVm.Schedules.filter(function (vm) {
						return vm.PersonId === personId;
					})[0];
			}

			function recreateScheduleVm(queryDate, timezone) {
				timezone = timezone || CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
				svc.groupScheduleVm = groupScheduleFactory.Create(svc.rawSchedules, queryDate, timezone);
			}

			function resetSchedules(schedules, queryDate, timezone) {
				svc.rawSchedules = schedules;
				recreateScheduleVm(queryDate, timezone);
			}

			function updateScheduleForPeoples(personIdList, queryDate, timezone, afterLoading) {

				teamScheduleSvc.getSchedules(queryDate, personIdList).then(function (result) {
					angular.forEach(result.Schedules, function (schedule) {
						for (var i = 0; i < svc.rawSchedules.length; i++) {
							if (schedule.PersonId === svc.rawSchedules[i].PersonId
								&& svc.rawSchedules[i].Date === schedule.Date) {
								svc.rawSchedules[i] = schedule;
								break;
							}
						}
					});
					recreateScheduleVm(queryDate, timezone);
					afterLoading && afterLoading();
				});
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
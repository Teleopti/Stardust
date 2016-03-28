﻿"use strict";

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

			var params = { personIds: currentPeoples, date: scheduleDateMoment.format('YYYY-MM-DD') };
			teamScheduleSvc.getSchedules.query(params).$promise.then(function (result) {
				svc.mergeSchedules(result.Schedules, scheduleDateMoment);
				afterLoading();
			});
		}

		svc.getEarliestStartOfSelectedSchedule = function(scheduleDateMoment, selectedPersonIds) {
			var startUpdated = false;
			var earlistStart = new Date("2099-12-31");
			for (var i = 0; i < svc.groupScheduleVm.Schedules.length; i++) {
				var schedule = svc.groupScheduleVm.Schedules[i];
				var scheduleStart = schedule.ScheduleStartTime();
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

			
		}
	}
]);
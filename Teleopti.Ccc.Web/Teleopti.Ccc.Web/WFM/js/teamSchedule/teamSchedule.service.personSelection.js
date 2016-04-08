"use strict";

angular.module("wfm.teamSchedule").service("PersonSelection", [
	function() {
		var svc = this;
		svc.personInfo = {};

		svc.clearPersonInfo = function() {
			svc.personInfo = {};
		}

		svc.setScheduleDate = function(currentDate)
		{
			svc.scheduleDate = currentDate;
		}

		svc.updatePersonInfo = function (schedules) {
			angular.forEach(schedules, function (personSchedule) {
				var personAbsenceCount = 0;
				for (var i = 0; i < personSchedule.Shifts.length; i++) {
					var shift = personSchedule.Shifts[i];
					if (shift.Date === svc.scheduleDate) {
						personAbsenceCount = shift.AbsenceCount();
						break;
					}
				}
				var allowSwap = personSchedule.AllowSwap();

				var selectedPerson = svc.personInfo[personSchedule.PersonId];
				if (selectedPerson === undefined || selectedPerson === null) {
					svc.personInfo[personSchedule.PersonId] = {
						name: personSchedule.Name,
						isSelected: false,
						allowSwap: allowSwap,
						personAbsenceCount: personAbsenceCount,
						scheduleEndTime: personSchedule.ScheduleEndTime()
					};
				} else {
					selectedPerson.allowSwap = allowSwap;
					selectedPerson.personAbsenceCount = personAbsenceCount;
					selectedPerson.scheduleEndTime = personSchedule.ScheduleEndTime();
				}
			});
		};

		svc.resetPersonInfo = function(schedules) {
			svc.clearPersonInfo();
			svc.updatePersonInfo(schedules);
		}

		svc.selectAllPerson = function(schedules) {
			svc.updatePersonInfo(schedules);
			for (var key in svc.personInfo) {
				svc.personInfo[key].isSelected = true;
			}
		}

		svc.getSelectedPersonInfoList = function() {
			var result = [];
			for (var key in svc.personInfo) {
				var schedule = svc.personInfo[key];
				if (schedule.isSelected) {
					result.push({
						personId: key,
						name:schedule.name,
						allowSwap: schedule.allowSwap,
						personAbsenceCount: schedule.personAbsenceCount,
						scheduleEndTime: schedule.scheduleEndTime
					});
				}
			}
			return result;
		}

		svc.getSelectedPersonIdList = function() {
			var personIds = [];
			var list = svc.getSelectedPersonInfoList();
			angular.forEach(list, function(element) {
				personIds.push(element.personId);
			});
			return personIds;
		}

		svc.isAnyAgentSelected = function() {
			var selectedPersonList = svc.getSelectedPersonIdList();
			return selectedPersonList.length > 0;
		}

		svc.canSwapShifts = function() {
			var personInfos = svc.getSelectedPersonInfoList();
			if (personInfos.length !== 2) return false;

			return personInfos[0].allowSwap && personInfos[1].allowSwap;
		}
	}
]);
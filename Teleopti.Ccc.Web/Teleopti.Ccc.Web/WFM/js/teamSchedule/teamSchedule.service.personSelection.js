"use strict";

angular.module("wfm.teamSchedule").service("PersonSelection", [
	function() {
		var svc = this;
		svc.personInfo = {};

		svc.clearPersonInfo = function() {
			svc.personInfo = {};
		}

		svc.updatePersonInfo = function(schedules) {
			angular.forEach(schedules, function(personSchedule) {
				var allowSwap = personSchedule.AllowSwap();
				var selectedPerson = svc.personInfo[personSchedule.PersonId];
				if (selectedPerson === undefined || selectedPerson === null) {
					svc.personInfo[personSchedule.PersonId] = {
						isSelected: false,
						allowSwap: allowSwap
					};
				} else {
					selectedPerson.allowSwap = allowSwap;
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

		var getSelectedPersonInfoList = function() {
			var result = [];
			for (var key in svc.personInfo) {
				var schedule = svc.personInfo[key];
				if (schedule.isSelected) {
					result.push({
						personId: key,
						allowSwap: schedule.allowSwap
					});
				}
			}
			return result;
		}

		svc.getSelectedPersonIdList = function() {
			var personIds = [];
			var list = getSelectedPersonInfoList();
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
			var personInfos = getSelectedPersonInfoList();
			if (personInfos.length !== 2) return false;

			return personInfos[0].allowSwap && personInfos[1].allowSwap;
		}
	}
]);
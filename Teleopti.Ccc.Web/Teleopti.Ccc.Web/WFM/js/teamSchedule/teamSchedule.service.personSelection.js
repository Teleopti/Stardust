"use strict";

angular.module("wfm.teamSchedule").service("PersonSelection", [
	function() {
		var svc = this;
		svc.personInfo = {};

		svc.clearPersonInfo = function() {
			svc.personInfo = {};
		};

		svc.updatePersonSelection = function (personSchedule) {
			var selectedPeople = svc.personInfo;
			if (personSchedule.IsSelected) {
				var absences = [], activities = [];
				var shiftsForCurrentDate = personSchedule.Shifts.filter(function (shift) {
					return personSchedule.Date.isSame(shift.Date, 'day');
				});
				if(shiftsForCurrentDate.length > 0){
					var projectionsForCurrentDate = shiftsForCurrentDate[0].Projections;
					angular.forEach(projectionsForCurrentDate, function(projection) {
						if (projection.ParentPersonAbsences && projection.ParentPersonAbsences.length > 0) {
							angular.forEach(projection.ParentPersonAbsences, function(personAbsId) {
								if (absences.indexOf(personAbsId) === -1) {
									absences.push(personAbsId);
								}
							});
						} else if (projection.ShiftLayerIds && !projection.IsOvertime) {
							angular.forEach(projection.ShiftLayerIds, function(shiftLayer) {
								if (activities.indexOf(shiftLayer) === -1) {
									activities.push(shiftLayer);
								}
							});
						}
					});
				}
				selectedPeople[personSchedule.PersonId] = {
					name: personSchedule.Name,
					checked: true,
					allowSwap: personSchedule.AllowSwap(),
					scheduleEndTime: personSchedule.ScheduleEndTime(),
					personAbsenceCount: absences.length,
					personActivityCount: activities.length,
					selectedAbsences: absences,
					selectedActivities: activities
				};
			} else if (!personSchedule.IsSelected && selectedPeople[personSchedule.PersonId]) {
				delete selectedPeople[personSchedule.PersonId];
			}
		};

		svc.updatePersonInfo = function (schedules) {
			angular.forEach(schedules, function (personSchedule) {
				var allowSwap = personSchedule.AllowSwap();

				var selectedPerson = svc.personInfo[personSchedule.PersonId];
				if(selectedPerson && selectedPerson.checked){
					personSchedule.IsSelected = true;
				}
				else {
					personSchedule.IsSelected = false;
				}

				if (selectedPerson) {
					selectedPerson.scheduleEndTime = personSchedule.ScheduleEndTime();
					selectedPerson.allowSwap = allowSwap;
					for (var i = 0; i < personSchedule.Shifts.length; i++) {
						if (!personSchedule.Shifts[i].Date.isSame(personSchedule.Date, 'day')) {
							continue;
						}
						angular.forEach(personSchedule.Shifts[i].Projections, function (projection) {
							if (projection.ParentPersonAbsences && projection.ParentPersonAbsences.length > 0) {
								var selected = true;
								for (var i = 0; i < projection.ParentPersonAbsences.length; i++) {
									if (selectedPerson.selectedAbsences.indexOf(projection.ParentPersonAbsences[i]) === -1) {
										selected = false;
										break;
									}
								}
								projection.Selected = selected;
							}
							else if (projection.ShiftLayerIds) {
								var selected = true;
								for (var i = 0; i < projection.ShiftLayerIds.length; i++) {
									if (selectedPerson.selectedActivities.indexOf(projection.ShiftLayerIds[i]) === -1) {
										selected = false;
										break;
									}
								}
								projection.Selected = selected;
							}
						});
					}
				}
			});
		};

		svc.toggleAllPersonProjections = function (personSchedule) {
			angular.forEach(personSchedule.Shifts, function (shift) {
				if (shift.Date.isSame(personSchedule.Date, 'day')) {
					angular.forEach(shift.Projections, function (projection) {
						if (projection.ParentPersonAbsences || (!projection.IsOvertime && projection.ShiftLayerIds)) {
							projection.Selected = personSchedule.IsSelected;
						}
					});
				}
			});
		};

		svc.updatePersonProjectionSelection = function(currentProjection, personSchedule){
			var selected = currentProjection.Selected;
			var personId = personSchedule.PersonId;
			var selectedPeople = svc.personInfo;
			angular.forEach(personSchedule.Shifts, function(shift) {
				if (shift.Date.isSame(personSchedule.Date, 'day')) {
					angular.forEach(shift.Projections, function(projection) {
						var sameActivity = currentProjection.ShiftLayerIds && angular.equals(projection.ShiftLayerIds, currentProjection.ShiftLayerIds);
						var sameAbsence = currentProjection.ParentPersonAbsences && angular.equals(currentProjection.ParentPersonAbsences, projection.ParentPersonAbsences);
						if (sameActivity || sameAbsence) {
							projection.Selected = currentProjection.Selected;
						}
					});
				}
			});
			if (selected && !selectedPeople[personId]) {

				selectedPeople[personId] = {
					name: personSchedule.Name,
					allowSwap: personSchedule.AllowSwap(),
					scheduleEndTime: personSchedule.ScheduleEndTime(),
					personAbsenceCount: 0,
					personActivityCount: 0,
					selectedAbsences: [],
					selectedActivities: []
				};
				if(currentProjection.ParentPersonAbsences !== null){
					selectedPeople[personId].selectedAbsences = selectedPeople[personId].selectedAbsences.concat(currentProjection.ParentPersonAbsences);
					selectedPeople[personId].personAbsenceCount = currentProjection.ParentPersonAbsences.length;
				}
				if(currentProjection.ShiftLayerIds !== null){
					selectedPeople[personId].selectedActivities = selectedPeople[personId].selectedActivities.concat(currentProjection.ShiftLayerIds);
					selectedPeople[personId].personActivityCount = currentProjection.ShiftLayerIds.length;
				}

			}
			else if (selected && selectedPeople[personId]) {
				if (currentProjection.ParentPersonAbsences !== null) {
					angular.forEach(currentProjection.ParentPersonAbsences, function(personAbs) {
						if (selectedPeople[personId].selectedAbsences.indexOf(personAbs) === -1) {
							selectedPeople[personId].selectedAbsences.push(personAbs);
							selectedPeople[personId].personAbsenceCount++;
						}
					});
				}
				if (currentProjection.ShiftLayerIds !== null) {
					angular.forEach(currentProjection.ShiftLayerIds, function(shiftLayer) {
						if (selectedPeople[personId].selectedActivities.indexOf(shiftLayer) === -1) {
							selectedPeople[personId].selectedActivities.push(shiftLayer);
							selectedPeople[personId].personActivityCount++;
						}
					});
				}
			}
			else if (!selected && selectedPeople[personId]) {
				if (currentProjection.ParentPersonAbsences) {
					angular.forEach(currentProjection.ParentPersonAbsences, function(personAbs) {
						var absenceIndex = selectedPeople[personId].selectedAbsences.indexOf(personAbs);
						if (absenceIndex > -1) {
							selectedPeople[personId].selectedAbsences.splice(absenceIndex, 1);
							selectedPeople[personId].personAbsenceCount--;
						}
					});
				}
				if (currentProjection.ShiftLayerIds) {
					angular.forEach(currentProjection.ShiftLayerIds, function(shiftLayer) {
						var activityIndex = selectedPeople[personId].selectedActivities.indexOf(shiftLayer);
						if (activityIndex > -1) {
							selectedPeople[personId].selectedActivities.splice(activityIndex, 1);
							selectedPeople[personId].personActivityCount--;
						}
					});
				}

				if (selectedPeople[personId].personAbsenceCount === 0 && selectedPeople[personId].personActivityCount === 0) {
					delete selectedPeople[personId];
				}
			}
			personSchedule.IsSelected = selectedPeople[personId] && selectedPeople[personId].personAbsenceCount == personSchedule.AbsenceCount() && selectedPeople[personId].personActivityCount == personSchedule.ActivityCount();
			if (selectedPeople[personId]) {
				selectedPeople[personId].checked = personSchedule.IsSelected;
			}
		};

		svc.selectAllPerson = function (schedules) {
			angular.forEach(schedules, function (schedule) {
				schedule.IsSelected = true;
				svc.updatePersonSelection(schedule);
			});
		};

		svc.unselectPersonsWithIds = function (personIds) {
			angular.forEach(personIds, function(id) {
				delete svc.personInfo[id];
			});
		};

		svc.getCheckedPersonIds = function () {
			var result = [];
			for (var personId in svc.personInfo) {
				if (svc.personInfo[personId].checked === true) {
					result.push(personId);
				}
			}
			return result;
		};

		svc.getCheckedPersonInfoList = function () {
			var result = [];
			for (var personId in svc.personInfo) {
				if (svc.personInfo[personId].checked === true) {
					result.push({
						personId: personId,
						name: svc.personInfo[personId].name,
						scheduleEndTime: svc.personInfo[personId].scheduleEndTime
					});
				}
			}
			return result;
		};

		svc.getSelectedPersonInfoList = function() {
			var result = [];
			for (var key in svc.personInfo) {
				var schedule = svc.personInfo[key];
				result.push({
					personId: key,
					name: schedule.name,
					allowSwap: schedule.allowSwap,
					scheduleEndTime: schedule.scheduleEndTime,
					personAbsenceCount: schedule.personAbsenceCount,
					personActivityCount: schedule.personActivityCount,
					selectedAbsences: schedule.selectedAbsences,
					selectedActivities: schedule.selectedActivities
				});
			}
			return result;
		};

		svc.getSelectedPersonIdList = function() {
			return Object.keys(svc.personInfo);
		};

		svc.isAnyAgentSelected = function() {
			var selectedPersonList = svc.getSelectedPersonIdList();
			return selectedPersonList.length > 0;
		};

		svc.anyAgentChecked = function () {
			var personInfoList = svc.getCheckedPersonInfoList();
			return personInfoList.length > 0;
		};

		svc.canSwapShifts = function() {
			var personIds = svc.getCheckedPersonIds();
			if (personIds.length !== 2) {
				return false;
			}
			return svc.personInfo[personIds[0]].allowSwap && svc.personInfo[personIds[1]].allowSwap;
		};

		svc.getTotalSelectedPersonAndProjectionCount = function() {
			var ret = {
				checkedPersonCount: 0,
				selectedActivityInfo: {
					PersonCount: 0,
					ActivityCount: 0
				},
				selectedAbsenceInfo: {
					PersonCount: 0,
					AbsenceCount: 0
				}
			};

			var selectedPersonInfo = svc.getSelectedPersonInfoList();
			for (var j = 0; j < selectedPersonInfo.length; j++) {
				var selectedPerson = selectedPersonInfo[j];
				if (selectedPerson.checked) {
					ret.checkedPersonCount++;
				}
				if (selectedPerson.personAbsenceCount > 0) {
					ret.selectedAbsenceInfo.AbsenceCount += selectedPerson.personAbsenceCount;
					ret.selectedAbsenceInfo.PersonCount++;
				}
				if (selectedPerson.personActivityCount > 0) {
					ret.selectedActivityInfo.ActivityCount += selectedPerson.personActivityCount;
					ret.selectedActivityInfo.PersonCount++;
				}
			}
			return ret;
		};
	}
]);
"use strict";

angular.module("wfm.teamSchedule").service("PersonSelection", [
	function() {
		var svc = this;
		svc.personInfo = {};

		svc.clearPersonInfo = function() {
			svc.personInfo = {};
		}

		// to be deleted
		svc.setScheduleDate = function(currentDate)
		{
			svc.scheduleDate = currentDate;
		}

		svc.updatePersonSelection = function (personSchedule) {
			var selectedPeople = svc.personInfo;
			if (personSchedule.IsSelected) {
				var absences = [], activities = [];
				var shiftsForCurrentDate = personSchedule.Shifts.filter(function (shift) {
					return personSchedule.Date.isSame(shift.Date, 'day');
				});
				if(shiftsForCurrentDate.length > 0){
					var projectionsForCurrentDate = shiftsForCurrentDate[0].Projections;
					angular.forEach(projectionsForCurrentDate, function (projection) {
					if (projection.ParentPersonAbsence && absences.indexOf(projection.ParentPersonAbsence) === -1) {
						absences.push(projection.ParentPersonAbsence);
					} else if (projection.ShiftLayerId && !projection.IsOvertime && activities.indexOf(projection.ShiftLayerId) === -1) {
						activities.push(projection.ShiftLayerId);
					}
				});
				}
				selectedPeople[personSchedule.PersonId] = {
					name: personSchedule.Name,
					checked: true,
					allowSwap: personSchedule.allowSwap,
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
				var personAbsencesCount = 0,
					personActivitiesCount = 0;
				var allowSwap = personSchedule.AllowSwap();

				var selectedPerson = svc.personInfo[personSchedule.PersonId];
				if (selectedPerson) {
					selectedPerson.scheduleEndTime = personSchedule.ScheduleEndTime();
					for (var i = 0; i < personSchedule.Shifts.length; i++) {
						if (!personSchedule.Shifts[i].Date.isSame(personSchedule.Date, 'day')) {
							continue;
						}
						var hasUnselected = false;
						angular.forEach(personSchedule.Shifts[i].Projections, function(projection) {
							if (projection.ParentPersonAbsence && selectedPerson.selectedAbsences.indexOf(projection.ParentPersonAbsence) > -1) {
								projection.Selected = true;
							}
							else if (projection.ShiftLayerId && selectedPerson.selectedActivities.indexOf(projection.ShiftLayerId) > -1) {
								projection.Selected = true;
							}
							else if(projection.ParentPersonAbsence || (projection.ShiftLayerId && !projection.IsOvertime)){
								hasUnselected = true;
							}
						});
						personSchedule.IsSelected = !hasUnselected;
					}
				}
			});
		};
		
		svc.toggleAllPersonProjections = function (personSchedule) {
			angular.forEach(personSchedule.Shifts, function (shift) {
				if (shift.Date.isSame(personSchedule.Date, 'day')) {
					angular.forEach(shift.Projections, function (projection) {
						if (projection.ParentPersonAbsence || (!projection.IsOvertime && projection.ShiftLayerId)) {
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
						var sameActivity = currentProjection.ShiftLayerId && projection.ShiftLayerId === currentProjection.ShiftLayerId;
						var sameAbsence = currentProjection.ParentPersonAbsence && currentProjection.ParentPersonAbsence === projection.ParentPersonAbsence;  
						if (sameActivity || sameAbsence) {
							projection.Selected = currentProjection.Selected;
						}
					});
				}
			});
			if (selected && !selectedPeople[personId]) {
				
				selectedPeople[personId] = {
					name: personSchedule.Name,
					allowSwap: personSchedule.allowSwap,
					scheduleEndTime: personSchedule.ScheduleEndTime(),
					personAbsenceCount: 0,
					personActivityCount: 0,
					selectedAbsences: [],
					selectedActivities: []
				};
				if(currentProjection.ParentPersonAbsence !== null){
					selectedPeople[personId].selectedAbsences.push(currentProjection.ParentPersonAbsence);
					selectedPeople[personId].personAbsenceCount = 1;
				}
				if(currentProjection.ShiftLayerId !== null){
					selectedPeople[personId].selectedActivities.push(currentProjection.ShiftLayerId);
					selectedPeople[personId].personActivityCount = 1;
				}
				
			}
			else if (selected && selectedPeople[personId]) {
				if (currentProjection.ParentPersonAbsence !== null && selectedPeople[personId].selectedAbsences.indexOf(currentProjection.ParentPersonAbsence) === -1) {
					selectedPeople[personId].selectedAbsences.push(currentProjection.ParentPersonAbsence);
					selectedPeople[personId].personAbsenceCount++;
				}
				if(currentProjection.ShiftLayerId !== null && selectedPeople[personId].selectedActivities.indexOf(currentProjection.ShiftLayerId) === -1){
					selectedPeople[personId].selectedActivities.push(currentProjection.ShiftLayerId);
					selectedPeople[personId].personActivityCount++;
				}
			}
			else if(!selected && selectedPeople[personId]){
				var absenceIndex = selectedPeople[personId].selectedAbsences.indexOf(currentProjection.ParentPersonAbsence);
				var activityIndex = selectedPeople[personId].selectedActivities.indexOf(currentProjection.ShiftLayerId);
				if(currentProjection.ParentPersonAbsence !== null && absenceIndex > -1){
					selectedPeople[personId].selectedAbsences.splice(absenceIndex, 1);
					selectedPeople[personId].personAbsenceCount--;
				}
				if(currentProjection.ShiftLayerId !== null && activityIndex > -1){
					selectedPeople[personId].selectedActivities.splice(activityIndex,1);
					selectedPeople[personId].personActivityCount--;
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
		}

		svc.getSelectedPersonIdList = function() {
			var personIds = [];
			for (var key in svc.personInfo) {
				personIds.push(key);
			}
			return personIds;
		}

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
		}
	}
]);
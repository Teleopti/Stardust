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
					Name: personSchedule.Name,
					Checked: true,
					AllowSwap: personSchedule.AllowSwap(),
					ScheduleStartTime: personSchedule.ScheduleStartTime(),
					ScheduleEndTime: personSchedule.ScheduleEndTime(),
					PersonAbsenceCount: absences.length,
					PersonActivityCount: activities.length,
					SelectedAbsences: absences,
					SelectedActivities: activities
				};
			} else if (!personSchedule.IsSelected && selectedPeople[personSchedule.PersonId]) {
				delete selectedPeople[personSchedule.PersonId];
			}
		};

		svc.updatePersonInfo = function (schedules) {
			angular.forEach(schedules, function (personSchedule) {
				var selectedPerson = svc.personInfo[personSchedule.PersonId];
				if (selectedPerson && selectedPerson.Checked)
					personSchedule.IsSelected = true;
				else
					personSchedule.IsSelected = false;

				if (selectedPerson) {
					selectedPerson.ScheduleEndTime = personSchedule.ScheduleEndTime();
					selectedPerson.AllowSwap = personSchedule.AllowSwap();;
					for (var i = 0; i < personSchedule.Shifts.length; i++) {
						if (!personSchedule.Shifts[i].Date.isSame(personSchedule.Date, 'day')) {
							continue;
						}
						angular.forEach(personSchedule.Shifts[i].Projections, function (projection) {
							var selected;
							if (projection.ParentPersonAbsences && projection.ParentPersonAbsences.length > 0) {
								selected = true;
								for (var i = 0; i < projection.ParentPersonAbsences.length; i++) {
									if (selectedPerson.SelectedAbsences.indexOf(projection.ParentPersonAbsences[i]) === -1) {
										selected = false;
										break;
									}
								}
								projection.Selected = selected;
							}
							else if (projection.ShiftLayerIds) {
								selected = true;
								for (var i = 0; i < projection.ShiftLayerIds.length; i++) {
									if (selectedPerson.SelectedActivities.indexOf(projection.ShiftLayerIds[i]) === -1) {
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

		svc.toggleAllPersonProjections = function (personSchedule, viewDate) {
			angular.forEach(personSchedule.Shifts, function (shift) {
				if (shift.Date.format("YYYY-MM-DD") === moment(viewDate).format("YYYY-MM-DD")) {
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
					Name: personSchedule.Name,
					AllowSwap: personSchedule.AllowSwap(),
					ScheduleStartTime: personSchedule.ScheduleStartTime(),
					ScheduleEndTime: personSchedule.ScheduleEndTime(),
					PersonAbsenceCount: 0,
					PersonActivityCount: 0,
					SelectedAbsences: [],
					SelectedActivities: []
				};


				if(currentProjection.ParentPersonAbsences !== null){
					selectedPeople[personId].SelectedAbsences = selectedPeople[personId].SelectedAbsences.concat(currentProjection.ParentPersonAbsences);
					selectedPeople[personId].PersonAbsenceCount = currentProjection.ParentPersonAbsences.length;
				}
				if(currentProjection.ShiftLayerIds !== null){
					selectedPeople[personId].SelectedActivities = selectedPeople[personId].SelectedActivities.concat(currentProjection.ShiftLayerIds);
					selectedPeople[personId].PersonActivityCount = currentProjection.ShiftLayerIds.length;
				}
			}
			else if (selected && selectedPeople[personId]) {
				if (currentProjection.ParentPersonAbsences !== null) {
					angular.forEach(currentProjection.ParentPersonAbsences, function(personAbs) {
						if (selectedPeople[personId].SelectedAbsences.indexOf(personAbs) === -1) {
							selectedPeople[personId].SelectedAbsences.push(personAbs);
							selectedPeople[personId].PersonAbsenceCount++;
						}
					});
				}
				if (currentProjection.ShiftLayerIds !== null) {
					angular.forEach(currentProjection.ShiftLayerIds, function(shiftLayer) {
						if (selectedPeople[personId].SelectedActivities.indexOf(shiftLayer) === -1) {
							selectedPeople[personId].SelectedActivities.push(shiftLayer);
							selectedPeople[personId].PersonActivityCount++;
						}
					});
				}
			}
			else if (!selected && selectedPeople[personId]) {
				if (currentProjection.ParentPersonAbsences) {
					angular.forEach(currentProjection.ParentPersonAbsences, function(personAbs) {
						var absenceIndex = selectedPeople[personId].SelectedAbsences.indexOf(personAbs);
						if (absenceIndex > -1) {
							selectedPeople[personId].SelectedAbsences.splice(absenceIndex, 1);
							selectedPeople[personId].PersonAbsenceCount--;
						}
					});
				}
				if (currentProjection.ShiftLayerIds) {
					angular.forEach(currentProjection.ShiftLayerIds, function(shiftLayer) {
						var activityIndex = selectedPeople[personId].SelectedActivities.indexOf(shiftLayer);
						if (activityIndex > -1) {
							selectedPeople[personId].SelectedActivities.splice(activityIndex, 1);
							selectedPeople[personId].PersonActivityCount--;
						}
					});
				}

				if (selectedPeople[personId].PersonAbsenceCount === 0 && selectedPeople[personId].PersonActivityCount === 0) {
					delete selectedPeople[personId];
				}
			}
			personSchedule.IsSelected = selectedPeople[personId]
										&& selectedPeople[personId].PersonAbsenceCount === personSchedule.AbsenceCount()
										&& selectedPeople[personId].PersonActivityCount === personSchedule.ActivityCount();
			if (selectedPeople[personId]) {
				selectedPeople[personId].Checked = personSchedule.IsSelected;
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
				if (svc.personInfo[personId].Checked === true) {
					result.push(personId);
				}
			}
			return result;
		};

		svc.getCheckedPersonInfoList = function () {
			var result = [];
			for (var personId in svc.personInfo) {
				if (svc.personInfo[personId].Checked === true) {
					result.push({
						PersonId: personId,
						Name: svc.personInfo[personId].Name,
						ScheduleEndTime: svc.personInfo[personId].ScheduleEndTime
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
					PersonId: key,
					Name: schedule.Name,
					AllowSwap: schedule.AllowSwap,
					ScheduleStartTime: schedule.ScheduleStartTime,
					ScheduleEndTime: schedule.ScheduleEndTime,
					PersonAbsenceCount: schedule.PersonAbsenceCount,
					PersonActivityCount: schedule.PersonActivityCount,
					SelectedAbsences: schedule.SelectedAbsences,
					SelectedActivities: schedule.SelectedActivities
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

			var isBothAllowSwap = svc.personInfo[personIds[0]].AllowSwap && svc.personInfo[personIds[1]].AllowSwap;
			var isOnlyTodaySchedules = moment(svc.personInfo[personIds[0]].ScheduleStartTime).format('YYYY-MM-DD') === moment(svc.personInfo[personIds[1]].ScheduleStartTime).format('YYYY-MM-DD');

			return isBothAllowSwap && isOnlyTodaySchedules;
		};

		svc.getTotalSelectedPersonAndProjectionCount = function() {
			var ret = {
				CheckedPersonCount: 0,
				SelectedActivityInfo: {
					PersonCount: 0,
					ActivityCount: 0
				},
				SelectedAbsenceInfo: {
					PersonCount: 0,
					AbsenceCount: 0
				}
			};

			var selectedPersonInfo = svc.getSelectedPersonInfoList();
			for (var j = 0; j < selectedPersonInfo.length; j++) {
				var selectedPerson = selectedPersonInfo[j];
				if (selectedPerson.Checked) {
					ret.CheckedPersonCount++;
				}
				if (selectedPerson.PersonAbsenceCount > 0) {
					ret.SelectedAbsenceInfo.AbsenceCount += selectedPerson.PersonAbsenceCount;
					ret.SelectedAbsenceInfo.PersonCount++;
				}
				if (selectedPerson.PersonActivityCount > 0) {
					ret.SelectedActivityInfo.ActivityCount += selectedPerson.PersonActivityCount;
					ret.SelectedActivityInfo.PersonCount++;
				}
			}
			return ret;
		};
	}
]);
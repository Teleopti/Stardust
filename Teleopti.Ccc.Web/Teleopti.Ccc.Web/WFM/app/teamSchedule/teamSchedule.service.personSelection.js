﻿"use strict";

angular.module("wfm.teamSchedule").service("PersonSelection", PersonSelectionService);

function PersonSelectionService() {
	var svc = this;
	svc.personInfo = {};
	
	svc.clearPersonInfo = function () {
		svc.personInfo = {};
	};

	svc.updatePersonSelection = function (personSchedule) {
		if (personSchedule.IsSelected) {
			var absences = [], activities = [];				
			if (personSchedule.Shifts && personSchedule.Shifts.length > 0) {
				personSchedule.Shifts.forEach(function(shift) {
					if (shift.Date !== personSchedule.Date) {
						return;
					}

					if (!shift.Projections) return;

					shift.Projections.forEach(function(projection) {
						if (projection.ParentPersonAbsences) {
							projection.ParentPersonAbsences.forEach(function (personAbsId) {
								var targetSelectedAbsence = new SelectedAbsence(personAbsId, shift.Date);

								if (lookUpIndex(absences, targetSelectedAbsence) < 0) {
									absences.push(targetSelectedAbsence);
								}
							});
						} else if (projection.ShiftLayerIds && !projection.IsOvertime) {
							var targetShiftLayerIds = projection.ShiftLayerIds;
							targetShiftLayerIds.forEach(function (shiftLayerId) {
								var targetSelectedActivity = new SelectedActivity(shiftLayerId, shift.Date);
								if (lookUpIndex(activities, targetSelectedActivity) < 0)
									activities.push(targetSelectedActivity);
							});
						}
					});
				});
			}

			svc.personInfo[personSchedule.PersonId] = createDefaultPersonInfo(personSchedule, absences, activities);
		} else if (!personSchedule.IsSelected && svc.personInfo[personSchedule.PersonId]) {
			delete svc.personInfo[personSchedule.PersonId];
		}
	};

	svc.preSelectPeople = function (preSelectedPersonIds, schedules, viewDate) {
		if (preSelectedPersonIds.length == 0) return;
		angular.forEach(schedules, function(personSchedule) {
			var personId = personSchedule.PersonId;
			if (preSelectedPersonIds.indexOf(personId) > -1) {
				personSchedule.IsSelected = true;
				svc.updatePersonSelection(personSchedule);
				svc.toggleAllPersonProjections(personSchedule, viewDate);
			}
		});
	}

	svc.updatePersonInfo = function(schedules) {
		schedules.forEach(function(personSchedule) {
			var personId = personSchedule.PersonId;
			if (svc.personInfo[personId] && svc.personInfo[personId].Checked)
				personSchedule.IsSelected = true;
			else
				personSchedule.IsSelected = false;

			if (svc.personInfo[personId]) {
				svc.personInfo[personId].Timezone = personSchedule.Timezone;
				svc.personInfo[personId].ScheduleEndTime = personSchedule.ScheduleEndTime();
				svc.personInfo[personId].AllowSwap = personSchedule.AllowSwap();

				personSchedule.Shifts.forEach(function(shift) {
					if (shift.Date !== personSchedule.Date) {
						return;
					}

					if (!shift.Projections) return;

					shift.Projections.forEach(function(projection) {
						if (projection.ParentPersonAbsences && projection.ParentPersonAbsences.length > 0) {
							projection.Selected =
								!projection.ParentPersonAbsences.some(function(absenceId) {
									var targetAbsence = new SelectedAbsence(absenceId, shift.Date);
									return lookUpIndex(svc.personInfo[personId].SelectedAbsences, targetAbsence) < 0;
								});
						} else if (projection.ShiftLayerIds) {
							projection.Selected =
								!projection.ShiftLayerIds.some(function(shiftLayerId) {
									var targetActivity = new SelectedActivity(shiftLayerId, shift.Date);
									return lookUpIndex(svc.personInfo[personId].SelectedActivities, targetActivity) < 0;
								});
						}
					});
				});
			}
		});
	};

	svc.isAllProjectionSelected = function (personSchedule, viewDate) {
		return personSchedule.Shifts.every( function (shift) {
			if (shift.Date === moment(viewDate).format("YYYY-MM-DD")) {
				return shift.Projections.every(function(projection) {
					if (projection.ParentPersonAbsences || (!projection.IsOvertime && projection.ShiftLayerIds)) {
						return projection.Selected;
					}
					return true;
				});
			}
			return true;
		});
	}

	svc.toggleAllPersonProjections = function (personSchedule, viewDate) {
		angular.forEach(personSchedule.Shifts, function (shift) {
			if (shift.Date === moment(viewDate).format("YYYY-MM-DD") || !personSchedule.IsSelected) {
				angular.forEach(shift.Projections, function (projection) {
					if (projection.ParentPersonAbsences || (!projection.IsOvertime && projection.ShiftLayerIds)) {
						projection.Selected = personSchedule.IsSelected;
					}
				});
			}
		});
	};

	svc.updatePersonProjectionSelection = function (currentProjection, viewDate) {
		var currentShift = currentProjection.Parent;
		var personSchedule = currentShift.Parent;
		var personId = personSchedule.PersonId;

		personSchedule.IsSelected = svc.isAllProjectionSelected(personSchedule, viewDate);
		
		if (currentProjection.Selected && !svc.personInfo[personId]) {
			svc.personInfo[personId] = createDefaultPersonInfo(personSchedule);			
		}

		var personInfo = svc.personInfo[personId];
		if (currentProjection.Selected) {
			if (currentProjection.ParentPersonAbsences && currentProjection.ParentPersonAbsences.length > 0) {
				currentProjection.ParentPersonAbsences.forEach(function(absenceId) {
					addAbsence(personInfo.SelectedAbsences, absenceId, currentShift.Date);

				});
			} else if (currentProjection.ShiftLayerIds && currentProjection.ShiftLayerIds.length > 0 && !currentProjection.IsOvertime) {
				currentProjection.ShiftLayerIds.forEach(function(shiftLayerId) {
					addActivity(personInfo.SelectedActivities, shiftLayerId, currentShift.Date);
				});
			}
		} else {
			if (currentProjection.ParentPersonAbsences && currentProjection.ParentPersonAbsences.length > 0) {
				currentProjection.ParentPersonAbsences.forEach(function (absenceId) {
					deleteAbsence(personInfo.SelectedAbsences, absenceId, currentShift.Date);
				});
			} else if (currentProjection.ShiftLayerIds && currentProjection.ShiftLayerIds.length > 0 && !currentProjection.IsOvertime) {
				currentProjection.ShiftLayerIds.forEach(function (shiftLayerId) {
					deleteActivity(personInfo.SelectedActivities, shiftLayerId, currentShift.Date);
				});
			}
		}

		if (svc.personInfo[personId]) {
			svc.personInfo[personId].Checked = personSchedule.IsSelected;
		}

		svc.updatePersonInfo([personSchedule]);

		if (personInfo.SelectedActivities.length === 0 && personInfo.SelectedAbsences.length === 0) {
			delete svc.personInfo[personId];
		}
	};

	svc.selectAllPerson = function (schedules) {
		angular.forEach(schedules, function (schedule) {
			schedule.IsSelected = true;
			svc.updatePersonSelection(schedule);
		});
	};
	
	svc.unselectAllPerson = function (schedules) {
		angular.forEach(schedules, function (personSchedule) {
			personSchedule.IsSelected = false;
			svc.updatePersonSelection(personSchedule);
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
		return svc.getSelectedPersonInfoList()
			.filter(function(info) {
				return info.Checked === true;
			});
	};

	svc.getSelectedPersonInfoList = function () {	
		return svc.getSelectedPersonIdList()
			.map(function(key) {
				return svc.personInfo[key];
			});
	};

	svc.getSelectedPersonIdList = function () {
		return Object.keys(svc.personInfo);
	};

	svc.isAnyAgentSelected = function () {
		var selectedPersonList = svc.getSelectedPersonIdList();
		return selectedPersonList.length > 0;
	};

	svc.anyAgentChecked = function () {
		var personInfoList = svc.getCheckedPersonInfoList();
		return personInfoList.length > 0;
	};

	svc.canSwapShifts = function () {
		var personIds = svc.getCheckedPersonIds();
		if (personIds.length !== 2) {
			return false;
		}

		var isBothAllowSwap = svc.personInfo[personIds[0]].AllowSwap && svc.personInfo[personIds[1]].AllowSwap;
		var isOnlyTodaySchedules = moment(svc.personInfo[personIds[0]].ScheduleStartTime).format('YYYY-MM-DD') === moment(svc.personInfo[personIds[1]].ScheduleStartTime).format('YYYY-MM-DD');
		var isBothOnSameTimezone = svc.personInfo[personIds[0]].Timezone.IanaId === svc.personInfo[personIds[1]].Timezone.IanaId;		
		return isBothAllowSwap && isOnlyTodaySchedules && isBothOnSameTimezone;
	};

	svc.getTotalSelectedPersonAndProjectionCount = function () {
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
			if (selectedPerson.SelectedAbsences.length > 0) {
				ret.SelectedAbsenceInfo.AbsenceCount += selectedPerson.SelectedAbsences.length;
				ret.SelectedAbsenceInfo.PersonCount++;
			}
			if (selectedPerson.SelectedActivities.length > 0) {
				ret.SelectedActivityInfo.ActivityCount += selectedPerson.SelectedActivities.length;
				ret.SelectedActivityInfo.PersonCount++;
			}
		}
		return ret;
	};
}

function SelectedAbsence(absenceId, date) {
	this.absenceId = absenceId;
	this.date = date;
	this.equals = function(other) {
		return this.absenceId === other.absenceId &&
			moment(this.date).format('YYYY-MM-DD') === moment(other.date).format('YYYY-MM-DD');
	}
}

function SelectedActivity(shiftLayerId, date) {
	this.shiftLayerId = shiftLayerId;
	this.date = date;
	this.equals = function (other) {
		return this.shiftLayerId === other.shiftLayerId &&
			moment(this.date).format('YYYY-MM-DD') === moment(other.date).format('YYYY-MM-DD');
	}
}


function lookUpIndex(array, target ) {	
	var index = -1;
	for (var i = 0; i < array.length; i++) {		
		if (target.equals(array[i])) {
			index = i;
			break;
		}		
	}
	return index;
}

function createDefaultPersonInfo(personSchedule, absences, activities) {
	return {
		PersonId: personSchedule.PersonId,
		Name: personSchedule.Name,
		Checked: true,
		AllowSwap: personSchedule.AllowSwap(),
		ScheduleStartTime: personSchedule.ScheduleStartTime(),
		ScheduleEndTime: personSchedule.ScheduleEndTime(),
		SelectedAbsences: absences || [],
		SelectedActivities: activities || [],
		Timezone: personSchedule.Timezone
	};
}

function addAbsence(absences, absenceId, date) {
	var targetSelectedAbsence = new SelectedAbsence(absenceId, date);
	var index = lookUpIndex(absences, targetSelectedAbsence);
	if (index < 0) {
		absences.push(targetSelectedAbsence);
	}
}

function deleteAbsence(absences, absenceId, date) {
	var targetSelectedAbsence = new SelectedAbsence(absenceId, date);
	var index = lookUpIndex(absences, targetSelectedAbsence);
	while (index >= 0) {
		absences.splice(index, 1);
		index = lookUpIndex(absences, targetSelectedAbsence);
	}
}

function addActivity(activities, shiftLayerId, date) {
	var targetSelectedActivity = new SelectedActivity(shiftLayerId, date);
	var index = lookUpIndex(activities, targetSelectedActivity);
	if (index < 0) {
		activities.push(targetSelectedActivity);
	}
}

function deleteActivity(activities, shiftLayerId, date) {
	var targetSelectedActivity = new SelectedActivity(shiftLayerId, date);
	var index = lookUpIndex(activities, targetSelectedActivity);
	while(index >= 0) {
		activities.splice(index, 1);
		index = lookUpIndex(activities, targetSelectedActivity);
	}
}
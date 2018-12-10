(function (angular) {
	'use strict';

	angular.module('wfm.teamSchedule').service('PersonSelection', PersonSelectionService);

	PersonSelectionService.$inject = ['serviceDateFormatHelper'];

	function PersonSelectionService(serviceDateFormatHelper) {
		var svc = this;
		svc.personInfo = {};

		svc.clearPersonInfo = function () {
			svc.personInfo = {};
		};

		svc.updatePersonSelection = function (personSchedule) {
			if (personSchedule.IsSelected) {
				var absences = [], activities = [];
				if (personSchedule.Shifts && personSchedule.Shifts.length > 0) {
					personSchedule.Shifts.forEach(function (shift) {
						if (shift.Date !== personSchedule.Date) {
							return;
						}

						if (!shift.Projections) return;

						shift.Projections.forEach(function (projection) {
							if (projection.ParentPersonAbsences) {
								projection.ParentPersonAbsences.forEach(function (personAbsId) {
									var targetSelectedAbsence = new SelectedAbsence(personAbsId, shift.Date);

									if (lookUpIndex(absences, targetSelectedAbsence) < 0) {
										absences.push(targetSelectedAbsence);
									}
								});
							} else if (projection.ShiftLayerIds && projection.Selectable()) {
								var targetShiftLayerIds = projection.ShiftLayerIds;
								targetShiftLayerIds.forEach(function (shiftLayerId) {
									var targetSelectedActivity = new SelectedActivity(shiftLayerId, shift.Date, projection.IsOvertime);
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
			if (preSelectedPersonIds.length === 0) return;
			angular.forEach(schedules, function (personSchedule) {
				var personId = personSchedule.PersonId;
				if (preSelectedPersonIds.indexOf(personId) > -1) {
					personSchedule.IsSelected = true;
					svc.updatePersonSelection(personSchedule);
					svc.toggleAllPersonProjections(personSchedule, viewDate);
				}
			});
		}

		svc.updatePersonInfo = function (schedules) {
			schedules.forEach(function (personSchedule) {
				var personId = personSchedule.PersonId;
				if (svc.personInfo[personId] && svc.personInfo[personId].Checked)
					personSchedule.IsSelected = true;
				else
					personSchedule.IsSelected = false;

				if (svc.personInfo[personId]) {
					svc.personInfo[personId].OrderIndex = personSchedule.Index;
					svc.personInfo[personId].Timezone = personSchedule.Timezone;
					svc.personInfo[personId].AllowSwap = personSchedule.AllowSwap();

					personSchedule.Shifts.forEach(function (shift) {
						if (shift.Date !== personSchedule.Date) {
							return;
						}

						if (!shift.Projections) return;

						shift.Projections.forEach(function (projection) {
							if (projection.ParentPersonAbsences && projection.ParentPersonAbsences.length > 0) {
								projection.Selected =
									!projection.ParentPersonAbsences.some(function (absenceId) {
										var targetAbsence = new SelectedAbsence(absenceId, shift.Date);
										return lookUpIndex(svc.personInfo[personId].SelectedAbsences, targetAbsence) < 0;
									});
							} else if (projection.ShiftLayerIds) {
								projection.Selected =
									!projection.ShiftLayerIds.some(function (shiftLayerId) {
										var targetActivity = new SelectedActivity(shiftLayerId, shift.Date, projection.IsOvertime);
										return lookUpIndex(svc.personInfo[personId].SelectedActivities, targetActivity) < 0;
									});
							}
						});
					});
				}
			});
		};

		svc.isAllProjectionSelected = function (personSchedule, viewDate) {
			return personSchedule.Shifts.every(function (shift) {
				if (shift.Date === serviceDateFormatHelper.getDateOnly(viewDate)) {
					return shift.Projections.every(function (projection) {
						if (projection.Selectable()) {
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
				if (shift.Date === serviceDateFormatHelper.getDateOnly(viewDate) || !personSchedule.IsSelected) {
					angular.forEach(shift.Projections, function (projection) {
						if (projection.Selectable()) {
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
					currentProjection.ParentPersonAbsences.forEach(function (absenceId) {
						addAbsence(personInfo.SelectedAbsences, absenceId, currentShift.Date);
					});
				} else if (currentProjection.ShiftLayerIds && currentProjection.Selectable()) {
					currentProjection.ShiftLayerIds.forEach(function (shiftLayerId) {
						addActivity(personInfo.SelectedActivities, shiftLayerId, currentShift.Date, currentProjection.IsOvertime);
					});
				}
			} else {
				if (currentProjection.ParentPersonAbsences && currentProjection.ParentPersonAbsences.length > 0) {
					currentProjection.ParentPersonAbsences.forEach(function (absenceId) {
						deleteAbsence(personInfo.SelectedAbsences, absenceId, currentShift.Date);
					});
				} else if (currentProjection.ShiftLayerIds && currentProjection.Selectable()) {
					currentProjection.ShiftLayerIds.forEach(function (shiftLayerId) {
						deleteActivity(personInfo.SelectedActivities, shiftLayerId, currentShift.Date, currentProjection.IsOvertime);
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
				svc.toggleAllPersonProjections(personSchedule, personSchedule.Date);
				if (!personSchedule.IsSelected && svc.personInfo[personSchedule.PersonId]) {
					delete svc.personInfo[personSchedule.PersonId];
				}
			});
			if (Object.keys(svc.personInfo).length > 0) {
				svc.personInfo = {};
			}
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
				.filter(function (info) {
					return info.Checked === true;
				});
		};

		svc.getSelectedPersonInfoList = function () {
			return svc.getSelectedPersonIdList()
				.map(function (key) {
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
			var isOnlyTodaySchedules = svc.personInfo[personIds[0]].ScheduleDate
				=== svc.personInfo[personIds[1]].ScheduleDate;
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

		svc.syncProjectionSelection = syncProjectionSelection;

		function SelectedAbsence(absenceId, date) {
			this.absenceId = absenceId;
			this.date = date;
			this.equals = function (other) {
				return this.absenceId === other.absenceId &&
					serviceDateFormatHelper.getDateOnly(this.date) === serviceDateFormatHelper.getDateOnly(other.date);
			}
		}

		function SelectedActivity(shiftLayerId, date, isOvertime) {
			this.shiftLayerId = shiftLayerId;
			this.date = date;
			this.isOvertime = isOvertime;
			this.equals = function (other) {
				return this.shiftLayerId === other.shiftLayerId &&
					serviceDateFormatHelper.getDateOnly(this.date) === serviceDateFormatHelper.getDateOnly(other.date);
			}
		}

		function lookUpIndex(array, target) {
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
			var isDayOff = personSchedule.DayOffs && personSchedule.DayOffs.filter(function (d) { return d.Date == personSchedule.Date; }).length > 0;
			var isEmptyDay = !isDayOff && (!absences || absences.length == 0) && (!activities || activities.length == 0);

			return {
				PersonId: personSchedule.PersonId,
				Name: personSchedule.Name,
				Checked: true,
				OrderIndex: personSchedule.Index,
				AllowSwap: personSchedule.AllowSwap(),
				IsDayOff: isDayOff,
				IsEmptyDay: isEmptyDay,
				IsFullDayAbsence: personSchedule.IsFullDayAbsence,
				ScheduleDate: personSchedule.Date,
				ScheduleEndTimeMoment: personSchedule.ScheduleEndTimeMoment && personSchedule.ScheduleEndTimeMoment(),
				SelectedAbsences: absences || [],
				SelectedActivities: activities || [],
				Timezone: personSchedule.Timezone,
				SelectedDayOffs: personSchedule.DayOffs || []
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

		function addActivity(activities, shiftLayerId, date, isOvertime) {
			var targetSelectedActivity = new SelectedActivity(shiftLayerId, date, isOvertime);
			var index = lookUpIndex(activities, targetSelectedActivity);
			if (index < 0) {
				activities.push(targetSelectedActivity);
			}
		}

		function deleteActivity(activities, shiftLayerId, date, isOvertime) {
			var targetSelectedActivity = new SelectedActivity(shiftLayerId, date, isOvertime);
			var index = lookUpIndex(activities, targetSelectedActivity);
			while (index >= 0) {
				activities.splice(index, 1);
				index = lookUpIndex(activities, targetSelectedActivity);
			}
		}

		function syncProjectionSelection(personSchedules) {
			var personInfo = this.personInfo
			personSchedules.forEach(function (sched) {
				if (!sched.Shifts) {
					return;
				}
				sched.Shifts.forEach(function (shift) {
					shift.Projections.forEach(function (proj) {
						if (!proj.ShiftLayerIds) {
							return;
						}
						proj.ShiftLayerIds.forEach(function (id) {
							if (!personInfo[sched.PersonId]) {
								return;
							}
							var selected = false;
							for (var i = 0; i < personInfo[sched.PersonId].SelectedActivities.length; i++) {
								if (personInfo[sched.PersonId].SelectedActivities[i].shiftLayerId === id) {
									selected = true;
									break;
								}
							}
							if (selected) {
								proj.Selected = true;
							}
						});
					});
				});
			}, this);
		}
	}
})(angular);
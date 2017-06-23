(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['$filter', 'teamsToggles', 'teamsPermissions', personScheduleFactory]);

	function personScheduleFactory($filter, toggles, permissions) {

		var personScheduleFactory = {
			Create: create
		};

		var getPersonAbsencesCount = function() {
			var personAbsences = [];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ParentPersonAbsences != null) {
					angular.forEach(projection.ParentPersonAbsences, function(personAbsId) {
						if (personAbsences.indexOf(personAbsId) === -1) {
							personAbsences.push(personAbsId);
						}
					});
				}
			});
			return personAbsences.length;
		};

		var getPersonActivitiesCount = function() {
			var personActivities =[];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ShiftLayerIds != null && !projection.IsOvertime) {
					angular.forEach(projection.ShiftLayerIds, function(shiftLayerId) {
						if (personActivities.indexOf(shiftLayerId) === -1) {
							personActivities.push(shiftLayerId);
						}
					});
				}

			});
			return personActivities.length;
		};

		function getProjectionTimeRange(schedule) {
			
			if (!schedule.Projection || schedule.Projection.length == 0) {
				return null;
			}
			
			return {
				Start:  schedule.Projection[0].Start,
				End: schedule.Projection[schedule.Projection.length - 1].End
			};
		}

		function create(schedule, timeLine) {
			if (!schedule) schedule = {};

			var personSchedule = new PersonSchedule(schedule, timeLine);

			var shiftVm = new ShiftViewModel(schedule, personSchedule, timeLine);
			var projectionVms = createProjections(schedule.Projection, timeLine, shiftVm);
			shiftVm.Projections = projectionVms;

			var dayOffVm = createDayOffViewModel(schedule.DayOff, timeLine, personSchedule);
						
			if (angular.isDefined(projectionVms)) personSchedule.Shifts = [shiftVm];
			if (angular.isDefined(dayOffVm)) personSchedule.DayOffs = [dayOffVm];
			
			return personSchedule;
		}

		function createProjections(projections, timeline, shiftVm) {
			if (angular.isUndefined(projections) || projections.length === 0) {
				return undefined;
			}

			var projectionVms = projections.map(function(projection) {
				return createShiftProjectionViewModel(projection, timeline, shiftVm);
			}).filter(function(result) {
				return angular.isDefined(result);
			});

			for (var i = 0; i < projectionVms.length; i ++) {
				projectionVms[i].SameTypeAsLast = i === 0? false: projectionVms[i].Description === projectionVms[i - 1].Description;
			}

			return projectionVms;
		}

		function createDayOffViewModel(dayOff, timeLine, personSchedule) {
			if (angular.isUndefined(dayOff) || dayOff === null) {
				return undefined;
			}

			var timelineStartMinute = timeLine.StartMinute;
			var timelineEndMinute = timeLine.EndMinute;
			var lengthPercentPerMinute = timeLine.LengthPercentPerMinute;
			
			var startTime = moment(dayOff.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			var endTime = moment(dayOff.End);
			var endTimeMinutes = endTime.diff(timeLine.Offset, 'minutes');

			if (startTimeMinutes > timelineEndMinute || endTimeMinutes < timelineStartMinute) {
				return undefined;
			}

			var displayStart = startTimeMinutes < timelineStartMinute ? timelineStartMinute : startTimeMinutes;
			var start = displayStart - timelineStartMinute;
			var startPosition = start * lengthPercentPerMinute;
			
			var displayEnd = endTimeMinutes <= timelineEndMinute ? endTimeMinutes : timelineEndMinute;
			var length = (displayEnd - displayStart) * lengthPercentPerMinute;

			var dayOffVm = new DayOffViewModel(dayOff.DayOffName, startPosition, length, personSchedule);
			return dayOffVm;
		}

		function useLightColor(color) {
			var getLumi = function (cstring) {
				var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
				if (!matched) return null;
				return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
			};
			var lightColor = '#00ffff';
			var darkColor = '#795548';
			var lumi = getLumi(color);
			if (!lumi) return false;
			return Math.abs(lumi - getLumi(lightColor)) > Math.abs(lumi - getLumi(darkColor));
		}

		function getContrastColor(hexcolor) {
			if (hexcolor[0] === '#') {
				hexcolor = hexcolor.substring(1);
			}
			var r = parseInt(hexcolor.substr(0, 2), 16);
			var g = parseInt(hexcolor.substr(2, 2), 16);
			var b = parseInt(hexcolor.substr(4, 2), 16);
			var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
			return (yiq >= 128) ? 'black' : 'white';
		}

		function createShiftProjectionViewModel(projection, timeLine, shiftVm) {
			if (!projection) projection = {};

			var startTime = moment(projection.Start);
			var endTime = moment(projection.End);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');
			var endTimeMinutes = endTime.diff(timeLine.Offset, 'minutes');
			
			var timelineStartMinute = timeLine.StartMinute;
			var timelineEndMinute = timeLine.EndMinute;

			var projectionMinutes = projection.Minutes;

			if ((startTimeMinutes > timelineEndMinute) || ((startTimeMinutes + projectionMinutes) <= timelineStartMinute)) {
				return undefined;
			}

			var start = startTimeMinutes < timelineStartMinute ? 0:startTimeMinutes - timelineStartMinute ;
			var startPosition = start * timeLine.LengthPercentPerMinute;

			var lengthReduction = 0;
			if (endTimeMinutes > timelineEndMinute) {
				lengthReduction += endTimeMinutes - timelineEndMinute;
			}
			if (startTimeMinutes < timelineStartMinute) {
				lengthReduction += timelineStartMinute - startTimeMinutes;
			}

			var lengthMinutes = projectionMinutes - lengthReduction;
			
			var length = lengthMinutes * timeLine.LengthPercentPerMinute;

			var shiftProjectionVm = new ProjectionViewModel(projection, shiftVm, length, startPosition);

			return shiftProjectionVm;
		}

		function merge(otherSchedule, timeLine) {
			var newShift = {
				Date: otherSchedule.Date,
				Projections: [],
				ProjectionTimeRange: getProjectionTimeRange(otherSchedule),
				AbsenceCount: getPersonAbsencesCount,
				ActivityCount: getPersonActivitiesCount,
				Parent: this
			};

			var otherProjections = createProjections(otherSchedule.Projection, timeLine, newShift);
			if (angular.isDefined(otherProjections)) {
				newShift.Projections = otherProjections;
				this.Shifts.push(newShift);
			}

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine, this);

			if (angular.isDefined(otherDayOffVm)) {
				this.DayOffs.push(otherDayOffVm);
			}
		}

		function mergeExtra(otherSchedule, timeLine) {
			var newShift = {
				Date: otherSchedule.Date,
				Projections: [],
				ProjectionTimeRange: getProjectionTimeRange(otherSchedule),
				AbsenceCount: getPersonAbsencesCount,
				ActivityCount: getPersonActivitiesCount,
				Parent: this
			};

			var otherProjections = createProjections(otherSchedule.Projection, timeLine, newShift);
			if (angular.isDefined(otherProjections)) {
				newShift.Projections = otherProjections;
				this.ExtraShifts.push(newShift);
			}

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine, this);

			if (angular.isDefined(otherDayOffVm)) {
				this.DayOffs.push(otherDayOffVm);
			}
		}

		function formatContractTimeMinutes(minutes) {
			return Math.floor(minutes / 60) + ':' + (minutes % 60 === 0 ? '00':minutes%60);
		}

		function PersonSchedule(schedule, timeLine) {
			this.ContractTime = formatContractTimeMinutes(schedule.ContractTimeMinutes);
			this.Date = schedule.Date;
			this.DayOffs = [];
			this.ExtraShifts = [];
			this.IsFullDayAbsence = schedule.IsFullDayAbsence;
			this.MultiplicatorDefinitionSetIds = schedule.MultiplicatorDefinitionSetIds;
			this.Name = schedule.Name;
			this.PersonId = schedule.PersonId;
			if (schedule.ShiftCategory)
				this.ShiftCategory = new ShiftCategory(schedule.ShiftCategory);
			this.Shifts = [];
			this.ExtraShifts = [];
			this.Timezone = schedule.Timezone;
			this.ViewRange = timeLine.MaximumViewRange;
		}

		PersonSchedule.prototype.AbsenceCount = function () {
			var shiftsOnCurrentDate = this.Shifts.filter(function (shift) {
				return this.Date === shift.Date;
			}, this);
			if (shiftsOnCurrentDate.length > 0) {
				return shiftsOnCurrentDate[0].AbsenceCount();
			}
			return 0;
		};

		PersonSchedule.prototype.ActivityCount = function () {
			var shiftsOnCurrentDate = this.Shifts.filter(function (shift) {
				return this.Date === shift.Date;
			}, this);
			if (shiftsOnCurrentDate.length > 0) {
				return shiftsOnCurrentDate[0].ActivityCount();
			}
			return 0;
		};

		PersonSchedule.prototype.AllowSwap = function () {
			return !this.IsFullDayAbsence;
		};

		PersonSchedule.prototype.IsSelected = false;

		PersonSchedule.prototype.Merge = merge;

		PersonSchedule.prototype.MergeExtra = mergeExtra;

		PersonSchedule.prototype.ScheduleEndTime = function () {
			var scheduleDate = this.Date;
			var end = moment(scheduleDate).endOf('day');
			for (var i = 0; i < this.Shifts.length; i++) {
				if (this.Shifts[i].Date === scheduleDate && this.Shifts[i].Projections.length > 0) {
					end = moment(this.Shifts[i].Projections[this.Shifts[i].Projections.length - 1].Start)
						.add(this.Shifts[i].Projections[this.Shifts[i].Projections.length - 1].Minutes, 'minutes');
				}
			}
			return end.format('YYYY-MM-DDTHH:mm:00');
		};

		PersonSchedule.prototype.ScheduleStartTime = function () {
			var start = this.Date;
			for (var i = 0; i < this.Shifts.length; i++) {
				if (this.Shifts[i].Date === start && this.Shifts[i].Projections.length > 0) {
					start = this.Shifts[i].Projections[0].Start;
				}
			}
			return moment(start).format('YYYY-MM-DDTHH:mm:00');
		};

		function ShiftViewModel(schedule, personSchedule, timeLine) {
			this.Date = schedule.Date;
			this.Parent = personSchedule;
			this.Projections = [];
			this.ProjectionTimeRange = getProjectionTimeRange(schedule);
		}

		ShiftViewModel.prototype.AbsenceCount = getPersonAbsencesCount;

		ShiftViewModel.prototype.ActivityCount = getPersonActivitiesCount;

		function ProjectionViewModel(projection, shiftVm, length, startPosition) {
			this.Color = projection.Color;
			this.Description = projection.Description;
			this.IsOvertime = projection.IsOvertime;
			this.Length = length;
			this.Minutes = projection.Minutes;
			this.Parent = shiftVm;
			this.ParentPersonAbsences = projection.ParentPersonAbsences;
			this.ShiftLayerIds = projection.ShiftLayerIds;
			this.Start = projection.Start;
			this.StartPosition = startPosition;
			this.UseLighterBorder = useLightColor(projection.Color);
		}

		ProjectionViewModel.prototype.SameTypeAsLast = false;

		ProjectionViewModel.prototype.Selected = false;

		ProjectionViewModel.prototype.TimeSpan = function () {
			var start = moment(this.Start);
			var end = moment(this.Start).add(this.Minutes, 'minute');
			if (!start.isSame(end, 'day')) {
				return $filter('date')(start.toDate(), 'short') + ' - ' + $filter('date')(end.toDate(), 'short');
			}
			return $filter('date')(start.toDate(), 'shortTime') + ' - ' + $filter('date')(end.toDate(), 'shortTime');
		};

		ProjectionViewModel.prototype.Selectable = function () {
			if (this.ParentPersonAbsences && this.ParentPersonAbsences.length > 0 && toggles.all().RemoveAbsenceEnabled) 
				return true;

			if (!this.ShiftLayerIds || this.ShiftLayerIds.length == 0) 
				return false;

			if (!this.IsOvertime && this.ShiftLayerIds && this.ShiftLayerIds.length > 0)
				return true;

			if((this.IsOvertime && this.ShiftLayerIds && this.ShiftLayerIds.length > 0) && toggles.all().WfmTeamSchedule_RemoveOvertime_42481 )
				return true;

			return false;
		};

		ProjectionViewModel.prototype.ToggleSelection = function () {
			this.Selected = !this.Selected;
		};

		function DayOffViewModel(name, startPosition, length, parent) {
			this.DayOffName = name;
			this.StartPosition = startPosition;
			this.Length = length;
			this.Parent = parent;
		}

		function ShiftCategory(shiftCategory) {
			this.Name = shiftCategory.Name || null;
			this.ShortName = shiftCategory.ShortName || null;
			this.DisplayColor = shiftCategory.DisplayColor || null;
			this.ContrastColor = getContrastColor(this.DisplayColor);
		}

		return personScheduleFactory;
	}
}());

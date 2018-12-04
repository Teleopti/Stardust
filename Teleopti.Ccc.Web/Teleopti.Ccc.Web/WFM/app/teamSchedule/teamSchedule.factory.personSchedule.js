(function () {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['serviceDateFormatHelper', 'colorUtils',
		personScheduleFactory]);

	function personScheduleFactory(
		serviceDateFormatHelper,
		colorUtils) {

		var personScheduleFactory = {
			Create: create
		};

		var getPersonAbsencesCount = function () {
			var personAbsences = [];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ParentPersonAbsences != null) {
					angular.forEach(projection.ParentPersonAbsences, function (personAbsId) {
						if (personAbsences.indexOf(personAbsId) === -1) {
							personAbsences.push(personAbsId);
						}
					});
				}
			});
			return personAbsences.length;
		};

		var getPersonActivitiesCount = function () {
			var personActivities = [];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ShiftLayerIds != null && !projection.IsOvertime) {
					angular.forEach(projection.ShiftLayerIds, function (shiftLayerId) {
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
				Start: schedule.Projection[0].Start,
				StartMoment: schedule.Projection[0].StartMoment,
				End: schedule.Projection[schedule.Projection.length - 1].End,
				EndMoment: schedule.Projection[schedule.Projection.length - 1].EndMoment,
			};
		}

		function create(schedule, timeLine, index) {
			if (!schedule) schedule = {};

			var personSchedule = new PersonSchedule(schedule, timeLine, index);

			var shiftVm = new ShiftViewModel(schedule, personSchedule);
			var projectionVms = createProjections(schedule.Projection, timeLine, shiftVm);
			shiftVm.Projections = projectionVms;

			var dayOffVm = createDayOffViewModel(schedule.Date, schedule.DayOff, timeLine, personSchedule);

			if (angular.isDefined(projectionVms)) personSchedule.Shifts = [shiftVm];

			if (angular.isDefined(dayOffVm)) personSchedule.DayOffs = [dayOffVm];

			return personSchedule;
		}

		function createProjections(projections, timeline, shiftVm) {
			if (angular.isUndefined(projections) || projections.length === 0) {
				return undefined;
			}

			var projectionVms = projections.map(function (projection) {
				return createShiftProjectionViewModel(projection, timeline, shiftVm);
			}).filter(function (result) {
				return angular.isDefined(result);
			});

			for (var i = 0; i < projectionVms.length; i++) {
				projectionVms[i].ShowDividedLine = i === 0 ? false : projectionVms[i].Description === projectionVms[i - 1].Description;
			}

			return projectionVms;
		}

		function createDayOffViewModel(scheduleDate, dayOff, timeLine, personSchedule) {
			if (angular.isUndefined(dayOff) || dayOff === null) {
				return undefined;
			}

			var timelineStartMinute = timeLine.StartMinute;
			var timelineEndMinute = timeLine.EndMinute;
			var lengthPercentPerMinute = timeLine.LengthPercentPerMinute;

			var startTimeMinutes = dayOff.StartMoment.diff(timeLine.Offset, 'minutes');
			var endTimeMinutes = dayOff.EndMoment.diff(timeLine.Offset, 'minutes');

			if (startTimeMinutes > timelineEndMinute || endTimeMinutes < timelineStartMinute) {
				return undefined;
			}

			var displayStart = startTimeMinutes < timelineStartMinute ? timelineStartMinute : startTimeMinutes;
			var start = displayStart - timelineStartMinute;
			var startPosition = start * lengthPercentPerMinute;

			var displayEnd = endTimeMinutes <= timelineEndMinute ? endTimeMinutes : timelineEndMinute;
			var length = (displayEnd - displayStart) * lengthPercentPerMinute;

			var dayOffVm = new DayOffViewModel(scheduleDate, dayOff.DayOffName, startPosition, length, personSchedule);
			return dayOffVm;
		}

		function createShiftProjectionViewModel(projection, timeLine, shiftVm) {
			if (!projection) projection = {};

			var startTimeMinutes = projection.StartMoment.diff(timeLine.Offset, 'minutes');
			var endTimeMinutes = projection.EndMoment.diff(timeLine.Offset, 'minutes');

			var timelineStartMinute = timeLine.StartMinute;
			var timelineEndMinute = timeLine.EndMinute;

			var projectionMinutes = projection.EndMoment.diff(projection.StartMoment, 'minutes');

			if ((startTimeMinutes >= timelineEndMinute) || ((startTimeMinutes + projectionMinutes) <= timelineStartMinute)) {
				return undefined;
			}

			var start = startTimeMinutes < timelineStartMinute ? 0 : startTimeMinutes - timelineStartMinute;
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

			var otherDayOffVm = createDayOffViewModel(otherSchedule.Date, otherSchedule.DayOff, timeLine, this);

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

			var otherDayOffVm = createDayOffViewModel(otherSchedule.Date, otherSchedule.DayOff, timeLine, this);

			if (angular.isDefined(otherDayOffVm)) {
				this.DayOffs.push(otherDayOffVm);
			}
		}

		function formatContractTimeMinutes(minutes) {
			return Math.floor(minutes / 60) + ':' + (minutes % 60 === 0 ? '00' : minutes % 60);
		}

		function PersonSchedule(schedule, timeLine, index) {
			this.Index = index;
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
			this.Timezone = schedule.Timezone;
			this.ViewRange = timeLine.MaximumViewRange;
			this.IsProtected = schedule.IsProtected;
			this.UnderlyingScheduleSummary = schedule.UnderlyingScheduleSummary;
		}

		PersonSchedule.prototype.IsDayOff = function () {
			var date = this.Date;
			return !!this.DayOffs.filter(function (d) {
				return d.Date == serviceDateFormatHelper.getDateOnly(date);
			}).length;
		};

		PersonSchedule.prototype.HasUnderlyingSchedules = function () {
			return !!this.UnderlyingScheduleSummary;
		}

		PersonSchedule.prototype.AbsenceCount = function () {
			if (this.Shifts[0] && this.Shifts[0].Date === this.Date) {
				return this.Shifts[0].AbsenceCount();
			}
			return 0;
		};

		PersonSchedule.prototype.ActivityCount = function () {
			if (this.Shifts[0] && this.Shifts[0].Date === this.Date) {
				return this.Shifts[0].ActivityCount();
			}
			return 0;
		};

		PersonSchedule.prototype.AllowSwap = function () {
			return !this.IsFullDayAbsence;
		};

		PersonSchedule.prototype.IsSelected = false;

		PersonSchedule.prototype.Merge = merge;

		PersonSchedule.prototype.MergeExtra = mergeExtra;

		PersonSchedule.prototype.ScheduleEndTimeMoment = function () {
			var shift = this.Shifts[0];
			if (shift && shift.Date === this.Date && !!shift.Projections.length) {
				return shift.Projections[shift.Projections.length - 1].EndMoment;
			}
			return moment.tz(this.Date + 'T23:59:00', this.Timezone.IanaId);
		};

		PersonSchedule.prototype.ScheduleStartTimeMoment = function () {
			var shift = this.Shifts[0];
			if (shift && shift.Date === this.Date && !!shift.Projections.length) {
				return shift.Projections[0].StartMoment;
			}
			return moment.tz(this.Date + 'T00:00:00', this.Timezone.IanaId);
		};

		PersonSchedule.prototype.HasHiddenScheduleAtStart = function () {
			return !!this.Shifts.length
				&& this.Shifts.some(function (shift) {
					return shift.Projections.length && shift.ProjectionTimeRange.StartMoment.isBefore(this.ViewRange.startMoment);
				}, this);
		}

		PersonSchedule.prototype.HasHiddenScheduleAtEnd = function () {
			return !!this.Shifts.length
				&& this.Shifts.some(function (shift) {
					return shift.Projections.length && shift.ProjectionTimeRange.EndMoment.isAfter(this.ViewRange.endMoment);
				}, this);
		}

		function ShiftViewModel(schedule, personSchedule) {
			this.Date = schedule.Date;
			this.Parent = personSchedule;
			this.Projections = [];
			this.ProjectionTimeRange = getProjectionTimeRange(schedule);
		}

		ShiftViewModel.prototype.AbsenceCount = getPersonAbsencesCount;

		ShiftViewModel.prototype.ActivityCount = getPersonActivitiesCount;

		function ProjectionViewModel(projection, shiftVm, length, startPosition) {
			this.Color = projection.Color;
			this.TextColor = colorUtils.getTextColorBasedOnBackgroundColor(projection.Color),
				this.Description = projection.Description;
			this.IsOvertime = projection.IsOvertime;
			this.Length = length;
			this.Parent = shiftVm;
			this.ParentPersonAbsences = projection.ParentPersonAbsences;
			this.ShiftLayerIds = projection.ShiftLayerIds;
			this.Start = projection.Start;
			this.End = projection.End;
			this.StartMoment = projection.StartMoment;
			this.EndMoment = projection.EndMoment;
			this.StartPosition = startPosition;
			this.UseLighterBorder = this.TextColor == 'white';
			this.TimeSpan = projection.TimeSpan;
			this.Minutes = projection.EndMoment.diff(projection.StartMoment, 'minutes');
		}

		ProjectionViewModel.prototype.ShowDividedLine = false;

		ProjectionViewModel.prototype.Selected = false;

		ProjectionViewModel.prototype.Selectable = function () {
			if (this.ParentPersonAbsences && this.ParentPersonAbsences.length > 0)
				return true;

			if (!this.ShiftLayerIds || this.ShiftLayerIds.length == 0)
				return false;

			if (this.ShiftLayerIds && this.ShiftLayerIds.length > 0)
				return true;

			return false;
		};

		ProjectionViewModel.prototype.ToggleSelection = function () {
			this.Selected = !this.Selected;
		};

		function DayOffViewModel(scheduleDate, name, startPosition, length, parent) {
			this.DayOffName = name;
			this.StartPosition = startPosition;
			this.Length = length;
			this.Parent = parent;
			this.Date = scheduleDate;
		}

		function ShiftCategory(shiftCategory) {
			this.Name = shiftCategory.Name;
			this.ShortName = shiftCategory.ShortName;
			this.DisplayColor = shiftCategory.DisplayColor;
			this.TextColor = colorUtils.getTextColorBasedOnBackgroundColor(this.DisplayColor);
		}

		return personScheduleFactory;
	}
})();

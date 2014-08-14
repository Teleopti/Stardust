define([
	'knockout',
	'resources',
	'ajax',
	'navigation',
    'errorview',
	'guidgenerator',
	'notifications'
], function (
	ko,
	resources,
	ajax,
	navigation,
    errorview,
	guidgenerator,
	notificationsViewModel
    ) {

	return function () {

		var self = this;

		var personName;

		self.PersonId = ko.observable();
		self.GroupId = ko.observable();
		self.ScheduleDate = ko.observable();
		self.OldStartMinutes = ko.observable();
		self.ProjectionLength = ko.observable();
		self.StartTime = ko.observable();
		self.Activities = ko.observableArray();
		self.WorkingShift = ko.observable();
		self.layer = ko.observable();
		self.SelectedStartMinutes = ko.observable();
		self.TimeZoneName = ko.observable();

		this.SetData = function (data) {
			self.PersonId(data.PersonId);
			personName = data.PersonName;
			self.GroupId(data.GroupId);
			self.ScheduleDate(data.Date);
			self.Activities(data.Activities);
		    self.TimeZoneName(data.TimeZoneName);
		};

		this.DisplayedStartTime = ko.computed({
			read: function () {
				if (!self.ScheduleDate() || !self.StartTime()) return '';
				return self.StartTime().format(resources.TimeFormatForMoment);
			},
			write: function (option) {
			    if (!self.layer()) return;
				var inputTime = getMomentFromInput(option);
				if (!isLayerWithinShift(inputTime)) {
					self.StartTime(self.getStartTimeFromMinutes(self.OldStartMinutes()));
				} else {
					self.StartTime(inputTime);
					self.layer().StartMinutes(self.getMinutesFromTime(self.StartTime()));
				}
			}
		});

		var isLayerWithinShift = function (startTime) {
			if (!self.WorkingShift()) return false;
			var shiftStartMinutes = self.WorkingShift().OriginalShiftStartMinutes;
			var shiftEndMinutes = self.WorkingShift().OriginalShiftEndMinutes;
			var inputMinutes = self.getMinutesFromTime(startTime);
		    if (inputMinutes < shiftStartMinutes && self.shiftOverMidnight())
		        inputMinutes += 1440;
			if (inputMinutes >= shiftStartMinutes && inputMinutes + self.layer().LengthMinutes() <= shiftEndMinutes)
				return true;
			return false;
		};

		this.update = function (layer) {
			
			if (layer) {
				self.layer(layer);
				self.OldStartMinutes(layer.StartMinutes());
				self.StartTime(self.getStartTimeFromMinutes(self.OldStartMinutes()));
				self.ProjectionLength(layer.LengthMinutes());
			}
		};

		// these two methods could be refactored to be more generic
		this.getMinutesFromTime = function(time) {
			return time.diff(self.ScheduleDate(), 'minutes');
		};

		this.getMinutesFromStartTime = function () {
			return self.getMinutesFromTime(self.StartTime());
		};

	    this.getStartTimeFromMinutes = function(minutes) {
	        var date = self.ScheduleDate();
	        return moment([date.year(), date.month(), date.date(), minutes / 60, minutes % 60]);
	    };


	    this.Apply = function () {
	        var activity = self.getActivity();
	        if (!activity) {
	            errorview.display(resources.FunctionNotAvailable);
	            return;
	        }
	        var trackId = guidgenerator.newGuid();
			var requestData = JSON.stringify({
				AgentId: self.PersonId(),
				ScheduleDate: self.ScheduleDate().format(),
				NewStartTime: self.StartTime().format(),
				OldStartTime: moment(self.getStartTimeFromMinutes(self.OldStartMinutes())).format(),
				ActivityId: activity.Id,
				OldProjectionLayerLength: self.ProjectionLength(),
				TrackedCommandInfo: { TrackId: trackId }
			});
		    ajax.ajax({
				    url: 'PersonScheduleCommand/MoveActivity',
				    type: 'POST',
				    data: requestData,
				    success: function(data, textStatus, jqXHR) {
					    navigation.GoToTeamSchedule(self.GroupId(), self.ScheduleDate());
				    },
				    statusCode501: function (jqXHR, textStatus, errorThrown) {
				    	errorview.display(resources.FunctionNotAvailable);
					    notificationsViewModel.RemoveNotification(trackId);
				    },
				    statusCode500: function (jqXHR, textStatus, errorThrown) {
				    	notificationsViewModel.UpdateNotification(trackId, 3);
				    }
			    }
		    );
		    notificationsViewModel.AddNotification(trackId, resources.MovingActivityFor + " " + personName + "... ");
	    };

	    this.getActivity = function() {
	        var activity = ko.utils.arrayFirst(self.Activities(), function (a) {
	            return a.Name === self.layer().Description;
	        });
            return activity;
	    };

		this.ErrorMessage = ko.computed(function () {
			return undefined;
		});

		var getMomentFromInput = function (input) {
		    var momentInput = moment(input, resources.TimeFormatForMoment);
		    var date = self.ScheduleDate();
		    if (!beforeMidnight(momentInput) && self.shiftOverMidnight())
		        return moment([date.year(), date.month(), date.date(), momentInput.hour(), momentInput.minute()]).add(1, 'day');
			return moment([date.year(), date.month(), date.date(), momentInput.hour(), momentInput.minute()]);
		};

	    this.shiftOverMidnight = function() {
	        return self.WorkingShift().OriginalShiftEndMinutes > 1440;
	    };

	    var beforeMidnight = function (momentInput) {
	        var shiftStartTime = self.getStartTimeFromMinutes(self.WorkingShift().OriginalShiftStartMinutes).format(resources.TimeFormatForMoment);
	        return momentInput <= moment("23:59", resources.TimeFormatForMoment) && momentInput >= moment(shiftStartTime, resources.TimeFormatForMoment);
	    };

	    this.isMovingToAnotherDay = function() {
	        return self.WorkingShift().OriginalShiftStartMinutes < 1440 && self.WorkingShift().Layers()[0].StartMinutes() >= 1440;
	    };
        
	    this.reset = function() {
	        var date = self.ScheduleDate();
	        var time = self.SelectedStartMinutes();
	        self.StartTime(moment([date.year(), date.month(), date.date(), time / 60, time % 60]));
	    };
	};
});
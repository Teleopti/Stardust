﻿Teleopti.MyTimeWeb.MyAdherence = (function () {
	var vm;

	function MyAdherenceViewModel(loadDataMethod, date) {
		var self = this;

		self.selectedDateInternal = ko.observable(date);
		self.datePickerFormat = ko.observable('YYYYMMDD');
		var format = $('#my-report-datepicker-format').val().toUpperCase();
		self.datePickerFormat(format);
		self.dataAvailable = ko.observable();
		self.goToAnotherDay = function (toDate) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/Adherence" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(toDate.format('YYYY-MM-DD')));
		};
		self.selectedDate = ko.computed({
			read: function () {
				return self.selectedDateInternal();
			},
			write: function (value) {
				if (value.format('YYYYMMDD') == date.format('YYYYMMDD')) return;
				self.goToAnotherDay(value);
			}
		});
		self.nextDay = function () {
			self.goToAnotherDay(self.selectedDate().clone().add('days', 1));
		};
		self.previousDay = function() {
			self.goToAnotherDay(self.selectedDate().clone().add('days', -1));
		};
		self.dateFormat = function() {
			return self.datePickerFormat;
		};

		self.goToOverview = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/Index" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(self.selectedDateInternal().format('YYYY-MM-DD')));
		};

		self.totalAdherence = ko.observable();
		self.intervalAdherence = ko.observableArray();
		self.intervalsPerDay = ko.observable();
		self.startInterval = ko.observable();
		self.lastInterval = ko.observable();
		self.intervalMinutes = ko.computed(function() {
			return 1440 / self.intervalsPerDay();
		});
		self.intervalsPerHour = ko.computed(function () {
			return self.intervalsPerDay()/24;
		});

		self.timeLines = ko.computed(function() {
			var times = [];
			var startInterval = self.startInterval();
			var lastInterval = self.lastInterval();
			if (startInterval && lastInterval) {
				var start = startInterval.IntervalId;
				var rest = start % self.intervalsPerHour();
				var time = start - rest;
				var end = lastInterval.IntervalId + self.intervalsPerHour() - (lastInterval.IntervalId % self.intervalsPerHour());
				while (time <= end) {
					times.push({
						'Time': moment().startOf('day').add('minutes', time * self.intervalMinutes()).format("HH:mm"),
						'Position': intervalLeftPos(time)
					});
					time = time + self.intervalsPerHour();
				}
			}
			return times;
		});

		function intervalLeftPos(intervalId) {
			var intervalsPerDay = self.intervalsPerDay();
			var startInterval = self.startInterval().IntervalId;
			startInterval = startInterval - startInterval % self.intervalsPerHour();
			if (intervalId < startInterval) {
				intervalId = intervalId + intervalsPerDay;
			}
			var number = intervalId - startInterval;
			return (number * 15) + 'px';
		};

		self.schedules = ko.computed(function () {
			var schedules = [];
			var intervals = self.intervalAdherence();
			if (self.startInterval() && self.lastInterval()) {
				var start = self.startInterval().IntervalId;
				var time = start;
				var end = self.lastInterval().IntervalId;
				while (time < end + 1) {
					var currentInterval = intervals[time - start];
					if (currentInterval) {
						var barLength = 80 * currentInterval.Adherence;
						schedules.push({
							'Color': currentInterval.Color,
							'Position': intervalLeftPos(currentInterval.IntervalId),
							'BarLength': barLength + 'px',
							'Margin': (80 - barLength) + 'px',
						});
					}
					time = time + 1;
				}
			}
			return schedules;
		});

		loadDataMethod(date);
	}

	function fillData(date) {
		$.ajax({
			url: 'MyTime/MyReport/AdherenceDetails',
			dataType: 'json',
			cache: false,
			data: { date: date.clone().utc().toDate().toJSON() },
			success: function (data) {
				vm.selectedDateInternal(date);
				vm.totalAdherence(data.TotalAdherence);
				vm.intervalsPerDay(data.IntervalsPerDay);
				if (data.Intervals && data.Intervals.length !== 0) {
					vm.startInterval(data.Intervals[0]);
					vm.lastInterval(data.Intervals[data.Intervals.length - 1]);
					vm.intervalAdherence(data.Intervals);
				}
				vm.dataAvailable(data.DataAvailable);
			}

		});
	}

	function bindData() {
		return Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
			vm = new MyAdherenceViewModel(fillData, getDate());
			var elementToBind = $('.myadherence')[0];
			ko.applyBindings(vm, elementToBind);
		});
	}
	
	function getDate() {
		var date = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
		if (date != '') {
			return moment(date, "YYYYMMDD");
		} else {
			return moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day');
		}
	}

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('MyReport/Adherence',
									Teleopti.MyTimeWeb.MyAdherence.MyAdherencePartialInit, Teleopti.MyTimeWeb.MyAdherence.MyAdherencePartialDispose);
		},

		MyAdherencePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			$('#page').removeClass('fixed-non-responsive');
			if (!$('.myadherence').length) {
				return;
			}

			bindData();

			readyForInteractionCallback();
			completelyLoadedCallback();
		},

		MyAdherencePartialDispose: function () {
			$('#page').addClass('fixed-non-responsive');
		},

		ForDay: function (date) {
			fillData(date);
		}
	};
})(jQuery);
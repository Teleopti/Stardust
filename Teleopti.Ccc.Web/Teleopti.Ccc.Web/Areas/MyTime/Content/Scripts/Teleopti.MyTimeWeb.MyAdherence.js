Teleopti.MyTimeWeb.MyAdherence = (function () {
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
			var intervals = self.intervalAdherence();
			if (self.startInterval() && self.lastInterval()) {
				var start = self.startInterval().IntervalId;
				var time = start;
				var end = self.lastInterval().IntervalId;
				if (end < start)
					end += self.intervalsPerDay();
				while (time < end + 1) {
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
			if (intervalId < self.startInterval().IntervalId) {
				intervalId = intervalId + self.intervalsPerDay();
			}
			var number = intervalId - self.startInterval().IntervalId;
			return (number * 15) + 'px';
		};

		self.schedules = ko.computed(function () {
			var schedules = [];
			var intervals = self.intervalAdherence();
			if (self.startInterval() && self.lastInterval()) {
				var start = self.startInterval().IntervalId;
				var time = start;
				var end = self.lastInterval().IntervalId;
				if (end < start)
					end += self.intervalsPerDay();
				while (time < end + 1) {
					var currentInterval = intervals[time - start];
					var barLength = currentInterval ? 80 * currentInterval.Adherence : 0;
					schedules.push({
						'Color': currentInterval ? currentInterval.Color : "",
						'Position': currentInterval ? intervalLeftPos(currentInterval.IntervalId) : '0px',
						'BarLength': barLength + 'px',
						'Margin': (80 - barLength) + 'px',
					});
					
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
				
				//vm.answeredCalls = data.AnsweredCalls;
				//vm.averageAfterCallWork = data.AverageAfterCallWork;
				//vm.averageHandlingTime = data.AverageHandlingTime;
				//vm.averageTalkTime = data.AverageTalkTime;
				//vm.readyTimePerScheduledReadyTime = data.ReadyTimePerScheduledReadyTime;
				vm.dataAvailable(data.DataAvailable);
			}

		});
	}

	function setWeekStart() {
		$.ajax({
			url: 'UserInfo/Culture',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
				ko.applyBindings(vm, $('div.navbar')[1]);
			}
		});
	};

	function bindData() {
		vm = new MyAdherenceViewModel(fillData, getDate());
		var elementToBind = $('.myadherence')[0];
		ko.applyBindings(vm, elementToBind);
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
									Teleopti.MyTimeWeb.MyAdherence.MyAdherencePartialInit);
		},

		MyAdherencePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			if (!$('.myadherence').length) {
				return;
			}

			bindData();
			setWeekStart();

			readyForInteractionCallback();
			completelyLoadedCallback();
		},

		ForDay: function (date) {
			fillData(date);
		}
	};
})(jQuery);
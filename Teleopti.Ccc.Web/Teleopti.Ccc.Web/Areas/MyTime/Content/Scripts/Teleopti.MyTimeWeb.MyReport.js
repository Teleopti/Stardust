Teleopti.MyTimeWeb.MyReport = (function () {
	var vm;

	function MyReportViewModel(loadDataMethod, date) {
		var self = this;

		self.adherence = ko.observable();
		self.answeredCalls = ko.observable();
		self.averageAfterCallWork = ko.observable();
		self.averageHandlingTime = ko.observable();
		self.averageTalkTime = ko.observable();
		self.readyTimePerScheduledReadyTime = ko.observable();

		self.selectedDateInternal = ko.observable(date);
		self.datePickerFormat = ko.observable('YYYYMMDD');
		var format = $('#my-report-datepicker-format').val().toUpperCase();
		self.datePickerFormat(format);
		self.dataAvailable = ko.observable();
		self.goToAnotherDay = function (toDate) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/Index" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(toDate.format('YYYY-MM-DD')));
		};
		self.goToAdherence = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/Adherence" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(self.selectedDateInternal().format('YYYY-MM-DD')));
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
		loadDataMethod(date);
	}

	function fillData(date) {
		$.ajax({
			url: 'MyTime/MyReport/Overview',
			dataType: 'json',
			data: { date: date.clone().utc().toDate().toJSON() },
			success: function (data) {
				vm.selectedDateInternal(date);
				vm.adherence(data.Adherence);
				vm.answeredCalls(data.AnsweredCalls);
				vm.averageAfterCallWork(data.AverageAfterCallWork);
				vm.averageHandlingTime(data.AverageHandlingTime);
				vm.averageTalkTime(data.AverageTalkTime);
				vm.readyTimePerScheduledReadyTime(data.ReadyTimePerScheduledReadyTime);
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
		vm = new MyReportViewModel(fillData, getDate());
		var elementToBind = $('.myreport-daily-metrics')[0];
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
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('MyReport/Index',
									Teleopti.MyTimeWeb.MyReport.MyReportPartialInit);
		},
		MyReportPartialInit: function () {
			if (!$('.myreport-daily-metrics').length) {
				return;
			}

			bindData();
			setWeekStart();
		},
		
		ForDay: function(date) {
			fillData(date);
		}		
	};
})(jQuery);
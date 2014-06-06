Teleopti.MyTimeWeb.MyQueueMetrics = (function () {
	var vm;

	function MyQueueMetricsViewModel(loadDataMethod, date) {
		var self = this;

		self.selectedDateInternal = ko.observable(date);
		self.datePickerFormat = ko.observable('YYYYMMDD');
		var format = $('#my-report-datepicker-format').val().toUpperCase();
		self.datePickerFormat(format);
		self.dataAvailable = ko.observable();
		self.goToAnotherDay = function (toDate) {
		    Teleopti.MyTimeWeb.Portal.NavigateTo("MyReport/QueueMetrics" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(toDate.format('YYYY-MM-DD')));
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

		self.myQueues = ko.observableArray();

		loadDataMethod(date);
	}

	function fillData(date) {
		$.ajax({
		    url: 'MyTime/MyReport/QueueMetricsDetails',
			dataType: 'json',
			data: { date: date.clone().utc().toDate().toJSON() },
			success: function (data) {
				vm.selectedDateInternal(date);
				vm.myQueues(data);
				vm.dataAvailable(data[0].DataAvailable);
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
	    vm = new MyQueueMetricsViewModel(fillData, getDate());
	    var elementToBind = $('#queueMetric')[0];
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
		    Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('MyReport/QueueMetrics',
									Teleopti.MyTimeWeb.MyQueueMetrics.MyQueueMetricsPartialInit, Teleopti.MyTimeWeb.MyQueueMetrics.MyQueueMetricsPartialDispose);
		},

		MyQueueMetricsPartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			//$('#page').removeClass('fixed-non-responsive');
		    if (!$('#queueMetric').length) {
				return;
			}

			bindData();
			setWeekStart();

			readyForInteractionCallback();
			completelyLoadedCallback();
		},

		MyQueueMetricsPartialDispose: function () {
			//$('#page').addClass('fixed-non-responsive');
		},

		ForDay: function (date) {
			fillData(date);
		}
	};
})(jQuery);
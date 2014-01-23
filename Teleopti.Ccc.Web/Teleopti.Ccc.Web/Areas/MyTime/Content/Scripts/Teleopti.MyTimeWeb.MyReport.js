Teleopti.MyTimeWeb.MyReport = (function() {

	var vm = new MyReportViewModel();

	function MyReportViewModel() {
		var self = this;
		self.AdherenceText = '';
		self.ReadinessText = '';
		self.AnsweredCallsText = '';
		self.AverageTalkTimeText = '';
		self.AverageAfterWorkText = '';
		self.AverageHandlingTimeText = '';
		self.Adherence = ko.observable();
		self.AnsweredCalls = ko.observable();
		self.AverageAfterWork = ko.observable();
		self.AverageHandlingTime = ko.observable();
		self.AverageTalkTime = ko.observable();
		self.Readiness = ko.observable();
		self.DisplayDate = ko.observable();
		self.nextWeekDate = ko.observable(moment());
		self.previousWeekDate = ko.observable(moment());
		self.selectedDate = ko.observable(moment().startOf('day').clone().add('days', -1));
		self.AdherenceValues = "";
		self.ReadinessValues = "";
		self.HandlingTimeValues = "";
		self.TalkTimeValues = "";
		self.AfterTalkTimeValues = "";
		self.AnsweredValues = "";
	}

	vm.selectedDate.subscribe(fillData);

	function fillData() {
		$.ajax({
			//url: 'MyTime/MyReport/OnDates?startDate=' +  moment(vm.selectedDate()).format("YYYY-MM-DD"),
			url: 'MyTime/MyReport/OnDates',
			dataType: 'json',
			data: { OnDate: moment(vm.selectedDate()).format("YYYY-MM-DD"), ShowWeek: false },
			success: function(data) {
				vm.Adherence(data.Adherence);
				vm.AnsweredCalls(data.AnsweredCalls);
				vm.AverageAfterWork(data.AverageAfterWork);
				vm.AverageHandlingTime(data.AverageHandlingTime);
				vm.AverageTalkTime(data.AverageTalkTime);
				vm.Readiness(data.Readiness);
				vm.DisplayDate(data.DisplayDate);
				vm.AdherenceText = data.AdherenceText;
				vm.ReadinessText = data.ReadinessText;
				vm.AnsweredCallsText = data.AnsweredCallsText;
				vm.AverageTalkTimeText = data.AverageTalkTimeText;
				vm.AverageAfterWorkText = data.AverageAfterWorkText;
				vm.AverageHandlingTimeText = data.AverageHandlingTimeText;

				vm.AdherenceValues = data.AdherenceValues;
				vm.ReadinessValues = data.ReadinessValues;
				vm.HandlingTimeValues = data.HandlingTimeValues;
				vm.TalkTimeValues = data.TalkTimeValues;
				vm.AfterTalkTimeValues = data.AfterTalkTimeValues;
				vm.AnsweredValues = data.AnsweredValues;
				drawTheChart(vm.AdherenceValues, vm.AdherenceText);

			},
			error: function(xhr, ajaxOptions, thrownError) {
				alert(xhr.status);
				alert(xhr.responseText);
				alert(thrownError);
			}
		});
	}

	function drawTheChart(datat, title) {
		
		// Create the data table.
		var data = new google.visualization.DataTable();
		data.addColumn('string', '');
		data.addColumn('number', '');
		
		datat.forEach(function (entry) {
			data.addRow([entry.Date, entry.Value]);
		});

		// Set chart options
		var options = {
			'width': '100%',
			'height': 250,
			vAxis: { minValue: 0 },
			'fontName': "Segoe UI",
			'title': title,
			titleFontSize: 22,
		};
		// Instantiate and draw our chart, passing in some options.
		var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
		chart.draw(data, options);
	}
	

	// to week view
	//vm.nextWeek = function () {
	//	vm.selectedDate(self.nextWeekDate());
	//};

	//vm.previousWeek = function () {
	//	vm.selectedDate(self.previousWeekDate());
	//};

	return {
		Init: function() {
			fillData();

			$.ajax({
				url: 'UserInfo/Culture',
				dataType: "json",
				type: 'GET',
				success: function(data) {
					$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
					ko.applyBindings(vm);
				}
			});
		},

		drawChart: function (datat, title) {
			drawTheChart(datat, title);
		},

		yesterday: function() {
			vm.selectedDate(moment().startOf('day').clone().add('days', -1));
		},

		nextDay: function() {
			vm.selectedDate(vm.selectedDate().clone().add('days', 1));
		},

		previousDay: function() {
			vm.selectedDate(vm.selectedDate().clone().add('days', -1));
		}
	};
})(jQuery);
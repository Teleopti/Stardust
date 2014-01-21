Teleopti.MyTimeWeb.MyReport = (function() {
	
	var vm = new MyReportViewModel();
	
	function MyReportViewModel() {
		var self = this;

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
			
	}

	vm.selectedDate.subscribe(fillData);
	
	function fillData() {
		$.ajax({
			//url: 'MyTime/MyReport/OnDates?startDate=' +  moment(vm.selectedDate()).format("YYYY-MM-DD"),
			url: 'MyTime/MyReport/OnDates',
			dataType: 'json',
			data: { OnDate: moment(vm.selectedDate()).format("YYYY-MM-DD"), ShowWeek: false },
			success: function (data) {
				vm.Adherence(data.Adherence);
				vm.AnsweredCalls(data.AnsweredCalls);
				vm.AverageAfterWork(data.AverageAfterWork);
				vm.AverageHandlingTime(data.AverageHandlingTime);
				vm.AverageTalkTime(data.AverageTalkTime);
				vm.Readiness(data.Readiness);
				vm.DisplayDate(data.DisplayDate);
				//vm.selectedDate = DisplayDate;
				
			},
			error: function (xhr, ajaxOptions, thrownError) {
				alert(xhr.status);
				alert(xhr.responseText);
				alert(thrownError);
			}
		});
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
				success: function (data) {
					$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
					ko.applyBindings(vm);
				}
			});
		},
		
		yesterday: function(){
			vm.selectedDate(moment().startOf('day').clone().add('days', -1));
		},
		
		nextDay: function () {
			vm.selectedDate(vm.selectedDate().clone().add('days', 1));
		},

		previousDay: function() {
			vm.selectedDate(vm.selectedDate().clone().add('days', -1));
		}		
	};
})(jQuery)
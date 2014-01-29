Teleopti.MyTimeWeb.MyReport = (function() {
    var vm;
    
	function MyReportViewModel(loadDataMethod) {
	    var self = this;
	    var yesterDayFromNow = moment(new Date(new Date().getTeleoptiTime())).add('days', -1).startOf('day');
	    self.fillDataMethod = loadDataMethod;
		self.adherence = ko.observable();
		self.answeredCalls = ko.observable();
		self.averageAfterCallWork = ko.observable();
		self.averageHandlingTime = ko.observable();
		self.averageTalkTime = ko.observable();
		self.readyTimePerScheduledReadyTime = ko.observable();
		self.nextWeekDate = ko.observable(moment());
		self.previousWeekDate = ko.observable(moment());
		self.selectedDateInternal = ko.observable(yesterDayFromNow);
		self.weekStart = ko.observable(1);
		self.dataAvailable = ko.observable();
		self.selectedDate = ko.computed({
		    read: function () {
		        return self.selectedDateInternal();
		    },
		    write: function (value) {
		        self.selectedDateInternal(value);
		        self.fillDataMethod(self.selectedDateInternal());
		    }
		});
	}
    
	function fillData(date) {
		$.ajax({
			url: 'MyTime/MyReport/OnDates',
			dataType: 'json',
			data: { date: date.toDate().toJSON() },
			success: function(data) {
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
                vm.weekStart(data.WeekStart);
            }
        });
    };
    
    function bindData() {
        vm = new MyReportViewModel(fillData);
        var elementToBind = $('.myreport-daily-metrics')[0];
        ko.applyBindings(vm, elementToBind);
    }

	return {
		Init: function() {
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
		yesterday: function() {
			vm.selectedDateInternal(moment().startOf('day').clone().add('days', -1));
		},
		nextDay: function() {
		    vm.selectedDateInternal(vm.selectedDate().clone().add('days', 1));
		},
		previousDay: function() {
		    vm.selectedDateInternal(vm.selectedDate().clone().add('days', -1));
		}
	};
})(jQuery);
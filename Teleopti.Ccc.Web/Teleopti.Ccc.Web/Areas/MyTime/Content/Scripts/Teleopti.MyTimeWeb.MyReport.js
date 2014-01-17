//MyReport = (function() {
	
	function MyReportViewModel() {
		var self = this;

		self.Adherence = ko.observable();
		self.AnsweredCalls = ko.observable();
		self.AverageAfterWork = ko.observable();
		self.AverageHandlingTime = ko.observable();
		self.AverageTalkTime = ko.observable();
		self.Readiness = ko.observable();
		$.ajax({
			url: 'MyTime/MyReport/OnDates',
			dataType: 'json',
			success: function(data) {
				self.Adherence(data.Adherence);
				self.AnsweredCalls(data.AnsweredCalls);
				self.AverageAfterWork(data.AverageAfterWork);
				self.AverageHandlingTime(data.AverageHandlingTime);
				self.AverageTalkTime(data.AverageTalkTime);
				self.Readiness(data.Readiness);
			},
			error: function(xhr, ajaxOptions, thrownError) {
				alert(xhr.status);
				alert(xhr.responseText);
				alert(thrownError);
			}
		});
	}

	
//})(jQuery)
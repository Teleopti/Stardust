define([
		'knockout',
		'ajax',
		'views/manageadherence/getpersondetails',
		'views/realtimeadherenceagents/getadherence'
],
	function(
		ko,
		ajax,
		getpersondetails,
		getadherence
	) {
		return function() {

			var that = {};
			that.PersonId = ko.observable();
			that.AgentName = ko.observable();
			that.DailyPercent = ko.observable();

			that.setViewOptions = function(options) {
				that.PersonId(options.id);
			}

			that.load = function() {
				getpersondetails.ServerCall(function (data) { that.AgentName(data.Name); }, that.PersonId());
				getadherence.ServerCall(function (data) { that.DailyPercent(data.AdherencePercent + "%"); }, that.PersonId());
				
			}

			return that;
		};
	}
);
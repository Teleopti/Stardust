﻿define([
		'knockout',
		'ajax',
		'views/manageadherence/getpersondetails',
		'views/manageadherence/getadherencedetails',
		'views/realtimeadherenceagents/getadherence'
],
	function(
		ko,
		ajax,
		getpersondetails,
		getadherencedetails,
		getadherence
	) {
		return function() {

			var that = {};
			that.PersonId = ko.observable();
			that.AgentName = ko.observable();
			that.DailyPercent = ko.observable();
			that.AdherenceDetails = ko.observableArray();

			that.setViewOptions = function(options) {
				that.PersonId(options.id);
			}

			that.load = function() {
				getpersondetails.ServerCall(function (data) { that.AgentName(data.Name); }, that.PersonId());
				getadherence.ServerCall(function (data) { that.DailyPercent(data.AdherencePercent + "%"); }, that.PersonId());
				getadherencedetails.ServerCall(function (data) { that.AdherenceDetails(data); }, that.PersonId());
			}

			return that;
		};
	}
);
﻿define([
		'knockout',
		'navigation'
],
	function (ko, navigation) {
		return function () {

			var that = {};
			that.OutOfAdherence = ko.observable();
			that.hasBeenUpdated = ko.observable(false);
			that.canOpenTeam = ko.observable(false);

			that.fill = function (data) {
				that.Id = data.Id;
				that.Name = data.Name;
				that.NumberOfAgents = data.NumberOfAgents;
				that.BusinessUnitId = data.BusinessUnitId;

			};

			// needs id from the vm
			that.openTeam = function () {
				if (that.canOpenTeam()) {
					navigation.GotoRealTimeAdherenceTeamDetails(that.BusinessUnitId, that.Id);
				}
			};

			return that;
		};
	}
);
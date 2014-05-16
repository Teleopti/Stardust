define([
		'knockout',
		'navigation'
],
	function (ko, navigation) {
		return function () {

			var that = {};
			that.OutOfAdherence = ko.observable();
			that.hasBeenUpdated = ko.observable(false);

			that.fill = function (data) {
				that.Id = data.Id;
				that.Name = data.Name;
				that.NumberOfAgents = data.NumberOfAgents;
			};

			// needs id from the vm
			that.openTeam = function () {
				navigation.GotoRealTimeAdherenceTeamDetails(that.Id);
			};

			return that;
		};
	}
);
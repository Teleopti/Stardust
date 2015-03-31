define([
		'knockout',
		'lazy',
		'views/realtimeadherenceteams/team',
		'resources',
		'navigation',
		'amplify'
],
	function (ko,
		lazy,
		team,
		resources,
		navigation,
		amplify) {
		return function () {

			var that = {};
			that.resources = resources;
			that.teams = ko.observableArray();
			that.siteName = ko.observable();
			that.BusinessUnitId = ko.observable();
			that.rootURI = ko.observable();
			that.fill = function (data) {
				for (var i = 0; i < data.length; i++) {
					var newTeam = team();
					data[i].BusinessUnitId = that.BusinessUnitId();
					newTeam.fill(data[i]);

					that.teams.push(newTeam);
				}
			};

			that.teamsToOpen = ko.observableArray();
			that.SetViewOptions = function(options) {
				that.BusinessUnitId(options.buid);
				that.rootURI('#realtimeadherencesites/' + that.BusinessUnitId());
			};
			var teamForId = function (id) {
				if (!id)
					return undefined;
				var theTeam = lazy(that.teams())
					.filter(function (x) { return x.Id == id; })
					.first();
				return theTeam;
			};

			that.update = function (data) {
				var existingTeam = teamForId(data.Id);
				if (existingTeam) {
					existingTeam.OutOfAdherence(data.OutOfAdherence);
					existingTeam.hasBeenUpdated(true);
				}
			};

			that.updateFromNotification = function (notification) {
				var data = JSON.parse(notification.BinaryData);
				data.Id = notification.DomainId;
				that.update(data);
			};

			that.setSiteName = function (data) {
				that.siteName(data.Name);
			};

			that.openSelectedTeams = function () {
				if (that.teamsToOpen().length === 0) {
					that.teamsToOpen(lazy(that.teams())
						.pluck("Id")
						.toArray());
				}
				amplify.store("MultipleTeams", that.teamsToOpen());
				navigation.GotoRealTimeAdherenceMultipleTeamDetails(that.BusinessUnitId());
			}
			
			return that;
		};
	}
);
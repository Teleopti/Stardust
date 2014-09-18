define([
		'knockout',
		'business_unit',
		'ajax',
		'navigation',
		'lazy'
], function(ko,
		businessUnit,
		ajax,
		navigation,
		lazy) {

		return function(resources) {
			var self = this;

			self.Resources = resources;
			self.MyTimeVisible = ko.observable(false);
			self.RealTimeAdherenceVisible = ko.observable(false);
			self.ActiveView = ko.observable("");
			self.UserName = ko.observable("");
			self.CurrentBusinessUnitId = ko.observable();
			self.CurrentDate = ko.observable();
			self.CurrentGroupId = ko.observable();
			self.IanaTimeZone = ko.observable("");
			self.changeScheduleForMultipleBUs = ko.observable(false);
			self.businessUnits = ko.observableArray();

			var businessUnitForId = function (id) {
				if (!id)
					return undefined;
				var bu = lazy(self.businessUnits())
					.filter(function (x) { return x.Id == id; })
					.first();
				return bu;
			};

			self.CurrentBusinessUnit = ko.computed(function () {
				return businessUnitForId(self.CurrentBusinessUnitId());
			});

			self.fillBusinessUnits = function (data) {
				for (var i = 0; i < data.length; i++) {
					var newBU = businessUnit();
					newBU.fill(data[i]);
					self.businessUnits.push(newBU);
				}
			};

			self.switchBusinessUnit = function(data) {
				var view = self.ActiveView();
				var buId = data.Id;
				if (view.indexOf("realtimeadherence") > -1)
					navigation.GotoRealTimeAdherenceViewOriginal(buId);
				else {
					var date = self.CurrentDate();
					if (date)
						navigation.GoToTeamScheduleForDate(buId, date);
					else navigation.GoToTeamScheduleOriginal(buId);
				}
			};

			self.navigateSchedules = function() {
				navigation.GoToTeamScheduleOriginal(self.CurrentBusinessUnitId());
			};
			self.navigateRealtimeAdherence = function () {
				navigation.GotoRealTimeAdherenceViewOriginal(self.CurrentBusinessUnitId());
			};
		};
	});

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
			self.ReportsVisible = ko.observable(false);
			self.TeamScheduleVisible = ko.observable(false);
			self.ActiveView = ko.observable("");
			self.UserName = ko.observable("");
			self.CurrentBusinessUnitId = ko.observable();
			self.CurrentDate = ko.observable();
			self.CurrentGroupId = ko.observable();
			self.IanaTimeZone = ko.observable("");
			self.businessUnits = ko.observableArray();
			self.reports = ko.observableArray();

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

			self.fillReports = function (data) {
				for (var i = 0; i < data.length; i++) {
					self.reports.push(data[i]);
				}
			}

			self.urlForHome = function() {
				return navigation.UrlForHome(self.CurrentBusinessUnitId(), self.RealTimeAdherenceVisible(), self.TeamScheduleVisible());
			};
			self.urlForTeamScheduleToday = function() {
				return navigation.UrlForTeamScheduleToday(self.CurrentBusinessUnitId());
			}
			self.urlForRealTimeAdherence = function() {
				return navigation.UrlForRealTimeAdherence(self.CurrentBusinessUnitId());
			};

			self.urlForChangingBusinessUnitId = function(data) {
				var view = self.ActiveView();
				var buId = data.Id;
				if (view.indexOf("realtimeadherence") > -1)
					return navigation.UrlForRealTimeAdherence(buId);
				else {
					var date = self.CurrentDate();
					if (date)
						return navigation.UrlForTeamScheduleForDate(buId, date);
					else return navigation.UrlForHome(buId, self.RealTimeAdherenceVisible(), self.TeamScheduleVisible());
				}
			};
		};
});


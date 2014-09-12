define([
		'knockout',
		'toggleQuerier',
		'business_unit',
		'ajax'
], function(ko,
		toggleQuerier,
		businessUnit,
		ajax) {

		return function(resources) {
			var self = this;

			self.Resources = resources;
			self.MyTimeVisible = ko.observable(false);
			self.RealTimeAdherenceVisible = ko.observable(false);
			self.ActiveView = ko.observable("");
			self.UserName = ko.observable("");
			self.IanaTimeZone = ko.observable("");
			self.CurrentBusinessUnit = ko.observable();
			self.changeScheduleForMultipleBUs = ko.observable(false);
			self.businessUnits = ko.observableArray();


			self.fillBusinessUnits = function (data) {
				for (var i = 0; i < data.length; i++) {
					var newBU = businessUnit();
					newBU.fill(data[i]);
					self.businessUnits.push(newBU);
				}
			};

			self.switchBusinessUnit = function(data) {
				
			}
			
			var checkBusinessUnitsFeature = function () {
				toggleQuerier('RTA_ChangeScheduleInAgentStateView_29934', {
					enabled: function () {
						ajax.ajax({
							url: "BusinessUnit",
							success: function (data) {
								self.changeScheduleForMultipleBUs(true);
								self.fillBusinessUnits(data);
								self.CurrentBusinessUnit(data[0].Name);
							}
						});
					}
				});
			};

			checkBusinessUnitsFeature();
		};
	});

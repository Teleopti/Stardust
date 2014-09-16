define([
		'knockout',
		'toggleQuerier',
		'business_unit',
		'ajax',
		'navigation'
], function(ko,
		toggleQuerier,
		businessUnit,
		ajax,
		navigation) {

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

			self.setCurrentBusinessUnit = function(data) {
				self.CurrentBusinessUnit(data);
			}

			self.switchBusinessUnit = function(data) {
				self.setCurrentBusinessUnit(data);
				navigation.GoToTeamScheduleOritinal(data.Id);
			}
			
			var checkBusinessUnitsFeature = function () {
				toggleQuerier('RTA_ChangeScheduleInAgentStateView_29934', {
					enabled: function () {
						ajax.ajax({
							url: "BusinessUnit",
							success: function (data) {
								self.fillBusinessUnits(data);
								self.setCurrentBusinessUnit(data[0]);
								self.changeScheduleForMultipleBUs(true);
							}
						});
					}
				});
			};

			checkBusinessUnitsFeature();
		};
	});

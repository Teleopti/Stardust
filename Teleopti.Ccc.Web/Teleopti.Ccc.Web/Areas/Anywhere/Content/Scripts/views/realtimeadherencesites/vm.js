define([
		'knockout',
		'lazy',
		'views/realtimeadherencesites/site',
		'views/realtimeadherencesites/business_unit',
		'resources',
		'amplify',
		'navigation',
		'subscriptions.adherencesites',
		'ajax',
		'toggleQuerier'
],
	function (
		ko,
		lazy,
		site,
		businessUnit,
		resources,
		amplify,
		navigation,
		subscriptions,
		ajax,
		toggleQuerier
	) {
		return function () {

			var that = {};

			that.resources = resources;

			that.sites = ko.observableArray();
			that.businessUnits = ko.observableArray();
			that.sitesToOpen = ko.observableArray();
			that.agentStatesForMultipleSites = ko.observable();
			that.monitorMultipleBusinessUnits = ko.observable();
			var siteForId = function (id) {
				if (!id)
					return undefined;
				var theSite = lazy(that.sites())
					.filter(function (x) { return x.Id == id; })
					.first();
				return theSite;
			};

			that.fill = function (data) {
				that.sites([]);
				for (var i = 0; i < data.length; i++) {
					var newSite = site();
					newSite.fill(data[i]);
					that.sites.push(newSite);
				}
			};

			that.fillBusinessUnits = function (data) {
				for (var i = 0; i < data.length; i++) {
					var newBU = businessUnit();
					newBU.fill(data[i]);
					that.businessUnits.push(newBU);
				}
			};
			
			that.updateFromNotification = function (notification) {
				
				var data = JSON.parse(notification.BinaryData);
				data.Id = notification.DomainId;
				that.update(data);
			};

			that.update = function (data) {
				var existingSite = siteForId(data.Id);
				if (existingSite) {
					existingSite.OutOfAdherence(data.OutOfAdherence);
					existingSite.hasBeenUpdated(true);
				}
			};

			that.openSelectedSites = function() {
				amplify.store("MultipleSites", that.sitesToOpen());
				navigation.GotoRealTimeAdherenceMultipleSiteDetails('MultipleSites');
			};

			that.businessUnitChanged = function(data, event) {
				that.load(event.target.options[event.target.selectedIndex].value);
			};

			that.load = function (businessUnitId) {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': businessUnitId ? businessUnitId : '' },
					url: "Sites",
					success: function(data) {
						that.fill(data);
						checkFeature();
						checkAgentsForMultipleTeamsFeature();
						if (that.businessUnits().length == 0)
							checkBusinessUnitsFeature();
					}
				});
				subscriptions.unsubscribeAdherence();
				subscriptions.subscribeAdherence(function (notification) {
					that.updateFromNotification(notification);
				}, businessUnitId, function () {
					$('.realtimeadherencesites').attr("data-subscription-done", " ");
				});
			};


			var checkFeature = function() {
				toggleQuerier('RTA_RtaLastStatesOverview_27789', { enabled: loadLastStates });
			};


			var loadLastStates = function () {
				for (var i = 0; i < that.sites().length; i++) {
					(function (s) {
						ajax.ajax({
							url: "Sites/GetOutOfAdherence?siteId=" + s.Id,
							success: function (d) {
								that.update(d);
							}
						});
					})(that.sites()[i]);
				}
			};

			var checkAgentsForMultipleTeamsFeature = function() {
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', { enabled: function() { that.agentStatesForMultipleSites(true); } });
			};

			var checkBusinessUnitsFeature = function() {
				toggleQuerier('RTA_MonitorMultipleBusinessUnits_28348', {
					enabled: function() {
						ajax.ajax({
							url: "BusinessUnit",
							success: function (data) {
								that.monitorMultipleBusinessUnits(true);
								that.fillBusinessUnits(data);
							}
						});
					}
				});
			};

			return that;
		};
	}
);
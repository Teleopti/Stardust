define([
		'knockout',
		'lazy',
		'views/realtimeadherencesites/site',
		'resources',
		'amplify',
		'navigation',
		'ajax',
		'views/realtimeadherencesites/subscriptions.adherencesites'
],
	function (
		ko,
		lazy,
		site,
		resources,
		amplify,
		navigation,
		ajax,
		subscriptions
	) {
		return function () {

			var that = {};

			that.resources = resources;
			that.sites = ko.observableArray();
			that.sitesToOpen = ko.observableArray();
			that.BusinessUnitId = ko.observable();
			that.Wfm_RTA_ProperReleaseToggle_36750 = ko.observable();

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
					data[i].BusinessUnitId = that.BusinessUnitId();
					newSite.fill(data[i]);
					that.sites.push(newSite);
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

			that.openSelectedSites = function () {
				if (that.sitesToOpen().length === 0) {
					that.sitesToOpen(lazy(that.sites())
						.pluck("Id")
						.toArray());
				}
				amplify.store("MultipleSites", that.sitesToOpen());
				navigation.GotoRealTimeAdherenceMultipleSiteDetails(that.BusinessUnitId());
			};

			that.businessUnitChanged = function(data, event) {
				that.load(event.target.options[event.target.selectedIndex].value);
			};

			that.load = function () {
				ajax.ajax({
					headers: { 'X-Business-Unit-Filter': that.BusinessUnitId() },
					url: "api/Sites",
					success: function (data) {
						that.fill(data);
					}
				});
				subscriptions.unsubscribeAdherence();
				subscriptions.subscribeAdherence(function (notification) {
					that.updateFromNotification(notification);
				}, that.BusinessUnitId(), function () {
					$('.realtimeadherencesites').attr("data-subscription-done", " ");
				});
			};

			return that;
		};
	}
);
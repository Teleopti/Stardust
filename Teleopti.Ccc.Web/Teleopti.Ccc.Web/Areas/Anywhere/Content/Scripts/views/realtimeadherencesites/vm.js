define([
		'knockout',
		'lazy',
		'views/realtimeadherencesites/site',
		'resources'
],
	function (
		ko,
		lazy,
		site,
		resources
	) {
		return function () {

			var that = {};

			that.resources = resources;

			that.sites = ko.observableArray();

			var siteForId = function (id) {
				if (!id)
					return undefined;
				var theSite = lazy(that.sites())
					.filter(function (x) { return x.Id == id; })
					.first();
				return theSite;
			};

			that.fill = function (data) {
				for (var i = 0; i < data.length; i++) {
					var newSite = site();
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

			return that;
		};
	}
);
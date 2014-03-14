define([
		'knockout',
		'lazy',
		'views/realtimeadherence/site'
],
	function (
		ko,
		lazy,
		site
	) {
	return function () {


		var that = {};
		
		that.sites = ko.observableArray();

		var siteForId = function (id) {
			if (!id)
				return undefined;
			var theSite = lazy(that.sites())
				.filter(function (x) { return x.Id == id; })
				.first();
			return theSite;
		};

		that.fill = function(data) {
			for (var i = 0; i < data.length; i++) {
				var newSite = site();
				newSite.Id = data[i].Id;
				newSite.Name = data[i].Name;

				that.sites.push(newSite);
			}
		};

		that.update = function (data) {
			var existingSite = siteForId(data.Id);
			if (existingSite)
				existingSite.OutOfAdherence(data.OutOfAdherence);
		};

		return that;
	};
}
);
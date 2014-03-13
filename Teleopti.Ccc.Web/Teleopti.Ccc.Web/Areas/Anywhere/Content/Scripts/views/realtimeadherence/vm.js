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
			if (!theSite) {
				theSite = site();
				theSite.Id = id;
				that.sites.push(theSite);
			}
			return theSite;
		};

		that.fill = function (data) {
			for (var i = 0; i < data.length; i++) {
				var theSite = siteForId(data[i].Id);
				theSite.Name = data[i].Name;
			}
		};

		that.update = function (data) {
			siteForId(data.Id).OutOfAdherence(data.OutOfAdherence);
		};

		return that;
	};
}
);
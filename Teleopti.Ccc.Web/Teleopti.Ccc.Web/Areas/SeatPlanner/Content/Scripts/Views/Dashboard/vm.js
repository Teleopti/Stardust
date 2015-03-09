define([
	'knockout',
	'jquery',
	'navigation',
	'lazy',
	'resources'
], function (
	ko,
	$,
	navigation,
	lazy,
	resources
	
	) {

	return function () {
		var self = this;
		var businessUnitId;
		this.Resources = resources;
		this.Loading = ko.observable(false);
		this.SetViewOptions = function (options) {
			businessUnitId = options.buid;
		};


		self.urlForSeatPlanner = function () {
			return navigation.UrlForSeatPlanner();
		};

		self.urlForSeatMap = function () {
			return navigation.UrlForSeatMap();
		}

	};
});

define([
		'knockout'
	], function (
		ko
		) {

		return function (name, layers) {

			this.Name = ko.observable(name);
			this.Layers = layers;

		};
	});

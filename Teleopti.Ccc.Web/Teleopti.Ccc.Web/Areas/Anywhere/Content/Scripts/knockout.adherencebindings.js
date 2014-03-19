define([
	'knockout',
	'gage'
], function(
	ko,
	gage
) {
	ko.bindingHandlers.gage = {
		init: function(element, valueAccessor, allBindingsAccessor) {
			var options = valueAccessor();
			var id = ko.utils.unwrapObservable(options.id);
			element.id = id;

			var gageObject = new gage({
				id: id,
				value: '?',
				min: 0,
				max: ko.utils.unwrapObservable(options.max),
				title: ko.utils.unwrapObservable(options.title),
				label: 'xxOut of adherence',
				relativeGaugeSize: true
			});
			element.gage = gageObject;
		},
		update: function(element, valueAccessor) {
		}
	};
});
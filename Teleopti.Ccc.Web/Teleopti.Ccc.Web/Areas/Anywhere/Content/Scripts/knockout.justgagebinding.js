define([
	'knockout',
	'justgage'
], function(
	ko,
	justgage
) {
	ko.bindingHandlers.justgage = {
		init: function(element, valueAccessor, allBindingsAccessor) {
			var options = valueAccessor();
			var id = ko.utils.unwrapObservable(options.id);
			element.id = id;
			var max = ko.utils.unwrapObservable(options.max);

			var gageObject = new justgage({
				id: id,
				value: 0,
				min: 0,
				max: max,
				title: ko.utils.unwrapObservable(options.title),
				label: ko.utils.unwrapObservable(options.label),
				relativeGaugeSize: true
			});
			element.gage = gageObject;
		},
		update: function (element, valueAccessor) {
			var options = valueAccessor();
			var value = ko.utils.unwrapObservable(options.value);
			if (value) {
				element.gage.refresh(value);
			}
		}
	};
});
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
			var max = ko.utils.unwrapObservable(options.max);
			if (max == 0) {
				//hack if no agents in team/site!
				max = -1;
			}

			var gageObject = new gage({
				id: id,
				value: 'xxx?',
				min: 0,
				max: max,
				title: ko.utils.unwrapObservable(options.title),
				label: 'xxOut of adherence',
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
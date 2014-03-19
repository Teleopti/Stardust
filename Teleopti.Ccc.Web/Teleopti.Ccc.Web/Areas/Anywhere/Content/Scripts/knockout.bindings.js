define([
	'knockout',
	'gage'
], function(
	ko,
	gage
) {
	ko.bindingHandlers.select2 = {
		init: function(element, valueAccessor) {
			var options = valueAccessor();
			options['width'] = 'resolve';

			var observable = options.value;
			// kinda strange, but we have to use the original select's event because select2 doesnt provide its own events
			$(element).on('change', function() {
				observable($(element).val());
			});
			$(element).select2(options);

			ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
				$(element).select2('destroy');
			});
		},
		update: function(element, valueAccessor) {
			var observable = valueAccessor().value;
			$(element).select2("val", observable());
		}
	};

	ko.bindingHandlers.timepicker = {
		init: function(element, valueAccessor, allBindingsAccessor) {
			var options = allBindingsAccessor().timepickerOptions || {};
			var $element = $(element);
			$element.timepicker(options);

			$element.on('change', function() {
				var observable = valueAccessor();
				var value = $element.val();
				value = value == '' ? undefined : value;
				observable(value);
			});
		},
		update: function(element, valueAccessor) {
			var $element = $(element);

			var value = ko.utils.unwrapObservable(valueAccessor());
			if (value) {
				$element.timepicker("setTime", value);
			} else {
				$element.val(value);
			}
		}
	};
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
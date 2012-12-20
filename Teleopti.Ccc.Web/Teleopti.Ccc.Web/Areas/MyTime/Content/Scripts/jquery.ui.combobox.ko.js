/*

data-bind="combobox: StartTimeMinimum"

alternative to using:

jqueryui: {
	widget: 'combobox',
	options: {
		value: StartTimeMinimum(),
		changed: function(event, ui) { StartTimeMinimum(ui.value); }
	}
}">

*/

(function ($) {

	ko.bindingHandlers.combobox = {
		init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
			var observable = valueAccessor();
			$(element).combobox({
				value: ko.utils.unwrapObservable(observable),
				changed: function (event, ui) {
					observable(ui.value);
				}
			});
		},
		update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
			var observable = valueAccessor();
			$(element).combobox({
				value: ko.utils.unwrapObservable(observable)
			});
		}
	};
	
	ko.bindingHandlers['combobox-enabled'] = {
		update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
			var observable = valueAccessor();
			$(element).combobox({
				enabled: ko.utils.unwrapObservable(observable)
			});
		}
	};

})(jQuery);

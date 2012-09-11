/*

data-bind="selectbox: PreferenceId"

alternative to using:

jqueryui: {
	widget: 'selectbox',
	options: {
		value: PreferenceId(),
		changed: function(event, ui) { PreferenceId(ui.item.value); }
	}
}">

*/

(function ($) {
	
	ko.bindingHandlers['selectbox'] = {
		update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
			var observable = valueAccessor();
			$(element).selectbox({
				value: ko.utils.unwrapObservable(observable),
				changed: function(event, ui) {
					observable(ui.item.value);
				}
			});
		}
	};

	ko.bindingHandlers['selectbox-enabled'] = {
		update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
			var observable = valueAccessor();
			$(element).selectbox({
				enabled: ko.utils.unwrapObservable(observable)
			});
		}
	};
	
})(jQuery);

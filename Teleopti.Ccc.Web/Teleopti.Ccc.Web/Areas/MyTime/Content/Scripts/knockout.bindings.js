ko.bindingHandlers['option-data'] = {
	update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var options = valueAccessor();
		var observable = options.member;
		var selected = $(element).find('option:selected');
		var data = selected.data(options.data);
		observable(data);
	}
};

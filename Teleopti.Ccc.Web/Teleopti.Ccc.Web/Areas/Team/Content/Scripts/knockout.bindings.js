
define([
		'knockout'
	], function (
		ko
	) {
		ko.bindingHandlers.datepicker = {
			init: function (element, valueAccessor, allBindingsAccessor) {
				//initialize datepicker with some optional options
				var options = allBindingsAccessor().datepickerOptions || {};
				$(element).datepicker(options);

				//when a user changes the date, update the view model
				ko.utils.registerEventHandler(element, "changeDate", function (event) {
					var value = valueAccessor();
					if (ko.isObservable(value)) {
						value(event.date);
					}
				});

				ko.utils.registerEventHandler(element, "change", function () {
					var value = valueAccessor();
					if (ko.isObservable(value)) {
						value(new Date(element.value));
					}
				});
			},
			update: function (element, valueAccessor) {
				var widget = $(element).data("datepicker");
				//when the view model is updated, update the widget
				if (widget) {
					widget.date = ko.utils.unwrapObservable(valueAccessor());

					if (!widget.date) {
						return;
					}

					if (typeof(widget.date) === "string") {
						widget.date = new Date(widget.date);
					}

					widget.setValue();
				}
			}
		};
	});
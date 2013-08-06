﻿ko.bindingHandlers['option-data'] = {
	update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var options = valueAccessor();
		var observable = options.member;
		var selected = $(element).find('option:selected');
		var data = selected.data(options.data);
		if (observable)
			observable(data);
	}
};

ko.bindingHandlers.animateBackground = {
	update: function (element, valueAccessor, allBindingsAccessor, viewmodel) {
		var value = valueAccessor(), allBindings = allBindingsAccessor();
		var fadeDuration = allBindings.fadeDuration || 1500;
		var valueUnwrapped = ko.utils.unwrapObservable(value);
		$(element).stop().animate({ backgroundColor: valueUnwrapped }, fadeDuration);
	}
};

ko.bindingHandlers['class'] = {
	update: function (element, valueAccessor) {
		if (element['__ko__previousClassValue__']) {
			$(element).removeClass(element['__ko__previousClassValue__']);
		}
		var value = ko.utils.unwrapObservable(valueAccessor());
		$(element).addClass(value);
		element['__ko__previousClassValue__'] = value;
	}
};

ko.bindingHandlers.hoverToggle = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		var css = valueAccessor();
		var targetElements = [element];
		if (allBindingsAccessor().hoverTarget) {
			targetElements = $(allBindingsAccessor().hoverTarget);
		}

		ko.utils.registerEventHandler(element, "mouseover", function () {
			var hoverIf = allBindingsAccessor().hoverIf;
			if (hoverIf === undefined) {
				hoverIf = true;
			}
			if (hoverIf) {
				$.each(targetElements, function (index, value) {
					ko.utils.toggleDomNodeCssClass(value, ko.utils.unwrapObservable(css), true);
				});
			}
		});

		ko.utils.registerEventHandler(element, "mouseleave", function () {
			$.each(targetElements, function (index, value) {
				ko.utils.toggleDomNodeCssClass(value, ko.utils.unwrapObservable(css), false);
			});
		});
	}
};

ko.bindingHandlers.fadeInIf = {
	update: function (element, valueAccessor, allBindingsAccessor) {
		var value = valueAccessor(), allBindings = allBindingsAccessor();

		var valueUnwrapped = ko.utils.unwrapObservable(value);

		var fadeInOpacity = allBindings.fadeInOpacity || 1.0;
		var fadeOutOpacity = allBindings.fadeOutOpacity || 0.1;
		var fadeInDuration = allBindings.fadeInDuration || 300;
		var fadeOutDuration = allBindings.fadeOutDuration || 300;
		var hideWhenFadedOut = allBindings.hideWhenFadedOut || false;

		$(element).stop();

		if (valueUnwrapped) {
			if (hideWhenFadedOut) {
				$(element).show();
			}
			$(element).animate({ opacity: fadeInOpacity }, fadeInDuration);
		} else {
			$(element).animate({ opacity: fadeOutOpacity }, fadeOutDuration, function () {
				if (hideWhenFadedOut) {
					$(element).hide();
				}
			});
		}
	}
};

//Increases the elements width (default by 20px) if the bound value is true
ko.bindingHandlers.increaseWidthIf = {
	update: function (element, valueAccessor, allBindingsAccessor) {
		var value = valueAccessor(), allBindings = allBindingsAccessor();
		if (!element.initialWidthForIncreaseIfBinding) {
			element.initialWidthForIncreaseIfBinding = $(element).width();
		}

		var valueUnwrapped = ko.utils.unwrapObservable(value);

		var increaseBy = allBindings.increaseBy || 20;
		var increaseDuration = allBindings.fadeInDuration || 150;
		var decreaseDuration = allBindings.fadeOutDuration || 150;

		$(element).stop();

		if (valueUnwrapped)
			$(element).animate({ width: element.initialWidthForIncreaseIfBinding + increaseBy }, decreaseDuration);
		else
			$(element).animate({ width: element.initialWidthForIncreaseIfBinding }, increaseDuration);
	}
};

ko.bindingHandlers.timepicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		var options = allBindingsAccessor().timepickerOptions || {};
		$(element).timepicker(options);

		ko.utils.registerEventHandler(element, "changeTime.timepicker", function (e) {
			var observable = valueAccessor();
			observable(e.time.value);
		});
	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		if (typeof value === 'function') return;
		if (value === undefined) value = '';
            
		$(element).timepicker("setTime", value);
	}
};

//Sets the tooltip (using bootstrapper) to the bound text
ko.bindingHandlers.tooltip = {
	update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
		var $element, options, tooltip;
		options = ko.utils.unwrapObservable(valueAccessor());
		$element = $(element);
		tooltip = $element.data('tooltip');
		if (tooltip) {
			$.extend(tooltip.options, options);
		} else {
			$element.tooltip(options);
		}
	}
};

ko.bindingHandlers.select2 = {
	init: function (element, valueAccessor) {
		var options = valueAccessor();
        options['escapeMarkup'] = function (m) { return m; };
        options['width'] = 'resolve';
        
		var observable = options.value;
		// kinda strange, but we have to use the original select's event because select2 doesnt provide its own events
		$(element).on('change', function () {
			observable($(element).val());
		});
		$(element).select2(options);

		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$(element).select2('destroy');
		});
	},
	update: function (element, valueAccessor) {
		var observable = valueAccessor().value;
		$(element).select2("val", observable());
	}
};

ko.bindingHandlers.button = {
	init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		$(element).button();
	},
	update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var value = ko.utils.unwrapObservable(valueAccessor()),
            disabled = ko.utils.unwrapObservable(value.disabled);

		$(element).button("option", "disabled", disabled);
	}
};

//wraps the datepickerbinding and sets the datepickeroptions
ko.bindingHandlers.mytimeDatePicker = {
	init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var isDisabled = $(element).attr('disabled') == 'disabled';
		$(element).attr('disabled', 'disabled');

		var ajax = new Teleopti.MyTimeWeb.Ajax();
		ajax.Ajax({
			url: 'UserInfo/Culture',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				allBindingsAccessor().datepickerOptions = { autoHide: true, weekStart: data.WeekStart };
				ko.bindingHandlers.datepicker.init(element, valueAccessor, allBindingsAccessor, viewModel);

				if (!isDisabled) $(element).removeAttr('disabled');
			}
		});
	}
};

ko.bindingHandlers.clickable = {
	update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var clickableCursor = (value) ? "pointer" : "not-allowed";
		$(element).css('cursor', clickableCursor);

	}
};

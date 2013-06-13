ko.bindingHandlers['option-data'] = {
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
		$(element).stop().animate({ backgroundColor: valueUnwrapped}, fadeDuration);
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

ko.bindingHandlers.datepicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		//initialize datepicker with some optional options
		var options = allBindingsAccessor().datepickerOptions || { showAnim: 'slideDown' };
		$(element).datepicker(options);

		//handle the field changing
		ko.utils.registerEventHandler(element, "change", function () {
			var observable = valueAccessor();
			observable(moment($(element).datepicker("getDate")));
			$(element).blur();
		});

		//handle the field keydown for enter key
		ko.utils.registerEventHandler(element, "keydown", function (key) {
			if (key.keyCode == 13) {
				var observable = valueAccessor();
				observable(moment($(element).datepicker("getDate")));
				$(element).blur();
			}
		});

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$(element).datepicker("destroy");
		});

	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		$(element).datepicker("setDate", new Date(value));
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
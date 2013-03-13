ko.bindingHandlers['option-data'] = {
	update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
		var options = valueAccessor();
		var observable = options.member;
		var selected = $(element).find('option:selected');
		var data = selected.data(options.data);
		observable(data);
	}
};

ko.bindingHandlers.percentageColor = {
    update: function(element, valueAccessor, allBindingsAccessor, viewmodel) {
        var value = valueAccessor(), allBindings = allBindingsAccessor();
        var okPercentage = allBindings.okPercentage || 70;
        var notOkPercentage = allBindings.notOkPercentage || 30;
        var fadeDuration = allBindings.fadeDuration || 800;
        var valueUnwrapped = ko.utils.unwrapObservable(value);

        var colorToSet = 'yellow';
        if (valueUnwrapped < notOkPercentage) colorToSet = 'red';
        else if (valueUnwrapped > okPercentage) colorToSet = 'green';

        $(element).stop().animate({ backgroundColor: colorToSet }, fadeDuration);

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


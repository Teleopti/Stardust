﻿//eventaggregator
//usage (subscribing):
//	ko.eventAggregator.subscribe(function(value){
//		doSomething with value
//		,null
//		,"myTopic"
//	});
//
//usage (publishing)
//ko.eventAggregator.notifySubscribers(value, "mytopic");
ko.eventAggregator = new ko.subscribable();

//eventAggregatorExtensions
ko.subscribable.fn.publishOn = function (topic) {
	this.subscribe(function (newValue) {
		ko.eventAggregator(newValue, topic);
	});
	return this; //support chaining
};

ko.subscribable.fn.subscribeTo = function (topic) {
	ko.eventAggregator.subscribe(this, null, topic);
	return this; //support chaining
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

ko.bindingHandlers.timepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
	    var options = allBindingsAccessor().timepickerOptions || {};
	    var $element = $(element);
	    $element.timepicker(options);

	    $element.on('change', function () {
	        var observable = valueAccessor();
	        var value = $element.val();
	        value = value == '' ? undefined : value;
	        observable(value);
	    });
	},
	update: function (element, valueAccessor) {
	    var $element = $(element);

	    var value = ko.utils.unwrapObservable(valueAccessor());
	    if (value) {
	        $element.timepicker("setTime", value);
	    } else {
	        $element.val(value);
	    }
	}
};

//Sets the tooltip (using bootstrapper) to the bound text
var TooltipBinding = function() {

    var getOptions = function (valueAccessor) {
        var options = ko.utils.unwrapObservable(valueAccessor());
        options.title = ko.utils.unwrapObservable(options.title);
        return options;
    };

    var trackIsShowing = function($element) {
        var tooltip = $element.data('bs.tooltip');
        tooltip.options.showing = false;
        $element
            .on("hide.bs.tooltip", function () {
                tooltip.showing = false;
            })
            .on("show.bs.tooltip", function () {
                tooltip.showing = true;
            });
    };

    var refreshIfShowing = function ($element) {
        var tooltip = $element.data('bs.tooltip');
        if (tooltip.showing) {
            var preserveAnimation = tooltip.options.animation;
            tooltip.options.animation = false;
            $element.tooltip('hide').tooltip('show');
            tooltip.options.animation = preserveAnimation;
        }
    };
    
    this.init = function(element, valueAccessor, allBindingsAccessor) {
        var $element = $(element);
        $element.tooltip(getOptions(valueAccessor));
        trackIsShowing($element);
    };

    this.update = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var tooltip = $element.data('bs.tooltip');
        $.extend(tooltip.options, getOptions(valueAccessor));
        refreshIfShowing($element);
    };

};
ko.bindingHandlers.tooltip = new TooltipBinding();

ko.bindingHandlers.select2 = {
	init: function (element, valueAccessor) {
		var options = valueAccessor();
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

ko.bindingHandlers.clickable = {
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var clickableCursor = (value) ? "pointer" : "not-allowed";
		$(element).css('cursor', clickableCursor);
	}
};

ko.bindingHandlers.nonEncodedTitle = {
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		var d = document.createElement('div');
		d.innerHTML = value;
		element.title = d.innerText;
	}
};

ko.bindingHandlers.selected = {
    update: function (element, valueAccessor) {
        var selected = ko.utils.unwrapObservable(valueAccessor());
        if (selected) element.select();
    }
};

ko.bindingHandlers.limitCharacters = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        element.value = element.value.substr(0, valueAccessor());
        allBindingsAccessor().value(element.value.substr(0, valueAccessor()));
    }
};

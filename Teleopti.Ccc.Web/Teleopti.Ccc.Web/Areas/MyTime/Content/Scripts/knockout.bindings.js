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
ko.subscribable.fn.publishOn = function(topic) {
	this.subscribe(function(newValue) {
		ko.eventAggregator(newValue, topic);
	});
	return this; //support chaining
};

ko.subscribable.fn.subscribeTo = function(topic) {
	ko.eventAggregator.subscribe(this, null, topic);
	return this; //support chaining
};

ko.bindingHandlers.hoverToggle = {
	init: function(element, valueAccessor, allBindingsAccessor) {
		var css = valueAccessor();
		var targetElements = [element];
		if (allBindingsAccessor().hoverTarget) {
			targetElements = $(allBindingsAccessor().hoverTarget);
		}

		ko.utils.registerEventHandler(element, 'mouseover', function() {
			var hoverIf = allBindingsAccessor().hoverIf;
			if (hoverIf === undefined) {
				hoverIf = true;
			}
			if (hoverIf) {
				$.each(targetElements, function(index, value) {
					ko.utils.toggleDomNodeCssClass(value, ko.utils.unwrapObservable(css), true);
				});
			}
		});

		ko.utils.registerEventHandler(element, 'mouseleave', function() {
			$.each(targetElements, function(index, value) {
				ko.utils.toggleDomNodeCssClass(value, ko.utils.unwrapObservable(css), false);
			});
		});
	}
};

ko.bindingHandlers.fadeInIf = {
	update: function(element, valueAccessor, allBindingsAccessor) {
		var value = valueAccessor(),
			allBindings = allBindingsAccessor();

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
			$(element).animate({ opacity: fadeOutOpacity }, fadeOutDuration, function() {
				if (hideWhenFadedOut) {
					$(element).hide();
				}
			});
		}
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
			$element.timepicker('setTime', value);
		} else {
			$element.val(value);
		}
	}
};

//Sets the tooltip (using bootstrapper) to the bound text
var TooltipBinding = function() {
	this.init = function(element, valueAccessor, allBindingsAccessor) {
		var local = ko.utils.unwrapObservable(valueAccessor()),
			options = {};

		ko.utils.extend(options, ko.bindingHandlers.tooltip.options);
		ko.utils.extend(options, local);

		var ua = navigator.userAgent;
		var mobile = /Mobile/i.test(ua) && /iPhone/i.test(ua);
		var ipad = /ipad/i.test(ua) || (/Android/i.test(ua) && !/Mobile/i.test(ua));
		if (mobile || ipad) {
			options.trigger = 'click';
		}

		$(element).tooltip(options);
		$(element).attr({ 'binding-tooltip': true });
		ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
			$(element).tooltip('destroy');
		});
		$(element).mouseleave(function(event) {
			$(this).tooltip('hide');
		});

		//Rewrite the position of a top placed tooltip on mobile & desktop
		$(element).on('click mouseover', function(event) {
			var tooltipEle = $(this)
				.siblings()
				.filter('.tooltip.top.in')[0];

			if (!tooltipEle) return;

			setTimeout(function() {
				if(!$(event.currentTarget).offset()) return;
				
				var targetWidth = event.currentTarget.offsetWidth;
				var targetHeight = event.currentTarget.offsetHeight;
				var targetOffsetTop = $(event.currentTarget).offset().top;
				var targetOffsetLeft = $(event.currentTarget).offset().left;

				var jqTooltipEle = $(tooltipEle);
				var tooltipWidth = jqTooltipEle.width();
				if (tooltipWidth > 140) tooltipWidth = 140;

				var tooltipHeight = jqTooltipEle.height();
				var tooltipArrowWidth = 10;
				var tooltipArrowHeight = 5;
				var topMarginBetweenTooltipAndTarget = 3;

				var positionLeft = targetOffsetLeft + targetWidth / 2 - tooltipWidth / 2;
				if (positionLeft < 0) {
					positionLeft = 0;
				}
				jqTooltipEle.find('.tooltip-arrow').css({
					'margin-left': -tooltipArrowWidth / 2
				});

				if (positionLeft + tooltipWidth > window.innerWidth) {
					var marginRight = positionLeft + tooltipWidth - window.innerWidth;

					positionLeft = positionLeft - marginRight;
					jqTooltipEle.find('.tooltip-arrow').css({
						left: '50%',
						'margin-left': marginRight - tooltipArrowWidth / 2
					});
				}

				var top = targetOffsetTop - tooltipHeight - tooltipArrowHeight - topMarginBetweenTooltipAndTarget;
				//Handle team schedule tooltip offset top seperately
				if (location.href.indexOf('MyTime#TeamSchedule/NewIndex') > -1 && top < 110) {
					top = 160 - tooltipHeight - tooltipArrowHeight;
				}

				//Use fixed position and append it to body to fix strange issue on Safari(work on others as well)
				jqTooltipEle.css({
					position: 'fixed',
					left: positionLeft,
					top: top,
					width: tooltipWidth
				});
				jqTooltipEle.remove();
				$('body').append(jqTooltipEle);
			}, 0);
		});
	};
	this.update = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
		var local = ko.utils.unwrapObservable(valueAccessor()),
			options = {};

		ko.utils.extend(options, ko.bindingHandlers.tooltip.options);
		ko.utils.extend(options, local);

		if (options.html) {
			//just call $(el).tooltip('fixTitle')
			$(element).tooltip('fixTitle');
		} else {
			$(element)
				.attr('title', options.title)
				.tooltip('fixTitle');
		}
	};
};
ko.bindingHandlers.tooltip = new TooltipBinding();

ko.bindingHandlers.select2 = {
	init: function(element, valueAccessor) {
		var options = valueAccessor();
		options['width'] = 'resolve';

		var observable = options.value;
		// kinda strange, but we have to use the original select's event because select2 doesnt provide its own events
		$(element).on('change', function() {
			observable($(element).val());
		});

		if (options['onOpening']) {
			$(element).on('select2-opening', function(event) {
				var promise = options['onOpening']();
				if (promise) {
					event.preventDefault();
					promise.then(function() {
						$(element).select2('open');
					});
				}
			});
		}

		$(element).select2(options);

		ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
			$(element).select2('destroy');
		});
	},
	update: function(element, valueAccessor) {
		var observable = valueAccessor().value;
		$(element).select2('val', observable());
	}
};

ko.bindingHandlers.clickable = {
	update: function(element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());

		var clickableCursor = value ? 'pointer' : 'not-allowed';
		$(element).css('cursor', clickableCursor);
	}
};

ko.bindingHandlers.nonEncodedTitle = {
	update: function(element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		var d = document.createElement('div');
		d.innerHTML = value;
		element.title = d.innerText;
	}
};

ko.bindingHandlers.selected = {
	update: function(element, valueAccessor) {
		var selected = ko.utils.unwrapObservable(valueAccessor());
		if (selected) element.select();
	}
};

ko.bindingHandlers.scrollIntoViewWhenClick = {
	init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
		$(element).click(function(event) {
			var ele = this;
			if (!navigator.userAgent.match(/iPhone/)) {
				setTimeout(function() {
					$(valueAccessor()).scrollTop($(ele).offset().top);
				}, 500);
			}
		});
	}
};

ko.bindingHandlers.outsideClickCallback = {
	init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
		$('body').unbind('click');
		$('body').on('click', function(event) {
			if (!$.contains(element, event.target)) {
				valueAccessor() && valueAccessor()();
			}
		});
	}
};

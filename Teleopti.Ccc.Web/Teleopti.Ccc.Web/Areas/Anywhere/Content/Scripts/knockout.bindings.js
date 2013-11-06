define(
    [
        'knockout'
    ], function (
        ko
    ) {
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
    });

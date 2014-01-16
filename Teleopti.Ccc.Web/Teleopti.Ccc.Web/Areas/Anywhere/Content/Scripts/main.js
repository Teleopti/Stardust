
require.config({

	paths: {
	    jquery: '../../../../Content/jquery/jquery-1.10.2',
	    lazy: '../../../../Content/lazy/lazy.min',
	    knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-1.1.4.min',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentLanguages: '../../../../Content/moment/moment.all.min',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko-amd',
		select2: '../../../../Content/select2/select2',
		timepicker: '../../../../Content/bootstrap-timepicker/js/bootstrap-timepicker',
	    
		knockoutBindings: 'knockout.bindings',
	    
		noext: '../../../../Content/require/noext',
		signalrrr: 'require/signalrrr',
		resources: 'require/resources',

		templates: '../templates',
		
		text: '../../../../Content/require/text'

	},

	// dependencies that requires loading order
	shim: {
		'jquery': {
			exports: 'jQuery'
		},
		'jqueryui': ['jquery'],
		'lazy': {
		    exports: 'Lazy'
		},
		'bootstrap': ['jquery'],
		
		'knockoutBindings': ['knockout'],
		'select2': ['jquery', 'knockoutBindings'],
		'timepicker': ['bootstrap', 'knockoutBindings'],

		'signalr': ['jquery'],

		'momentLanguages': ['moment'],
		'momentDatepicker': ['momentLanguages'],
		'momentDatepickerKo': ['momentDatepicker'],
		'swipeListener': ['jquery']
	}
});

require([
        'jquery',
        'modernizr',
        'respond',
        'bootstrap',
        'layout'
    ], function() {

    });

window.test = {
    callViewMethodWhenReady: function (viewName, method) {
        
        var args = Array.prototype.slice.call(arguments, 2);
        
        require(['views/' + viewName + '/view'], function (view) {
            
            var callMethodIfReady = function () {
                if (view.ready)
                    view[method].apply(view, args);
                else
                    setTimeout(callMethodIfReady, 20);
            };
            setTimeout(callMethodIfReady, 0);

        });
    }
};


require.config({

    urlArgs: "bust=v3",
    
	paths: {
		jquery: '../../../../Content/Scripts/jquery-1.9.1',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-1.0.1.min',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentLanguages: '../../../../Content/moment/moment.all.min',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko',
		
		noext: '../../../../Content/require/noext',
		sigr: '../../../../Content/require/sigr',

		templates: '../templates',
		
		text: '../../../../Content/require/text'
		
	},

	// dependencies that requires loading order
	shim: {
		'jquery': {
			exports: 'jQuery'
		},
		'jqueryui': ['jquery'],
		'bootstrap': ['jquery'],

		'signalr': ['jquery'],
		
		'Areas/Anywhere/Content/Scripts/../../../../signalr/hubs?': ['jquery', 'signalr'],

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

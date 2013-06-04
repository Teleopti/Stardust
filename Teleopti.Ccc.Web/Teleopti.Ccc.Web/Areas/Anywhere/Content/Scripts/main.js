
require.config({

	paths: {
		jquery: '../../../../Content/Scripts/jquery-1.8.3',
		knockout: '../../../../Content/Scripts/knockout-2.2.0',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-1.1.2.min',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentLanguages: '../../../../Content/moment/moment.all.min',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko',
		
		noext: '../../../../Content/require/noext',
		
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
		// wtf!
		'Areas/Anywhere/Content/Scripts/../../../../signalr/hubs?noext=1&noext=1': ['jquery', 'signalr'],
		'Areas/Anywhere/Content/Scripts/../../../../signalr/hubs?noext=1': ['jquery', 'signalr'],
		
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
], function () {
	
});

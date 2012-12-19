
require.config({

	paths: {
		jquery: '../../../../Content/Scripts/jquery-1.8.3',
		knockout: '../../../../Content/Scripts/knockout-2.2.0',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-1.0.0-alpha2.min',
		signalrHubs: '../../../../Content/signalr/signalr-hubs',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko',

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
		'signalrHubs': ['signalr'],
		'momentDatepicker': ['moment'],
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

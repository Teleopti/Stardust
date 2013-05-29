
require.config({

	paths: {
		jquery: '../../../../Content/Scripts/jquery-1.9.1',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/bootstrap.min',
		signalr: '../../../../Content/signalr/jquery.signalR-1.1.1.min',
		
		noext: '../../../../Content/require/noext',
		signalrrr: 'require/signalrrr',
		
		templates: '../templates',
		
		text: '../../../../Content/require/text'

	},

	// dependencies that requires loading order
	shim: {
		'bootstrap': ['jquery'],
		'signalr': ['jquery']
	}
});

require([
        'jquery',
        'modernizr',
        'bootstrap',
        'layout'
    ], function() {

    });
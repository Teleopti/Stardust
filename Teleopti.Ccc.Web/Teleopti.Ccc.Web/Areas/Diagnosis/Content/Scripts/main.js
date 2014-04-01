
require.config({

	paths: {
	    jquery: '../../../../Content/jquery/jquery-1.10.2',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		signalr: '../../../../Content/signalr/jquery.signalR-1.2.0.min',
		
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
        'view'
    ], function() {
	
});
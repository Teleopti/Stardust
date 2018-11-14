var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.12.4',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		signalr: '../../../../Content/signalr/jquery.signalR-2.3.0.min',
		buster: '../../../../Content/busterjs/buster-test',

		noext: '../../../../Content/require/noext',
		signalrrr: 'require/signalrrr',

		templates: '../../templates',

		text: '../../../../../Content/require/text'

	},

	// dependencies that requires loading order
	shim: {
		'bootstrap': ['jquery'],
		'signalr': ['jquery'],
		'buster': {
			exports: 'buster'
		}
	}
};

var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.10.2',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		buster: '../../../../Content/busterjs/buster-test',

		noext: '../../../../Content/require/noext',

		templates: '../templates',

		text: '../../../../Content/require/text'

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
var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.10.2',
		signalr: '../../../../Content/signalr/jquery.signalR-1.2.0.min',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		buster: '../../../../Content/busterjs/buster-test',
		signalrrr: 'require/signalrrr',
		viewmodel: '../vm',
	},
	
	// dependencies that requires loading order
	shim: {
		'buster': {
			exports: 'buster'
		}
	}
};
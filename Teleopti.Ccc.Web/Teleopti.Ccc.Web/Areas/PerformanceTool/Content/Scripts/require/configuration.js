var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.10.2',
		buster: '../../../../Content/busterjs/buster-test',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
	},
	
	// dependencies that requires loading order
	shim: {
		'buster': {
			exports: 'buster'
		},
		'knockout': ['jquery'],
	}
};
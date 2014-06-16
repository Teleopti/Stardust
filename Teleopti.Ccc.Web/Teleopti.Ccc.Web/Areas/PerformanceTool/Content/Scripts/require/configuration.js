var requireconfiguration = {
	paths: {
		buster: '../../../../Content/busterjs/buster-test',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
	},
	
	// dependencies that requires loading order
	shim: {
		'buster': {
			exports: 'buster'
		}
	}
};
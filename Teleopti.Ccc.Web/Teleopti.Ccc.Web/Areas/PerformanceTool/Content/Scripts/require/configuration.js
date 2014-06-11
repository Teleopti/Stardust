var requireconfiguration = {
	paths: {
		buster: '../../../../Content/busterjs/buster-test',
	},
	
	// dependencies that requires loading order
	shim: {
		'buster': {
			exports: 'buster'
		}
	}
};
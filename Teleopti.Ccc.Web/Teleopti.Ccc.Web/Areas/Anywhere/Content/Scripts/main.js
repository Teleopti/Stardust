
require.config({

	paths: {
		jquery: 'vendor/jquery-1.8.1.min',
		jqueryui: 'vendor/jquery-ui-1.9.0/jquery-ui-1.9.0.custom',
		knockout: 'vendor/knockout-2.1.0',
		moment: 'vendor/moment',
		bootstrap: 'vendor/bootstrap.min',
		signals: 'vendor/signals',
		crossroads: 'vendor/crossroads',
		hasher: 'vendor/hasher',
		flotr2: 'vendor/flotr2.amd',
		bean: 'vendor/bean-min',
		underscore: 'vendor/underscore-min',
		touchPunch: 'vendor/jquery.ui.touch-punch',

		templates: '../templates',

		text: 'vendor/require/text'

	},

	// dependencies that requires loading order
	shim: {
		'jquery': {
			exports: 'jQuery'
		},
		'jqueryui': ['jquery'],
		'bootstrap': ['jquery'],
		'underscore': {
			exports: '_'
		},
		'touchPunch': ['jquery'],
		'vendor/bootstrap-datepicker': ['jquery'],
		'widgets/editlayer/jquery.ui.editlayer': ['jqueryui'],
		'widgets/editlayer/jquery.ui.editlayer.ko': ['knockout', 'widgets/editlayer/jquery.ui.editlayer'],
		'flotr2': ['bean', 'underscore']
	}
});

require([
	'jquery',
	'jqueryui',
	'vendor/modernizr-2.6.1-respond-1.1.0.min',
	'bootstrap',
	'touchPunch',
	'vendor/bootstrap-datepicker',
	'widgets/editlayer/jquery.ui.editlayer',
	'widgets/editlayer/jquery.ui.editlayer.ko',
	'layout'
], function () {

	alert("hello world");
	
});

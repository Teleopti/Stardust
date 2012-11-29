
require.config({

	paths: {
		jquery: '/Content/Scripts/jquery-1.8.2',
		jqueryui: '/Content/jqueryui/jquery-ui-1.9.1.custom',
		knockout: '/Content/Scripts/knockout-2.1.0',
		modernizr: '/Content/modernizr/modernizr-2.6.2.min',
		respond: '/Content/respondjs/respond.min',
		moment: '/Content/moment/moment',
		bootstrap: '/Content/bootstrap/bootstrap.min',
		signals: '/Content/signals/signals',
		crossroads: '/Content/crossroads/crossroads',
		hasher: '/Content/hasher/hasher',
		touchPunch: '/Content/jqueryui.touch-punch/jquery.ui.touch-punch',
		swipeListener: '/Content/jquery-plugin/jquery.swipeListener',

		templates: '../templates',

		text: '/Content/require/text'
		
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
		'touchPunch': ['jqueryui'],
		'swipeListener': ['jquery']
		//'vendor/bootstrap-datepicker': ['jquery'],
		//'widgets/editlayer/jquery.ui.editlayer': ['jqueryui'],
		//'widgets/editlayer/jquery.ui.editlayer.ko': ['knockout', 'widgets/editlayer/jquery.ui.editlayer'],
	}
});

require([
	'jquery',
	'jqueryui',
	'modernizr',
	'respond',
	'bootstrap',
	'touchPunch',
	//'vendor/bootstrap-datepicker',
	//'widgets/editlayer/jquery.ui.editlayer',
	//'widgets/editlayer/jquery.ui.editlayer.ko',
	'layout'
], function () {
	
});

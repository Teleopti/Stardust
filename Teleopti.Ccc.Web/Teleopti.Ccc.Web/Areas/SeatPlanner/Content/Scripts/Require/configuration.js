﻿/// <reference path="C:\Data\Main2\Teleopti.Ccc.Web\Teleopti.Ccc.Web\Content/amplify/amplify.min.js" />
var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.10.2',
		jqueryui: '../../../../Content/jqueryui/jquery-ui-1.10.2.custom.min',
		lazy: '../../../../Content/lazy/lazy.min',
		knockout: '../Scripts/Knockout/knockout-3.2.0',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-2.1.2.min',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentLanguages: '../../../../Content/moment/moment-with-locales',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko-amd',
		select2: '../../../../Content/select2/select2',
		timepicker: '../../../../Content/bootstrap-timepicker/js/bootstrap-timepicker',
		buster: '../../../../Content/busterjs/buster-test',
		amplify: '../../../../Content/amplify/amplify.min',
		momentTimezoneData: '../../../../Content/moment-timezone/moment-timezone-with-data',
		fabric: '../Scripts/FabricJS/fabric.require',
		fabricViewport: '../Scripts/FabricJS/fabricjs_viewport',

		////depends on eve, hardcoded in  justgage.20130410.js
		//justgage: '../../../../Content/justgage/justgage.20130410',
		//raphael: '../../../../Content/justgage/raphael-min',

		knockoutBindings: 'Knockout/knockout.bindings',
		knockoutMapping: 'Knockout/knockout.mapping',


		noext: '../../../../Content/require/noext',
		//signalrrr: 'require/signalrrr',
		resourcesr: 'require/resourcesr',
		//xregexp: 'require/xregexp-all-min',

		templates: '../templates',
		text: '../../../../Content/require/text'
	},
	
	// dependencies that requires loading order
	shim: {
		'buster': {
			exports: 'buster'
		},
		'jquery': {
			exports: 'jQuery'
		},
		'jqueryui': ['jquery'],
		'lazy': {
			exports: 'Lazy'
		},
		'bootstrap': ['jquery'],
		'knockout': ['jquery'],
		'knockoutBindings': ['knockout'],
		'knockoutMapping': {
			deps: ['knockout'],
			exports: 'knockoutMapping'
		},
		'fabricViewport': ['fabric'],
		'select2': ['jquery', 'knockoutBindings'],
		'timepicker': ['bootstrap', 'knockoutBindings'],
		'signalr': ['jquery'],
		'momentLanguages': ['moment'],
		'momentDatepicker': ['momentLanguages'],
		'momentDatepickerKo': ['momentDatepicker'],
		'swipeListener': ['jquery'],
		//'justgage': {
		//	exports: 'JustGage',
		//	deps: ['require/raphaelloader']
		//},
		//'amplify': {
		//	exports: 'amplify'
		//}
	}
};
var requireconfiguration = {
	paths: {
		jquery: '../../../../Content/jquery/jquery-1.10.2',
		lazy: '../../../../Content/lazy/lazy.min',
		knockout: '../../../../Content/Scripts/knockout-2.2.1',
		modernizr: '../../../../Content/modernizr/modernizr-2.6.2.min',
		respond: '../../../../Content/respondjs/respond.min',
		moment: '../../../../Content/moment/moment',
		bootstrap: '../../../../Content/bootstrap/Scripts/bootstrap.min',
		signals: '../../../../Content/signals/signals',
		signalr: '../../../../Content/signalr/jquery.signalR-1.2.0.min',
		crossroads: '../../../../Content/crossroads/crossroads',
		hasher: '../../../../Content/hasher/hasher',
		swipeListener: '../../../../Content/jquery-plugin/jquery.swipeListener',
		momentDatepicker: '../../../../Content/moment-datepicker/moment-datepicker',
		momentLanguages: '../../../../Content/moment/moment.all.min',
		momentDatepickerKo: '../../../../Content/moment-datepicker/moment-datepicker-ko-amd',
		select2: '../../../../Content/select2/select2',
		timepicker: '../../../../Content/bootstrap-timepicker/js/bootstrap-timepicker',
		buster: '../../../../Content/busterjs/buster-test',

		justgage: '../../../../Content/justgage/justgage.1.0.1.min',
		raphael: '../../../../Content/justgage/raphael-min',

		knockoutBindings: 'knockout.bindings',

		noext: '../../../../Content/require/noext',
		signalrrr: 'require/signalrrr',
		resources: 'require/resources',

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
		'select2': ['jquery', 'knockoutBindings'],
		'timepicker': ['bootstrap', 'knockoutBindings'],
		'signalr': ['jquery'],
		'momentLanguages': ['moment'],
		'momentDatepicker': ['momentLanguages'],
		'momentDatepickerKo': ['momentDatepicker'],
		'swipeListener': ['jquery'],
		'justgage': {
			exports: 'JustGage',
			deps: ['raphael']
		}
	}
};
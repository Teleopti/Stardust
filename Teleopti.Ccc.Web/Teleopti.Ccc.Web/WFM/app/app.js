'use strict';

// if (!d3) {
// 	var d3;
// 	require(['/TeleoptiWFM/Web/WFM/dist/resources/d3.js'], function(d3mod) {
// 		d3 = d3mod;
// 	});
// }

angular.module('externalModules', [
	'ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'ngSanitize',
	'pascalprecht.translate',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection',
	'ngStorage',
	'dialogs.main',
	'ui.bootstrap.persian.datepicker'
]);

(function() {
	'use strict';
	angular.module('wfm.outbound', ['outboundServiceModule', 'ngAnimate', 'pascalprecht.translate', 'wfm.cardList', 'wfm.daterangepicker']);
    angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);
})();
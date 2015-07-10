(function() {

	'use strict';

    angular.module('wfm.outbound', ['outboundServiceModule', 'ngAnimate', 'pascalprecht.translate']);
    angular.module('outboundServiceModule', ['ngResource', 'pascalprecht.translate']);
})();
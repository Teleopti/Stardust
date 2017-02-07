(function () {
	'use strict';

	angular
	.module('wfm.dataProtection')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('dataProtection', {
			url: '/fdpa',
			templateUrl: 'app/global/data-protection/data-protection.html',
			controller: 'DataProtectionCtrl as vm'
		})
	}
})();

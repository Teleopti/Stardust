'use strict';
angular.module('wfm.settings')
	.controller('SettingsDefaultCtrl', [
		'$state', '$location', function ($state, $location) {
			if ($location.url() == $state.current.url)
				$state.go('settings.skillcreate');
		}
	]);

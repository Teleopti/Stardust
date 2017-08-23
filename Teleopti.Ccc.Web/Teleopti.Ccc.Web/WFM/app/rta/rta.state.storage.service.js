(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaStateStorageService', rtaStateStorageService);

        rtaStateStorageService.$inject = ['$state'];

	function rtaStateStorageService($state) {

		var service = {
            setState: setState,
            getState: getState,
            removeState: removeState
		}

		return service;
        ///////////////////////
        
        function setState(state, params) {
            sessionStorage.setItem('state', state);
            sessionStorage.setItem('params', JSON.stringify(params));
        }

        function getState() {
            return {
                'state': sessionStorage.getItem('state'),
                'params': JSON.parse(sessionStorage.getItem('params'))
            }
        };

        function removeState() {
            sessionStorage.removeItem('state');
            sessionStorage.removeItem('params');
        };
	};
})();

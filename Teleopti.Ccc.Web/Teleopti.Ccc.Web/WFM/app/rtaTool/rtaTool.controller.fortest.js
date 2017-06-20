'use strict';
(function() {
	angular
		.module('wfm.rtaTool')
		.factory('RtaToolControllerBuilder', RtaToolControllerBuilder);

	RtaToolControllerBuilder.$inject = ['$controller', '$interval', '$httpBackend', '$rootScope', '$log'];

	function RtaToolControllerBuilder($controller, $interval, $httpBackend, $rootScope, $log) {
		var controllerName = "hejsan";
		var scope;

		var service = {
			setup: setup,
			createController: createController
		};

		return service;
		/////////////////////

		function setup(name) {
			controllerName = name;
			scope = $rootScope.$new();
			return scope;
		};

		function createController() {

			var vm = $controller(controllerName, {
				$scope: scope
			});

			scope.$digest();
			safeBackendFlush();

			var callbacks = {
				apply: function(apply) {
					if (angular.isFunction(apply)) {
						apply();
						scope.$digest();
					} else {
						scope.$apply(apply);
					}
					safeBackendFlush();
					return callbacks;
				},

				wait: function(milliseconds) {
					$interval.flush(milliseconds);
					safeBackendFlush();
					return callbacks;
				},
				vm: vm
			};

			return callbacks;

			function safeBackendFlush() {
				try { // the internal mock will throw if no requests were made
					$httpBackend.flush();
				} catch (e) {
					if (e.message.includes("No pending request to flush !"))
						return;
					$log.error(e.message);
				}
			};
		}

	};
})();

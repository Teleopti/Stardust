'use strict';
(function() {
	angular
		.module('wfm.rta')
		.factory('ControllerBuilder', ControllerBuilder);

	ControllerBuilder.$inject = ['$controller', '$interval', '$httpBackend', '$rootScope'];

	function ControllerBuilder($controller, $interval, $httpBackend, $rootScope) {
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
					if (typeof apply === "function") {
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
					console.error(e.message)
				}
			};
		}

	};
})();

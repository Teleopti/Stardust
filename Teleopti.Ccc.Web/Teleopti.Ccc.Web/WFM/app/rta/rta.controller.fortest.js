'use strict';
(function() {
	angular
		.module('wfm.rta')
		.service('ControllerBuilder', function($controller, $interval, $httpBackend, $rootScope) {

			var controllerName = "hejsan";
			var scope;

			this.setup = function(name) {
				controllerName = name;
				scope = $rootScope.$new();
				return scope;
			};

			var safeBackendFlush = function () {
				try { // the internal mock will throw if no requests were made
					$httpBackend.flush();
				} catch (e) {
				}
			};

			this.createController = function () {

				$controller(controllerName, {
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
					}

				};

				return callbacks;
			}
		});
})();
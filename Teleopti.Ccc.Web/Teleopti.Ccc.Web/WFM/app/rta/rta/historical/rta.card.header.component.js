(function () {
	'use strict'
	
	function RtaCardHeaderController() {
		var ctrl = this;

		ctrl.toggle = function toggle() {
			return ctrl.rtaCardController.toggle()
		}
	}

	angular
		.module('wfm.rta')
		.component('rtaCardHeader', {
			templateUrl: 'app/rta/rta/historical/rta-card-header-component.html',
			controller: RtaCardHeaderController,
			transclude: true,
			require: {
				rtaCardController: '^rtaCard'
			},
			bindings: {
				classes: '@',
				styles: '@'
			}
		})
}());

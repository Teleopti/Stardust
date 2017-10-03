(function() {
	'use strict'

	angular
		.module('wfm.rta')
		.component('rtaCard', {
			templateUrl: 'app/rta/rta/historical/rta-card-component.html',
			controller: function() {
				var ctrl = this

				ctrl.toggle = function toggle() {
					ctrl.ngOpen = !ctrl.ngOpen;
				}

			},
			transclude: {
				header: 'rtaCardHeader',
				body: 'rtaCardBody'
			},
			bindings: {
				ngOpen: '='
			}
		})
}());

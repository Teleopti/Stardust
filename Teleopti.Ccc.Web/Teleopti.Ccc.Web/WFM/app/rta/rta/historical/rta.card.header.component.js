(function () {
	'use strict'

	var mainClass = [
		'wfm-card-title'
	]

	function RtaCardHeaderController() {
		var ctrl = this;

		ctrl.toggle = function toggle() {
			return ctrl.rtaCardController.toggle()
		}

		var classes = ctrl.classes != null ? cleanStrArr(ctrl.classes.split(' ')) : []
		ctrl.allClasses = mainClass.concat(classes).join(' ')
	}

	function cleanStrArr(arr) {
		return arr
			.map(function (str) {
				return str.trim()
			})
			.filter(function (str) {
				return str != ""
			});
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

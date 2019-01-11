(function () {
	'use strict';
	angular
		.module('wfm.rta')
		.component('rtaCardPanel', {
			bindings: {
				open: "=",
				leftBorderColor: "<",
				classes: "<"
			},
			transclude: {
				'header': 'rtaCardPanelHeader',
				'content': 'rtaCardPanelContent'
			},
			template: 
				'<div class="panel">' +
				'<div class="card-panel-header-wrapper pointer" ng-transclude="header" ng-click="$ctrl.open = !$ctrl.open" ng-style="{\'border-left\': \'10px solid \' + $ctrl.leftBorderColor}" ng-class="$ctrl.classes"></div>' +
				'<div class="card-panel-content-wrapper" ng-transclude="content" ng-if="$ctrl.open"></div>' +
				'</div>'
		});
})();
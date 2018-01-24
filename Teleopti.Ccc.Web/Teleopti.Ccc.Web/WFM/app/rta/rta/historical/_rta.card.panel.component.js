(function () {
	'use strict';
	angular
		.module('wfm.rta')
		.component('rtaCardPanel', {
			bindings: {
				open: "=",
				leftBorderColor: "<"
			},
			transclude: {
				'header': 'rtaCardPanelHeader',
				'content': 'rtaCardPanelContent'
			},
			template: `
				<div class="panel material-depth-1">
					<div class="card-panel-header-wrapper pointer"
						ng-transclude="header" 
						ng-click="$ctrl.open = !$ctrl.open" 
						ng-style="{'border-left': '10px solid ' + $ctrl.leftBorderColor}"></div>
					<div class="card-panel-content-wrapper" ng-transclude="content" ng-if="$ctrl.open"></div>
				</div>
				`
		});
})();
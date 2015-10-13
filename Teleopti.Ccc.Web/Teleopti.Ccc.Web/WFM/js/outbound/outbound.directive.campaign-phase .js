(function() {
    'use strict';

    angular.module('wfm.outbound').directive('phasePicker', [phasePicker]);

	function phasePicker() {
		return {
			controller: ['$scope', '$element', phasePickerCtrl],
			templateUrl: 'js/outbound/html/phase-picker.tpl.html',
			scope: {
				'phaseStatistics': '=?',
				'activePhaseCode': '=',
                'disableAction': '=?'
			}
		};

		function phasePickerCtrl($scope, $element) {

			$scope.phases = [
				{
					phaseCode: 1,
					phaseName: 'Planned',
					isActive: false,
					totalNum: 0,
					warningNum:0
				},
				{
					phaseCode: 2,
					phaseName: 'Scheduled',
					isActive: false,
					totalNum: 0,
					warningNum: 0
				},
				{
					phaseCode: 4,
					phaseName: 'Ongoing',
					isActive: false,
					totalNum: 0,
					warningNum: 0
				},
				{
					phaseCode: 8,
					phaseName: 'Done',
					isActive: false,
					totalNum: 0,
					warningNum: 0
				}
			];

			initActivePhase();
			$scope.$watch('phaseStatistics', function (newValue, oldValue) {
				if (newValue !== oldValue) {
					setPhaseStastistics(newValue);

				}
					
			});

			$scope.togglePhase = function(phase) {
				phase.isActive = ! phase.isActive;
			}

			$scope.$watch(calculatePhaseCode, function (value) {				
				$scope.activePhaseCode = value;
			});
			
			function setPhaseStastistics(data) {
				var data = angular.copy(data);
				$scope.phases.forEach(function (e) {
					if (e.phaseCode == 1) {
						e.totalNum = data.Planned;
						e.warningNum = data.PlannedWarning;
					}
					if (e.phaseCode == 2) {
						e.totalNum = data.Scheduled;
						e.warningNum = data.ScheduledWarning;
					}
					if (e.phaseCode == 4) {
						e.totalNum = data.OnGoing;
						e.warningNum = data.OnGoingWarning;
					}
					if (e.phaseCode == 8) {
						e.totalNum = data.Done;
					}
				});
			}

			function calculatePhaseCode() {
				var i;
				var sum = 0;
				for (i = 0; i < $scope.phases.length; i ++) {
					if ($scope.phases[i].isActive)
						sum += $scope.phases[i].phaseCode;
				}
				return sum;
			}

			function initActivePhase() {
				var i;
				var phaseCode = $scope.activePhaseCode;
				for (i = 0; i < $scope.phases.length; i++) {					
					$scope.phases[i].isActive = phaseCode & 1;					
					phaseCode = phaseCode >> 1;
					
									
				}

			}
			
			

		}

	};


	


})();
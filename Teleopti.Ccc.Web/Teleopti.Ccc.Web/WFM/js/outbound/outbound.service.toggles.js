(function() {

	angular.module('wfm.outbound').service('OutboundToggles', ['$q', 'Toggle', outboundToggles]);

	function outboundToggles($q, Toggle) {
		var self = this;

		self.toggleNames = [
			'Wfm_Outbound_Campaign_GanttChart_34259',
			'Wfm_Outbound_Campaign_GanttChart_Travel_34924'
		];

		self.toggles = {};
		self.ready = false;

		$q.all(self.toggleNames.map(function(t) {
			return Toggle.isFeatureEnabled.query({ toggle: t }).$promise;
		})).then(function (resp) {			
			for (var i = 0; i < self.toggleNames.length; i ++) {
				self.toggles[self.toggleNames[i]] = resp[i].IsEnabled;
			}			
			self.ready = true;
		});

		self.isGanttEnabled = function () {
			return self.ready && self.toggles['Wfm_Outbound_Campaign_GanttChart_34259'];
		};

		self.isTravelEnabled=function() {
						return self.ready && self.toggles['Wfm_Outbound_Campaign_GanttChart_Travel_34924'];
					}
	}

})();
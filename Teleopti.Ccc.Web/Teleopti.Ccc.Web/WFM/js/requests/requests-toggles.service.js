(function() {

	angular.module('wfm.requests').service('RequestsToggles', ['$q', 'Toggle', requestsToggles]);

	function requestsToggles($q, Toggle) {
		var self = this;

		this.toggleNames = [
			'Wfm_Requests_Basic_35986',
			'Wfm_Requests_People_Search_36294',
			'Wfm_Requests_Performance_36295',
			'Wfm_Requests_ApproveDeny_36297'
		];
	
		this.togglePromise = $q.all(self.toggleNames.map(function (t) {
			return Toggle.isFeatureEnabled.query({ toggle: t }).$promise;
		})).then(function (resp) {
			var toggles = {};
			for (var i = 0; i < self.toggleNames.length; i ++) {
				toggles[self.toggleNames[i]] = resp[i].IsEnabled;
			}
			return addAuxiliaryMethod(toggles);
		});

		function addAuxiliaryMethod(toggles) {
			toggles.isRequestsEnabled = function() {
				return toggles['Wfm_Requests_Basic_35986'];
			};

			toggles.isPeopleSearchEnabled = function () {
				return toggles['Wfm_Requests_People_Search_36294'];
			}

			toggles.isPaginationEnabled = function() {
				return toggles['Wfm_Requests_Performance_36295'];
			};

			toggles.isRequestsCommandsEnabled = function () {
				return toggles['Wfm_Requests_ApproveDeny_36297'];
			};

			return toggles;
		}	
	}

})();
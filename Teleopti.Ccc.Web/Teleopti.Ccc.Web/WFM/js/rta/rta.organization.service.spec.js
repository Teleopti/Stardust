
'use strict';
describe('RtaOrganizationService', function() {
	var $q,
		$rootScope,
		$filter;

	var rtaSvrc = {
		getSites: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{
					"Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
					"Name": "London",
					"NumberOfAgents": 46
				}, {
					"Id": "6a21c802-7a34-4917-8dfd-9b5e015ab461",
					"Name": "Paris",
					"NumberOfAgents": 50
				}]);
				return {
					$promise: queryDeferred.promise
				};
			}
		},

		getAgents: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{
					"Name": "Julian Feldman",
					"PersonId": "cb67d5f1-5dd1-45af-b4e2-9b5e015b2572",
					"SiteId": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
					"SiteName": "London",
					"TeamId": "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
					"TeamName": "Students"
				}]);
				return {
					$promise: queryDeferred.promise
				};
			}
		},

		getStates: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{
					"PersonId": "cb67d5f1-5dd1-45af-b4e2-9b5e015b2572",
					"State": "Ready",
					StateStartTime: "\/Date(1429254905000)\/",
					"Activity": "Phone",
					"NextActivity": "Short break",
					"NextActivityStartTime": "\/Date(1432109700000)\/",
					"Alarm": "In Adherence",
					"AlarmStart": "\/Date(1432105910000)\/",
					"AlarmColor": "#00FF00",
					"TimeInState": 15473375
				}]);
				return {
					$promise: queryDeferred.promise
				};
			}
		},

		getAdherenceForAllSites: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([]);
				return {
					$promise: queryDeferred.promise
				};
			}
		}
	};

	beforeEach(function() {
		module('wfm.rta');
		module(function($provide) {
			$provide.service('RtaService', function() {
				return rtaSvrc;
			});
		});
	});

	beforeEach(inject(function(_$q_, _$rootScope_, _$filter_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$filter = _$filter_;
	}));

});

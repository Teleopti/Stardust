'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		mockResourceplannerSvrc

		beforeEach(function(){
			module('wfm.resourceplanner');
			module('externalModules');
		});


	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		mockResourceplannerSvrc = {
			getPlanningPeriod:{
				query: function() {
					var deferred = $q.defer();
					deferred.resolve();
					return deferred.promise;
			}
		},
		getDayoffRules:{
			query: function() {
				var deferred = $q.defer();
				deferred.resolve({Id:0, Default:true});
				return { $promise: deferred.promise};
			}
		},
		saveDayoffRules:{
			update: function() {
				var deferred = $q.defer();
				deferred.resolve();
				return { $promise: deferred.promise };
			}
		}
	};
	}));






	var setupScope = function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		scope.$digest();
		return scope;
	}
});

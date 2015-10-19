
'use strict';
describe('RtaAgentsFilters', function() {

	beforeEach(module('wfm'));

	it('should filter with the whole input string', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman'
		}, {
			name: 'Kevin Glancy'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Julian Feldman');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian Feldman');
	}));

	it('should filter with partial of the input string', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman'
		}, {
			name: 'Kevin Glancy'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Julia');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian Feldman');
	}));

	it('should filter with the input string case insensitively', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman'
		}, {
			name: 'Kevin Glancy'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'juLia');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian Feldman');
	}));

	it('should filter with the multiple keywords', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman'
		}, {
			name: 'Julian Glancy'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Ju F');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian Feldman');
	}));

	it('should return empty if not found on the multiple keywords', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman'
		}, {
			name: 'Kevin Glancy'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'jul kev');

		expect(filteredData.length).toEqual(0);
	}));

	it('should return if any property matches', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman',
			team: 'Team London'
		}, {
			name: 'Kevin Glancy',
			team: 'Team London'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Julian');

		expect(filteredData[0].name).toEqual('Julian Feldman');
	}));

	it('should filter on non-string values', inject(function($filter) {
		var data = [{
			id: 123
		}, {
			id: 456
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, '12');

		expect(filteredData[0].id).toEqual(123);
	}));
});

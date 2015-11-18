
'use strict';
describe('RtaAgentsFilters', function() {

	beforeEach(module('wfm.rta'));

	it('should filter with the whole input string without space', inject(function($filter) {
		var data = [{
			name: 'Julian'
		}, {
			name: 'Kevin'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Julian');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian');
	}));

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

	it('should not filter on null values', inject(function($filter) {
		var data = [{
			name: "Julian",
			state: null
		}, {
			name: null,
			state: "Ready"
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Julian');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian');
	}));

	it('should filter with the multiple keywords in different columns', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman',
			teamName: 'London'
		}, {
			name: 'Julian Glancy',
			teamName: 'Students'
		}, {
			name: 'Kevin Glancy',
			teamName: 'Students'
		}, {
			name: 'Kevin Glancer',
			teamName: 'Students'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, 'Ju Stu');

		expect(filteredData.length).toEqual(1);
		expect(filteredData[0].name).toEqual('Julian Glancy');
		expect(filteredData[0].teamName).toEqual('Students');
	}));

	it('should filter on specified columns', inject(function($filter) {
		var data = [{
			name: 'Julian Feldman',
			state: 'Phone',
			teamId: '123',
			siteId: '123',
			personId: '123'
		}];
		var filter = $filter('agentFilter');

		var filteredData = filter(data, '123', ["name","state"]);

		expect(filteredData.length).toEqual(0);
	}));
});

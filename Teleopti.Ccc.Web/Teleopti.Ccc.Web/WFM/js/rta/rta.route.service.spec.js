'use strict';
describe('RtaRouteService', function() {
	var target, $state;

	beforeEach(module('wfm.rta'));
	beforeEach(inject(function(_$state_, RtaRouteService) {
		target = RtaRouteService;
		$state = _$state_;
	}));

	it('should go back to sites overview', function() {
		spyOn($state, 'go');

		target.goToSites();

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should go back to teams view', function() {
		spyOn($state, 'go');

		target.goToTeams('d970a45a-90ff-4111-bfe1-9b5e015ab45c');

		expect($state.go).toHaveBeenCalledWith('rta-teams', {
			siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c'
		});
	});

	it('should get change schedule url for an agent', function() {
		var today = new Date('2015-11-02');
		jasmine.clock().mockDate(today);

		expect(target.urlForChangingSchedule("928dd0bc-bf40-412e-b970-9b5e015aadea", "34590a63-6331-4921-bc9f-9b5e015ab495", "11610fe4-0130-4568-97de-9b5e015b2564"))
			.toEqual("../Anywhere#teamschedule/928dd0bc-bf40-412e-b970-9b5e015aadea/34590a63-6331-4921-bc9f-9b5e015ab495/11610fe4-0130-4568-97de-9b5e015b2564/20151102");
	});

	it('should get agent details url', function() {
		expect(target.urlForAgentDetails('11610fe4-0130-4568-97de-9b5e015b2564'))
		.toEqual('#/rta/agent-details/11610fe4-0130-4568-97de-9b5e015b2564');
	});

	it('should get sites overview url', function() {
		expect(target.urlForSites()).toEqual('#/rta');
	});

	it('should get teams overview url', function() {
		expect(target.urlForTeams('d970a45a-90ff-4111-bfe1-9b5e015ab45c')).toEqual('#/rta/teams/d970a45a-90ff-4111-bfe1-9b5e015ab45c');
	});
});

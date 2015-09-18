'use strict';
describe('RtaOrganizationService', function () {

	beforeEach(module('wfm.rta'));

	it('should get the site name from the site id', function () {
		inject(function (RtaOrganizationService) {

			var siteId = 'd970a45a-90ff-4111-bfe1-9b5e015ab45c';
			var siteName = RtaOrganizationService.getSiteName(siteId);

			expect(siteName).toBe('London');

		})
	});

	fit('should get the team name from the team id', function () {
		inject(function (RtaOrganizationService) {

			var teamId = 42;
			var teamName = RtaOrganizationService.getTeamName(teamId);

			expect(teamName).toBe('Preferences');
		})
	});

});
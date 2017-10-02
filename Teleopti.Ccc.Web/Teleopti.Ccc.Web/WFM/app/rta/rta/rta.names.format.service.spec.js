'use strict';
describe('rtaNamesFormatService', function () {
	var target;
	var sites = buildSites();
	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));

	beforeEach(inject(function (rtaNamesFormatService) {
		target = rtaNamesFormatService;
	}));

	afterEach(function () {
	});

	it('should get sites name', function () {
		var siteIds = ["parisGuid"];
		var teamIds = [];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Sites: Paris");
	});

	it('should get sites name sepated by comma', function () {
		var siteIds = ["parisGuid", "londonGuid"];
		var teamIds = [];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Sites: Paris, London");
	});

	it('should get sites name with trailing dots', function () {
		var siteIds = ["parisGuid", "londonGuid", "helsinkiGuid"];
		var teamIds = [];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Sites: Paris, London, ...");
	});

	it('should get teams name', function () {
		var siteIds = [];
		var teamIds = ["team1Guid"];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Teams: Team 1");
	});

	it('should get teams name sepated by comma', function () {
		var siteIds = [];
		var teamIds = ["team1Guid", "team2Guid"];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Teams: Team 1, Team 2");
	});

	it('should get two teams name with trailing dots', function () {
		var siteIds = [];
		var teamIds = ["team1Guid", "team2Guid", "team3Guid"];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Teams: Team 1, Team 2, ...");
	});

	it('should get two sites and teams name with trailing dots', function () {
		var siteIds = ["londonGuid", "helsinkiGuid", "denverGuid"];
		var teamIds = ["team1Guid", "team2Guid", "team3Guid"];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("Sites: London, Helsinki, ..., Teams: Team 1, Team 2, ...");
	});

	it('should get two sites and teams name with trailing dots with translation', function () {
		var siteIds = ["londonGuid", "helsinkiGuid", "denverGuid"];
		var teamIds = ["team1Guid", "team2Guid", "team3Guid"];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds, "Sajter: ", "Lag: ")).toEqual("Sajter: London, Helsinki, ..., Lag: Team 1, Team 2, ...");
	});

	it('should get no sites and teams name', function () {
		var siteIds = [];
		var teamIds = [];
		expect(target.getSelectedFieldText(sites, siteIds, teamIds)).toEqual("");
	});

});

function buildSites() {
	return [
		{
			Id: "parisGuid",
			Name: "Paris",
			Teams: [
				{
					Id: "team1Guid",
					Name: "Team 1",
				},
				{
					Id: "team2Guid",
					Name: "Team 2",
				},
				{
					Id: "team3Guid",
					Name: "Team 3",
				},
				{
					Id: "team4Guid",
					Name: "Team 4",
				},
			]
		},
		{
			Id: "londonGuid",
			Name: "London",
			Teams: [
				{
					Id: "studentsGuid",
					Name: "Students",
				},
				{
					Id: "teamDailyGuid",
					Name: "Team Daily",
				},
				{
					Id: "teamMixedGuid",
					Name: "Team Mixed",
				},
			]
		},
		{
			Id: "helsinkiGuid",
			Name: "Helsinki",
			Teams: [
				{
					Id: "teamMin-MaxGuid",
					Name: "Team Min-Max",
				}
			]
		},
		{
			Id: "denverGuid",
			Name: "Denver",
			Teams: [
				{
					Id: "teamLindaGuid",
					Name: "Team Linda",
				}
			]
		}
	];
}
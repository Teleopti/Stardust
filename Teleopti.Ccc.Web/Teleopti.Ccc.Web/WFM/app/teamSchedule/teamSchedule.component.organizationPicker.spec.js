describe('organizationPicker component tests', function () {

	var $componentController;


	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (_$componentController_) {
		$componentController = _$componentController_;
	}));

	it("should populate hierachy list", inject(function () {
		var bindings = {
			availableGroups: {
				sites:[ {
					Id:'site1', Name:'site1', Children:[ {
						Id: 'team1', Name:'team1'
					}
					]
				},
				{
					Id:"site2", Name:'site2', Children:[ {
						Id: 'team2', Name:'team2'
					},
					{
						Id: 'team3', Name:'team3'
					}
					]
				}
				],
				logonUserTeamId: 'logonUserTeamId'
			},

			onPick:function() {}
		};

		var ctrl = $componentController('organizationPicker', null, bindings);
		ctrl.$onInit();

		expect(ctrl.groupList.length).toEqual(2);
		expect(ctrl.groupList[0].id).toEqual("site1");
		expect(ctrl.groupList[1].id).toEqual("site2");
		expect(ctrl.groupList[0].teams.length).toEqual(1);
		expect(ctrl.groupList[1].teams.length).toEqual(2);
	}));

	it("should extract the right abbreviation of the selected time zone ", inject(function () {
		var bindings = {
			availableGroups: {
				sites:[ {
					Id:'site1', Name:'site1', Children:[ {
						Id: 'team1', Name:'team1'
					}
					]
				},
				{
					Id:"site2", Name:'site2', Children:[ {
						Id: 'team2', Name:'team2'
					},
					{
						Id: 'team3', Name:'team3'
					}
					]
				}
				],
				logonUserTeamId: 'logonUserTeamId'
			},

			onPick:function() {}
		};

		var ctrl = $componentController('organizationPicker', null, bindings);
		ctrl.$onInit();

		ctrl.selectedTeamIds = ['team1'];
		var displayName = ctrl.formatSelectedDisplayName();
		expect(displayName).toEqual("team1");

		ctrl.selectedTeamIds = [];
		displayName = ctrl.formatSelectedDisplayName();
		expect(displayName).toEqual("Organization");
	}));

	it("Should trigger onPick when selection done", function () {
		var selectedTeams=[];
		var bindings = {
			availableGroups: [
				{
					Id: 'site1',
					Name: 'site1',
					Children: [
						{
							Id: 'team1',
							Name: 'team1'
						}
					]
				}, {
					Id: "site2",
					Name: 'site2',
					Children: [
						{
							Id: 'team2',
							Name: 'team2'
						},
						{
							Id: 'team3',
							Name: 'team3'
						}
					]

				}
			],
			onPick: function(input) {
				selectedTeams = input.groups;
			}
		};
		var ctrl = $componentController('organizationPicker', null, bindings);
		ctrl.$onInit();

		ctrl.selectedTeamIds = ['team1', 'team2'];
		ctrl.onSelectionDone();
		expect(selectedTeams.length).toEqual(2);
	});

});
﻿'use strict';

describe('RtaMainController', function () {
  var
    $httpBackend,
    $fakeBackend,
    $controllerBuilder,
    $state,
    $interval,
    scope,
    vm;

  var skills1, skills2;
  var stateParams = {};
  var goodColor = '#C2E085';
  var warningColor = '#FFC285';
  var dangerColor = '#EE8F7D';

  beforeEach(module('wfm.rta'));


  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        stateParams = {};
        return stateParams;
      });
    });
  });

  beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_) {
    $httpBackend = _$httpBackend_;
    $interval = _$interval_;
    $state = _$state_;
    $fakeBackend = _FakeRtaBackend_;
    $controllerBuilder = _ControllerBuilder_;

    scope = $controllerBuilder.setup('RtaMainController');

    skills1 = [
      {
        Name: 'Channel Sales',
        Id: '1'
      },
      {
        Name: 'Phone',
        Id: '2'
      }
    ];

    skills2 = [
      {
        Name: 'Invoice',
        Id: '3'
      },
      {
        Name: 'BTS',
        Id: '4'
      }
    ];

    $fakeBackend.clear();
    spyOn($state, 'go');
    $state.current.name = 'rta-refact';
  }));

  describe('RtaFilterComponent handling', function () {
    it('should get skills', function () {
      $fakeBackend
        .withSkill({
          Name: 'Channel Sales',
          Id: '1'
        })
        .withSkill({
          Name: 'Phone',
          Id: '2'
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.skills[0].Name).toEqual('Channel Sales');
      expect(vm.skills[0].Id).toEqual('1');
      expect(vm.skills[1].Name).toEqual('Phone');
      expect(vm.skills[1].Id).toEqual('2');
    });

    it('should get skill areas', function () {
      $fakeBackend
        .withSkillAreas([{
          Id: "5",
          Name: "my skill area 1",
          Skills: skills1
        },
        {
          Id: "6",
          Name: "my skill area 2",
          Skills: skills2
        }
        ]);

      vm = $controllerBuilder.createController().vm;

      expect(vm.skillAreas.length).toEqual(2);
      expect(vm.skillAreas[0].Id).toEqual('5');
      expect(vm.skillAreas[0].Name).toEqual('my skill area 1');
      expect(vm.skillAreas[0].Skills[0].Id).toEqual('1');
      expect(vm.skillAreas[0].Skills[0].Name).toEqual('Channel Sales');
      expect(vm.skillAreas[0].Skills[1].Id).toEqual('2');
      expect(vm.skillAreas[0].Skills[1].Name).toEqual('Phone');
      expect(vm.skillAreas[1].Id).toEqual('6');
      expect(vm.skillAreas[1].Name).toEqual('my skill area 2');
      expect(vm.skillAreas[1].Skills[0].Id).toEqual('3');
      expect(vm.skillAreas[1].Skills[0].Name).toEqual('Invoice');
      expect(vm.skillAreas[1].Skills[1].Id).toEqual('4');
      expect(vm.skillAreas[1].Skills[1].Name).toEqual('BTS');
    });

    it('should get organization', function () {
      $fakeBackend.withOrganization({
        Id: 'LondonGuid',
        Name: 'London',
        Teams: [{
          Id: '1',
          Name: 'Team Preferences'
        }, {
          Id: '2',
          Name: 'Team Students'
        }]
      })

      vm = $controllerBuilder.createController().vm;

      expect(vm.organization[0].Id).toEqual('LondonGuid');
      expect(vm.organization[0].Name).toEqual('London');
      expect(vm.organization[0].Teams[0].Id).toEqual('1');
      expect(vm.organization[0].Teams[0].Name).toEqual('Team Preferences');
      expect(vm.organization[0].Teams[1].Id).toEqual('2');
      expect(vm.organization[0].Teams[1].Name).toEqual('Team Students');
    });

    it('should get organization by skill', function () {
      stateParams.skillIds = ['1'];
      $fakeBackend
        .withSkill({
          Name: 'Channel Sales',
          Id: '1'
        })
        .withOrganizationOnSkills({
          Id: '2',
          Name: 'London',
          Teams: [{
            Id: '3',
            Name: 'Team Preferences'
          }]
        }, '1');

      vm = $controllerBuilder.createController().vm;

      expect(vm.organization.length).toEqual(1);
      expect(vm.organization[0].Id).toEqual('2');
      expect(vm.organization[0].Name).toEqual('London');
      expect(vm.organization[0].Teams[0].Id).toEqual('3');
      expect(vm.organization[0].Teams[0].Name).toEqual('Team Preferences');
    });

  });

  describe('RtaOverviewComponent handling', function () {

    it('should build site card view model', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: "londonGuid",
          Name: "London",
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: "warning"
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.siteCards[0].site.Id).toEqual("londonGuid");
      expect(vm.siteCards[0].site.Name).toEqual("London");
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(5);
      expect(vm.siteCards[0].site.Color).toEqual(warningColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
    });

    it('should update adherence', function () {
      $fakeBackend.withSiteAdherence({
        Id: "londonGuid",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });

      var c = $controllerBuilder.createController();
      vm = c.vm;
      c.apply(function () {
        $fakeBackend.clearSiteAdherences()
          .withSiteAdherence({
            Id: "londonGuid",
            Name: "London",
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: "good"
          });
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual("londonGuid");
      expect(vm.siteCards[0].site.Name).toEqual("London");
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
    });

    it('should update adherence for site with skill', function () {
      $fakeBackend.withSiteAdherence({
        Id: "londonGuid",
        SkillId: "phoneId",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['phoneId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: "londonGuid",
            SkillId: "phoneId",
            Name: "London",
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: "good"
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual("londonGuid");
      expect(vm.siteCards[0].site.Name).toEqual("London");
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
    });

    it('should update adherence for sites with skills', function () {
      $fakeBackend
      .withSiteAdherence({
        Id: "londonGuid",
        SkillId: "phoneId",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      })
      .withSiteAdherence({
        Id: "parisGuid",
        SkillId: "emailId",
        Name: "Paris",
        AgentsCount:8,
        InAlarmCount: 4,
        Color: "warning"
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['phoneId', 'emailId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: "londonGuid",
            SkillId: "phoneId",
            Name: "London",
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: "good"
          })
           .withSiteAdherence({
            Id: "parisGuid",
            SkillId: "phoneId",
            Name: "Paris",
            AgentsCount: 8,
            InAlarmCount: 3,
            Color: "good"
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual("londonGuid");
      expect(vm.siteCards[0].site.Name).toEqual("London");
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
      expect(vm.siteCards[1].site.Id).toEqual("parisGuid");
      expect(vm.siteCards[1].site.Name).toEqual("Paris");
      expect(vm.siteCards[1].site.AgentsCount).toEqual(8);
      expect(vm.siteCards[1].site.InAlarmCount).toEqual(3);
      expect(vm.siteCards[1].site.Color).toEqual(goodColor);
      expect(vm.siteCards[1].isOpen).toEqual(false);
      expect(typeof vm.siteCards[1].fetchTeamData).toBe("function");
    });

    it('should update adherence for site when clearing selection', function () {
      $fakeBackend.withSiteAdherence({
        Id: "londonGuid",
        SkillId: "phoneId",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['phoneId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: "londonGuid",
            SkillId: "phoneId",
            Name: "London",
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: "good"
          })
      })
        .wait(5000);
      c.apply(function () {
        vm.skillIds = [];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: "londonGuid",
            Name: "London",
            AgentsCount: 11,
            InAlarmCount: 9,
            Color: "danger"
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual("londonGuid");
      expect(vm.siteCards[0].site.Name).toEqual("London");
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(9);
      expect(vm.siteCards[0].site.Color).toEqual(dangerColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
    });

    it('should stop polling when page is about to destroy', function () {
      $controllerBuilder.createController()
        .wait(5000);

      scope.$emit('$destroy');
      $interval.flush(5000);
      $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get site cards with skill id', function () {
      $fakeBackend
        .withSkill({
          Name: 'Channel Sales',
          Id: 'channelSalesGuid'
        })
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "channelSalesGuid",
          Name: "London",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        })
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(1);
      expect(vm.siteCards[0].site.Id).toEqual('parisGuid');
      expect(vm.siteCards[0].site.SkillId).toEqual('channelSalesGuid');
    });

    it('should get site cards with skill area id', function () {
      $fakeBackend
        .withSkillAreas([{
          Id: "5",
          Name: "my skill area 1",
          Skills: skills1
        }])
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "1",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        });
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(1);
      expect(vm.siteCards[0].site.Id).toEqual('parisGuid');
      expect(vm.siteCards[0].site.SkillId).toEqual('1');
    });

    it('should get site cards after clearing filter input', function () {
      stateParams.skillIds = 'channelSalesGuid';
      $fakeBackend
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "channelSalesGuid",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        });
      vm = $controllerBuilder.createController().vm;
      $httpBackend.flush();

      vm.filterOutput(undefined);
      $fakeBackend
        .clearSiteAdherences()
        .withSiteAdherence({
          Id: "parisGuid",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: "good"
        })
        .withSiteAdherence({
          Id: "londonGuid",
          Name: "London",
          AgentsCount: 10,
          InAlarmCount: 8,
          Color: "danger"
        });

      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(2);
      expect(vm.siteCards[0].site.Id).toEqual('parisGuid');
      expect(vm.siteCards[0].site.Name).toEqual('Paris');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(5);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[1].site.Id).toEqual('londonGuid');
      expect(vm.siteCards[1].site.Name).toEqual('London');
      expect(vm.siteCards[1].site.AgentsCount).toEqual(10);
      expect(vm.siteCards[1].site.InAlarmCount).toEqual(8);
      expect(vm.siteCards[1].site.Color).toEqual(dangerColor);
    });

    it('should go to sites by skill state', function () {
      $fakeBackend
        .withSkill({
          Name: 'Channel Sales',
          Id: 'channelSalesGuid'
        })
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "channelSalesGuid",
          Name: "London",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        })
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, { skillIds: ['channelSalesGuid'], skillAreaId: undefined }, { notify: false });
    });

    it('should go to sites by skill area state', function () {
      $fakeBackend
        .withSkillAreas([{
          Id: "5",
          Name: "my skill area 1",
          Skills: skills1
        }])
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "1",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        });
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, { skillAreaId: '5', skillIds: undefined }, { notify: false });
    });

    it('should go to sites with skill when changing selection from skill area to skill', function () {
      stateParams.skillAreaId = "skillAreaId";
      $fakeBackend
        .withSkill({
          Id: "phoneGuid",
          Name: "Phone"
        })
        .withSiteAdherence({
          Id: "londonGuid",
          SkillId: "phoneGuid",
          Name: "London",
          AgentsCount: 10,
          InAlarmCount: 1,
          Color: "good"
        });

      var c = $controllerBuilder.createController();
      vm = c.vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: undefined,
        skillIds: ['phoneGuid']
      },
        { notify: false });
    });

    it('should go to sites with skill area when changing selection from skill to skill area', function () {
      stateParams.skillIds = ['phoneGuid'];
      $fakeBackend
        .withSkillAreas([{
          Id: "5",
          Name: "my skill area 1",
          Skills: skills1
        }])
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "1",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        });
      vm = $controllerBuilder.createController().vm

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: "5",
        skillIds: undefined
      },
        { notify: false });
    });

    it('should clear url when sending in empty input in filter', function () {
      stateParams.skillIds = ['phoneGuid'];
      $fakeBackend
        .withSkillAreas([{
          Id: "5",
          Name: "my skill area 1",
          Skills: skills1
        }])
        .withSiteAdherence({
          Id: "parisGuid",
          SkillId: "1",
          Name: "Paris",
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: "good"
        });
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(undefined);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: undefined,
        skillIds: undefined
      },
        { notify: false });
    });

  });

});

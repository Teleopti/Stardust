'use strict';
fdescribe('RtaMainController', function () {
  var
    $httpBackend,
    $fakeBackend,
    $controllerBuilder,
    $state,
    scope,
    vm;

  var skills1, skills2;
  var stateParams = {};

  beforeEach(module('wfm.rta'));


  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        stateParams = {};
        return stateParams;
      });
    });
  });

  beforeEach(inject(function (_$httpBackend_, _FakeRtaBackend_, _ControllerBuilder_) {
    $httpBackend = _$httpBackend_;
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
    ]

    $fakeBackend.clear();
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
      expect(vm.siteCards[0].site.Color).toEqual("warning");
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
      expect(vm.siteCards[0].site.Color).toEqual("good");
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe("function");
    });

  });

});

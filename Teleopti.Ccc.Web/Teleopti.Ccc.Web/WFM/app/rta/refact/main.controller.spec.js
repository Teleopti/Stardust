'use strict';

describe('RtaMainController', function () {
  var
    $controllerBuilder,
    $fakeBackend,
    $httpBackend,
    $interval,
    $state;

  var
    stateParams,
    scope,
    vm;

  var
    channelSales,
    phone,
    invoice,
    bts;

  var
    skills1,
    skills2,
    allSkills;

  var
    skillArea1,
    skillArea2;

  var skillAreas;

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

  beforeEach(function () {
    module(function ($provide) {
      $provide.value('skills', function () {
        return allSkills;
      });
    });
  });

  beforeEach(function () {
    module(function ($provide) {
      $provide.value('skillAreas', function () {
        return skillAreas;
      });
    });
  });

  beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$httpBackend_, _$interval_, _$state_) {
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;
    $httpBackend = _$httpBackend_;
    $interval = _$interval_;
    $state = _$state_;

    scope = $controllerBuilder.setup('RtaMainController');

    channelSales = {
      Name: 'Channel Sales',
      Id: 'channelSalesId'
    };

    phone = {
      Name: 'Phone',
      Id: 'phoneId'
    };

    invoice = {
      Name: 'Invoice',
      Id: 'invoiceId'
    };

    bts = {
      Name: 'BTS',
      Id: 'btsId'
    };

    skills1 = [channelSales, phone];
    skills2 = [invoice, bts];
    allSkills = [channelSales, phone, invoice, bts];

    skillArea1 = {
      Id: 'skillArea1Id',
      Name: 'SkillArea1',
      Skills: skills1
    };
    skillArea2 = {
      Id: 'skillArea2Id',
      Name: 'SkillArea2',
      Skills: skills2
    };
    skillAreas = [skillArea1, skillArea2];

    $fakeBackend.clear();
    spyOn($state, 'go');
    $state.current.name = 'rta-refact';
  }));

  describe('RtaFilterComponent handling', function () {
    it('should get skills', function () {
      vm = $controllerBuilder.createController(allSkills).vm;

      expect(vm.skills[0].Name).toEqual('Channel Sales');
      expect(vm.skills[0].Id).toEqual('channelSalesId');
      expect(vm.skills[1].Name).toEqual('Phone');
      expect(vm.skills[1].Id).toEqual('phoneId');
      expect(vm.skills[2].Name).toEqual('Invoice');
      expect(vm.skills[2].Id).toEqual('invoiceId');
      expect(vm.skills[3].Name).toEqual('BTS');
      expect(vm.skills[3].Id).toEqual('btsId');
    });

    it('should get skill areas', function () {
      vm = $controllerBuilder.createController(undefined, skillAreas).vm;

      expect(vm.skillAreas.length).toEqual(2);
      expect(vm.skillAreas[0].Id).toEqual('skillArea1Id');
      expect(vm.skillAreas[0].Name).toEqual('SkillArea1');
      expect(vm.skillAreas[0].Skills[0].Id).toEqual('channelSalesId');
      expect(vm.skillAreas[0].Skills[0].Name).toEqual('Channel Sales');
      expect(vm.skillAreas[0].Skills[1].Id).toEqual('phoneId');
      expect(vm.skillAreas[0].Skills[1].Name).toEqual('Phone');
      expect(vm.skillAreas[1].Id).toEqual('skillArea2Id');
      expect(vm.skillAreas[1].Name).toEqual('SkillArea2');
      expect(vm.skillAreas[1].Skills[0].Id).toEqual('invoiceId');
      expect(vm.skillAreas[1].Skills[0].Name).toEqual('Invoice');
      expect(vm.skillAreas[1].Skills[1].Id).toEqual('btsId');
      expect(vm.skillAreas[1].Skills[1].Name).toEqual('BTS');
    });

  });

  describe('RtaOverviewComponent handling', function () {

    it('should build site card view model', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: 'warning'
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.siteCards[0].site.Id).toEqual('londonId');
      expect(vm.siteCards[0].site.Name).toEqual('London');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(5);
      expect(vm.siteCards[0].site.Color).toEqual(warningColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe('function');
    });

    it('should build site card view model when open in url is true', function () {
      stateParams.open = 'true';
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: 'warning'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.siteCards[0].teams.length).toEqual(1);
      expect(vm.siteCards[0].teams[0].SiteId).toEqual('londonId');
      expect(vm.siteCards[0].teams[0].Id).toEqual('greenId');
    });

    it('should build site card view model with skill ids when open in url is true', function () {
      stateParams.open = 'true';
      stateParams.skillIds = ['phoneId'];

      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 5,
          SkillId: 'phoneId',
          Color: 'warning'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          SkillId: 'phoneId',
          Id: 'greenId'
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.siteCards[0].teams.length).toEqual(1);
      expect(vm.siteCards[0].teams[0].SiteId).toEqual('londonId');
      expect(vm.siteCards[0].teams[0].Id).toEqual('greenId');
    });

    it('should update adherence for teams when open in url is true', function () {
      stateParams.open = 'true';
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: 'warning'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId',
          InAlarmCount: 2,
          Color: 'good'
        });

      var c = $controllerBuilder.createController();
      vm = c.vm;
      c.apply(function () {
        $fakeBackend
          .clearTeamAdherences()
          .withTeamAdherence({
            SiteId: 'londonId',
            Id: 'greenId',
            InAlarmCount: 6,
            Color: 'warning'
          });
      })
        .wait(5000);

      expect(vm.siteCards[0].teams[0].InAlarmCount).toEqual(6);
      expect(vm.siteCards[0].teams[0].Color).toEqual('warning');
    });

    it('should build site card view model when preselected skill', function () {
      stateParams.skillIds = ['phoneId'];
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          SkillId: 'phoneId',
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: 'warning'
        })
        .withSiteAdherence({
          Id: 'parisId',
          Name: 'Paris',
          SkillId: 'invoiceId',
          AgentsCount: 10,
          InAlarmCount: 5,
          Color: 'warning'
        });

      vm = $controllerBuilder.createController().vm;

      expect(vm.siteCards.length).toEqual(1);
      expect(vm.siteCards[0].site.Id).toEqual('londonId');
      expect(vm.siteCards[0].site.Name).toEqual('London');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(5);
      expect(vm.siteCards[0].site.Color).toEqual(warningColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe('function');
    });

    it('should build site card view model when preselected skill area', function () {
      stateParams.skillAreaId = 'skillArea1Id';
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'channelSalesId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });

      vm = $controllerBuilder.createController(undefined, skillAreas).vm;

      expect(vm.siteCards[0].site.Id).toEqual('londonId');
      expect(vm.siteCards[0].site.Name).toEqual('London');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe('function');
    });

    it('should update adherence', function () {
      $fakeBackend.withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

      var c = $controllerBuilder.createController();
      vm = c.vm;
      c.apply(function () {
        $fakeBackend.clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            Name: 'London',
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: 'good'
          });
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual('londonId');
      expect(vm.siteCards[0].site.Name).toEqual('London');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe('function');
    });

    it('should update adherence for site with skill', function () {
      $fakeBackend.withSiteAdherence({
        Id: 'londonId',
        SkillId: 'phoneId',
        InAlarmCount: 5,
        Color: 'warning'
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['phoneId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            SkillId: 'phoneId',
            InAlarmCount: 2,
            Color: 'good'
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
    });

    it('should update adherence for site with skill area', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'channelSalesId',
          InAlarmCount: 5,
          Color: 'warning'
        })
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'phoneId',
          InAlarmCount: 4,
          Color: 'warning'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['channelSalesId', 'phoneId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            SkillId: 'channelSalesId',
            InAlarmCount: 2,
            Color: 'good'
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
    });

    it('should update adherence for site with preselected skill', function () {
      stateParams.skillIds = ['channelSalesId'];
      $fakeBackend.withSiteAdherence({
        Id: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 5,
        Color: 'warning'
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            SkillId: 'channelSalesId',
            InAlarmCount: 2,
            Color: 'good'
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
    });

    it('should update adherence for site with preselected skill area', function () {
      stateParams.skillAreaId = 'skillArea1Id';
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'channelSalesId',
          InAlarmCount: 5,
          Color: 'warning'
        })
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'phoneId',
          InAlarmCount: 4,
          Color: 'warning'
        });
      var c = $controllerBuilder.createController(undefined, skillAreas);
      vm = c.vm;

      c.apply(function () {
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            SkillId: 'channelSalesId',
            InAlarmCount: 2,
            Color: 'good'
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.InAlarmCount).toEqual(2);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
    });

    it('should update adherence for site when clearing selection', function () {
      $fakeBackend.withSiteAdherence({
        Id: 'londonId',
        SkillId: 'phoneId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.skillIds = ['phoneId'];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            SkillId: 'phoneId',
            Name: 'London',
            AgentsCount: 11,
            InAlarmCount: 2,
            Color: 'good'
          })
      })
        .wait(5000);
      c.apply(function () {
        vm.skillIds = [];
        $fakeBackend
          .clearSiteAdherences()
          .withSiteAdherence({
            Id: 'londonId',
            Name: 'London',
            AgentsCount: 11,
            InAlarmCount: 9,
            Color: 'danger'
          })
      })
        .wait(5000);

      expect(vm.siteCards[0].site.Id).toEqual('londonId');
      expect(vm.siteCards[0].site.Name).toEqual('London');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(9);
      expect(vm.siteCards[0].site.Color).toEqual(dangerColor);
      expect(vm.siteCards[0].isOpen).toEqual(false);
      expect(typeof vm.siteCards[0].fetchTeamData).toBe('function');
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
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId',
          Name: 'London',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        })
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(1);
      expect(vm.siteCards[0].site.Id).toEqual('parisId');
      expect(vm.siteCards[0].site.SkillId).toEqual('channelSalesId');
    });

    it('should get site cards with skill area id', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId',
          Name: 'Paris',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });
      vm = $controllerBuilder.createController(undefined, skillAreas).vm;

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(1);
      expect(vm.siteCards[0].site.Id).toEqual('parisId');
      expect(vm.siteCards[0].site.SkillId).toEqual('channelSalesId');
    });

    it('should get site cards after clearing filter input', function () {
      stateParams.skillIds = 'channelSalesGuid';
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId',
          Name: 'Paris',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(undefined);
      $fakeBackend
        .clearSiteAdherences()
        .withSiteAdherence({
          Id: 'parisId',
          Name: 'Paris',
          AgentsCount: 11,
          InAlarmCount: 5,
          Color: 'good'
        })
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London',
          AgentsCount: 10,
          InAlarmCount: 8,
          Color: 'danger'
        });
      $httpBackend.flush();

      expect(vm.siteCards.length).toEqual(2);
      expect(vm.siteCards[0].site.Id).toEqual('parisId');
      expect(vm.siteCards[0].site.Name).toEqual('Paris');
      expect(vm.siteCards[0].site.AgentsCount).toEqual(11);
      expect(vm.siteCards[0].site.InAlarmCount).toEqual(5);
      expect(vm.siteCards[0].site.Color).toEqual(goodColor);
      expect(vm.siteCards[1].site.Id).toEqual('londonId');
      expect(vm.siteCards[1].site.Name).toEqual('London');
      expect(vm.siteCards[1].site.AgentsCount).toEqual(10);
      expect(vm.siteCards[1].site.InAlarmCount).toEqual(8);
      expect(vm.siteCards[1].site.Color).toEqual(dangerColor);
    });

  });
});

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

  describe('Redirects', function () {

    it('should go to sites by skill state', function () {
      vm = $controllerBuilder.createController(allSkills).vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, { skillIds: ['channelSalesId'], skillAreaId: undefined }, { notify: false });
    });

    it('should go to sites by skill area state', function () {
      vm = $controllerBuilder.createController(undefined, skillAreas).vm;

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, { skillAreaId: 'skillArea1Id', skillIds: undefined }, { notify: false });
    });

    it('should go to sites with skill when changing selection from skill area to skill', function () {
      stateParams.skillAreaId = 'skillArea1Id';
      var c = $controllerBuilder.createController(allSkills, skillAreas);
      vm = c.vm;

      vm.filterOutput(vm.skills[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: undefined,
        skillIds: ['channelSalesId']
      },
        { notify: false });
    });

    it('should go to sites with skill area when changing selection from skill to skill area', function () {
      stateParams.skillIds = ['channelSalesId'];
      vm = $controllerBuilder.createController(allSkills, skillAreas).vm

      vm.filterOutput(vm.skillAreas[0]);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: 'skillArea1Id',
        skillIds: undefined
      },
        { notify: false });
    });

    it('should clear url when sending in empty input in filter', function () {
      stateParams.skillIds = ['channelSalesId'];
      vm = $controllerBuilder.createController().vm;

      vm.filterOutput(undefined);
      $httpBackend.flush();

      expect($state.go).toHaveBeenCalledWith($state.current.name, {
        skillAreaId: undefined,
        skillIds: undefined
      },
        { notify: false });
    });

    it('should go to agents for selected site', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          Name: 'Paris',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for the right selected site after deselecting some other', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId'
        })
        .withSiteAdherence({
          Id: 'londonId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.getSelectedItems(vm.siteCards[1]);
        vm.getSelectedItems(vm.siteCards[1]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for selected site with selected skill', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;
      vm.filterOutput(channelSales);
      $httpBackend.flush();

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: ['channelSalesId'], skillAreaId: undefined });
    });

    it('should go to agents for selected site with preselected skill', function () {
      stateParams.skillIds = ['channelSalesId'];
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: ['channelSalesId'], skillAreaId: undefined });
    });


    it('should go to agents for selected site after clearing skill selection', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;
      vm.filterOutput(channelSales);
      $httpBackend.flush();

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.filterOutput();
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for selected site after clearing skill selection when skill was preselected', function () {
      stateParams.skillIds = ['channelSalesId'];
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      vm.filterOutput();
      $httpBackend.flush();

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for selected site with selected skill area', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'phoneId'
        });
      var c = $controllerBuilder.createController(undefined, skillAreas);
      vm = c.vm;
      vm.filterOutput(skillArea1);
      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: 'skillArea1Id' });
    });

    it('should go to agents for selected site with preselected skill area', function () {
      stateParams.skillAreaId = 'skillArea1Id';
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'phoneId'
        });
      var c = $controllerBuilder.createController(undefined, skillAreas);
      vm = c.vm;

      c.apply(function () {
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: 'skillArea1Id' });
    });

    it('should reset selected sites when changing skill', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'phoneId'
        });

      var c = $controllerBuilder.createController(skills1);
      vm = c.vm;

      c.apply(function () {
        vm.filterOutput(skills1[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0]);
        vm.filterOutput(skills1[1]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: ['londonId'], teamIds: [], skillIds: ['phoneId'], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['londonId'], teamIds: [], skillIds: ['phoneId'], skillAreaId: undefined });
    });

    it('should go to agents for selected team', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for selected team with selected skill', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'channelSalesId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          SkillId: 'channelSalesId',
          Id: 'greenId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.filterOutput(channelSales);
        $httpBackend.flush();
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: ['channelSalesId'], skillAreaId: undefined });
    });

    it('should go to agents for selected team with preselected skill', function () {
      stateParams.skillIds = ['channelSalesId'];
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          SkillId: 'channelSalesId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          SkillId: 'channelSalesId',
          Id: 'greenId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: ['channelSalesId'], skillAreaId: undefined });
    });

    it('should go to agents for selected team with selected skill area', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'channelSalesId',
          Id: 'greenId'
        })
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'phoneId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'phoneId',
          Id: 'greenId'
        })
        ;
      var c = $controllerBuilder.createController(undefined, skillAreas);
      vm = c.vm;
      c.apply(function () {
        vm.filterOutput(skillArea1);
        $httpBackend.flush();
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: 'skillArea1Id' });
    });

    it('should go to agents for selected team with preselected skill area', function () {
      stateParams.skillAreaId = 'skillArea1Id';
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'channelSalesId',
          Id: 'greenId'
        })
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'phoneId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'phoneId',
          Id: 'greenId'
        })
        ;
      var c = $controllerBuilder.createController(undefined, skillAreas);
      vm = c.vm;
      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });

      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: 'skillArea1Id' });
    });

    it('should reset selected teams when changing skill', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'channelSalesId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'channelSalesId',
          Id: 'greenId'
        })
        .withSiteAdherence({
          Id: 'parisId',
          SkillId: 'phoneId'
        })
        .withTeamAdherence({
          SiteId: 'parisId',
          SkillId: 'phoneId',
          Id: 'redId'
        });

      var c = $controllerBuilder.createController(skills1);
      vm = c.vm;

      c.apply(function () {
        vm.filterOutput(skills1[0]);
        $httpBackend.flush();
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.filterOutput(skills1[1]);
        $httpBackend.flush();
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: [], teamIds: ['redId'], skillIds: ['phoneId'], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['redId'], skillIds: ['phoneId'], skillAreaId: undefined });
    });

    it('should go to agents for site if all teams under it are selected one by one', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'redId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.siteCards[0].isSelected = true; //site gets to be selected in component, don't know how to set it in controller test
        vm.getSelectedItems(vm.siteCards[0].teams[1]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for teams if all teams under a site were selected and then one was unselected', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'redId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.getSelectedItems(vm.siteCards[0].teams[1]);
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: [], teamIds: ['redId'], skillIds: [], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['redId'], skillIds: [], skillAreaId: undefined });
    });

    it('should go to agents for teams if site were selected and then one was unselected', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'redId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0]);
        vm.getSelectedItems(vm.siteCards[0].teams[1]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
    });

    it('should always clear teams selection for teams under site if site is selected', function () {
      $fakeBackend
        .withSiteAdherence({
          Id: 'londonId',
          Name: 'London'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId'
        })
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'redId'
        });
      var c = $controllerBuilder.createController();
      vm = c.vm;

      c.apply(function () {
        vm.siteCards[0].isOpen = true;
        vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
        $httpBackend.flush();
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.getSelectedItems(vm.siteCards[0].teams[1]);
        vm.getSelectedItems(vm.siteCards[0].teams[0]);
        vm.getSelectedItems(vm.siteCards[0]);
        vm.goToAgents();
      });

      expect(vm.selectedItems).toEqual({ siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
      expect($state.go).toHaveBeenCalledWith('rta.agents', { siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
    });

  });

  it('should select teams under site when site is not selected and team is', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'redId'
      });
    var c = $controllerBuilder.createController();
    vm = c.vm;

    c.apply(function () {
      vm.siteCards[0].isOpen = true;
      vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
      $httpBackend.flush();
      vm.getSelectedItems(vm.siteCards[0].teams[0]);
      vm.siteCards[0].isOpen = false;
      vm.siteCards[0].isOpen = true;
      vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
      $httpBackend.flush();

    });

    expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
  });

  it('should select all teams under site when site is selected and initially closed', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController()
    vm = c.vm;

    c.apply(function () {
      vm.getSelectedItems(vm.siteCards[0]);
      vm.siteCards[0].isOpen = true;
      vm.siteCards[0].fetchTeamData(vm.siteCards[0]);
      $httpBackend.flush();
    });

    expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
  });

});

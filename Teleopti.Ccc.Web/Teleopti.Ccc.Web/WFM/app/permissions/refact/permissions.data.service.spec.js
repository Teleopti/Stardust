'use strict';
//DONT REMOVE X
xdescribe('permissionsDataService', function () {
  var permissionsDataService;

  var role = {
    BuiltIn: false,
    DescriptionText: 'Agent',
    Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
    IsAnyBuiltIn: true,
    IsMyRole: false,
    Name: 'Agent'
  };
  // ChildNodes: [
  //   {
  //     Id: 'fe113bc0-979a-4b6c-9e7c-ef601c7e02d1',
  //     Type: 'Site',
  //     Name: 'Site1',
  //     IsSelected: true,
  //     ChildNodes: [
  //       {
  //         Id: 'e6377d56-277d-4c22-97f3-b218741b2480',
  //         Type: 'Team',
  //         Name: 'Team1',
  //         IsSelected: true
  //       }
  //     ]
  //   }
  // ],
  var BusinessUnit = {
    ChildNodes: [
      {
        Id: 'fe113bc0-979a-4b6c-9e7c-ef601c7e02d1',
        Type: 'Site',
        Name: 'Site1',
        IsSelected: true,
        ChildNodes: [
          {
            Id: 'e6377d56-277d-4c22-97f3-b218741b2480',
            Type: 'Team',
            Name: 'Team1',
            IsSelected: true
          }
        ]
      }
    ],
    Id: '928dd0bc-bf40-412e-b970-9b5e015aadea',
    Name: 'TeleoptiCCCDemo',
    Type: 'BusinessUnit',
    IsSelected: true
  };


  beforeEach(function () {
    module('wfm.permissions');
  });

  beforeEach(inject(function (_permissionsDataService_) {
    permissionsDataService = _permissionsDataService_;
  }));

  it('should prepare data for sending to server', function(){
    permissionsDataService.setSelectedRole(role);

    var data = permissionsDataService.prepareData(BusinessUnit);

    expect(data.Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
    expect(data.BusinessUnits[0]).toEqual('928dd0bc-bf40-412e-b970-9b5e015aadea');
    expect(data.Sites[0]).toEqual('fe113bc0-979a-4b6c-9e7c-ef601c7e02d1');
    expect(data.Teams[0]).toEqual('e6377d56-277d-4c22-97f3-b218741b2480');
  });
});

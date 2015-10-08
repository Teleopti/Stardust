'use strict';
describe('RtaOrganizationService', function () {
    var $q,
        $rootScope,
        $filter;

    var rtaSvrc = {
        getSites: {
            query: function () {
                var queryDeferred = $q.defer();
                queryDeferred.resolve([
                    {
                        "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                        "Name": "London",
                        "NumberOfAgents": 46
                    }
                ]);
                return { $promise: queryDeferred.promise };
            }
        },

        getAgents: {
          query: function () {
              var queryDeferred = $q.defer();
              queryDeferred.resolve([
                  {
                      "Name": "Julian Feldman",
                      "PersonId": "cb67d5f1-5dd1-45af-b4e2-9b5e015b2572",
                      "SiteId": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                      "SiteName": "London",
                      "TeamId": "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
                      "TeamName": "Students"
                  }
              ]);
              return { $promise: queryDeferred.promise };
          }
        },

        getTeamsForSelectedSites: {
          query: function () {
              var queryDeferred = $q.defer();
              queryDeferred.resolve([
                  {
                      "Id": "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
                      "Name": "Students",
                      "NumberOfAgents": 7,
                      "SiteId": "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
                  },
                  {
                    "Id": "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
                    "Name": "Team 1",
                    "NumberOfAgents": 11,
                    "SiteId": "6a21c802-7a34-4917-8dfd-9b5e015ab461"
                  }
              ]);
              return { $promise: queryDeferred.promise };
          }
        },

        getAdherenceForAllSites: {
            query: function () {
                var queryDeferred = $q.defer();
                queryDeferred.resolve([]);
                return { $promise: queryDeferred.promise };
            }
        }
    };

    beforeEach(function () {
        module('wfm.rta');
        module(function ($provide) {
            $provide.service('RtaService', function () { return rtaSvrc; });
        });
    });

    beforeEach(inject(function( _$q_,  _$rootScope_, _$filter_) {
        $q = _$q_;
        $rootScope = _$rootScope_;
        $filter = _$filter_;
        }));

    it('should get all the sites from the RtaService ', function (done) {
        inject(function (RtaOrganizationService) {

            var scope = $rootScope.$new();
            var sites = RtaOrganizationService.getSites();

            sites.$promise
                .then(function(result){
                    expect(result.length).not.toBe(0);
                    expect(result[0].Name).toBe('London');
                    done();
                });

            scope.$digest();

        });
    });

    it('should get all the teams', function (done) {
        inject(function (RtaOrganizationService) {

            var scope = $rootScope.$new();
            var siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
            var teams = RtaOrganizationService.getTeams(siteId);

            teams.$promise
                .then(function(result){
                    expect(result.length).not.toBe(0);
                    expect(result[0].Name).toBe('Students');

                    done();
                });

            scope.$digest();
        });
    });

    it('should get all the agents', function (done) {
       inject(function (RtaOrganizationService) {

           var scope = $rootScope.$new();
           var teamId = "e5f968d7-6f6d-407c-81d5-9b5e015ab495";
           var agents = RtaOrganizationService.getAgents(teamId);

           agents.$promise
               .then(function(result){
                   expect(result.length).not.toBe(0);
                   expect(result[0].Name).toBe('Julian Feldman');
                   expect(result[0].TeamName).toBe('Students');
                   expect(result[0].SiteName).toBe('London');
                   done();
               });

           scope.$digest();
       }) ;
    });

    it('should get all the teams for the selected sites', function (done) {
       inject(function (RtaOrganizationService) {
           var scope = $rootScope.$new();
           var siteIds = ['d970a45a-90ff-4111-bfe1-9b5e015ab45c','6a21c802-7a34-4917-8dfd-9b5e015ab461'];
           var teams = RtaOrganizationService.getTeams(siteIds);

           teams.$promise
               .then(function(result){
               	expect(result.length).not.toBe(0);
                expect(result[0].Id).toBe("e5f968d7-6f6d-407c-81d5-9b5e015ab495");
                expect(result[1].Id).toBe("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");

                   done();
               });

           scope.$digest();
       });
    });

    it('should get the correct site name from the site id', function (done) {
      inject(function (RtaOrganizationService) {
        var scope = $rootScope.$new();
        var siteIds = ['d970a45a-90ff-4111-bfe1-9b5e015ab45c'];
        var siteNamePromise = RtaOrganizationService.getSiteName(siteIds);

          siteNamePromise
          .then(function(result){
            expect(result).toEqual('London');
            done();
          });
          scope.$digest();
      });
    });
});

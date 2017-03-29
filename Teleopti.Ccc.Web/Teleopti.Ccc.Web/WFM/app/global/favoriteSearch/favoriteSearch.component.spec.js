describe('Favorite Search Component', function () {
	'use strict';

	var $componentController,
	    rootScope,
	    $compile,
	    favoriteSearchDataService,
	    httpBackend,
	    scope;


	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.favoriteSearch'));

	beforeEach(function () {
		favoriteSearchDataService = new FakeFavoriteSearchDataService();

		module(function ($provide) {
			$provide.service('FavoriteSearchDataService',
				function () {
					return favoriteSearchDataService;
				});
		});
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_, _$httpBackend_) {
		$componentController = _$componentController_;
		rootScope = _$rootScope_;
		$compile = _$compile_;
		httpBackend = _$httpBackend_;

		httpBackend.expectGET('../api/FavoriteSearch/GetPermission').respond(200, '');

		scope = rootScope.$new();
	}));

	it('should populate favorite list', function () {
		scope.getSearch = function () { };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();

		var ctrl = element.isolateScope().$ctrl;

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[0].Id).toEqual("1");
		expect(ctrl.favoriteSearchList[1].Id).toEqual("2");
		expect(ctrl.currentName).toEqual('test1');
	});

	it('should save new favorite', function () {
		scope.getSearch = function () { return { TeamIds: ['team1'], SearchTerm: 'agent' }; };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;

		ctrl.currentName = 'myNewSearch';
		ctrl.save();

		expect(ctrl.favoriteSearchList.length).toEqual(3);
		expect(ctrl.favoriteSearchList[0].Name).toEqual("myNewSearch");
		expect(ctrl.favoriteSearchList[0].TeamIds[0]).toEqual("team1");
		expect(ctrl.favoriteSearchList[0].SearchTerm).toEqual("agent");
	});

	it('should update current favorite search', function () {
		scope.getSearch = function () {
			return { TeamIds: ['fakeTeamId'], SearchTerm: 'agent' };
		};
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		ctrl.isTest = true;
		ctrl.currentName = 'test1';
		ctrl.save();

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[0].TeamIds[0]).toEqual("fakeTeamId");
		expect(ctrl.favoriteSearchList[0].SearchTerm).toEqual("agent");
	});

	it('should make specific favorite default', function () {
		scope.getSearch = function () { return { TeamIds: ["fakeTeamId"], SearchTerm: "agent" }; };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		var newDefaultName = 'test2';
		ctrl.toggleDefault(newDefaultName);

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[0].IsDefault).toEqual(false);
		expect(ctrl.favoriteSearchList[0].Name).toEqual('test1');
		expect(ctrl.favoriteSearchList[1].IsDefault).toEqual(true);
	});


	it('should delete specific favorite', function () {
		scope.getSearch = function () { };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		var nameToBeDeleted = 'test1';
		ctrl.delete(nameToBeDeleted);

		expect(ctrl.favoriteSearchList.length).toEqual(1);
		expect(ctrl.favoriteSearchList[0].Id).toEqual('2');
	});

	it('should reset current favorite when its deleted', function () {
		scope.getSearch = function () { return {TeamIds:['team1', 'team2'],SearchTerm:""}};
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		ctrl.isTest = true;
		ctrl.select({
			Id: '1',
			Name: 'test1',
			TeamIds: ['Team1Id', 'Team2Id'],
			SearchTerm: '',
			IsDefault: true
		});
		var nameToBeDeleted = 'test1';
		ctrl.delete(nameToBeDeleted);

		expect(ctrl.currentName).toEqual('');
		expect(ctrl.enableSave()).toEqual(false);
		expect(ctrl.currentFavorite).toEqual(false);
	});

	it('should clear input when current favorite is cleared', function () {
		scope.getSearch = function () { return { TeamIds: ['team1', 'team2'], SearchTerm: "" } };
		scope.applyFavorite = function () { };
		scope.selectedFavorite = {
			Name: 'test',
			TeamIds: [],
			SearchTerm: 'ash'
		}

		var element = angular.element('<favorite-search current-favorite="selectedFavorite"  get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);

		scope.$digest();
		var ctrl = element.isolateScope().$ctrl;

		expect(ctrl.currentName).toEqual('test');

		scope.selectedFavorite = undefined;
		scope.$digest();

		expect(ctrl.currentName).toEqual('');

	});

	function FakeFavoriteSearchDataService() {
		var fakeData = [
			{
				Id: '1',
				Name: 'test1',
				TeamIds: ['Team1Id', 'Team2Id'],
				SearchTerm: '',
				IsDefault: true
			},
			{
				Id: '2',
				Name: 'test2',
				TeamIds: ['Team1Id', 'Team2Id'],
				SearchTerm: '',
				IsDefault: false
			}
		];

		this.getFavoriteSearchList = function () {
			return {
				then: function (cb) { cb({ data: fakeData }); }
			}
		}

		this.add = function (currentName, currentSearch) {
			var result = {
				Id: 'id',
				Name: currentName,
				TeamIds: currentSearch.TeamIds,
				SearchTerm: currentSearch.SearchTerm,
				IsDefault: false
			};
			return {
				then: function (cb) { cb({ data: result }); }
			}
		}

		this.update = function () {
			return { then: function (cb) { cb(); } };
		}

		this.delete = function () {
			return { then: function (cb) { cb(); } };
		}

		this.changeDefault = function() {
			return { then: function (cb) { cb(); } };
		};

		this.getPermission = function() {
			return { then: function (cb) { cb({data: true}); } };
		};

		this.hasPermission = function(){
			return { then: function(cb){
				cb({data: true});
			}}
		};

		this.initPermission = function(){

		};
	}

});
(function() {
  'use strict';

  angular
    .module('skillGroupService', ['ngResource'])
    .service('SkillGroupSvc', SkillGroupSvc);

	SkillGroupSvc.$inject = ['$http'];
	
  function SkillGroupSvc($http) {
    var self = this;
    self.getSkills        = getSkills;
    self.createSkillGroup = createSkillGroup;
    self.getSkillGroups   = getSkillGroups;
    self.deleteSkillGroup = deleteSkillGroup;
    self.modifySkillGroup = modifySkillGroup;
    
    function getSkills() {
      return $http.get('../api/intraday/skills');
    };

    function createSkillGroup(name, skills) {
      return $http.post('../api/skillgroup/create', {
        Name: name,
        Skills: skills
      });
    };

    function getSkillGroups() {
      return $http.get('../api/skillgroup/skillgroups');
    };

    function deleteSkillGroup(skillArea) {
      return $http.delete('../api/skillgroup/delete/' + skillArea.id);
    };

    function modifySkillGroup(data) {
      if (data.currentSkillGroup) {
        return $http.put('../api/skillgroup/update', data.currentSkillGroup);
      }
    };
  }
})();
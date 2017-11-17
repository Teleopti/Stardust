(function() {
  'use strict';

  angular
    .module('skillGroupService', ['ngResource'])
    .service('SkillGroupSvc', SkillGroupSvc);

	SkillGroupSvc.$inject = ['$http'];
	
  function SkillGroupSvc($http) {
    var self = this;
    self.getSkills         = getSkills;
    self.createSkillGroup  = createSkillGroup;
    self.getSkillGroups    = getSkillGroups;
    self.deleteSkillGroup  = deleteSkillGroup;
    self.modifySkillGroups = modifySkillGroups;
    
    function getSkills() {
      return $http.get('../api/intraday/skills');
    };

    function createSkillGroup(skillgroup) {
      return $http.post('../api/skillgroup/create', skillgroup);
    };

    function getSkillGroups() {
      return $http.get('../api/skillgroup/skillgroups');
    };

    function deleteSkillGroup(skillGroup) {
      return $http.delete('../api/skillgroup/delete/' + skillGroup.Id);
    };

    function modifySkillGroups(skillGroups) {
      if (skillGroups) {
        return $http.put('../api/skillgroup/update', skillGroups);
      }
		};
  }
})();
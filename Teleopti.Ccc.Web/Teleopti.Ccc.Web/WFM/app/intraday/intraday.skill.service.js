(function() {
  "use strict";
  angular.module("wfm.intraday").service("intradaySkillService", [
    "$resource",
    function($resource) {
      var skillItem;

      var setSkill = function(item) {
        skillItem = item;
      };

      var getSkill = function() {
        return skillItem;
      };

      return {
        setSkill: setSkill,
        getSkill: getSkill
      };
    }
  ]);
})();

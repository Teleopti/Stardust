(function () {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.controller('skillPrioController', skillPrioController);

	skillPrioController.$inject = ['$stateParams', 'Toggle', '$filter', 'NoticeService', '$translate', '$q', 'skillPrioService'];
	function skillPrioController($stateParams, toggleService, $filter, NoticeService, $translate, $q, skillPrioService) {
		var vm = this;
		vm.selectedActivity = 'Backoffice';
		vm.sortedSkills = [];
		vm.activitys = getActivitys();
		vm.skills = getSkills();
		vm.addSkill = addSkill;
		vm.removeSkill = removeSkill;
		vm.noSortedSkills = noSortedSkills;
		vm.displayAutoComplete = displayAutoComplete;
		vm.query = query;
		vm.save = save;

		////////////////
		
		function getActivitys() {
			return skillPrioService.getMockActivitys();
		};

		function getSkills() {
			return skillPrioService.getMockSkills();
		};
		
		function removeFromUnsorted(skill) {
			vm.skills.splice(vm.skills.indexOf(skill), 1);
		}; 

		function addToUnsorted(skill){
			vm.skills.push(skill);
		};

		function removeFromSorted(skill){
			if(skill.hasParent){
				vm.sortedSkills.forEach(function(parent){
					if(parent.value === skill.value){
						skill.hasParent = false;
						parent.siblings.splice(parent.siblings.indexOf(skill), 1)
					};
				})
			}else{
				skill.siblings = new Array();
				skill.hasParent = false;
				vm.sortedSkills.splice(vm.sortedSkills.indexOf(skill), 1)
			}
			
		};

		function promoteSiblings(arr){
			arr.forEach(function(sibling){
				addSkill(sibling.skill, sibling.value, sibling.isSibling)
			});
		}

		function removeSkill(skill){
			var skillRepository = []
			if(skill.siblings.length > 0){
				skill.siblings[0].hasParent = false;
				skill.siblings.forEach(function(sibling){
					var temp = {skill: sibling, value:skill.value, isSibling:true}
					skillRepository.push(temp);
				});
			}
			removeFromSorted(skill)
			addToUnsorted(skill)
			if(skillRepository.length > 0){
				promoteSiblings(skillRepository);
			}
		}

		function query(query) {
			var results = $filter('filter')(vm.skills,query);
				return results
		};

		function addSkill(skill, prio, isSibling) {
			if(!skill || !prio) return;
			skill.value = prio;
			var isDuplicate = false;
			vm.sortedSkills.forEach(function (sortedSkill) {
				if (skill.value === sortedSkill.value) {
					skill.hasParent = true;
					sortedSkill.siblings.push(skill);
					isDuplicate = true;
				}
			});
			if (!isDuplicate) {
				vm.sortedSkills.push(skill);
			}
			if(!isSibling){
				removeFromUnsorted(skill);
			}
		};

		function displayAutoComplete(skill, position) {
			if (!skill || !position) return;
			var currentAutocompletePosition = "showAutoComplete" + position
			if (skill[currentAutocompletePosition]) {
				return skill[currentAutocompletePosition] = !skill[currentAutocompletePosition]
			} else {
				skill[currentAutocompletePosition] = true;
				return 
			}
		};

		function save(){
			NoticeService.success('All changes are saved', 5000, true);
		}

		function noSortedSkills() {
			if (vm.sortedSkills.length > 0) {
				return false
			} else {
				return true
			}
		};

	}
})();
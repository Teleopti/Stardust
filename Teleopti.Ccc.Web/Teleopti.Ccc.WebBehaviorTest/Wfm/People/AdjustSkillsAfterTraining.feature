@OnlyRunIfEnabled('WfmPeople_AdjustSkill_34138')
Feature: AdjustSkillsAfterTraining
	In order to let new employed agents work with new skills after training
	As a resource planner
	I want to assign new skills to the agents

Background: 
Given there is a site named 'London'
	And there is a team named 'Team Red' on site 'London'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	And there is a skill with
	| Field    | Value        |
	| Name     | Direct Sales |
	| Activity | Phone        |
	And there is a skill with
	| Field    | Value         |
	| Name     | Channel Sales |
	| Activity | Phone         |
	And I have a role with
	 | Field              | Value       |
	 | Name               | Administrator |
	 | Access to everyone | true        |
	 | Access to people   | true        |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team Red   |
	 | Start Date | 2015-01-21 |
	 And Ashley Andeen has a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And John Smith has a person period with
	 | Field      | Value      |
	 | Team       | Team Red      |
	 | Start Date | 2015-01-21 |
	 And John Smith has a person period with 
	| Field      | Value        |
	| Start date | 2012-06-18   |
	| Skill      | Direct Sales |
	And I have a person period with
	 | Field      | Value      |
	 | Team       | Team Red      |
	 | Start Date | 2015-01-21 |
	 And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |


Scenario: can select people
	When I view people
	And I select John in people list
	And I select Ashley in people list
	Then I should see an indicator telling me 2 person selected

@ignore
Scenario: can deselect people
	When I view people
	And I select John in people list
	And I select Ashley in people list
	And I open the command adjust skill
	And I remove 'John Smith' from my cart
	And I navigate to search view
	Then I should see 'John Smith' deselected
	And I should see an indicator telling me '1' person selected

@ignore
Scenario: can empty selection cart
	When I view people
	And I select 'John Smith' in people list
	And I select 'Ashley Andeen' in people list
	And I clear my cart
	Then I should not see the people selection indicator

@ignore
Scenario: can open adjust skill panel for selected people
	When I view people
	And I select 'John Smith' in people list
	And I open the command to adjust skill
	Then I should see adjust skill panel and 'John Smith' in my selection cart

@ignore
Scenario: can change skills for selected people
	When I view people
	And I select 'John Smith' in people list
	And I open the command to adjust skill
	Then I should see 'John Smith' has skill 'Direct Sales'
	When I edit skills from date '2015-09-01' with
	| Skill         | Has   |
	| Direct Sales  | false |
	| Channel Sales | true  |
	Then I should see from date '2015-09-01','John Smith' has skills
	| Skill         | 
	| Channel Sales | 

@ignore
Scenario: can see notification for changing shift bag after assigning skills
	When I view people
	And I select 'John Smith' in people list
	And I open the command to adjust skill
	And I edit skills from date '2015-09-01' with
	| Skill         | Has   |
	| Direct Sales  | false |
	| Channel Sales | true  |
	Then I should see a friendly notification telling me I might want to change shift bag
	And I should see the command which guides me to shift bag editing panel
	
@ignore
Scenario: can change shift bag for selected people
	When I view people
	And I select 'John Smith' in people list
	And I open the command to change shift bag
	And I assign shift bag 'Foobar' from date '2015-09-01'
	Then I should see 'John Smith' has shift bag 'Email' from date '2015-09-01'

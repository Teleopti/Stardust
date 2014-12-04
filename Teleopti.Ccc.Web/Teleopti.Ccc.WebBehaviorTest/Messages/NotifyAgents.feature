Feature: Notify agents from RTA
	In order to notify agents for urgent matters
	As a team leader
	I want to send sms to the agents from RTA

Scenario: Send message
	Given there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
	When I view real time adherence for team 'Red'
	And I choose to send a message
	And I select 
	| Person Name   |
	| Pierre Baldi  |
	| Ashley Andeen |
	And I choose to continue
	Then I should see receivers as
	| Person Name   |
	| Pierre Baldi  |
	| Ashley Andeen |
	When I input the message
	And I confirm to send the message
	Then I should see send message succeeded

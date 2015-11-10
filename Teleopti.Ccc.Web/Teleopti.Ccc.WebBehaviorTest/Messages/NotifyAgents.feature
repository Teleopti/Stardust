Feature: Notify agents from RTA
	In order to notify agents for urgent matters
	As a team leader
	I want to send sms to the agents from RTA

@OnlyRunIfEnabled('RTA_NotifyViaSMS_31567')
Scenario: Access from RTA
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
	And I select 
	| Name          |
	| Pierre Baldi  |
	| Ashley Andeen |
	And I choose to continue
	Then The message tool should be opened in a new window

@ignore
@OnlyRunIfEnabled('RTA_NotifyViaSMS_31567')
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
	When I send message for 
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |
	And I input the message
	 | Subject         | Body         |
	 | message subject | message body |
	And I confirm to send the message
	Then I should see send message succeeded
	And I should see receivers as
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |

@ignore
#It will be redirected to Anywhere with toggle 'MyTimeWeb_KeepUrlAfterLogon_34762' off
@OnlyRunIfEnabled('MyTimeWeb_KeepUrlAfterLogon_34762')
@OnlyRunIfEnabled('RTA_NotifyViaSMS_31567')
Scenario: Send message after application sign in
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
	When I (without signed in) send message for 
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |
	Then I should be redirected to the sign in page
	When I sign in
	Then I should see receivers as
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |

@ignore
#It will be redirected to Anywhere with toggle 'MyTimeWeb_KeepUrlAfterLogon_34762' off
@OnlyRunIfEnabled('MyTimeWeb_KeepUrlAfterLogon_34762')
@OnlyRunIfEnabled('RTA_NotifyViaSMS_31567')
@WindowsAsDefaultIdentityProviderLogon
Scenario: Send message after windows sign in
	Given there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	 And I am a user with
	| Field                  | Value |
	| Windows authentication | true  |
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	 And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
	When I (without signed in) send message for 
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |
	Then I should see receivers as
	 | Name          |
	 | Pierre Baldi  |
	 | Ashley Andeen |

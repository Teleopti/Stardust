- Purpose,
This is the folder where Teleopti PS Tech will drop new/updated payroll .dll's and folders:
C:\Program Files (x86)\Teleopti\TeleoptiCCC\ServiceBus\Payroll.DeployNew
When the windows service "Teleopti Service Bus" starts, it will copy files from the above folder to the runtime folder:
C:\Program Files (x86)\Teleopti\TeleoptiCCC\ServiceBus\Payroll

- Default payroll
Any payroll .dll-files put in the root folder (Payroll.DeployNew) will be copied into runtime root folder.
Files in root of "Payroll.DeployNew" will be available for all data souceres (nhib files) with any given data source name *)

- Specific payrolls for one data source only,
If you run a multi-DB environment, you can add sub-folders to Payroll.DeployNew, e.g "Payroll.DeployNew\Acme"
Payrolls in folder "Acme" will only be available for clients having data source name set to "Acme", i.e. <session-factory name="Acme">

*)
Below is an example of a nhib file with data soruce name set to "Acme"
see nhib file: C:\Program Files (x86)\Teleopti\ConfigurationFiles\TeleoptiCCC7.nhib.xml
<session-factory name=" Acme ">

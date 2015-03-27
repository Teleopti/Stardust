--putting back super user role on system user (if it has been deleted by etl tool)
if not exists (select * from PersonInApplicationRole where person='3F0886AB-7B25-4E95-856A-0D726EDC2A67' and ApplicationRole='193AD35C-7735-44D7-AC0C-B8EDA0011E5F')
begin
 insert into PersonInApplicationRole values('3F0886AB-7B25-4E95-856A-0D726EDC2A67','193AD35C-7735-44D7-AC0C-B8EDA0011E5F',getdate())
end

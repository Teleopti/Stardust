alter table status.CustomStatusStep
add constraint df_LastPing
default dateadd(year, -1, getdate()) for lastping

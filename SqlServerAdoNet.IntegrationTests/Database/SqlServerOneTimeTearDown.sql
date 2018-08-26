USE [master];
GO

DECLARE @kill varchar(8000) = '';  

SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
FROM sys.dm_exec_sessions
WHERE database_id = db_id('AdoNetTest');

EXEC(@kill);
GO

IF EXISTS(select * from sys.databases where name='AdoNetTest') 
BEGIN
	DROP DATABASE [AdoNetTest];
END;
GO



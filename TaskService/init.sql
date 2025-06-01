IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TaskDB')
BEGIN
    CREATE DATABASE TaskDB;
    PRINT 'Database TaskDB created';
END
GO
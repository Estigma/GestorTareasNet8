IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ClientDB')
BEGIN
    CREATE DATABASE ClientDB;
    PRINT 'Database ClientDB created';
END
GO
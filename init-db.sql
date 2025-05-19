IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InternetCafe_User')
BEGIN
    CREATE DATABASE InternetCafe_User;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InternetCafe_ComputerSession')
BEGIN
    CREATE DATABASE InternetCafe_ComputerSession;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InternetCafe_Account')
BEGIN
    CREATE DATABASE InternetCafe_Account;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InternetCafe_Statistics')
BEGIN
    CREATE DATABASE InternetCafe_Statistics;
END
GO
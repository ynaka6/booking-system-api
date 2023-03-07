# About Install
## Install EntityFrameworkCore MySQL
```
dotnet add package MySql.EntityFrameworkCore --version 7.0.0
dotnet add package MySql.Data.EntityFrameworkCore --version 8.0.22
```

## generator controller
```
dotnet tool install -g dotnet-aspnet-codegenerator --version 7.0.2
```

## dotnet ef
```
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 7.0.2
dotnet tool install --global dotnet-ef --version 7.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 7.0.2
dotnet add package Pomelo.EntityFrameworkCore.MySql  --version 7.0.2
```

## migrate
```
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef database update --context ApplicationDbContext
```



## Referrence
https://mysqlconnector.net/tutorials/efcore/
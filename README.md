# StuDo-Back

[![Build Status](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLabRTUMIREA.StuDo-Back?branchName=develop)](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=67&branchName=develop)
## How to start
- Create "./studo/appsettings.Secret.json" in project
- Fill it according "./studo/appsettings.Secret.Schema.json":
    ```js
    {
    "FillDbOptions": {
        "Firstname": "Testname", // Default user's name 
        "Surname": "Testsurname", // Default user's surname
        "Email": "test@gmail.com", // Default user's email
        "Password": "test123456" // Default user's password
    },
    "JwtOptions": {
        "SecretKey": "superlongrandomsecretkeyforjwtoptions", // Random string for jwt signiture
        "Audience": "http://localhost:5301", // JWT audience
        "Issuer": "http://localhost:5301", // JWT issuer
        "RefreshTokenLifeTime": "10:00:00", // JWT refresh token life time
        "AccessTokenLifeTime": "00:05:00" // JWT access token life time
    },
    "LogsOptions": {
        "SecretKey": "superlongrandomsecretkeyforlogsoptions" // Random string for logs options
    },
    "USE_DEBUG_EMAIL_SENDER": true | false, // Debug email sender doesn't send real emails
    "FILL_DB": true | false, // Set to true in FIRST start to fill database with roles and default user
    "MIGRATE" : true | false, // Set to true if you have database migrations to be applied
    "ConnectionStrings": {
        "PostgreSQL": "Server=localhost;Port=5432;Database=TestDBstudoback;User Id=postgres;Password=password" // Connection string to your real Postgres database
    }
    }
    ```
- Use `dotnet run` to start app in Development environment (appsettings.Development.json will be used)
### First database init
- Need to trigger "FILL_DB" to true
    - Add it from console like `dotnet run FILL_DB=true`
    - Add property in "./studo/appsettings.Secret.json":
        ```js
        {
        "FILL_DB": true
        }
        ```
    > You'll receive a message "Result of creating user is Succeeded" in console  
    It means that default user was added.  
    You can receive a message "User with 'test@gmail.com' is already exists"  
    It means that default user is already exists


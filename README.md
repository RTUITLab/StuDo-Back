# StuDo-Back

## How to start
- Create "appsettings.Secret.json" in project
- Fill it according "appsettings.Secret.Schema.json"
### First init of local database
- Need to trigger "FILL_DB" to true
- Add property in "appsettings.Secret.json"
- Add it from console like "dotnet run FILL_DB=true"
#### You'll receive a message "Result of creating user is Succeeded" in console  
#### It means that default user was added and you don't need anymore to trigger "FILL_DB" to true
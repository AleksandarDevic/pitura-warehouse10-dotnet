# Warehouse10-Pitura App

# Prerequisite for first deployment:
    - Set 'logout' time in tables: OperatorTerminal and JobInProgress where it is null. Call endpoint for that using Swagger.
    
# Deployment:
    - cd cd .\src\Web.Api\
    - dotnet publish -c Release -o ./publish
    <!-- - dotnet publish -c Release -o ./publish /p:EnvironmentName=Production -->
    -IIS:
        -PhysicalPath: C:\inetpub\wwwroot\Warehouse10-BE
        -Port: 5079
        -ApplicationPool: No Managed Code
    


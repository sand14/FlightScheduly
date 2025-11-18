var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(5432)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var usersDatabase = postgres.AddDatabase("usersdb");
var flightDayDatabase = postgres.AddDatabase("flightdaydb");
    
var authApi = builder.AddProject<Projects.Auth_Api>("auth-api")
    .WithReference(usersDatabase)
    .WaitFor(usersDatabase);

var flightDayApi = builder.AddProject<Projects.FlightDay_Api>("flightday-api")
    .WithReference(flightDayDatabase)
    .WaitFor(flightDayDatabase);

builder.Build().Run();
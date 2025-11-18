var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(5432)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var usersDatabase = postgres.AddDatabase("usersdb");
    
var authApi = builder.AddProject<Projects.Auth_Api>("auth-api")
    .WithReference(usersDatabase)
    .WaitFor(usersDatabase);

builder.Build().Run();
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume()
                      .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var database = postgres.AddDatabase("GameStore");

var redis = builder.AddRedis("redis");

var gameStoreApi = builder.AddProject<GameStoreApi>("gamestore-api")
       .WithHttpHealthCheck("/health")
       .WithReference(database)
       .WaitFor(database)
       .WithReference(redis)
       .WaitFor(redis);

builder.AddProject<GameStoreFrontEnd>("gamestore-frontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(gameStoreApi)
    .WaitFor(gameStoreApi);

builder.Build().Run();

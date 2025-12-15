using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres", port: 5432)
    .WithDataVolume()
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var database = postgres.AddDatabase("GameStore");

var redis = builder.AddRedis("redis", port: 6379);

var gameStoreApi = builder.AddProject<GameStoreApi>("gamestoreapi")
       .WithHttpHealthCheck("/health")
       .WithReference(database)
       .WaitFor(database)
       .WithReference(redis)
       .WaitFor(redis);

builder.AddProject<GameStoreFrontEnd>("gamestorefrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(gameStoreApi)
    .WaitFor(gameStoreApi);

builder.Build().Run();

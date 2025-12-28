var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DemoServer>("demo-server");
builder.AddProject<Projects.McpStreamableServer>("mcp-streamable-server");
builder.AddProject<Projects.McpStreamableAuth>("mcp-streamable-auth");

builder.Build().Run();

using FullstackProject.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MapStore>();

var app = builder.Build();

//  Helps with proxies like Render (prevents HTTPS redirect weirdness)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//  Health endpoint with NO API key required
app.MapGet("/healthz", () => Results.Ok("ok"));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Optional: if this causes issues on Render, you can comment it out.
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();

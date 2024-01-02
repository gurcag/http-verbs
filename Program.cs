using HttpVerbsApi.DbContexts;
using HttpVerbsApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(opt => opt.UseInMemoryDatabase("ProductDb"));

var app = builder.Build();

// Entity Framework In-Memory Initial Data Generation
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ProductDbContext>();
    await context.GenerateInitialData();
}

app.MapMethods("/products", new[] { "OPTIONS" }, (HttpContext context) =>
{
    context.Response.Headers.AppendCommaSeparatedValues
        ("Allow", ["GET, POST, PUT, PATCH, DELETE, OPTIONS"]
    );
    return Results.NoContent();
});

app.MapGet("/products", async (ProductDbContext db) =>
    Results.Ok(await db.Products.ToListAsync()));

app.MapGet("/products/{id}", async (long id, ProductDbContext db) =>
    await db.Products.FindAsync(id)
        is Product product
            ? Results.Ok(product)
            : Results.NotFound());

app.MapPost("/products", async (Product product, ProductDbContext db) =>
{
    bool productExists = await db.Products.AnyAsync(x => x.Id == product.Id);

    // HTTP 409 Error Response for the resource already exists
    if (productExists) return Results.Conflict();

    await db.Products.AddAsync(product);
    await db.SaveChangesAsync();

    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", async (long id, Product product, ProductDbContext db) =>
{
    if (string.IsNullOrEmpty(product?.Name) 
        || string.IsNullOrEmpty(product?.Description))
    {
        // HTTP 400 for the invalid request model
        return Results.BadRequest();
    }

    var productEntity = await db.Products.FindAsync(id);

    if (productEntity is null) return Results.NotFound();

    productEntity.Name = product.Name;
    productEntity.Description = product.Description;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPatch("/products/{id}", async (long id, HttpContext httpContext, ProductDbContext db) =>
{
    if (!httpContext.Request.HasJsonContentType())
    {
        Results.BadRequest();
    }

    JsonPatchDocument? patchDocument;

    using (var streamReader = new StreamReader(httpContext.Request.Body))
    {
        var httpContent = await streamReader.ReadToEndAsync();
        patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument>(httpContent);
    }

    if (patchDocument is null) return Results.BadRequest();

    var productEntity = await db.Products.FindAsync(id);

    if (productEntity is null) return Results.NotFound();

    patchDocument.ApplyTo(productEntity);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (long id, ProductDbContext db) =>
{
    if (await db.Products.FindAsync(id) is Product product)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();

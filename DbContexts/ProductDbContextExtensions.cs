using HttpVerbsApi.Models;

namespace HttpVerbsApi.DbContexts
{
    public static class ProductDbContextExtensions
    {
        public static async Task<int> GenerateInitialData(this ProductDbContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            await context.AddRangeAsync(
                new Product
                {
                    Id = 1,
                    Name = "Name1",
                    Description = "Description1",
                    DateCreated = DateTime.Now,
                },
                new Product
                {
                    Id = 2,
                    Name = "Name2",
                    Description = "Description2",
                    DateCreated = DateTime.Now,
                }
                );

            return await context.SaveChangesAsync();
        }
    }
}

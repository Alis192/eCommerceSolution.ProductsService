using eCommerce.DataAccessLayer.Context;
using eCommerce.DataAccessLayer.Repositories;
using eCommerce.DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductService.DataAccessLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            //TO DO: Add data access layer services into the IoC container


            //When the application starts, it automatically creates an instance of ApplicationDbContext and passes the configured DbContextOptions to the constructor of ApplicationDbContext.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySQL(configuration.GetConnectionString("DefaultConnection")!);
            });

            services.AddScoped<IProductsRepository, ProductsRepository>();

            return services;
        }
    }
}

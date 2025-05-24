using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductService.BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            //TO DO: Add data access layer services into the IoC container

            services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);
            
            services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

            services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();

            services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

            return services;
        }
    }
}
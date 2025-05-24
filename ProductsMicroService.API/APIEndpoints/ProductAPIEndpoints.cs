using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;

namespace eCommerce.ProductsMicroService.API.APIEndpoints
{
    public static class ProductAPIEndpoints
    {
        public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
        {
            //GET /api/products
            app.MapGet("/api/products", async (IProductsService productService) =>
            {
                List<ProductResponse?> products = await productService.GetProducts();
                return Results.Ok(products);
            });

            //GET /api/products/search/product-id/00000000-0000-0000-0000-000000000000
            app.MapGet("/api/products/search/product-id/{ProductID:guid}", async (IProductsService productService, Guid ProductID) =>
            {
                //await Task.Delay(200);
                //throw new NotImplementedException();

                ProductResponse? products = await productService.GetProductByCondition(temp => temp.ProductID == ProductID);
                
                if (products == null)
                    return Results.NotFound();

                return Results.Ok(products);
            });


            //GET /api/products/search/xxxxx
            app.MapGet("/api/products/search/{SearchString}", async (IProductsService productService, string SearchString) =>
            {
                List<ProductResponse?> productsByProductName = await productService.GetProductsByCondition(temp => temp.ProductName != null && temp.ProductName.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

                List<ProductResponse?> productsByCategory = await productService.GetProductsByCondition(temp => temp.Category != null && temp.Category.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

                //It will assign the products that are either in the productsByProductName or productsByCategory
                var products = productsByProductName.Union(productsByCategory).ToList();

                return Results.Ok(products);
            });


            //POST /api/products
            app.MapPost("/api/products", async (IProductsService productService, ProductAddRequest productAddRequest, IValidator<ProductAddRequest> productAddRequestValidator) =>
            {
                //Validate the ProductAddRequest object using Fluent Validation
                ValidationResult validationResult = await productAddRequestValidator.ValidateAsync(productAddRequest);

                //Check the validation result
                if (!validationResult.IsValid)
                {
                    Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());
                    return Results.ValidationProblem(errors);
                }

                var addedProductResponse = await productService.AddProduct(productAddRequest);
                
                if (addedProductResponse != null)
                    return Results.Created($"/api/products/search/product-id/{addedProductResponse.ProductID}", addedProductResponse);
                else 
                    return Results.Problem("Error in adding product");
            });



            //PUT /api/products
            app.MapPut("/api/products", async (IProductsService productService, ProductUpdateRequest productUpdateRequest, IValidator<ProductUpdateRequest> productUpdateRequestValidator) =>
            {
                //Validate the ProductAddRequest object using Fluent Validation
                ValidationResult validationResult = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

                //Check the validation result
                if (!validationResult.IsValid)
                {
                    Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());
                    return Results.ValidationProblem(errors);
                }

                var updatedProductResponse = await productService.UpdateProduct(productUpdateRequest);

                if (updatedProductResponse != null)
                    return Results.Ok(updatedProductResponse);
                else
                    return Results.Problem("Error in updating product");
            });


            //DELETE /api/products/00000000-0000-0000-0000-000000000000
            app.MapDelete("/api/products/{ProductID:guid}", async (IProductsService productService, Guid ProductID) =>
            {
                bool isDeleted = await productService.DeleteProduct(ProductID);
                if (isDeleted)
                    return Results.Ok(true);
                else
                    return Results.Problem("Error in deleting product");
            });

            return app;
        }
    }
}

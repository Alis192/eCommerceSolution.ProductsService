using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using eCommerce.ProductService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
        private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
        private readonly IMapper _mapper;
        private readonly IProductsRepository _productsRepository;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator, IValidator<ProductUpdateRequest> productUpdateRequestValidator, IMapper mapper, IProductsRepository productsRepository, IRabbitMQPublisher rabbitMQPublisher)
        {
            _productAddRequestValidator = productAddRequestValidator;
            _productUpdateRequestValidator = productUpdateRequestValidator;
            _mapper = mapper;
            _productsRepository = productsRepository;
            _rabbitMQPublisher = rabbitMQPublisher;
        }


        public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
        {
            if (productAddRequest == null)
            {
                throw new ArgumentNullException(nameof(productAddRequest));
            }

            //Validate the request  
            ValidationResult validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);
            
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage)); //Error 1, Error 2, Error 3 ...
                throw new ArgumentException(errors);  
            }

            //Attempt to add the product 
            Product productInput = _mapper.Map<Product>(productAddRequest); //Map productAddRequest to Product

            Product? addedProduct = await _productsRepository.AddProduct(productInput);

            if (addedProduct == null)
            {
                return null;
            }

            ProductResponse addedProductResponse = _mapper.Map<ProductResponse>(addedProduct); //Map Product to ProductResponse

            return addedProductResponse;

        }

        public async Task<bool> DeleteProduct(Guid productID)
        {
            Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productID);

            if (existingProduct == null)
            {
                return false;
            }

            bool isDeleted = await _productsRepository.DeleteProduct(productID);

            if (isDeleted)
            {
                ProductDeletionMessage message = new ProductDeletionMessage(existingProduct.ProductID, existingProduct.ProductName);
                string routingKey = "product.delete";
                _rabbitMQPublisher.Publish<ProductDeletionMessage>(routingKey, message);
            }

            return isDeleted;
        }

        public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
        {
            Product? product =await _productsRepository.GetProductByCondition(conditionExpression);

            if (product == null)
            {
                return null;
            }

            ProductResponse productResponse = _mapper.Map<ProductResponse>(product);

            return productResponse;
        }

        public async Task<List<ProductResponse?>> GetProducts()
        {
            IEnumerable<Product?> products = await _productsRepository.GetProducts();

            //It invokes the mapper internally for each object in the collection. If you have a collection of 1000 objects, it will invoke the mapper 1000 times.
            IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);

            return productResponses.ToList();
        }

        public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
        {
            IEnumerable<Product?> products = await _productsRepository.GetProductsByCondition(conditionExpression);

            //It invokes the mapper internally for each object in the collection. If you have a collection of 1000 objects, it will invoke the mapper 1000 times.
            IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);

            return productResponses.ToList();
        }

        public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
        {
            Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productUpdateRequest.ProductID);
        
            if (existingProduct == null)
            {
                throw new ArgumentException("Product not found");   
            }

            //Validate the request using FluentValidation
            ValidationResult validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

            //Check the validation result
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage)); //Error 1, Error 2, Error 3 ...
                throw new ArgumentException(errors);
            }

            Product product = _mapper.Map<Product>(productUpdateRequest); //Invokes the mapper to map ProductUpdateRequest to Product
        
            //Check if product name is changed 
            bool isProductNameChanged = !string.Equals(existingProduct.ProductName, product.ProductName, StringComparison.OrdinalIgnoreCase);

            Product? updatedProduct = await _productsRepository.UpdateProduct(product);

            if (isProductNameChanged)
            {
                string routingKey = "product.update.name";
                var message = new ProductNameUpdateMessage(product.ProductID, product.ProductName);

                _rabbitMQPublisher.Publish<ProductNameUpdateMessage>(routingKey, message);
            }

                ProductResponse? updatedProductResponse = _mapper.Map<ProductResponse>(updatedProduct);

            return updatedProductResponse;
        }
    }
}

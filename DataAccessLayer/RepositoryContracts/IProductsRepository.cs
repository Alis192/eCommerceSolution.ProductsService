using eCommerce.DataAccessLayer.Entities;
using System.Linq.Expressions;

namespace eCommerce.DataAccessLayer.RepositoryContracts
{
    /// <summary>
    /// Represents a repository for managing 'Products' tables.
    /// </summary>
    public interface IProductsRepository
    {
        /// <summary>
        /// Returns all products from the table asynchrounously.
        /// </summary>
        /// <returns>Returns all products from the table</returns>
        Task<IEnumerable<Product>> GetProducts();

        /// <summary>
        /// Retrieves all products that satisfy the condition asynchronously.
        /// </summary>
        /// <param name="conditionExpression">The condition to filter products</param>
        /// <returns>Returning a collection of matching products</returns>
        Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression);

        /// <summary>
        /// Retrieves a single product based on specified condition asynchronously.
        /// </summary>
        /// <param name="conditionExpression">The condition to filter the product</param>
        /// <returns>Returns a single product or null if not found</returns>
        Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression);


        /// <summary>
        /// Adds a single prodcut to the table asynchrounously.
        /// </summary>
        /// <param name="product">The product to be added</param>
        /// <returns>Returns the added product object or null if unsuccessful</returns>
        Task<Product?> AddProduct(Product product);


        /// <summary>
        /// Updates a single product in the table asynchrounously.
        /// </summary>
        /// <param name="product">The product to be updates</param>
        /// <returns>Returns the updated productl or null if not found</returns>
        Task<Product?> UpdateProduct(Product product);


        /// <summary>
        /// Deletes a single product from the table asynchrounously.
        /// </summary>
        /// <param name="productID">The product ID to be deleted</param>
        /// <returns>Returns true if deletion is successfull, otherwise false</returns>
        Task<bool> DeleteProduct(Guid productID);
    }
}

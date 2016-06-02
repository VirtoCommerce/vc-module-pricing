using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using VirtoCommerce.PricingModule.Client.Client;
using VirtoCommerce.PricingModule.Client.Model;

namespace VirtoCommerce.PricingModule.Client.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IVirtoCommercePricingApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Create pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Pricelist</returns>
        Pricelist PricingModuleCreatePriceList(Pricelist priceList);

        /// <summary>
        /// Create pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>ApiResponse of Pricelist</returns>
        ApiResponse<Pricelist> PricingModuleCreatePriceListWithHttpInfo(Pricelist priceList);
        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>PricelistAssignment</returns>
        PricelistAssignment PricingModuleCreatePricelistAssignment(PricelistAssignment assignment);

        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        ApiResponse<PricelistAssignment> PricingModuleCreatePricelistAssignmentWithHttpInfo(PricelistAssignment assignment);
        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>
        /// Delete pricelist assignment by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns></returns>
        void PricingModuleDeleteAssignments(List<string> ids);

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>
        /// Delete pricelist assignment by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> PricingModuleDeleteAssignmentsWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete pricelists
        /// </summary>
        /// <remarks>
        /// Delete pricelists by given array of pricelist ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns></returns>
        void PricingModuleDeletePriceLists(List<string> ids);

        /// <summary>
        /// Delete pricelists
        /// </summary>
        /// <remarks>
        /// Delete pricelists by given array of pricelist ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> PricingModuleDeletePriceListsWithHttpInfo(List<string> ids);
        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>List&lt;Pricelist&gt;</returns>
        List<Pricelist> PricingModuleEvaluatePriceLists(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        ApiResponse<List<Pricelist>> PricingModuleEvaluatePriceListsWithHttpInfo(PriceEvaluationContext evalContext);
        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>List&lt;Price&gt;</returns>
        List<Price> PricingModuleEvaluatePrices(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>ApiResponse of List&lt;Price&gt;</returns>
        ApiResponse<List<Price>> PricingModuleEvaluatePricesWithHttpInfo(PriceEvaluationContext evalContext);
        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>
        /// Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>PricelistAssignment</returns>
        PricelistAssignment PricingModuleGetNewPricelistAssignments();

        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>
        /// Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        ApiResponse<PricelistAssignment> PricingModuleGetNewPricelistAssignmentsWithHttpInfo();
        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Pricelist</returns>
        Pricelist PricingModuleGetPriceListById(string id);

        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>ApiResponse of Pricelist</returns>
        ApiResponse<Pricelist> PricingModuleGetPriceListByIdWithHttpInfo(string id);
        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>
        /// Get all pricelists for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Pricelist&gt;</returns>
        List<Pricelist> PricingModuleGetPriceLists();

        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>
        /// Get all pricelists for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        ApiResponse<List<Pricelist>> PricingModuleGetPriceListsWithHttpInfo();
        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>PricelistAssignment</returns>
        PricelistAssignment PricingModuleGetPricelistAssignmentById(string id);

        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        ApiResponse<PricelistAssignment> PricingModuleGetPricelistAssignmentByIdWithHttpInfo(string id);
        /// <summary>
        /// Get pricelist assignments
        /// </summary>
        /// <remarks>
        /// Get array of all pricelist assignments for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;PricelistAssignment&gt;</returns>
        List<PricelistAssignment> PricingModuleGetPricelistAssignments();

        /// <summary>
        /// Get pricelist assignments
        /// </summary>
        /// <remarks>
        /// Get array of all pricelist assignments for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;PricelistAssignment&gt;</returns>
        ApiResponse<List<PricelistAssignment>> PricingModuleGetPricelistAssignmentsWithHttpInfo();
        /// <summary>
        /// Get pricelists for a product
        /// </summary>
        /// <remarks>
        /// Get all pricelists for given product.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>List&lt;Pricelist&gt;</returns>
        List<Pricelist> PricingModuleGetProductPriceLists(string productId);

        /// <summary>
        /// Get pricelists for a product
        /// </summary>
        /// <remarks>
        /// Get all pricelists for given product.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        ApiResponse<List<Pricelist>> PricingModuleGetProductPriceListsWithHttpInfo(string productId);
        /// <summary>
        /// Get array of product prices
        /// </summary>
        /// <remarks>
        /// Get an array of valid product prices for each currency.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>List&lt;Price&gt;</returns>
        List<Price> PricingModuleGetProductPrices(string productId);

        /// <summary>
        /// Get array of product prices
        /// </summary>
        /// <remarks>
        /// Get an array of valid product prices for each currency.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>ApiResponse of List&lt;Price&gt;</returns>
        ApiResponse<List<Price>> PricingModuleGetProductPricesWithHttpInfo(string productId);
        /// <summary>
        /// Update pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns></returns>
        void PricingModuleUpdatePriceList(Pricelist priceList);

        /// <summary>
        /// Update pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> PricingModuleUpdatePriceListWithHttpInfo(Pricelist priceList);
        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns></returns>
        void PricingModuleUpdatePriceListAssignment(PricelistAssignment assignment);

        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> PricingModuleUpdatePriceListAssignmentWithHttpInfo(PricelistAssignment assignment);
        /// <summary>
        /// Update prices
        /// </summary>
        /// <remarks>
        /// Update prices of product for given pricelist.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns></returns>
        void PricingModuleUpdateProductPriceLists(string productId, Pricelist priceList);

        /// <summary>
        /// Update prices
        /// </summary>
        /// <remarks>
        /// Update prices of product for given pricelist.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> PricingModuleUpdateProductPriceListsWithHttpInfo(string productId, Pricelist priceList);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Create pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of Pricelist</returns>
        System.Threading.Tasks.Task<Pricelist> PricingModuleCreatePriceListAsync(Pricelist priceList);

        /// <summary>
        /// Create pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of ApiResponse (Pricelist)</returns>
        System.Threading.Tasks.Task<ApiResponse<Pricelist>> PricingModuleCreatePriceListAsyncWithHttpInfo(Pricelist priceList);
        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of PricelistAssignment</returns>
        System.Threading.Tasks.Task<PricelistAssignment> PricingModuleCreatePricelistAssignmentAsync(PricelistAssignment assignment);

        /// <summary>
        /// Create pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleCreatePricelistAssignmentAsyncWithHttpInfo(PricelistAssignment assignment);
        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>
        /// Delete pricelist assignment by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PricingModuleDeleteAssignmentsAsync(List<string> ids);

        /// <summary>
        /// Delete pricelist assignments
        /// </summary>
        /// <remarks>
        /// Delete pricelist assignment by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleDeleteAssignmentsAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete pricelists
        /// </summary>
        /// <remarks>
        /// Delete pricelists by given array of pricelist ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PricingModuleDeletePriceListsAsync(List<string> ids);

        /// <summary>
        /// Delete pricelists
        /// </summary>
        /// <remarks>
        /// Delete pricelists by given array of pricelist ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleDeletePriceListsAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        System.Threading.Tasks.Task<List<Pricelist>> PricingModuleEvaluatePriceListsAsync(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluate pricelists by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleEvaluatePriceListsAsyncWithHttpInfo(PriceEvaluationContext evalContext);
        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of List&lt;Price&gt;</returns>
        System.Threading.Tasks.Task<List<Price>> PricingModuleEvaluatePricesAsync(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluate prices by given context
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of ApiResponse (List&lt;Price&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Price>>> PricingModuleEvaluatePricesAsyncWithHttpInfo(PriceEvaluationContext evalContext);
        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>
        /// Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of PricelistAssignment</returns>
        System.Threading.Tasks.Task<PricelistAssignment> PricingModuleGetNewPricelistAssignmentsAsync();

        /// <summary>
        /// Get a new pricelist assignment
        /// </summary>
        /// <remarks>
        /// Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleGetNewPricelistAssignmentsAsyncWithHttpInfo();
        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Task of Pricelist</returns>
        System.Threading.Tasks.Task<Pricelist> PricingModuleGetPriceListByIdAsync(string id);

        /// <summary>
        /// Get pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Task of ApiResponse (Pricelist)</returns>
        System.Threading.Tasks.Task<ApiResponse<Pricelist>> PricingModuleGetPriceListByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>
        /// Get all pricelists for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        System.Threading.Tasks.Task<List<Pricelist>> PricingModuleGetPriceListsAsync();

        /// <summary>
        /// Get pricelists
        /// </summary>
        /// <remarks>
        /// Get all pricelists for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleGetPriceListsAsyncWithHttpInfo();
        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>Task of PricelistAssignment</returns>
        System.Threading.Tasks.Task<PricelistAssignment> PricingModuleGetPricelistAssignmentByIdAsync(string id);

        /// <summary>
        /// Get pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleGetPricelistAssignmentByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get pricelist assignments
        /// </summary>
        /// <remarks>
        /// Get array of all pricelist assignments for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;PricelistAssignment&gt;</returns>
        System.Threading.Tasks.Task<List<PricelistAssignment>> PricingModuleGetPricelistAssignmentsAsync();

        /// <summary>
        /// Get pricelist assignments
        /// </summary>
        /// <remarks>
        /// Get array of all pricelist assignments for all catalogs.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;PricelistAssignment&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<PricelistAssignment>>> PricingModuleGetPricelistAssignmentsAsyncWithHttpInfo();
        /// <summary>
        /// Get pricelists for a product
        /// </summary>
        /// <remarks>
        /// Get all pricelists for given product.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        System.Threading.Tasks.Task<List<Pricelist>> PricingModuleGetProductPriceListsAsync(string productId);

        /// <summary>
        /// Get pricelists for a product
        /// </summary>
        /// <remarks>
        /// Get all pricelists for given product.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleGetProductPriceListsAsyncWithHttpInfo(string productId);
        /// <summary>
        /// Get array of product prices
        /// </summary>
        /// <remarks>
        /// Get an array of valid product prices for each currency.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of List&lt;Price&gt;</returns>
        System.Threading.Tasks.Task<List<Price>> PricingModuleGetProductPricesAsync(string productId);

        /// <summary>
        /// Get array of product prices
        /// </summary>
        /// <remarks>
        /// Get an array of valid product prices for each currency.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of ApiResponse (List&lt;Price&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Price>>> PricingModuleGetProductPricesAsyncWithHttpInfo(string productId);
        /// <summary>
        /// Update pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PricingModuleUpdatePriceListAsync(Pricelist priceList);

        /// <summary>
        /// Update pricelist
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdatePriceListAsyncWithHttpInfo(Pricelist priceList);
        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PricingModuleUpdatePriceListAssignmentAsync(PricelistAssignment assignment);

        /// <summary>
        /// Update pricelist assignment
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdatePriceListAssignmentAsyncWithHttpInfo(PricelistAssignment assignment);
        /// <summary>
        /// Update prices
        /// </summary>
        /// <remarks>
        /// Update prices of product for given pricelist.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PricingModuleUpdateProductPriceListsAsync(string productId, Pricelist priceList);

        /// <summary>
        /// Update prices
        /// </summary>
        /// <remarks>
        /// Update prices of product for given pricelist.
        /// </remarks>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdateProductPriceListsAsyncWithHttpInfo(string productId, Pricelist priceList);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class VirtoCommercePricingApi : IVirtoCommercePricingApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommercePricingApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="apiClient">An instance of ApiClient.</param>
        /// <returns></returns>
        public VirtoCommercePricingApi(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Configuration = apiClient.Configuration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the API client object
        /// </summary>
        /// <value>An instance of the ApiClient</value>
        public ApiClient ApiClient { get; set; }

        /// <summary>
        /// Create pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Pricelist</returns>
        public Pricelist PricingModuleCreatePriceList(Pricelist priceList)
        {
             ApiResponse<Pricelist> localVarResponse = PricingModuleCreatePriceListWithHttpInfo(priceList);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>ApiResponse of Pricelist</returns>
        public ApiResponse<Pricelist> PricingModuleCreatePriceListWithHttpInfo(Pricelist priceList)
        {
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleCreatePriceList");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePriceList: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePriceList: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Pricelist>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Pricelist)ApiClient.Deserialize(localVarResponse, typeof(Pricelist)));
            
        }

        /// <summary>
        /// Create pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of Pricelist</returns>
        public async System.Threading.Tasks.Task<Pricelist> PricingModuleCreatePriceListAsync(Pricelist priceList)
        {
             ApiResponse<Pricelist> localVarResponse = await PricingModuleCreatePriceListAsyncWithHttpInfo(priceList);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of ApiResponse (Pricelist)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Pricelist>> PricingModuleCreatePriceListAsyncWithHttpInfo(Pricelist priceList)
        {
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleCreatePriceList");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePriceList: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePriceList: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Pricelist>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Pricelist)ApiClient.Deserialize(localVarResponse, typeof(Pricelist)));
            
        }
        /// <summary>
        /// Create pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>PricelistAssignment</returns>
        public PricelistAssignment PricingModuleCreatePricelistAssignment(PricelistAssignment assignment)
        {
             ApiResponse<PricelistAssignment> localVarResponse = PricingModuleCreatePricelistAssignmentWithHttpInfo(assignment);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        public ApiResponse<PricelistAssignment> PricingModuleCreatePricelistAssignmentWithHttpInfo(PricelistAssignment assignment)
        {
            // verify the required parameter 'assignment' is set
            if (assignment == null)
                throw new ApiException(400, "Missing required parameter 'assignment' when calling VirtoCommercePricingApi->PricingModuleCreatePricelistAssignment");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (assignment.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(assignment); // http body (model) parameter
            }
            else
            {
                localVarPostBody = assignment; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePricelistAssignment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePricelistAssignment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }

        /// <summary>
        /// Create pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of PricelistAssignment</returns>
        public async System.Threading.Tasks.Task<PricelistAssignment> PricingModuleCreatePricelistAssignmentAsync(PricelistAssignment assignment)
        {
             ApiResponse<PricelistAssignment> localVarResponse = await PricingModuleCreatePricelistAssignmentAsyncWithHttpInfo(assignment);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleCreatePricelistAssignmentAsyncWithHttpInfo(PricelistAssignment assignment)
        {
            // verify the required parameter 'assignment' is set
            if (assignment == null)
                throw new ApiException(400, "Missing required parameter 'assignment' when calling VirtoCommercePricingApi->PricingModuleCreatePricelistAssignment");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (assignment.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(assignment); // http body (model) parameter
            }
            else
            {
                localVarPostBody = assignment; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePricelistAssignment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleCreatePricelistAssignment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }
        /// <summary>
        /// Delete pricelist assignments Delete pricelist assignment by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns></returns>
        public void PricingModuleDeleteAssignments(List<string> ids)
        {
             PricingModuleDeleteAssignmentsWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete pricelist assignments Delete pricelist assignment by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PricingModuleDeleteAssignmentsWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommercePricingApi->PricingModuleDeleteAssignments");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeleteAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeleteAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete pricelist assignments Delete pricelist assignment by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task PricingModuleDeleteAssignmentsAsync(List<string> ids)
        {
             await PricingModuleDeleteAssignmentsAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete pricelist assignments Delete pricelist assignment by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist assignment ids</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleDeleteAssignmentsAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommercePricingApi->PricingModuleDeleteAssignments");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeleteAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeleteAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Delete pricelists Delete pricelists by given array of pricelist ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns></returns>
        public void PricingModuleDeletePriceLists(List<string> ids)
        {
             PricingModuleDeletePriceListsWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete pricelists Delete pricelists by given array of pricelist ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PricingModuleDeletePriceListsWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommercePricingApi->PricingModuleDeletePriceLists");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeletePriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeletePriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete pricelists Delete pricelists by given array of pricelist ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task PricingModuleDeletePriceListsAsync(List<string> ids)
        {
             await PricingModuleDeletePriceListsAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete pricelists Delete pricelists by given array of pricelist ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of pricelist ids</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleDeletePriceListsAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommercePricingApi->PricingModuleDeletePriceLists");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeletePriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleDeletePriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Evaluate pricelists by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>List&lt;Pricelist&gt;</returns>
        public List<Pricelist> PricingModuleEvaluatePriceLists(PriceEvaluationContext evalContext)
        {
             ApiResponse<List<Pricelist>> localVarResponse = PricingModuleEvaluatePriceListsWithHttpInfo(evalContext);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Evaluate pricelists by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        public ApiResponse<List<Pricelist>> PricingModuleEvaluatePriceListsWithHttpInfo(PriceEvaluationContext evalContext)
        {
            // verify the required parameter 'evalContext' is set
            if (evalContext == null)
                throw new ApiException(400, "Missing required parameter 'evalContext' when calling VirtoCommercePricingApi->PricingModuleEvaluatePriceLists");

            var localVarPath = "/api/pricing/pricelists/evaluate";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (evalContext.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(evalContext); // http body (model) parameter
            }
            else
            {
                localVarPostBody = evalContext; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }

        /// <summary>
        /// Evaluate pricelists by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        public async System.Threading.Tasks.Task<List<Pricelist>> PricingModuleEvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
             ApiResponse<List<Pricelist>> localVarResponse = await PricingModuleEvaluatePriceListsAsyncWithHttpInfo(evalContext);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Evaluate pricelists by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleEvaluatePriceListsAsyncWithHttpInfo(PriceEvaluationContext evalContext)
        {
            // verify the required parameter 'evalContext' is set
            if (evalContext == null)
                throw new ApiException(400, "Missing required parameter 'evalContext' when calling VirtoCommercePricingApi->PricingModuleEvaluatePriceLists");

            var localVarPath = "/api/pricing/pricelists/evaluate";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (evalContext.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(evalContext); // http body (model) parameter
            }
            else
            {
                localVarPostBody = evalContext; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }
        /// <summary>
        /// Evaluate prices by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>List&lt;Price&gt;</returns>
        public List<Price> PricingModuleEvaluatePrices(PriceEvaluationContext evalContext)
        {
             ApiResponse<List<Price>> localVarResponse = PricingModuleEvaluatePricesWithHttpInfo(evalContext);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Evaluate prices by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>ApiResponse of List&lt;Price&gt;</returns>
        public ApiResponse<List<Price>> PricingModuleEvaluatePricesWithHttpInfo(PriceEvaluationContext evalContext)
        {
            // verify the required parameter 'evalContext' is set
            if (evalContext == null)
                throw new ApiException(400, "Missing required parameter 'evalContext' when calling VirtoCommercePricingApi->PricingModuleEvaluatePrices");

            var localVarPath = "/api/pricing/evaluate";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (evalContext.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(evalContext); // http body (model) parameter
            }
            else
            {
                localVarPostBody = evalContext; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePrices: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePrices: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Price>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Price>)ApiClient.Deserialize(localVarResponse, typeof(List<Price>)));
            
        }

        /// <summary>
        /// Evaluate prices by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of List&lt;Price&gt;</returns>
        public async System.Threading.Tasks.Task<List<Price>> PricingModuleEvaluatePricesAsync(PriceEvaluationContext evalContext)
        {
             ApiResponse<List<Price>> localVarResponse = await PricingModuleEvaluatePricesAsyncWithHttpInfo(evalContext);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Evaluate prices by given context 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="evalContext">Pricing evaluation context</param>
        /// <returns>Task of ApiResponse (List&lt;Price&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Price>>> PricingModuleEvaluatePricesAsyncWithHttpInfo(PriceEvaluationContext evalContext)
        {
            // verify the required parameter 'evalContext' is set
            if (evalContext == null)
                throw new ApiException(400, "Missing required parameter 'evalContext' when calling VirtoCommercePricingApi->PricingModuleEvaluatePrices");

            var localVarPath = "/api/pricing/evaluate";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (evalContext.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(evalContext); // http body (model) parameter
            }
            else
            {
                localVarPostBody = evalContext; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePrices: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleEvaluatePrices: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Price>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Price>)ApiClient.Deserialize(localVarResponse, typeof(List<Price>)));
            
        }
        /// <summary>
        /// Get a new pricelist assignment Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>PricelistAssignment</returns>
        public PricelistAssignment PricingModuleGetNewPricelistAssignments()
        {
             ApiResponse<PricelistAssignment> localVarResponse = PricingModuleGetNewPricelistAssignmentsWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get a new pricelist assignment Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        public ApiResponse<PricelistAssignment> PricingModuleGetNewPricelistAssignmentsWithHttpInfo()
        {

            var localVarPath = "/api/pricing/assignments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetNewPricelistAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetNewPricelistAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }

        /// <summary>
        /// Get a new pricelist assignment Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of PricelistAssignment</returns>
        public async System.Threading.Tasks.Task<PricelistAssignment> PricingModuleGetNewPricelistAssignmentsAsync()
        {
             ApiResponse<PricelistAssignment> localVarResponse = await PricingModuleGetNewPricelistAssignmentsAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get a new pricelist assignment Get a new pricelist assignment object. Create new pricelist assignment, but does not save one.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleGetNewPricelistAssignmentsAsyncWithHttpInfo()
        {

            var localVarPath = "/api/pricing/assignments/new";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetNewPricelistAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetNewPricelistAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }
        /// <summary>
        /// Get pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Pricelist</returns>
        public Pricelist PricingModuleGetPriceListById(string id)
        {
             ApiResponse<Pricelist> localVarResponse = PricingModuleGetPriceListByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>ApiResponse of Pricelist</returns>
        public ApiResponse<Pricelist> PricingModuleGetPriceListByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommercePricingApi->PricingModuleGetPriceListById");

            var localVarPath = "/api/pricing/pricelists/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceListById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceListById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Pricelist>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Pricelist)ApiClient.Deserialize(localVarResponse, typeof(Pricelist)));
            
        }

        /// <summary>
        /// Get pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Task of Pricelist</returns>
        public async System.Threading.Tasks.Task<Pricelist> PricingModuleGetPriceListByIdAsync(string id)
        {
             ApiResponse<Pricelist> localVarResponse = await PricingModuleGetPriceListByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist id</param>
        /// <returns>Task of ApiResponse (Pricelist)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Pricelist>> PricingModuleGetPriceListByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommercePricingApi->PricingModuleGetPriceListById");

            var localVarPath = "/api/pricing/pricelists/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceListById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceListById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Pricelist>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Pricelist)ApiClient.Deserialize(localVarResponse, typeof(Pricelist)));
            
        }
        /// <summary>
        /// Get pricelists Get all pricelists for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Pricelist&gt;</returns>
        public List<Pricelist> PricingModuleGetPriceLists()
        {
             ApiResponse<List<Pricelist>> localVarResponse = PricingModuleGetPriceListsWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get pricelists Get all pricelists for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        public ApiResponse<List<Pricelist>> PricingModuleGetPriceListsWithHttpInfo()
        {

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }

        /// <summary>
        /// Get pricelists Get all pricelists for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        public async System.Threading.Tasks.Task<List<Pricelist>> PricingModuleGetPriceListsAsync()
        {
             ApiResponse<List<Pricelist>> localVarResponse = await PricingModuleGetPriceListsAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get pricelists Get all pricelists for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleGetPriceListsAsyncWithHttpInfo()
        {

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }
        /// <summary>
        /// Get pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>PricelistAssignment</returns>
        public PricelistAssignment PricingModuleGetPricelistAssignmentById(string id)
        {
             ApiResponse<PricelistAssignment> localVarResponse = PricingModuleGetPricelistAssignmentByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>ApiResponse of PricelistAssignment</returns>
        public ApiResponse<PricelistAssignment> PricingModuleGetPricelistAssignmentByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommercePricingApi->PricingModuleGetPricelistAssignmentById");

            var localVarPath = "/api/pricing/assignments/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignmentById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignmentById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }

        /// <summary>
        /// Get pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>Task of PricelistAssignment</returns>
        public async System.Threading.Tasks.Task<PricelistAssignment> PricingModuleGetPricelistAssignmentByIdAsync(string id)
        {
             ApiResponse<PricelistAssignment> localVarResponse = await PricingModuleGetPricelistAssignmentByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Pricelist assignment id</param>
        /// <returns>Task of ApiResponse (PricelistAssignment)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<PricelistAssignment>> PricingModuleGetPricelistAssignmentByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommercePricingApi->PricingModuleGetPricelistAssignmentById");

            var localVarPath = "/api/pricing/assignments/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignmentById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignmentById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<PricelistAssignment>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (PricelistAssignment)ApiClient.Deserialize(localVarResponse, typeof(PricelistAssignment)));
            
        }
        /// <summary>
        /// Get pricelist assignments Get array of all pricelist assignments for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;PricelistAssignment&gt;</returns>
        public List<PricelistAssignment> PricingModuleGetPricelistAssignments()
        {
             ApiResponse<List<PricelistAssignment>> localVarResponse = PricingModuleGetPricelistAssignmentsWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get pricelist assignments Get array of all pricelist assignments for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;PricelistAssignment&gt;</returns>
        public ApiResponse<List<PricelistAssignment>> PricingModuleGetPricelistAssignmentsWithHttpInfo()
        {

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<PricelistAssignment>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<PricelistAssignment>)ApiClient.Deserialize(localVarResponse, typeof(List<PricelistAssignment>)));
            
        }

        /// <summary>
        /// Get pricelist assignments Get array of all pricelist assignments for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;PricelistAssignment&gt;</returns>
        public async System.Threading.Tasks.Task<List<PricelistAssignment>> PricingModuleGetPricelistAssignmentsAsync()
        {
             ApiResponse<List<PricelistAssignment>> localVarResponse = await PricingModuleGetPricelistAssignmentsAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get pricelist assignments Get array of all pricelist assignments for all catalogs.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;PricelistAssignment&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<PricelistAssignment>>> PricingModuleGetPricelistAssignmentsAsyncWithHttpInfo()
        {

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignments: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetPricelistAssignments: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<PricelistAssignment>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<PricelistAssignment>)ApiClient.Deserialize(localVarResponse, typeof(List<PricelistAssignment>)));
            
        }
        /// <summary>
        /// Get pricelists for a product Get all pricelists for given product.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>List&lt;Pricelist&gt;</returns>
        public List<Pricelist> PricingModuleGetProductPriceLists(string productId)
        {
             ApiResponse<List<Pricelist>> localVarResponse = PricingModuleGetProductPriceListsWithHttpInfo(productId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get pricelists for a product Get all pricelists for given product.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>ApiResponse of List&lt;Pricelist&gt;</returns>
        public ApiResponse<List<Pricelist>> PricingModuleGetProductPriceListsWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleGetProductPriceLists");

            var localVarPath = "/api/catalog/products/{productId}/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }

        /// <summary>
        /// Get pricelists for a product Get all pricelists for given product.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of List&lt;Pricelist&gt;</returns>
        public async System.Threading.Tasks.Task<List<Pricelist>> PricingModuleGetProductPriceListsAsync(string productId)
        {
             ApiResponse<List<Pricelist>> localVarResponse = await PricingModuleGetProductPriceListsAsyncWithHttpInfo(productId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get pricelists for a product Get all pricelists for given product.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of ApiResponse (List&lt;Pricelist&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Pricelist>>> PricingModuleGetProductPriceListsAsyncWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleGetProductPriceLists");

            var localVarPath = "/api/catalog/products/{productId}/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Pricelist>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Pricelist>)ApiClient.Deserialize(localVarResponse, typeof(List<Pricelist>)));
            
        }
        /// <summary>
        /// Get array of product prices Get an array of valid product prices for each currency.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>List&lt;Price&gt;</returns>
        public List<Price> PricingModuleGetProductPrices(string productId)
        {
             ApiResponse<List<Price>> localVarResponse = PricingModuleGetProductPricesWithHttpInfo(productId);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get array of product prices Get an array of valid product prices for each currency.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>ApiResponse of List&lt;Price&gt;</returns>
        public ApiResponse<List<Price>> PricingModuleGetProductPricesWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleGetProductPrices");

            var localVarPath = "/api/products/{productId}/prices";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPrices: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPrices: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Price>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Price>)ApiClient.Deserialize(localVarResponse, typeof(List<Price>)));
            
        }

        /// <summary>
        /// Get array of product prices Get an array of valid product prices for each currency.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of List&lt;Price&gt;</returns>
        public async System.Threading.Tasks.Task<List<Price>> PricingModuleGetProductPricesAsync(string productId)
        {
             ApiResponse<List<Price>> localVarResponse = await PricingModuleGetProductPricesAsyncWithHttpInfo(productId);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get array of product prices Get an array of valid product prices for each currency.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <returns>Task of ApiResponse (List&lt;Price&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Price>>> PricingModuleGetProductPricesAsyncWithHttpInfo(string productId)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleGetProductPrices");

            var localVarPath = "/api/products/{productId}/prices";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPrices: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleGetProductPrices: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Price>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Price>)ApiClient.Deserialize(localVarResponse, typeof(List<Price>)));
            
        }
        /// <summary>
        /// Update pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns></returns>
        public void PricingModuleUpdatePriceList(Pricelist priceList)
        {
             PricingModuleUpdatePriceListWithHttpInfo(priceList);
        }

        /// <summary>
        /// Update pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PricingModuleUpdatePriceListWithHttpInfo(Pricelist priceList)
        {
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleUpdatePriceList");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceList: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceList: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task PricingModuleUpdatePriceListAsync(Pricelist priceList)
        {
             await PricingModuleUpdatePriceListAsyncWithHttpInfo(priceList);

        }

        /// <summary>
        /// Update pricelist 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="priceList"></param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdatePriceListAsyncWithHttpInfo(Pricelist priceList)
        {
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleUpdatePriceList");

            var localVarPath = "/api/pricing/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceList: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceList: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Update pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns></returns>
        public void PricingModuleUpdatePriceListAssignment(PricelistAssignment assignment)
        {
             PricingModuleUpdatePriceListAssignmentWithHttpInfo(assignment);
        }

        /// <summary>
        /// Update pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PricingModuleUpdatePriceListAssignmentWithHttpInfo(PricelistAssignment assignment)
        {
            // verify the required parameter 'assignment' is set
            if (assignment == null)
                throw new ApiException(400, "Missing required parameter 'assignment' when calling VirtoCommercePricingApi->PricingModuleUpdatePriceListAssignment");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (assignment.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(assignment); // http body (model) parameter
            }
            else
            {
                localVarPostBody = assignment; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceListAssignment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceListAssignment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task PricingModuleUpdatePriceListAssignmentAsync(PricelistAssignment assignment)
        {
             await PricingModuleUpdatePriceListAssignmentAsyncWithHttpInfo(assignment);

        }

        /// <summary>
        /// Update pricelist assignment 
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="assignment">PricelistAssignment</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdatePriceListAssignmentAsyncWithHttpInfo(PricelistAssignment assignment)
        {
            // verify the required parameter 'assignment' is set
            if (assignment == null)
                throw new ApiException(400, "Missing required parameter 'assignment' when calling VirtoCommercePricingApi->PricingModuleUpdatePriceListAssignment");

            var localVarPath = "/api/pricing/assignments";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (assignment.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(assignment); // http body (model) parameter
            }
            else
            {
                localVarPostBody = assignment; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceListAssignment: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdatePriceListAssignment: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Update prices Update prices of product for given pricelist.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns></returns>
        public void PricingModuleUpdateProductPriceLists(string productId, Pricelist priceList)
        {
             PricingModuleUpdateProductPriceListsWithHttpInfo(productId, priceList);
        }

        /// <summary>
        /// Update prices Update prices of product for given pricelist.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PricingModuleUpdateProductPriceListsWithHttpInfo(string productId, Pricelist priceList)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleUpdateProductPriceLists");
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleUpdateProductPriceLists");

            var localVarPath = "/api/catalog/products/{productId}/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdateProductPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdateProductPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update prices Update prices of product for given pricelist.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task PricingModuleUpdateProductPriceListsAsync(string productId, Pricelist priceList)
        {
             await PricingModuleUpdateProductPriceListsAsyncWithHttpInfo(productId, priceList);

        }

        /// <summary>
        /// Update prices Update prices of product for given pricelist.
        /// </summary>
        /// <exception cref="VirtoCommerce.PricingModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="productId">Product id</param>
        /// <param name="priceList">Pricelist with new product prices</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PricingModuleUpdateProductPriceListsAsyncWithHttpInfo(string productId, Pricelist priceList)
        {
            // verify the required parameter 'productId' is set
            if (productId == null)
                throw new ApiException(400, "Missing required parameter 'productId' when calling VirtoCommercePricingApi->PricingModuleUpdateProductPriceLists");
            // verify the required parameter 'priceList' is set
            if (priceList == null)
                throw new ApiException(400, "Missing required parameter 'priceList' when calling VirtoCommercePricingApi->PricingModuleUpdateProductPriceLists");

            var localVarPath = "/api/catalog/products/{productId}/pricelists";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (productId != null) localVarPathParams.Add("productId", ApiClient.ParameterToString(productId)); // path parameter
            if (priceList.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(priceList); // http body (model) parameter
            }
            else
            {
                localVarPostBody = priceList; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdateProductPriceLists: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling PricingModuleUpdateProductPriceLists: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
    }
}

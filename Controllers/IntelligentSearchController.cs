using Microsoft.AspNetCore.Mvc;
using WholesaleRetailStore.Services;
using WholesaleRetailStore.Models;

namespace WholesaleRetailStore.Controllers
{
    /// <summary>
    /// API controller for AI-enhanced intelligent search functionality.
    /// Provides smart product search with synonyms, fuzzy matching, and intelligent ranking.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IntelligentSearchController : ControllerBase
    {
        private readonly IntelligentSearchService _searchService;

        public IntelligentSearchController(IntelligentSearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Performs intelligent product search with AI-powered ranking.
        /// </summary>
        /// <param name="query">Search query string</param>
        /// <param name="maxResults">Maximum number of results to return (default: 20)</param>
        /// <returns>List of intelligently ranked products</returns>
        /// <response code="200">Returns the intelligently ranked search results</response>
        /// <response code="400">If the search query is invalid</response>
        [HttpGet("search")]
        public async Task<ActionResult<List<Product>>> IntelligentSearch(
            [FromQuery] string query,
            [FromQuery] int maxResults = 20)
        {
            try
            {
                if (maxResults <= 0 || maxResults > 100)
                {
                    return BadRequest("maxResults must be between 1 and 100");
                }

                var results = await _searchService.IntelligentSearchAsync(query, maxResults);
                
                return Ok(new
                {
                    Query = query,
                    ResultCount = results.Count,
                    MaxResults = maxResults,
                    Products = results,
                    SearchType = "AI-Enhanced Intelligent Search"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred during intelligent search", Details = ex.Message });
            }
        }

        /// <summary>
        /// Gets intelligent search suggestions based on partial input.
        /// </summary>
        /// <param name="partial">Partial search query</param>
        /// <param name="maxSuggestions">Maximum suggestions to return (default: 5)</param>
        /// <returns>List of intelligent search suggestions</returns>
        /// <response code="200">Returns the search suggestions</response>
        [HttpGet("suggestions")]
        public async Task<ActionResult<List<string>>> GetSearchSuggestions(
            [FromQuery] string partial,
            [FromQuery] int maxSuggestions = 5)
        {
            try
            {
                if (maxSuggestions <= 0 || maxSuggestions > 20)
                {
                    return BadRequest("maxSuggestions must be between 1 and 20");
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(partial, maxSuggestions);
                
                return Ok(new
                {
                    PartialQuery = partial,
                    Suggestions = suggestions,
                    SuggestionCount = suggestions.Count,
                    SearchType = "AI-Powered Auto-Complete"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred getting search suggestions", Details = ex.Message });
            }
        }

        /// <summary>
        /// Gets intelligent product categories for enhanced filtering.
        /// </summary>
        /// <returns>List of AI-suggested categories</returns>
        /// <response code="200">Returns the intelligent categories</response>
        [HttpGet("categories")]
        public ActionResult<List<string>> GetIntelligentCategories()
        {
            try
            {
                var categories = _searchService.GetIntelligentCategories();
                
                return Ok(new
                {
                    Categories = categories,
                    CategoryCount = categories.Count,
                    SearchType = "AI-Powered Category Classification"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred getting categories", Details = ex.Message });
            }
        }

        /// <summary>
        /// Gets related search terms based on AI analysis of the query.
        /// </summary>
        /// <param name="query">Original search query</param>
        /// <returns>List of AI-suggested related terms</returns>
        /// <response code="200">Returns related search terms</response>
        [HttpGet("related")]
        public ActionResult<List<string>> GetRelatedSearchTerms([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Query parameter is required");
                }

                var relatedTerms = _searchService.GetRelatedSearchTerms(query);
                
                return Ok(new
                {
                    OriginalQuery = query,
                    RelatedTerms = relatedTerms,
                    RelatedCount = relatedTerms.Count,
                    SearchType = "AI-Powered Semantic Analysis"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred getting related terms", Details = ex.Message });
            }
        }

        /// <summary>
        /// Comprehensive intelligent search with all AI features combined.
        /// </summary>
        /// <param name="query">Search query string</param>
        /// <returns>Comprehensive search results with suggestions and related terms</returns>
        /// <response code="200">Returns comprehensive AI-enhanced search results</response>
        [HttpGet("comprehensive")]
        public async Task<ActionResult> ComprehensiveIntelligentSearch([FromQuery] string query)
        {
            try
            {
                var searchResults = await _searchService.IntelligentSearchAsync(query, 10);
                var suggestions = await _searchService.GetSearchSuggestionsAsync(query, 5);
                var relatedTerms = _searchService.GetRelatedSearchTerms(query);
                var categories = _searchService.GetIntelligentCategories();

                return Ok(new
                {
                    Query = query,
                    SearchResults = new
                    {
                        Products = searchResults,
                        Count = searchResults.Count
                    },
                    Suggestions = suggestions,
                    RelatedTerms = relatedTerms,
                    Categories = categories,
                    SearchType = "Comprehensive AI-Enhanced Search",
                    Features = new[]
                    {
                        "Synonym Matching",
                        "Fuzzy Search",
                        "Intelligent Ranking",
                        "Auto-Complete Suggestions",
                        "Semantic Analysis",
                        "Category Classification",
                        "Stock-Aware Scoring"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred during comprehensive search", Details = ex.Message });
            }
        }
    }
}

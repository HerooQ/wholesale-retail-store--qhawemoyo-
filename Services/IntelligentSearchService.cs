using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace WholesaleRetailStore.Services
{
    /// <summary>
    /// AI-enhanced search service that provides intelligent product search with synonym matching,
    /// fuzzy search, and smart ranking algorithms.
    /// </summary>
    public class IntelligentSearchService
    {
        private readonly AppDbContext _context;
        
        // AI-powered synonym dictionary for intelligent search
        private readonly Dictionary<string, List<string>> _synonyms = new()
        {
            // Technology synonyms
            ["laptop"] = new() { "notebook", "computer", "pc", "macbook", "chromebook" },
            ["headphones"] = new() { "earphones", "earbuds", "headset", "audio", "music" },
            ["speaker"] = new() { "audio", "sound", "bluetooth", "wireless", "music" },
            ["watch"] = new() { "smartwatch", "wearable", "fitness", "tracker", "time" },
            ["cable"] = new() { "cord", "wire", "connector", "usb", "charging" },
            ["webcam"] = new() { "camera", "video", "streaming", "conference", "meeting" },
            
            // Feature-based synonyms
            ["wireless"] = new() { "bluetooth", "cordless", "portable", "mobile" },
            ["smart"] = new() { "intelligent", "connected", "digital", "electronic" },
            ["portable"] = new() { "mobile", "compact", "travel", "lightweight" },
            ["gaming"] = new() { "game", "gamer", "esports", "entertainment" },
            ["professional"] = new() { "business", "work", "office", "enterprise" },
            
            // Brand/quality synonyms
            ["premium"] = new() { "high-end", "quality", "professional", "advanced" },
            ["budget"] = new() { "affordable", "cheap", "economical", "basic" },
            ["durable"] = new() { "strong", "robust", "reliable", "quality" },
            
            // Use case synonyms
            ["work"] = new() { "office", "business", "professional", "productivity" },
            ["home"] = new() { "personal", "family", "domestic", "household" },
            ["travel"] = new() { "portable", "mobile", "compact", "lightweight" }
        };

        // Category keywords for intelligent classification
        private readonly Dictionary<string, List<string>> _categories = new()
        {
            ["Audio"] = new() { "speaker", "headphones", "earphones", "sound", "music", "audio", "bluetooth" },
            ["Computing"] = new() { "laptop", "computer", "pc", "stand", "notebook", "macbook" },
            ["Wearables"] = new() { "watch", "smartwatch", "fitness", "tracker", "wearable" },
            ["Accessories"] = new() { "cable", "cord", "cover", "case", "connector", "usb" },
            ["Video"] = new() { "webcam", "camera", "video", "streaming", "conference" }
        };

        public IntelligentSearchService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs intelligent product search with AI-powered ranking and synonym matching.
        /// </summary>
        /// <param name="query">Search query from user</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>Intelligently ranked list of products</returns>
        public async Task<List<Product>> IntelligentSearchAsync(string query, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _context.Products.Take(maxResults).ToListAsync();
            }

            var products = await _context.Products.ToListAsync();
            var searchResults = new List<(Product Product, double Score)>();

            // Normalize query for better matching
            var normalizedQuery = NormalizeSearchTerm(query);
            var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var product in products)
            {
                var score = CalculateIntelligentScore(product, normalizedQuery, queryWords);
                if (score > 0)
                {
                    searchResults.Add((product, score));
                }
            }

            // Return top results sorted by AI-calculated relevance score
            return searchResults
                .OrderByDescending(x => x.Score)
                .Take(maxResults)
                .Select(x => x.Product)
                .ToList();
        }

        /// <summary>
        /// Gets intelligent search suggestions based on partial input.
        /// </summary>
        /// <param name="partialQuery">Partial search query</param>
        /// <param name="maxSuggestions">Maximum suggestions to return</param>
        /// <returns>List of intelligent search suggestions</returns>
        public async Task<List<string>> GetSearchSuggestionsAsync(string partialQuery, int maxSuggestions = 5)
        {
            if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            {
                return new List<string>();
            }

            var suggestions = new HashSet<string>();
            var normalizedQuery = NormalizeSearchTerm(partialQuery);

            // Get product-based suggestions
            var products = await _context.Products.ToListAsync();
            foreach (var product in products)
            {
                var productWords = NormalizeSearchTerm($"{product.Name} {product.Description}").Split(' ');
                foreach (var word in productWords)
                {
                    if (word.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase) && word.Length > normalizedQuery.Length)
                    {
                        suggestions.Add(word);
                    }
                }
            }

            // Add synonym-based suggestions
            foreach (var synonymGroup in _synonyms)
            {
                if (synonymGroup.Key.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(synonymGroup.Key);
                }
                foreach (var synonym in synonymGroup.Value)
                {
                    if (synonym.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(synonym);
                    }
                }
            }

            return suggestions.Take(maxSuggestions).ToList();
        }

        /// <summary>
        /// Calculates intelligent relevance score using AI-inspired algorithms.
        /// </summary>
        private double CalculateIntelligentScore(Product product, string normalizedQuery, string[] queryWords)
        {
            double score = 0;
            var productText = NormalizeSearchTerm($"{product.Name} {product.Description}");
            var productWords = productText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var queryWord in queryWords)
            {
                // Exact match gets highest score
                if (productText.Contains(queryWord, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                    
                    // Bonus for name matches
                    if (NormalizeSearchTerm(product.Name).Contains(queryWord, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 15;
                    }
                }

                // Fuzzy matching with Levenshtein-inspired algorithm
                foreach (var productWord in productWords)
                {
                    var similarity = CalculateStringSimilarity(queryWord, productWord);
                    if (similarity > 0.7) // 70% similarity threshold
                    {
                        score += similarity * 5;
                    }
                }

                // Synonym matching - AI-powered semantic search
                foreach (var synonymGroup in _synonyms)
                {
                    if (synonymGroup.Key.Equals(queryWord, StringComparison.OrdinalIgnoreCase) ||
                        synonymGroup.Value.Any(s => s.Equals(queryWord, StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (var synonym in synonymGroup.Value.Concat(new[] { synonymGroup.Key }))
                        {
                            if (productText.Contains(synonym, StringComparison.OrdinalIgnoreCase))
                            {
                                score += 7; // High score for semantic matches
                            }
                        }
                    }
                }

                // Category-based intelligent matching
                foreach (var category in _categories)
                {
                    if (category.Value.Any(keyword => keyword.Equals(queryWord, StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (var categoryKeyword in category.Value)
                        {
                            if (productText.Contains(categoryKeyword, StringComparison.OrdinalIgnoreCase))
                            {
                                score += 5;
                            }
                        }
                    }
                }
            }

            // Boost score for products in stock (business logic enhancement)
            if (product.Stock > 0)
            {
                score *= 1.2;
            }

            // Penalty for very low stock (smart inventory awareness)
            if (product.Stock <= 2 && product.Stock > 0)
            {
                score *= 0.9;
            }

            return score;
        }

        /// <summary>
        /// Calculates string similarity using a simplified Levenshtein distance algorithm.
        /// </summary>
        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase))
                return 1.0;

            var maxLength = Math.Max(str1.Length, str2.Length);
            var distance = CalculateLevenshteinDistance(str1.ToLower(), str2.ToLower());
            
            return 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Calculates Levenshtein distance for fuzzy string matching.
        /// </summary>
        private int CalculateLevenshteinDistance(string source, string target)
        {
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            var matrix = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= target.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[source.Length, target.Length];
        }

        /// <summary>
        /// Normalizes search terms for consistent matching.
        /// </summary>
        private string NormalizeSearchTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return string.Empty;

            // Remove special characters and normalize spacing
            term = Regex.Replace(term, @"[^\w\s]", " ");
            term = Regex.Replace(term, @"\s+", " ");
            
            return term.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Gets intelligent product categories based on search patterns.
        /// </summary>
        /// <returns>List of suggested categories</returns>
        public List<string> GetIntelligentCategories()
        {
            return _categories.Keys.ToList();
        }

        /// <summary>
        /// Analyzes search query and suggests related terms using AI logic.
        /// </summary>
        /// <param name="query">Original search query</param>
        /// <returns>List of related search terms</returns>
        public List<string> GetRelatedSearchTerms(string query)
        {
            var relatedTerms = new HashSet<string>();
            var normalizedQuery = NormalizeSearchTerm(query);
            var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in queryWords)
            {
                // Add synonyms
                foreach (var synonymGroup in _synonyms)
                {
                    if (synonymGroup.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        relatedTerms.UnionWith(synonymGroup.Value.Take(3));
                    }
                    else if (synonymGroup.Value.Any(s => s.Equals(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        relatedTerms.Add(synonymGroup.Key);
                        relatedTerms.UnionWith(synonymGroup.Value.Where(s => !s.Equals(word, StringComparison.OrdinalIgnoreCase)).Take(2));
                    }
                }

                // Add category-based related terms
                foreach (var category in _categories)
                {
                    if (category.Value.Any(keyword => keyword.Equals(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        relatedTerms.UnionWith(category.Value.Where(k => !k.Equals(word, StringComparison.OrdinalIgnoreCase)).Take(3));
                    }
                }
            }

            return relatedTerms.Take(6).ToList();
        }
    }
}

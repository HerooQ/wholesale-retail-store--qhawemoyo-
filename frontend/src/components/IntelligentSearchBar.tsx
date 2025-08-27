import React, { useState, useEffect, useRef } from 'react';
import { Search, Sparkles, Brain, Zap, X } from 'lucide-react';
import { ApiService } from '../services/api';

interface IntelligentSearchBarProps {
  onSearch: (query: string, useAI?: boolean) => void;
  onSuggestionSelect?: (suggestion: string) => void;
  placeholder?: string;
  className?: string;
}

interface SearchSuggestion {
  text: string;
  type: 'suggestion' | 'related' | 'category';
  icon: React.ReactNode;
}

export const IntelligentSearchBar: React.FC<IntelligentSearchBarProps> = ({
  onSearch,
  onSuggestionSelect,
  placeholder = "Search products with AI...",
  className = ""
}) => {
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState<SearchSuggestion[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [isAIEnabled, setIsAIEnabled] = useState(false); // Start with AI disabled until endpoints are verified
  const [isLoading, setIsLoading] = useState(false);
  const [relatedTerms, setRelatedTerms] = useState<string[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  
  const searchRef = useRef<HTMLInputElement>(null);
  const suggestionsRef = useRef<HTMLDivElement>(null);
  const debounceRef = useRef<NodeJS.Timeout | null>(null);

  // Load AI categories on component mount
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const result = await ApiService.getIntelligentCategories();
        setCategories(result.Categories);
      } catch (error) {
        console.error('Failed to load AI categories:', error);
      }
    };
    loadCategories();
  }, []);

  // Debounced search suggestions
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    if (query.length >= 2 && isAIEnabled) {
      debounceRef.current = setTimeout(async () => {
        await loadSuggestions(query);
      }, 300);
    } else {
      setSuggestions([]);
      setRelatedTerms([]);
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [query, isAIEnabled]);

  const loadSuggestions = async (searchQuery: string) => {
    if (!searchQuery.trim()) return;
    
    setIsLoading(true);
    try {
      const [suggestionsResult, relatedResult] = await Promise.all([
        ApiService.getSearchSuggestions(searchQuery, 5).catch(() => ({ Suggestions: [] })),
        ApiService.getRelatedSearchTerms(searchQuery).catch(() => ({ RelatedTerms: [] }))
      ]);

      const newSuggestions: SearchSuggestion[] = [];

      // Add auto-complete suggestions
      if (suggestionsResult?.Suggestions && Array.isArray(suggestionsResult.Suggestions)) {
        suggestionsResult.Suggestions.forEach(suggestion => {
          newSuggestions.push({
            text: suggestion,
            type: 'suggestion',
            icon: <Search className="w-4 h-4 text-gray-400" />
          });
        });
      }

      // Add related terms with AI icon
      if (relatedResult?.RelatedTerms && Array.isArray(relatedResult.RelatedTerms)) {
        relatedResult.RelatedTerms.slice(0, 3).forEach(term => {
          if (!newSuggestions.some(s => s.text.toLowerCase() === term.toLowerCase())) {
            newSuggestions.push({
              text: term,
              type: 'related',
              icon: <Brain className="w-4 h-4 text-purple-500" />
            });
          }
        });
      }

      // Add relevant categories
      categories.slice(0, 2).forEach(category => {
        if (category.toLowerCase().includes(searchQuery.toLowerCase()) ||
            searchQuery.toLowerCase().includes(category.toLowerCase())) {
          newSuggestions.push({
            text: category,
            type: 'category',
            icon: <Zap className="w-4 h-4 text-blue-500" />
          });
        }
      });

      setSuggestions(newSuggestions.slice(0, 8));
      setRelatedTerms(relatedResult.RelatedTerms || []);
    } catch (error) {
      console.error('Failed to load AI suggestions:', error);
      setSuggestions([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    setShowSuggestions(true);
  };

  const handleSearch = () => {
    if (query.trim()) {
      onSearch(query.trim(), isAIEnabled);
      setShowSuggestions(false);
      searchRef.current?.blur();
    }
  };

  const handleSuggestionClick = (suggestion: SearchSuggestion) => {
    setQuery(suggestion.text);
    setShowSuggestions(false);
    onSuggestionSelect?.(suggestion.text);
    onSearch(suggestion.text, isAIEnabled);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    } else if (e.key === 'Escape') {
      setShowSuggestions(false);
      searchRef.current?.blur();
    }
  };

  const clearSearch = () => {
    setQuery('');
    setSuggestions([]);
    setRelatedTerms([]);
    setShowSuggestions(false);
    searchRef.current?.focus();
  };

  const toggleAI = () => {
    setIsAIEnabled(!isAIEnabled);
    if (!isAIEnabled && query.length >= 2) {
      loadSuggestions(query);
    } else {
      setSuggestions([]);
      setRelatedTerms([]);
    }
  };

  // Close suggestions when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (suggestionsRef.current && !suggestionsRef.current.contains(event.target as Node) &&
          searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowSuggestions(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const getSuggestionTypeLabel = (type: string) => {
    switch (type) {
      case 'suggestion': return 'Auto-complete';
      case 'related': return 'AI Related';
      case 'category': return 'Category';
      default: return '';
    }
  };

  const getSuggestionTypeColor = (type: string) => {
    switch (type) {
      case 'suggestion': return 'text-gray-500';
      case 'related': return 'text-purple-500';
      case 'category': return 'text-blue-500';
      default: return 'text-gray-500';
    }
  };

  return (
    <div className={`relative ${className}`}>
      <div className="relative">
        {/* Search Input */}
        <div className="relative">
          <input
            ref={searchRef}
            type="text"
            value={query}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            onFocus={() => query.length >= 2 && setShowSuggestions(true)}
            placeholder={placeholder}
            className="w-full pl-12 pr-20 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-gray-900 placeholder-gray-500"
          />
          
          {/* Search Icon */}
          <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          
          {/* Clear Button */}
          {query && (
            <button
              onClick={clearSearch}
              className="absolute right-16 top-1/2 transform -translate-y-1/2 p-1 hover:bg-gray-100 rounded-full"
            >
              <X className="w-4 h-4 text-gray-400" />
            </button>
          )}
          
          {/* AI Toggle Button */}
          <button
            onClick={toggleAI}
            className={`absolute right-2 top-1/2 transform -translate-y-1/2 p-2 rounded-lg transition-colors ${
              isAIEnabled 
                ? 'bg-gradient-to-r from-purple-500 to-blue-500 text-white shadow-md' 
                : 'bg-gray-100 text-gray-400 hover:bg-gray-200'
            }`}
            title={isAIEnabled ? 'AI Search Enabled' : 'Enable AI Search'}
          >
            <Sparkles className="w-4 h-4" />
          </button>
        </div>

        {/* AI Status Indicator */}
        {isAIEnabled && (
          <div className="absolute -bottom-6 left-0 flex items-center space-x-1 text-xs text-purple-600">
            <Brain className="w-3 h-3" />
            <span>AI-Enhanced Search Active</span>
          </div>
        )}
      </div>

      {/* Suggestions Dropdown */}
      {showSuggestions && suggestions.length > 0 && (
        <div 
          ref={suggestionsRef}
          className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-80 overflow-y-auto"
        >
          {isLoading && (
            <div className="px-4 py-3 text-center text-gray-500">
              <div className="flex items-center justify-center space-x-2">
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-purple-500"></div>
                <span>AI is thinking...</span>
              </div>
            </div>
          )}
          
          {suggestions.map((suggestion, index) => (
            <button
              key={`${suggestion.type}-${index}`}
              onClick={() => handleSuggestionClick(suggestion)}
              className="w-full px-4 py-3 text-left hover:bg-gray-50 border-b border-gray-100 last:border-b-0 flex items-center justify-between group"
            >
              <div className="flex items-center space-x-3">
                {suggestion.icon}
                <span className="text-gray-900 group-hover:text-gray-700">
                  {suggestion.text}
                </span>
              </div>
              <span className={`text-xs px-2 py-1 rounded-full bg-gray-100 ${getSuggestionTypeColor(suggestion.type)}`}>
                {getSuggestionTypeLabel(suggestion.type)}
              </span>
            </button>
          ))}
          
          {/* Related Terms Section */}
          {relatedTerms.length > 0 && (
            <div className="px-4 py-3 bg-gray-50 border-t border-gray-200">
              <div className="flex items-center space-x-2 mb-2">
                <Brain className="w-4 h-4 text-purple-500" />
                <span className="text-xs font-medium text-gray-600">AI Suggestions</span>
              </div>
              <div className="flex flex-wrap gap-2">
                {relatedTerms.slice(0, 6).map((term, index) => (
                  <button
                    key={index}
                    onClick={() => handleSuggestionClick({ text: term, type: 'related', icon: null })}
                    className="px-3 py-1 text-xs bg-purple-100 text-purple-700 rounded-full hover:bg-purple-200 transition-colors"
                  >
                    {term}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

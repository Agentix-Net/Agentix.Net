using System.Text;
using System.Text.RegularExpressions;

namespace Agentix.Rag.Embeddings.Local.Tokenization;

/// <summary>
/// Simple BERT tokenizer for local embedding processing.
/// Implements basic BERT tokenization without external dependencies.
/// </summary>
public class BertTokenizer
{
    private const int MaxSequenceLength = 512;
    private const int ClsTokenId = 101;
    private const int SepTokenId = 102;
    private const int PadTokenId = 0;
    private const int UnkTokenId = 100;

    /// <summary>
    /// Tokenizes the input text into BERT-compatible tokens.
    /// </summary>
    /// <param name="text">Input text to tokenize</param>
    /// <returns>Tokenization result with input IDs and attention mask</returns>
    public TokenizationResult Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return CreateEmptyResult();
        }

        // Basic text preprocessing
        var cleanedText = CleanText(text);
        
        // Simple word-level tokenization (basic implementation)
        var words = TokenizeToWords(cleanedText);
        
        // Convert to token IDs (simplified approach)
        var tokenIds = ConvertWordsToTokenIds(words);
        
        // Add special tokens and create attention mask
        return CreateTokenizationResult(tokenIds);
    }

    private static string CleanText(string text)
    {
        // Basic text cleaning
        text = text.Trim();
        text = Regex.Replace(text, @"\s+", " "); // Normalize whitespace
        return text;
    }

    private static List<string> TokenizeToWords(string text)
    {
        // Basic word tokenization (simplified)
        var words = new List<string>();
        
        // Split on whitespace and punctuation
        var tokens = Regex.Split(text, @"(\W+)")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        foreach (var token in tokens)
        {
            if (Regex.IsMatch(token, @"^\W+$"))
            {
                // Punctuation - add as-is
                words.Add(token.Trim());
            }
            else
            {
                // Word - convert to lowercase and add
                var word = token.ToLowerInvariant().Trim();
                if (!string.IsNullOrEmpty(word))
                {
                    words.Add(word);
                }
            }
        }

        return words;
    }

    private static List<int> ConvertWordsToTokenIds(List<string> words)
    {
        // Simplified token ID assignment
        // In a real implementation, this would use a vocabulary file
        var tokenIds = new List<int>();

        foreach (var word in words)
        {
            var tokenId = GetTokenId(word);
            tokenIds.Add(tokenId);
        }

        return tokenIds;
    }

    private static int GetTokenId(string word)
    {
        // Very simplified token ID generation
        // This is a basic hash-based approach for demonstration
        // In production, you'd use a proper vocabulary file
        
        if (string.IsNullOrEmpty(word))
            return UnkTokenId;

        // Use a simple hash for consistent token IDs
        var hash = word.GetHashCode();
        var tokenId = Math.Abs(hash % 29000) + 1000; // Keep in reasonable range
        
        return tokenId;
    }

    private static TokenizationResult CreateTokenizationResult(List<int> tokenIds)
    {
        // Add [CLS] token at the beginning
        var inputIds = new List<int> { ClsTokenId };
        inputIds.AddRange(tokenIds);
        
        // Add [SEP] token at the end
        inputIds.Add(SepTokenId);
        
        // Truncate if too long
        if (inputIds.Count > MaxSequenceLength)
        {
            inputIds = inputIds.Take(MaxSequenceLength - 1).ToList();
            inputIds.Add(SepTokenId);
        }
        
        // Pad to consistent length for batching
        var targetLength = Math.Min(inputIds.Count + (8 - inputIds.Count % 8), MaxSequenceLength);
        while (inputIds.Count < targetLength)
        {
            inputIds.Add(PadTokenId);
        }
        
        // Create attention mask (1 for real tokens, 0 for padding)
        var attentionMask = new int[inputIds.Count];
        for (int i = 0; i < inputIds.Count; i++)
        {
            attentionMask[i] = inputIds[i] == PadTokenId ? 0 : 1;
        }
        
        return new TokenizationResult
        {
            InputIds = inputIds.ToArray(),
            AttentionMask = attentionMask
        };
    }

    private static TokenizationResult CreateEmptyResult()
    {
        // Return minimal valid result for empty input
        var inputIds = new[] { ClsTokenId, SepTokenId };
        var attentionMask = new[] { 1, 1 };
        
        return new TokenizationResult
        {
            InputIds = inputIds,
            AttentionMask = attentionMask
        };
    }
}

/// <summary>
/// Result of text tokenization.
/// </summary>
public class TokenizationResult
{
    /// <summary>
    /// Token IDs for the input text.
    /// </summary>
    public int[] InputIds { get; set; } = Array.Empty<int>();
    
    /// <summary>
    /// Attention mask indicating which tokens are padding.
    /// </summary>
    public int[] AttentionMask { get; set; } = Array.Empty<int>();
} 
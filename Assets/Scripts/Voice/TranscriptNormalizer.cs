using System.Text.RegularExpressions;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// The single seam between raw ASR output and <see cref="VoiceCommandMatcher"/>. Produces
    /// a derived, matcher-ready string -- the raw transcript passed in is never modified in
    /// place and must still be logged/stored verbatim by the caller. Isolated here so it can be
    /// tuned or replaced without touching either the ASR backend or the matcher.
    ///
    /// Whisper's output differs from plain keyword text in ways that can break keyword matching even when
    /// recognition itself is correct: it's capitalized and punctuated, and clause boundaries
    /// (a spoken "and") can arrive as a comma or period instead of the word. This class fixes
    /// both without changing the matcher's logic -- the matcher still just splits on the literal
    /// words "and"/"then" and lowercase-tokenizes; this class makes sure a Whisper transcript
    /// presents that way after normalization.
    /// </summary>
    public static class TranscriptNormalizer
    {
        // Comma/period that sits between two clauses (more non-whitespace content follows)
        // carries the same meaning as a spoken "and" -- translate it rather than discard it, so
        // e.g. "show me the heart, focus in on it." keeps its command chaining. A trailing
        // sentence-final period/comma (nothing meaningful follows) is just stripped instead --
        // there's no second clause for it to separate.
        // Negative lookbehind/lookahead on digits so decimal points inside a number (e.g.
        // "40.5 percent") aren't mistaken for a clause boundary.
        private static readonly Regex ClauseBoundaryPunctuation =
            new Regex(@"(?<!\d)[,.]+(?!\d)\s*(?=\S)", RegexOptions.Compiled);

        private static readonly Regex TrailingPunctuation =
            new Regex(@"[,.!?;:]+\s*$", RegexOptions.Compiled);

        public static string Normalize(string rawTranscript)
        {
            if (string.IsNullOrWhiteSpace(rawTranscript))
                return string.Empty;

            string text = rawTranscript;

            // Clause boundaries first (must run before case-folding only in the sense that
            // punctuation detection doesn't care about case, so order vs. lowercasing doesn't
            // matter here -- kept explicit for readability).
            text = ClauseBoundaryPunctuation.Replace(text, " and ");
            text = TrailingPunctuation.Replace(text, string.Empty);

            // Case-insensitive matching -- do it once here rather than in the matcher, since
            // Whisper capitalizes sentences/proper-noun-like
            // anatomical terms.
            text = text.ToLowerInvariant();

            return text.Trim();
        }
    }
}

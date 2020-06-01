// © James Singleton. EUPL-1.2 (see the LICENSE file for the full license governing this code).

namespace octoplot.api
{
    public class Product
    {
        /// <summary>
        /// Number of results (in all pages)
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// URL of next page (may be null)
        /// </summary>
        public string? Next { get; set; }

        /// <summary>
        /// URL of previous page (may be null)
        /// </summary>
        public string? Previous { get; set; }

        /// <summary>
        /// Rates in half-hour periods
        /// </summary>
        public Rate[]? Results { get; set; }
    }
}

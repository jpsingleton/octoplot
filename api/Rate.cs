// © James Singleton. EUPL-1.2 (see the LICENSE file for the full license governing this code).

using System;

namespace octoplot.api
{
    public class Rate
    {
        /// <summary>
        /// Price per kWh (in pence) excluding VAT
        /// </summary>
        public decimal Value_Exc_Vat { get; set; }

        /// <summary>
        /// Price per kWh (in pence) including VAT
        /// </summary>
        public decimal Value_Inc_Vat { get; set; }

        /// <summary>
        /// Start of period (UTC)
        /// </summary>
        public DateTime Valid_From { get; set; }

        /// <summary>
        /// End of period (UTC)
        /// </summary>
        public DateTime Valid_To { get; set; }
    }
}

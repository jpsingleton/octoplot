// © James Singleton. EUPL-1.2 (see the LICENSE file for the full license governing this code).

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using octoplot.api;

namespace octoplot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Grid Supply Point - https://en.wikipedia.org/wiki/Distribution_network_operator
            var gsp = Region.NorthScotland;
            if (args.Length == 1 && args[0].Length == 1)
            {
                gsp = args[0].ToUpper();
            }

            var url = $"products/AGILE-18-02-21/electricity-tariffs/E-1R-AGILE-18-02-21-{gsp}/standard-unit-rates/";

            Console.WriteLine("Getting pricing data...");
            Console.WriteLine();

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.octopus.energy/v1/")
            };
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not get pricing data! :(");
                Console.WriteLine(response.ReasonPhrase);
                return;
            }

            var result = await response.Content.ReadAsAsync<Product>();
            var futureResults = result.Results.Where(r => r.Valid_To > DateTime.UtcNow).ToList();

            var gmtZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            var start = TimeZoneInfo.ConvertTimeFromUtc(futureResults.Min(r => r.Valid_From), gmtZone);
            var end = TimeZoneInfo.ConvertTimeFromUtc(futureResults.Max(r => r.Valid_To), gmtZone);

            Console.WriteLine($"Pricing data from {start:ddd dd MMM HH:mm} to {end:ddd dd MMM HH:mm} for region {gsp}");
            Console.WriteLine();

            // Main body of graph
            var min = futureResults.Min(r => r.Value_Inc_Vat);
            var max = futureResults.Max(r => r.Value_Inc_Vat);
            var ratesInTimeOrder = futureResults.OrderBy(r => r.Valid_From).ToList();
            for (var ii = Math.Round(max, MidpointRounding.AwayFromZero);
                ii > Math.Min(Math.Round(min, MidpointRounding.AwayFromZero), 0);
                ii--)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (ii >= 0) Console.ForegroundColor = ConsoleColor.White;
                if (ii > 7) Console.ForegroundColor = ConsoleColor.Yellow;
                if (ii > 15) Console.ForegroundColor = ConsoleColor.Red;

                if (ii >= 0) Console.Write(" ");
                Console.Write($"{ii:00}p ");
                foreach (var rate in ratesInTimeOrder)
                {
                    Console.Write(rate.Value_Inc_Vat > ii ? "£" : " ");
                }

                if (ii == 0)
                {
                    Console.WriteLine();
                    Console.Write("-----");
                    for (var jj = 0; jj < ratesInTimeOrder.Count; jj++)
                    {
                        Console.Write("-");
                    }
                }

                Console.WriteLine();
            }

            Console.ResetColor();

            // X-axis time scale
            var hours = ratesInTimeOrder
                .Where(r => r.Valid_From.Minute == 0)
                .Select(r => $"{TimeZoneInfo.ConvertTimeFromUtc(r.Valid_From, gmtZone).Hour:00}").ToList();

            Console.Write("     ");
            foreach (var tens in hours.Select(h => h.First()))
            {
                Console.Write($"{(tens != '0' ? tens : ' ')} ");
            }

            Console.WriteLine();
            Console.Write("     ");
            foreach (var ones in hours.Select(h => h.Last()))
            {
                Console.Write($"{ones} ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // Textual message
            var minPeriods = ratesInTimeOrder.Where(r => r.Value_Inc_Vat == min)
                .OrderBy(r => r.Valid_From).ToList();

            var minPeriod = minPeriods.FirstOrDefault();
            if (null == minPeriod) return;

            // Use first and last date if same price over multiple _contiguous_ periods
            if (minPeriods.Count > 1)
            {
                foreach (var period in minPeriods.Skip(1))
                {
                    if (minPeriod.Valid_To == period.Valid_From)
                    {
                        minPeriod.Valid_To = period.Valid_To;
                    }
                }
            }

            var prefix = minPeriod.Value_Inc_Vat < 0 ? "Paid most for electricity" :
                minPeriod.Value_Inc_Vat == 0 ? "Free electricity" : "Electricity is cheapest";

            Console.WriteLine(
                $"{prefix} between {TimeZoneInfo.ConvertTimeFromUtc(minPeriod.Valid_From, gmtZone):ddd dd MMM HH:mm} " +
                $"and {TimeZoneInfo.ConvertTimeFromUtc(minPeriod.Valid_To, gmtZone):ddd dd MMM HH:mm} " +
                $"({minPeriod.Value_Inc_Vat}p/kWh inc. VAT)");

            Console.WriteLine();
        }
    }
}

using Newtonsoft.Json;
using SBPriceCheckerCore.Models;
using SBPriceCheckerCore.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBPriceCheckerCore.Helpers
{
    public class Helper
    {
        private static string DECIMAL_SEPARATOR = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private static string GROUP_SEPARATOR = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator;

        public double ConvertPTNumberStrToDouble(string strValue)
        {
            if (strValue.Contains(GROUP_SEPARATOR))
            {
                string valueRep = strValue.Replace(GROUP_SEPARATOR, DECIMAL_SEPARATOR);
                return Convert.ToDouble(valueRep);
            }
            else
            {
                if (DECIMAL_SEPARATOR.Equals(","))
                    strValue = strValue.Replace(".", ",");

                return Convert.ToDouble(strValue);
            }
        }

        private readonly string path_ = Path.GetTempPath();

        public async Task InsertBeersRecordAsync(List<Beer> beersList, string store)
        {
            DateTime date = DateTime.Now;

            string dateString = String.Format("{0:HH_dd-MM-yyyy}", date);
            string fileName = dateString + "_" + store + ".prices";

            var filePath = Path.Combine(path_, fileName);

            using (var stream = File.OpenWrite(filePath))
            using (var writer = new StreamWriter(stream))
                await writer.WriteAsync(JsonConvert.SerializeObject(beersList)).ConfigureAwait(false);
        }

        public bool PricesInCache(string store)
        {
            DateTime date = DateTime.Now;

            string dateString = String.Format("{0:HH_dd-MM-yyyy}", date);
            string fileName = dateString + "_" + store + ".prices";

            var filePath = Path.Combine(path_, fileName);

            if (File.Exists(filePath))
            {
                return true;
            }

            return false;
        }

        public async Task<string> ReadBeersRecordAsync(string store)
        {
            DateTime date = DateTime.Now;

            string dateString = String.Format("{0:HH_dd-MM-yyyy}", date);
            string fileName = dateString + "_" + store + ".prices";

            var filePath = Path.Combine(path_, fileName);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
                return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        public async Task StorePricesInCache()
        {
            List<Beer> _allBeers = new List<Beer>();

            IEnumerable<Beer> beersContinente = await new Continente().GetBeers();
            _allBeers.AddRange(beersContinente);

            IEnumerable<Beer> beersJumbo = await new Jumbo().GetBeers();
            _allBeers.AddRange(beersJumbo);

            IEnumerable<Beer> beersElCorteIngles = await new ElCorteIngles().GetBeers();
            _allBeers.AddRange(beersElCorteIngles);

            //IEnumerable<Beer> beersIntermarche = await new Intermarche().GetBeers();
            //_allBeers.AddRange(beersIntermarche);

            if (_allBeers.Any())
                await InsertBeersRecordAsync(_allBeers, "APIBeer").ConfigureAwait(false);
        }
    }
}

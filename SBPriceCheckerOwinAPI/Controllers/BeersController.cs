using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Http;
using System.Net.Http;
using SBPriceCheckerCore.Models;
using SBPriceCheckerCore.Parsers;

namespace SBPriceCheckerOwinAPI.Controllers
{
    public class BeersController : ApiController
    {
        // Mock a data store:
        private static List<Beer> _Db = new List<Beer>
            {
                new Beer { Id = 1, Name = "Cerveja com Álcool", Total = 1, Capacity = 0.5, Price = 1.15, hasDiscount = false},
                new Beer { Id = 2, Name = "Cerveja com Álcool", Total = 20, Capacity = 0.25, Price = 14.49, hasDiscount = true},
                new Beer { Id = 3, Name = "Cerveja com Álcool Pack Económico", Total = 10, Capacity = 0.25, Price = 7.49, hasDiscount = true}
            };

        public IEnumerable<Beer> Get()
        {
            Console.WriteLine("Pedido efectuado");

            Continente parser = new Continente();
            return parser.GetBeers();

            //return _Db;
        }
    }
}

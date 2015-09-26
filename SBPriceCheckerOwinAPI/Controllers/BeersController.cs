using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Http;
using System.Net.Http;
using SBPriceCheckerCore.Models;
using SBPriceCheckerCore.Parsers;
using System.Web.OData;

namespace SBPriceCheckerOwinAPI.Controllers
{
    public class BeersController : ApiController
    {
        // Mock a data store:
        private static List<Beer> _Db = new List<Beer>
            {
                new Beer { id = 1, name = "Cerveja com Álcool", total = 1, capacity = 0.5, priceBefore = 1.15, hasDiscount = false},
                new Beer { id = 2, name = "Cerveja com Álcool", total = 20, capacity = 0.25, priceBefore = 14.49, hasDiscount = true},
                new Beer { id = 3, name = "Cerveja com Álcool Pack Económico", total = 10, capacity = 0.25, priceBefore = 7.49, hasDiscount = true}
            };

        [EnableQueryAttribute]
        public IQueryable<Beer> Get()
        {
            Continente parser = new Continente();
            return parser.GetBeers();

            //return _Db;
        }
    }
}

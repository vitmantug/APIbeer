using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Owin;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using SBPriceCheckerCore.Helpers;
using Newtonsoft.Json.Serialization;
using SpringComp.Owin;

namespace SBPriceCheckerOwinAPI
{
    public class Startup
    {
        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            // Microsoft Azure table columns can hold values less than or equal to 64KB.

            app.UseHttpTracking(
                new HttpTrackingOptions
                {
                    TrackingStore = new HttpTrackingStore(),
                    TrackingIdPropertyName = "x-tracking-id",
                    MaximumRecordedRequestLength = 64 * 1024,
                    MaximumRecordedResponseLength = 64 * 1024,
                }
                );

            var webApiConfiguration = ConfigureWebApi();
            // Use the extension method provided by the WebApi.Owin library:
            app.UseWebApi(webApiConfiguration);

            webApiConfiguration.MapHttpAttributeRoutes();
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            // If you do this in the WebApiConfig you will get JSON by default, 
            // but it will still allow you to return XML if you pass text/xml as the request Accept header
            //var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            //config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            // That makes sure you get json on most queries, but you can get xml when you send text/xml
            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // /api/beers?format=json
            //config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("format", "json", "application/json"));

            // Supporting only JSON in ASP.NET Web API – THE RIGHT WAY - NOT Working on Raspberry Pi
            //var jsonFormatter = new JsonMediaTypeFormatter();
            ////optional: set serializer settings here
            //config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));

            //the most common approach to support JSON only is to clear other formatters and leave only JsonMediaTypeFormatter around.
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.Indent = true;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return config;
        }
    }
}

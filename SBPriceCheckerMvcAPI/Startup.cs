using Microsoft.Owin;
using Owin;
using SpringComp.Owin;

[assembly: OwinStartupAttribute(typeof(SBPriceCheckerMvcAPI.Startup))]
namespace SBPriceCheckerMvcAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHttpTracking(
                new HttpTrackingOptions
                {
                    TrackingStore = new HttpTrackingStore(),
                    TrackingIdPropertyName = "x-tracking-id",
                    MaximumRecordedRequestLength = 64 * 1024,
                    MaximumRecordedResponseLength = 64 * 1024,
                }
            );

            ConfigureAuth(app);
        }
    }
}

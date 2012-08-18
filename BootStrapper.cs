using Nancy;
using Nancy.Conventions;

namespace ThatExtraMile.be
{
    public class BootStrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            Conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", "static"));
        }
    }
}
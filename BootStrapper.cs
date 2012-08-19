using Nancy;
using Nancy.Conventions;

namespace ThatExtraMile.be
{
    public class BootStrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            Conventions.StaticContentsConventions.Clear();
            Conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", "static"));
        }
    }
}
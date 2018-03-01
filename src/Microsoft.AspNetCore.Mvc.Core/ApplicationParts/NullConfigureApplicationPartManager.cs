namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class NullConfigureApplicationPartManager : ConfigureApplicationPartManager
    {
        public override void Configure(ApplicationPartManager partManager, AssemblyPartDiscoveryModel model)
        {
            // Do nothing
        }
    }
}

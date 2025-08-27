using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        // OptionSet value for msdyn_agreement
        private const int AgreementOptionSetValue = 805640009;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get services
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            trace.Trace("AgreementDeleteLogPlugin started.");

            // Only run on Delete
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Message is {context.MessageName}, not Delete. Exiting.");
                return;
            }

            // Validate target
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference target))
            {
                trace.Trace("Target missing or invalid. Exiting.");
                return;
            }

            if (!string.Equals(target.LogicalName, "msdyn_agreement", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Target entity is {target.LogicalName}, not msdyn_agreement. Exiting.");
                return;
            }

            try
            {
                var service = serviceFactory.CreateOrganizationService(null);

                // Create delete log
                var deleteLog = new Entity("gif_deletelog")
                {
                    ["gif_entityid"] = target.Id,                      // Just the GUID
                    ["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue),
                    ["gif_name"] = $"Agreement Deleted - {target.Id}"
                };

                service.Create(deleteLog);
                trace.Trace($"gif_deletelog created for Agreement {target.Id}");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex}");
                throw new InvalidPluginExecutionException("Error creating Agreement delete log", ex);
            }
        }
    }
}

using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get execution context
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("AgreementDeleteLogPlugin started.");

            // Ensure Delete message
            if (context.MessageName?.ToLower() != "delete")
            {
                trace.Trace($"Message is {context.MessageName}, not Delete. Exiting.");
                return;
            }

            // Ensure Target exists
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference target))
            {
                trace.Trace("Target missing or invalid. Exiting.");
                return;
            }

            // Ensure correct entity
            if (!string.Equals(target.LogicalName, "msdyn_agreement", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Target entity is {target.LogicalName}, not msdyn_agreement. Exiting.");
                return;
            }

            trace.Trace($"Agreement Delete detected: ID={target.Id}");

            try
            {
                // Use SYSTEM context to guarantee Create permissions
                var service = serviceFactory.CreateOrganizationService(null);

                // Create gif_deletelog record
                var deleteLog = new Entity("gif_deletelog");
                deleteLog["gif_name"] = $"Agreement Deleted - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                deleteLog["gif_entityid"] = target.Id.ToString();
                deleteLog["gif_entityname"] = target.LogicalName;
                deleteLog["gif_deletedon"] = DateTime.UtcNow;
                deleteLog["gif_deletedby"] = new EntityReference("systemuser", context.InitiatingUserId);

                service.Create(deleteLog);
                trace.Trace($"gif_deletelog record created for Agreement {target.Id}");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex}");
                throw new InvalidPluginExecutionException("Error creating Agreement delete log", ex);
            }
        }
    }
}

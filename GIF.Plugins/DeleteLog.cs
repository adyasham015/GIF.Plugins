using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        // Set the OptionSet integer value for "msdyn_agreement" in gif_entityname
        private const int AgreementOptionSetValue = 100000001; // Replace with actual value from your environment

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get execution context
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("AgreementDeleteLogPlugin started.");

            // Only run on Delete message
            if (context.MessageName?.ToLower() != "delete")
            {
                trace.Trace($"Message is {context.MessageName}, not Delete. Exiting.");
                return;
            }

            // Ensure Target exists and is EntityReference
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
                // SYSTEM context
                var service = serviceFactory.CreateOrganizationService(null);

                // Create gif_deletelog record
                var deleteLog = new Entity("gif_deletelog");

                // Required fields
                deleteLog["gif_name"] = $"Agreement Deleted - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                // gif_entityid: set as string (if Text) or EntityReference (if Lookup)
                deleteLog["gif_entityid"] = target.Id.ToString();

                // gif_entityname: OptionSetValue
                deleteLog["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue);

                deleteLog["gif_deletedon"] = DateTime.UtcNow;
                deleteLog["gif_deletedby"] = new EntityReference("systemuser", context.InitiatingUserId);

                // Trace before create
                trace.Trace($"Creating gif_deletelog record with Name={deleteLog["gif_name"]}, EntityId={deleteLog["gif_entityid"]}, EntityName={AgreementOptionSetValue}, DeletedBy={context.InitiatingUserId}");

                service.Create(deleteLog);

                trace.Trace($"gif_deletelog record successfully created for Agreement {target.Id}");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex}");
                throw new InvalidPluginExecutionException("Error creating Agreement delete log", ex);
            }
        }
    }
}

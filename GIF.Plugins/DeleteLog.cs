using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        private const string AgreementEntityName = "msdyn_agreement";
        private const string DeleteLogEntityName = "gif_deletelog";

        // OptionSet value for entity name (custom OptionSet)
        private const int AgreementOptionSetValue = 805640009;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get services
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("AgreementDeleteLogPlugin execution started.");

            // Ensure it's a Delete message
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"MessageName: {context.MessageName}. Exiting plugin (only runs on Delete).");
                return;
            }

            // Ensure Target exists and is an EntityReference
            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
            {
                trace.Trace("Target parameter is missing or not an EntityReference. Exiting.");
                return;
            }

            // Ensure it's the Agreement entity
            if (!string.Equals(target.LogicalName, AgreementEntityName, StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Target entity: {target.LogicalName}. Plugin only runs for {AgreementEntityName}. Exiting.");
                return;
            }

            try
            {
                var service = serviceFactory.CreateOrganizationService(context.UserId);

                // Create delete log record
                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id, // Agreement GUID
                    ["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue),
                    ["gif_name"] = $"Agreement Deleted - {target.Id}"
                };

                service.Create(deleteLog);
                trace.Trace($"Delete log created for Agreement ID: {target.Id}");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record.", ex);
            }

            trace.Trace("AgreementDeleteLogPlugin execution completed successfully.");
        }
    }
}

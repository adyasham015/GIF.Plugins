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

            // Only run on Delete
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"MessageName: {context.MessageName}. Exiting plugin.");
                return;
            }

            // Ensure Target exists and is an EntityReference
            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
            {
                trace.Trace("Target parameter is missing or not an EntityReference. Exiting.");
                return;
            }

            if (!string.Equals(target.LogicalName, AgreementEntityName, StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Target entity: {target.LogicalName}. Exiting plugin.");
                return;
            }

            try
            {
                var service = serviceFactory.CreateOrganizationService(context.UserId);

                // Create delete log entity
                var deleteLog = new Entity(DeleteLogEntityName);

                // Assign fields carefully
                if (deleteLog.Attributes.Contains("gif_entityid"))
                {
                    // If field is Lookup type
                    deleteLog["gif_entityid"] = new EntityReference(AgreementEntityName, target.Id);
                }
                else
                {
                    // If field is plain GUID
                    deleteLog["gif_entityid"] = target.Id;
                }

                deleteLog["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue);
                deleteLog["gif_name"] = $"Agreement Deleted - {target.Id}";
                deleteLog["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId);

                service.Create(deleteLog);
                trace.Trace($"Delete log created for Agreement ID: {target.Id}");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex.Message}");
                throw new InvalidPluginExecutionException("Error creating Agreement delete log.", ex);
            }

            trace.Trace("AgreementDeleteLogPlugin completed.");
        }
    }
}

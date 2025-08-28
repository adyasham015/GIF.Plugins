using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        private const string AgreementEntityName = "msdyn_agreement";
        private const string DeleteLogEntityName = "gif_deletelog";

        // Exact OptionSet value for "msdyn_agreement"
        private const int AgreementOptionSetValue = 805640009;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("AgreementDeleteLogPlugin started.");

            // Only run on Delete
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
                return;

            // Validate Target
            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
                return;

            if (!string.Equals(target.LogicalName, AgreementEntityName, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                // Create delete log record
                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id.ToString(),                 // GUID as string
                    ["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue), // OptionSet
                    ["gif_name"] = $"Agreement Deleted - {target.Id}",       // Single Line of Text
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId) // Owner
                };

                service.Create(deleteLog);
                tracingService.Trace($"Delete log created for Agreement {target.Id}");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error creating delete log: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record.", ex);
            }

            tracingService.Trace("AgreementDeleteLogPlugin completed successfully.");
        }
    }
}

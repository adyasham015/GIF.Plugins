using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        // Agreement entity and delete log entity names
        private const string AgreementEntityName = "msdyn_agreement";
        private const string DeleteLogEntityName = "gif_deletelog";

        // OptionSet value for msdyn_agreement in gif_entityname
        private const int AgreementOptionSetValue = 805640009;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get required services
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("AgreementDeleteLogPlugin started.");

            // Only proceed if message is Delete
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                tracingService.Trace($"Message {context.MessageName} is not Delete. Exiting plugin.");
                return;
            }

            // Validate target
            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
            {
                tracingService.Trace("Target parameter missing or invalid. Exiting plugin.");
                return;
            }

            // Only run for Agreement entity
            if (!string.Equals(target.LogicalName, AgreementEntityName, StringComparison.OrdinalIgnoreCase))
            {
                tracingService.Trace($"Target entity {target.LogicalName} is not {AgreementEntityName}. Exiting plugin.");
                return;
            }

            try
            {
                // Use context.UserId to ensure proper permissions
                var service = serviceFactory.CreateOrganizationService(context.UserId);

                // Create delete log record
                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id, // plain GUID
                    ["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue),
                    ["gif_name"] = $"Agreement Deleted - {target.Id}",
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
                };

                service.Create(deleteLog);
                tracingService.Trace($"Delete log created for Agreement ID: {target.Id}");
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

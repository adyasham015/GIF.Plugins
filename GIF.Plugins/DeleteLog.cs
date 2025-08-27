using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("AgreementDeleteLogPlugin started.");

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference target))
            {
                trace.Trace("Target is missing or invalid, exiting.");
                return;
            }

            if (!string.Equals(target.LogicalName, "msdyn_agreement", StringComparison.OrdinalIgnoreCase))
            {
                trace.Trace($"Target entity is not msdyn_agreement ({target.LogicalName}), exiting.");
                return;
            }

            try
            {
                var deleteLog = new Entity("gif_deletelog"); // GIF Delete Log entity
                deleteLog["gif_name"] = $"Agreement Deleted - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                deleteLog["gif_entityid"] = target.Id.ToString();
                deleteLog["gif_entityname"] = target.LogicalName;
                deleteLog["gif_deletedon"] = DateTime.UtcNow;
                deleteLog["gif_deletedby"] = new EntityReference("systemuser", context.InitiatingUserId);

                service.Create(deleteLog);
                trace.Trace($"Delete log created for Agreement {target.Id}.");
            }
            catch (Exception ex)
            {
                trace.Trace($"Error creating delete log: {ex}");
                throw new InvalidPluginExecutionException("Error creating delete log", ex);
            }
        }
    }
}

using MediaBrowser.Model.Tasks;
using System.Collections.Generic;

namespace Jellyfin.Plugin.JavaScriptInjector.JellyfinVersionSpecific
{
    public static class StartupServiceHelper
    {
        public static IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo()
            {
                Type = TaskTriggerInfoType.StartupTrigger
            };
        }
    }
}
using System.Collections.Generic;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.Tests.Fakes
{
    internal class FakeDialogService : IDialogService
    {
        public List<string> Warnings { get; } = new List<string>();

        public List<string> Errors { get; } = new List<string>();

        public List<SystemInfo> ShownData { get; } = new List<SystemInfo>();

        public void ShowWarning(string message) => Warnings.Add(message);

        public void ShowError(string message) => Errors.Add(message);

        public void ShowData(SystemInfo systemInfo) => ShownData.Add(systemInfo);
    }
}

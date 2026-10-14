using System.Collections.Generic;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.Tests.Fakes
{
    internal class FakeDialogService : IDialogService
    {
        public List<string> Informations { get; } = new List<string>();

        public List<string> Warnings { get; } = new List<string>();

        public List<string> Errors { get; } = new List<string>();

        public void ShowInformation(string message) => Informations.Add(message);

        public void ShowWarning(string message) => Warnings.Add(message);

        public void ShowError(string message) => Errors.Add(message);
    }
}

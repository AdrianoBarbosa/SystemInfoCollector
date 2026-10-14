using System;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.Tests.Fakes
{
    internal class FakeSystemInfoRepository : ISystemInfoRepository
    {
        public string FilePath { get; set; } = @"C:\Dados\SystemInfoCollector\system-info.json";

        public bool FileExists { get; set; }

        public Exception SaveException { get; set; }

        public SystemInfo SavedSystemInfo { get; private set; }

        public int SaveCount { get; private set; }

        public SystemInfo LoadResult { get; set; } = new SystemInfo();

        public Exception LoadException { get; set; }

        public bool Exists() => FileExists;

        public void Save(SystemInfo systemInfo)
        {
            SaveCount++;
            if (SaveException != null)
            {
                throw SaveException;
            }

            SavedSystemInfo = systemInfo;
            FileExists = true;
        }

        public SystemInfo Load()
        {
            if (LoadException != null)
            {
                throw LoadException;
            }

            return LoadResult;
        }
    }
}

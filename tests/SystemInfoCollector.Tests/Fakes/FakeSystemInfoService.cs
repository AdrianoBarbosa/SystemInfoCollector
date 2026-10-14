using System;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.Tests.Fakes
{
    internal class FakeSystemInfoService : ISystemInfoService
    {
        public SystemInfo Result { get; set; } = new SystemInfo();

        public Exception ExceptionToThrow { get; set; }

        public SystemInfo Collect()
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return Result;
        }
    }
}

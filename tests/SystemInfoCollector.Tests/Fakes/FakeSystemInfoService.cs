using System;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.Tests.Fakes
{
    internal class FakeSystemInfoService : ISystemInfoService
    {
        public SystemInfo Result { get; set; } = new SystemInfo();

        public Exception ExceptionToThrow { get; set; }

        public Action OnCollect { get; set; }

        public SystemInfo Collect()
        {
            OnCollect?.Invoke();

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return Result;
        }
    }
}

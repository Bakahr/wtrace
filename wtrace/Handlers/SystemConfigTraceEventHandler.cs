﻿using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.IO;
using System.Text;

namespace LowLevelDesign.WinTrace.Handlers
{
    class SystemConfigTraceEventHandler : ITraceEventHandler
    {
        private readonly TextWriter output;
        private readonly StringBuilder buffer = new StringBuilder();

        public SystemConfigTraceEventHandler(TextWriter output)
        {
            this.output = output;
        }

        public void SubscribeToEvents(KernelTraceEventParser kernel)
        {
            kernel.SystemConfigCPU += Kernel_SystemConfigCPU;
            kernel.SystemConfigNIC += HandleConfigNIC;
            kernel.SystemConfigLogDisk += Kernel_SystemConfigLogDisk;
        }

        public void PrintStatistics()
        {
            output.WriteLine("======= System Configuration =======");
            output.WriteLine(buffer);
        }

        private void Kernel_SystemConfigCPU(SystemConfigCPUTraceData data)
        {
            buffer.AppendLine($"Host: {data.ComputerName} ({data.DomainName})");
            buffer.AppendLine($"CPU: {data.MHz}MHz {data.NumberOfProcessors}cores {data.MemSize}MB");
        }

        private void HandleConfigNIC(SystemConfigNICTraceData data)
        {
            buffer.AppendLine($"NIC: {data.NICDescription} {data.IpAddresses}");
        }

        private void Kernel_SystemConfigLogDisk(SystemConfigLogDiskTraceData data)
        {
            long size = (data.BytesPerSector*data.SectorsPerCluster*data.TotalNumberOfClusters) >> 30;
            buffer.AppendLine($"LOGICAL DISK: {data.DiskNumber} {data.DriveLetterString} {data.FileSystem} " +
                $"{size}GB");
        }
    }
}

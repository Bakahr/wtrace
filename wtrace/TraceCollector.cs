﻿using LowLevelDesign.WinTrace.Handlers;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.IO;
using System.Threading;

namespace LowLevelDesign.WinTrace
{
    class TraceCollector : IDisposable
    {
        private readonly TraceEventSession session;

        private bool disposing = false;
        private bool disposed = false;
        private ITraceEventHandler[] handlers;

        public TraceCollector(int pid, TextWriter output)
        {
            session = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
            session.EnableKernelProvider(
                 KernelTraceEventParser.Keywords.FileIOInit  | KernelTraceEventParser.Keywords.FileIO
                | KernelTraceEventParser.Keywords.NetworkTCPIP
                | KernelTraceEventParser.Keywords.Registry
            );

            handlers = new ITraceEventHandler[] {
                new SystemConfigTraceEventHandler(output),
                new FileIOTraceEventHandler(pid, output),
                new NetworkTraceEventHandler(pid, output)
            };
            foreach (var handler in handlers) {
                handler.SubscribeToEvents(session.Source.Kernel);
            }
        }

        public void Start()
        {
            session.Source.Process();
        }

        public void Dispose()
        {
            if (disposed || disposing) {
                return;
            }
            disposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (session.IsActive) {
                session.Stop();

                // This timeout is needed to handle all the DCStop events 
                // (in case we ever are going to do anything about them)
                Thread.Sleep(3000);

                foreach (var handler in handlers) {
                    handler.PrintStatistics();
                }

                if (disposing) {
                    session.Dispose();
                }
                disposed = true;
            }
        }

        ~TraceCollector()
        {
            if (disposed) {
                return;
            }
            Dispose(false);
        }
    }
}

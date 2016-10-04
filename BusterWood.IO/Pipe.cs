﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.InputOutput
{
    class Pipe : IReader, IWriter
    {
        static readonly IOException PipeClosed = new IOException("Pipe has been closed");

        readonly object readerLock = new object(); // only one reader at a time
        readonly object writerLock = new object(); // only one writer at a time
        readonly object gate = new object();  // protect other fields
        Slice<byte> data;
        Exception readError;  // error to give to writes if the reader is closed 
        Exception writeError; // error to give to reads if the writer is closed 

        public IOResult Read(Slice<byte> buf)
        {
            lock (readerLock)
                lock (gate)
                {
                    for (;;)
                    {
                        if (readError != null)
                            return new IOResult(0, PipeClosed);
                        if (data.Length > 0)
                            break;
                        if (writeError != null)
                            return new IOResult(0, writeError);
                        Monitor.Wait(readerLock);
                    }
                    int bytesCopied = data.CopyTo(buf);
                    data = data.SubSlice(bytesCopied);
                    if (data.Length == 0)
                    {
                        data = new Slice<byte>();
                        lock(writerLock)
                            Monitor.Pulse(writerLock);
                    }
                    return new IOResult(bytesCopied, null);
                }
        }

        public Task<IOResult> ReadAsync(Slice<byte> buf)
        {
            throw new NotImplementedException();
        }

        public IOResult Write(Slice<byte> buf)
        {
            Exception err = null;
            lock (writerLock)
                lock (gate)
                {
                    if (writeError != null)
                        return new IOResult(0, PipeClosed);
                    data = buf;
                    lock (readerLock)
                        Monitor.Pulse(readerLock);
                    for(;;)
                    {
                        if (data.Length == 0)
                            break;
                        if (readError != null)
                        {
                            err = readError;
                            break;
                        }
                        if (writeError != null)
                            err = PipeClosed;
                        Monitor.Wait(writerLock);
                    }
                    var n = buf.Length - data.Length;
                    data = new Slice<byte>();
                    return new IOResult(n, err);
                }
        }

        public Task<IOResult> WriteAsync(Slice<byte> buf)
        {
            throw new NotImplementedException();
        }
    }
}

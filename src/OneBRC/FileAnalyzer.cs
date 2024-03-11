﻿using System;
using System.IO;
using System.Text;

namespace OneBRC;

public static class FileAnalyzer
{
    private const byte NewLine = (byte)'\n';
    
    public static long[] FindOffsets(string filePath, int threadCount)
    {
        var info = new FileInfo(filePath);

        var roughSize = info.Length / threadCount;
        var offsets = new long[threadCount];
        
        using var handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        Span<byte> buffer = stackalloc byte[128];

        long currentOffset = 0;

        for (int i = 0; i < threadCount; i++)
        {
            offsets[i] = currentOffset;

            if (i + 1 == threadCount) break;
            
            currentOffset += roughSize;
            
            int read = RandomAccess.Read(handle, buffer, currentOffset);

            var bytes = buffer[..read];

            int byteNewlineIndex = bytes.IndexOf(NewLine);

            if (byteNewlineIndex >= 0)
            {
                currentOffset += byteNewlineIndex + 1;
            }
#if(DEBUG)
            read = RandomAccess.Read(handle, buffer, currentOffset);
            bytes = buffer[..read];
            var line = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(line);
#endif
        }

        return offsets;
    }
}
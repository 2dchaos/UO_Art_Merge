/* Adapted from UO-SDK
* 
* Copyright (C) 2013 Ian Karlinsey
* 
* 
* UOArtMerge is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* UOArtMerge is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with UltimaLive.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima
{
    public sealed class FileIndex
    {
        public Entry3D[] Index { get; private set; }
        public Stream Stream { get; private set; }
        public long IdxLength { get; private set; }
        private string MulPath;

        public Stream Seek(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return null;
            }

            Entry3D e = Index[index];

            if (e.lookup < 0)
            {
                length = extra = 0;
                return null;
            }

            length = e.length & 0x7FFFFFFF;
            extra = e.extra;


            if (e.length < 0)
            {
                length = extra = 0;
                return null;
            }

            if ((Stream == null) || (!Stream.CanRead) || (!Stream.CanSeek))
            {
                if (MulPath == null)
                    Stream = null;
                else
                    Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream == null)
            {
                length = extra = 0;
                return null;
            }
            else if (Stream.Length < e.lookup)
            {
                length = extra = 0;
                return null;
            }


            Stream.Seek(e.lookup, SeekOrigin.Begin);
            return Stream;
        }

        public bool Valid(int index, out int length, out int extra, out bool patched)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            Entry3D e = Index[index];

            if (e.lookup < 0)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            length = e.length & 0x7FFFFFFF;
            extra = e.extra;

            if ((e.length & (1 << 31)) != 0)
            {
                patched = true;
                return true;
            }

            if (e.length < 0)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            if ((MulPath == null) || !File.Exists(MulPath))
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            if ((Stream == null) || (!Stream.CanRead) || (!Stream.CanSeek))
            {
                Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream.Length < e.lookup)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            patched = false;

            return true;
        }

        public FileIndex(string idxFileIncludingPath, string mulFileIncludingPath, int file)
        {
            if ((idxFileIncludingPath != null) && (mulFileIncludingPath != null))
            {
                MulPath = mulFileIncludingPath;

                using (FileStream index = new FileStream(idxFileIncludingPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    int count = (int)(index.Length / 12);
                    IdxLength = index.Length;
                    Index = new Entry3D[count];
                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    byte[] buffer = new byte[index.Length];
                    index.Read(buffer, 0, (int)index.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)index.Length);
                    gc.Free();
                }
            }
            else
            {
                Stream = null;
                Index = new Entry3D[1];
                return;
            }
        }
    }

    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct Entry3D
    {
        public int lookup;
        public int length;
        public int extra;
    }
}
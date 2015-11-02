/* Copyright 2010, 2015 Jesse McGrew
 * 
 * This file is part of ZILF.
 * 
 * ZILF is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * ZILF is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ZILF.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.IO;
using System.Text;
using Zilf.Language;

namespace Zilf.Interpreter.Values
{
    [BuiltinType(StdAtom.CHANNEL, PrimType.VECTOR)]
    abstract class ZilChannel : ZilObject
    {
        [ChtypeMethod]
        public static ZilChannel FromVector(Context ctx, ZilVector vector)
        {
            throw new InterpreterError("CHTYPE to CHANNEL not supported");
        }

        public override ZilAtom GetTypeAtom(Context ctx)
        {
            return ctx.GetStdAtom(StdAtom.CHANNEL);
        }

        public override PrimType PrimType
        {
            get { return PrimType.VECTOR; }
        }

        public abstract void Reset(Context ctx);
        public abstract void Close(Context ctx);
        public abstract long? GetFileLength();
        public abstract char? ReadChar();
        public abstract bool WriteChar(char c);
        public abstract int WriteNewline();
        public abstract int WriteString(string s);
    }

    sealed class ZilFileChannel : ZilChannel
    {
        private readonly FileAccess fileAccess;
        private readonly string path;
        private Stream stream;

        public ZilFileChannel(string path, FileAccess fileAccess)
        {
            this.path = path;
            this.fileAccess = fileAccess;
        }

        public override string ToString()
        {
            return string.Format(
                "#CHANNEL [{0} {1}]",
                fileAccess == FileAccess.Read ? "READ" : "NONE",
                ZilString.Quote(path));
        }

        public override ZilObject GetPrimitive(Context ctx)
        {
            return new ZilVector(new ZilObject[]
            {
                ctx.GetStdAtom(fileAccess == FileAccess.Read ? StdAtom.READ : StdAtom.NONE),
                new ZilString(path)
            });
        }

        public override void Reset(Context ctx)
        {
            if (stream == null)
                stream = ctx.OpenChannelStream(path, fileAccess);
        }

        public override void Close(Context ctx)
        {
            if (stream != null)
            {
                try
                {
                    stream.Close();
                }
                finally
                {
                    stream = null;
                }
            }
        }

        public override long? GetFileLength()
        {
            if (stream == null)
            {
                return null;
            }

            try
            {
                return stream.Length;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        public override char? ReadChar()
        {
            if (stream == null)
                return null;

            var result = stream.ReadByte();

            return result == -1 ? (char?)null : (char)result;
        }

        public override bool WriteChar(char c)
        {
            return false;
        }

        public override int WriteNewline()
        {
            return 0;
        }

        public override int WriteString(string s)
        {
            return 0;
        }
    }

    sealed class ZilStringChannel : ZilChannel
    {
        private readonly StringBuilder sb = new StringBuilder();

        public ZilStringChannel(FileAccess fileAccess)
        {
            if (fileAccess != FileAccess.Write)
                throw new ArgumentException("Only Write mode is supported", "fileAccess");
        }

        public string String
        {
            get { return sb.ToString(); }
        }

        public override string ToString()
        {
            return string.Format(
                "#CHANNEL [PRINT STRING {0}]",
                ZilString.Quote(sb.ToString()));
        }

        public override ZilObject GetPrimitive(Context ctx)
        {
            return new ZilVector(new ZilObject[]
            {
                ctx.GetStdAtom(StdAtom.PRINT),
                ctx.GetStdAtom(StdAtom.STRING),
                new ZilString(sb.ToString()),
            });
        }

        public override void Reset(Context ctx)
        {
            // nada
        }

        public override void Close(Context ctx)
        {
            // nada
        }

        public override long? GetFileLength()
        {
            return null;
        }

        public override char? ReadChar()
        {
            return null;
        }

        public override bool WriteChar(char c)
        {
            sb.Append(c);
            return true;
        }

        public override int WriteNewline()
        {
            int oldLen = sb.Length;
            sb.AppendLine();
            return sb.Length - oldLen;
        }

        public override int WriteString(string s)
        {
            sb.Append(s);
            return s.Length;
        }
    }

    sealed class ZilConsoleChannel : ZilChannel
    {
        public ZilConsoleChannel(FileAccess fileAccess)
        {
            if (fileAccess != FileAccess.Write)
                throw new ArgumentException("Only Write mode is supported", "fileAccess");
        }

        public override string ToString()
        {
            return string.Format("#CHANNEL [PRINT CONSOLE]");
        }

        public override ZilObject GetPrimitive(Context ctx)
        {
            return new ZilVector(new ZilObject[]
            {
                ctx.GetStdAtom(StdAtom.PRINT),
                ctx.GetStdAtom(StdAtom.CONSOLE),
            });
        }

        public override void Reset(Context ctx)
        {
            // nada
        }

        public override void Close(Context ctx)
        {
            // nada
        }

        public override long? GetFileLength()
        {
            return null;
        }

        public override char? ReadChar()
        {
            return null;
        }

        public override bool WriteChar(char c)
        {
            Console.Write(c);
            return true;
        }

        public override int WriteNewline()
        {
            Console.WriteLine();
            return Environment.NewLine.Length;
        }

        public override int WriteString(string s)
        {
            Console.Write(s);
            return s.Length;
        }
    }
}

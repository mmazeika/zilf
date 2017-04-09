using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zilf.Emit.Zap
{
    public sealed class GameBuilder : IGameBuilder
    {
        const string INDENT = "\t";

        internal static readonly NumericOperand ZERO = new NumericOperand(0);
        internal static readonly NumericOperand ONE = new NumericOperand(1);
        internal static readonly ConstantLiteralOperand VOCAB = new ConstantLiteralOperand("VOCAB");

        // TODO: share unicode translation table with Zapf
        static readonly Dictionary<char, byte> DefaultUnicodeMapping = MakeDefaultUnicodeMapping();

        #region Default Unicode Mapping

        static Dictionary<char, byte> MakeDefaultUnicodeMapping()
        {
            return new Dictionary<char, byte>(69)
            {
                { '�', 155 },
                { '�', 156 },
                { '�', 157 },
                { '�', 158 },
                { '�', 159 },
                { '�', 160 },
                { '�', 161 },
                { '�', 162 },
                { '�', 163 },
                { '�', 164 },
                { '�', 165 },
                { '�', 166 },
                { '�', 167 },
                { '�', 168 },
                { '�', 169 },
                { '�', 170 },
                { '�', 171 },
                { '�', 172 },
                { '�', 173 },
                { '�', 174 },
                { '�', 175 },
                { '�', 176 },
                { '�', 177 },
                { '�', 178 },
                { '�', 179 },
                { '�', 180 },
                { '�', 181 },
                { '�', 182 },
                { '�', 183 },
                { '�', 184 },
                { '�', 185 },
                { '�', 186 },
                { '�', 187 },
                { '�', 188 },
                { '�', 189 },
                { '�', 190 },
                { '�', 191 },
                { '�', 192 },
                { '�', 193 },
                { '�', 194 },
                { '�', 195 },
                { '�', 196 },
                { '�', 197 },
                { '�', 198 },
                { '�', 199 },
                { '�', 200 },
                { '�', 201 },
                { '�', 202 },
                { '�', 203 },
                { '�', 204 },
                { '�', 205 },
                { '�', 206 },
                { '�', 207 },
                { '�', 208 },
                { '�', 209 },
                { '�', 210 },
                { '�', 211 },
                { '�', 212 },
                { '�', 213 },
                { '�', 214 },
                { '�', 215 },
                { '�', 216 },
                { '�', 217 },
                { '�', 218 },
                { '�', 219 },
                { '�', 220 },
                { '�', 221 },
                { '�', 222 },
                { '�', 223 }
            };
        }

        #endregion

        // all global names go in here
        readonly Dictionary<string, string> symbols = new Dictionary<string, string>(250);

        readonly List<RoutineBuilder> routines = new List<RoutineBuilder>(100);
        readonly List<ObjectBuilder> objects = new List<ObjectBuilder>(100);
        readonly Dictionary<string, PropertyBuilder> props = new Dictionary<string, PropertyBuilder>(32);
        readonly Dictionary<string, FlagBuilder> flags = new Dictionary<string, FlagBuilder>(32);
        readonly Dictionary<string, IOperand> constants = new Dictionary<string, IOperand>(100);
        readonly List<GlobalBuilder> globals = new List<GlobalBuilder>(100);
        readonly List<TableBuilder> impureTables = new List<TableBuilder>(10);
        readonly List<TableBuilder> pureTables = new List<TableBuilder>(10);
        readonly List<WordBuilder> vocabulary = new List<WordBuilder>(100);
        readonly HashSet<char> siBreaks = new HashSet<char>();
        readonly Dictionary<string, IOperand> stringPool = new Dictionary<string, IOperand>(100);
        readonly Dictionary<int, NumericOperand> numberPool = new Dictionary<int, NumericOperand>(50);

        readonly IZapStreamFactory streamFactory;
        internal readonly int zversion;
        internal readonly DebugFileBuilder debug;
        readonly GameOptions options;

        IRoutineBuilder entryRoutine;

        Stream stream;
        TextWriter writer;

        public GameBuilder(int zversion, string outFile, bool wantDebugInfo, GameOptions options = null)
            : this(zversion, new ZapStreamFactory(outFile), wantDebugInfo, options)
        {
        }

        public GameBuilder(int zversion, IZapStreamFactory streamFactory, bool wantDebugInfo, GameOptions options = null)
        {
            if (!IsSupportedZversion(zversion))
                throw new ArgumentOutOfRangeException(nameof(zversion), "Unsupported Z-machine version");
            this.zversion = zversion;
            this.streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            GetOptionsTypeForZVersion(zversion, out var requiredOptionsType, out var concreteOptionsType);

            if (options != null)
            {
                const string SOptionsNotCompatible = "Options not compatible with this Z-machine version";

                if (requiredOptionsType.IsAssignableFrom(options.GetType()))
                {
                    this.options = options;
                }
                else
                {
                    throw new ArgumentException(SOptionsNotCompatible, nameof(options));
                }
            }
            else
            {
                this.options = (GameOptions)Activator.CreateInstance(concreteOptionsType);
            }

            debug = wantDebugInfo ? new DebugFileBuilder() : null;

            stream = streamFactory.CreateMainStream();
            writer = new StreamWriter(stream);

            Begin();
        }

        public void Dispose()
        {
            if (writer != null)
            {
                var w = writer;
                writer = null;
                w.Dispose();
            }

            if (stream != null)
            {
                var s = stream;
                stream = null;
                s.Dispose();
            }
        }

        static void GetOptionsTypeForZVersion(int zversion, out Type requiredOptionsType, out Type concreteOptionsType)
        {
            switch (zversion)
            {
                case 3:
                    requiredOptionsType = typeof(GameOptions.V3);
                    concreteOptionsType = typeof(GameOptions.V3);
                    break;

                case 4:
                    requiredOptionsType = typeof(GameOptions.V4Plus);
                    concreteOptionsType = typeof(GameOptions.V4);
                    break;

                case 5:
                case 7:
                case 8:
                    requiredOptionsType = typeof(GameOptions.V5Plus);
                    concreteOptionsType = typeof(GameOptions.V5);
                    break;

                case 6:
                    requiredOptionsType = typeof(GameOptions.V6);
                    concreteOptionsType = typeof(GameOptions.V6);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(zversion));
            }
        }

        static bool IsSupportedZversion(int zversion)
        {
            return zversion >= 1 && zversion <= 8;
        }

        void Begin()
        {
            if (zversion == 3)
            {
                var v3options = (GameOptions.V3)options;
                if (v3options.TimeStatusLine)
                {
                    writer.WriteLine(INDENT + ".TIME");
                }
                if (v3options.SoundEffects)
                {
                    writer.WriteLine(INDENT + ".SOUND");
                }
            }
            else if (zversion > 3)
            {
                writer.WriteLine(INDENT + ".NEW {0}", zversion);

                if (zversion == 4)
                {
                    var v4options = (GameOptions.V4)options;
                    if (v4options.SoundEffects)
                    {
                        writer.WriteLine(INDENT + ".SOUND");
                    }
                }
                else
                {
                    // character set
                    var v5options = (GameOptions.V5Plus)options;
                    if (v5options.LanguageEscapeChar != null)
                    {
                        writer.WriteLine(INDENT + ".LANG {0},{1}", v5options.LanguageId, (ushort)v5options.LanguageEscapeChar);
                    }
                    if (v5options.Charset0 != null || v5options.Charset1 != null || v5options.Charset2 != null)
                    {
                        writer.WriteLine(INDENT + ".CHRSET 0," + ExpandChrSet(v5options.Charset0));
                        writer.WriteLine(INDENT + ".CHRSET 1," + ExpandChrSet(v5options.Charset1));
                        writer.WriteLine(INDENT + ".CHRSET 2," + ExpandChrSet(v5options.Charset2));
                    }

                    // build the header
                    writer.WriteLine();
                    writer.WriteLine(INDENT + ".BYTE {0}", zversion);
                    writer.WriteLine(INDENT + ".BYTE FLAGS");
                    writer.WriteLine(INDENT + ".WORD RELEASEID");
                    writer.WriteLine(INDENT + ".WORD ENDLOD");
                    writer.WriteLine(INDENT + ".WORD START");
                    writer.WriteLine(INDENT + ".WORD VOCAB");
                    writer.WriteLine(INDENT + ".WORD OBJECT");
                    writer.WriteLine(INDENT + ".WORD GLOBAL");
                    writer.WriteLine(INDENT + ".WORD IMPURE");
                    writer.WriteLine(INDENT + ".WORD FLAGS2");
                    writer.WriteLine(INDENT + ".BYTE 0,0,0,0,0,0");     // serial
                    writer.WriteLine(INDENT + ".WORD WORDS");
                    writer.WriteLine(INDENT + ".WORD 0,0");             // length, checksum

                    writer.WriteLine(INDENT + ".WORD 0");       // $1E interpreter number/version
                    writer.WriteLine(INDENT + ".WORD 0");       // $20 screen height/width (characters)
                    writer.WriteLine(INDENT + ".WORD 0");       // $22 screen width (units)
                    writer.WriteLine(INDENT + ".WORD 0");       // $24 screen height (units)
                    writer.WriteLine(INDENT + ".WORD 0");       // $26 font width/height (units) (height/width in V6)
                    writer.WriteLine(INDENT + ".WORD {0}", zversion == 6 ? "FOFF" : "0");     // $28 routines offset (V6)
                    writer.WriteLine(INDENT + ".WORD {0}", zversion == 6 ? "SOFF" : "0");     // $2A strings offset (V6)
                    writer.WriteLine(INDENT + ".WORD 0");       // $2C default background/foreground color
                    writer.WriteLine(INDENT + ".WORD TCHARS");  // $2E terminating characters table
                    writer.WriteLine(INDENT + ".WORD 0");       // $30 output stream 3 width accumulator (V6)
                    writer.WriteLine(INDENT + ".WORD 0");       // $32 Z-Machine Standard revision number
                    writer.WriteLine(INDENT + ".WORD CHRSET");  // $34 alphabet table
                    writer.WriteLine(INDENT + ".WORD EXTAB");   // $36 header extension table
                    writer.WriteLine(INDENT + ".WORD 0");       // $38 unused
                    writer.WriteLine(INDENT + ".WORD 0");       // $3A unused
                    writer.WriteLine(INDENT + ".WORD 0");       // $3C unused (Inform version number part 1)
                    writer.WriteLine(INDENT + ".WORD 0");       // $3E unused (Inform version number part 2)
                }
            }

            writer.WriteLine(INDENT + ".INSERT \"{0}\"", streamFactory.GetFrequentWordsFileName(false));
            writer.WriteLine(INDENT + ".INSERT \"{0}\"", streamFactory.GetDataFileName(false));
        }

        static string ExpandChrSet(string alphabet)
        {
            var sb = new StringBuilder(100);
            if (alphabet == null)
                alphabet = "";

            for (int i = 26; i > alphabet.Length; i--)
            {
                sb.Append("32,");
            }

            foreach (char c in alphabet)
            {
                if (DefaultUnicodeMapping.TryGetValue(c, out var b) == false)
                    b = (byte)c;

                sb.Append(b);
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public IDebugFileBuilder DebugFile
        {
            get { return debug; }
        }

        public IGameOptions Options
        {
            get { return options; }
        }

        public IOperand DefineConstant(string name, IOperand value)
        {
            name = SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            constants.Add(name, value);
            symbols.Add(name, "constant");
            if (value is INumericOperand num)
                return new NumericConstantOperand(name, num.Value);
            else
                return new ConstantLiteralOperand(name);
        }

        public IGlobalBuilder DefineGlobal(string name)
        {
            name = SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            var gb = new GlobalBuilder(name);
            globals.Add(gb);
            symbols.Add(name, "global");
            return gb;
        }

        public ITableBuilder DefineTable(string name, bool pure)
        {
            if (name == null)
                name = "T?" + Convert.ToString(pureTables.Count + impureTables.Count);
            else
                name = SanitizeSymbol(name);

            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            var tb = new TableBuilder(name);
            if (pure)
                pureTables.Add(tb);
            else
                impureTables.Add(tb);
            symbols.Add(name, "table");
            return tb;
        }

        public IRoutineBuilder DefineRoutine(string name, bool entryPoint, bool cleanStack)
        {
            name = SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            if (entryPoint && entryRoutine != null)
                throw new ArgumentException("Entry routine already defined");

            var result = new RoutineBuilder(this, name, entryPoint, cleanStack);
            routines.Add(result);
            symbols.Add(name, "routine");

            if (entryPoint)
                entryRoutine = result;

            return result;
        }

        public IObjectBuilder DefineObject(string name)
        {
            name = SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            var result = new ObjectBuilder(this, 1 + objects.Count, name);
            objects.Add(result);
            symbols.Add(name, "object");
            return result;
        }

        public IPropertyBuilder DefineProperty(string name)
        {
            name = "P?" + SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            int max = (zversion >= 4) ? 63 : 31;
            int num = max - props.Count;

            var result = new PropertyBuilder(name, num);
            props.Add(name, result);
            symbols.Add(name, "property");
            return result;
        }

        public IFlagBuilder DefineFlag(string name)
        {
            name = SanitizeSymbol(name);
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(name));

            int max = (zversion >= 4) ? 47 : 31;
            int num = max - flags.Count;

            var result = new FlagBuilder(name, num);
            flags.Add(name, result);
            symbols.Add(name, "flag");
            return result;
        }

        public IWordBuilder DefineVocabularyWord(string word)
        {
            string name = "W?" + SanitizeSymbol(word.ToUpperInvariant());
            if (symbols.ContainsKey(name))
                throw new ArgumentException("Global symbol already defined: " + name, nameof(word));

            var result = new WordBuilder(name, word.ToLowerInvariant());
            vocabulary.Add(result);
            symbols.Add(name, "word");
            return result;
        }

        public void RemoveVocabularyWord(string word)
        {
            string name = "W?" + SanitizeSymbol(word.ToUpperInvariant());
            if (symbols.ContainsKey(name))
            {
                symbols.Remove(name);
                vocabulary.RemoveAll(wb => wb.Name == name);
            }
        }

        public ICollection<char> SelfInsertingBreaks
        {
            get { return siBreaks; }
        }

        public static string SanitizeString(string text)
        {
            // escape '"' as '""'
            var sb = new StringBuilder(text);

            for (int i = sb.Length - 1; i >= 0; i--)
                if (sb[i] == '"')
                    sb.Insert(i, '"');

            return sb.ToString();
        }

        public static string SanitizeSymbol(string symbol)
        {
            switch (symbol)
            {
                case ".":
                    return "$PERIOD";
                case ",":
                    return "$COMMA";
                case "\"":
                    return "$QUOTE";
                case "'":
                    return "$APOSTROPHE";
            }

            var sb = new StringBuilder(symbol);

            for (int i = 0; i < sb.Length; i++)
            {
                char c = sb[i];
                if (char.IsLetterOrDigit(c) ||
                    c == '?' || c == '#' || c == '-')
                    continue;

                sb[i] = '$';
                sb.Insert(i + 1, ((ushort)c).ToString("x4"));
                i += 4;
            }

            return sb.ToString();
        }

        public INumericOperand MakeOperand(int value)
        {
            switch (value)
            {
                case 0:
                    return ZERO;

                case 1:
                    return ONE;

                default:
                    if (numberPool.TryGetValue(value, out var result) == false)
                    {
                        result = new NumericOperand(value);
                        numberPool.Add(value, result);
                    }
                    return result;
            }
        }

        public IOperand MakeOperand(string value)
        {
            if (stringPool.TryGetValue(value, out var result) == false)
            {
                result = new ConstantLiteralOperand("STR?" + stringPool.Count);
                stringPool.Add(value, result);
            }
            return result;
        }

        public int MaxPropertyLength
        {
            get { return zversion > 3 ? 64 : 8; }
        }

        public int MaxProperties
        {
            get { return zversion > 3 ? 63 : 31; }
        }

        public int MaxFlags
        {
            get { return zversion > 3 ? 48 : 32; }
        }

        public int MaxCallArguments
        {
            get { return zversion > 3 ? 7 : 3; }
        }

        public INumericOperand Zero
        {
            get { return ZERO; }
        }

        public INumericOperand One
        {
            get { return ONE; }
        }

        public IConstantOperand VocabularyTable
        {
            get { return VOCAB; }
        }

        public void Finish()
        {
            // finish main file
            writer.WriteLine();
            writer.WriteLine(INDENT + ".INSERT \"{0}\"", streamFactory.GetStringFileName(false));
            writer.WriteLine(INDENT + ".END");
            writer.Close();

            // write data file
            stream = streamFactory.CreateDataStream();
            writer = new StreamWriter(stream);

            writer.WriteLine(INDENT + "; Data to accompany {0}", streamFactory.GetMainFileName(true));

            // assembly constants
            FinishSymbols();

            // impure data
            FinishGlobals();
            FinishObjects();
            FinishImpureTables();

            // pure data
            writer.WriteLine();
            writer.WriteLine("IMPURE::");   // sic

            FinishSyntax();
            FinishPureTables();

            // end of resident memory
            writer.WriteLine();
            writer.WriteLine("ENDLOD::");

            // debug records
            if (debug != null)
            {
                foreach (var pair in from p in debug.Files
                    orderby p.Value
                    select p)
                    writer.WriteLine(INDENT + ".DEBUG-FILE {0},\"{1}\",\"{2}\"",
                        pair.Value,
                        Path.GetFileNameWithoutExtension(pair.Key),
                        pair.Key);
                foreach (string name in from f in flags.Keys orderby f select f)
                    writer.WriteLine(INDENT + ".DEBUG-ATTR {0},\"{0}\"", name);
                foreach (string name in from p in props.Keys orderby p select p)
                    writer.WriteLine(INDENT + ".DEBUG-PROP {0},\"{0}\"", name);
                foreach (string name in from g in globals orderby g.Name select g.Name)
                    writer.WriteLine(INDENT + ".DEBUG-GLOBAL {0},\"{0}\"", name);
                foreach (string name in from t in impureTables.Concat(pureTables)
                    orderby t.Name
                    select t.Name)
                    writer.WriteLine(INDENT + ".DEBUG-ARRAY {0},\"{0}\"", name);
                foreach (string line in debug.StoredLines)
                    writer.WriteLine(INDENT + line);
            }

            // end of data file
            writer.WriteLine(INDENT + ".ENDI");
            writer.Close();

            // write strings file
            stream = streamFactory.CreateStringStream();
            writer = new StreamWriter(stream);

            writer.WriteLine(INDENT + "; Strings to accompany {0}", streamFactory.GetMainFileName(true));

            FinishStrings();

            writer.WriteLine(INDENT + ".ENDI");
            writer.Close();

            // write frequent words file if necessary
            if (!streamFactory.FrequentWordsFileExists)
            {
                stream = streamFactory.CreateFrequentWordsStream();
                writer = new StreamWriter(stream);

                writer.WriteLine(INDENT + "; Dummy frequent words file for {0}", streamFactory.GetMainFileName(true));
                writer.WriteLine(INDENT + ".FSTR FSTR?DUMMY,\"\"");
                writer.WriteLine("WORDS::");
                for (int i = 0; i < 96; i++)
                    writer.WriteLine(INDENT + "FSTR?DUMMY");

                writer.WriteLine(INDENT + ".ENDI");
                writer.Close();
            }

            // done
            writer = null;
            stream = null;
        }

        void FinishSymbols()
        {
            writer.WriteLine();

            if (zversion >= 5)
                writer.WriteLine(INDENT + "FLAGS=0");

            ushort flags2 = 0;
            bool defineExtab = true, defineTchars = true, defineChrset = false, defineStart = false;

            if (zversion >= 5)
            {
                var v5options = (GameOptions.V5Plus)options;
                if (v5options.DisplayOps)
                {
                    flags2 |= 8;
                }
                if (v5options.Undo)
                {
                    flags2 |= 16;
                }
                if (v5options.Mouse)
                {
                    flags2 |= 32;
                }
                if (v5options.Color)
                {
                    flags2 |= 64;
                }
                if (v5options.SoundEffects)
                {
                    flags2 |= 128;
                }
                defineExtab = v5options.HeaderExtensionTable == null;
                defineTchars = !symbols.ContainsKey("TCHARS");
                defineChrset = v5options.Charset0 == null && v5options.Charset1 == null && v5options.Charset2 == null;

                if (zversion == 6)
                {
                    var v6options = (GameOptions.V6)options;
                    if (v6options.Menus)
                    {
                        flags2 |= 256;
                    }
                    defineStart = true;
                }
            }

            writer.WriteLine(INDENT + "FLAGS2={0}", flags2);

            if (defineExtab)
                writer.WriteLine(INDENT + "EXTAB=0");
            if (defineTchars)
                writer.WriteLine(INDENT + "TCHARS=0");
            if (defineChrset)
                writer.WriteLine(INDENT + "CHRSET=0");
            if (defineStart)
                writer.WriteLine(INDENT + "START=" + entryRoutine);

            // flags
            if (flags.Count > 0)
                writer.WriteLine();
            foreach (var pair in from fp in flags
                                 orderby fp.Value.Number
                                 select fp)
            {
                writer.WriteLine(INDENT + "{0}={1}", pair.Key, pair.Value.Number);
                writer.WriteLine(INDENT + "FX?{0}={1}",
                    pair.Key,
                    1 << (15 - (pair.Value.Number % 16)));
            }

            // properties
            if (props.Count > 0)
                writer.WriteLine();
            foreach (var pair in from pp in props
                                 orderby pp.Value.Number
                                 select pp)
                writer.WriteLine(INDENT + "{0}={1}", pair.Key, pair.Value.Number);

            // constants
            if (constants.Count > 0)
                writer.WriteLine();
            foreach (var pair in from cp in constants
                                 orderby cp.Key
                                 select cp)
                writer.WriteLine(INDENT + "{0}={1}", pair.Key, pair.Value);
        }

        void FinishObjects()
        {
            writer.WriteLine();
            writer.WriteLine("OBJECT:: .TABLE");

            // property defaults
            var propNums = Enumerable.Range(1, (zversion >= 4) ? 63 : 31);
            var propDefaults = from num in propNums
                               join p in this.props on num equals p.Value.Number into propGroup
                               from prop in propGroup.DefaultIfEmpty()
                               let name = prop.Key
                               let def = prop.Value?.DefaultValue
                               select new { num, name, def };

            foreach (var row in propDefaults)
            {
                if (row.name != null)
                    writer.WriteLine(INDENT + "; {0}", row.name);
                else
                    writer.WriteLine(INDENT + "; Unused property #{0}", row.num);

                writer.WriteLine(INDENT + ".WORD {0}", (object)row.def ?? "0");
            }

            // object structures
            if (objects.Count > 0)
                writer.WriteLine();

            foreach (ObjectBuilder ob in objects)
                writer.WriteLine(INDENT + ".OBJECT {0},{1},{2}{3},{4},{5},{6},{7}",
                    ob.SymbolicName,
                    ob.Flags1,
                    ob.Flags2,
                    (zversion < 4) ? "" : "," + ob.Flags3,
                    (object)ob.Parent ?? "0",
                    (object)ob.Sibling ?? "0",
                    (object)ob.Child ?? "0",
                    "?PTBL?" + ob.SymbolicName);

            writer.WriteLine(INDENT + ".ENDT");

            // property tables
            foreach (ObjectBuilder ob in objects)
            {
                writer.WriteLine();
                writer.WriteLine("?PTBL?{0}:: .TABLE", ob.SymbolicName);
                ob.WriteProperties(writer);
                writer.WriteLine(INDENT + ".ENDT");
            }
        }

        void MoveGlobal(string name, int index)
        {
            var curIndex = globals.FindIndex(g => g.Name == name);
            if (curIndex >= 0 && curIndex != index)
            {
                GlobalBuilder gb = globals[curIndex];
                globals.RemoveAt(curIndex);
                globals.Insert(index, gb);
            }
        }

        void FinishGlobals()
        {
            writer.WriteLine();
            writer.WriteLine("GLOBAL:: .TABLE");

            // V3 needs HERE, SCORE, and MOVES to be first
            if (zversion < 4)
            {
                MoveGlobal("HERE", 0);
                MoveGlobal("SCORE", 1);
                MoveGlobal("MOVES", 2);
            }

            // global variables
            foreach (GlobalBuilder gb in globals)
                writer.WriteLine(INDENT + ".GVAR {0}={1}", gb.Name, (object)gb.DefaultValue ?? "0");

            writer.WriteLine(INDENT + ".ENDT");
        }

        void FinishImpureTables()
        {
            // impure user tables
            foreach (TableBuilder tb in impureTables)
            {
                writer.WriteLine();
                writer.WriteLine("{0}:: .TABLE {1}", tb.Name, tb.Size);
                tb.WriteTo(writer);
                writer.WriteLine(INDENT + ".ENDT");
            }
        }

        void FinishSyntax()
        {
            // vocabulary table
            writer.WriteLine();
            writer.WriteLine("VOCAB:: .TABLE");

            if (siBreaks.Count > 255)
                throw new InvalidOperationException("Too many self-inserting breaks");

            writer.WriteLine(INDENT + ".BYTE {0}", siBreaks.Count);
            foreach (char c in siBreaks)
            {
                if ((byte)c != c)
                    throw new InvalidOperationException(string.Format(
                        "Self-inserting break character out of range (${0:x4})", (ushort)c));

                writer.WriteLine(INDENT + ".BYTE {0}", (byte)c);
            }

            if (vocabulary.Count == 0)
            {
                writer.WriteLine(INDENT + ".BYTE 7");
                writer.WriteLine(INDENT + ".WORD 0");
            }
            else
            {
                int zwordBytes = (zversion < 4) ? 4 : 6;
                int dataBytes = vocabulary[0].Size;

                writer.WriteLine(INDENT + ".BYTE {0}", zwordBytes + dataBytes);
                writer.WriteLine(INDENT + ".WORD {0}", vocabulary.Count);

                writer.WriteLine(INDENT + ".VOCBEG {0},{1}", zwordBytes + dataBytes, zwordBytes);
                vocabulary.Sort((a, b) => string.Compare(a.Word, b.Word, StringComparison.Ordinal));
                foreach (WordBuilder wb in vocabulary)
                {
                    writer.WriteLine("{0}:: .ZWORD \"{1}\"", wb.Name, GameBuilder.SanitizeString(wb.Word));
                    wb.WriteTo(writer);
                }
                writer.WriteLine(INDENT + ".VOCEND");
            }

            writer.WriteLine(INDENT + ".ENDT");
        }

        void FinishPureTables()
        {
            if (zversion >= 5)
            {
                var v5options = (GameOptions.V5Plus)options;
                if (v5options.Charset0 != null || v5options.Charset1 != null || v5options.Charset2 != null)
                {
                    writer.WriteLine();
                    writer.WriteLine("CHRSET:: .TABLE 78");
                    writer.WriteLine(INDENT + ".BYTE {0}", ExpandChrSet(v5options.Charset0));
                    writer.WriteLine(INDENT + ".BYTE {0}", ExpandChrSet(v5options.Charset1));
                    writer.WriteLine(INDENT + ".BYTE {0}", ExpandChrSet(v5options.Charset2));
                    writer.WriteLine(INDENT + ".ENDT");
                }
            }

            // pure user tables
            foreach (TableBuilder tb in pureTables)
            {
                writer.WriteLine();
                writer.WriteLine("{0}:: .TABLE {1}", tb.Name, tb.Size);
                tb.WriteTo(writer);
                writer.WriteLine(INDENT + ".ENDT");
            }
        }

        void FinishStrings()
        {
            // strings
            if (stringPool.Count > 0)
                writer.WriteLine();

            var query = from p in stringPool
                        orderby p.Key
                        select p;

            foreach (var pair in query)
                writer.WriteLine(INDENT + ".GSTR {0},\"{1}\"", pair.Value, SanitizeString(pair.Key));
        }

        internal void WriteOutput(string str)
        {
            writer.WriteLine(str);
        }

        internal void WriteOutput(MemoryStream mstr)
        {
            writer.Flush();
            stream.Write(mstr.GetBuffer(), 0, (int)mstr.Length);
        }
    }
}
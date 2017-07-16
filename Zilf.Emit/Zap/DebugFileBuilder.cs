using System.Collections.Generic;

namespace Zilf.Emit.Zap
{
    class DebugFileBuilder : IDebugFileBuilder
    {
        readonly Dictionary<string, int> files = new Dictionary<string, int>();
        readonly List<string> storedLines = new List<string>();

        public int GetFileNumber(string filename)
        {
            if (filename == null)
                return 0;

            if (files.TryGetValue(filename, out int result) == false)
            {
                result = files.Count + 1;
                files[filename] = result;
            }
            return result;
        }

        public IEnumerable<string> StoredLines => storedLines;
        public IDictionary<string, int> Files => files;

        public void MarkAction(IOperand action, string name)
        {
            storedLines.Add($".DEBUG-ACTION {action},\"{name}\"");
        }

        public void MarkObject(IObjectBuilder obj, DebugLineRef start, DebugLineRef end)
        {
            storedLines.Add(string.Format(
                ".DEBUG-OBJECT {0},\"{0}\",{1},{2},{3},{4},{5},{6}",
                obj,
                GetFileNumber(start.File),
                start.Line,
                start.Column,
                GetFileNumber(end.File),
                end.Line,
                end.Column));
        }

        public void MarkRoutine(IRoutineBuilder routine, DebugLineRef start, DebugLineRef end)
        {
            ((RoutineBuilder)routine).defnStart = start;
            ((RoutineBuilder)routine).defnEnd = end;
        }

        public void MarkSequencePoint(IRoutineBuilder routine, DebugLineRef point)
        {
            ((RoutineBuilder)routine).MarkSequencePoint(point);
        }
    }
}
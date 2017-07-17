/* Copyright 2010-2017 Jesse McGrew
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Zilf.Common;

namespace Zilf.Emit.Zap
{
    class RoutineBuilder : ConstantOperandBase, IRoutineBuilder
    {
        internal static readonly Label RTRUE = new Label("TRUE");
        internal static readonly Label RFALSE = new Label("FALSE");
        internal static readonly VariableOperand STACK = new VariableOperand("STACK");
        const string INDENT = "\t";

        readonly GameBuilder game;
        readonly string name;
        readonly bool entryPoint, cleanStack;

        internal DebugLineRef defnStart, defnEnd;

        readonly PeepholeBuffer<ZapCode> peep;
        int nextLabelNum;
        string pendingDebugText;

        readonly List<LocalBuilder> requiredParams = new List<LocalBuilder>();
        readonly List<LocalBuilder> optionalParams = new List<LocalBuilder>();
        readonly List<LocalBuilder> locals = new List<LocalBuilder>();

        public RoutineBuilder(GameBuilder game, string name, bool entryPoint, bool cleanStack)
        {
            this.game = game;
            this.name = name;
            this.entryPoint = entryPoint;
            this.cleanStack = cleanStack;

            peep = new PeepholeBuffer<ZapCode>() { Combiner = new PeepholeCombiner(this) };
            RoutineStart = DefineLabel();
        }

        public override string ToString()
        {
            return name;
        }

        public bool CleanStack => cleanStack;
        public ILabel RTrue => RTRUE;
        public ILabel RFalse => RFALSE;
        public IVariable Stack => STACK;

        bool LocalExists(string localName)
        {
            return requiredParams.Concat(optionalParams).Concat(locals).Any(lb => lb.Name == localName);
        }

        /// <exception cref="InvalidOperationException">This is an entry point routine.</exception>
        /// <exception cref="ArgumentException">A local variable named <paramref name="paramName"/> is already defined.</exception>
        public ILocalBuilder DefineRequiredParameter(string paramName)
        {
            paramName = GameBuilder.SanitizeSymbol(paramName);

            if (entryPoint)
                throw new InvalidOperationException("Entry point may not have parameters");
            if (LocalExists(paramName))
                throw new ArgumentException("Local variable already exists: " + paramName, nameof(paramName));

            var local = new LocalBuilder(paramName);
            requiredParams.Add(local);
            return local;
        }

        /// <exception cref="InvalidOperationException">This is an entry point routine.</exception>
        /// <exception cref="ArgumentException">A local variable named <paramref name="paramName"/> is already defined.</exception>
        public ILocalBuilder DefineOptionalParameter(string paramName)
        {
            paramName = GameBuilder.SanitizeSymbol(paramName);

            if (entryPoint)
                throw new InvalidOperationException("Entry point may not have parameters");
            if (LocalExists(paramName))
                throw new ArgumentException("Local variable already exists: " + paramName, nameof(paramName));

            var local = new LocalBuilder(paramName);
            optionalParams.Add(local);
            return local;
        }

        /// <exception cref="InvalidOperationException">This is an entry point routine.</exception>
        /// <exception cref="ArgumentException">A local variable named <paramref name="localName"/> is already defined.</exception>
        public ILocalBuilder DefineLocal(string localName)
        {
            localName = GameBuilder.SanitizeSymbol(localName);

            if (entryPoint)
                throw new InvalidOperationException("Entry point may not have local variables");
            if (LocalExists(localName))
                throw new ArgumentException("Local variable already exists: " + localName, nameof(localName));

            var local = new LocalBuilder(localName);
            locals.Add(local);
            return local;
        }

        public ILabel RoutineStart { get; }

        public ILabel DefineLabel()
        {
            return new Label("?L" + nextLabelNum++);
        }

        public void MarkLabel(ILabel label)
        {
            peep.MarkLabel(label);
        }

        void AddLine(string code, ILabel target, PeepholeLineType type)
        {
            ZapCode zc;
            zc.Text = code;
            zc.DebugText = pendingDebugText;
            pendingDebugText = null;

            peep.AddLine(zc, target, type);
        }

        public void MarkSequencePoint(DebugLineRef lineRef)
        {
            if (game.debug != null)
                pendingDebugText =
                    $".DEBUG-LINE {game.debug.GetFileNumber(lineRef.File)},{lineRef.Line},{lineRef.Column}";
        }

        public void Branch(ILabel label)
        {
            AddLine("JUMP", label, PeepholeLineType.BranchAlways);
        }

        public bool HasArgCount => game.zversion >= 5;

        /// <exception cref="ArgumentException">This condition requires a variable, but <paramref name="left"/> is not a variable.</exception>
        /// <exception cref="ArgumentException">The wrong number of operands were provided.</exception>
        public void Branch(Condition cond, IOperand left, IOperand right, ILabel label, bool polarity)
        {
            string opcode;
            bool leftVar = false, nullary = false, unary = false;

            switch (cond)
            {
                case Condition.DecCheck:
                    opcode = "DLESS?";
                    leftVar = true;
                    break;
                case Condition.Greater:
                    opcode = "GRTR?";
                    break;
                case Condition.IncCheck:
                    opcode = "IGRTR?";
                    leftVar = true;
                    break;
                case Condition.Inside:
                    opcode = "IN?";
                    break;
                case Condition.Less:
                    opcode = "LESS?";
                    break;
                case Condition.TestAttr:
                    opcode = "FSET?";
                    break;
                case Condition.TestBits:
                    opcode = "BTST";
                    break;
                case Condition.PictureData:
                    opcode = "PICINF";
                    break;
                case Condition.MakeMenu:
                    opcode = "MENU";
                    break;

                case Condition.ArgProvided:
                    opcode = "ASSIGNED?";
                    leftVar = true;
                    unary = true;
                    break;

                case Condition.Verify:
                    opcode = "VERIFY";
                    nullary = true;
                    break;
                case Condition.Original:
                    opcode = "ORIGINAL?";
                    nullary = true;
                    break;

                default:
                    throw UnhandledCaseException.FromEnum(cond, "conditional operation");
            }

            if (leftVar && !(left is IVariable))
                throw new ArgumentException("This condition requires a variable", nameof(left));

            if (nullary)
            {
                if (left != null || right != null)
                    throw new ArgumentException("Expected no operands for nullary condition");
            }
            else if (unary)
            {
                if (right != null)
                    throw new ArgumentException("Expected only one operand for unary condition", nameof(right));
            }
            else
            {
                if (right == null)
                    throw new ArgumentException("Expected two operands for binary condition", nameof(right));
            }

            Contract.Assert(leftVar || !unary);
            AddLine(
                nullary
                    ? opcode
                    : unary
                        ? $"{opcode} '{left}"        // see assert above
                        : $"{opcode} {(leftVar ? "'" : "")}{left},{right}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void BranchIfZero(IOperand operand, ILabel label, bool polarity)
        {
            AddLine(
                "ZERO? " + operand,
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void BranchIfEqual(IOperand value, IOperand option1, ILabel label, bool polarity)
        {
            AddLine(
                $"EQUAL? {value},{option1}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void BranchIfEqual(IOperand value, IOperand option1, IOperand option2, ILabel label, bool polarity)
        {
            AddLine(
                $"EQUAL? {value},{option1},{option2}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void BranchIfEqual(IOperand value, IOperand option1, IOperand option2, IOperand option3, ILabel label, bool polarity)
        {
            AddLine(
                $"EQUAL? {value},{option1},{option2},{option3}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void Return(IOperand result)
        {
            if (result == GameBuilder.ONE)
                AddLine("RTRUE", RTRUE, PeepholeLineType.BranchAlways);
            else if (result == GameBuilder.ZERO)
                AddLine("RFALSE", RFALSE, PeepholeLineType.BranchAlways);
            else if (result == STACK)
                AddLine("RSTACK", null, PeepholeLineType.Terminator);
            else
                AddLine("RETURN " + result, null, PeepholeLineType.Terminator);
        }

        public bool HasUndo => game.zversion >= 5;

        public void EmitNullary(NullaryOp op, IVariable result)
        {
            string opcode;

            switch (op)
            {
                case NullaryOp.RestoreUndo:
                    opcode = "IRESTORE";
                    break;
                case NullaryOp.SaveUndo:
                    opcode = "ISAVE";
                    break;
                case NullaryOp.ShowStatus:
                    opcode = "USL";
                    break;
                case NullaryOp.Catch:
                    opcode = "CATCH";
                    break;
                default:
                    throw UnhandledCaseException.FromEnum(op, "nullary operation");
            }

            AddLine(
                $"{opcode}{OptResult(result)}",
                null,
                PeepholeLineType.Plain);
        }

        [NotNull]
        static string OptResult([CanBeNull] IVariable result)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (result == null)
                return string.Empty;

            return " >" + result;
        }

        public void EmitUnary(UnaryOp op, IOperand value, IVariable result)
        {
            if (op == UnaryOp.Neg)
            {
                AddLine(
                    $"SUB 0,{value}{OptResult(result)}",
                    null,
                    PeepholeLineType.Plain);
                return;
            }

            string opcode;
            bool pred = false;

            switch (op)
            {
                case UnaryOp.Not:
                    opcode = "BCOM";
                    break;
                case UnaryOp.GetParent:
                    opcode = "LOC";
                    break;
                case UnaryOp.GetPropSize:
                    opcode = "PTSIZE";
                    break;
                case UnaryOp.LoadIndirect:
                    opcode = "VALUE";
                    break;
                case UnaryOp.Random:
                    opcode = "RANDOM";
                    break;
                case UnaryOp.GetChild:
                    opcode = "FIRST?";
                    pred = true;
                    break;
                case UnaryOp.GetSibling:
                    opcode = "NEXT?";
                    pred = true;
                    break;
                case UnaryOp.RemoveObject:
                    opcode = "REMOVE";
                    break;
                case UnaryOp.DirectInput:
                    opcode = "DIRIN";
                    break;
                case UnaryOp.DirectOutput:
                    opcode = "DIROUT";
                    break;
                case UnaryOp.OutputBuffer:
                    opcode = "BUFOUT";
                    break;
                case UnaryOp.OutputStyle:
                    opcode = "HLIGHT";
                    break;
                case UnaryOp.SplitWindow:
                    opcode = "SPLIT";
                    break;
                case UnaryOp.SelectWindow:
                    opcode = "SCREEN";
                    break;
                case UnaryOp.ClearWindow:
                    opcode = "CLEAR";
                    break;
                case UnaryOp.GetCursor:
                    opcode = "CURGET";
                    break;
                case UnaryOp.EraseLine:
                    opcode = "ERASE";
                    break;
                case UnaryOp.SetFont:
                    opcode = "FONT";
                    break;
                case UnaryOp.CheckUnicode:
                    opcode = "CHECKU";
                    break;
                case UnaryOp.FlushStack:
                    opcode = "FSTACK";
                    break;
                case UnaryOp.PopUserStack:
                    opcode = "POP";
                    break;
                case UnaryOp.PictureTable:
                    opcode = "PICSET";
                    break;
                case UnaryOp.MouseWindow:
                    opcode = "MOUSE-LIMIT";
                    break;
                case UnaryOp.ReadMouse:
                    opcode = "MOUSE-INFO";
                    break;
                case UnaryOp.PrintForm:
                    opcode = "PRINTF";
                    break;
                default:
                    throw UnhandledCaseException.FromEnum(op, "unary operation");
            }

            if (pred)
            {
                var label = DefineLabel();

                AddLine(
                    $"{opcode} {value}{OptResult(result)}",
                    label,
                    PeepholeLineType.BranchPositive);

                peep.MarkLabel(label);
            }
            else
            {
                AddLine(
                    $"{opcode} {value}{OptResult(result)}",
                    null,
                    PeepholeLineType.Plain);
            }
        }

        public void EmitBinary(BinaryOp op, IOperand left, IOperand right, IVariable result)
        {
            // optimize special cases
            if (op == BinaryOp.Add &&
                (left == game.One && right == result || right == game.One && left == result))
            {
                AddLine("INC '" + result, null, PeepholeLineType.Plain);
                return;
            }

            if (op == BinaryOp.Sub && left == result && right == game.One)
            {
                AddLine("DEC '" + result, null, PeepholeLineType.Plain);
                return;
            }

            if (op == BinaryOp.StoreIndirect && right == Stack && game.zversion != 6)
            {
                AddLine("POP " + left, null, PeepholeLineType.Plain);
                return;
            }

            string opcode;

            switch (op)
            {
                case BinaryOp.Add:
                    opcode = "ADD";
                    break;
                case BinaryOp.And:
                    opcode = "BAND";
                    break;
                case BinaryOp.ArtShift:
                    opcode = "ASHIFT";
                    break;
                case BinaryOp.Div:
                    opcode = "DIV";
                    break;
                case BinaryOp.GetByte:
                    opcode = "GETB";
                    break;
                case BinaryOp.GetPropAddress:
                    opcode = "GETPT";
                    break;
                case BinaryOp.GetProperty:
                    opcode = "GETP";
                    break;
                case BinaryOp.GetNextProp:
                    opcode = "NEXTP";
                    break;
                case BinaryOp.GetWord:
                    opcode = "GET";
                    break;
                case BinaryOp.LogShift:
                    opcode = "SHIFT";
                    break;
                case BinaryOp.Mod:
                    opcode = "MOD";
                    break;
                case BinaryOp.Mul:
                    opcode = "MUL";
                    break;
                case BinaryOp.Or:
                    opcode = "BOR";
                    break;
                case BinaryOp.Sub:
                    opcode = "SUB";
                    break;
                case BinaryOp.MoveObject:
                    opcode = "MOVE";
                    break;
                case BinaryOp.SetFlag:
                    opcode = "FSET";
                    break;
                case BinaryOp.ClearFlag:
                    opcode = "FCLEAR";
                    break;
                case BinaryOp.DirectOutput:
                    opcode = "DIROUT";
                    break;
                case BinaryOp.SetCursor:
                    opcode = "CURSET";
                    break;
                case BinaryOp.SetColor:
                    opcode = "COLOR";
                    break;
                case BinaryOp.Throw:
                    opcode = "THROW";
                    break;
                case BinaryOp.StoreIndirect:
                    opcode = "SET";
                    break;
                case BinaryOp.FlushUserStack:
                    opcode = "FSTACK";
                    break;
                case BinaryOp.GetWindowProperty:
                    opcode = "WINGET";
                    break;
                case BinaryOp.ScrollWindow:
                    opcode = "SCROLL";
                    break;
                default:
                    throw UnhandledCaseException.FromEnum(op, "binary operation");
            }

            AddLine(
                $"{opcode} {left},{right}{OptResult(result)}",
                null,
                PeepholeLineType.Plain);
        }

        public void EmitTernary(TernaryOp op, IOperand left, IOperand center, IOperand right, IVariable result)
        {
            string opcode;

            switch (op)
            {
                case TernaryOp.PutByte:
                    opcode = "PUTB";
                    break;
                case TernaryOp.PutProperty:
                    opcode = "PUTP";
                    break;
                case TernaryOp.PutWord:
                    opcode = "PUT";
                    break;
                case TernaryOp.CopyTable:
                    opcode = "COPYT";
                    break;
                case TernaryOp.PutWindowProperty:
                    opcode = "WINPUT";
                    break;
                case TernaryOp.DrawPicture:
                    opcode = "DISPLAY";
                    break;
                case TernaryOp.WindowStyle:
                    opcode = "WINATTR";
                    break;
                case TernaryOp.MoveWindow:
                    opcode = "WINPOS";
                    break;
                case TernaryOp.WindowSize:
                    opcode = "WINSIZE";
                    break;
                case TernaryOp.SetMargins:
                    opcode = "MARGIN";
                    break;
                case TernaryOp.SetCursor:
                    opcode = "CURSET";
                    break;
                case TernaryOp.DirectOutput:
                    opcode = "DIROUT";
                    break;
                case TernaryOp.ErasePicture:
                    opcode = "DCLEAR";
                    break;
                default:
                    throw UnhandledCaseException.FromEnum(op, "ternary operation");
            }

            AddLine(
                $"{opcode} {left},{center},{right}{OptResult(result)}",
                null,
                PeepholeLineType.Plain);
        }

        public void EmitEncodeText(IOperand src, IOperand length, IOperand srcOffset, IOperand dest)
        {
            AddLine(
                $"ZWSTR {src},{length},{srcOffset},{dest}",
                null,
                PeepholeLineType.Plain);
        }

        public void EmitTokenize(IOperand text, IOperand parse, IOperand dictionary, IOperand flag)
        {
            var sb = new StringBuilder("LEX ");
            sb.Append(text);
            sb.Append(',');
            sb.Append(parse);

            if (dictionary != null)
            {
                sb.Append(',');
                sb.Append(dictionary);

                if (flag != null)
                {
                    sb.Append(',');
                    sb.Append(flag);
                }
            }

            AddLine(sb.ToString(), null, PeepholeLineType.Plain);
        }

        public void EmitRestart()
        {
            AddLine("RESTART", null, PeepholeLineType.Terminator);
        }

        public void EmitQuit()
        {
            AddLine("QUIT", null, PeepholeLineType.Terminator);
        }

        public bool HasBranchSave => game.zversion < 4;

        public void EmitSave(ILabel label, bool polarity)
        {
            AddLine("SAVE", label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void EmitRestore(ILabel label, bool polarity)
        {
            AddLine("RESTORE", label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public bool HasStoreSave => game.zversion >= 4;

        public void EmitSave(IVariable result)
        {
            AddLine("SAVE >" + result, null, PeepholeLineType.Plain);
        }

        public void EmitRestore(IVariable result)
        {
            AddLine("RESTORE >" + result, null, PeepholeLineType.Plain);
        }

        public bool HasExtendedSave => game.zversion >= 5;
        public void EmitSave(IOperand table, IOperand size, IOperand filename,
            IVariable result)
        {
            AddLine(
                $"SAVE {table},{size},{filename} >{result}",
                null,
                PeepholeLineType.Plain);
        }

        public void EmitRestore(IOperand table, IOperand size, IOperand filename,
            IVariable result)
        {
            AddLine(
                $"RESTORE {table},{size},{filename} >{result}",
                null,
                PeepholeLineType.Plain);
        }

        public void EmitScanTable(IOperand value, IOperand table, IOperand length, IOperand form,
            IVariable result, ILabel label, bool polarity)
        {
            var sb = new StringBuilder("INTBL? ");
            sb.Append(value);
            sb.Append(',');
            sb.Append(table);
            sb.Append(',');
            sb.Append(length);
            if (form != null)
            {
                sb.Append(',');
                sb.Append(form);
            }
            sb.Append(" >");
            sb.Append(result);

            AddLine(sb.ToString(), label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void EmitGetChild(IOperand value, IVariable result, ILabel label, bool polarity)
        {
            AddLine(
                $"FIRST? {value} >{result}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void EmitGetSibling(IOperand value, IVariable result, ILabel label, bool polarity)
        {
            AddLine(
                $"NEXT? {value} >{result}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void EmitPrintNewLine()
        {
            AddLine("CRLF", null, PeepholeLineType.Plain);
        }

        public void EmitPrint(string text, bool crlfRtrue)
        {
            var opcode = crlfRtrue ? "PRINTR" : "PRINTI";

            AddLine(
                $"{opcode} \"{GameBuilder.SanitizeString(text)}\"",
                null,
                crlfRtrue ? PeepholeLineType.HeavyTerminator : PeepholeLineType.Plain);
        }

        public void EmitPrint(PrintOp op, IOperand value)
        {
            string opcode;

            switch (op)
            {
                case PrintOp.Address:
                    opcode = "PRINTB";
                    break;
                case PrintOp.Character:
                    opcode = "PRINTC";
                    break;
                case PrintOp.Number:
                    opcode = "PRINTN";
                    break;
                case PrintOp.Object:
                    opcode = "PRINTD";
                    break;
                case PrintOp.PackedAddr:
                    opcode = "PRINT";
                    break;
                case PrintOp.Unicode:
                    opcode = "PRINTU";
                    break;
                default:
                    throw UnhandledCaseException.FromEnum(op, "print operation");
            }

            AddLine($"{opcode} {value}", null, PeepholeLineType.Plain);
        }

        public void EmitPrintTable(IOperand table, IOperand width, IOperand height, IOperand skip)
        {
            var sb = new StringBuilder("PRINTT ");
            sb.Append(table);
            sb.Append(',');
            sb.Append(width);

            if (height != null)
            {
                sb.Append(',');
                sb.Append(height);

                if (skip != null)
                {
                    sb.Append(',');
                    sb.Append(skip);
                }
            }

            AddLine(sb.ToString(), null, PeepholeLineType.Plain);
        }

        public void EmitPlaySound(IOperand number, IOperand effect, IOperand volume, IOperand routine)
        {
            var sb = new StringBuilder("SOUND ");
            sb.Append(number);

            if (effect != null)
            {
                sb.Append(',');
                sb.Append(effect);

                if (volume != null)
                {
                    sb.Append(',');
                    sb.Append(volume);

                    if (routine != null)
                    {
                        sb.Append(',');
                        sb.Append(routine);
                    }
                }
            }

            AddLine(sb.ToString(), null, PeepholeLineType.Plain);
        }

        public void EmitRead(IOperand chrbuf, IOperand lexbuf, IOperand interval, IOperand routine,
            IVariable result)
        {
            var sb = new StringBuilder("READ ");
            sb.Append(chrbuf);

            if (lexbuf != null)
            {
                sb.Append(',');
                sb.Append(lexbuf);

                if (interval != null)
                {
                    sb.Append(',');
                    sb.Append(interval);

                    if (routine != null)
                    {
                        sb.Append(',');
                        sb.Append(routine);
                    }
                }
            }

            if (result != null)
            {
                sb.Append(" >");
                sb.Append(result);
            }

            AddLine(sb.ToString(), null, PeepholeLineType.Plain);
        }

        public void EmitReadChar(IOperand interval, IOperand routine, IVariable result)
        {
            var sb = new StringBuilder("INPUT 1");

            if (interval != null)
            {
                sb.Append(',');
                sb.Append(interval);

                if (routine != null)
                {
                    sb.Append(',');
                    sb.Append(routine);
                }
            }

            sb.Append(" >");
            sb.Append(result);

            AddLine(sb.ToString(), null, PeepholeLineType.Plain);
        }

        /// <exception cref="ArgumentException">Too many arguments were supplied for the Z-machine version.</exception>
        public void EmitCall(IOperand routine, IOperand[] args, IVariable result)
        {
            /* V1-3: CALL (0-3, store)
             * V4: CALL1 (0, store), CALL2 (1, store), XCALL (0-7, store)
             * V5: ICALL1 (0), ICALL2 (1), ICALL (0-3), IXCALL (0-7) */

            if (args.Length > game.MaxCallArguments)
                throw new ArgumentException(
                    $"Too many arguments in routine call: {args.Length} supplied, {game.MaxCallArguments} allowed");

            var sb = new StringBuilder();

            if (game.zversion < 4)
            {
                // V1-3: use only CALL opcode (3 args max), pop result if not needed
                sb.Append("CALL ");
                sb.Append(routine);
                foreach (var arg in args)
                {
                    sb.Append(',');
                    sb.Append(arg);
                }

                if (result != null)
                {
                    sb.Append(" >");
                    sb.Append(result);
                }

                AddLine(sb.ToString(), null, PeepholeLineType.Plain);

                if (result == null && cleanStack)
                    AddLine("FSTACK", null, PeepholeLineType.Plain);
            }
            else if (game.zversion == 4)
            {
                // V4: use CALL/CALL1/CALL2/XCALL opcodes, pop result if not needed
                string opcode;
                switch (args.Length)
                {
                    case 0:
                        opcode = "CALL1";
                        break;
                    case 1:
                        opcode = "CALL2";
                        break;
                    case 2:
                    case 3:
                        opcode = "CALL";
                        break;
                    default:
                        opcode = "XCALL";
                        break;
                }

                sb.Append(opcode);
                sb.Append(' ');
                sb.Append(routine);
                foreach (var arg in args)
                {
                    sb.Append(',');
                    sb.Append(arg);
                }

                if (result != null)
                {
                    sb.Append(" >");
                    sb.Append(result);
                }

                AddLine(sb.ToString(), null, PeepholeLineType.Plain);

                if (result == null && cleanStack)
                    AddLine("FSTACK", null, PeepholeLineType.Plain);
            }
            else
            {
                // V5-V6: use CALL/CALL1/CALL2/XCALL if want result
                // use ICALL/ICALL1/ICALL2/IXCALL if not
                string opcode;
                if (result == null)
                {
                    switch (args.Length)
                    {
                        case 0:
                            opcode = "ICALL1";
                            break;
                        case 1:
                            opcode = "ICALL2";
                            break;
                        case 2:
                        case 3:
                            opcode = "ICALL";
                            break;
                        default:
                            opcode = "IXCALL";
                            break;
                    }
                }
                else
                {
                    switch (args.Length)
                    {
                        case 0:
                            opcode = "CALL1";
                            break;
                        case 1:
                            opcode = "CALL2";
                            break;
                        case 2:
                        case 3:
                            opcode = "CALL";
                            break;
                        default:
                            opcode = "XCALL";
                            break;
                    }
                }

                sb.Append(opcode);
                sb.Append(' ');
                sb.Append(routine);
                foreach (var arg in args)
                {
                    sb.Append(',');
                    sb.Append(arg);
                }

                if (result != null)
                {
                    sb.Append(" >");
                    sb.Append(result);
                }

                AddLine(sb.ToString(), null, PeepholeLineType.Plain);
            }
        }

        public void EmitStore(IVariable dest, IOperand src)
        {
            if (dest != src)
            {
                if (dest == STACK)
                {
                    AddLine($"PUSH {src}", null, PeepholeLineType.Plain);
                }
                else if (src == STACK)
                {
                    AddLine(
                        game.zversion == 6 ? $"POP >{dest}" : $"POP \'{dest}",
                        null,
                        PeepholeLineType.Plain);
                }
                else
                {
                    AddLine($"SET '{dest},{src}", null, PeepholeLineType.Plain);
                }
            }
        }

        public void EmitPopStack()
        {
            if (cleanStack)
            {
                if (game.zversion <= 4)
                {
                    AddLine("FSTACK", null, PeepholeLineType.Plain);
                }
                else if (game.zversion == 6)
                {
                    AddLine("FSTACK 1", null, PeepholeLineType.Plain);
                }
                else
                {
                    AddLine("ICALL2 0,STACK", null, PeepholeLineType.Plain);
                }
            }
        }

        public void EmitPushUserStack(IOperand value, IOperand stack, ILabel label, bool polarity)
        {
            AddLine($"XPUSH {value},{stack}",
                label,
                polarity ? PeepholeLineType.BranchPositive : PeepholeLineType.BranchNegative);
        }

        public void Finish()
        {
            game.WriteOutput(string.Empty);

            var sb = new StringBuilder();

            // write routine header
            if (game.debug != null)
            {
                sb.Append(INDENT);
                sb.Append(".DEBUG-ROUTINE ");
                sb.Append(game.debug.GetFileNumber(defnStart.File));
                sb.Append(',');
                sb.Append(defnStart.Line);
                sb.Append(',');
                sb.Append(defnStart.Column);
                sb.Append(",\"");
                sb.Append(name);
                sb.Append('"');
                foreach (var lb in requiredParams.Concat(optionalParams).Concat(locals))
                {
                    sb.Append(",\"");
                    sb.Append(lb.Name);
                    sb.Append('"');
                }
                sb.AppendLine();
            }

            sb.Append(INDENT);
            sb.Append(".FUNCT ");
            sb.Append(name);

            foreach (var lb in requiredParams)
            {
                sb.Append(',');
                sb.Append(lb.Name);
            }

            foreach (var lb in optionalParams.Concat(locals))
            {
                sb.Append(',');
                sb.Append(lb.Name);

                if (game.zversion < 5 && lb.DefaultValue != null)
                {
                    sb.Append('=');
                    sb.Append(lb.DefaultValue);
                }
            }

            game.WriteOutput(sb.ToString());

            if (entryPoint && game.zversion != 6)
            {
                game.WriteOutput("START::");
            }

            // write preamble
            var preamble = new PeepholeBuffer<ZapCode>();
            preamble.MarkLabel(RoutineStart);

            // write values for optional params and locals for V5+
            if (game.zversion >= 5)
            {
                foreach (var lb in optionalParams)
                {
                    if (lb.DefaultValue != null)
                    {
                        var nextLabel = DefineLabel();

                        preamble.AddLine(
                            new ZapCode { Text = $"ASSIGNED? '{lb}" },
                            nextLabel,
                            PeepholeLineType.BranchPositive);
                        preamble.AddLine(
                            new ZapCode { Text = $"SET '{lb},{lb.DefaultValue}" },
                            null,
                            PeepholeLineType.Plain);
                        preamble.MarkLabel(nextLabel);
                    }
                }

                foreach (var lb in locals)
                {
                    if (lb.DefaultValue != null)
                        preamble.AddLine(
                            new ZapCode { Text = $"SET '{lb},{lb.DefaultValue}" },
                            null,
                            PeepholeLineType.Plain);
                }
            }

            peep.InsertBufferFirst(preamble);

            // write routine body
            peep.Finish((label, code, dest, type) =>
            {
                if (code.DebugText != null)
                    game.WriteOutput(INDENT + code.DebugText);

                if (type == PeepholeLineType.BranchAlways)
                {
                    if (dest == RTRUE)
                    {
                        game.WriteOutput(INDENT + "RTRUE");
                        return;
                    }
                    if (dest == RFALSE)
                    {
                        game.WriteOutput(INDENT + "RFALSE");
                        return;
                    }
                }

                if (code.Text == "CRLF+RTRUE")
                {
                    var labelPrefix = label == null ? "" : label + ":";
                    game.WriteOutput($"{labelPrefix}{INDENT}CRLF");

                    game.WriteOutput(INDENT + "RTRUE");
                    return;
                }

                sb.Length = 0;
                if (label != null)
                {
                    sb.Append(label);
                    sb.Append(':');
                }
                sb.Append(INDENT);
                sb.Append(code.Text);

                switch (type)
                {
                    case PeepholeLineType.BranchAlways:
                        sb.Append(' ');
                        sb.Append(dest);
                        break;
                    case PeepholeLineType.BranchPositive:
                        sb.Append(" /");
                        sb.Append(dest);
                        break;
                    case PeepholeLineType.BranchNegative:
                        sb.Append(" \\");
                        sb.Append(dest);
                        break;
                }

                game.WriteOutput(sb.ToString());
            });

            if (game.debug != null)
                game.WriteOutput(
                    INDENT +
                    $".DEBUG-ROUTINE-END {game.debug.GetFileNumber(defnEnd.File)},{defnEnd.Line},{defnEnd.Column}");
        }

        class PeepholeCombiner : IPeepholeCombiner<ZapCode>
        {
            readonly RoutineBuilder routineBuilder;

            public PeepholeCombiner(RoutineBuilder routineBuilder)
            {
                this.routineBuilder = routineBuilder;
            }

            void BeginMatch([NotNull] IEnumerable<CombinableLine<ZapCode>> lines)
            {
                Contract.Requires(lines != null);
                enumerator = lines.GetEnumerator();
                matches = new List<CombinableLine<ZapCode>>();
            }

            bool Match([ItemNotNull] [NotNull] params Predicate<CombinableLine<ZapCode>>[] criteria)
            {
                Contract.Requires(criteria != null);

                while (matches.Count < criteria.Length)
                {
                    if (enumerator.MoveNext() == false)
                        return false;

                    matches.Add(enumerator.Current);
                }
                
                return criteria.Zip(matches, (c, m) => c(m)).All(ok => ok);
            }

            void EndMatch()
            {
                enumerator.Dispose();
                enumerator = null;

                matches = null;
            }

            IEnumerator<CombinableLine<ZapCode>> enumerator;
            List<CombinableLine<ZapCode>> matches;

            CombinerResult<ZapCode> Combine1To1(string newText, PeepholeLineType? type = null, [CanBeNull] ILabel target = null)
            {
                return new CombinerResult<ZapCode>(
                    1,
                    new[] {
                        new CombinableLine<ZapCode>(
                            matches[0].Label,
                            new ZapCode {
                                Text = newText,
                                DebugText = matches[0].Code.DebugText
                            },
                            target ?? matches[0].Target,
                            type ?? matches[0].Type)
                    });
            }

            CombinerResult<ZapCode> Combine2To1(string newText, PeepholeLineType? type = null, [CanBeNull] ILabel target = null)
            {
                return new CombinerResult<ZapCode>(
                    2,
                    new[] {
                        new CombinableLine<ZapCode>(
                            matches[0].Label,
                            new ZapCode {
                                Text = newText,
                                DebugText = matches[0].Code.DebugText ?? matches[1].Code.DebugText
                            },
                            target ?? matches[1].Target,
                            type ?? matches[1].Type)
                    });
            }

            CombinerResult<ZapCode> Combine2To2(
                string newText1, string newText2,
                PeepholeLineType? type1 = null, PeepholeLineType? type2 = null,
                [CanBeNull] ILabel target1 = null, [CanBeNull] ILabel target2 = null)
            {
                return new CombinerResult<ZapCode>(
                    2,
                    new[] {
                        new CombinableLine<ZapCode>(
                            matches[0].Label,
                            new ZapCode {
                                Text = newText1,
                                DebugText = matches[0].Code.DebugText
                            },
                            target1 ?? matches[0].Target,
                            type1 ?? matches[0].Type),
                        new CombinableLine<ZapCode>(
                            matches[1].Label,
                            new ZapCode {
                                Text = newText2,
                                DebugText = matches[1].Code.DebugText
                            },
                            target2 ?? matches[1].Target,
                            type2 ?? matches[1].Type)
                    });
            }

            static CombinerResult<ZapCode> Consume(int numberOfLines)
            {
                return new CombinerResult<ZapCode>(numberOfLines, Enumerable.Empty<CombinableLine<ZapCode>>());
            }

            static readonly Regex equalZeroRegex =
                new Regex(@"^EQUAL\? (?:(?<var>[^,]+),0|0,(?<var>[^,]+))$");

            static readonly Regex bandConstantToStackRegex =
                new Regex(@"^BAND (?:(?<var>[^,]+),(?<const>-?\d+)|(?<const>-?\d+),(?<var>[^,]+)) >STACK$");

            static readonly Regex bandConstantWithStackRegex =
                new Regex(@"^BAND (?:STACK,(?<const>-?\d+)|(?<const>-?\d+),STACK) >(?<dest>.*)$");

            static readonly Regex borConstantToStackRegex =
                new Regex(@"^BOR (?:(?<var>[^,]+),(?<const>-?\d+)|(?<const>-?\d+),(?<var>[^,]+)) >STACK$");

            static readonly Regex borConstantWithStackRegex =
                new Regex(@"^BOR (?:STACK,(?<const>-?\d+)|(?<const>-?\d+),STACK) >(?<dest>.*)$");

            static readonly Regex popToVariableRegex =
                new Regex(@"^POP ['>]");

            /// <inheritdoc />
            public CombinerResult<ZapCode> Apply(IEnumerable<CombinableLine<ZapCode>> lines)
            {
                Match rm = null, rm2 = null;

                BeginMatch(lines);
                try
                {
                    if (Match(a => (rm = equalZeroRegex.Match(a.Code.Text)).Success))
                    {
                        // EQUAL? x,0 | EQUAL? 0,x => ZERO? x
                        Contract.Assume(rm != null);
                        return Combine1To1("ZERO? " + rm.Groups["var"]);
                    }

                    if (Match(a => a.Code.Text == "JUMP" && (a.Target == RTRUE || a.Target == RFALSE)))
                    {
                        // JUMP to TRUE/FALSE => RTRUE/RFALSE
                        return Combine1To1(matches[0].Target == RTRUE ? "RTRUE" : "RFALSE");
                    }

                    if (Match(a => a.Code.Text.StartsWith("PUSH ", StringComparison.Ordinal), b => b.Code.Text == "RSTACK"))
                    {
                        // PUSH + RSTACK => RFALSE/RTRUE/RETURN
                        switch (matches[0].Code.Text)
                        {
                            case "PUSH 0":
                                return Combine2To1("RFALSE", PeepholeLineType.BranchAlways, RFALSE);
                            case "PUSH 1":
                                return Combine2To1("RTRUE", PeepholeLineType.BranchAlways, RTRUE);
                            default:
                                return Combine2To1("RETURN " + matches[0].Code.Text.Substring(5));
                        }
                    }

                    if (Match(a => a.Code.Text.EndsWith(">STACK", StringComparison.Ordinal), b => popToVariableRegex.IsMatch(b.Code.Text)))
                    {
                        // >STACK + POP 'dest => >dest
                        var a = matches[0].Code.Text;
                        var b = matches[1].Code.Text;
                        return Combine2To1(a.Substring(0, a.Length - 5) + b.Substring(5));
                    }

                    if (Match(a => a.Code.Text.StartsWith("PUSH ", StringComparison.Ordinal), b => popToVariableRegex.IsMatch(b.Code.Text)))
                    {
                        // PUSH + POP 'dest => SET 'dest
                        var a = matches[0].Code.Text;
                        var b = matches[1].Code.Text;
                        return Combine2To1("SET '" + b.Substring(5) + "," + a.Substring(5));
                    }

                    if (Match(a => a.Code.Text.StartsWith("INC '", StringComparison.Ordinal), b => b.Code.Text.StartsWith("GRTR? ", StringComparison.Ordinal)))
                    {
                        string str;
                        if ((str = matches[0].Code.Text.Substring(5)) != "STACK" &&
                            matches[1].Code.Text.StartsWith("GRTR? " + str, StringComparison.Ordinal))
                        {
                            // INC 'v + GRTR? v => IGRTR? 'v
                            return Combine2To1("IGRTR? '" + matches[1].Code.Text.Substring(6));
                        }
                    }

                    if (Match(a => a.Code.Text.StartsWith("DEC '", StringComparison.Ordinal), b => b.Code.Text.StartsWith("LESS? ", StringComparison.Ordinal)))
                    {
                        string str;
                        if ((str = matches[0].Code.Text.Substring(5)) != "STACK" &&
                            matches[1].Code.Text.StartsWith("LESS? " + str, StringComparison.Ordinal))
                        {
                            // DEC 'v + LESS? v => DLESS? 'v
                            return Combine2To1("DLESS? '" + matches[1].Code.Text.Substring(6));
                        }
                    }

                    if (Match(
                        a => (a.Code.Text.StartsWith("EQUAL? ", StringComparison.Ordinal) ||
                              a.Code.Text.StartsWith("ZERO? ", StringComparison.Ordinal)) &&
                             a.Type == PeepholeLineType.BranchPositive,
                        b => (b.Code.Text.StartsWith("EQUAL? ", StringComparison.Ordinal) ||
                              b.Code.Text.StartsWith("ZERO? ", StringComparison.Ordinal)) &&
                             b.Type == PeepholeLineType.BranchPositive))
                    {
                        if (matches[0].Target == matches[1].Target)
                        {
                            string[] GetParts(string text)
                            {
                                return text.StartsWith("ZERO? ", StringComparison.Ordinal)
                                    ? new[] { text.Substring(6), "0" }
                                    : text.Substring(7).Split(',');
                            }

                            var aparts = GetParts(matches[0].Code.Text);
                            var bparts = GetParts(matches[1].Code.Text);

                            if (aparts[0] == bparts[0] && aparts.Length < 4)
                            {
                                var sb = new StringBuilder(matches[0].Code.Text.Length + matches[1].Code.Text.Length);

                                if (aparts.Length + bparts.Length <= 5)
                                {
                                    // EQUAL? v,a,b /L + EQUAL? v,c /L => EQUAL? v,a,b,c /L
                                    sb.Append("EQUAL? ");
                                    sb.Append(aparts[0]);
                                    foreach (var part in aparts.Skip(1).Concat(bparts.Skip(1)))
                                    {
                                        sb.Append(',');
                                        sb.Append(part);
                                    }
                                    return Combine2To1(sb.ToString());
                                }
                                else
                                {
                                    // EQUAL? v,a,b /L + EQUAL? v,c,d /L => EQUAL? v,a,b,c /L + EQUAL? v,d /L
                                    var allRhs = aparts.Skip(1).Concat(bparts.Skip(1)).ToArray();

                                    sb.Append("EQUAL? ");
                                    sb.Append(aparts[0]);
                                    foreach (var rhs in allRhs.Take(3))
                                    {
                                        sb.Append(',');
                                        sb.Append(rhs);
                                    }
                                    var first = sb.ToString();

                                    sb.Length = 0;
                                    sb.Append("EQUAL? ");
                                    sb.Append(aparts[0]);
                                    foreach (var rhs in allRhs.Skip(3))
                                    {
                                        sb.Append(',');
                                        sb.Append(rhs);
                                    }
                                    var second = sb.ToString();

                                    return Combine2To2(first, second);
                                }
                            }
                        }
                    }

                    if (Match(a => a.Code.Text == "CRLF", b => b.Code.Text == "RTRUE"))
                    {
                        // combine CRLF + RTRUE into a single terminator
                        // this can be pulled through a branch and thus allows more PRINTR transformations
                        return Combine2To1("CRLF+RTRUE", PeepholeLineType.Terminator);
                    }

                    if (Match(a => a.Code.Text.StartsWith("PRINTI ", StringComparison.Ordinal), b => b.Code.Text == "CRLF+RTRUE"))
                    {
                        // PRINTI + (CRLF + RTRUE) => PRINTR
                        return Combine2To1("PRINTR " + matches[0].Code.Text.Substring(7), PeepholeLineType.HeavyTerminator);
                    }

                    // BAND v,c >STACK + ZERO? STACK /L =>
                    //     when c == 0:              simple branch
                    //     when c is a power of two: BTST v,c \L
                    if (Match(a => (rm = bandConstantToStackRegex.Match(a.Code.Text)).Success,
                              b => b.Code.Text == "ZERO? STACK"))
                    {
                        var variable = rm.Groups["var"].Value;
                        Contract.Assume(variable != null);
                        var constantValue = int.Parse(rm.Groups["const"].Value);

                        if (constantValue == 0)
                        {
                            if (rm.Groups["var"].Value != "STACK")
                            {
                                return matches[1].Type == PeepholeLineType.BranchPositive
                                    ? Combine2To1("JUMP", PeepholeLineType.BranchAlways, matches[1].Target)
                                    : Consume(2);
                            }
                        }
                        else if ((constantValue & (constantValue - 1)) == 0)
                        {
                            var oppositeType = matches[1].Type == PeepholeLineType.BranchPositive
                                ? PeepholeLineType.BranchNegative
                                : PeepholeLineType.BranchPositive;

                            return Combine2To1("BTST " + variable + "," + constantValue, oppositeType);
                        }
                    }

                    // BAND v,c1 >STACK + BAND STACK,c2 >dest => BAND v,(c1&c2) >dest
                    if (Match(a => (rm = bandConstantToStackRegex.Match(a.Code.Text)).Success,
                              b => (rm2 = bandConstantWithStackRegex.Match(b.Code.Text)).Success))
                    {
                        var variable = rm.Groups["var"].Value;
                        var dest = rm2.Groups["dest"].Value;
                        Contract.Assume(variable != null);
                        Contract.Assume(dest != null);
                        var constant1 = int.Parse(rm.Groups["const"].Value);
                        var constant2 = int.Parse(rm2.Groups["const"].Value);
                        var combined = constant1 & constant2;
                        return Combine2To1("BAND " + variable + "," + combined + " >" + dest);
                    }

                    // BOR v,c1 >STACK + BOR STACK,c2 >dest => BOR v,(c1|c2) >dest
                    if (Match(a => (rm = borConstantToStackRegex.Match(a.Code.Text)).Success,
                              b => (rm2 = borConstantWithStackRegex.Match(b.Code.Text)).Success))
                    {
                        var variable = rm.Groups["var"].Value;
                        var dest = rm2.Groups["dest"].Value;
                        Contract.Assume(variable != null);
                        Contract.Assume(dest != null);
                        var constant1 = int.Parse(rm.Groups["const"].Value);
                        var constant2 = int.Parse(rm2.Groups["const"].Value);
                        var combined = constant1 | constant2;
                        return Combine2To1("BOR " + variable + "," + combined + " >" + dest);
                    }

                    // no matches
                    return new CombinerResult<ZapCode>();
                }
                finally
                {
                    EndMatch();
                }
            }

            public ZapCode SynthesizeBranchAlways()
            {
                return new ZapCode { Text = "JUMP" };
            }

            public bool AreIdentical(ZapCode a, ZapCode b)
            {
                return a.Text == b.Text;
            }

            public ZapCode MergeIdentical(ZapCode a, ZapCode b)
            {
                return new ZapCode
                {
                    Text = a.Text,
                    DebugText = a.DebugText ?? b.DebugText
                };
            }

            public SameTestResult AreSameTest(ZapCode a, ZapCode b)
            {
                // if the stack is involved, all bets are off
                if (a.Text.Contains("STACK") || b.Text.Contains("STACK"))
                    return SameTestResult.Unrelated;

                // if the instructions are identical, they must be the same test
                if (a.Text == b.Text)
                    return SameTestResult.SameTest;

                /* otherwise, they can be related if 'a' is a store+branch instruction
                 * and 'b' is ZERO? testing the result stored by 'a'. the z-machine's
                 * store+branch instructions all branch upon storing a nonzero value,
                 * so we always return OppositeTest in this case. */
                if (b.Text.StartsWith("ZERO? ", StringComparison.Ordinal) && a.Text.EndsWith(">" + b.Text.Substring(6), StringComparison.Ordinal))
                    return SameTestResult.OppositeTest;

                return SameTestResult.Unrelated;
            }

            public ControlsConditionResult ControlsConditionalBranch(ZapCode a, ZapCode b)
            {
                /* if 'a' pushes a constant and 'b' is ZERO? testing the stack, the
                 * answer depends on the value of the constant. */
                if (a.Text.StartsWith("PUSH ", StringComparison.Ordinal) && int.TryParse(a.Text.Substring(5), out int value) &&
    b.Text == "ZERO? STACK")
                {
                    if (value == 0)
                        return ControlsConditionResult.CausesBranchIfPositive;
                    else
                        return ControlsConditionResult.CausesNoOpIfPositive;
                }

                return ControlsConditionResult.Unrelated;
            }

            public ILabel NewLabel()
            {
                return routineBuilder.DefineLabel();
            }
        }
    }
}
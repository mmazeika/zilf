﻿/* Copyright 2010, 2015 Jesse McGrew
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

namespace Zilf.Language
{
    /// <summary>
    /// Contains values for atoms used internally.
    /// </summary>
    enum StdAtom
    {
        None,

        [Atom("+")]
        Plus,
        [Atom("-")]
        Minus,
        [Atom("*")]
        Times,
        [Atom("/")]
        Divide,
        [Atom("=")]
        Eq,

        ACTIONS,
        ACTIVATION,
        ADD,
        ADECL,
        ADJ,
        ADJECTIVE,
        AGAIN,
        AND,
        ANDB,
        ANY,
        APPLICABLE,
        APPLY,
        ATBL,
        ATOM,
        B,
        BACK,
        BAND,
        BCOM,
        BIG,
        BIND,
        BOR,
        BTST,
        BUFOUT,
        BUZZ,
        BYTE,
        BYTELENGTH,
        C,
        CARRIED,
        CEXIT,
        CEXITFLAG,
        CEXITSTR,
        CHANNEL,
        CHARACTER,
        [Atom("CLEAN-STACK?")]
        CLEAN_STACK_P,
        CLEAR,
        COLOR,
        [Atom("COMPACT-SYNTAXES?")]
        COMPACT_SYNTAXES_P,
        [Atom("COMPACT-VOCABULARY?")]
        COMPACT_VOCABULARY_P,
        [Atom("COMPILATION-FLAGS")]
        COMPILATION_FLAGS,
        COND,
        CONSOLE,
        CONSTANT,
        CONSTRUCTOR,
        COPYT,
        CR,
        CRLF,
        [Atom("CRLF-CHARACTER")]
        CRLF_CHARACTER,
        CURGET,
        CURSET,
        D,
        DEC,
        DECL,
        DEFAULT,
        [Atom("DEFAULT-DEFINITION")]
        DEFAULT_DEFINITION,
        DEFINED,
        DEFINITIONS,
        DEFSTRUCT,
        [Atom("DELAY-DEFINITION")]
        DELAY_DEFINITION,
        DESC,
        DEXIT,
        DEXITOBJ,
        DEXITSTR,
        DIR,
        DIRECTIONS,
        DIRIN,
        DIROUT,
        DISPLAY,
        [Atom("DISPLAY-OPS?")]
        DISPLAY_OPS_P,
        DIV,
        [Atom("DLESS?")]
        DLESS_P,
        DO,
        [Atom("DO-FUNNY-GLOBALS?")]
        DO_FUNNY_GLOBALS_P,
        ELSE,
        [Atom("EQUAL?")]
        EQUAL_P,
        EZIP,
        [Atom("F?")]
        F_P,
        FALSE,
        [Atom("FALSE-VALUE")]
        FALSE_VALUE,
        [Atom("FATAL-VALUE")]
        FATAL_VALUE,
        FCLEAR,
        FCN,
        FEXIT,
        FEXITFCN,
        FIND,
        FIRST,
        [Atom("FIRST?")]
        FIRST_P,
        FIX,
        FLAGS,
        FORM,
        FSET,
        [Atom("FSET?")]
        FSET_P,
        FSUBR,
        FUNCTION,
        [Atom("G?")]
        G_P,
        [Atom("G=?")]
        GEq_P,
        GET,
        [Atom("GET-CLASSIFICATION")]
        GET_CLASSIFICATION,
        GETB,
        GETP,
        GETPT,
        GLOBAL,
        [Atom("GLOBAL-VARS-TABLE")]
        GLOBAL_VARS_TABLE,
        GO,
        GVAL,
        HAVE,
        HELD,
        HERE,
        HLIGHT,
        IF,
        IFFLAG,
        [Atom("IGRTR?")]
        IGRTR_P,
        IN,
        [Atom("IN?")]
        IN_P,
        [Atom("IN-ROOM")]
        IN_ROOM,
        [Atom("IN-ZILCH")]
        IN_ZILCH,
        INC,
        INCHAN,
        [Atom("INIT-ARGS")]
        INIT_ARGS,
        INITIAL,
        INPUT,
        [Atom("INTBL?")]
        INTBL_P,
        INTERRUPTS,
        IRESTORE,
        IS,
        ISAVE,
        ITABLE,
        KERNEL,
        [Atom("L?")]
        L_P,
        LAST,
        [Atom("LAST-OBJECT")]
        LAST_OBJECT,
        LENGTH,
        [Atom("L=?")]
        LEq_P,
        LEXV,
        LIST,
        LOC,
        [Atom("LOCAL-GLOBALS")]
        LOCAL_GLOBALS,
        [Atom("LONG-WORD-TABLE")]
        LONG_WORD_TABLE,
        [Atom("LONG-WORDS")]
        LONG_WORDS,
        [Atom("LOW-DIRECTION")]
        LOW_DIRECTION,
        LTABLE,
        LVAL,
        MACRO,
        [Atom("MAKE-VERB-DATA")]
        MAKE_VERB_DATA,
        [Atom("MAKE-VWORD")]
        MAKE_VWORD,
        MANY,
        [Atom("MAP-CONTENTS")]
        MAP_CONTENTS,
        [Atom("MAP-DIRECTIONS")]
        MAP_DIRECTIONS,
        [Atom("MDL-ZIL?")]
        MDL_ZIL_P,
        MENU,
        MOD,
        MOUSE,
        MOVE,
        MOVES,
        MUL,
        N,
        [Atom("NEW-PARSER?")]
        NEW_PARSER_P,
        [Atom("NEW-SFLAGS")]
        NEW_SFLAGS,
        [Atom("NEW-VOC?")]
        NEW_VOC_P,
        NEXIT,
        NEXITSTR,
        [Atom("NEXT?")]
        NEXT_P,
        NEXTP,
        NODECL,
        NONE,
        NOT,
        NOTYPE,
        NOUN,
        NTH,
        OBJECT,
        OBLIST,
        OFFSET,
        [Atom("ON-GROUND")]
        ON_GROUND,
        [Atom("ONE-BYTE-PARTS-OF-SPEECH")]
        ONE_BYTE_PARTS_OF_SPEECH,
        OPEN,
        OR,
        ORB,
        OUTCHAN,
        PACKAGE,
        [Atom("PARSER-TABLE")]
        PARSER_TABLE,
        PARTICLE,
        PATBL,
        PATTERN,
        PER,
        PLTABLE,
        [Atom("PLUS-MODE")]
        PLUS_MODE,
        PREACTIONS,
        PREDGEN,
        PREP,
        PREPOSITIONS,
        [Atom("PRESERVE-SPACES?")]
        PRESERVE_SPACES_P,
        PRINT,
        PRINTB,
        PRINTC,
        PRINTD,
        PRINTI,
        PRINTN,
        PRINTR,
        PRINTTYPE,
        [Atom("PRMANY-CRLF")]
        PRMANY_CRLF,
        PROG,
        PROPDEF,
        PROPSPEC,
        PRSTBL,
        PRTBL,
        PSEUDO,
        PTABLE,
        PTSIZE,
        PURE,
        PUSH,
        PUT,
        PUTB,
        PUTP,
        QUIT,
        QUOTE,
        RANDOM,
        READ,
        REDEFINE,
        RELEASEID,
        REMOVE,
        REPEAT,
        [Atom("REPLACE-DEFINITION")]
        REPLACE_DEFINITION,
        REST,
        RESTART,
        RESTORE,
        RETURN,
        [Atom("REVERSE-DEFINED")]
        REVERSE_DEFINED,
        REXIT,
        RFALSE,
        ROOM,
        ROOMS,
        [Atom("ROOMS-AND-LGS-FIRST")]
        ROOMS_AND_LGS_FIRST,
        [Atom("ROOMS-FIRST")]
        ROOMS_FIRST,
        [Atom("ROOMS-LAST")]
        ROOMS_LAST,
        ROOT,
        ROUTINE,
        RSTACK,
        RTRUE,
        SAVE,
        SCORE,
        SCREEN,
        [Atom("SEARCH-ALL")]
        SEARCH_ALL,
        [Atom("SEARCH-DO-TAKE")]
        SEARCH_DO_TAKE,
        [Atom("SEARCH-MANY")]
        SEARCH_MANY,
        [Atom("SEARCH-MUST-HAVE")]
        SEARCH_MUST_HAVE,
        SEGMENT,
        SEMI,
        SERIAL,
        SET,
        SETG,
        SIBREAKS,
        SORRY,
        SOUND,
        [Atom("SOUND-EFFECTS?")]
        SOUND_EFFECTS_P,
        SPLICE,
        SPLIT,
        [Atom("START-OFFSET")]
        START_OFFSET,
        STRING,
        STRUCTURED,
        SUB,
        SUBR,
        SYNONYM,
        T,
        [Atom("T?")]
        T_P,
        TABLE,
        TADJ,
        TAKE,
        TBUZZ,
        TCHARS,
        TDIR,
        TELL,
        [Atom("TEMP-TABLE")]
        TEMP_TABLE,
        TIME,
        TO,
        TOBJECT,
        TPREP,
        [Atom("TRUE-VALUE")]
        TRUE_VALUE,
        TVERB,
        TZERO,
        UEXIT,
        UNDO,
        [Atom("USE-COLOR?")]
        USE_COLOR_P,
        [Atom("USE-MENUS?")]
        USE_MENUS_P,
        [Atom("USE-MOUSE?")]
        USE_MOUSE_P,
        [Atom("USE-SOUND?")]
        USE_SOUND_P,
        [Atom("USE-UNDO?")]
        USE_UNDO_P,
        USL,
        VALUE,
        VECTOR,
        VERB,
        [Atom("VERB-POINTER")]
        VERB_POINTER,
        [Atom("VERB-STUFF-ID")]
        VERB_STUFF_ID,
        VERBS,
        VERIFY,
        [Atom("VERSION?")]
        VERSION_P,
        VOC,
        VOCAB,
        VTBL,
        VWORD,
        WORD,
        [Atom("WORD-ADJ-ID")]
        WORD_ADJ_ID,
        [Atom("WORD-CLASSIFICATION-NUMBER")]
        WORD_CLASSIFICATION_NUMBER,
        [Atom("WORD-DIR-ID")]
        WORD_DIR_ID,
        [Atom("WORD-FLAG-TABLE")]
        WORD_FLAG_TABLE,
        [Atom("WORD-FLAGS")]
        WORD_FLAGS,
        [Atom("WORD-FLAGS-IN-TABLE")]
        WORD_FLAGS_IN_TABLE,
        [Atom("WORD-FLAGS-LIST")]
        WORD_FLAGS_LIST,
        [Atom("WORD-LEXICAL-WORD")]
        WORD_LEXICAL_WORD,
        [Atom("WORD-SEMANTIC-STUFF")]
        WORD_SEMANTIC_STUFF,
        [Atom("WORD-VERB-STUFF")]
        WORD_VERB_STUFF,
        WORDLENGTH,
        X,
        XORB,
        XZIP,
        YZIP,
        [Atom("ZAP-TO-SOURCE-DIRECTORY?")]
        ZAP_TO_SOURCE_DIRECTORY_P,
        [Atom("ZERO?")]
        ZERO_P,
        ZILCH,
        ZILF,
        [Atom("ZIL-VERSION")]
        ZIL_VERSION,
        ZIP,
        ZORKID,
        ZVAL,
        ZWSTR,
    }
}

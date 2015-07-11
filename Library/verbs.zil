"Verbs"

<DIRECTIONS NORTH SOUTH EAST WEST NORTHEAST NORTHWEST SOUTHEAST SOUTHWEST
            IN OUT UP DOWN>

<SYNONYM NORTH N>
<SYNONYM SOUTH S>
<SYNONYM EAST E>
<SYNONYM WEST W>
<SYNONYM NORTHEAST NE>
<SYNONYM NORTHWEST NW>
<SYNONYM SOUTHEAST SE>
<SYNONYM SOUTHWEST SW>
<SYNONYM IN INTO INSIDE>
<SYNONYM OUT OUTSIDE>
<SYNONYM UP U>
<SYNONYM DOWN D>

<SYNONYM THROUGH THRU>

;"Syntaxes for regular verbs"

<SYNTAX LOOK = V-LOOK>
<VERB-SYNONYM LOOK L>

<SYNTAX WALK OBJECT = V-WALK>
<SYNTAX WALK IN OBJECT (FIND DOORBIT) (IN-ROOM) = V-ENTER>
<SYNTAX WALK THROUGH OBJECT (FIND DOORBIT) (IN-ROOM) = V-ENTER>
<VERB-SYNONYM WALK GO>

<SYNTAX ENTER OBJECT (FIND DOORBIT) (IN-ROOM) = V-ENTER>

<SYNTAX QUIT = V-QUIT>

<SYNTAX TAKE OBJECT (FIND TAKEBIT) (MANY ON-GROUND IN-ROOM) = V-TAKE>
<VERB-SYNONYM TAKE GRAB>
<SYNTAX PICK UP OBJECT (FIND TAKEBIT) (MANY ON-GROUND IN-ROOM) = V-TAKE>
<SYNTAX PICK OBJECT (FIND TAKEBIT) (MANY ON-GROUND IN-ROOM) UP OBJECT (FIND KLUDGEBIT) = V-TAKE>
<SYNTAX GET OBJECT (FIND TAKEBIT) (MANY ON-GROUND IN-ROOM) = V-TAKE>

<SYNTAX DROP OBJECT (MANY HAVE HELD CARRIED) = V-DROP>
<SYNTAX PUT DOWN OBJECT (MANY HAVE HELD CARRIED) = V-DROP>
<SYNTAX PUT OBJECT (MANY HAVE HELD CARRIED) DOWN OBJECT (FIND KLUDGEBIT) = V-DROP>

<SYNTAX EXAMINE OBJECT = V-EXAMINE PRE-REQUIRES-LIGHT>
<SYNTAX LOOK AT OBJECT = V-EXAMINE PRE-REQUIRES-LIGHT>
<VERB-SYNONYM EXAMINE X>

<SYNTAX WEAR OBJECT (FIND WEARBIT) (HAVE TAKE) = V-WEAR>
<VERB-SYNONYM WEAR DON>
<SYNTAX PUT ON OBJECT (FIND WEARBIT) (HAVE TAKE) = V-WEAR>

<SYNTAX UNWEAR OBJECT (FIND WORNBIT) (HAVE HELD CARRIED) = V-UNWEAR>
<VERB-SYNONYM UNWEAR DOFF>
<SYNTAX TAKE OFF OBJECT (FIND WORNBIT) (HAVE HELD CARRIED) = V-UNWEAR>

<SYNTAX PUT OBJECT (MANY TAKE HELD CARRIED ON-GROUND IN-ROOM) ON OBJECT (FIND SURFACEBIT) = V-PUT-ON PRE-PUT-ON>
<SYNTAX PUT UP OBJECT (MANY TAKE HELD CARRIED ON-GROUND IN-ROOM) ON OBJECT (FIND SURFACEBIT) = V-PUT-ON PRE-PUT-ON>
<VERB-SYNONYM PUT HANG PLACE>

<SYNTAX PUT OBJECT (MANY TAKE HELD CARRIED ON-GROUND IN-ROOM) IN OBJECT (FIND CONTBIT) = V-PUT-IN PRE-PUT-IN>
<VERB-SYNONYM PUT PLACE INSERT>

<SYNTAX INVENTORY = V-INVENTORY>
<VERB-SYNONYM INVENTORY I>

<SYNTAX CONTEMPLATE OBJECT = V-THINK-ABOUT>
<VERB-SYNONYM CONTEMPLATE CONSIDER>
<SYNTAX THINK ABOUT OBJECT = V-THINK-ABOUT>

<SYNTAX OPEN OBJECT (FIND OPENABLEBIT) = V-OPEN>

<SYNTAX CLOSE OBJECT (FIND OPENBIT) = V-CLOSE>
<VERB-SYNONYM CLOSE SHUT>

<SYNTAX LOCK OBJECT WITH OBJECT (HAVE HELD CARRIED) = V-LOCK>

<SYNTAX UNLOCK OBJECT (FIND LOCKEDBIT) WITH OBJECT (HAVE HELD CARRIED) = V-UNLOCK>

<SYNTAX TURN ON OBJECT (FIND DEVICEBIT) = V-TURN-ON>
<SYNTAX TURN OBJECT (FIND DEVICEBIT) ON OBJECT (FIND KLUDGEBIT) = V-TURN-ON>

<SYNTAX TURN OFF OBJECT (FIND DEVICEBIT) = V-TURN-OFF>
<SYNTAX TURN OBJECT (FIND DEVICEBIT) OFF OBJECT (FIND KLUDGEBIT) = V-TURN-OFF>

<SYNTAX FLIP OBJECT (FIND DEVICEBIT) = V-FLIP>
<VERB-SYNONYM FLIP SWITCH TOGGLE>

<SYNTAX WAIT = V-WAIT>
<VERB-SYNONYM WAIT Z>

<SYNTAX AGAIN = V-AGAIN>
<VERB-SYNONYM AGAIN G>

<SYNTAX READ OBJECT (FIND READBIT) (TAKE HELD CARRIED ON-GROUND IN-ROOM) = V-READ PRE-REQUIRES-LIGHT>
<VERB-SYNONYM READ PERUSE>

<SYNTAX EAT OBJECT (FIND EDIBLEBIT) (TAKE HELD CARRIED ON-GROUND IN-ROOM) = V-EAT>
<VERB-SYNONYM EAT SCARF DEVOUR GULP CHEW>

<SYNTAX DRINK OBJECT = V-DRINK>

<SYNTAX SMELL OBJECT = V-SMELL>

<SYNTAX PUSH OBJECT = V-PUSH>
<VERB-SYNONYM PUSH SHOVE>

<SYNTAX PULL OBJECT = V-PULL>
<VERB-SYNONYM PULL YANK DRAG CARRY>

<SYNTAX FILL OBJECT (FIND CONTBIT) = V-FILL>
<SYNTAX EMPTY OBJECT (FIND CONTBIT) = V-EMPTY>

<SYNTAX ATTACK OBJECT = V-ATTACK>
<VERB-SYNONYM ATTACK HIT SMASH BREAK KILL DESTROY>

<SYNTAX GIVE OBJECT (HAVE HELD CARRIED) TO OBJECT (FIND PERSONBIT) = V-GIVE>
<SYNTAX GIVE OBJECT (FIND PERSONBIT) OBJECT (HAVE HELD CARRIED) = V-SGIVE>

<SYNTAX WAVE = V-WAVE-HANDS>
<SYNTAX WAVE OBJECT (TAKE HAVE HELD CARRIED) = V-WAVE>

<SYNTAX THROW OBJECT (TAKE HAVE HELD CARRIED) AT OBJECT (ON-GROUND IN-ROOM) = V-THROW-AT>

<SYNTAX BURN OBJECT ;(FIND BURNBIT) = V-BURN>
<VERB-SYNONYM BURN ROAST TORCH>

<SYNTAX RUB OBJECT = V-RUB>

<SYNTAX LOOK UNDER OBJECT (FIND SURFACEBIT) = V-LOOK-UNDER PRE-REQUIRES-LIGHT>

<SYNTAX SEARCH OBJECT (FIND CONTBIT) = V-SEARCH PRE-REQUIRES-LIGHT>
<SYNTAX LOOK IN OBJECT (FIND CONTBIT) = V-SEARCH PRE-REQUIRES-LIGHT>

<SYNTAX WAKE OBJECT (FIND PERSONBIT) = V-WAKE>
<SYNTAX WAKE UP OBJECT (FIND PERSONBIT) = V-WAKE>
<SYNTAX WAKE OBJECT (FIND PERSONBIT) UP OBJECT (FIND KLUDGEBIT) = V-WAKE>

<SYNTAX JUMP = V-JUMP>
<SYNTAX SWIM = V-SWIM>
<SYNTAX CLIMB = V-CLIMB>
<SYNTAX CLIMB OBJECT = V-CLIMB>

<SYNTAX YES = V-YES>
<SYNTAX NO = V-NO>

;"Syntaxes for game verbs"

<SYNTAX VERSION = V-VERSION>

<SYNTAX UNDO = V-UNDO>
<SYNTAX SAVE = V-SAVE>
<SYNTAX RESTORE = V-RESTORE>
<SYNTAX RESTART = V-RESTART>

<SYNTAX BRIEF = V-BRIEF>
<SYNTAX SUPERBRIEF = V-SUPERBRIEF>
<SYNTAX VERBOSE = V-VERBOSE>

;"debugging verbs - remove"
<IF-DEBUG
    <SYNTAX DROB OBJECT = V-DROB>
    <SYNTAX DSEND OBJECT TO OBJECT = V-DSEND>
    <SYNTAX DOBJL OBJECT = V-DOBJL>
    ;<SYNTAX DVIS = V-DVIS>
    ;<SYNTAX DMETALOC OBJECT = V-DMETALOC>
    ;<SYNTAX DACCESS OBJECT = V-DACCESS>
    ;<SYNTAX DHELD OBJECT IN OBJECT = V-DHELD>
    ;<SYNTAX DHELDP OBJECT = V-DHELDP>
    ;<SYNTAX DLIGHT = V-DLIGHT>
    <SYNTAX DCONT = V-DCONT>
    <SYNTAX DTURN = V-DTURN>
>

;"Constants"

;"TODO: these belong in parser.zil?"
;"Room action handlers may get: M-BEG, M-END, M-ENTER, M-LOOK"
;"Object DESCFCNs may get: M-OBJDESC?, no arg"
;"DARKNESS-F may get: M-LOOK, M-SCOPE?, M-LIT-TO-DARK, M-DARK-TO-LIT,
    M-DARK-TO-DARK, M-DARK-CANT-GO"
<CONSTANT M-BEG 1>
<CONSTANT M-END 2>
<CONSTANT M-ENTER 3>
<CONSTANT M-LOOK 4>
<CONSTANT M-OBJDESC? 5>
<CONSTANT M-SCOPE? 6>
<CONSTANT M-LIT-TO-DARK 7>
<CONSTANT M-DARK-TO-LIT 8>
<CONSTANT M-DARK-TO-DARK 9>
<CONSTANT M-DARK-CANT-GO 10>
<CONSTANT M-NOW-DARK 11>
<CONSTANT M-NOW-LIT 12>

;"Helper routines for action handlers"

<ROUTINE YOU-MASHER ("OPT" WHOM)
    <TELL "I don't think " T <OR .WHOM ,PRSO> " would appreciate that." CR>>

<ROUTINE POINTLESS (VING "OPT" PREP REV? "AUX" F S)
    <COND (.REV? <SET F ,PRSI> <SET S ,PRSO>)
          (ELSE <SET F ,PRSO> <SET S ,PRSI>)>
    <TELL .VING>
    <COND (.F
           <TELL !\  T .F>
           <COND (.PREP
                  <TELL !\  .PREP>
                  <COND (.S <TELL !\  T .S>)>)>)>
    <TELL " doesn't seem like it will help." CR>>

<ROUTINE NOT-POSSIBLE (V)
    <TELL "That's not something you can " .V "." CR>>

<ROUTINE RHETORICAL ()
    <TELL "That was a rhetorical question." CR>>

<ROUTINE BE-SPECIFIC ()
    <TELL "You'll have to be more specific." CR>>

<ROUTINE SILLY ()
    <TELL "You must be joking." CR>>

<ROUTINE TSD ()
    <TELL "Not here, not now." CR>>

<DEFMAC IF-PLURAL ('O 'IF-PL 'IF-SG)
    <FORM COND <LIST <FORM FSET? .O ',PLURALBIT> .IF-PL> <LIST ELSE .IF-SG>>>

<ROUTINE PRE-REQUIRES-LIGHT ()
    <COND (<NOT ,HERE-LIT> <TELL "It's too dark to see anything here." CR>)>>

;"Action handler routines"

<ROUTINE V-BRIEF ()
    <TELL "Brief descriptions." CR>
    <SETG MODE ,BRIEF>>

<ROUTINE V-VERBOSE ()
    <TELL "Verbose descriptions." CR>
    <SETG MODE ,VERBOSE>
    <V-LOOK>>

<ROUTINE V-SUPERBRIEF ()
    <TELL "Superbrief descriptions." CR>
    <SETG MODE ,SUPERBRIEF>>

<ROUTINE V-LOOK ()
    <COND (<DESCRIBE-ROOM ,HERE T>
           <DESCRIBE-OBJECTS ,HERE>)>>

;"Prints a room description, handling darkness and briefness.

If the room is HERE and the player doesn't have a light source, this prints
a generic room name and description.

Otherwise, the real name is printed, optionally followed by a description
(if MODE is VERBOSE, or if it's BRIEF and this is the first time describing
the room, or if LONG is true).

Uses:
  MODE

Args:
  RM: The room to describe.
  LONG: If true, print the room description even in BRIEF mode.

Returns:
  True if the objects in the room should also be described, otherwise
  false."
<ROUTINE DESCRIBE-ROOM (RM "OPT" LONG "AUX" P)
    <COND
        (<AND <==? .RM ,HERE> <NOT ,HERE-LIT>>
         <DARKNESS-F ,M-LOOK>
         <RFALSE>)
        (ELSE
         ;"print the room's real name"
         <TELL D .RM CR>
         ;"If this is an implicit LOOK, check briefness."
         <COND (<NOT .LONG>
                <COND (<EQUAL? ,MODE ,SUPERBRIEF>
                       <RFALSE>)
                      (<AND <FSET? .RM ,TOUCHBIT>
                            <NOT <EQUAL? ,MODE ,VERBOSE>>>
                       <RTRUE>)>)>
         <FSET .RM ,TOUCHBIT>
         ;"either print the room's LDESC or call its ACTION with M-LOOK"
         <COND (<SET P <GETP .RM ,P?LDESC>>
                <TELL .P CR>)
               (ELSE
                <APPLY <GETP .RM ,P?ACTION> ,M-LOOK>)>
         <RTRUE>)>>

<DEFAULT-DEFINITION DARKNESS-F

    <ROUTINE DARKNESS-F (ARG)
        <COND (<=? .ARG ,M-LOOK>
               <TELL "It is pitch black. You can't see a thing." CR>)
              (<=? .ARG ,M-SCOPE?>
               <T? <SCOPE-STAGE? GENERIC INVENTORY GLOBALS>>)
              (<=? .ARG ,M-NOW-DARK>
               <TELL "You are plunged into darkness." CR>)
              (<=? .ARG ,M-NOW-LIT>
               <TELL "You can see your surroundings now." CR CR>
               <RFALSE>)
              (ELSE <RFALSE>)>>
>

;"Describes the objects in a room.

Objects are described in four passes:
1. All non-person objects with DESCFCNs, FDESCS, and LDESCs.
2. All non-person objects not covered by #1.
3. The visible contents of containers and surfaces.
4. All objects with PERSONBIT other than WINNER.

Uses:
  WINNER

Args:
  RM: The room."
<ROUTINE DESCRIBE-OBJECTS (RM "AUX" P F N S)
    <MAP-CONTENTS (I .RM)
        <COND
            ;"objects with DESCFCNs"
            (<SET P <GETP .I ,P?DESCFCN>>
             <CRLF>
             <APPLY .P>
             <CRLF>)
            ;"un-moved objects with FDESCs"
            (<AND <NOT <FSET? .I ,TOUCHBIT>>
                  <SET P <GETP .I ,P?FDESC>>>
             <TELL CR .P CR>)
            ;"objects with LDESCs"
            (<SET P <GETP .I ,P?LDESC>>
             <TELL CR .P CR>)>>
    ;"See if there are any non fdesc, ndescbit, personbit objects in room"
    <MAP-CONTENTS (I .RM)
        <COND (<GENERIC-DESC? .I>
               <SET N T>
               <RETURN>)>>
    ;"go through the N objects"
    <COND (.N
           <TELL CR "There ">
           <ISARE-LIST .RM GENERIC-DESC? ,L-ISMANY>
           <TELL " here." CR>)>
    ;"describe visible contents of containers and surfaces"
    <MAP-CONTENTS (I .RM)
        <COND (<AND <NOT <FSET? .I ,NDESCBIT>>
                    <FIRST? .I>
                    <SEE-INSIDE? .I>>
               <DESCRIBE-CONTENTS .I>)>>
    ;"See if there are any NPCs"
    <SET N <>>
    <MAP-CONTENTS (I .RM)
        <COND (<NPC-DESC? .I>
               <SET N T>
               <RETURN>)>>
    ;"go through the N NPCs"
    <COND (.N
           <ISARE-LIST .RM NPC-DESC? ,L-SUFFIX>
           <TELL " here." CR>)>>

<ROUTINE GENERIC-DESC? (OBJ "AUX" P)
    <T? <NOT <OR <==? .OBJ ,WINNER>
                 <FSET? .OBJ ,NDESCBIT>
                 <FSET? .OBJ ,PERSONBIT>
                 <AND <NOT <FSET? .OBJ ,TOUCHBIT>>
                      <GETP .OBJ ,P?FDESC>>
                 <GETP .OBJ ,P?LDESC>
                 <AND <SET P <GETP .OBJ ,P?DESCFCN>> <APPLY .P ,M-OBJDESC?>>>>>>

<ROUTINE NPC-DESC? (OBJ "AUX" P)
    <T? <AND <FSET? .OBJ ,PERSONBIT>
             <NOT <OR <==? .OBJ ,WINNER>
                      <FSET? .OBJ ,NDESCBIT>
                      <AND <NOT <FSET? .OBJ ,TOUCHBIT>>
                           <GETP .OBJ ,P?FDESC>>
                      <GETP .OBJ ,P?LDESC>
                      <AND <SET P <GETP .OBJ ,P?DESCFCN>> <APPLY .P ,M-OBJDESC?>>>>>>>
                      


;"Prints a (short) string with the first letter capitalized."
<ROUTINE PRINT-CAP-STR (S "AUX" MAX C)
    <DIROUT 3 ,TEMPTABLE>
    <PRINT .S>
    <DIROUT -3>
    <SET MAX <GET ,TEMPTABLE 0>>
    <COND (.MAX
           <INC MAX>
           <DO (I 2 .MAX)
               <SET C <GETB ,TEMPTABLE .I>>
               <COND (<AND <=? .I 2> <G=? .C !\a> <L=? .C !\z>>
                      <SET C <- .C %<- <ASCII !\a> <ASCII !\A>>>>)>
               <PRINTC .C>>)>>

;"Prints an object name with the first letter capitalized."
<ROUTINE PRINT-CAP-OBJ (OBJ "AUX" MAX C)
    <DIROUT 3 ,TEMPTABLE>
    <PRINTD .OBJ>
    <DIROUT -3>
    <SET MAX <GET ,TEMPTABLE 0>>
    <COND (.MAX
           <INC MAX>
           <DO (I 2 .MAX)
               <SET C <GETB ,TEMPTABLE .I>>
               <COND (<AND <=? .I 2> <G=? .C !\a> <L=? .C !\z>>
                      <SET C <- .C %<- <ASCII !\a> <ASCII !\A>>>>)>
               <PRINTC .C>>)>>

;"Implements <TELL A .OBJ>."
<ROUTINE PRINT-INDEF (OBJ "AUX" A)
    <COND (<FSET? .OBJ ,NARTICLEBIT>)
          (<SET A <GETP .OBJ ,P?ARTICLE>> <TELL .A> <PRINTC !\ >)
          (<FSET? .OBJ ,PLURALBIT> <TELL "some ">)
          (<FSET? .OBJ ,VOWELBIT> <TELL "an ">)
          (ELSE <TELL "a ">)>
    <PRINTD .OBJ>>

;"Implements <TELL T .OBJ>."
<ROUTINE PRINT-DEF (OBJ)
    <COND (<NOT <FSET? .OBJ ,NARTICLEBIT>> <TELL "the ">)>
    <PRINTD .OBJ>>

;"Implements <TELL CA .OBJ>."
<ROUTINE PRINT-CINDEF (OBJ "AUX" A)
    <COND (<FSET? .OBJ ,NARTICLEBIT>
           <PRINT-CAP-OBJ .OBJ>
           <RTRUE>)>
    <COND (<SET A <GETP .OBJ ,P?ARTICLE>> <PRINT-CAP-STR .A> <PRINTC !\ >)
          (<FSET? .OBJ ,PLURALBIT> <TELL "Some ">)
          (<FSET? .OBJ ,VOWELBIT> <TELL "An ">)
          (ELSE <TELL "A ">)>
    <PRINTD .OBJ>>

;"Implements <TELL CT .OBJ>."
<ROUTINE PRINT-CDEF (OBJ)
    <COND (<FSET? .OBJ ,NARTICLEBIT>
           <PRINT-CAP-OBJ .OBJ>
           <RTRUE>)
          (ELSE <TELL "The " D .OBJ>)>>

;"Prints a sentence describing the contents of a surface or container."
<ROUTINE DESCRIBE-CONTENTS (OBJ)
    <COND (<FSET? .OBJ ,SURFACEBIT> <TELL "On">)
          (ELSE <TELL "In">)>
    <TELL " " T .OBJ " ">
    <ISARE-LIST .OBJ>
    <TELL "." CR>>

;"Prints a space followed by a parenthetical describing the contents of a
surface or container, for use in inventory listings."
<ROUTINE INV-DESCRIBE-CONTENTS (OBJ "AUX" N F)
    <COND (<FSET? .OBJ ,SURFACEBIT> <TELL " (holding ">)
          (ELSE <TELL " (containing ">)>
    <SET F <FIRST? .OBJ>>
    <COND (<NOT .F>
           <TELL "nothing)">
           <RETURN>)>
    <MAP-CONTENTS (I .OBJ)
        <SET N <+ .N 1>>>
    <COND (<==? .N 1>
           <TELL A .F>)
          (<==? .N 2>
           <TELL A .F " and " A <NEXT? .F>>)
          (ELSE
           <MAP-CONTENTS (I .OBJ)
               <TELL A .I>
               <SET N <- .N 1>>
               <COND (<0? .N>)
                     (<==? .N 1> <TELL ", and ">)
                     (ELSE <TELL ", ">)>>)>
    <TELL ")">>

;"Prints a list describing the contents of a surface or container.

The list begins with 'is' or 'are' depending on the count and plurality of
the child objects, and no trailing punctuation is printed. For example:

  If the container is empty:
    is nothing

  If the container has one singular object:
    is a shirt

  If the container has one plural object:
    are some pants

  If the container has two objects:
    are a shirt and a hat

  If the container has three objects:
    are a shirt, a hat, and a watch

Args:
  O: The object whose contents are to be listed.
  FILTER: An optional routine to select children to list.
    If provided, the list will only include objects for which the filter
    returns true; otherwise it'll list all contents.
  FLAGS: A combination of option flags:
    L-SUFFIX: Print 'is' or 'are' after the list instead of before.
    L-ISMANY: Use 'is' before a list of objects unless the first one has
      PLURALBIT. (Ignored if L-SUFFIX is given.)

Returns:
  The number of objects listed."
<ROUTINE ISARE-LIST (O "OPT" FILTER FLAGS "AUX" N F)
    <MAP-CONTENTS (I .O)
        <COND (<OR <NOT .FILTER> <APPLY .FILTER .I>>
               <OR .F <SET F .I>>
               <SET N <+ .N 1>>)>>
    <COND (<==? .N 0>
           <COND (<BTST .FLAGS ,L-SUFFIX>
                  <TELL "nothing is">)
                 (ELSE <TELL "is nothing">)>)
          (<==? .N 1>
           <AND <BTST .FLAGS ,L-SUFFIX> <TELL A .F>>
           <IF-PLURAL .F <TELL "are "> <TELL "is ">>
           <OR <BTST .FLAGS ,L-SUFFIX> <TELL A .F>>)
          (<==? .N 2>
           <OR <BTST .FLAGS ,L-SUFFIX>
               <COND (<OR <NOT <BTST .FLAGS ,L-ISMANY>>
                          <FSET? .F ,PLURALBIT>>
                      <TELL "are ">)
                     (ELSE <TELL "is ">)>>
           <TELL A .F " and " A <NEXT? .F>>
           <AND <BTST .FLAGS ,L-SUFFIX> <TELL " are">>)
          (ELSE
           <OR <BTST .FLAGS ,L-SUFFIX>
               <COND (<OR <NOT <BTST .FLAGS ,L-ISMANY>>
                          <FSET? .F ,PLURALBIT>>
                      <TELL "are ">)
                     (ELSE <TELL "is ">)>>
           <MAP-CONTENTS (I .O)
               <COND (<OR <NOT .FILTER> <APPLY .FILTER .I>>
                      <TELL A .I>
                      <SET N <- .N 1>>
                      <COND (<0? .N>)
                            (<==? .N 1> <TELL ", and ">)
                            (ELSE <TELL ", ">)>)>>
           <AND <BTST .FLAGS ,L-SUFFIX> <TELL " are">>)>
    <RETURN .N>>

<CONSTANT L-SUFFIX 1>
<CONSTANT L-ISMANY 2>

;"Direction properties have a different format on V4+, where object numbers are words."
<VERSION?
    (ZIP
        <CONSTANT UEXIT 1>          ;"size of unconditional exit"
        <CONSTANT NEXIT 2>          ;"size of non-exit"
        <CONSTANT FEXIT 3>          ;"size of function exit"
        <CONSTANT CEXIT 4>          ;"size of conditional exit"
        <CONSTANT DEXIT 5>          ;"size of door exit"

        <CONSTANT EXIT-RM 0>        ;GET/B
        <CONSTANT NEXIT-MSG 0>      ;GET
        <CONSTANT FEXIT-RTN 0>      ;GET
        <CONSTANT CEXIT-VAR 1>      ;GETB
        <CONSTANT CEXIT-MSG 1>      ;GET
        <CONSTANT DEXIT-OBJ 1>      ;GET/B
        <CONSTANT DEXIT-MSG 1>      ;GET)
    (T
        <CONSTANT UEXIT 2>
        <CONSTANT NEXIT 3>
        <CONSTANT FEXIT 4>
        <CONSTANT CEXIT 5>
        <CONSTANT DEXIT 6>

        <CONSTANT EXIT-RM 0>
        <CONSTANT NEXIT-MSG 0>
        <CONSTANT FEXIT-RTN 0>
        <CONSTANT CEXIT-VAR 4>
        <CONSTANT CEXIT-MSG 1>
        <CONSTANT DEXIT-OBJ 1>
        <CONSTANT DEXIT-MSG 2>)>

;"Checks whether PRSA is a meta-verb that does not cause time to pass."
<DEFMAC GAME-VERB? ()
    <FORM VERB? QUIT VERSION WAIT SAVE RESTORE INVENTORY ;DLIGHT UNDO ;DSEND
                SUPERBRIEF BRIEF VERBOSE AGAIN !,EXTRA-GAME-VERBS>>

<COND (<NOT <GASSIGNED? EXTRA-GAME-VERBS>> <SETG EXTRA-GAME-VERBS '()>)>

<CONSTANT CANT-GO-THAT-WAY "You can't go that way.">

<ROUTINE V-WALK ("AUX" PT PTS RM THERE-LIT)
    <COND (<NOT ,PRSO-DIR>
           <PRINTR "You must give a direction to walk in.">)
          (<0? <SET PT <GETPT ,HERE ,PRSO>>>
           <COND (<OR ,HERE-LIT <NOT <DARKNESS-F ,M-DARK-CANT-GO>>>
                  <TELL ,CANT-GO-THAT-WAY CR>)>
           <RTRUE>)
          (<==? <SET PTS <PTSIZE .PT>> ,UEXIT>
           <SET RM <GET/B .PT ,EXIT-RM>>)
          (<==? .PTS ,NEXIT>
           <TELL <GET .PT ,NEXIT-MSG> CR>
           <RTRUE>)
          (<==? .PTS ,FEXIT>
           <OR <SET RM <APPLY <GET .PT ,FEXIT-RTN>>>
               <RTRUE>>)
          (<==? .PTS ,CEXIT>
           <COND (<VALUE <GETB .PT ,CEXIT-VAR>>
                  <SET RM <GET/B .PT ,EXIT-RM>>)
                 (<SET RM <GET .PT ,CEXIT-MSG>>
                  <TELL .RM CR>
                  <RTRUE>)
                 (<AND <NOT ,HERE-LIT> <DARKNESS-F ,M-DARK-CANT-GO>>
                  <RTRUE>)
                 (ELSE
                  <TELL ,CANT-GO-THAT-WAY CR>
                  <RTRUE>)>)
          (<==? .PTS ,DEXIT>
           <COND (<FSET? <GET/B .PT ,DEXIT-OBJ> ,OPENBIT>
                  <SET RM <GET/B .PT ,EXIT-RM>>)
                 (<SET RM <GET .PT ,DEXIT-MSG>>
                  <TELL .RM CR>
                  <RTRUE>)
                 (ELSE
                  <TELL "You'll have to open " T <GET/B .PT ,DEXIT-OBJ>
                        " first." CR>
                  <RTRUE>)>)
          (ELSE
           <TELL "Broken exit (" N .PTS ")." CR>
           <RTRUE>)>
    <GOTO .RM>>

<ROUTINE V-ENTER ()
    <COND (<FSET? ,PRSO ,DOORBIT>
           <PERFORM ,V?WALK <DOOR-DIR ,PRSO>>
           <RTRUE>)
          (ELSE
           <NOT-POSSIBLE "get inside"> <RTRUE>)>>

;"Finds a direction from HERE that leads through the given door.

Returns:
  A direction property, or false if no direction leads through the door."
<ROUTINE DOOR-DIR DD (DOOR)
    <MAP-DIRECTIONS (D PT ,HERE)
        <COND (<AND <==? <PTSIZE .PT> ,DEXIT>
                    <==? <GET/B .PT ,DEXIT-OBJ> .DOOR>>
               <RETURN .D .DD>)>>
    <RFALSE>>

;"Finds the room on the other side of a given door from HERE.

Returns:
  A room, or false if no direction leads through the door."
<ROUTINE OTHER-SIDE (DOOR "AUX" D)
    <COND (<SET D <DOOR-DIR .DOOR>>
           <GET/B <GETPT ,HERE .D> ,EXIT-RM>)
          (ELSE <>)>>

<ROUTINE V-QUIT ()
    <TELL "Are you sure you want to quit?">
    <COND (<YES?>
           <TELL CR "Thanks for playing." CR>
           <QUIT>)
          (ELSE
           <TELL CR "OK - not quitting." CR>)>>

<ROUTINE V-EXAMINE ("AUX" P (N <>))
    <COND (<SET P <GETP ,PRSO P?LDESC>>
           <TELL .P CR>
           <SET N T>)>
    <COND (<SET P <GETP ,PRSO P?TEXT>>
           <TELL .P CR>
           <SET N T>)>
    <COND (<FSET? ,PRSO ,OPENABLEBIT>
           <TELL CT ,PRSO " is ">
           <COND (<FSET? ,PRSO ,OPENBIT> <TELL "open.">)
                 (ELSE <TELL "closed.">)>
           <CRLF>
           <SET N T>)>
    <COND (<AND <FIRST? ,PRSO> <SEE-INSIDE? ,PRSO>>
           <DESCRIBE-CONTENTS ,PRSO>
           <SET N T>)>
    <COND (<NOT .N>
           <TELL "You see nothing special about " T ,PRSO "." CR>)>>

<ROUTINE V-LOOK-UNDER ()
    <COND (<AND <N=? ,PRSO ,WINNER> <FSET? ,PRSO ,PERSONBIT>>
           <YOU-MASHER>
           <RTRUE>)
          (<NOT ,HERE-LIT> <TELL "It's too dark." CR>)
          (ELSE <TELL "You can't see anything of interest." CR>)>>

<ROUTINE V-SEARCH ()
    <COND (<PRSO? ,WINNER> <PERFORM ,V?INVENTORY>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (<NOT <FSET? ,PRSO ,CONTBIT>> <NOT-POSSIBLE "look inside">)
          (<AND <FSET? ,PRSO ,OPENABLEBIT> <NOT <SEE-INSIDE? ,PRSO>>>
           <TELL CT ,PRSO <IF-PLURAL ,PRSO " are" " is"> " closed." CR>)
          (<NOT <FIRST? ,PRSO>>
           <TELL CT ,PRSO <IF-PLURAL ,PRSO " are" " is"> " empty." CR>)
          (ELSE <DESCRIBE-CONTENTS ,PRSO>)>>

<ROUTINE V-INVENTORY ()
    ;"check for light first"
    <COND (,HERE-LIT
           <COND (<FIRST? ,WINNER>
                  <TELL "You are carrying:" CR>
                  <MAP-CONTENTS (I ,WINNER)
                      <TELL "   " A .I>
                      <AND <FSET? .I ,WORNBIT> <TELL " (worn)">>
                      <AND <FSET? .I ,LIGHTBIT> <TELL " (providing light)">>
                      <COND (<FSET? .I ,CONTBIT>
                             <COND (<FSET? .I ,OPENABLEBIT>
                                    <COND (<FSET? .I ,OPENBIT> <TELL " (open)">)
                                          (ELSE <TELL " (closed)">)>)>
                             <COND (<SEE-INSIDE? .I> <INV-DESCRIBE-CONTENTS .I>)>)>
                      <CRLF>>)
                 (ELSE
                  <TELL "You are empty-handed." CR>)>)
          (ELSE
           <TELL "It's too dark to see what you're carrying." CR>)>>

<ROUTINE V-TAKE ("AUX" HOLDER S X)
    <COND (<PRSO? ,WINNER>
           <COND (<=? ,P-V-WORD ,W?GET> <TELL "Not quite." CR>)
                 (<=? ,P-V-WORD ,W?TAKE ,W?GRAB> <TSD>)
                 (<=? ,P-V-WORD ,W?PICK> <TELL "You aren't my type." CR>)
                 (ELSE <SILLY>)>
           <RTRUE>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER> <RTRUE>)
          (<NOT <FSET? ,PRSO ,TAKEBIT>> <NOT-POSSIBLE "pick up"> <RTRUE>)
          (<IN? ,PRSO ,WINNER>
           <PRINTR "You already have that.">)>
    ;"See if picked up object is being taken from a container"
    <COND (<SET HOLDER <TAKE-HOLDER ,PRSO ,WINNER>>
           <COND (<FSET? .HOLDER ,PERSONBIT>
                  <TELL "That seems to belong to " T .HOLDER "." CR>
                  <RTRUE>)
                 (<BLOCKS-TAKE? .HOLDER>
                  <TELL CT .HOLDER " is in the way." CR>
                  <RTRUE>)
                 (<NOT <TAKE-CAPACITY-CHECK ,PRSO>>)
                 (<AND <FSET? .HOLDER ,CONTBIT>
                       <HELD? ,PRSO .HOLDER>
                       <NOT <HELD? ,WINNER .HOLDER>>>
                  <TELL "You reach ">
                  <COND (<HELD? ,WINNER .HOLDER>
                         <TELL "out of ">)
                        (ELSE <TELL "in ">)>
                  <TELL T .HOLDER " and ">
                  <COND (<FSET? ,PRSO ,WEARBIT>
                         <TELL "wear ">
                         <FSET ,PRSO ,WORNBIT>)
                        (ELSE <TELL "take ">)>
                  <TELL T ,PRSO "." CR>
                  <FSET ,PRSO ,TOUCHBIT>
                  <MOVE ,PRSO ,WINNER>
                  <RTRUE>)>)>
    <COND (<NOT <TAKE-CAPACITY-CHECK ,PRSO>>)
          (<FSET? ,PRSO ,WEARBIT>
           <TELL "You wear " T ,PRSO "." CR>
           <FSET ,PRSO ,WORNBIT>
           <MOVE ,PRSO ,WINNER>
           <FSET ,PRSO ,TOUCHBIT>)
          (ELSE
           <TELL "You pick up " T ,PRSO "." CR>
           <FSET ,PRSO ,TOUCHBIT>
           <MOVE ,PRSO ,WINNER>)>>

;"Locates the container, person, or room that restricts the ability to take a
given object.

If at least one thing between the taker and the object is a closed container
or a person, the closest one to the taker will be returned. Otherwise, the
innermost non-surface container or room that encloses the object will be
returned.

Args:
  OBJ: The object being taken.
  TAKER: The object doing the taking.

Returns:
  The closed container or person blocking the take, or the open container
  or room allowing the take, or ROOMS if the objects have no common parent."
<ROUTINE TAKE-HOLDER (OBJ TAKER "AUX" CEIL BLOCKER ALLOWER HAD-ALLOWER?)
    <SET CEIL <COMMON-PARENT? .OBJ .TAKER>>
    <COND (<0? .CEIL> <RETURN ,ROOMS>)>
    ;"Walk up the tree from OBJ to CEIL"
    <DO (L <LOC .OBJ> <0? .L> <SET L <LOC .L>>)
        <COND (<==? .L .CEIL>
               <RETURN>)
              (<BLOCKS-TAKE? .L>
               ;"Keep the furthest blocker from OBJ"
               <SET BLOCKER .L>)
              (<OR <AND <FSET? .L ,CONTBIT>
                        <NOT <FSET? .L ,SURFACEBIT>>>
                   <IN? .L ,ROOMS>>
               ;"Keep the closest allower to OBJ"
               <OR .ALLOWER <SET ALLOWER .L>>)>>
    ;"Walk up the tree from TAKER to CEIL, setting variables in reverse"
    <SET HAD-ALLOWER? .ALLOWER>
    <DO (L <LOC .TAKER> <0? .L> <SET L <LOC .L>>)
        <COND (<==? .L .CEIL>
               <RETURN>)
              (<BLOCKS-TAKE? .L>
               ;"Keep the closest blocker to TAKER"
               <OR .BLOCKER <SET BLOCKER .L>>)
              (<OR <AND <FSET? .L ,CONTBIT>
                        <NOT <FSET? .L ,SURFACEBIT>>>
                   <IN? .L ,ROOMS>>
               ;"Keep the furthest blocker from TAKER unless we already found
                 one on the first walk."
               <OR .HAD-ALLOWER? <SET ALLOWER .L>>)>>
    <OR .BLOCKER .ALLOWER>>

<ROUTINE BLOCKS-TAKE? (OBJ)
    <T? <OR <FSET? .OBJ ,PERSONBIT>
            <AND <FSET? .OBJ ,CONTBIT>
                 <FSET? .OBJ ,OPENABLEBIT>
                 <NOT <FSET? .OBJ ,OPENBIT>>>>>>

<DEFMAC COMMON-PARENT? ('A 'B)
    <FORM COMMON-PARENT-R .A .B ',HERE>>

<ROUTINE COMMON-PARENT-R CPR (A B ROOT "AUX" N F R)
    <OR .ROOT <RFALSE>>
    ;"If ROOT is equal to A or B, it's the common parent"
    <COND (<EQUAL? .ROOT .A .B> <RETURN .ROOT>)>
    ;"If ROOT directly contains either A or B, it's the common parent"
    <COND (<OR <IN? .A .ROOT> <IN? .B .ROOT>> <RETURN .ROOT>)>
    ;"Look for common parent in each subtree, keeping any matching
      tree and counting the number found."
    <MAP-CONTENTS (I .ROOT)
        <COND (<SET R <COMMON-PARENT-R .A .B .I>>
               <SET F .R>
               <SET N <+ .N 1>>
               ;"If we found matching parents in two children,
                ROOT is the common parent."
               <COND (<G? .N 1> <RETURN .ROOT .CPR>)>)>>
    ;"One child contained both objects, so the common parent is whatever
      COMMON-PARENT-R returned for it."
    .F>

<DEFAULT-DEFINITION TAKE-CAPACITY-CHECK

    <ROUTINE TAKE-CAPACITY-CHECK (O "AUX" (CAP <GETP ,WINNER ,P?CAPACITY>) CWT NWT)
        <COND (<L? .CAP 0> <RTRUE>)>
        <SET CWT <- <WEIGHT ,WINNER> <GETP ,WINNER ,P?SIZE>>>
        <SET NWT <WEIGHT .O>>
        <COND (<G? <+ .CWT .NWT> .CAP>
               <TELL "You're carrying too much to pick up " T .O "." CR>
               <RFALSE>)>
        <RTRUE>>
>

<ROUTINE V-DROP ()
    <COND
        (<NOT <IN? ,PRSO ,WINNER>>
         <PRINTR "You don't have that.">)
        (ELSE
         <MOVE ,PRSO ,HERE>
         <FSET ,PRSO ,TOUCHBIT>
         <FCLEAR ,PRSO ,WORNBIT>
         <TELL "You drop " T ,PRSO "." CR>)>>

<ROUTINE PRE-PUT-ON ()
    <COND (<PRSI? ,WINNER> <PERFORM ,V?WEAR ,PRSO> <RTRUE>)
          (<PRSO? ,WINNER> <PERFORM ,V?ENTER ,PRSI> <RTRUE>)
          (<NOT <HAVE-TAKE-CHECK ,PRSO ,SF-HAVE>> <RTRUE>)>>

<ROUTINE V-PUT-ON ("AUX" S CCAP CSIZE X W B)
    <COND (<FSET? ,PRSI ,PERSONBIT> <YOU-MASHER ,PRSI> <RTRUE>)
          (<NOT <AND <FSET? ,PRSI ,CONTBIT>
                     <FSET? ,PRSI ,SURFACEBIT>>>
           <NOT-POSSIBLE "put things on">
           <RTRUE>)
          (<NOT <IN? ,PRSO ,WINNER>>
           <PRINTR "You don't have that.">)
          (<OR <EQUAL? ,PRSO ,PRSI> <HELD? ,PRSI ,PRSO>>
           <PRINTR "You can't put something on itself.">)
          (ELSE
           <SET S <GETP ,PRSO ,P?SIZE>>
           <COND (<G=? <SET CCAP <GETP ,PRSI ,P?CAPACITY>> 0>
                  ;<TELL D ,PRSI " has a capacity prop of " N .CCAP CR>)
                 (ELSE
                  ;<TELL D ,PRSI " has no capacity prop.  Will take endless amount of objects as long as each object is size 5 or under" CR>
                  <SET CCAP 5>
                  ;"set bottomless flag"
                  <SET B 1>)>
           <SET CSIZE <GETP ,PRSI ,P?SIZE>>
           ;<TELL D ,PRSO "size is " N .S ", " D ,PRSI " size is " N .CSIZE ", capacity ">
                 ;<COND (<0? .B>
                            ;<TELL N .CCAP CR>)
                        ;(ELSE <TELL "infinite" CR>)>
           <COND (<OR <G? .S .CCAP> <G? .S .CSIZE>>
                  <TELL "That won't fit on " T ,PRSI "." CR>
                  <RETURN>)>
           <COND (<0? .B>
                  ;"Determine weight of contents of IO"
                  <SET W <CONTENTS-WEIGHT ,PRSI>>
                  ;<TELL "Back from Contents-weight loop" CR>
                  <SET X <+ .W .S>>
                  <COND (<G? .X .CCAP>
                         <TELL "There's not enough room on " T ,PRSI "." CR>
                         ;<TELL D ,PRSO " of size " N .S " can't fit, since current weight of " D ,PRSI "'s contents is " N .W " and " D ,PRSI "'s capacity is " N .CCAP CR>
                         <RETURN>)>
                  ; <TELL D ,PRSO " of size " N .S " can fit, since current weight of of " D ,PRSI "'s contents is " N .W " and " D ,PRSI "'s capacity is " N .CCAP CR>
                  )>
           <MOVE ,PRSO ,PRSI>
           <FSET ,PRSO ,TOUCHBIT>
           <FCLEAR ,PRSO ,WORNBIT>
           <TELL "You put " T ,PRSO " on " T ,PRSI "." CR>)>>

<ROUTINE PRE-PUT-IN ()
    <COND (<PRSI? ,WINNER> <TSD> <RTRUE>)
          (<PRSO? ,WINNER> <PERFORM ,V?ENTER ,PRSI> <RTRUE>)
          (<NOT <HAVE-TAKE-CHECK ,PRSO ,SF-HAVE>> <RTRUE>)>>

<ROUTINE V-PUT-IN ("AUX" S CCAP CSIZE X W B)
    ;<TELL "In the PUT-IN routine" CR>
    <COND (<FSET? ,PRSI ,PERSONBIT> <YOU-MASHER ,PRSI> <RTRUE>)
          (<OR <NOT <FSET? ,PRSI ,CONTBIT>>
               <FSET? ,PRSI ,SURFACEBIT>>
           <NOT-POSSIBLE "put things in">
           <RTRUE>)
          (<AND <NOT <FSET? ,PRSI ,OPENBIT>>
                <FSET? ,PRSI ,OPENABLEBIT>>
           <TELL CT ,PRSI " is closed." CR>)
          ;"always closed case"
          (<AND <NOT <FSET? ,PRSI ,OPENBIT>>
                <FSET? ,PRSI ,CONTBIT>>
           <TELL "You see no way to put things into " T ,PRSI  "." CR>)
          (<NOT <IN? ,PRSO ,WINNER>>
           <PRINTR "You aren't holding that.">)
          (<OR <EQUAL? ,PRSO ,PRSI> <HELD? ,PRSI ,PRSO>>
           <PRINTR "You can't put something in itself.">)
          (ELSE
           <SET S <GETP ,PRSO ,P?SIZE>>
           <COND (<G=? <SET CCAP <GETP ,PRSI ,P?CAPACITY>> 0>
                  <IF-DEBUG <COND (,DBCONT
                                   <TELL D ,PRSI " has a capacity prop of " N .CCAP CR>)>>)
                 (ELSE
                  <IF-DEBUG <COND (,DBCONT
                                   <TELL D ,PRSI " has no capacity prop.  Will take endless amount of objects as long as each object is size 5 or under" CR>)>>
                  <SET CCAP 5>
                  ;"set bottomless flag"
                  <SET B 1>)>
           <SET CSIZE <GETP ,PRSI ,P?SIZE>>
           ;<COND (<AND ,DBCONT> <TELL D ,PRSI " has a size of " N .CSIZE CR>)>
        <IF-DEBUG <COND (,DBCONT
               <TELL D ,PRSO "size is " N .S ", " D ,PRSI " size is " N .CSIZE CR>)>>
        ;<COND (<0? .B>
        ;<TELL N .CCAP CR>)
        ;(ELSE <TELL "infinite" CR>)>
        <COND (<OR <G? .S .CCAP>
                   <G? .S .CSIZE>>
               <TELL "That won't fit in " T ,PRSI "." CR>
               <RETURN>)>
        <COND (<0? .B>
               ;"Determine weight of contents of IO"
               <SET W <CONTENTS-WEIGHT ,PRSI>>
               ;<TELL "Back from Contents-weight loop" CR>
               <SET X <+ .W .S>>
               <COND (<G? .X .CCAP>
                      <TELL "There's not enough room in " T ,PRSI "." CR>
                      <IF-DEBUG <COND (,DBCONT
                             <TELL D ,PRSO " can't fit, since current bulk of "
                                   D ,PRSI "'s contents is " N .W " and "
                                   D ,PRSI "'s capacity is " N .CCAP CR>)>>
                      <RETURN>)>
               <IF-DEBUG <COND (,DBCONT
                      <TELL D ,PRSO " can fit, since current bulk of " D ,PRSI
                            "'s contents is " N .W " and " D ,PRSI "'s capacity is "
                            N .CCAP CR>)>>)>
        <MOVE ,PRSO ,PRSI>
        <FSET ,PRSO ,TOUCHBIT>
        <FCLEAR ,PRSO ,WORNBIT>
        <TELL "You put " T ,PRSO " in " T ,PRSI "." CR>)>>

;"Calculates the weight of all objects in a container, non-recursively."
<ROUTINE CONTENTS-WEIGHT (O "AUX" X W)
    ;"add size of objects inside container - does not recurse through containers
      within this container"
    <MAP-CONTENTS (I .O)
        ;<TELL "Content-weight loop for " D .O ", which contains " D .I CR>
        <SET W <+ .W <GETP .I ,P?SIZE>>>
        ;<TELL "Content weight of " D .O " is now " N .W CR>>
    ;<TELL "Total weight of contents of " D .O " is " N .W CR>
    .W>

;"Calculates the weight of an object, including its contents recursively."
<ROUTINE WEIGHT (O "AUX" X W)
    ;"Unlike CONTENTS-WEIGHT - drills down through all contents, adding sizes of all objects + contents"
    ;"start with size of container itself"
    <SET W <GETP .O ,P?SIZE>>
    ;"add size of objects inside container"
    <MAP-CONTENTS (I .O)
         ;<TELL "Looping to set weight.  I is currently " D .I CR>
         ;<SET W <+ .W <GETP .I ,P?SIZE>>>
         ;<TELL "Weight of " D .O " is now " N .W CR>
         <COND (<OR <FSET? .I ,CONTBIT>
                    <FSET? .I ,PERSONBIT>>
                ;<TELL "Weightloop: found container " D .I CR>
                <SET X <WEIGHT .I>>
                <SET W <+ .W .X>>
                ;<TELL "Weightloop-containerloop: Weight of " D .O " is now " N .W CR>)
               (ELSE
                <SET W <+ .W <GETP .I ,P?SIZE>>>)>>
    ;<TELL "Total weight (its size + contents' size) of " D .O " is " N .W CR>
    .W>

<ROUTINE V-WEAR ()
    <COND (<FSET? ,PRSO ,WEARBIT>
           <PERFORM ,V?TAKE ,PRSO>)
          (ELSE <NOT-POSSIBLE "wear">)>
    <RTRUE>>

<ROUTINE V-UNWEAR ()
    <COND (<AND <FSET? ,PRSO ,WORNBIT>
                <IN? ,PRSO ,WINNER>>
           <PERFORM ,V?DROP ,PRSO>)
          (ELSE <TELL "You aren't wearing that." CR>)>>

<ROUTINE V-EAT ()
    <COND (<PRSO? ,WINNER> <TSD> <RTRUE>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER> <RTRUE>)
          (<FSET? ,PRSO ,EDIBLEBIT>
           ;"TODO: move this held check to syntax flags"
           <COND (<HELD? ,PRSO>
                  <TELL "You devour " T ,PRSO "." CR>
                  <REMOVE ,PRSO>)
                 (ELSE <TELL "You're not holding that." CR>)>)
          (ELSE <TELL "That's hardly edible." CR>)>>

<ROUTINE V-VERSION ()
     <TELL ,GAME-BANNER "|Release ">
     <PRINTN <BAND <LOWCORE RELEASEID> *3777*>>
     <TELL " / Serial number ">
     <LOWCORE-TABLE SERIAL 6 PRINTC>
     <TELL %<STRING " / " ,ZIL-VERSION " lib " ,ZILLIB-VERSION>>
     <CRLF>>

<ROUTINE V-THINK-ABOUT ()
    <COND (<PRSO? ,WINNER>
           <TELL "Yes, yes, you're very important." CR>)
          (ELSE
           <TELL "You contemplate " T ,PRSO " for a bit, but nothing fruitful comes to mind." CR>)>>

<ROUTINE V-OPEN ()
    <COND (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER> <RTRUE>)
          (<NOT <FSET? ,PRSO ,OPENABLEBIT>> <NOT-POSSIBLE "open"> <RTRUE>)
          (<FSET? ,PRSO ,OPENBIT>
           <PRINTR "It's already open.">)
          (<FSET? ,PRSO ,LOCKEDBIT>
           <TELL "You'll have to unlock it first." CR>)
          (ELSE
           <FSET ,PRSO ,TOUCHBIT>
           <FSET ,PRSO ,OPENBIT>
           <TELL "You open " T ,PRSO "." CR>
           <AND ,HERE-LIT <FSET? ,PRSO ,CONTBIT> <DESCRIBE-CONTENTS ,PRSO>>
           <NOW-LIT?>)>>

<ROUTINE V-CLOSE ()
    <COND (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER> <RTRUE>)
          (<NOT <FSET? ,PRSO ,OPENABLEBIT>> <NOT-POSSIBLE "close"> <RTRUE>)
          ;(<FSET? ,PRSO ,SURFACEBIT> <NOT-POSSIBLE "close"> <RTRUE>)
          (<NOT <FSET? ,PRSO ,OPENBIT>>
           <PRINTR "It's already closed.">)
          (ELSE
           <FSET ,PRSO ,TOUCHBIT>
           <FCLEAR ,PRSO ,OPENBIT>
           <TELL "You close " T ,PRSO "." CR>
           <NOW-DARK?>)>>

<ROUTINE V-LOCK ()
    <NOT-POSSIBLE "lock">
    <RTRUE>>

<ROUTINE V-UNLOCK ()
    <NOT-POSSIBLE "unlock">
    <RTRUE>>

<ROUTINE V-WAIT ("AUX" T INTERRUPT ENDACT)
    <SET T 1>
    <TELL "Time passes." CR>
    <REPEAT ()
        ;<TELL "THE WAIT TURN IS " N .T CR>
        <SET ENDACT <APPLY <GETP ,HERE ,P?ACTION> ,M-END>>
        ;<TELL "ENDACT IS NOW " D .ENDACT CR>
        <SET INTERRUPT <CLOCKER>>
        ;<TELL "INTERRUPT IS NOW " D .INTERRUPT CR>
        <SET T <+ .T 1>>
        <COND (<OR <G? .T ,STANDARD-WAIT>
                   .ENDACT
                   .INTERRUPT>
               <RETURN>)>>>

<ROUTINE V-AGAIN ()
    <SETG AGAINCALL T>
    <RESTORE-READBUF>
    <RESTORE-LEX>
    ;<TELL "In V-AGAIN - Previous readbuf and lexbuf restored." CR>
    ;<DUMPBUF>
    ;<DUMPLEX>
    <COND (<NOT <EQUAL? <GET ,READBUF 1> 0>>
           <COND (<PARSER>
                  ;<TELL "Doing PERFORM within V-AGAIN" CR>
                  <PERFORM ,PRSA ,PRSO ,PRSI>
                  <COND (<NOT <GAME-VERB?>>
                         <APPLY <GETP ,HERE ,P?ACTION> ,M-END>
                         <CLOCKER>)>
                  <SETG HERE <LOC ,WINNER>>)>)
          (ELSE <TELL "Nothing to repeat." CR>)>
    <SETG AGAINCALL <>>>

<ROUTINE V-READ ("AUX" T)
    <COND (<NOT <FSET? ,PRSO ,READBIT>> <NOT-POSSIBLE "read"> <RTRUE>)
          (<SET T <GETP ,PRSO ,P?TEXT>>
           <TELL .T CR>)
          (<SET T <GETP ,PRSO ,P?TEXT-HELD>>
           <COND (<IN? ,PRSO ,WINNER>
                  <TELL .T CR>)
                 (ELSE
                  <TELL "You must be holding that to be able to read it." CR>)>)
          (ELSE
           <PERFORM ,V?EXAMINE ,PRSO>)>>

<ROUTINE V-TURN-ON ()
    ;<TELL "CURRENTLY IN TURN-ON" CR>
    <COND (<PRSO? ,WINNER> <TSD> <RTRUE>)
          (<NOT <FSET? ,PRSO ,DEVICEBIT>> <NOT-POSSIBLE "switch on and off"> <RTRUE>)
          (<FSET? ,PRSO ,ONBIT>
           <TELL "It's already on." CR>)
          (ELSE
           <FSET ,PRSO ,ONBIT>
           <TELL "You switch on " T ,PRSO "." CR>)>>

<ROUTINE V-TURN-OFF ()
    ;<TELL "CURRENTLY IN TURN-OFF" CR>
    <COND (<PRSO? ,WINNER>
           <TELL <PICK-ONE-R <PLTABLE "Baseball." "Cold showers.">> CR>)
          (<NOT <FSET? ,PRSO ,DEVICEBIT>>
           <NOT-POSSIBLE "switch on and off"> <RTRUE>)
          (<NOT <FSET? ,PRSO ,ONBIT>>
           <TELL "It's already off." CR>)
          (ELSE
           <FCLEAR ,PRSO ,ONBIT>
           <TELL "You switch off " T ,PRSO "." CR>)>>

<ROUTINE V-FLIP ()
    <COND (<NOT <FSET? ,PRSO ,DEVICEBIT>>
           <COND (<FSET? ,PRSO ,SURFACEBIT>
                  <POINTLESS "Taking your frustration out on">)
                 (ELSE <NOT-POSSIBLE "switch on and off">)>)
          (<FSET? ,PRSO ,ONBIT>
           <PERFORM ,V?TURN-OFF ,PRSO>)
          (ELSE
           <PERFORM ,V?TURN-ON ,PRSO>)>
    <RTRUE>>

<ROUTINE V-PUSH ()
    <COND (<PRSO? ,WINNER> <TELL "No, you seem close to the edge." CR>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <POINTLESS "Pushing">)>
    <RTRUE>>

<ROUTINE V-PULL ()
    <COND (<PRSO? ,WINNER> <TELL "That would demean both of us." CR>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <POINTLESS "Pulling">)>
    <RTRUE>>

<ROUTINE V-YES ()
    <RHETORICAL>
    <RTRUE>>

<ROUTINE V-NO ()
    <RHETORICAL>
    <RTRUE>>

<ROUTINE V-DRINK ()
    <TELL "You aren't ">
    <ITALICIZE "that">
    <TELL " thirsty." CR>>

<ROUTINE V-FILL ()
    <BE-SPECIFIC>
    <RTRUE>>

<ROUTINE V-EMPTY ()
    <BE-SPECIFIC>
    <RTRUE>>

<ROUTINE V-SMELL ()
    <TELL "You smell nothing unexpected." CR>>

<ROUTINE V-ATTACK ()
    <COND (<PRSO? ,WINNER> <TELL "Let's hope it doesn't come to that." CR>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <POINTLESS "Taking your frustration out on">)>
    <RTRUE>>

<ROUTINE V-THROW-AT ()
    <COND (<PRSO? ,WINNER> <TELL "Get" <IF-PLURAL ,PRSO " them" " it"> " yourself." CR>)
          (<FSET? ,PRSI ,PERSONBIT> <YOU-MASHER ,PRSI>)
          (ELSE <POINTLESS "Taking your frustration out on" <> T>)>
    <RTRUE>>

<ROUTINE V-GIVE ()
    <COND (<PRSI? ,WINNER> <TELL "Get it yourself." CR>)
          (<PRSO? ,WINNER> <SILLY>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (<NOT <FSET? ,PRSI ,PERSONBIT>> <NOT-POSSIBLE "give things to">)
          (ELSE <TELL CT ,PRSI <IF-PLURAL ,PRSI " don't" " doesn't">
                      " take " T ,PRSO "." CR>)>>

<ROUTINE V-SGIVE ()
    <PERFORM ,V?GIVE ,PRSI ,PRSO>
    <RTRUE>>

<ROUTINE V-WAVE-HANDS ()
    <POINTLESS "Waving your hands">
    <RTRUE>>

<ROUTINE V-WAVE ()
    <SILLY>
    <RTRUE>>

<ROUTINE V-CLIMB ()
    <COND (,PRSO <NOT-POSSIBLE "climb">) (ELSE <SILLY>)>
    <RTRUE>>

<ROUTINE V-SWIM ()
    <SILLY>
    <RTRUE>>

<ROUTINE V-JUMP ()
    <POINTLESS "Jumping in place">
    <RTRUE>>

<ROUTINE V-WAKE ()
    <COND (<PRSO? ,WINNER> <TELL "If only this were a dream." CR>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <NOT-POSSIBLE "wake">)>>

<ROUTINE V-RUB ()
    <COND (<PRSO? ,WINNER> <TSD>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <POINTLESS "Rubbing">)>>

<ROUTINE V-BURN ()
    <COND (<PRSO? ,WINNER> <TELL "What is this, the Friars Club?" CR>)
          (<FSET? ,PRSO ,PERSONBIT> <YOU-MASHER>)
          (ELSE <POINTLESS "Recklessly incinerating">)>>

;"Action handlers for game verbs"

<ROUTINE V-UNDO ("AUX" R)
    <IFFLAG
        (UNDO
         <COND (<NOT ,USAVE>
                <TELL "Cannot undo any further." CR>
                <RETURN>)>
         <SET R <IRESTORE>>
         <COND (<EQUAL? .R 0>
                <TELL "Undo failed." CR>)>)
        (ELSE <TELL "Undo is not available in this version." CR>)>>

<ROUTINE V-SAVE ("AUX" S)
     ;<TELL "Now in save routine" CR>
    <TELL "Saving..." CR>
    <COND (<SAVE> <V-LOOK>)
          (ELSE <TELL "Save failed." CR>)>>

<ROUTINE V-RESTORE ("AUX" R)
    ; <TELL "Now in restore routine" CR>
    <COND (<NOT <RESTORE>>
           <TELL "Restore failed." CR>)>>

<ROUTINE V-RESTART ()
    <TELL "Are you sure you want to restart?" CR>
    <COND (<YES?>
           <RESTART>)
          (ELSE
           <TELL "Restart aborted." CR>)>>

;"Debugging verbs"
<IF-DEBUG
    <ROUTINE V-DROB ()
        <TELL "REMOVING CONTENTS OF " D ,PRSO " FROM THE GAME." CR>
        <ROB ,PRSO>>

    <ROUTINE V-DSEND ()
        <TELL "SENDING CONTENTS OF " D ,PRSO " TO " D ,PRSI "." CR>
        <ROB ,PRSO ,PRSI>>

    <ROUTINE V-DOBJL ()
        <MAP-CONTENTS (I ,PRSO)
            <TELL "The objects in " T ,PRSO " include " D .I CR>>>

    ;<ROUTINE V-DVIS ("AUX" P)
        <SET P <VISIBLE? ,BILL>>
        <COND (<NOT .P>
                    <TELL "The bill is not visible." CR>)
                        (ELSE
                    <TELL "The bill is visible." CR>)>
        <SET P <VISIBLE? ,GRIME>>
        <COND (<NOT .P>
                    <TELL "The grime is not visible." CR>)
                        (ELSE
                    <TELL "The grime is visible." CR>)>
        >

    ;<ROUTINE V-DMETALOC ("AUX" P)
        <SET P <META-LOC ,PRSO>>
        <COND (<NOT .P>
                <TELL "META-LOC returned false." CR>)
                  (ELSE
                <TELL "Meta-Loc of " D ,PRSO " is " D .P CR>)>
        <SET P <META-LOC ,GRIME>>
        <COND (<NOT .P>
                <TELL "META-LOC of grime returned false." CR>)
                  (ELSE
                <TELL "Meta-Loc of grime is " D .P CR>)
                >
        >

    ;<ROUTINE V-DACCESS ("AUX" P)
        <SET P <ACCESSIBLE? ,PRSO>>
        <COND (<NOT .P>
                <TELL D ,PRSO " is not accessible" CR>)
                  (ELSE
                <TELL D ,PRSO " is accessible" CR>)>
        <SET P <ACCESSIBLE? ,BILL>>
        <COND (<NOT .P>
                <TELL "Bill is not accessible" CR>)
                  (ELSE
                <TELL "Bill is accessible" CR>)>
        >

    ;<ROUTINE V-DHELD ("AUX" P)
        <SET P <HELD? ,PRSO ,PRSI>>
        <COND (<NOT .P>
                <TELL D ,PRSO " is not held by " D ,PRSI CR>)
                  (ELSE
                <TELL D ,PRSO " is held by " D ,PRSI CR>)>
        <SET P <HELD? ,PRSO ,FOYER>>
        <COND (<NOT .P>
                <TELL D ,PRSO " is not held by the Foyer" CR>)
                  (ELSE
                <TELL D ,PRSO " is held by the Foyer" CR>)>
        >

    ;<ROUTINE V-DHELDP ("AUX" P)
        <SET P <HELD? ,PRSO>>
        <COND (<NOT .P>
                <TELL D ,PRSO " is not held by the player" CR>)
                  (ELSE
                <TELL D ,PRSO " is held by the player" CR>)>
        <SET P <HELD? ,BILL>>
        <COND (<NOT .P>
                <TELL "Bill is not held by the player" CR>)
                  (ELSE
                <TELL "Bill is held by the player" CR>)>
        >

    ;<ROUTINE V-DLIGHT ()
        <COND (<FSET? ,FLASHLIGHT ,LIGHTBIT>
                    <FCLEAR ,FLASHLIGHT ,LIGHTBIT>
                    <FCLEAR ,FLASHLIGHT ,ONBIT>
                    <TELL "Flashlight is turned off." CR>
                    <NOW-DARK>)
                  (ELSE
                    <TELL "Flashlight is turned on." CR>
                    <NOW-LIT>
                    ;"always set LIGHTBIT after calling NOW-LIT"
                    <FSET ,FLASHLIGHT ,LIGHTBIT>
                    <FSET ,FLASHLIGHT ,ONBIT>
                    )>
        >

    <ROUTINE V-DCONT ()
        <COND (,DBCONT
               <SET DBCONT <>>
               <TELL "Reporting of PUT-IN process with containers turned off." CR>)
              (ELSE
               <SET DBCONT T>
               <TELL "Reporting of PUT-IN process with containers turned on." CR>)>>

    <ROUTINE V-DTURN ()
        <COND (,DTURNS
               <SET DTURNS <>>
               <TELL "Reporting of TURN # turned off." CR>)
              (ELSE
               <SET DTURNS T>
               <TELL "Reporting of TURN # turned on." CR>)>>>

<VERSION ZIP>

<SETG NEW-VOC? T>

<INSERT-FILE "testing">

<OBJECT STARTROOM
    (IN ROOMS)
    (DESC "Start Room")
    (NORTH TO HALLWAY)
    (FLAGS LIGHTBIT)>

<OBJECT APPLE
    (IN STARTROOM)
    (DESC "apple")
    (SYNONYM APPLE)
    (FLAGS TAKEBIT VOWELBIT)>

<OBJECT HALLWAY
    (IN ROOMS)
    (DESC "Hallway")
    (NORTH TO CLOSET)
    (SOUTH TO STARTROOM)
    (FLAGS LIGHTBIT)>

<OBJECT CLOSET
    (IN ROOMS)
    (DESC "Closet")
    (SOUTH TO HALLWAY)
    (FLAGS LIGHTBIT)>

<TEST-SETUP ()
    ;"In case a test leaves it in the wrong place..."
    <MOVE ,WINNER ,STARTROOM>>

<TEST-CASE ("GO without direction")
    <COMMAND [GO]>
    <EXPECT "Which way do you want to go?|">
    <CHECK <IN? ,WINNER ,STARTROOM>>>

<TEST-CASE ("Repeat motion with AGAIN")
    <COMMAND [NORTH]>
    <EXPECT "Hallway|">
    <COMMAND [AGAIN]>
    <EXPECT "Closet|">
    <CHECK <IN? ,WINNER ,CLOSET>>>

<TEST-CASE ("Direction preceded by garbage")
    <COMMAND ["1234" NORTH]>
    <EXPECT "That sentence has no verb.|">
    <CHECK <IN? ,WINNER ,STARTROOM>>
    <COMMAND [APPLE \, NORTH]>
    <EXPECT "Talking to an apple, huh?|">
    <CHECK <IN? ,WINNER ,STARTROOM>>>

<TEST-CASE ("Direction followed by garbage")
    <COMMAND [NORTH ME]>
    <EXPECT "That sentence has no verb.|">
    <CHECK <IN? ,WINNER ,STARTROOM>>
    <COMMAND [NORTH LOOK]>
    <EXPECT "I didn't expect the word \"look\" there.|">
    <CHECK <IN? ,WINNER ,STARTROOM>>
    <COMMAND [NORTH GO THROUGH APPLE]>
    <EXPECT "I don't understand what \"north\" is doing in that sentence.|">
    <CHECK <IN? ,WINNER ,STARTROOM>>>

<TEST-CASE ("Multiple directions")
    <COMMAND [EAST NORTH]>
    <EXPECT "I didn't expect the word \"north\" there.|">
    <CHECK <IN? ,WINNER ,STARTROOM>>>

<TEST-GO ,STARTROOM>

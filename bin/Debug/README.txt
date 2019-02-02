
TINY ADIAGO RELEASE NOTES

20170516
	simplified note "duration-ing"
	note resting
		whole note rest = wr or rw
		half note rest = hr or rh
		quarter note rest = qr or rq
	relatative note duration
		quarter = <note>
		half = <note>h
		quarter = <note>q
		quarter = <note>hh
		eight = <note>qh or <note>hq
	fixed duration / rest
		whole = <note>1
		half = <note>.5
		1/32 = <note>.03125
		whole rest = r1
		half rest = r.5
		eigth rest = r.0625

20170514
	added back commas as whitespace
	added individual note and chord durations and rests (quarter/whole/half/sixteeth and custom)
	improved chord demo progression
	added support for multi-'track' parsing and playback. 
	

20170506
	added common chords as direct parsing
	changed chords to be all capital letters
	notes now entered as lowercase
	added automatic note/chord durationing and song BPM support (BPM120;BPM30;BPM200)

20170501
	fixed chord playback bug
	added prettier parsing/song display
	added command-line help

20170428
	Fixed parsing library bug
	
20170426
	Added advanced music parsing library (disabled because of a bug)
	Added improved adiago for string example
// VAR globals.ink // a mettre quand trigger

=== tutomange ===
// trigger 4
Par ici, la salade est de saison #speaker: Simone #portrait: Simone
{gamepad:
Goûte, clique X #speaker: Simone #portrait: Simone
-else:
Goûte, clique F #speaker: Simone #portrait: Simone
}
// collider pas trop grand pour ne pas aller trop loin, avec un trigger qui demande de rentrer
~tutomange_completed=true // il faudrait verifier qu'il clique pour dire "completed"
-> tutofin   // a enlever quand trigger
-> END
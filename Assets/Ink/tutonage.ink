// VAR globals.ink // a mettre quand trigger

=== tutonage ===
// trigger 3 sortie du terrier, faire surface, manger
{gamepad:
Pour faire surface ou plonger, clique B #speaker: Simone #portrait: Simone
-else:
Pour faire surface ou plonger, clique E #speaker: Simone #portrait: Simone
}
~tutonage_completed=true // il faudrait verifier qu'il clique ou qu'il est avancé pour dire "completed"
// mettre Simone dans les herbes
-> tutomange  // a enlever quand trigger
-> END

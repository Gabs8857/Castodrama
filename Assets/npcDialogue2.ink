VAR score = 0

VAR question_1 = ""
VAR reponse_1 = ""
VAR explication_q1 = ""

-> start

=== start ===

Castor : Salut, alors t'en as pensé quoi du stream ? # speaker:Castor
Laura Musqué : C'était super. # speaker:LauraMusque
Laura Musqué : Tu veux voir ton bilan ? # speaker:LauraMusque

+ Oui
    -> bilan

+ Non
    Laura Musqué : Ok, on continue. # speaker:LauraMusque
    -> END


=== bilan ===

Laura Musqué : Ton score est de {score} / 1 # speaker:LauraMusque

Laura Musqué : Pour la question "{question_1}" tu as répondu {reponse_1} # speaker:LauraMusque
Laura Musqué : {explication_q1} # speaker:LauraMusque

{score == 1:
    Laura Musqué : Incroyable, sans faute ! # speaker:LauraMusque
- else:
    {score > 1:

        Laura Musqué : Pas mal, mais il reste encore des choses à apprendre. # speaker:LauraMusque
    }
}
-> END

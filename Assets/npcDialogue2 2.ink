VAR score = 0
VAR firstAnswer = ""
VAR secondAnswer = ""
VAR q1_explanation = ""
VAR q2_explanation = ""

-> start

=== start ===

Castor : Salut, alors t'en as pensé quoi du stream ?
Laura Musqué : C'était super.
Laura Musqué : Tu veux voir ton bilan ?

+ Oui
    -> bilan

+ Non
    Laura Musqué : Ok, on continue.
    -> END


=== bilan ===

Laura Musqué : ton score est de {score} / 2

Laura Musqué : Pour la question une tu as répondu {firstAnswer}
Laura Musqué : {q1_explanation}

Laura Musqué : Pour la question deux tu as répondu {secondAnswer}
Laura Musqué : {q2_explanation}

{score == 2:
    Laura Musqué : Parfait.
- else:
    Laura Musqué : Pas mal, continue.
}

-> END

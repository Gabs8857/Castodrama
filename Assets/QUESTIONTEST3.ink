VAR score = 0
VAR firstAnswer = ""
VAR secondAnswer = ""
VAR q1_explanation = ""
VAR q2_explanation = ""

-> start

=== start ===
-> q1

=== q1 ===
Pourquoi traquait-on les castors européens ?

+ Leurs oreilles
    ~ firstAnswer = "Leurs oreilles"
    ~ q1_explanation = "Les oreilles n’étaient pas recherchées..."
    -> q2

+ Leur pelage
    ~ firstAnswer = "Leur pelage"
    ~ score += 1
    ~ q1_explanation = "C'est en effet pour le pelage"
    -> q2

+ Leur castoreum
    ~ firstAnswer = "Leur castoreum"
    ~ q1_explanation = "C'est partiellement correct, mais pas la raison principale"
    -> q2


=== q2 ===
Et la glande du castor, le castoreum, elle se trouve où ?

+ Sous la queue
    ~ secondAnswer = "Sous la queue"
    ~ score += 1
    ~ q2_explanation = "Bonne réponse... C'est en effet sous la queue que se trouve le castoreum"
    -> result

+ Dans le ventre
    ~ secondAnswer = "Dans le ventre"
    ~ q2_explanation = "Faux... C'est sous la queue"
    -> result

+ Dans la joue
    ~ secondAnswer = "Dans la joue"
    ~ q2_explanation = "Faux... C'est sous la queue"
    -> result


=== result ===
-> END
VAR score = 0

VAR question_1 = ""
VAR reponse_1 = ""
VAR explication_q1 = ""

-> start

=== start ===
-> q1

=== q1 ===
~ question_1 = "Pourquoi traquait-on les castors européens ?"
{question_1} # speaker:Castor

+ Leurs oreilles
    ~ reponse_1 = "Leurs oreilles"
    ~ explication_q1 = "Les oreilles n'étaient pas recherchées..."
    -> result

+ Leur pelage
    ~ reponse_1 = "Leur pelage"
    ~ score += 1
    ~ explication_q1 = "C'est en effet pour le pelage"
    -> result

+ Leur castoreum
    ~ reponse_1 = "Leur castoreum"
    ~ explication_q1 = "C'est partiellement correct, mais pas la raison principale"
    -> result

+ [Temps écoulé]
    ~ reponse_1 = "Pas de réponse"
    ~ explication_q1 = "Tu n'as pas répondu à temps..."
    -> result




=== result ===
-> END

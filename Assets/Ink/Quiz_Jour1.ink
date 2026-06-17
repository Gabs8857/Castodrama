VAR score = 0
VAR question_1 = "" 
VAR reponse_1 = "" 
VAR explication_q1 = ""
VAR question_2 = "" 
VAR reponse_2 = "" 
VAR explication_q2 = ""
VAR question_3 = "" 
VAR reponse_3 = "" 
VAR explication_q3 = ""
VAR question_4 = "" 
VAR reponse_4 = "" 
VAR explication_q4 = ""
VAR question_5 = "" 
VAR reponse_5 = "" 
VAR explication_q5 = ""
-> start
=== start ===
-> q1

=== q1 ===
~ question_1 = "Question 1 jour 1 ?"
j1q1: Connaissance du castor !
+ Mammifère ! # correct
    -> q2
+ Poisson !
    -> q2
+ Drama ? 
    -> drama_j1q1
+ jsp
    -> q2

=== drama_j1q1 ===
Le Pape a historiquement affirmé que le castor est un poisson, ce qui le rend consommable pendant la période de carême.
+ Mammifère !!
    ->q2
+ Poisson !!
    ->q2
+ Je sais toujours pas 
    ->q2
    
=== q2 ===
~ question_2 = "Question 2 jour 1 ?"
Question 2 jour 1 ?
+ Réponse A # correct
    -> q3
+ Réponse B
    -> q3
+ Réponse C
    -> q3
+ [Temps ecoule]
    ~ reponse_2 = "Pas de réponse"
    ~ explication_q2 = "Tu n'as pas répondu à temps."
    -> q3

=== q3 ===
~ question_3 = "Question 3 jour 1 ?"
Question 3 jour 1 ?
+ Réponse A # correct
    -> q4
+ Réponse B
    -> q4
+ Réponse C
    -> q4
+ [Temps ecoule]
    ~ reponse_3 = "Pas de réponse"
    ~ explication_q3 = "Tu n'as pas répondu à temps."
    -> q4

=== q4 ===
~ question_4 = "Question 4 jour 1 ?"
Question 4 jour 1 ?
+ Réponse A # correct
    -> q5
+ Réponse B
    -> q5
+ Réponse C
    -> q5
+ [Temps ecoule]
    ~ reponse_4 = "Pas de réponse"
    ~ explication_q4 = "Tu n'as pas répondu à temps."
    -> q5

=== q5 ===
~ question_5 = "Question 5 jour 1 ?"
Question 5 jour 1 ?
+ Réponse A # correct
    -> result
+ Réponse B
    -> result
+ Réponse C
    -> result
+ [Temps ecoule]
    ~ reponse_5 = "Pas de réponse"
    ~ explication_q5 = "Tu n'as pas répondu à temps."
    -> result

=== result ===
-> END

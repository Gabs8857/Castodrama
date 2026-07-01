VAR score = 0
VAR question_6 = "" 
VAR reponse_6 = "" 
VAR explication_q6 = ""
VAR question_7 = "" 
VAR reponse_7 = "" 
VAR explication_q7 = ""
VAR question_8 = "" 
VAR reponse_8 = "" 
VAR explication_q8 = ""
VAR question_9 = "" 
VAR reponse_9 = "" 
VAR explication_q9 = ""
VAR question_10 = "" 
VAR reponse_10 = "" 
VAR explication_q10 = ""
VAR question_11 = "" 
VAR reponse_11 = "" 
VAR explication_q11 = ""

-> start
=== start ===
-> q1

=== q1 ===
~ question_6 = "Question 1 jour 2 ?"
Question 1 jour 2 ?
+ Réponse A # correct
    -> q2
+ Réponse B
    -> q2
+ [Temps ecoule]
    ~ reponse_6 = "Pas de réponse"
    ~ explication_q6 = "Tu n'as pas répondu à temps."
    -> q2

=== q2 ===
~ question_7 = "Question 2 jour 2 ?"
Question 2 jour 2 ?
+ Réponse A # correct
    -> q3
+ Réponse B
    -> q3
+ [Temps ecoule]
    ~ reponse_7 = "Pas de réponse"
    ~ explication_q7 = "Tu n'as pas répondu à temps."
    -> q3

=== q3 ===
~ question_8 = "Question 3 jour 2 ?"
Question 3 jour 2 ?
+ Réponse A # correct
    -> q4
+ Réponse B
    -> q4
+ [Temps ecoule]
    ~ reponse_8 = "Pas de réponse"
    ~ explication_q8 = "Tu n'as pas répondu à temps."
    -> q4

=== q4 ===
~ question_9 = "Question 4 jour 2 ?"
Question 4 jour 2 ?
+ Réponse A # correct
    -> q5
+ Réponse B
    -> q5
+ [Temps ecoule]
    ~ reponse_9 = "Pas de réponse"
    ~ explication_q9 = "Tu n'as pas répondu à temps."
    -> q5

=== q5 ===
~ question_10 = "Question 5 jour 2 ?"
Question 5 jour 2 ?
+ Réponse A # correct
    -> q6
+ Réponse B
    -> q6
+ [Temps ecoule]
    ~ reponse_10 = "Pas de réponse"
    ~ explication_q10 = "Tu n'as pas répondu à temps."
    -> q6

=== q6 ===
~ question_11 = "Question 6 jour 2 ?"
Question 6 jour 2 ?
+ Réponse A # correct
    -> result
+ Réponse B
    -> result
+ [Temps ecoule]
    ~ reponse_11 = "Pas de réponse"
    ~ explication_q11 = "Tu n'as pas répondu à temps."
    -> result

=== result ===
-> END

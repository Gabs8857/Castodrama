VAR score = 0
VAR question_12 = "" 
VAR reponse_12 = "" 
VAR explication_q12 = ""
VAR question_13 = "" 
VAR reponse_13 = "" 
VAR explication_q13 = ""
VAR question_14 = "" 
VAR reponse_14 = "" 
VAR explication_q14 = ""
VAR question_15 = "" 
VAR reponse_15 = "" 
VAR explication_q15 = ""
VAR question_16 = "" 
VAR reponse_16 = "" 
VAR explication_q16 = ""
VAR question_17 = "" 
VAR reponse_17 = "" 
VAR explication_q17 = ""

-> start
=== start ===
-> q1

=== q1 ===
~ question_13 = "Question 1 jour 3 ?"
Question 1 jour 3 ?
+ Réponse A # correct
    -> q2
+ Réponse B
    -> q2
+ [Temps ecoule]
    ~ reponse_13 = "Pas de réponse"
    ~ explication_q13 = "Tu n'as pas répondu à temps."
    -> q2

=== q2 ===
~ question_14 = "Question 2 jour 3 ?"
Question 2 jour 3 ?
+ Réponse A # correct
    -> q3
+ Réponse B
    -> q3
+ [Temps ecoule]
    ~ reponse_14 = "Pas de réponse"
    ~ explication_q14 = "Tu n'as pas répondu à temps."
    -> q3

=== q3 ===
~ question_15 = "Question 3 jour 3 ?"
Question 3 jour 3 ?
+ Réponse A # correct
    -> q4
+ Réponse B
    -> q4
+ [Temps ecoule]
    ~ reponse_15 = "Pas de réponse"
    ~ explication_q15 = "Tu n'as pas répondu à temps."
    -> q4

=== q4 ===
~ question_15 = "Question 4 jour 3 ?"
Question 4 jour 3 ?
+ Réponse A # correct
    -> q5
+ Réponse B
    -> q5
+ [Temps ecoule]
    ~ reponse_15 = "Pas de réponse"
    ~ explication_q15 = "Tu n'as pas répondu à temps."
    -> q5

=== q5 ===
~ question_16 = "Question 5 jour 3 ?"
Question 5 jour 3 ?
+ Réponse A # correct
    -> q6
+ Réponse B
    -> q6
+ [Temps ecoule]
    ~ reponse_16 = "Pas de réponse"
    ~ explication_q16 = "Tu n'as pas répondu à temps."
    -> q6

=== q6 ===
~ question_17 = "Question 6 jour 3 ?"
Question 6 jour 3 ?
+ Réponse A # correct
    -> result
+ Réponse B
    -> result
+ [Temps ecoule]
    ~ reponse_17 = "Pas de réponse"
    ~ explication_q17 = "Tu n'as pas répondu à temps."
    -> result

=== result ===
-> END

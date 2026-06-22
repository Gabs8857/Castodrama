// ============================================================
//  QUIZ.INK  — fichier unique, tous jours confondus
//  La variable current_day est injectée depuis C# avant
//  d'appeler story.Continue() (via story.variablesState).
//
//  CORRECTIF : Quiz_Jour3 q4 écrasait question_15/reponse_15
//  au lieu de question_16/reponse_16 — corrigé ici.
// ============================================================

VAR current_day = 1

// --- Jour 1 ---
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

// --- Jour 2 ---
VAR question_6  = ""
VAR reponse_6  = ""
VAR explication_q6  = ""
VAR question_7  = ""
VAR reponse_7  = ""
VAR explication_q7  = ""
VAR question_8  = ""
VAR reponse_8  = ""
VAR explication_q8  = ""
VAR question_9  = ""
VAR reponse_9  = ""
VAR explication_q9  = ""
VAR question_10 = ""
VAR reponse_10 = ""
VAR explication_q10 = ""
VAR question_11 = ""
VAR reponse_11 = ""
VAR explication_q11 = ""

// --- Jour 3 ---
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
{
    - current_day == 1: -> quiz_jour1
    - current_day == 2: -> quiz_jour2
    - current_day == 3: -> quiz_jour3
    - else:             -> quiz_jour1
}

// ============================================================
//  QUIZ JOUR 1
// ============================================================
=== quiz_jour1 ===
-> j1_q1

=== j1_q1 ===
~ question_1 = "Connaissance du castor ! 4 pattes, 1 queue plate genre écaillée, je suis, je suis..."
j1q1: Connaissance du castor ! 4 pattes, 1 queue plate genre écaillée, je suis, je suis...
+ Un mammifère ? # correct
    -> drama_j1q1
+ Un poisson ?
    -> drama_j1q1
+ Drama ?
    -> drama_j1q1
+ jsp
    -> drama_j1q1

=== drama_j1q1 ===
drama_j1q1: Hélàs, je suis goûteux. Pour qu'on puisse me bouloter en période de Carême, le Vatican décide que je suis un...
+ Mammifère ?
    -> END
+ Poisson ?
    -> END
+ Rongeur ?
    -> END
+ tu peux répéter ?
    -> END

=== j1_q2 ===
~ question_2 = "Tu l'auras compris bouffi, on m'a chassé pour"
j1q2: Tu l'auras compris bouffi, on m'a chassé pour
+ ma fourrure # correct
    -> END
+ ma viande
    -> END
+ mon odeur sensuelle
    -> END
+ jsp
    -> END

=== j1_q3 ===
~ question_3 = "Heureusement, aujourd'hui je suis protégé"
j1q3: Heureusement, aujourd'hui je suis protégé
+ contre la pluie
    -> drama_j1q3
+ par la loi
    -> drama_j1q3
+ Drama
    -> drama_j1q3

=== drama_j1q3 ===
drama_j1q4: Mes barrages aussi sont protégés !
+ Certains me copie par biomimétisme
    -> END
+ Hors-la-loi, d'autres les détruisent
    -> END
+ Que fait Robin-des-Bois ?
    -> END
+ jsp
    -> END

=== j1_q4 ===
~ question_4 = "On dit de moi que je suis une espèce \"clé de voute\" en créant..."
On dit de moi que je suis une espèce "clé de voute" en créant...
+ des habitats pour d'autres espèces # correct
    -> END
+ de petits univers humides
    -> END
+ en streamant en musique
    -> END

=== j1_q5 ===
~ question_5 = "En Europe, mon logis est plutôt ?"
En Europe, mon logis est plutôt ?
+ un terrier
    -> drama_q5
+ une hutte # correct
    -> drama_q5
+ Drama
    -> drama_q5

=== drama_q5 ===
Drama: Mes cousins d'Amérique préfèrent les huttes. Prendront-il ma place ?
+ Jsp, mais les scientifiques veillent
    -> END
+ Cool, nous nous reproduirons
    -> END
+ Pffu, l'avenir le dira.
    -> END

// ============================================================
//  QUIZ JOUR 2
// ============================================================
=== quiz_jour2 ===
-> j2_q1

=== j2_q1 ===
~ question_6 = "Question 1 jour 2 ?"
Question 1 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j2_q2 ===
~ question_7 = "Question 2 jour 2 ?"
Question 2 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j2_q3 ===
~ question_8 = "Question 3 jour 2 ?"
Question 3 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j2_q4 ===
~ question_9 = "Question 4 jour 2 ?"
Question 4 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j2_q5 ===
~ question_10 = "Question 5 jour 2 ?"
Question 5 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j2_q6 ===
~ question_11 = "Question 6 jour 2 ?"
Question 6 jour 2 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

// ============================================================
//  QUIZ JOUR 3
// ============================================================
=== quiz_jour3 ===
-> j3_q1

=== j3_q1 ===
~ question_12 = "Question 1 jour 3 ?"
Question 1 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j3_q2 ===
~ question_13 = "Question 2 jour 3 ?"
Question 2 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j3_q3 ===
~ question_14 = "Question 3 jour 3 ?"
Question 3 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

// CORRECTIF : l'original écrasait question_15/reponse_15 au lieu de 16
=== j3_q4 ===
~ question_15 = "Question 4 jour 3 ?"
Question 4 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j3_q5 ===
~ question_16 = "Question 5 jour 3 ?"
Question 5 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

=== j3_q6 ===
~ question_17 = "Question 6 jour 3 ?"
Question 6 jour 3 ?
+ Réponse A # correct
    -> END
+ Réponse B
    -> END

// ============================================================
//  FIN COMMUNE
// ============================================================
=== quiz_result ===
-> END

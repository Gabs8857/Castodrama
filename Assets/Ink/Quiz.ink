// ============================================================
//  QUIZ.INK  — fichier unique, tous jours confondus
//  La variable current_day est injectée depuis C# avant
//  d'appeler story.Continue() (via story.variablesState).
//
//  CORRECTIF : Quiz_Jour3 q4 écrasait question_15/reponse_15
//  au lieu de question_16/reponse_16 — corrigé ici.
// ============================================================
INCLUDE globals.ink
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
~ question_1 = "Connaissance"
4 pattes, 1 queue plate genre écaillée, je suis...
+ [Un mammifère !] # correct
    -> END
+ [Un poisson !]
    -> END
+ [Drama !]
    -> END
+ [jsp...]
    -> END

=== drama_j1_q1 ===
~ question_2 = "Drama_Connaissance"
Hélàs, je suis goûteux. Pour qu'on me boulote lors du Carême, le Vatican a dit que j'étais un...
+ [Mammifère] # correct
    -> END
+ [Poisson]
    -> END
+ [Rongeur]
    -> END
+ [Tu peux répéter ?]
    -> END

=== j1_q2 ===
~ question_3 = "Chasse"
Tu l'auras compris, on m'a chassé pour...
+ [Ma fourrure] # correct
    -> END
+ [Ma viande] # correct
    -> END
+ [Mon odeur sensuelle] # correct
    -> END
+ [Jsp...]
    -> END

=== j1_q3 ===
~ question_4 = "Protection"
Aujourd'hui je suis protégé...
+ [Contre la pluie.]
    -> END
+ [Par la loi.]   # correct
    -> END
+ Drama !
    -> END

=== drama_j1_q3 ===
~ question_5 = "Drama_Protection"
Mes barrages aussi sont protégés !
+ [Certains les copient.]    # correct //par biomimétisme
    -> END
+ [D'autres les détruisent.] # correct //Hors-la-loi
    -> END
+ [Que fait Robin-des-Bois ?]
    -> END

=== j1_q4 ===
~ question_6 = "CleDeVoute"
On dit de moi que je suis une espèce "clé de voute" en créant...
+ [Des habitats pour d'autres.] # correct
    -> END
+ [De petits univers humides.] # correct
    -> END
+ [En streamant en musique.]
    -> END

=== j1_q5 ===
~ question_7 = "Logis"
En Europe, mon logis est plutôt...
+ [Un terrier.] # correct
    -> END
+ [Une hutte.]
    -> END
+ [Drama !]
    -> END

=== drama_j1_q5 ===
~ question_8 = "DramaLogis"
Mes cousins d'Amérique préfèrent les huttes. Prendront-il ma place ?
+ [Jsp, mais des observateurs veillent.] # correct
    -> END
+ [Cool, nous nous reproduirons.] // pas de repro possible
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

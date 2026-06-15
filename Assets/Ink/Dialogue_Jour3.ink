VAR score = 0
VAR signatures_total = 0
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
Laura Musqué : C'est la dernière nuit ! # speaker:LauraMusque
Laura Musqué : Tu veux voir le bilan final ? # speaker:LauraMusque

+ Oui
    -> bilan
+ Non
    Laura Musqué : D'accord, tu es sûr ? # speaker:LauraMusque
    -> END

=== bilan ===
Laura Musqué : Score du dernier jour : {score} / 6 # speaker:LauraMusque
Laura Musqué : Q1 — {question_12} : {reponse_12} # speaker:LauraMusque
Laura Musqué : {explication_q12} # speaker:LauraMusque
Laura Musqué : Q2 — {question_13} : {reponse_13} # speaker:LauraMusque
Laura Musqué : {explication_q13} # speaker:LauraMusque
Laura Musqué : Q3 — {question_14} : {reponse_14} # speaker:LauraMusque
Laura Musqué : {explication_q14} # speaker:LauraMusque
Laura Musqué : Q4 — {question_15} : {reponse_15} # speaker:LauraMusque
Laura Musqué : {explication_q15} # speaker:LauraMusque
Laura Musqué : Q5 — {question_16} : {reponse_16} # speaker:LauraMusque
Laura Musqué : {explication_q16} # speaker:LauraMusque
Laura Musqué : Q6 — {question_17} : {reponse_17} # speaker:LauraMusque
Laura Musqué : {explication_q17} # speaker:LauraMusque

Laura Musqué : Au total tu as récolté {signatures_total} signatures ! # speaker:LauraMusque

{signatures_total >= 1000:
    Laura Musqué : Incroyable ! Tu as atteint l'objectif des 1000 signatures ! Le barrage est sauvé ! # speaker:LauraMusque
- else:
    Laura Musqué : Dommage, l'objectif n'est pas atteint. Mais tu as sensibilisé beaucoup de gens ! # speaker:LauraMusque
}

-> END

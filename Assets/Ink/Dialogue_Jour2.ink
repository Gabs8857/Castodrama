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
Laura Musqué : Bonsoir ! Belle journée de stream ? # speaker:LauraMusque
Laura Musqué : Tu veux voir ton bilan du jour 2 ? # speaker:LauraMusque

+ Oui
    -> bilan
+ Non
    Laura Musqué : D'accord, repose-toi bien ! # speaker:LauraMusque
    -> END

=== bilan ===
Laura Musqué : Score du jour : {score} / 5 # speaker:LauraMusque
Laura Musqué : Q1 — {question_6} : {reponse_6} # speaker:LauraMusque
Laura Musqué : {explication_q6} # speaker:LauraMusque
Laura Musqué : Q2 — {question_7} : {reponse_7} # speaker:LauraMusque
Laura Musqué : {explication_q7} # speaker:LauraMusque
Laura Musqué : Q3 — {question_8} : {reponse_8} # speaker:LauraMusque
Laura Musqué : {explication_q8} # speaker:LauraMusque
Laura Musqué : Q4 — {question_9} : {reponse_9} # speaker:LauraMusque
Laura Musqué : {explication_q9} # speaker:LauraMusque
Laura Musqué : Q5 — {question_10} : {reponse_10} # speaker:LauraMusque
Laura Musqué : {explication_q10} # speaker:LauraMusque
Laura Musqué : Q6 — {question_11} : {reponse_11} # speaker:LauraMusque
Laura Musqué : {explication_q11} # speaker:LauraMusque
{score >= 4:
    Laura Musqué : Excellent résultat, le chat est en feu ! # speaker:LauraMusque
- else:
    Laura Musqué : Pas mal, on progresse ! Bonne nuit. # speaker:LauraMusque
}

-> END

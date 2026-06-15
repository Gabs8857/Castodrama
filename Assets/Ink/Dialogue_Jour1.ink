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
Laura Musqué : Bonsoir ! Belle journée de stream ? # speaker:LauraMusque
Laura Musqué : Tu veux voir ton bilan du jour 1 ? # speaker:LauraMusque

+ Oui
    -> bilan
+ Non
    Laura Musqué : D'accord, repose-toi bien ! # speaker:LauraMusque
    -> END

=== bilan ===
Laura Musqué : Score du jour : {score} / 5 # speaker:LauraMusque
Laura Musqué : Q1 — {question_1} : {reponse_1} # speaker:LauraMusque
Laura Musqué : {explication_q1} # speaker:LauraMusque
Laura Musqué : Q2 — {question_2} : {reponse_2} # speaker:LauraMusque
Laura Musqué : {explication_q2} # speaker:LauraMusque
Laura Musqué : Q3 — {question_3} : {reponse_3} # speaker:LauraMusque
Laura Musqué : {explication_q3} # speaker:LauraMusque
Laura Musqué : Q4 — {question_4} : {reponse_4} # speaker:LauraMusque
Laura Musqué : {explication_q4} # speaker:LauraMusque
Laura Musqué : Q5 — {question_5} : {reponse_5} # speaker:LauraMusque
Laura Musqué : {explication_q5} # speaker:LauraMusque

{score >= 4:
    Laura Musqué : Excellent résultat, le chat est en feu ! # speaker:LauraMusque
- else:
    Laura Musqué : Pas mal, on progresse ! Bonne nuit. # speaker:LauraMusque
}

-> END

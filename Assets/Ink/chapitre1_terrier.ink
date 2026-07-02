// ============================================================
//  TUTO
// ============================================================

//pour le tuto dans l'ordre 
//tutodebut + tutomarche + tutocarte + tutohalo + tutonage + tutomange + tutofin

//-> tutodebut
//-> tutocarte
//-> tutohalo
//-> tutonage
//-> tutomange
//-> tutofin
VAR marcher_completed=false
VAR gamepad=true
VAR carte=false

VAR tutomarche_completed=false
VAR tutocarte_completed=false
VAR tutonage_completed=false
VAR tutomange_completed=false
VAR tutohalo_completed=false

// junior est couché, trigger1 autour de junior au demaragge1
// pour lancer le dialogue immediatement
=== tutodebut ===
Junior, approche-toi s'il te plait.
#speaker:Socrate #portrait:Socrate
Bien Père ! #speaker:Junior #portrait:Junior
->tutomarche
->END


=== tutomarche ===
{gamepad:
Pour bouger, utilise le joystick gauche #speaker: Simone #portrait: Simone
-else:
Pour bouger, utilise les touches ZQSD ou les flêches #speaker: Simone #portrait: Simone
}
->END

// VAR globals.ink // a mettre quand trigger


=== tutocarte === // dans trigger2
//junior s'est levé et a marché vers ses parents
~ tutomarche_completed=true
Nous, Castors, vivons la nuit. 
Si nous avons une mauvaise vue, nous la compensons par d'autres sens bien plus développés comme le toucher et l'odorat, et une grande connaissance de notre environnement.
~carte=true
// la carte s'affiche sur l'ecran
C'est comme une carte mentale qui ressemblerait à ça.

Approche-toi, regarde ces points lumineux. 
~tutocarte_completed=true
-> END

// VAR globals.ink // a mettre quand trigger


=== tutohalo ===
Les verts indiquent les herbes, les tons jaunes, les arbres, dont nous reconnaissons les différentes essences. 


// trigger supplémentaire ? ou bien on met les deux ensemble
// le halo de junior passe au jaune
Mère, Pére, que se passe-t-il ?

Junior, dans le jeu comme dans la vie, il y a des priorités. #portrait: Socrate #speaker: Socrate
Le halo qui t'entoure est une aide pour te déplacer dans le noir, il signale aussi ton humeur.  #portrait: Simone #speaker: Simone
En vert, tout va bien, en jaune : il fait faim !  #portrait: Socrate #speaker: Socrate
Tu en découvriras d'autres lors de tes explorations 

Junior, sortons bouloter ! #portrait: Simone #speaker: Simone
~tutohalo_completed=true
->END


=== tutonage ===
// trigger 3 sortie du terrier, faire surface, manger
{gamepad:
Pour faire surface ou plonger, clique B #speaker: Simone #portrait: Simone
-else:
Pour faire surface ou plonger, clique E #speaker: Simone #portrait: Simone
}
~tutonage_completed=true // il faudrait verifier qu'il clique ou qu'il est avancé pour dire "completed"
// mettre Simone dans les herbes
-> END


=== tutomange ===
// trigger 4
Par ici, la salade est de saison #speaker: Simone #portrait: Simone
{gamepad:
Goûte, clique X #speaker: Simone #portrait: Simone
-else:
Goûte, clique F #speaker: Simone #portrait: Simone
}
// collider pas trop grand pour ne pas aller trop loin, avec un trigger qui demande de rentrer
~tutomange_completed=true // il faudrait verifier qu'il clique pour dire "completed"
-> END


=== tutofin ===
//trigger 5  pas trop grand pour ne pas aller trop loin, avec un trigger qui demande de rentrer qui invite a rentrer
// et aussi trigger 6 si on rentre dans le terrier avec le meme dialogue (la il faudra reflechir car il faudra detruire les deux triggers ou bien invalider les dialogues avec es variables)
Hâtons nous vers le terrier.
Regarde le cadran en haut a gauche, le soleil se lève bientôt.
Demain sera une grande nuit pour le peuple castor !
-> END
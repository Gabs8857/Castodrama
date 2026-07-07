VAR marcher_completed=false
VAR gamepad=false
VAR carte=false

-> debut

-> nouscastor

// junior est couché, trigger autour de junior au demaragge1
// pour lancer le dialogue immediatement

=== debut ===
Junior, approche-toi s'il te plait.
#speaker:Socrate #portrait:Socrate
Bien Père ! #speaker:Junior #portrait:Junior

{gamepad:
Pour bouger, utilise le joystick gauche #speaker: Simone #portrait: Simone
-else:
Pour bouger, utilise les touches ZQSD ou les flêches #speaker: Simone #portrait: Simone
}
//junior se leve et marche vers ses parents, second trigger
~ marcher_completed=true
-> nouscastor
->DONE

=== nouscastor ===
Nous, Castors, vivons la nuit. 
Si nous avons une mauvaise vue, nous la compensons par d'autres sens bien plus développés comme le toucher et l'odorat, et une grande connaissance de notre environnement.
~carte=true
// la carte s'affiche sur l'ecran
C'est comme une carte mentale qui ressemblerait à ça.

Regarde les points lumineux. 
Les verts indiquent les herbes, les tons jaunes, les arbres, dont nous reconnaissons les différentes essences. 


// le halo de junior passe au jaune
Mère, Pére, que se passe-t-il ?

Junior, dans le jeu comme dans la vie, il y a des priorités. #portrait: Socrate #speaker: Socrate
Le halo qui t'entoure est une aide pour te déplacer dans le noir, il signale aussi ton humeur.  #portrait: Simone #speaker: Simone
En vert, tout va bien, en jaune : il fait faim !  #portrait: Socrate #speaker: Socrate
Tu en découvriras d'autres lors de tes explorations 

Junior, sortons bouloter ! #portrait: Simone #speaker: Simone
faire surface, manger
-> nage
->DONE

=== nage ===
// trigger sortie du terrier
{gamepad:
Pour faire surface ou plonger, clique B #speaker: Simone #portrait: Simone
-else:
Pour faire surface ou plonger, clique E #speaker: Simone #portrait: Simone
}
// mettre Simone dans les herbes
-> DONE

=== manger ===
// trigger trigger
Par ici, la salade est de saison #speaker: Simone #portrait: Simone
{gamepad:
Goûte, clique A #speaker: Simone #portrait: Simone
-else:
Goûte, clique F #speaker: Simone #portrait: Simone
}

// collider pas trop grand

=== retour ===
Hâte-on nous vers le terrier
Regarde le cadran en haut a gauche, le soleil se lève bientôt.
Demain sera une grande nuit pour le peuple castor !
-> DONE










-> END
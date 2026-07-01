
using UnityEngine;
using Ink.Runtime;

[RequireComponent(typeof(Collider))]
public class CampDialogueTrigger : MonoBehaviour
{
    [Header("Fichier Ink à jouer")]
    [SerializeField] private TextAsset inkJSON;

    [Header("Nom de la variable globale Ink à tester")]
    [SerializeField] private string inkBoolName = "retour_camp_started";

    [Header("Quel tag déclenche ?")]
    [SerializeField] private string tagDetecte = "Player";



    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name} ► le Collider devrait être ‘Is Trigger’");
    }

    void OnTriggerEnter(Collider other)
    {

        if (!other.CompareTag(tagDetecte))
            return;

 
        if (inkJSON == null)
        {
            Debug.LogError($"{name}Ink JSON non assigné !");
            return;
        }


        if (!ConditionRemplie())
            return; 

        DialogueManager.Instance?.EnterDialogueMode(inkJSON);

        Destroy(gameObject);         
    }


    bool ConditionRemplie()
    {
        var vars = DialogueManager.Instance?.DialogueVariables.variables;
        if (vars == null) return false;

        return vars.TryGetValue(inkBoolName, out var val) &&
               (val as BoolValue)?.value == true;
    }
}

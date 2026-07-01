using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class DialogueVariables
{
    // Accès public aux variables
    public Dictionary<string, Ink.Runtime.Object> variables { get; private set; }

    private Story globalVariablesStory;
    private const string SAVE_KEY = "INK_GLOBAL_VARS";

    // =========================================================================
    // CONSTRUCTEUR
    // =========================================================================
    public DialogueVariables(TextAsset globalsJSON)
    {
        variables = new Dictionary<string, Ink.Runtime.Object>();

        if (globalsJSON != null)
        {
            globalVariablesStory = new Story(globalsJSON.text);
            // Charger les variables déclarées dans le fichier globals.ink
            foreach (string name in globalVariablesStory.variablesState)
            {
                variables[name] = globalVariablesStory.variablesState.GetVariableWithName(name);
            }
            LoadVariables(); // Charger les valeurs sauvegardées
        }
        else
        {
            Debug.LogError("[DialogueVariables] globalsJSON est NULL !");
        }
    }

    // =========================================================================
    // SYNCHRONISATION AVEC LES STORIES
    // =========================================================================
    public void StartListening(Story story)
    {
        if (story == null) return;

        // Injecter les variables globales dans la nouvelle Story
        foreach (var kvp in variables)
        {
            try
            {
                story.variablesState.SetGlobal(kvp.Key, kvp.Value);
            }
            catch
            {
                // Ignorer les erreurs de type (ex: variable absente du fichier Ink)
            }
        }

        // Écouter les changements de variables dans cette Story
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story)
    {
        if (story != null)
        {
            story.variablesState.variableChangedEvent -= VariableChanged;
        }
        SaveVariables(); // Sauvegarder à la fin du dialogue
    }

    private void VariableChanged(string name, Ink.Runtime.Object value)
    {
        // Mettre à jour le cache local
        variables[name] = value;
    }

    // =========================================================================
    // SAUVEGARDE / CHARGEMENT
    // =========================================================================
    public void SaveVariables()
    {
        if (globalVariablesStory == null) return;
        try
        {
            // Synchroniser les variables globales
            foreach (var kvp in variables)
            {
                globalVariablesStory.variablesState.SetGlobal(kvp.Key, kvp.Value);
            }
            // Sauvegarder dans PlayerPrefs
            PlayerPrefs.SetString(SAVE_KEY, globalVariablesStory.state.ToJson());
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueVariables] Erreur lors de la sauvegarde: {e.Message}");
        }
    }

    private void LoadVariables()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;
        try
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            globalVariablesStory.state.LoadJson(json);
            // Recharger les variables dans le dictionnaire
            foreach (string name in globalVariablesStory.variablesState)
            {
                variables[name] = globalVariablesStory.variablesState.GetVariableWithName(name);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DialogueVariables] Erreur lors du chargement: {e.Message}");
        }
    }
}
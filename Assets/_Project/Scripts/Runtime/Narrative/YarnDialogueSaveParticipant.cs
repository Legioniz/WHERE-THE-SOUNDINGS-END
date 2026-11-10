using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>
    /// Persists Yarn's typed variable storage inside the game's existing save document.
    /// Assign the same DialogueRunner used by the active scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class YarnDialogueSaveParticipant : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        public DialogueRunner Runner
        {
            get { return dialogueRunner; }
            set { dialogueRunner = value; }
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.dialogue == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain dialogue state.");

            if (dialogueRunner == null || dialogueRunner.VariableStorage == null)
                return;

            var variables = dialogueRunner.VariableStorage.GetAllVariables();
            data.dialogue.floatVariables.Clear();
            data.dialogue.stringVariables.Clear();
            data.dialogue.boolVariables.Clear();

            foreach (KeyValuePair<string, float> pair in variables.FloatVariables)
            {
                data.dialogue.floatVariables.Add(new YarnFloatVariableSaveData
                {
                    variableName = pair.Key,
                    value = pair.Value
                });
            }

            foreach (KeyValuePair<string, string> pair in variables.StringVariables)
            {
                data.dialogue.stringVariables.Add(new YarnStringVariableSaveData
                {
                    variableName = pair.Key,
                    value = pair.Value
                });
            }

            foreach (KeyValuePair<string, bool> pair in variables.BoolVariables)
            {
                data.dialogue.boolVariables.Add(new YarnBoolVariableSaveData
                {
                    variableName = pair.Key,
                    value = pair.Value
                });
            }
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.dialogue == null ||
                dialogueRunner == null || dialogueRunner.VariableStorage == null)
                return;

            Dictionary<string, float> floats = new Dictionary<string, float>();
            Dictionary<string, string> strings = new Dictionary<string, string>();
            Dictionary<string, bool> bools = new Dictionary<string, bool>();

            for (int i = 0; i < data.dialogue.floatVariables.Count; i++)
            {
                YarnFloatVariableSaveData variable = data.dialogue.floatVariables[i];
                if (variable != null && !string.IsNullOrWhiteSpace(variable.variableName))
                    floats[variable.variableName] = variable.value;
            }

            for (int i = 0; i < data.dialogue.stringVariables.Count; i++)
            {
                YarnStringVariableSaveData variable = data.dialogue.stringVariables[i];
                if (variable != null && !string.IsNullOrWhiteSpace(variable.variableName))
                    strings[variable.variableName] = variable.value ?? string.Empty;
            }

            for (int i = 0; i < data.dialogue.boolVariables.Count; i++)
            {
                YarnBoolVariableSaveData variable = data.dialogue.boolVariables[i];
                if (variable != null && !string.IsNullOrWhiteSpace(variable.variableName))
                    bools[variable.variableName] = variable.value;
            }

            dialogueRunner.VariableStorage.SetAllVariables(floats, strings, bools, true);
        }
    }
}

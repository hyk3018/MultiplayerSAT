using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using TK.Core.Common;
using UnityEngine;

namespace Client.UI
{
    public class CommandPanelUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject commandButtonPrefab;

        private void Awake()
        {
            gameObject.SetActive(false);
            CommandSensor.CommandTypesChange += OnCommandTypesChange;
        }

        private void OnDestroy()
        {
            CommandSensor.CommandTypesChange -= OnCommandTypesChange;
        }

        // Update panel with button per command available
        private void OnCommandTypesChange(Dictionary<CommandExecutor, List<CommandExecutionData>> availableCommands)
        {
            transform.RemoveAllChildGameObjects();
            gameObject.SetActive(false);
            var setActive = false;
            foreach (CommandExecutor executor in availableCommands.Keys)
            {
                foreach (CommandExecutionData commandData in availableCommands[executor])
                {
                    if (!setActive)
                    {
                        gameObject.SetActive(true);
                        setActive = true;
                    }
                    var go = Instantiate(commandButtonPrefab, transform);
                    var commandButton = go.GetComponent<CommandButtonUI>();
                    commandButton.Initialise(executor, commandData);
                }
            }

        }
    }
}
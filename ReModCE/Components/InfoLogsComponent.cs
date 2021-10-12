﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using ReModCE.UI;
using VRC;

namespace ReModCE.Components
{
    internal class InfoLogsComponent : ModComponent
    {
        private ConfigValue<bool> JoinLeaveLogsEnabled;
        private ReMenuToggle _joinLeaveLogsToggle;

        public InfoLogsComponent()
        {
            JoinLeaveLogsEnabled = new ConfigValue<bool>(nameof(JoinLeaveLogsEnabled), true);
            JoinLeaveLogsEnabled.OnValueChanged += () => _joinLeaveLogsToggle.Toggle(JoinLeaveLogsEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _joinLeaveLogsToggle = menu.AddToggle("JoinLeaveLogs", "Join/Leave Logs",
                "Enable whether player joins/leaves should be logged in console.", JoinLeaveLogsEnabled.SetValue,
                JoinLeaveLogsEnabled);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            ReLogger.Msg(ConsoleColor.Cyan, $"{player.field_Private_APIUser_0.displayName} joined the instance.");
        }

        public override void OnPlayerLeft(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            ReLogger.Msg(ConsoleColor.White, $"{player.field_Private_APIUser_0.displayName} left the instance.");
        }
    }
}

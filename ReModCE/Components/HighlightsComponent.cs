﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Core;
using ReModCE.Managers;
using ReModCE.UI;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace ReModCE.Components
{
    internal class HighlightsComponent : ModComponent
    {
        private HighlightsFXStandalone _friendsHighlights;
        private HighlightsFXStandalone _othersHighlights;

        private ConfigValue<Color> FriendsColor;
        private ConfigValue<Color> OthersColor;
        private ConfigValue<bool> ESPEnabled;
        private ReMenuToggle _espToggle;
        private ReMenuButton _friendsColorButton;
        private ReMenuButton _othersColorButton;

        public HighlightsComponent()
        {
            FriendsColor = new ConfigValue<Color>(nameof(FriendsColor), Color.yellow);
            OthersColor = new ConfigValue<Color>(nameof(OthersColor), Color.magenta);

            ESPEnabled = new ConfigValue<bool>(nameof(ESPEnabled), false);
            ESPEnabled.OnValueChanged += () => _espToggle.Toggle(ESPEnabled);

            RiskyFunctionsManager.Instance.OnRiskyFunctionsChanged += allowed =>
            {
                if (_espToggle != null)
                {
                    _espToggle.Interactable = allowed;
                }
                if (!allowed)
                    ESPEnabled.SetValue(false);
            };
        }

        public override void OnUiManagerInitEarly()
        {
            var highlightsFx = HighlightsFX.field_Private_Static_HighlightsFX_0;

            _friendsHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _friendsHighlights.highlightColor = FriendsColor;
            _othersHighlights = highlightsFx.gameObject.AddComponent<HighlightsFXStandalone>();
            _othersHighlights.highlightColor = OthersColor;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Visuals");
            _espToggle = menu.AddToggle("ESP", "ESP/Highlights", "Enable ESP (Highlight players through walls)", b =>
            {
                ESPEnabled.SetValue(b);
                ToggleESP(b);
            }, ESPEnabled);

            _friendsColorButton = menu.AddButton("FriendsColor", $"<color=#{FriendsColor.Value.ToHex()}>Friends</color> Color",
                $"Set your <color=#{FriendsColor.Value.ToHex()}>friends</color> highlight color",
                () =>
                {
                    PopupColorInput(_friendsColorButton, "Friends", FriendsColor);
                });

            _othersColorButton = menu.AddButton("OthersColor", $"<color=#{OthersColor.Value.ToHex()}>Others</color> Color",
                $"Set <color=#{OthersColor.Value.ToHex()}>other</color> peoples highlight color",
                () =>
                {
                    PopupColorInput(_othersColorButton, "Others", OthersColor);
                });
        }

        private void PopupColorInput(ReMenuButton button, string who, ConfigValue<Color> configValue)
        {
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Input hex color code",
                $"#{configValue.Value.ToHex()}", InputField.InputType.Standard, false, "Submit",
                (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return;

                    if (!ColorUtility.TryParseHtmlString(s, out var color))
                        return;

                    configValue.SetValue(color);

                    button.Text = $"<color=#{configValue.Value.ToHex()}>{who}</color> Color";
                }, null);
        }

        private void ToggleESP(bool enabled)
        {
            var playerManager = PlayerManager.field_Private_Static_PlayerManager_0;
            if (playerManager == null)
                return;

            foreach (var player in playerManager.GetPlayers())
            {
                HighlightPlayer(player, enabled);
            }
        }

        private void HighlightPlayer(Player player, bool highlighted)
        {
            if (!RiskyFunctionsManager.Instance.RiskyFunctionAllowed)
                return;

            if (player.field_Private_APIUser_0.IsSelf)
                return;

            var selectRegion = player.transform.Find("SelectRegion");
            if (selectRegion == null)
                return;

            GetHighlightsFX(player.field_Private_APIUser_0).Method_Public_Void_Renderer_Boolean_0(selectRegion.GetComponent<Renderer>(), highlighted);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!ESPEnabled)
                return;

            HighlightPlayer(player, ESPEnabled);
        }

        private HighlightsFXStandalone GetHighlightsFX(APIUser apiUser)
        {
            if (APIUser.IsFriendsWith(apiUser.id))
                return _friendsHighlights;

            return _othersHighlights;
        }
    }
}

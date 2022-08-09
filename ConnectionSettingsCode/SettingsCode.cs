using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectionSettingsCode
{
    public class SettingsCode : IMenuElement, ISelectable
    {
        public MenuPage Parent { get; }
        public string Version { get; }
        public string ModName { get; }

        public bool Hidden => controlPanel.Hidden;

        private readonly VerticalItemPanel controlPanel;
        private readonly IEnumerable<IValueElement> elements;

        /// <summary>
        /// Creates a settings code control, to be placed on the root of the page. Works for standard value types (string,
        /// int, float, bool, enum).
        /// </summary>
        /// <param name="parent">The menu page to place the settings control on</param>
        /// <param name="mod">The mod associated with these settings</param>
        /// <param name="elements">The elements to include in the settings code</param>
        public SettingsCode(MenuPage parent, Mod mod, IEnumerable<IValueElement> elements)
        {
            Parent = parent;
            Version = mod.GetVersion();
            ModName = mod.GetName();

            SmallButton copy = new(parent, "Copy Settings");
            copy.OnClick += () => EncodeAndCopySettings(copy);
            SmallButton paste = new(parent, "Apply Settings");
            paste.OnClick += () => PasteAndDecodeSettings(paste);

            controlPanel = new VerticalItemPanel(parent, new(-SpaceParameters.HSPACE_MEDIUM, -450 + SpaceParameters.VSPACE_SMALL),
                SpaceParameters.VSPACE_SMALL, false, copy, paste);
            parent.backButton.SymSetNeighbor(Neighbor.Left, this);

            this.elements = elements;
        }

        /// <inheritdoc/>
        public SettingsCode(MenuPage parent, Mod mod, params IValueElement[] elements) : this(parent, mod, (IEnumerable<IValueElement>)elements) { }

        public void MoveTo(Vector2 pos) => controlPanel.MoveTo(pos);

        public void Translate(Vector2 delta) => controlPanel.Translate(delta);

        public void Hide() => controlPanel.Hide();

        public void Show() => controlPanel.Show();

        public void Destroy() => controlPanel.Destroy();

        public void SetNeighbor(Neighbor neighbor, ISelectable selectable) => controlPanel.SetNeighbor(neighbor, selectable);

        public ISelectable GetISelectable(Neighbor neighbor) => controlPanel.GetISelectable(neighbor);

        public Selectable GetSelectable(Neighbor neighbor) => controlPanel.GetSelectable(neighbor);

        private void EncodeAndCopySettings(SmallButton button)
        {
            GUIUtility.systemCopyBuffer = $"{ModName};{Version};{SettingsCodeUtil.Encode(elements.Select(x => x.Value))}";
            button.Text.StartCoroutine(DisplayMessage(button, 2.5f, "Copied!"));
        }

        private void PasteAndDecodeSettings(SmallButton button)
        {
            string[] pieces = GUIUtility.systemCopyBuffer.Split(';');
            if (pieces.Length != 3 || pieces[0] != ModName)
            {
                button.Text.StartCoroutine(DisplayMessage(button, 2.5f, "Invalid Code"));
                return;
            }
            if (pieces[1] != Version)
            {
                button.Text.StartCoroutine(DisplayMessage(button, 2.5f, "Version Mismatch"));
                return;
            }
            IEnumerable<JValue>? values = SettingsCodeUtil.Decode(pieces[2]);
            if (values == null)
            {
                button.Text.StartCoroutine(DisplayMessage(button, 2.5f, "Invalid Code"));
                return;
            }
            foreach ((JValue val, IValueElement elem) in values.Zip(elements, (val, elem) => (val, elem)))
            {
                object converted;
                switch (val.Type)
                {
                    case JTokenType.String:
                        converted = (string)val!;
                        break;
                    case JTokenType.Boolean:
                        converted = (bool)val;
                        break;
                    case JTokenType.Float:
                        converted = (float)val;
                        break;
                    case JTokenType.Integer:
                        converted = (int)val;
                        if (elem.ValueType.IsEnum && Enum.IsDefined(elem.ValueType, converted))
                        {
                            converted = Enum.ToObject(elem.ValueType, (int)converted);
                        }
                        break;
                    default:
                        converted = val;
                        break;
                }
                elem.SetValue(converted);
            }
            button.Text.StartCoroutine(DisplayMessage(button, 2.5f, "Applied Settings"));
        }

        private IEnumerator DisplayMessage(SmallButton button, float delay, string text)
        {
            string cachedText = button.Text.text;
            button.Lock();
            button.Text.text = text;
            yield return new WaitForSeconds(delay);
            button.Text.text = cachedText;
            button.Unlock();
        }
    }
}

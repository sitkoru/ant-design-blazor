﻿using System;
using AntDesign.Core.Extensions;
using AntDesign.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AntDesign
{
    public partial class InputPassword : Input<string>
    {
        private bool _visible = false;
        private string _eyeIcon;

        [Parameter]
        public bool VisibilityToggle { get; set; } = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Type = "password";
            ToggleVisibility(new MouseEventArgs());
        }

        protected override void SetClasses()
        {
            base.SetClasses();
            //ant-input-password-large ant-input-affix-wrapper ant-input-affix-wrapper-lg
            ClassMapper
                .If($"{PrefixCls}-password-large", () => Size == InputSize.Large)
                .If($"{PrefixCls}-password-small", () => Size == InputSize.Small)
                .If($"{PrefixCls}-password-rtl", () => RTL);

            AffixWrapperClass = string.Join(" ", AffixWrapperClass, $"{PrefixCls}-password");

            if (VisibilityToggle)
            {
                Suffix = new RenderFragment((builder) =>
                {
                    int i = 0;
                    builder.OpenElement(i++, "span");
                    builder.AddAttribute(i++, "class", $"{PrefixCls}-suffix");
                    builder.OpenComponent<Icon>(i++);
                    builder.AddAttribute(i++, "class", $"{PrefixCls}-password-icon");
                    builder.AddAttribute(i++, "type", _eyeIcon);
                    builder.AddAttribute(i++, "onclick", CallbackFactory.Create<MouseEventArgs>(this, async args =>
                    {
                        var element = await JsInvokeAsync<HtmlElement>(JSInteropConstants.GetDomInfo, Ref);

                        IsFocused = true;
                        await this.FocusAsync(Ref);

                        ToggleVisibility(args);

                        if (element.SelectionStart != 0)
                            await Js.SetSelectionStartAsync(Ref, element.SelectionStart);
                    }));
                    builder.CloseComponent();
                    builder.CloseElement();
                });
            }
        }

        private void ToggleVisibility(MouseEventArgs args)
        {
            if (VisibilityToggle)
            {
                if (_visible)
                {
                    _eyeIcon = "eye";
                    Type = "text";
                }
                else
                {
                    _eyeIcon = "eye-invisible";
                    Type = "password";
                }

                _visible = !_visible;
            }
        }
    }
}
